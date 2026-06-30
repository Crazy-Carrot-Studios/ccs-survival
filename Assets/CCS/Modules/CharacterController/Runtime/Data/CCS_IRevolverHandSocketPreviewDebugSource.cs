// =============================================================================
// SCRIPT: CCS_IRevolverHandSocketPreviewDebugSource
// CATEGORY: Modules / CharacterController / Runtime / Data
// PURPOSE: Read-only validation-scene source for right-hand revolver socket preview testing.
// PLACEMENT: Implemented by CCS_CharacterControllerDiagnosticsManager on validation scenes only.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: Presentation-only. Does not drive aim animation, fire, ammo, damage, or ownership.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public interface CCS_IRevolverHandSocketPreviewDebugSource
    {
        bool ForceRevolverHandSocketPreview { get; }
    }
}
