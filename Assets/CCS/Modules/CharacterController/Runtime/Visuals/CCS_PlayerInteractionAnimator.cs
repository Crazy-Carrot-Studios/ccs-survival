using System;
using System.Collections;

using CCS.Modules.Interaction;

using Unity.Netcode;

using UnityEngine;

// =============================================================================
// SCRIPT: CCS_PlayerInteractionAnimator
// CATEGORY: Modules / CharacterController / Runtime / Visuals
// PURPOSE: Drives interaction Animator triggers and locks movement during interact anims.
// PLACEMENT: PF_CCS_CharacterController_TestPlayer_Networked / VisualRoot.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Lock begins on scanner E accept via BeginInteractionLock. Trigger fires on completed.
//        Normal unlock is driven by CCS_InteractionAnimationStateExitBehaviour OnStateExit.
//        Fallback timer unlocks only if Animator state exit is missed.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    [DefaultExecutionOrder(210)]
    public sealed class CCS_PlayerInteractionAnimator : MonoBehaviour,
        CCS_IInteractionBusySource,
        CCS_ICharacterControlLockSource,
        CCS_IInteractionLockController
    {
        #region Variables

        [SerializeField] private Animator animator;
        [SerializeField] private Component interactionSourceComponent;
        [SerializeField] private bool enableInteractionDebugLogs;
        [SerializeField] private float pickUpRightHandLockDuration = CCS_InteractionConstants.PickUpRightHandLockDuration;
        [SerializeField] private float walkThroughDoorRightHandLockDuration =
            CCS_InteractionConstants.WalkThroughDoorRightHandLockDuration;

        private CCS_IInteractionAnimationSource interactionSource;
        private NetworkObject cachedNetworkObject;
        private CCS_CharacterMotor cachedCharacterMotor;
        private Coroutine unlockCoroutine;
        private bool loggedMissingController;
        private bool loggedFallbackAnimator;
        private bool loggedMissingInteractionLayer;
        private bool isInteractionBusy;
        private CCS_InteractionAnimationKey activeInteractionAnimationKey =
            CCS_InteractionAnimationKey.PickUp_RH;

        #endregion

        #region Properties

        public bool IsInteractionBusy => isInteractionBusy;

        public bool IsControlLocked => isInteractionBusy;

        #endregion

        #region Events

        public event Action<bool> InteractionBusyChanged;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            ResolveReferences();
            EnsureInteractionLayerWeight();
        }

        private void OnEnable()
        {
            if (interactionSource != null)
            {
                interactionSource.InteractionCompleted += HandleInteractionCompleted;
            }

            EnsureInteractionLayerWeight();
        }

        private void Start()
        {
            EnsureInteractionLayerWeight();
        }

        private void OnDisable()
        {
            if (interactionSource != null)
            {
                interactionSource.InteractionCompleted -= HandleInteractionCompleted;
            }

            StopUnlockCoroutine();
            SetInteractionBusy(false, activeInteractionAnimationKey, logChange: false);
        }

        #endregion

        #region Public Methods

        public void BeginInteractionLock(CCS_InteractionAnimationKey animationKey)
        {
            if (isInteractionBusy || !IsLocalAnimationOwner())
            {
                return;
            }

            activeInteractionAnimationKey = animationKey;
            SetInteractionBusy(true, animationKey);
            HardStopMotorPlanarMotion();
            StartUnlockCoroutine(animationKey);
        }

        public void CancelInteractionLock()
        {
            if (!isInteractionBusy)
            {
                return;
            }

            StopUnlockCoroutine();
            ReleaseInteractionLock(activeInteractionAnimationKey, logReleased: true);
        }

        public void NotifyInteractionAnimationStateEntered(CCS_InteractionAnimationKey animationKey)
        {
            if (!enableInteractionDebugLogs || !isInteractionBusy || animationKey != activeInteractionAnimationKey)
            {
                return;
            }

            Debug.Log(
                $"[Interaction Lock] Animation state entered {FormatAnimationKey(animationKey)}",
                this);
        }

        public void NotifyInteractionAnimationStateExited(CCS_InteractionAnimationKey animationKey)
        {
            if (enableInteractionDebugLogs)
            {
                Debug.Log(
                    $"[Interaction Lock] Animation state exited {FormatAnimationKey(animationKey)}",
                    this);
            }

            CompleteInteractionAnimationLock(animationKey);
        }

        public void CompleteInteractionAnimationLock(CCS_InteractionAnimationKey animationKey)
        {
            if (!isInteractionBusy || animationKey != activeInteractionAnimationKey || !IsLocalAnimationOwner())
            {
                return;
            }

            StopUnlockCoroutine();
            ReleaseInteractionLock(animationKey, logReleased: true);
        }

        public void PlayInteractionAnimation(CCS_InteractionAnimationKey animationKey)
        {
            if (!TryResolveAnimator(out Animator resolvedAnimator))
            {
                return;
            }

            EnsureInteractionLayerWeight(resolvedAnimator);

            string triggerName = CCS_InteractionAnimationKeyUtility.ToAnimatorTriggerName(animationKey);
            resolvedAnimator.SetTrigger(Animator.StringToHash(triggerName));
        }

        #endregion

        #region Private Methods

        private void ResolveReferences()
        {
            if (interactionSource == null)
            {
                if (interactionSourceComponent is CCS_IInteractionAnimationSource fromComponent)
                {
                    interactionSource = fromComponent;
                }
                else
                {
                    interactionSource = GetComponentInParent<CCS_IInteractionAnimationSource>();
                }
            }

            if (cachedCharacterMotor == null)
            {
                cachedCharacterMotor = GetComponentInParent<CCS_CharacterMotor>();
            }

            TryResolveAnimator(out _);
        }

        private void EnsureInteractionLayerWeight()
        {
            if (!TryResolveAnimator(out Animator resolvedAnimator))
            {
                return;
            }

            EnsureInteractionLayerWeight(resolvedAnimator);
        }

        private void EnsureInteractionLayerWeight(Animator resolvedAnimator)
        {
            if (resolvedAnimator == null)
            {
                return;
            }

            int interactionLayerIndex = resolvedAnimator.GetLayerIndex(
                CCS_CharacterControllerConstants.AnimatorInteractionReservedLayerName);
            if (interactionLayerIndex < 0)
            {
                if (!loggedMissingInteractionLayer)
                {
                    loggedMissingInteractionLayer = true;
                    Debug.LogWarning(
                        "[CCS Player Interaction Animator] Animator layer "
                        + CCS_CharacterControllerConstants.AnimatorInteractionReservedLayerName
                        + " was not found. Interaction animations may not play.",
                        this);
                }

                return;
            }

            float currentWeight = resolvedAnimator.GetLayerWeight(interactionLayerIndex);
            if (currentWeight < 0.999f)
            {
                resolvedAnimator.SetLayerWeight(interactionLayerIndex, 1f);
            }
        }

        private void HandleInteractionCompleted(CCS_InteractionCompletedEvent completedEvent)
        {
            if (!IsLocalAnimationOwner())
            {
                return;
            }

            if (!completedEvent.Result.Succeeded)
            {
                CancelInteractionLock();
                return;
            }

            PlayInteractionAnimation(completedEvent.Result.AnimationKey);
        }

        private void HardStopMotorPlanarMotion()
        {
            if (cachedCharacterMotor == null)
            {
                cachedCharacterMotor = GetComponentInParent<CCS_CharacterMotor>();
            }

            cachedCharacterMotor?.HardStopPlanarMotion();
        }

        private void StartUnlockCoroutine(CCS_InteractionAnimationKey animationKey)
        {
            StopUnlockCoroutine();
            unlockCoroutine = StartCoroutine(UnlockAfterDuration(animationKey));
        }

        private IEnumerator UnlockAfterDuration(CCS_InteractionAnimationKey animationKey)
        {
            float duration = ResolveLockDuration(animationKey);
            if (duration > 0f)
            {
                yield return new WaitForSeconds(duration);
            }

            unlockCoroutine = null;

            if (!isInteractionBusy || animationKey != activeInteractionAnimationKey)
            {
                yield break;
            }

            ReleaseInteractionLock(animationKey, logReleased: true, logFallback: true);
        }

        private void StopUnlockCoroutine()
        {
            if (unlockCoroutine == null)
            {
                return;
            }

            StopCoroutine(unlockCoroutine);
            unlockCoroutine = null;
        }

        private float ResolveLockDuration(CCS_InteractionAnimationKey animationKey)
        {
            switch (animationKey)
            {
                case CCS_InteractionAnimationKey.WalkThroughDoor_RH:
                    return walkThroughDoorRightHandLockDuration;
                case CCS_InteractionAnimationKey.PickUp_RH:
                default:
                    return pickUpRightHandLockDuration;
            }
        }

        private void ReleaseInteractionLock(
            CCS_InteractionAnimationKey animationKey,
            bool logReleased,
            bool logFallback = false)
        {
            if (!isInteractionBusy)
            {
                return;
            }

            if (logReleased && enableInteractionDebugLogs)
            {
                string suffix = logFallback ? " (fallback timer)" : string.Empty;
                Debug.Log(
                    $"[Interaction Lock] Released {FormatAnimationKey(animationKey)}{suffix}",
                    this);
            }

            SetInteractionBusy(false, animationKey);
        }

        private void SetInteractionBusy(bool busy, CCS_InteractionAnimationKey animationKey, bool logChange = true)
        {
            if (isInteractionBusy == busy)
            {
                return;
            }

            isInteractionBusy = busy;
            activeInteractionAnimationKey = animationKey;

            if (logChange && busy && enableInteractionDebugLogs)
            {
                Debug.Log(
                    $"[Interaction Lock] Started {FormatAnimationKey(animationKey)}",
                    this);
            }

            InteractionBusyChanged?.Invoke(busy);
        }

        private static string FormatAnimationKey(CCS_InteractionAnimationKey animationKey)
        {
            return CCS_InteractionAnimationKeyUtility.ToAnimatorTriggerName(animationKey);
        }

        private bool IsLocalAnimationOwner()
        {
            if (cachedNetworkObject == null)
            {
                cachedNetworkObject = GetComponentInParent<NetworkObject>();
            }

            NetworkObject networkObject = cachedNetworkObject;
            if (networkObject == null || !networkObject.IsSpawned)
            {
                return true;
            }

            return networkObject.IsOwner;
        }

        private bool TryResolveAnimator(out Animator resolvedAnimator)
        {
            if (animator != null && CCS_PlayerAnimatorResolver.IsAuthoritativeGameplayAnimator(animator))
            {
                resolvedAnimator = animator;
                return true;
            }

            CCS_PlayerRuntimeFacade facade = GetComponentInParent<CCS_PlayerRuntimeFacade>(true);
            if (facade != null
                && facade.Animator != null
                && CCS_PlayerAnimatorResolver.IsAuthoritativeGameplayAnimator(facade.Animator))
            {
                animator = facade.Animator;
                resolvedAnimator = animator;
                loggedMissingController = false;
                return true;
            }

            if (CCS_PlayerAnimatorResolver.TryResolveAuthoritativeAnimator(
                    transform,
                    out resolvedAnimator,
                    out bool usedFallback))
            {
                animator = resolvedAnimator;
                loggedMissingController = false;
                if (usedFallback && !loggedFallbackAnimator)
                {
                    loggedFallbackAnimator = true;
                    Debug.LogWarning(
                        "[CCS Player Interaction Animator] Used fallback authoritative Animator resolution.",
                        this);
                }

                return true;
            }

            resolvedAnimator = null;

            if (!loggedMissingController)
            {
                loggedMissingController = true;
                Debug.LogWarning(
                    "[CCS Player Interaction Animator] No authoritative humanoid Animator was found. Visual interaction triggers were skipped.",
                    this);
            }

            return false;
        }

        private static bool HasPlayableController(Animator candidate)
        {
            return CCS_PlayerAnimatorResolver.IsAuthoritativeGameplayAnimator(candidate);
        }

        #endregion
    }
}
