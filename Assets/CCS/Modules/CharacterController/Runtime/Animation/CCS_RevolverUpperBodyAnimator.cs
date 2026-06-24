using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_RevolverUpperBodyAnimator
// CATEGORY: Modules / CharacterController / Runtime / Animation
// PURPOSE: Drives RevolverUpperBody animator layer from aim and weapon animation state.
// PLACEMENT: PF_CCS_CharacterController_TestPlayer_Networked / VisualRoot.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: v0.6.11 — Wild West one-handed revolver aim enter/loop/exit on RevolverUpperBody.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    [DefaultExecutionOrder(250)]
    public sealed class CCS_RevolverUpperBodyAnimator : MonoBehaviour
    {
        #region Variables

        private const string MasterTestSceneName = "SCN_CCS_CharacterController_MasterTest";

        private static readonly int RevolverAimHeldHash =
            Animator.StringToHash(CCS_CharacterControllerConstants.AnimatorRevolverAimHeldParameter);
        private static readonly int RevolverFireTriggerHash =
            Animator.StringToHash(CCS_CharacterControllerConstants.AnimatorRevolverFireTriggerParameter);
        private static readonly int RevolverReloadTriggerHash =
            Animator.StringToHash(CCS_CharacterControllerConstants.AnimatorRevolverReloadTriggerParameter);
        private static readonly int RevolverIsReloadingHash =
            Animator.StringToHash(CCS_CharacterControllerConstants.AnimatorRevolverIsReloadingParameter);
        private static readonly int RevolverIsMovingHash =
            Animator.StringToHash(CCS_CharacterControllerConstants.AnimatorRevolverIsMovingParameter);
        private static readonly int RevolverAimIdleFullDrawStateHash =
            Animator.StringToHash(CCS_CharacterControllerConstants.AnimatorRevolverAimIdleFullDrawStateName);
        private static readonly int RevolverIdleToAimStateHash =
            Animator.StringToHash(CCS_CharacterControllerConstants.AnimatorRevolverIdleToAimStateName);

        [SerializeField] private Animator animator;
        [SerializeField] private Component revolverAnimationStateComponent;
        [SerializeField] private Component interactionLockSourceComponent;
        [SerializeField] private CCS_CharacterMotor characterMotor;
        [SerializeField] private float layerFadeOutSpeed = 10f;
        [SerializeField] private float aimStateCrossFadeDuration = 0.05f;
        [SerializeField] private bool enableRevolverAnimationDebug;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        [SerializeField] private bool enableMasterTestForceAimDebugHotkey = true;
#endif

        private CCS_IRevolverAnimationState revolverAnimationState;
        private CCS_ICharacterControlLockSource controlLockSource;
        private int revolverLayerIndex = -1;
        private float currentLayerWeight;
        private bool loggedMissingLayer;
        private bool subscribedRevolverEvents;
        private bool lastAppliedRevolverAimHeld;
        private bool lastAppliedRevolverIsMoving;
        private float lastLocomotionSpeedNormalized;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            EnsureRevolverEventSubscription();
        }

        private void OnDisable()
        {
            UnsubscribeRevolverEvents();
            currentLayerWeight = 0f;
            ApplyLayerWeight(0f);
        }

        private void Update()
        {
            if (animator == null)
            {
                return;
            }

            ResolveReferences();
            EnsureRevolverEventSubscription();
            UpdateAnimatorParameters();
            UpdateLayerWeight();
            HandleMasterTestForceAimDebugHotkey();
            DrawRuntimeDebugOverlay();
        }

        #endregion

        #region Private Methods

        private void ResolveReferences()
        {
            if (animator == null)
            {
                TryResolveAnimator(out animator);
            }

            if (characterMotor == null)
            {
                characterMotor = GetComponentInParent<CCS_CharacterMotor>();
            }

            if (revolverAnimationState == null)
            {
                if (revolverAnimationStateComponent is CCS_IRevolverAnimationState fromComponent)
                {
                    revolverAnimationState = fromComponent;
                }
                else if (revolverAnimationStateComponent == null)
                {
                    revolverAnimationState = GetComponentInParent<CCS_IRevolverAnimationState>();
                }
            }

            if (controlLockSource == null)
            {
                if (interactionLockSourceComponent is CCS_ICharacterControlLockSource fromLockComponent)
                {
                    controlLockSource = fromLockComponent;
                }
                else
                {
                    controlLockSource = GetComponent<CCS_ICharacterControlLockSource>()
                        ?? GetComponentInParent<CCS_ICharacterControlLockSource>();
                }
            }

            if (revolverLayerIndex < 0 && animator != null)
            {
                revolverLayerIndex = animator.GetLayerIndex(
                    CCS_CharacterControllerConstants.AnimatorRevolverUpperBodyLayerName);
                if (revolverLayerIndex < 0 && !loggedMissingLayer)
                {
                    loggedMissingLayer = true;
                    Debug.LogWarning(
                        "[Revolver Upper Body Animator] Animator layer "
                        + CCS_CharacterControllerConstants.AnimatorRevolverUpperBodyLayerName
                        + " was not found.",
                        this);
                }
            }
        }

        private void EnsureRevolverEventSubscription()
        {
            if (revolverAnimationState == null || subscribedRevolverEvents)
            {
                return;
            }

            SubscribeRevolverEvents();
        }

        private void SubscribeRevolverEvents()
        {
            if (revolverAnimationState == null)
            {
                return;
            }

            revolverAnimationState.RevolverFired += HandleRevolverFired;
            revolverAnimationState.RevolverReloadStarted += HandleRevolverReloadStarted;
            revolverAnimationState.RevolverReloadCompleted += HandleRevolverReloadCompleted;
            subscribedRevolverEvents = true;
        }

        private void UnsubscribeRevolverEvents()
        {
            if (revolverAnimationState == null || !subscribedRevolverEvents)
            {
                subscribedRevolverEvents = false;
                return;
            }

            revolverAnimationState.RevolverFired -= HandleRevolverFired;
            revolverAnimationState.RevolverReloadStarted -= HandleRevolverReloadStarted;
            revolverAnimationState.RevolverReloadCompleted -= HandleRevolverReloadCompleted;
            subscribedRevolverEvents = false;
        }

        private void UpdateLayerWeight()
        {
            if (revolverLayerIndex < 0)
            {
                return;
            }

            bool shouldUseLayer = ShouldUseRevolverUpperBodyLayer();
            if (shouldUseLayer)
            {
                currentLayerWeight = 1f;
                ApplyLayerWeight(1f);
            }
            else
            {
                currentLayerWeight = Mathf.MoveTowards(
                    currentLayerWeight,
                    0f,
                    layerFadeOutSpeed * Time.deltaTime);
                ApplyLayerWeight(currentLayerWeight);
            }
        }

        private bool ShouldUseRevolverUpperBodyLayer()
        {
            if (IsControlLocked() || !ResolveRevolverOwned())
            {
                return false;
            }

            if (ResolveRevolverAimHeld())
            {
                return true;
            }

            return IsRevolverLayerInActiveAimFlow();
        }

        private bool IsRevolverLayerInActiveAimFlow()
        {
            if (animator == null || revolverLayerIndex < 0)
            {
                return false;
            }

            if (animator.IsInTransition(revolverLayerIndex))
            {
                AnimatorStateInfo nextState = animator.GetNextAnimatorStateInfo(revolverLayerIndex);
                if (!IsRevolverEmptyState(nextState))
                {
                    return true;
                }
            }

            AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(revolverLayerIndex);
            return !IsRevolverEmptyState(currentState);
        }

        private static bool IsRevolverEmptyState(AnimatorStateInfo stateInfo)
        {
            return stateInfo.IsName(CCS_CharacterControllerConstants.AnimatorRevolverEmptyStateName);
        }

        private bool ResolveRevolverAimHeld()
        {
            if (IsControlLocked() || revolverAnimationState == null)
            {
                return false;
            }

            return revolverAnimationState.RevolverAimHeld;
        }

        private bool ResolveRevolverOwned()
        {
            return revolverAnimationState != null && revolverAnimationState.IsRevolverOwned;
        }

        private bool IsControlLocked()
        {
            return controlLockSource != null && controlLockSource.IsControlLocked;
        }

        private void ApplyLayerWeight(float weight)
        {
            if (revolverLayerIndex >= 0)
            {
                animator.SetLayerWeight(revolverLayerIndex, weight);
            }
        }

        private void UpdateAnimatorParameters()
        {
            if (revolverAnimationState == null || revolverLayerIndex < 0)
            {
                return;
            }

            bool interactionLocked = IsControlLocked();
            bool aimHeld = !interactionLocked && ResolveRevolverAimHeld();
            bool isReloading = !interactionLocked && revolverAnimationState.RevolverIsReloading;
            float speedNormalized = 0f;
            bool isMoving = !interactionLocked && ResolveRevolverIsMoving(out speedNormalized);

            animator.SetBool(RevolverAimHeldHash, aimHeld);
            animator.SetBool(RevolverIsReloadingHash, isReloading);
            animator.SetBool(RevolverIsMovingHash, isMoving);

            lastAppliedRevolverAimHeld = aimHeld;
            lastAppliedRevolverIsMoving = isMoving;
            lastLocomotionSpeedNormalized = speedNormalized;
        }

        private bool ResolveRevolverIsMoving(out float speedNormalized)
        {
            speedNormalized = 0f;
            if (characterMotor == null)
            {
                return false;
            }

            CCS_CharacterMovementProfile profile = characterMotor.MovementProfile;
            float sprintSpeed = profile != null ? profile.SprintSpeed : 0f;
            speedNormalized = sprintSpeed > 0f
                ? Mathf.Clamp01(characterMotor.CurrentSpeed / sprintSpeed)
                : 0f;

            return speedNormalized > CCS_CharacterControllerConstants.RevolverAimWalkSpeedThreshold;
        }

        private void HandleRevolverFired()
        {
            if (animator == null || IsControlLocked())
            {
                return;
            }

            animator.SetTrigger(RevolverFireTriggerHash);
        }

        private void HandleRevolverReloadStarted()
        {
            if (animator == null || IsControlLocked())
            {
                return;
            }

            animator.SetTrigger(RevolverReloadTriggerHash);
            animator.SetBool(RevolverIsReloadingHash, true);
        }

        private void HandleRevolverReloadCompleted()
        {
            if (animator == null)
            {
                return;
            }

            animator.SetBool(RevolverIsReloadingHash, false);
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void HandleMasterTestForceAimDebugHotkey()
        {
            if (!enableMasterTestForceAimDebugHotkey || !IsMasterTestSceneActive())
            {
                return;
            }

            if (!ResolveRevolverOwned())
            {
                return;
            }

            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || !keyboard.f9Key.wasPressedThisFrame)
            {
                return;
            }

            ForceDebugPlayWildWestAimState();
        }

        private void ForceDebugPlayWildWestAimState()
        {
            if (animator == null || revolverLayerIndex < 0)
            {
                Debug.LogWarning("[Revolver Debug] Cannot force-play aim: missing animator or RevolverUpperBody layer.", this);
                return;
            }

            currentLayerWeight = 1f;
            ApplyLayerWeight(1f);
            animator.SetBool(RevolverAimHeldHash, true);
            animator.SetBool(RevolverIsMovingHash, false);
            lastAppliedRevolverAimHeld = true;
            lastAppliedRevolverIsMoving = false;

            animator.CrossFadeInFixedTime(
                RevolverIdleToAimStateHash,
                aimStateCrossFadeDuration,
                revolverLayerIndex,
                0f);

            Debug.Log(
                "[Revolver Debug] F9 force-play: layer="
                + CCS_CharacterControllerConstants.AnimatorRevolverUpperBodyLayerName
                + " state="
                + CCS_CharacterControllerConstants.AnimatorRevolverIdleToAimStateName
                + " clip="
                + CCS_CharacterControllerConstants.WildWestRevolverIdleToAimClipPath,
                this);
        }
#endif

        private static bool IsMasterTestSceneActive()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            return activeScene.IsValid() && activeScene.name == MasterTestSceneName;
        }

        private void DrawRuntimeDebugOverlay()
        {
            if (!enableRevolverAnimationDebug)
            {
                return;
            }

            bool aimHeld = ResolveRevolverAimHeld();
            bool isRevolverOwned = ResolveRevolverOwned();
            string controllerPath = ResolveRuntimeAnimatorControllerPath();
            string layerName = revolverLayerIndex >= 0
                ? CCS_CharacterControllerConstants.AnimatorRevolverUpperBodyLayerName
                : "Missing";
            float liveLayerWeight = revolverLayerIndex >= 0
                ? animator.GetLayerWeight(revolverLayerIndex)
                : 0f;
            string stateName = GetActiveLayerStateName();
            string currentClipName = GetActiveLayerClipName(false);
            string nextStateName = GetNextLayerStateName();
            string nextClipName = GetActiveLayerClipName(true);
            string aimPhase = ResolveAimPhaseLabel(stateName);

            GUI.Label(
                new Rect(12f, 280f, 960f, 280f),
                "Revolver Animation Debug (v0.6.11 WildWestDefault)\n"
                + "Runtime Animator Controller: "
                + controllerPath
                + "\nisRevolverOwned: "
                + isRevolverOwned
                + "\nAimHeld input: "
                + aimHeld
                + "\nRevolverAimHeld parameter: "
                + lastAppliedRevolverAimHeld
                + "\nRevolverIsMoving parameter: "
                + lastAppliedRevolverIsMoving
                + "\nLocomotion speed normalized: "
                + lastLocomotionSpeedNormalized.ToString("0.000")
                + "\nactive layer: "
                + layerName
                + "\nlayer weight (tracked/live): "
                + currentLayerWeight.ToString("0.000")
                + " / "
                + liveLayerWeight.ToString("0.000")
                + "\nactive state: "
                + stateName
                + "\naim phase: "
                + aimPhase
                + "\ncurrent clip: "
                + currentClipName
                + "\nnext state: "
                + nextStateName
                + "\nnext clip: "
                + nextClipName
                + "\nexpected aim idle clip: "
                + CCS_CharacterControllerConstants.WildWestRevolverAimIdleFullDrawClipPath
                + "\nexpected aim walk clip: "
                + CCS_CharacterControllerConstants.WildWestRevolverAimWalkClipPath
                + "\nexpected fire clip: "
                + CCS_CharacterControllerConstants.WildWestRevolverFireFanningClipPath
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                + "\nMaster Test F9 force-play: "
                + (enableMasterTestForceAimDebugHotkey ? "enabled" : "disabled")
#endif
                );
        }

        private static string ResolveAimPhaseLabel(string stateName)
        {
            if (stateName == CCS_CharacterControllerConstants.AnimatorRevolverEmptyStateName)
            {
                return "empty";
            }

            if (stateName == CCS_CharacterControllerConstants.AnimatorRevolverIdleToAimStateName
                || stateName == CCS_CharacterControllerConstants.AnimatorRevolverWalkToAimWalkStateName)
            {
                return "enter aim";
            }

            if (stateName == CCS_CharacterControllerConstants.AnimatorRevolverAimIdleFullDrawStateName)
            {
                return "idle aim loop";
            }

            if (stateName == CCS_CharacterControllerConstants.AnimatorRevolverAimWalkStateName)
            {
                return "aimed walk loop";
            }

            if (stateName == CCS_CharacterControllerConstants.AnimatorRevolverAimToIdleStateName
                || stateName == CCS_CharacterControllerConstants.AnimatorRevolverAimWalkToWalkStateName)
            {
                return "exit aim";
            }

            if (stateName == CCS_CharacterControllerConstants.AnimatorRevolverFireStateName)
            {
                return "fire";
            }

            if (stateName == CCS_CharacterControllerConstants.AnimatorRevolverReloadStateName)
            {
                return "reload";
            }

            return "other";
        }

        private string ResolveRuntimeAnimatorControllerPath()
        {
            if (animator == null || animator.runtimeAnimatorController == null)
            {
                return "Missing";
            }

            RuntimeAnimatorController runtimeController = animator.runtimeAnimatorController;
            if (runtimeController is AnimatorOverrideController overrideController
                && overrideController.runtimeAnimatorController != null)
            {
                runtimeController = overrideController.runtimeAnimatorController;
            }

            return CCS_CharacterControllerConstants.PlayerLocomotionAnimatorControllerPath
                + " (runtime name: "
                + runtimeController.name
                + ")";
        }

        private string GetActiveLayerStateName()
        {
            return ResolveLayerStateName(animator.GetCurrentAnimatorStateInfo(revolverLayerIndex));
        }

        private string GetNextLayerStateName()
        {
            if (animator == null || revolverLayerIndex < 0 || !animator.IsInTransition(revolverLayerIndex))
            {
                return "None";
            }

            return ResolveLayerStateName(animator.GetNextAnimatorStateInfo(revolverLayerIndex));
        }

        private static string ResolveLayerStateName(AnimatorStateInfo stateInfo)
        {
            if (stateInfo.IsName(CCS_CharacterControllerConstants.AnimatorRevolverEmptyStateName))
            {
                return CCS_CharacterControllerConstants.AnimatorRevolverEmptyStateName;
            }

            if (stateInfo.IsName(CCS_CharacterControllerConstants.AnimatorRevolverIdleToAimStateName))
            {
                return CCS_CharacterControllerConstants.AnimatorRevolverIdleToAimStateName;
            }

            if (stateInfo.IsName(CCS_CharacterControllerConstants.AnimatorRevolverAimIdleFullDrawStateName))
            {
                return CCS_CharacterControllerConstants.AnimatorRevolverAimIdleFullDrawStateName;
            }

            if (stateInfo.IsName(CCS_CharacterControllerConstants.AnimatorRevolverAimToIdleStateName))
            {
                return CCS_CharacterControllerConstants.AnimatorRevolverAimToIdleStateName;
            }

            if (stateInfo.IsName(CCS_CharacterControllerConstants.AnimatorRevolverWalkToAimWalkStateName))
            {
                return CCS_CharacterControllerConstants.AnimatorRevolverWalkToAimWalkStateName;
            }

            if (stateInfo.IsName(CCS_CharacterControllerConstants.AnimatorRevolverAimWalkStateName))
            {
                return CCS_CharacterControllerConstants.AnimatorRevolverAimWalkStateName;
            }

            if (stateInfo.IsName(CCS_CharacterControllerConstants.AnimatorRevolverAimWalkToWalkStateName))
            {
                return CCS_CharacterControllerConstants.AnimatorRevolverAimWalkToWalkStateName;
            }

            if (stateInfo.IsName(CCS_CharacterControllerConstants.AnimatorRevolverFireStateName))
            {
                return CCS_CharacterControllerConstants.AnimatorRevolverFireStateName;
            }

            if (stateInfo.IsName(CCS_CharacterControllerConstants.AnimatorRevolverReloadStateName))
            {
                return CCS_CharacterControllerConstants.AnimatorRevolverReloadStateName;
            }

            return "Other(" + stateInfo.shortNameHash + ")";
        }

        private string GetActiveLayerClipName(bool nextClip)
        {
            if (animator == null || revolverLayerIndex < 0)
            {
                return "None";
            }

            AnimatorClipInfo[] clipInfos = nextClip
                ? animator.GetNextAnimatorClipInfo(revolverLayerIndex)
                : animator.GetCurrentAnimatorClipInfo(revolverLayerIndex);

            if (clipInfos == null || clipInfos.Length == 0)
            {
                return "None";
            }

            return clipInfos[0].clip != null ? clipInfos[0].clip.name : "None";
        }

        private bool TryResolveAnimator(out Animator resolvedAnimator)
        {
            Animator[] animators = GetComponentsInChildren<Animator>(true);
            for (int i = 0; i < animators.Length; i++)
            {
                Animator candidate = animators[i];
                if (candidate != null && candidate.runtimeAnimatorController != null)
                {
                    animator = candidate;
                    resolvedAnimator = candidate;
                    loggedMissingLayer = false;
                    revolverLayerIndex = -1;
                    return true;
                }
            }

            resolvedAnimator = null;
            return false;
        }

        #endregion
    }
}
