// =============================================================================
// SCRIPT: CCS_InteractionEventArgs
// CATEGORY: Modules / Interaction / Runtime / Events
// PURPOSE: Event payload for interaction detection and request outcomes.
// PLACEMENT: Passed to CCS_InteractionService event subscribers.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Does not carry inventory, quest, or UI data in 0.3.9.
// =============================================================================

namespace CCS.Modules.Interaction
{
    public sealed class CCS_InteractionEventArgs
    {
        #region Public Methods

        public CCS_InteractionEventArgs(
            CCS_IInteractable interactable,
            string displayName,
            float distance,
            string message = "")
        {
            Interactable = interactable;
            DisplayName = displayName ?? string.Empty;
            Distance = distance;
            Message = message ?? string.Empty;
        }

        #endregion

        #region Properties

        public CCS_IInteractable Interactable { get; }

        public string DisplayName { get; }

        public float Distance { get; }

        public string Message { get; }

        #endregion
    }
}
