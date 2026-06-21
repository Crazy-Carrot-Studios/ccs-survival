// =============================================================================
// SCRIPT: CCS_IInteractionLockController
// CATEGORY: Modules / Interaction / Runtime / Contracts
// PURPOSE: Request immediate interaction busy / control lock before executor runs.
// PLACEMENT: Implemented by CCS_PlayerInteractionAnimator on the test player.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Scanner calls BeginInteractionLock on accepted E before InteractionCompleted.
// =============================================================================

namespace CCS.Modules.Interaction
{
    public interface CCS_IInteractionLockController
    {
        #region Public Methods

        void BeginInteractionLock(CCS_InteractionAnimationKey animationKey);

        void CancelInteractionLock();

        #endregion
    }
}
