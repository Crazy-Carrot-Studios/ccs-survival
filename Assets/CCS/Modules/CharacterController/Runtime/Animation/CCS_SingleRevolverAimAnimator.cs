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
    public sealed class CCS_SingleRevolverAimAnimator : MonoBehaviour,
        CCS_ICharacterAnimationPresenter,
        CCS_IRevolverAimPresentationReadinessSource
    {
        private const float HolsterLayerWeightFadeThreshold = 0.001f;

        [SerializeField] private Animator animator;
        [SerializeField] private Component revolverAnimationStateComponent;
        [SerializeField] private Component revolverAimSetupPoseDebugSourceComponent;
        [SerializeField] private CCS_RevolverReticlePresentationProfile reticlePresentationProfile;
        [SerializeField] private string upperBodyLayerName = CCS_CharacterControllerConstants.SingleRevolverUpperBodyLayerName;

        private CCS_IRevolverAnimationState revolverAnimationState;
        private CCS_IRevolverAimSetupPoseDebugSource revolverAimSetupPoseDebugSource;
        private bool resolvedRevolverAimSetupPoseDebugSource;
        private int upperBodyLayerIndex = -1;
        private int isAimingHash;
        private int revolverDrawTriggerHash;
        private int revolverHolsterTriggerHash;
        private int upperBodyEmptyStateHash;
        private int revolverDrawStateHash;
        private int revolverAimHoldStateHash;
        private int revolverHolsterStateHash;
        private bool presentationEnabled;
        private bool aimPresentationActive;
        private bool aimPresentationReadyForReticle;
        private bool aimPresentationInReticleRevealWindow;
        private bool reticleRevealEventReceived;
        private bool loggedMissingSetup;
        private bool previousDesiredPresentationAiming;
        private bool holsterPresentationActive;
        private bool previousRevealWindow;
        private bool previousReadyForReticle;

        public bool IsAimPresentationActive => aimPresentationActive;

        public bool IsAimPresentationReadyForReticle => aimPresentationReadyForReticle;

        public bool IsAimPresentationInReticleRevealWindow => aimPresentationInReticleRevealWindow;

        public void NotifyRevolverAimHoldAnimationEvent()
        {
            if (!presentationEnabled || holsterPresentationActive || !ResolveDesiredPresentationAiming())
            {
                return;
            }

            reticleRevealEventReceived = true;
        }

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
            previousDesiredPresentationAiming = false;
            holsterPresentationActive = false;
            aimPresentationActive = false;
            aimPresentationReadyForReticle = false;
            aimPresentationInReticleRevealWindow = false;
            reticleRevealEventReceived = false;
            previousRevealWindow = false;
            previousReadyForReticle = false;
        }

        private void LateUpdate()
        {
            if (!presentationEnabled || revolverAnimationState == null || animator == null || upperBodyLayerIndex < 0)
            {
                aimPresentationActive = false;
                aimPresentationReadyForReticle = false;
                aimPresentationInReticleRevealWindow = false;
                return;
            }

            bool debugSetupPoseActive = IsDebugRevolverAimSetupPoseActive();
            if (!revolverAnimationState.IsRevolverOwned && !debugSetupPoseActive)
            {
                if (previousDesiredPresentationAiming || holsterPresentationActive || animator.GetLayerWeight(upperBodyLayerIndex) > HolsterLayerWeightFadeThreshold)
                {
                    ResetPresentationImmediate();
                }

                aimPresentationActive = false;
                aimPresentationReadyForReticle = false;
                aimPresentationInReticleRevealWindow = false;
                reticleRevealEventReceived = false;
                return;
            }

            bool desiredPresentationAiming = ResolveDesiredPresentationAiming();
            if (desiredPresentationAiming != previousDesiredPresentationAiming)
            {
                if (desiredPresentationAiming)
                {
                    BeginAimPresentation();
                }
                else
                {
                    BeginHolsterPresentation();
                }

                previousDesiredPresentationAiming = desiredPresentationAiming;
            }
            else if (desiredPresentationAiming)
            {
                animator.SetBool(isAimingHash, true);
            }

            UpdateHolsterLayerWeight();
            UpdateAimPresentationReadiness();
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

        private bool ResolveDesiredPresentationAiming()
        {
            bool gameplayAiming = revolverAnimationState.IsAiming;
            return gameplayAiming || IsDebugRevolverAimSetupPoseActive();
        }

        private bool IsDebugRevolverAimSetupPoseActive()
        {
            return TryResolveRevolverAimSetupPoseDebugSource(out CCS_IRevolverAimSetupPoseDebugSource debugSource)
                && debugSource.ForceRevolverAimSetupPose;
        }

        private bool TryResolveRevolverAimSetupPoseDebugSource(out CCS_IRevolverAimSetupPoseDebugSource debugSource)
        {
            if (resolvedRevolverAimSetupPoseDebugSource)
            {
                debugSource = revolverAimSetupPoseDebugSource;
                return debugSource != null;
            }

            resolvedRevolverAimSetupPoseDebugSource = true;
            if (revolverAimSetupPoseDebugSourceComponent is CCS_IRevolverAimSetupPoseDebugSource fromComponent)
            {
                revolverAimSetupPoseDebugSource = fromComponent;
            }
            else
            {
                revolverAimSetupPoseDebugSource = CCS_RevolverAimSetupPoseDebugRegistry.ActiveSource;
            }

            debugSource = revolverAimSetupPoseDebugSource;
            return debugSource != null;
        }

        private void CacheAnimatorContract()
        {
            isAimingHash = CCS_CharacterAnimationParameterIds.Active.IsAimingHash;
            revolverDrawTriggerHash = CCS_CharacterAnimationParameterIds.Active.RevolverDrawTriggerHash;
            revolverHolsterTriggerHash = CCS_CharacterAnimationParameterIds.Active.RevolverHolsterTriggerHash;
            upperBodyEmptyStateHash = Animator.StringToHash(CCS_CharacterControllerConstants.SingleRevolverUpperBodyEmptyStateName);
            revolverDrawStateHash = Animator.StringToHash(CCS_CharacterControllerConstants.SingleRevolverDrawStateName);
            revolverAimHoldStateHash = Animator.StringToHash(CCS_CharacterControllerConstants.SingleRevolverAimHoldStateName);
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
            reticleRevealEventReceived = false;
            animator.SetLayerWeight(upperBodyLayerIndex, 1f);
            animator.SetBool(isAimingHash, true);
            animator.ResetTrigger(revolverHolsterTriggerHash);
            animator.SetTrigger(revolverDrawTriggerHash);
        }

        private void BeginHolsterPresentation()
        {
            holsterPresentationActive = true;
            reticleRevealEventReceived = false;
            aimPresentationReadyForReticle = false;
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
            previousDesiredPresentationAiming = false;
            holsterPresentationActive = false;
            aimPresentationActive = false;
            aimPresentationReadyForReticle = false;
            aimPresentationInReticleRevealWindow = false;
            reticleRevealEventReceived = false;
            previousRevealWindow = false;
            previousReadyForReticle = false;
        }

        private void UpdateAimPresentationReadiness()
        {
            bool desiredPresentationAiming = ResolveDesiredPresentationAiming();
            aimPresentationActive = desiredPresentationAiming || holsterPresentationActive;

            if (!desiredPresentationAiming || holsterPresentationActive)
            {
                aimPresentationReadyForReticle = false;
                aimPresentationInReticleRevealWindow = false;
                reticleRevealEventReceived = false;
                LogReadinessTransitions();
                return;
            }

            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(upperBodyLayerIndex);
            if (animator.IsInTransition(upperBodyLayerIndex))
            {
                aimPresentationReadyForReticle = false;
                aimPresentationInReticleRevealWindow = false;
                LogReadinessTransitions();
                return;
            }

            int stateHash = stateInfo.shortNameHash;
            if (stateHash == revolverHolsterStateHash)
            {
                aimPresentationReadyForReticle = false;
                aimPresentationInReticleRevealWindow = false;
                reticleRevealEventReceived = false;
                LogReadinessTransitions();
                return;
            }

            bool inHoldState = stateHash == revolverAimHoldStateHash;
            CCS_RevolverReticleRevealSource revealSource = ResolveReticleRevealSource();
            switch (revealSource)
            {
                case CCS_RevolverReticleRevealSource.StateReadiness:
                    UpdateStateReadinessReveal(stateInfo, inHoldState);
                    break;

                case CCS_RevolverReticleRevealSource.AnimationEventWithStateFallback:
                    aimPresentationReadyForReticle = reticleRevealEventReceived || inHoldState;
                    aimPresentationInReticleRevealWindow = false;
                    break;

                default:
                    aimPresentationReadyForReticle = reticleRevealEventReceived;
                    aimPresentationInReticleRevealWindow = false;
                    break;
            }

            LogReadinessTransitions();
        }

        private void UpdateStateReadinessReveal(AnimatorStateInfo stateInfo, bool inHoldState)
        {
            int stateHash = stateInfo.shortNameHash;
            if (inHoldState)
            {
                aimPresentationReadyForReticle = true;
                aimPresentationInReticleRevealWindow = true;
                return;
            }

            if (stateHash == revolverDrawStateHash && IsRevealDuringDrawEnabled())
            {
                float drawNormalizedTime = stateInfo.normalizedTime % 1f;
                float revealThreshold = ResolveDrawRevealNormalizedThreshold(stateInfo.length);
                bool inRevealWindow = drawNormalizedTime >= revealThreshold;
                aimPresentationReadyForReticle = false;
                aimPresentationInReticleRevealWindow = inRevealWindow;
                LogReadinessTransitions(inRevealWindow && !previousRevealWindow);
                return;
            }

            aimPresentationReadyForReticle = false;
            aimPresentationInReticleRevealWindow = false;
        }

        private CCS_RevolverReticleRevealSource ResolveReticleRevealSource()
        {
            return reticlePresentationProfile != null
                ? reticlePresentationProfile.ReticleRevealSource
                : CCS_RevolverReticleRevealSource.AnimationEvent;
        }

        private bool IsRevealDuringDrawEnabled()
        {
            if (reticlePresentationProfile == null)
            {
                return false;
            }

            return reticlePresentationProfile.ReticleRevealSource == CCS_RevolverReticleRevealSource.StateReadiness
                && reticlePresentationProfile.RevealDuringDraw;
        }

        private float ResolveDrawRevealNormalizedThreshold(float drawClipLengthSeconds)
        {
            if (reticlePresentationProfile != null)
            {
                return reticlePresentationProfile.ComputeDrawRevealNormalizedThreshold(drawClipLengthSeconds);
            }

            return 0.55f;
        }

        private void LogReadinessTransitions(bool forceRevealLog = false)
        {
            if (!CCS_AimPresentationDiagnosticsRegistry.EnableReticleTransitionLogging)
            {
                previousRevealWindow = aimPresentationInReticleRevealWindow;
                previousReadyForReticle = aimPresentationReadyForReticle;
                return;
            }

            if (forceRevealLog
                || (aimPresentationInReticleRevealWindow && !previousRevealWindow))
            {
                Debug.Log("[Reticle Presentation] Reveal window reached during draw.", this);
            }
            else if (aimPresentationReadyForReticle && !previousReadyForReticle)
            {
                Debug.Log("[Reticle Presentation] Aim hold animation event received.", this);
            }
            else if (!aimPresentationInReticleRevealWindow && previousRevealWindow && holsterPresentationActive)
            {
                Debug.Log("[Reticle Presentation] Holster started — reticle hidden.", this);
            }

            previousRevealWindow = aimPresentationInReticleRevealWindow;
            previousReadyForReticle = aimPresentationReadyForReticle;
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
