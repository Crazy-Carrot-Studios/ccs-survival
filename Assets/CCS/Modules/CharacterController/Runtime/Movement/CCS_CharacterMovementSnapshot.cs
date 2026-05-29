using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CharacterMovementSnapshot
// CATEGORY: Modules / CharacterController / Runtime / Movement
// PURPOSE: Read-only movement state snapshot for consumers and diagnostics.
// PLACEMENT: Returned by CCS_CharacterMovementService.CurrentSnapshot.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Immutable after creation. No UI or networking serialization in 0.3.8.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public readonly struct CCS_CharacterMovementSnapshot
    {
        #region Public Methods

        public CCS_CharacterMovementSnapshot(
            CCS_CharacterMovementState movementState,
            CCS_CharacterGroundingState groundingState,
            Vector3 worldVelocity,
            float planarSpeed,
            bool isSprinting,
            bool isCrouching,
            bool isJumping)
        {
            MovementState = movementState;
            GroundingState = groundingState;
            WorldVelocity = worldVelocity;
            PlanarSpeed = planarSpeed;
            IsSprinting = isSprinting;
            IsCrouching = isCrouching;
            IsJumping = isJumping;
        }

        #endregion

        #region Properties

        public CCS_CharacterMovementState MovementState { get; }

        public CCS_CharacterGroundingState GroundingState { get; }

        public Vector3 WorldVelocity { get; }

        public float PlanarSpeed { get; }

        public bool IsSprinting { get; }

        public bool IsCrouching { get; }

        public bool IsJumping { get; }

        #endregion
    }
}
