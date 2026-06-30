// =============================================================================
// SCRIPT: CCS_IRevolverAimSetupPoseDebugSource
// CATEGORY: Modules / CharacterController / Runtime / Data
// PURPOSE: Read-only validation-scene source for revolver aim setup pose testing.
// PLACEMENT: Implemented by CCS_CharacterControllerDiagnosticsManager on validation scenes only.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: Presentation-only. Does not drive gameplay aim, fire, ammo, damage, or ownership.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public interface CCS_IRevolverAimSetupPoseDebugSource
    {
        bool ForceRevolverAimSetupPose { get; }
    }
}
