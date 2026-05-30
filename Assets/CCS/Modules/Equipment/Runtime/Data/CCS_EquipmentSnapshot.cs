using System.Collections.Generic;

// =============================================================================
// SCRIPT: CCS_EquipmentSnapshot
// CATEGORY: Modules / Equipment / Runtime / Data
// PURPOSE: Read-only equipment state snapshot for queries and future save hooks.
// PLACEMENT: Returned by CCS_PlayerEquipmentService snapshot methods.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Immutable after creation. No save serialization in 0.4.1.
// =============================================================================

namespace CCS.Modules.Equipment
{
    public sealed class CCS_EquipmentSnapshot
    {
        #region Public Methods

        public CCS_EquipmentSnapshot(
            IReadOnlyList<CCS_EquippedItem> equippedItems,
            int occupiedSlotCount,
            int totalSlotCount,
            int totalAdditionalInventorySlots,
            float totalAdditionalCarryWeight)
        {
            EquippedItems = equippedItems;
            OccupiedSlotCount = occupiedSlotCount;
            TotalSlotCount = totalSlotCount;
            TotalAdditionalInventorySlots = totalAdditionalInventorySlots < 0 ? 0 : totalAdditionalInventorySlots;
            TotalAdditionalCarryWeight = totalAdditionalCarryWeight < 0f ? 0f : totalAdditionalCarryWeight;
        }

        #endregion

        #region Properties

        public IReadOnlyList<CCS_EquippedItem> EquippedItems { get; }

        public int OccupiedSlotCount { get; }

        public int TotalSlotCount { get; }

        public int TotalAdditionalInventorySlots { get; }

        public float TotalAdditionalCarryWeight { get; }

        #endregion
    }
}
