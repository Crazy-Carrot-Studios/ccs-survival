using UnityEngine;

// =============================================================================
// SCRIPT: CCS_RevolverReticleAnimationEventReceiver
// CATEGORY: Modules / CharacterController / Runtime / Animation
// PURPOSE: Receives Fulldraw_Idle animation event and forwards reticle reveal signal.
// PLACEMENT: Same GameObject as Kevin's Animator (presentation branch).
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: Presentation-only relay. Does not drive gameplay aim, fire, ammo, damage, or ownership.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public sealed class CCS_RevolverReticleAnimationEventReceiver : MonoBehaviour
    {
        [SerializeField] private CCS_SingleRevolverAimAnimator aimAnimator;

        private bool loggedMissingAimAnimator;

        private void Awake()
        {
            if (aimAnimator == null)
            {
                aimAnimator = GetComponentInParent<CCS_SingleRevolverAimAnimator>();
            }

            if (aimAnimator == null && !loggedMissingAimAnimator)
            {
                loggedMissingAimAnimator = true;
                Debug.LogWarning(
                    "[Revolver Reticle Animation Event Receiver] Missing CCS_SingleRevolverAimAnimator.",
                    this);
            }
        }

        public void CCS_OnRevolverAimHoldStarted()
        {
            if (aimAnimator == null)
            {
                return;
            }

            aimAnimator.NotifyRevolverAimHoldAnimationEvent();
        }
    }
}
