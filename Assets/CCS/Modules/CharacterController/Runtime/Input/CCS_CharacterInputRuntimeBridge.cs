using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CharacterInputRuntimeBridge
// CATEGORY: Modules / CharacterController / Runtime / Input
// PURPOSE: Test/dev input provider with serialized axes until New Input System is wired.
// PLACEMENT: Owned by scene bootstrap or movement service for 0.3.8 smoke tests.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Not a MonoBehaviour. Set fields from tests or temporary scene wiring.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public sealed class CCS_CharacterInputRuntimeBridge : CCS_ICharacterInputProvider
    {
        #region Variables

        private Vector2 moveInput;
        private Vector2 lookInput;
        private bool jumpPressed;
        private bool sprintHeld;
        private bool crouchHeld;

        #endregion

        #region Public Methods

        public CCS_CharacterInputSnapshot GetInputSnapshot()
        {
            return new CCS_CharacterInputSnapshot(
                moveInput,
                lookInput,
                jumpPressed,
                sprintHeld,
                crouchHeld);
        }

        public void SetMoveInput(Vector2 move)
        {
            moveInput = move;
        }

        public void SetLookInput(Vector2 look)
        {
            lookInput = look;
        }

        public void SetJumpPressed(bool pressed)
        {
            jumpPressed = pressed;
        }

        public void SetSprintHeld(bool held)
        {
            sprintHeld = held;
        }

        public void SetCrouchHeld(bool held)
        {
            crouchHeld = held;
        }

        public void ClearInput()
        {
            moveInput = Vector2.zero;
            lookInput = Vector2.zero;
            jumpPressed = false;
            sprintHeld = false;
            crouchHeld = false;
        }

        #endregion
    }
}
