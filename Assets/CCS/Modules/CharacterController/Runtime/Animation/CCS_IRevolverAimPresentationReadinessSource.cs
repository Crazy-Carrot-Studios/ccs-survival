// =============================================================================
// SCRIPT: CCS_IRevolverAimPresentationReadinessSource
// CATEGORY: Modules / CharacterController / Runtime / Animation
// PURPOSE: Read-only aim presentation readiness contract for reticle gating.
// PLACEMENT: Implemented by CCS_SingleRevolverAimAnimator on player Model root.
// AUTHOR: James Schilz
// CREATED: 2026-06-30
// NOTES: Presentation-only. Does not drive gameplay aim, fire, ammo, damage, or ownership.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public interface CCS_IRevolverAimPresentationReadinessSource
    {
        bool IsAimPresentationActive { get; }

        bool IsAimPresentationReadyForReticle { get; }
    }
}
