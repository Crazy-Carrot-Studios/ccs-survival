using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CharacterInputRuntimeBridge
// CATEGORY: Modules / CharacterController / Runtime / Input
// PURPOSE: Manual/test input provider; gameplay uses CCS_CharacterInputActionProvider.
// PLACEMENT: Owned by movement service default provider or test harnesses.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Not a MonoBehaviour. Set fields from tests. Gameplay reads Input Actions via provider.
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
