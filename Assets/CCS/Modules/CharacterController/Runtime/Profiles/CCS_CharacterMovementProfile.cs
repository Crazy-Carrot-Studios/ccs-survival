using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CharacterMovementProfile
// CATEGORY: Modules / CharacterController / Runtime / Profiles
// PURPOSE: Movement tuning and Unity CharacterController capsule settings.
// PLACEMENT: Embedded on CCS_CharacterControllerProfile or referenced as sub-asset later.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Uses CharacterController only. Animator root motion must remain OFF.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    [System.Serializable]
    public sealed class CCS_CharacterMovementProfile
    {
        #region Variables

        [Header("Locomotion Speeds")]
        [Tooltip("Meters per second while walking.")]
        [SerializeField] private float walkSpeed = 4f;

        [Tooltip("Meters per second while running/sprinting.")]
        [SerializeField] private float runSpeed = 7f;

        [Tooltip("Meters per second while crouching.")]
        [SerializeField] private float crouchSpeed = 2f;

        [Header("Jump And Gravity")]
        [Tooltip("Desired jump apex height in meters.")]
        [SerializeField] private float jumpHeight = 1.2f;

        [Tooltip("Gravity acceleration (negative for downward).")]
        [SerializeField] private float gravity = -20f;

        [Tooltip("Small downward velocity applied while grounded to keep contact with slopes.")]
        [SerializeField] private float groundedStickForce = -2f;

        [Header("Character Controller Capsule")]
        [Tooltip("CharacterController height in meters.")]
        [SerializeField] private float controllerHeight = 1.8f;

        [Tooltip("CharacterController radius in meters.")]
        [SerializeField] private float controllerRadius = 0.35f;

        [Tooltip("Maximum step height handled by CharacterController.")]
        [SerializeField] private float stepOffset = 0.35f;

        [Tooltip("Maximum walkable slope angle in degrees.")]
        [SerializeField] private float slopeLimit = 45f;

        [Header("Survival Core Hooks")]
        [Tooltip("Stamina drain per second while sprinting. Raised via movement events for composition wiring.")]
        [SerializeField] private float staminaDrainPerSecondWhileSprinting = 4f;

        [Tooltip("Stamina cost applied once when a jump succeeds.")]
        [SerializeField] private float jumpStaminaCost = 5f;

        #endregion

        #region Properties

        public float WalkSpeed => walkSpeed;

        public float RunSpeed => runSpeed;

        public float CrouchSpeed => crouchSpeed;

        public float JumpHeight => jumpHeight;

        public float Gravity => gravity;

        public float GroundedStickForce => groundedStickForce;

        public float ControllerHeight => controllerHeight;

        public float ControllerRadius => controllerRadius;

        public float StepOffset => stepOffset;

        public float SlopeLimit => slopeLimit;

        public float StaminaDrainPerSecondWhileSprinting => staminaDrainPerSecondWhileSprinting;

        public float JumpStaminaCost => jumpStaminaCost;

        #endregion
    }
}
