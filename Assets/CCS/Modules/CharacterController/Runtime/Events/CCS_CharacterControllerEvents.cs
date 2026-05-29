// =============================================================================
// SCRIPT: CCS_CharacterControllerEvents
// CATEGORY: Modules / CharacterController / Runtime / Events
// PURPOSE: Event name constants and delegate contracts for character controller systems.
// PLACEMENT: Instance events on CCS_CharacterMovementService document contracts here.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: OnStaminaDrainRequested does not modify Survival Core stats directly.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public static class CCS_CharacterControllerEvents
    {
        public const string MovementStateChangedEventName = "CharacterController.MovementStateChanged";
        public const string GroundedStateChangedEventName = "CharacterController.GroundedStateChanged";
        public const string JumpedEventName = "CharacterController.Jumped";
        public const string LandedEventName = "CharacterController.Landed";
        public const string SprintStateChangedEventName = "CharacterController.SprintStateChanged";
        public const string CrouchStateChangedEventName = "CharacterController.CrouchStateChanged";
        public const string StaminaDrainRequestedEventName = "CharacterController.StaminaDrainRequested";
    }

    public delegate void CharacterMovementStateChangedHandler(CCS_CharacterMovementEventArgs eventArgs);

    public delegate void CharacterGroundedStateChangedHandler(
        CCS_CharacterGroundingState previousState,
        CCS_CharacterGroundingState currentState);

    public delegate void CharacterJumpedHandler(CCS_CharacterMovementSnapshot snapshot);

    public delegate void CharacterLandedHandler(CCS_CharacterMovementSnapshot snapshot);

    public delegate void CharacterSprintStateChangedHandler(bool isSprinting);

    public delegate void CharacterCrouchStateChangedHandler(bool isCrouching);

    public delegate void CharacterStaminaDrainRequestedHandler(float drainPerSecond);
}
