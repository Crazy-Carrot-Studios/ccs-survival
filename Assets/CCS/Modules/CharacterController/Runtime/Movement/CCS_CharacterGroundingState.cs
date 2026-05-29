// =============================================================================
// SCRIPT: CCS_CharacterGroundingState
// CATEGORY: Modules / CharacterController / Runtime / Movement
// PURPOSE: Ground contact classification for CharacterController movement.
// PLACEMENT: Used by motor, service, and grounding change events.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Derived from CharacterController.isGrounded and vertical velocity.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public enum CCS_CharacterGroundingState
    {
        Grounded = 0,
        Airborne = 1
    }
}
