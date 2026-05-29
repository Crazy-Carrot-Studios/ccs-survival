using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CharacterMovementInput
// CATEGORY: Modules / CharacterController / Runtime / Movement
// PURPOSE: Processed locomotion input consumed by CCS_CharacterControllerMotor.
// PLACEMENT: Built by CCS_CharacterMovementService from input snapshots and look yaw.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: World-space planar move uses camera/character yaw. Safe when input is zero.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public readonly struct CCS_CharacterMovementInput
    {
        #region Public Methods

        public CCS_CharacterMovementInput(
            Vector3 worldPlanarMove,
            bool jumpPressed,
            bool sprintHeld,
            bool crouchHeld)
        {
            WorldPlanarMove = worldPlanarMove;
            JumpPressed = jumpPressed;
            SprintHeld = sprintHeld;
            CrouchHeld = crouchHeld;
        }

        public static CCS_CharacterMovementInput Empty =>
            new CCS_CharacterMovementInput(Vector3.zero, false, false, false);

        #endregion

        #region Properties

        public Vector3 WorldPlanarMove { get; }

        public bool JumpPressed { get; }

        public bool SprintHeld { get; }

        public bool CrouchHeld { get; }

        public bool HasPlanarInput => WorldPlanarMove.sqrMagnitude > 0.0001f;

        #endregion
    }
}
