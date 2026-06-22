using UnityEngine;

// =============================================================================
// SCRIPT: CCS_RevolverUpperBodyAnimator
// CATEGORY: Modules / CharacterController / Runtime / Animation
// PURPOSE: Drives RevolverUpperBody animator layer from aim and weapon animation state.
// PLACEMENT: PF_CCS_CharacterController_TestPlayer_Networked / VisualRoot.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Does not read input directly. Suppresses layer during interaction control lock.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    [DefaultExecutionOrder(225)]
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

        [SerializeField] private Animator animator;
        [SerializeField] private Component revolverAnimationStateComponent;
        [SerializeField] private Component interactionLockSourceComponent;
        [SerializeField] private float layerFadeInSpeed = 8f;
        [SerializeField] private float layerFadeOutSpeed = 10f;

        private CCS_IRevolverAnimationState revolverAnimationState;
        private CCS_ICharacterControlLockSource controlLockSource;
        private int revolverLayerIndex = -1;
        private float currentLayerWeight;
        private bool loggedMissingLayer;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            SubscribeRevolverEvents();
        }

        private void OnDisable()
        {
            UnsubscribeRevolverEvents();
            currentLayerWeight = 0f;
            ApplyLayerWeight(0f);
        }

        private void LateUpdate()
        {
            if (animator == null)
            {
                return;
            }

            ResolveReferences();
            UpdateLayerWeight();
            UpdateAnimatorParameters();
        }

        #endregion

        #region Private Methods

        private void ResolveReferences()
        {
            if (animator == null)
            {
                TryResolveAnimator(out animator);
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
                if (interactionLockSourceComponent is CCS_ICharacterControlLockSource fromComponent)
                {
                    controlLockSource = fromComponent;
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

        private void SubscribeRevolverEvents()
        {
            if (revolverAnimationState == null)
            {
                return;
            }

            revolverAnimationState.RevolverFired += HandleRevolverFired;
            revolverAnimationState.RevolverReloadStarted += HandleRevolverReloadStarted;
            revolverAnimationState.RevolverReloadCompleted += HandleRevolverReloadCompleted;
        }

        private void UnsubscribeRevolverEvents()
        {
            if (revolverAnimationState == null)
            {
                return;
            }

            revolverAnimationState.RevolverFired -= HandleRevolverFired;
            revolverAnimationState.RevolverReloadStarted -= HandleRevolverReloadStarted;
            revolverAnimationState.RevolverReloadCompleted -= HandleRevolverReloadCompleted;
        }

        private void UpdateLayerWeight()
        {
            if (revolverLayerIndex < 0)
            {
                return;
            }

            bool shouldUseLayer = ShouldUseRevolverUpperBodyLayer();
            float targetWeight = shouldUseLayer ? 1f : 0f;
            float fadeSpeed = targetWeight > currentLayerWeight ? layerFadeInSpeed : layerFadeOutSpeed;
            currentLayerWeight = Mathf.MoveTowards(currentLayerWeight, targetWeight, fadeSpeed * Time.deltaTime);
            ApplyLayerWeight(currentLayerWeight);
        }

        private bool ShouldUseRevolverUpperBodyLayer()
        {
            if (IsControlLocked())
            {
                return false;
            }

            return revolverAnimationState != null && revolverAnimationState.RevolverAimHeld;
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
            bool aimHeld = !interactionLocked && revolverAnimationState.RevolverAimHeld;
            bool isReloading = !interactionLocked && revolverAnimationState.RevolverIsReloading;

            animator.SetBool(RevolverAimHeldHash, aimHeld);
            animator.SetBool(RevolverIsReloadingHash, isReloading);
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
                    return true;
                }
            }

            resolvedAnimator = null;
            return false;
        }

        #endregion
    }
}
