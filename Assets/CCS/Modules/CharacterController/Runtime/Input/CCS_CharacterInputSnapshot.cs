using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CharacterInputSnapshot
// CATEGORY: Modules / CharacterController / Runtime / Input
// PURPOSE: Frame input sample for movement and look (move, look, jump, sprint, crouch).
// PLACEMENT: Produced by CCS_ICharacterInputProvider implementations.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Axes are -1..1 unless noted. No Input System package dependency in 0.3.8.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public readonly struct CCS_CharacterInputSnapshot
    {
        #region Public Methods

        public CCS_CharacterInputSnapshot(
            Vector2 move,
            Vector2 look,
            bool jumpPressed,
            bool sprintHeld,
            bool crouchHeld)
        {
            Move = move;
            Look = look;
            JumpPressed = jumpPressed;
            SprintHeld = sprintHeld;
            CrouchHeld = crouchHeld;
        }

        public static CCS_CharacterInputSnapshot Empty =>
            new CCS_CharacterInputSnapshot(Vector2.zero, Vector2.zero, false, false, false);

        #endregion

        #region Properties

        public Vector2 Move { get; }

        public Vector2 Look { get; }

        public bool JumpPressed { get; }

        public bool SprintHeld { get; }

        public bool CrouchHeld { get; }

        #endregion
    }
}
