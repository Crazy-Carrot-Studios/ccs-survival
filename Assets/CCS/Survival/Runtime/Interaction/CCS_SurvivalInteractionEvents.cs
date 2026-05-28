using System;
using CCS.Core;

// =============================================================================
// SCRIPT: CCS_SurvivalInteractionEvents
// CATEGORY: Survival / Runtime / Interaction
// PURPOSE: Event payloads for interaction target changes, performed actions, and pickups.
// PLACEMENT: Dispatched through CCS_RuntimeHost.EventDispatcher when a host is wired.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Optional listeners for future UI/inventory modules. No static singleton dispatch.
// =============================================================================

namespace CCS.Survival.Interaction
{
    public readonly struct CCS_SurvivalInteractionTargetChangedEvent : CCS_IEvent
    {
        public CCS_SurvivalInteractionTargetChangedEvent(string interactionPrompt)
        {
            InteractionPrompt = interactionPrompt ?? string.Empty;
            Timestamp = DateTime.UtcNow;
        }

        public string InteractionPrompt { get; }

        public DateTime Timestamp { get; }
    }

    public readonly struct CCS_SurvivalInteractionPerformedEvent : CCS_IEvent
    {
        public CCS_SurvivalInteractionPerformedEvent(string interactableLabel)
        {
            InteractableLabel = interactableLabel ?? string.Empty;
            Timestamp = DateTime.UtcNow;
        }

        public string InteractableLabel { get; }

        public DateTime Timestamp { get; }
    }

    public readonly struct CCS_SurvivalPickupCollectedEvent : CCS_IEvent
    {
        public CCS_SurvivalPickupCollectedEvent(string pickupId, string displayName, int amount)
        {
            PickupId = pickupId ?? string.Empty;
            DisplayName = displayName ?? string.Empty;
            Amount = amount;
            Timestamp = DateTime.UtcNow;
        }

        public string PickupId { get; }

        public string DisplayName { get; }

        public int Amount { get; }

        public DateTime Timestamp { get; }
    }
}
