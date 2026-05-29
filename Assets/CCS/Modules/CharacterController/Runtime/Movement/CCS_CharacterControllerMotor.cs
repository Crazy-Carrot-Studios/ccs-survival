using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CharacterControllerMotor
// CATEGORY: Modules / CharacterController / Runtime / Movement
// PURPOSE: Unity CharacterController locomotion (walk/run/crouch/jump/gravity/slopes).
// PLACEMENT: Owned by CCS_CharacterMovementService. Requires scene CharacterController.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: No Rigidbody. Animator root motion must remain OFF — motor owns displacement.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public sealed class CCS_CharacterControllerMotor
    {
        #region Variables

        private UnityEngine.CharacterController characterController;
        private CCS_CharacterMovementProfile movementProfile;
        private float verticalVelocity;
        private bool wasGroundedLastFrame;

        #endregion

        #region Public Methods

        public void Initialize(UnityEngine.CharacterController controller, CCS_CharacterMovementProfile profile)
        {
            characterController = controller;
            movementProfile = profile;
            verticalVelocity = 0f;
            wasGroundedLastFrame = false;

            if (characterController != null && movementProfile != null)
            {
                ApplyCapsuleSettings();
            }
        }

        public CCS_CharacterMovementSnapshot Tick(
            CCS_CharacterMovementInput input,
            float deltaTime,
            out bool landedThisFrame,
            out bool jumpedThisFrame)
        {
            landedThisFrame = false;
            jumpedThisFrame = false;

            if (characterController == null || movementProfile == null || deltaTime <= 0f)
            {
                return new CCS_CharacterMovementSnapshot(
                    CCS_CharacterMovementState.Idle,
                    CCS_CharacterGroundingState.Airborne,
                    Vector3.zero,
                    0f,
                    false,
                    false,
                    false);
            }

            bool isGrounded = characterController.isGrounded;
            if (isGrounded && !wasGroundedLastFrame)
            {
                landedThisFrame = true;
            }

            bool isCrouching = input.CrouchHeld;
            bool isSprinting = input.SprintHeld && !isCrouching && input.HasPlanarInput;
            float targetSpeed = ResolveTargetSpeed(isCrouching, isSprinting, input.HasPlanarInput);

            Vector3 planarVelocity = input.WorldPlanarMove;
            if (planarVelocity.sqrMagnitude > 1f)
            {
                planarVelocity.Normalize();
            }

            planarVelocity *= targetSpeed;

            if (isGrounded)
            {
                if (input.JumpPressed)
                {
                    verticalVelocity = Mathf.Sqrt(movementProfile.JumpHeight * -2f * movementProfile.Gravity);
                    jumpedThisFrame = true;
                }
                else if (verticalVelocity < 0f)
                {
                    verticalVelocity = movementProfile.GroundedStickForce;
                }
            }
            else
            {
                verticalVelocity += movementProfile.Gravity * deltaTime;
            }

            Vector3 displacement = (planarVelocity + Vector3.up * verticalVelocity) * deltaTime;
            characterController.Move(displacement);

            isGrounded = characterController.isGrounded;
            wasGroundedLastFrame = isGrounded;

            CCS_CharacterGroundingState groundingState = isGrounded
                ? CCS_CharacterGroundingState.Grounded
                : CCS_CharacterGroundingState.Airborne;

            CCS_CharacterMovementState movementState = ResolveMovementState(
                input,
                isGrounded,
                isCrouching,
                isSprinting,
                jumpedThisFrame,
                planarVelocity.magnitude);

            Vector3 worldVelocity = displacement / deltaTime;
            return new CCS_CharacterMovementSnapshot(
                movementState,
                groundingState,
                worldVelocity,
                planarVelocity.magnitude,
                isSprinting,
                isCrouching,
                jumpedThisFrame);
        }

        public void ApplyCapsuleSettings()
        {
            if (characterController == null || movementProfile == null)
            {
                return;
            }

            characterController.height = movementProfile.ControllerHeight;
            characterController.radius = movementProfile.ControllerRadius;
            characterController.stepOffset = movementProfile.StepOffset;
            characterController.slopeLimit = movementProfile.SlopeLimit;
        }

        #endregion

        #region Private Methods

        private float ResolveTargetSpeed(bool isCrouching, bool isSprinting, bool hasPlanarInput)
        {
            if (!hasPlanarInput)
            {
                return 0f;
            }

            if (isCrouching)
            {
                return movementProfile.CrouchSpeed;
            }

            if (isSprinting)
            {
                return movementProfile.RunSpeed;
            }

            return movementProfile.WalkSpeed;
        }

        private static CCS_CharacterMovementState ResolveMovementState(
            CCS_CharacterMovementInput input,
            bool isGrounded,
            bool isCrouching,
            bool isSprinting,
            bool jumpedThisFrame,
            float planarSpeed)
        {
            if (!isGrounded)
            {
                return jumpedThisFrame
                    ? CCS_CharacterMovementState.Jumping
                    : CCS_CharacterMovementState.Falling;
            }

            if (planarSpeed <= 0.01f)
            {
                return isCrouching ? CCS_CharacterMovementState.Crouching : CCS_CharacterMovementState.Idle;
            }

            if (isCrouching)
            {
                return CCS_CharacterMovementState.Crouching;
            }

            return isSprinting
                ? CCS_CharacterMovementState.Running
                : CCS_CharacterMovementState.Walking;
        }

        #endregion

        #region Properties

        public bool IsGrounded => characterController != null && characterController.isGrounded;

        public float VerticalVelocity => verticalVelocity;

        #endregion
    }
}
