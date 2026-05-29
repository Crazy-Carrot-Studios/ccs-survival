// =============================================================================
// SCRIPT: CCS_SurvivalInventorySlotSnapshot
// CATEGORY: Survival / Runtime / Inventory / Data
// PURPOSE: Read-only snapshot of a single inventory slot for debug/UI consumers.
// PLACEMENT: Returned by CCS_ISurvivalInventoryService.TryGetSlotSnapshot.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Value type copy; safe for overlay readout without exposing mutable slots.
// =============================================================================

namespace CCS.Survival.Inventory
{
    public readonly struct CCS_SurvivalInventorySlotSnapshot
    {
        public CCS_SurvivalInventorySlotSnapshot(
            int slotIndex,
            bool isEmpty,
            string itemId,
            string displayName,
            int amount)
        {
            SlotIndex = slotIndex;
            IsEmpty = isEmpty;
            ItemId = itemId ?? string.Empty;
            DisplayName = displayName ?? string.Empty;
            Amount = amount;
        }

        public int SlotIndex { get; }

        public bool IsEmpty { get; }

        public string ItemId { get; }

        public string DisplayName { get; }

        public int Amount { get; }
    }
}
