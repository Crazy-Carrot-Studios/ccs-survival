using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SingleRevolverAimAnimator
// CATEGORY: Modules / CharacterController / Runtime / Animation
// PURPOSE: Presentation-only single revolver draw/hold/holster upper-body Animator driver.
// PLACEMENT: PF_CCS_CharacterController_Player_Networked / Model (presentation branch).
// AUTHOR: James Schilz
// CREATED: 2026-06-29
// NOTES: Does not own aim/fire gameplay. Disables gracefully when layer or parameters are missing.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    [DefaultExecutionOrder(210)]
    public sealed class CCS_SingleRevolverAimAnimator : MonoBehaviour, CCS_ICharacterAnimationPresenter
    {
        private const float HolsterLayerWeightFadeThreshold = 0.001f;

        [SerializeField] private Animator animator;
        [SerializeField] private Component revolverAnimationStateComponent;
        [SerializeField] private string upperBodyLayerName = CCS_CharacterControllerConstants.SingleRevolverUpperBodyLayerName;

        private CCS_IRevolverAnimationState revolverAnimationState;
        private int upperBodyLayerIndex = -1;
        private int isAimingHash;
        private int revolverDrawTriggerHash;
        private int revolverHolsterTriggerHash;
        private int upperBodyEmptyStateHash;
        private int revolverHolsterStateHash;
        private bool presentationEnabled;
        private bool loggedMissingSetup;
        private bool previousGameplayAiming;
        private bool holsterPresentationActive;

        private void Awake()
        {
            ResolveReferences();
            CacheAnimatorContract();
        }

        private void OnDisable()
        {
            if (!presentationEnabled || animator == null || upperBodyLayerIndex < 0)
            {
                return;
            }

            animator.SetBool(isAimingHash, false);
            animator.ResetTrigger(revolverDrawTriggerHash);
            animator.ResetTrigger(revolverHolsterTriggerHash);
            animator.SetLayerWeight(upperBodyLayerIndex, 0f);
            previousGameplayAiming = false;
            holsterPresentationActive = false;
        }

        private void LateUpdate()
        {
            if (!presentationEnabled || revolverAnimationState == null || animator == null || upperBodyLayerIndex < 0)
            {
                return;
            }

            if (!revolverAnimationState.IsRevolverOwned)
            {
                if (previousGameplayAiming || holsterPresentationActive || animator.GetLayerWeight(upperBodyLayerIndex) > HolsterLayerWeightFadeThreshold)
                {
                    ResetPresentationImmediate();
                }

                return;
            }

            bool gameplayAiming = revolverAnimationState.IsAiming;
            if (gameplayAiming != previousGameplayAiming)
            {
                if (gameplayAiming)
                {
                    BeginAimPresentation();
                }
                else
                {
                    BeginHolsterPresentation();
                }

                previousGameplayAiming = gameplayAiming;
            }
            else if (gameplayAiming)
            {
                animator.SetBool(isAimingHash, true);
            }

            UpdateHolsterLayerWeight();
        }

        public void SetLocomotion(float speedNormalized, bool isGrounded, bool isSprinting)
        {
        }

        public void SetGrounded(bool isGrounded)
        {
        }

        public void TriggerJump()
        {
        }

        public void SetWeaponMode(CCS_CharacterWeaponAnimationMode mode)
        {
        }

        public void SetAimingPresentation(bool isAiming)
        {
        }

        public void TriggerInteractionPresentation(int interactionTypeId)
        {
        }

        public void TriggerFirePresentation()
        {
        }

        public void TriggerReloadPresentation()
        {
        }

        private void ResolveReferences()
        {
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>(true);
            }

            if (revolverAnimationStateComponent != null
                && revolverAnimationStateComponent is CCS_IRevolverAnimationState fromComponent)
            {
                revolverAnimationState = fromComponent;
            }
            else if (revolverAnimationState == null)
            {
                revolverAnimationState = GetComponentInParent<CCS_IRevolverAnimationState>();
            }
        }

        private void CacheAnimatorContract()
        {
            isAimingHash = CCS_CharacterAnimationParameterIds.Active.IsAimingHash;
            revolverDrawTriggerHash = CCS_CharacterAnimationParameterIds.Active.RevolverDrawTriggerHash;
            revolverHolsterTriggerHash = CCS_CharacterAnimationParameterIds.Active.RevolverHolsterTriggerHash;
            upperBodyEmptyStateHash = Animator.StringToHash(CCS_CharacterControllerConstants.SingleRevolverUpperBodyEmptyStateName);
            revolverHolsterStateHash = Animator.StringToHash(CCS_CharacterControllerConstants.SingleRevolverHolsterStateName);

            if (animator == null || animator.runtimeAnimatorController == null)
            {
                DisablePresentation("[CCS Single Revolver Aim Animator] Missing Animator or runtime controller.");
                return;
            }

            upperBodyLayerIndex = animator.GetLayerIndex(upperBodyLayerName);
            if (upperBodyLayerIndex < 0)
            {
                DisablePresentation(
                    "[CCS Single Revolver Aim Animator] Missing Animator layer '"
                    + upperBodyLayerName
                    + "'. Presentation disabled.");
                return;
            }

            if (!HasRequiredParameters(animator))
            {
                DisablePresentation(
                    "[CCS Single Revolver Aim Animator] Missing required aim presentation parameters. Presentation disabled.");
                return;
            }

            presentationEnabled = true;
            animator.SetLayerWeight(upperBodyLayerIndex, 0f);
        }

        private static bool HasRequiredParameters(Animator targetAnimator)
        {
            AnimatorControllerParameter[] parameters = targetAnimator.parameters;
            bool hasIsAiming = false;
            bool hasDrawTrigger = false;
            bool hasHolsterTrigger = false;

            for (int i = 0; i < parameters.Length; i++)
            {
                string parameterName = parameters[i].name;
                if (parameterName == CCS_CharacterAnimationParameterIds.Active.IsAiming)
                {
                    hasIsAiming = true;
                }
                else if (parameterName == CCS_CharacterAnimationParameterIds.Active.RevolverDrawTrigger)
                {
                    hasDrawTrigger = true;
                }
                else if (parameterName == CCS_CharacterAnimationParameterIds.Active.RevolverHolsterTrigger)
                {
                    hasHolsterTrigger = true;
                }
            }

            return hasIsAiming && hasDrawTrigger && hasHolsterTrigger;
        }

        private void BeginAimPresentation()
        {
            holsterPresentationActive = false;
            animator.SetLayerWeight(upperBodyLayerIndex, 1f);
            animator.SetBool(isAimingHash, true);
            animator.ResetTrigger(revolverHolsterTriggerHash);
            animator.SetTrigger(revolverDrawTriggerHash);
        }

        private void BeginHolsterPresentation()
        {
            holsterPresentationActive = true;
            animator.SetBool(isAimingHash, false);
            animator.ResetTrigger(revolverDrawTriggerHash);
            animator.SetTrigger(revolverHolsterTriggerHash);
            animator.SetLayerWeight(upperBodyLayerIndex, 1f);
        }

        private void UpdateHolsterLayerWeight()
        {
            if (!holsterPresentationActive)
            {
                return;
            }

            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(upperBodyLayerIndex);
            if (stateInfo.shortNameHash == upperBodyEmptyStateHash
                && stateInfo.normalizedTime >= 0f
                && !animator.IsInTransition(upperBodyLayerIndex))
            {
                animator.SetLayerWeight(upperBodyLayerIndex, 0f);
                holsterPresentationActive = false;
            }
            else if (stateInfo.shortNameHash == revolverHolsterStateHash
                     && stateInfo.normalizedTime >= 0.99f
                     && !animator.IsInTransition(upperBodyLayerIndex))
            {
                animator.SetLayerWeight(upperBodyLayerIndex, 0f);
                holsterPresentationActive = false;
            }
        }

        private void ResetPresentationImmediate()
        {
            animator.SetBool(isAimingHash, false);
            animator.ResetTrigger(revolverDrawTriggerHash);
            animator.ResetTrigger(revolverHolsterTriggerHash);
            animator.SetLayerWeight(upperBodyLayerIndex, 0f);
            previousGameplayAiming = false;
            holsterPresentationActive = false;
        }

        private void DisablePresentation(string warningMessage)
        {
            presentationEnabled = false;
            if (!loggedMissingSetup)
            {
                loggedMissingSetup = true;
                Debug.LogWarning(warningMessage, this);
            }
        }
    }
}
