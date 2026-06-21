using System;

// =============================================================================
// SCRIPT: CCS_IInteractionAnimationSource
// CATEGORY: Modules / Interaction / Runtime / Contracts
// PURPOSE: Visual-only contract for local interaction completion notifications.
// PLACEMENT: Implemented by CCS_NetworkInteractionScanner on the player root.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Keeps CharacterController visuals free of Netcode assembly references.
// =============================================================================

namespace CCS.Modules.Interaction
{
    public interface CCS_IInteractionAnimationSource
    {
        #region Events

        event Action<CCS_InteractionCompletedEvent> InteractionCompleted;

        #endregion
    }
}
