// =============================================================================
// SCRIPT: CCS_IInteractionTargetSource
// CATEGORY: Modules / Interaction / Runtime / Contracts
// PURPOSE: Read-only contract for pickup-ready state and prompt text.
// PLACEMENT: Implemented by CCS_NetworkInteractionScanner on the player root.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Observation-only surface for interaction prompt presentation.
// =============================================================================

namespace CCS.Modules.Interaction
{
    public interface CCS_IInteractionTargetSource
    {
        #region Properties

        bool HasPickupReadyTarget { get; }

        string CurrentPromptText { get; }

        #endregion
    }
}
