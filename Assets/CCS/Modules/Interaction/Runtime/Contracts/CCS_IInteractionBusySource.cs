using System;

// =============================================================================
// SCRIPT: CCS_IInteractionBusySource
// CATEGORY: Modules / Interaction / Runtime / Contracts
// PURPOSE: Read-only contract for local-owner interaction animation busy state.
// PLACEMENT: Implemented by CCS_PlayerInteractionAnimator on the test player.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Used by scanner and prompt presenter to block overlap during interact anims.
// =============================================================================

namespace CCS.Modules.Interaction
{
    public interface CCS_IInteractionBusySource
    {
        #region Properties

        bool IsInteractionBusy { get; }

        #endregion

        #region Events

        event Action<bool> InteractionBusyChanged;

        #endregion
    }
}
