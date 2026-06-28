using UnityEngine;

// =============================================================================
// SCRIPT: CCS_RevolverUpperBodyAnimator
// CATEGORY: Modules / CharacterController / Runtime / Animation
// PURPOSE: Drives RevolverUpperBody animator layer from aim and weapon animation state.
// PLACEMENT: PF_CCS_CharacterController_TestPlayer_Networked / VisualRoot.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: v0.6.15 — simplified right-arm masked aim: IdleToAim → FullDraw → IdleToAim reverse.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    [DefaultExecutionOrder(250)]
    public sealed class CCS_RevolverUpperBodyAnimator : MonoBehaviour
    {
        #region Variables

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
        private static readonly int RevolverAimToIdleReturnStateHash =
            Animator.StringToHash(CCS_CharacterControllerConstants.AnimatorRevolverAimToIdleReturnStateName);
        private static readonly int RevolverNoAimStateHash =
            Animator.StringToHash(CCS_CharacterControllerConstants.AnimatorRevolverNoAimStateName);
        private static readonly int RevolverAimIdleFullDrawStateHash =
            Animator.StringToHash(CCS_CharacterControllerConstants.AnimatorRevolverAimIdleFullDrawStateName);
        private static readonly int RevolverIdleToAimStateHash =
            Animator.StringToHash(CCS_CharacterControllerConstants.AnimatorRevolverIdleToAimStateName);
        private static readonly int RevolverWildWestAimIdleStateHash =
            Animator.StringToHash(CCS_CharacterControllerConstants.AnimatorRevolverWildWestAimIdleStateName);
        private static readonly int RevolverWildWestFireStateHash =
            Animator.StringToHash(CCS_CharacterControllerConstants.AnimatorRevolverWildWestFireStateName);

        [SerializeField] private Animator animator;
        [SerializeField] private Component revolverAnimationStateComponent;
        [SerializeField] private Component interactionLockSourceComponent;
        [SerializeField] private CCS_CharacterMotor characterMotor;
        [SerializeField] private bool suppressFireUpperBodyAnimation = true;
        [SerializeField] private float aimStateCrossFadeDuration = 0.05f;
        [SerializeField] private bool enableRevolverAnimationDebug;

        [Header("Aim Layer Weight")]
        [SerializeField] private float aimLayerBlendInSpeed = 16f;
        [SerializeField] private float aimLayerBlendOutSpeed = 18f;

        [Header("Wild West Preview (optional)")]
        [Tooltip("When enabled, crossfades to CCS_WW_Revolver_AimIdle_RH preview states instead of full-draw flow.")]
        [SerializeField] private bool useWildWestRightHandRevolverAimPreview;

        private CCS_IRevolverAnimationState revolverAnimationState;
        private CCS_ICharacterControlLockSource controlLockSource;
        private int revolverLayerIndex = -1;
        private float currentLayerWeight;
        private bool loggedMissingLayer;
        private bool subscribedRevolverEvents;
        private bool lastAppliedRevolverAimHeld;
        private readonly System.Collections.Generic.HashSet<int> animatorParameterHashes =
            new System.Collections.Generic.HashSet<int>();
        private bool animatorParametersCached;
        private CCS_RevolverAimPhase currentAimPhase = CCS_RevolverAimPhase.NoAim;
        private bool externalAimOverrideActive;
        private bool externalRevolverAimHeld;
        private bool externalRevolverOwned;
        private bool externalRevolverReloading;

        #endregion

        #region Properties

        public CCS_RevolverAimPhase CurrentAimPhase => currentAimPhase;

        public bool IsReticleAimPhaseActive => currentAimPhase == CCS_RevolverAimPhase.FullDraw;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            CacheAnimatorParameters();
            EnsureRevolverEventSubscription();
        }

        private void OnDisable()
        {
            UnsubscribeRevolverEvents();
            currentLayerWeight = 0f;
            currentAimPhase = CCS_RevolverAimPhase.NoAim;
            ApplyLayerWeight(0f);
            animatorParametersCached = false;
            animatorParameterHashes.Clear();
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
        }

        #endregion

        #region Public Methods

        public string BuildRuntimeDebugSnapshot()
        {
            if (animator == null)
            {
                return string.Empty;
            }

            bool aimHeld = ResolveRevolverAimHeld();
            _ = aimHeld;
            string controllerPath = ResolveRuntimeAnimatorControllerPath();
            string layerName = revolverLayerIndex >= 0
                ? CCS_CharacterControllerConstants.AnimatorRevolverUpperBodyLayerName
                : "Missing";
            float liveLayerWeight = revolverLayerIndex >= 0
                ? animator.GetLayerWeight(revolverLayerIndex)
                : 0f;
            string stateName = GetActiveLayerStateName();
            string currentClipName = GetActiveLayerClipName(false);
            string baseStateName = ResolveBaseLayerStateName();
            return "Animator Controller: "
                + controllerPath
                + "\nAim Layer: "
                + layerName
                + "\nAim Layer Index: "
                + revolverLayerIndex
                + "\nAim Layer Weight: "
                + liveLayerWeight.ToString("0.000")
                + " (tracked "
                + currentLayerWeight.ToString("0.000")
                + ")\nAim State: "
                + stateName
                + "\nBase State: "
                + baseStateName
                + "\nMask: AM_CCS_Revolver_UpperBodyRightArm_Aim"
                + "\nPhase: "
                + ResolveAimPhaseDisplayLabel(currentAimPhase)
                + "\nRevolverAimHeld: "
                + lastAppliedRevolverAimHeld
                + "\nClip: "
                + currentClipName;
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public void ForceDebugPlayWildWestAimStateForTesting()
        {
            if (animator == null || revolverLayerIndex < 0)
            {
                Debug.LogWarning(
                    "[Revolver Debug] Cannot force-play aim: missing animator or RevolverUpperBody layer.",
                    this);
                return;
            }

            currentLayerWeight = 1f;
            ApplyLayerWeight(1f);
            SetAnimatorBoolIfPresent(RevolverAimHeldHash, true);
            lastAppliedRevolverAimHeld = true;

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

        public void SetRevolverAimHeldExternal(bool aimHeld, bool revolverOwned, bool isReloading)
        {
            externalAimOverrideActive = true;
            externalRevolverAimHeld = aimHeld;
            externalRevolverOwned = revolverOwned;
            externalRevolverReloading = isReloading;
        }

        public void ClearExternalRevolverAimOverride()
        {
            externalAimOverrideActive = false;
            externalRevolverAimHeld = false;
            externalRevolverOwned = false;
            externalRevolverReloading = false;
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
                else if (revolverLayerIndex >= 0)
                {
                    CacheAnimatorParameters();
                }
            }
        }

        private void CacheAnimatorParameters()
        {
            if (animator == null)
            {
                animatorParametersCached = false;
                animatorParameterHashes.Clear();
                return;
            }

            if (animatorParametersCached && animatorParameterHashes.Count > 0)
            {
                return;
            }

            animatorParameterHashes.Clear();
            AnimatorControllerParameter[] parameters = animator.parameters;
            for (int i = 0; i < parameters.Length; i++)
            {
                animatorParameterHashes.Add(parameters[i].nameHash);
            }

            animatorParametersCached = true;
        }

        private bool HasAnimatorParameter(int parameterHash)
        {
            return animator != null && animatorParameterHashes.Contains(parameterHash);
        }

        private void SetAnimatorBoolIfPresent(int parameterHash, bool value)
        {
            if (HasAnimatorParameter(parameterHash))
            {
                animator.SetBool(parameterHash, value);
            }
        }

        private void SetAnimatorTriggerIfPresent(int parameterHash)
        {
            if (HasAnimatorParameter(parameterHash))
            {
                animator.SetTrigger(parameterHash);
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
                currentAimPhase = CCS_RevolverAimPhase.NoAim;
                return;
            }

            if (IsControlLocked() || !ResolveRevolverOwned())
            {
                currentAimPhase = CCS_RevolverAimPhase.NoAim;
                currentLayerWeight = Mathf.MoveTowards(
                    currentLayerWeight,
                    0f,
                    aimLayerBlendOutSpeed * Time.deltaTime);
                ApplyLayerWeight(currentLayerWeight);
                return;
            }

            currentAimPhase = ResolveCurrentAimPhase();
            float targetWeight = ResolveTargetLayerWeight(currentAimPhase);
            float blendSpeed = targetWeight >= currentLayerWeight
                ? aimLayerBlendInSpeed
                : aimLayerBlendOutSpeed;
            currentLayerWeight = Mathf.MoveTowards(
                currentLayerWeight,
                targetWeight,
                blendSpeed * Time.deltaTime);
            ApplyLayerWeight(currentLayerWeight);
        }

        private float ResolveTargetLayerWeight(CCS_RevolverAimPhase phase)
        {
            switch (phase)
            {
                case CCS_RevolverAimPhase.NoAim:
                    return 0f;
                case CCS_RevolverAimPhase.Drawing:
                    return ResolveDrawingLayerWeight();
                case CCS_RevolverAimPhase.FullDraw:
                case CCS_RevolverAimPhase.Returning:
                    return 1f;
                default:
                    return 0f;
            }
        }

        private float ResolveDrawingLayerWeight()
        {
            if (animator == null || revolverLayerIndex < 0)
            {
                return ResolveRevolverAimHeld() ? 1f : 0f;
            }

            if (animator.IsInTransition(revolverLayerIndex))
            {
                AnimatorStateInfo nextState = animator.GetNextAnimatorStateInfo(revolverLayerIndex);
                if (nextState.IsName(CCS_CharacterControllerConstants.AnimatorRevolverIdleToAimStateName))
                {
                    return Mathf.Clamp01(nextState.normalizedTime);
                }
            }

            AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(revolverLayerIndex);
            if (currentState.IsName(CCS_CharacterControllerConstants.AnimatorRevolverIdleToAimStateName))
            {
                return Mathf.Clamp01(currentState.normalizedTime);
            }

            return ResolveRevolverAimHeld() ? 1f : 0f;
        }

        private CCS_RevolverAimPhase ResolveCurrentAimPhase()
        {
            if (animator == null || revolverLayerIndex < 0)
            {
                return ResolveRevolverAimHeld() ? CCS_RevolverAimPhase.Drawing : CCS_RevolverAimPhase.NoAim;
            }

            if (animator.IsInTransition(revolverLayerIndex))
            {
                AnimatorStateInfo nextState = animator.GetNextAnimatorStateInfo(revolverLayerIndex);
                CCS_RevolverAimPhase nextPhase = ResolveAimPhaseFromStateInfo(nextState);
                if (nextPhase != CCS_RevolverAimPhase.NoAim)
                {
                    return nextPhase;
                }
            }

            AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(revolverLayerIndex);
            return ResolveAimPhaseFromStateInfo(currentState);
        }

        private static CCS_RevolverAimPhase ResolveAimPhaseFromStateInfo(AnimatorStateInfo stateInfo)
        {
            if (stateInfo.IsName(CCS_CharacterControllerConstants.AnimatorRevolverNoAimStateName))
            {
                return CCS_RevolverAimPhase.NoAim;
            }

            if (stateInfo.IsName(CCS_CharacterControllerConstants.AnimatorRevolverIdleToAimStateName))
            {
                return CCS_RevolverAimPhase.Drawing;
            }

            if (stateInfo.IsName(CCS_CharacterControllerConstants.AnimatorRevolverAimIdleFullDrawStateName))
            {
                return CCS_RevolverAimPhase.FullDraw;
            }

            if (stateInfo.IsName(CCS_CharacterControllerConstants.AnimatorRevolverAimToIdleReturnStateName))
            {
                return CCS_RevolverAimPhase.Returning;
            }

            return CCS_RevolverAimPhase.NoAim;
        }

        private bool ResolveRevolverAimHeld()
        {
            if (externalAimOverrideActive)
            {
                return externalRevolverOwned && externalRevolverAimHeld;
            }

            if (IsControlLocked() || revolverAnimationState == null)
            {
                return false;
            }

            return revolverAnimationState.RevolverAimHeld;
        }

        private bool ResolveRevolverOwned()
        {
            if (externalAimOverrideActive)
            {
                return externalRevolverOwned;
            }

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
            bool isReloading = externalAimOverrideActive
                ? externalRevolverReloading
                : (revolverAnimationState != null && revolverAnimationState.RevolverIsReloading);

            SetAnimatorBoolIfPresent(RevolverAimHeldHash, aimHeld);
            SetAnimatorBoolIfPresent(RevolverIsReloadingHash, isReloading);
            lastAppliedRevolverAimHeld = aimHeld;
        }

        private void HandleRevolverFired()
        {
            if (animator == null || IsControlLocked())
            {
                return;
            }

            if (suppressFireUpperBodyAnimation)
            {
                return;
            }

            if (useWildWestRightHandRevolverAimPreview && revolverLayerIndex >= 0)
            {
                currentLayerWeight = 1f;
                ApplyLayerWeight(1f);
                animator.CrossFadeInFixedTime(
                    RevolverWildWestFireStateHash,
                    aimStateCrossFadeDuration,
                    revolverLayerIndex,
                    0f);
            }

            SetAnimatorTriggerIfPresent(RevolverFireTriggerHash);
        }

        private void TryApplyWildWestRightHandPreviewState(bool isMoving)
        {
            if (animator == null || revolverLayerIndex < 0)
            {
                return;
            }

            if (isMoving)
            {
                return;
            }

            AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(revolverLayerIndex);
            if (currentState.shortNameHash == RevolverWildWestAimIdleStateHash
                || currentState.shortNameHash == RevolverWildWestFireStateHash)
            {
                return;
            }

            currentLayerWeight = 1f;
            ApplyLayerWeight(1f);
            animator.CrossFadeInFixedTime(
                RevolverWildWestAimIdleStateHash,
                aimStateCrossFadeDuration,
                revolverLayerIndex,
                0f);
        }

        private void HandleRevolverReloadStarted()
        {
            if (animator == null || IsControlLocked())
            {
                return;
            }

            SetAnimatorTriggerIfPresent(RevolverReloadTriggerHash);
            SetAnimatorBoolIfPresent(RevolverIsReloadingHash, true);
        }

        private void HandleRevolverReloadCompleted()
        {
            if (animator == null)
            {
                return;
            }

            SetAnimatorBoolIfPresent(RevolverIsReloadingHash, false);
        }

        private static string ResolveAimPhaseDisplayLabel(CCS_RevolverAimPhase phase)
        {
            switch (phase)
            {
                case CCS_RevolverAimPhase.Drawing:
                    return "Drawing";
                case CCS_RevolverAimPhase.FullDraw:
                    return "FullDraw";
                case CCS_RevolverAimPhase.Returning:
                    return "Returning";
                default:
                    return "NoAim";
            }
        }

        private static string ResolveAimPhaseLabel(string stateName)
        {
            if (stateName == CCS_CharacterControllerConstants.AnimatorRevolverNoAimStateName)
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

            if (stateName == CCS_CharacterControllerConstants.AnimatorRevolverAimToIdleReturnStateName
                || stateName == CCS_CharacterControllerConstants.AnimatorRevolverAimToIdleStateName)
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

        private string ResolveBaseLayerStateName()
        {
            if (animator == null)
            {
                return "Missing";
            }

            AnimatorClipInfo[] clips = animator.GetCurrentAnimatorClipInfo(0);
            if (clips.Length > 0 && clips[0].clip != null)
            {
                return clips[0].clip.name;
            }

            return "Layer0";
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
            if (stateInfo.IsName(CCS_CharacterControllerConstants.AnimatorRevolverNoAimStateName))
            {
                return CCS_CharacterControllerConstants.AnimatorRevolverNoAimStateName;
            }

            if (stateInfo.IsName(CCS_CharacterControllerConstants.AnimatorRevolverIdleToAimStateName))
            {
                return CCS_CharacterControllerConstants.AnimatorRevolverIdleToAimStateName;
            }

            if (stateInfo.IsName(CCS_CharacterControllerConstants.AnimatorRevolverAimIdleFullDrawStateName))
            {
                return CCS_CharacterControllerConstants.AnimatorRevolverAimIdleFullDrawStateName;
            }

            if (stateInfo.IsName(CCS_CharacterControllerConstants.AnimatorRevolverAimToIdleReturnStateName))
            {
                return CCS_CharacterControllerConstants.AnimatorRevolverAimToIdleReturnStateName;
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
                    animatorParametersCached = false;
                    return true;
                }
            }

            resolvedAnimator = null;
            return false;
        }

        #endregion
    }
}
