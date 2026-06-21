using CCS.Modules.Interaction;

using UnityEngine;

// =============================================================================
// SCRIPT: CCS_InteractionAnimationStateExitBehaviour
// CATEGORY: Modules / CharacterController / Runtime / Visuals
// PURPOSE: Releases interaction movement lock when an interact Animator state exits.
// PLACEMENT: Interact_PickUp_RH and Interact_WalkThroughDoor_RH states on player AC.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Normal unlock path. Fallback timer on CCS_PlayerInteractionAnimator is safety only.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public sealed class CCS_InteractionAnimationStateExitBehaviour : StateMachineBehaviour
    {
        #region Variables

        [SerializeField] private CCS_InteractionAnimationKey animationKey;

        #endregion

        #region StateMachineBehaviour

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            CCS_PlayerInteractionAnimator interactionAnimator =
                animator.GetComponentInParent<CCS_PlayerInteractionAnimator>();
            interactionAnimator?.NotifyInteractionAnimationStateEntered(animationKey);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            CCS_PlayerInteractionAnimator interactionAnimator =
                animator.GetComponentInParent<CCS_PlayerInteractionAnimator>();
            if (interactionAnimator == null)
            {
                return;
            }

            interactionAnimator.NotifyInteractionAnimationStateExited(animationKey);
        }

        #endregion
    }
}
