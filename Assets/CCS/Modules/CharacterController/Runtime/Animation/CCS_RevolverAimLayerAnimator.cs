using UnityEngine;



// =============================================================================

// SCRIPT: CCS_RevolverAimLayerAnimator

// CATEGORY: Modules / CharacterController / Runtime / Animation

// PURPOSE: Presentation-only revolver draw/hold/holster upper-body Animator driver.

// PLACEMENT: PF_CCS_CharacterController_Player_Networked / Model.

// AUTHOR: James Schilz

// CREATED: 2026-06-25

// NOTES: v0.7.13 drives right layer and experimental mirrored left layer. No gameplay weapons.

// =============================================================================



namespace CCS.Modules.CharacterController

{

    [DefaultExecutionOrder(210)]

    public sealed class CCS_RevolverAimLayerAnimator : MonoBehaviour, CCS_ICharacterAnimationPresenter

    {

        private struct AimLayerChannel

        {

            public int LayerIndex;

            public int IsAimingHash;

            public int DrawTriggerHash;

            public int HolsterTriggerHash;

            public int EmptyStateHash;

            public int HolsterStateHash;

            public bool HolsterPresentationActive;

        }



        [SerializeField] private Animator animator;

        [SerializeField] private Component aimPresentationInputComponent;

        [SerializeField] private string upperBodyLayerName = CCS_CharacterControllerConstants.SingleRevolverUpperBodyLayerName;

        [SerializeField] private string experimentalLeftUpperBodyLayerName =

            CCS_CharacterControllerConstants.SingleRevolverLeftUpperBodyLayerName;



        private CCS_IRevolverAimPresentationInput aimPresentationInput;

        private AimLayerChannel rightLayerChannel;

        private AimLayerChannel leftLayerChannel;

        private bool rightPresentationEnabled;

        private bool leftPresentationEnabled;

        private bool previousDesiredPresentationAiming;

        private bool loggedMissingSetup;



        private void Awake()

        {

            ResolveReferences();

            CacheAnimatorContract();

        }



        private void OnDisable()

        {

            ResetLayerChannel(rightLayerChannel, rightPresentationEnabled);

            ResetLayerChannel(leftLayerChannel, leftPresentationEnabled);

            previousDesiredPresentationAiming = false;

        }



        private void LateUpdate()

        {

            if (aimPresentationInput == null || animator == null)

            {

                return;

            }



            bool desiredPresentationAiming = aimPresentationInput.IsAimPresentationActive;

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

                HoldAimPresentation();

            }



            UpdateHolsterLayerWeight(ref rightLayerChannel, rightPresentationEnabled);

            UpdateHolsterLayerWeight(ref leftLayerChannel, leftPresentationEnabled);

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



            if (aimPresentationInputComponent is CCS_IRevolverAimPresentationInput fromComponent)

            {

                aimPresentationInput = fromComponent;

            }

            else if (aimPresentationInput == null)

            {

                aimPresentationInput = GetComponentInParent<CCS_IRevolverAimPresentationInput>();

            }

        }



        private void CacheAnimatorContract()

        {

            if (animator == null || animator.runtimeAnimatorController == null)

            {

                DisablePresentation("[Revolver Aim Layer Animator] Missing Animator or runtime controller.");

                return;

            }



            rightPresentationEnabled = TryCacheRightLayerChannel(out rightLayerChannel);

            leftPresentationEnabled = TryCacheLeftLayerChannel(out leftLayerChannel);



            if (!rightPresentationEnabled)

            {

                DisablePresentation("[Revolver Aim Layer Animator] Missing required right aim presentation layer.");

            }

        }



        private bool TryCacheRightLayerChannel(out AimLayerChannel channel)

        {

            channel = default;

            if (!HasRequiredRightParameters(animator))

            {

                return false;

            }



            int layerIndex = animator.GetLayerIndex(upperBodyLayerName);

            if (layerIndex < 0)

            {

                return false;

            }



            channel = BuildLayerChannel(

                layerIndex,

                CCS_CharacterAnimationParameterIds.Active.IsAimingHash,

                CCS_CharacterAnimationParameterIds.Active.RevolverDrawTriggerHash,

                CCS_CharacterAnimationParameterIds.Active.RevolverHolsterTriggerHash,

                CCS_CharacterControllerConstants.SingleRevolverUpperBodyEmptyStateName,

                CCS_CharacterControllerConstants.SingleRevolverHolsterStateName);

            animator.SetLayerWeight(layerIndex, 0f);

            return true;

        }



        private bool TryCacheLeftLayerChannel(out AimLayerChannel channel)

        {

            channel = default;

            if (!HasRequiredLeftParameters(animator))

            {

                return false;

            }



            int layerIndex = animator.GetLayerIndex(experimentalLeftUpperBodyLayerName);

            if (layerIndex < 0)

            {

                return false;

            }



            channel = BuildLayerChannel(

                layerIndex,

                CCS_CharacterAnimationParameterIds.ExperimentalLeft.LeftIsAimingHash,

                CCS_CharacterAnimationParameterIds.ExperimentalLeft.LeftRevolverDrawTriggerHash,

                CCS_CharacterAnimationParameterIds.ExperimentalLeft.LeftRevolverHolsterTriggerHash,

                CCS_CharacterControllerConstants.SingleRevolverLeftUpperBodyEmptyStateName,

                CCS_CharacterControllerConstants.SingleRevolverLeftHolsterStateName);

            animator.SetLayerWeight(layerIndex, 0f);

            return true;

        }



        private static AimLayerChannel BuildLayerChannel(

            int layerIndex,

            int isAimingHash,

            int drawTriggerHash,

            int holsterTriggerHash,

            string emptyStateName,

            string holsterStateName)

        {

            return new AimLayerChannel

            {

                LayerIndex = layerIndex,

                IsAimingHash = isAimingHash,

                DrawTriggerHash = drawTriggerHash,

                HolsterTriggerHash = holsterTriggerHash,

                EmptyStateHash = Animator.StringToHash(emptyStateName),

                HolsterStateHash = Animator.StringToHash(holsterStateName),

            };

        }



        private static bool HasRequiredRightParameters(Animator targetAnimator)

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



        private static bool HasRequiredLeftParameters(Animator targetAnimator)

        {

            AnimatorControllerParameter[] parameters = targetAnimator.parameters;

            bool hasIsAiming = false;

            bool hasDrawTrigger = false;

            bool hasHolsterTrigger = false;



            for (int i = 0; i < parameters.Length; i++)

            {

                string parameterName = parameters[i].name;

                if (parameterName == CCS_CharacterAnimationParameterIds.ExperimentalLeft.LeftIsAiming)

                {

                    hasIsAiming = true;

                }

                else if (parameterName == CCS_CharacterAnimationParameterIds.ExperimentalLeft.LeftRevolverDrawTrigger)

                {

                    hasDrawTrigger = true;

                }

                else if (parameterName == CCS_CharacterAnimationParameterIds.ExperimentalLeft.LeftRevolverHolsterTrigger)

                {

                    hasHolsterTrigger = true;

                }

            }



            return hasIsAiming && hasDrawTrigger && hasHolsterTrigger;

        }



        private void BeginAimPresentation()

        {

            BeginAimPresentationForChannel(ref rightLayerChannel, rightPresentationEnabled);

            BeginAimPresentationForChannel(ref leftLayerChannel, leftPresentationEnabled);

        }



        private void HoldAimPresentation()

        {

            HoldAimPresentationForChannel(rightLayerChannel, rightPresentationEnabled);

            HoldAimPresentationForChannel(leftLayerChannel, leftPresentationEnabled);

        }



        private void BeginHolsterPresentation()

        {

            BeginHolsterPresentationForChannel(ref rightLayerChannel, rightPresentationEnabled);

            BeginHolsterPresentationForChannel(ref leftLayerChannel, leftPresentationEnabled);

        }



        private void BeginAimPresentationForChannel(ref AimLayerChannel channel, bool enabled)

        {

            if (!enabled || channel.LayerIndex < 0)

            {

                return;

            }



            channel.HolsterPresentationActive = false;

            animator.SetLayerWeight(channel.LayerIndex, 1f);

            animator.SetBool(channel.IsAimingHash, true);

            animator.ResetTrigger(channel.HolsterTriggerHash);

            animator.SetTrigger(channel.DrawTriggerHash);

        }



        private void HoldAimPresentationForChannel(AimLayerChannel channel, bool enabled)

        {

            if (!enabled || channel.LayerIndex < 0)

            {

                return;

            }



            animator.SetBool(channel.IsAimingHash, true);

        }



        private void BeginHolsterPresentationForChannel(ref AimLayerChannel channel, bool enabled)

        {

            if (!enabled || channel.LayerIndex < 0)

            {

                return;

            }



            channel.HolsterPresentationActive = true;

            animator.SetBool(channel.IsAimingHash, false);

            animator.ResetTrigger(channel.DrawTriggerHash);

            animator.SetTrigger(channel.HolsterTriggerHash);

            animator.SetLayerWeight(channel.LayerIndex, 1f);

        }



        private void UpdateHolsterLayerWeight(ref AimLayerChannel channel, bool enabled)

        {

            if (!enabled || !channel.HolsterPresentationActive || channel.LayerIndex < 0)

            {

                return;

            }



            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(channel.LayerIndex);

            if (stateInfo.shortNameHash == channel.EmptyStateHash

                && stateInfo.normalizedTime >= 0f

                && !animator.IsInTransition(channel.LayerIndex))

            {

                animator.SetLayerWeight(channel.LayerIndex, 0f);

                channel.HolsterPresentationActive = false;

            }

            else if (stateInfo.shortNameHash == channel.HolsterStateHash

                     && stateInfo.normalizedTime >= 0.99f

                     && !animator.IsInTransition(channel.LayerIndex))

            {

                animator.SetLayerWeight(channel.LayerIndex, 0f);

                channel.HolsterPresentationActive = false;

            }

        }



        private void ResetLayerChannel(AimLayerChannel channel, bool enabled)

        {

            if (!enabled || animator == null || channel.LayerIndex < 0)

            {

                return;

            }



            animator.SetBool(channel.IsAimingHash, false);

            animator.ResetTrigger(channel.DrawTriggerHash);

            animator.ResetTrigger(channel.HolsterTriggerHash);

            animator.SetLayerWeight(channel.LayerIndex, 0f);

        }



        private void DisablePresentation(string message)

        {

            rightPresentationEnabled = false;

            leftPresentationEnabled = false;

            if (!loggedMissingSetup)

            {

                loggedMissingSetup = true;

                Debug.LogWarning(message, this);

            }

        }

    }

}


