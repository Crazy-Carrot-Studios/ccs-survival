using System;
using CCS.Core;

// =============================================================================
// SCRIPT: CCS_SurvivalInventoryEvents
// CATEGORY: Survival / Runtime / Inventory / Events
// PURPOSE: Event payloads and handler types for inventory mutations.
// PLACEMENT: Dispatched through CCS_RuntimeHost.EventDispatcher and local service events.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Pickup bridge in Phase 2C will listen to pickup collected events, not these directly.
// =============================================================================

namespace CCS.Survival.Inventory
{
    public readonly struct CCS_SurvivalInventoryChangedEvent : CCS_IEvent
    {
        public CCS_SurvivalInventoryChangedEvent(int occupiedSlotCount, int slotCount)
        {
            OccupiedSlotCount = occupiedSlotCount;
            SlotCount = slotCount;
            Timestamp = DateTime.UtcNow;
        }

        public int OccupiedSlotCount { get; }

        public int SlotCount { get; }

        public DateTime Timestamp { get; }
    }

    public readonly struct CCS_SurvivalItemAddedEvent : CCS_IEvent
    {
        public CCS_SurvivalItemAddedEvent(string itemId, string displayName, int amountAdded, int newTotalCount)
        {
            ItemId = itemId ?? string.Empty;
            DisplayName = displayName ?? string.Empty;
            AmountAdded = amountAdded;
            NewTotalCount = newTotalCount;
            Timestamp = DateTime.UtcNow;
        }

        public string ItemId { get; }

        public string DisplayName { get; }

        public int AmountAdded { get; }

        public int NewTotalCount { get; }

        public DateTime Timestamp { get; }
    }

    public readonly struct CCS_SurvivalItemRemovedEvent : CCS_IEvent
    {
        public CCS_SurvivalItemRemovedEvent(string itemId, string displayName, int amountRemoved, int newTotalCount)
        {
            ItemId = itemId ?? string.Empty;
            DisplayName = displayName ?? string.Empty;
            AmountRemoved = amountRemoved;
            NewTotalCount = newTotalCount;
            Timestamp = DateTime.UtcNow;
        }

        public string ItemId { get; }

        public string DisplayName { get; }

        public int AmountRemoved { get; }

        public int NewTotalCount { get; }

        public DateTime Timestamp { get; }
    }
}
