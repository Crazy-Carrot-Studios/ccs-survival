// =============================================================================
// SCRIPT: CCS_InteractionEvents
// CATEGORY: Modules / Interaction / Runtime / Events
// PURPOSE: Event name constants and delegate contracts for interaction systems.
// PLACEMENT: Instance events on CCS_InteractionService document contracts here.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Service raises requests; interactable target decides gameplay outcome.
// =============================================================================

namespace CCS.Modules.Interaction
{
    public static class CCS_InteractionEvents
    {
        public const string InteractableFoundEventName = "Interaction.InteractableFound";
        public const string InteractableLostEventName = "Interaction.InteractableLost";
        public const string InteractionRequestedEventName = "Interaction.Requested";
        public const string InteractionSucceededEventName = "Interaction.Succeeded";
        public const string InteractionFailedEventName = "Interaction.Failed";
    }

    public delegate void InteractionInteractableFoundHandler(CCS_InteractionEventArgs eventArgs);

    public delegate void InteractionInteractableLostHandler(CCS_InteractionEventArgs eventArgs);

    public delegate void InteractionRequestedHandler(CCS_InteractionEventArgs eventArgs);

    public delegate void InteractionSucceededHandler(CCS_InteractionEventArgs eventArgs);

    public delegate void InteractionFailedHandler(CCS_InteractionEventArgs eventArgs);
}
