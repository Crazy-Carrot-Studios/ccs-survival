// =============================================================================
// SCRIPT: CCS_IRevolverAimPresentationInput
// CATEGORY: Modules / CharacterController / Runtime / Animation
// PURPOSE: Read-only shared aim presentation state for stripped baseline consumers.
// PLACEMENT: Implemented by CCS_RevolverAimPresentationGate.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: Presentation-only. No fire, damage, ammo, or ownership.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public interface CCS_IRevolverAimPresentationInput
    {
        bool IsAimPresentationActive { get; }

        bool IsAimPresentationRequested { get; }
    }
}
