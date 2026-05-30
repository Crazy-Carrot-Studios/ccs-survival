using System.Collections.Generic;

// =============================================================================
// SCRIPT: CCS_InventorySnapshot
// CATEGORY: Modules / Inventory / Runtime / Data
// PURPOSE: Read-only inventory state snapshot for queries and future save hooks.
// PLACEMENT: Returned by container and player inventory service snapshot methods.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Immutable after creation. No save serialization in 0.4.0.
// =============================================================================

namespace CCS.Modules.Inventory
{
    public sealed class CCS_InventorySnapshot
    {
        #region Public Methods

        public CCS_InventorySnapshot(IReadOnlyList<CCS_ItemStack> slotStacks, int slotCount, int usedSlotCount, int totalItemQuantity)
        {
            SlotStacks = slotStacks;
            SlotCount = slotCount;
            UsedSlotCount = usedSlotCount;
            TotalItemQuantity = totalItemQuantity;
        }

        #endregion

        #region Properties

        public IReadOnlyList<CCS_ItemStack> SlotStacks { get; }

        public int SlotCount { get; }

        public int UsedSlotCount { get; }

        public int TotalItemQuantity { get; }

        #endregion
    }
}
