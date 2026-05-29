using System;
using CCS.Core;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_ISurvivalInventoryService
// CATEGORY: Survival / Runtime / Inventory / Services
// PURPOSE: Survival inventory service contract for add/remove/query operations.
// PLACEMENT: Registered by CCS_SurvivalInventoryModule on CCS_ServiceRegistry.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: No UI dependencies. Authority context deferred to future multiplayer pass.
// =============================================================================

namespace CCS.Survival.Inventory
{
    public interface CCS_ISurvivalInventoryService : CCS_ISurvivalService
    {
        event Action InventoryChanged;

        event Action<string, string, int, int> ItemAdded;

        event Action<string, string, int, int> ItemRemoved;

        int SlotCount { get; }

        int OccupiedSlotCount { get; }

        CCS_Result TryAddItem(CCS_SurvivalItemDefinition definition, int amount, out int remainingAmount);

        CCS_Result TryRemoveItem(string itemId, int amount, out int removedAmount);

        bool HasItem(string itemId, int amount);

        int GetItemCount(string itemId);

        bool TryGetSlotSnapshot(int slotIndex, out CCS_SurvivalInventorySlotSnapshot snapshot);

        string BuildCompactItemSummary(int maxEntries);
    }
}
