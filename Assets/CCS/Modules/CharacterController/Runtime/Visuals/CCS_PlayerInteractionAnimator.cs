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
        }

        private void OnEnable()
        {
            if (interactionSource != null)
            {
                interactionSource.InteractionCompleted += HandleInteractionCompleted;
            }
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
            SetInteractionBusy(false, activeInteractionAnimationKey);
        }

        public void PlayInteractionAnimation(CCS_InteractionAnimationKey animationKey)
        {
            if (!TryResolveAnimator(out Animator resolvedAnimator))
            {
                return;
            }

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
            SetInteractionBusy(false, animationKey);
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

        private void SetInteractionBusy(bool busy, CCS_InteractionAnimationKey animationKey, bool logChange = true)
        {
            if (isInteractionBusy == busy)
            {
                return;
            }

            isInteractionBusy = busy;
            activeInteractionAnimationKey = animationKey;

            if (logChange && enableInteractionDebugLogs)
            {
                string triggerName = CCS_InteractionAnimationKeyUtility.ToAnimatorTriggerName(animationKey);
                Debug.Log(
                    busy
                        ? $"[Interaction] Movement lock started: {triggerName}"
                        : $"[Interaction] Movement lock released: {triggerName}",
                    this);
            }

            InteractionBusyChanged?.Invoke(busy);
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
            if (animator != null && HasPlayableController(animator))
            {
                resolvedAnimator = animator;
                return true;
            }

            Animator[] animators = GetComponentsInChildren<Animator>(true);
            for (int i = 0; i < animators.Length; i++)
            {
                Animator candidate = animators[i];
                if (candidate == null || !HasPlayableController(candidate))
                {
                    continue;
                }

                animator = candidate;
                resolvedAnimator = candidate;
                loggedMissingController = false;
                return true;
            }

            resolvedAnimator = null;

            if (!loggedMissingController)
            {
                loggedMissingController = true;
                Debug.LogWarning(
                    "[CCS Player Interaction Animator] No child Animator with a runtime AnimatorController was found. Visual interaction triggers were skipped.",
                    this);
            }

            return false;
        }

        private static bool HasPlayableController(Animator candidate)
        {
            return candidate != null
                && candidate.isActiveAndEnabled
                && candidate.runtimeAnimatorController != null;
        }

        #endregion
    }
}
