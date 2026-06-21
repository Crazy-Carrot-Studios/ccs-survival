// =============================================================================
// SCRIPT: CCS_ICharacterControlLockSource
// CATEGORY: Modules / CharacterController / Runtime / Contracts
// PURPOSE: Read-only contract for blocking locomotion input while an action plays.
// PLACEMENT: Implemented by CCS_PlayerInteractionAnimator on the test player.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Motor checks this before applying movement, jump, and sprint.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public interface CCS_ICharacterControlLockSource
    {
        #region Properties

        bool IsControlLocked { get; }

        #endregion
    }
}
