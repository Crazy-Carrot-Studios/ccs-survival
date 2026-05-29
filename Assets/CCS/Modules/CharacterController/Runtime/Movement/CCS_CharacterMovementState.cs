// =============================================================================
// SCRIPT: CCS_CharacterMovementState
// CATEGORY: Modules / CharacterController / Runtime / Movement
// PURPOSE: High-level locomotion states for character controller movement.
// PLACEMENT: Used by CCS_CharacterMovementService and movement snapshots.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Placeholder states for 0.3.8 foundation. No combat or interaction states.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public enum CCS_CharacterMovementState
    {
        Idle = 0,
        Walking = 1,
        Running = 2,
        Crouching = 3,
        Jumping = 4,
        Falling = 5
    }
}
