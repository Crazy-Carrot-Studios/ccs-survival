// =============================================================================
// SCRIPT: CCS_CharacterMovementMode
// CATEGORY: Modules / CharacterController / Runtime / Data
// PURPOSE: Defines supported character movement modes for the controller module.
// PLACEMENT: Runtime data enum. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: GroundedThirdPerson and AimStrafeLocomotion are active in v0.6.3.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public enum CCS_CharacterMovementMode
    {
        GroundedThirdPerson = 0,
        Swimming = 1,
        Mounted = 2,
        Vehicle = 3,
        Disabled = 4,
        AimStrafeLocomotion = 5
    }
}
