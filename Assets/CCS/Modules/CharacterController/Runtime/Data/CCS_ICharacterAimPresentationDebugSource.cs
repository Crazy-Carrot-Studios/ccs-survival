// =============================================================================
// SCRIPT: CCS_ICharacterAimPresentationDebugSource
// CATEGORY: Modules / CharacterController / Runtime / Data
// PURPOSE: Read-only validation-scene source for forced aim presentation testing.
// PLACEMENT: Implemented by CCS_CharacterControllerDiagnosticsManager on validation scenes only.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: Presentation-only. Does not drive gameplay aim, fire, ammo, or damage.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public interface CCS_ICharacterAimPresentationDebugSource
    {
        bool ForceAimPresentation { get; }
    }
}
