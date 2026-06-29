using System;
using System.Collections;

using CCS.Modules.Interaction;

using Unity.Netcode;

using UnityEngine;

// =============================================================================
// SCRIPT: CCS_PlayerInteractionAnimator
// CATEGORY: Modules / CharacterController / Runtime / Visuals
// PURPOSE: Owns gameplay interaction lock/busy state during pickups and door interactions.
// PLACEMENT: PF_CCS_CharacterController_Player_Networked / VisualRoot.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: v0.7.3 locomotion-only reset — gameplay interaction lock only; no Animator triggers.
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

        [SerializeField] private Component interactionSourceComponent;
        [SerializeField] private bool enableInteractionDebugLogs;
        [SerializeField] private float pickUpRightHandLockDuration = CCS_InteractionConstants.PickUpRightHandLockDuration;
        [SerializeField] private float walkThroughDoorRightHandLockDuration =
            CCS_InteractionConstants.WalkThroughDoorRightHandLockDuration;

        private CCS_IInteractionAnimationSource interactionSource;
        private NetworkObject cachedNetworkObject;
        private CCS_CharacterMotor cachedCharacterMotor;
        private Coroutine unlockCoroutine;
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

            ReleaseInteractionLock(completedEvent.Result.AnimationKey, logReleased: true);
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

        #endregion
    }
}
