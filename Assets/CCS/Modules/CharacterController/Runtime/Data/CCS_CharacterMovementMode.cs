// =============================================================================
// SCRIPT: CCS_CharacterMovementMode
// CATEGORY: Modules / CharacterController / Runtime / Data
// PURPOSE: Defines supported character movement modes for the controller module.
// PLACEMENT: Runtime data enum. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Only GroundedThirdPerson is active in v0.2.0.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public enum CCS_CharacterMovementMode
    {
        GroundedThirdPerson = 0,
        Swimming = 1,
        Mounted = 2,
        Vehicle = 3,
        Disabled = 4
    }
}
