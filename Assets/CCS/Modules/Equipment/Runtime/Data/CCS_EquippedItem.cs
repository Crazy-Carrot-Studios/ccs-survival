using CCS.Modules.Inventory;

// =============================================================================
// SCRIPT: CCS_EquippedItem
// CATEGORY: Modules / Equipment / Runtime / Data
// PURPOSE: Runtime equipped item instance referencing inventory item identity.
// PLACEMENT: Stored in CCS_EquipmentSlot. Does not duplicate inventory ownership.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: References equipment and inventory definitions. No UI or combat data.
// =============================================================================

namespace CCS.Modules.Equipment
{
    public sealed class CCS_EquippedItem
    {
        #region Public Methods

        public CCS_EquippedItem(
            CCS_EquipmentSlotType slot,
            CCS_EquipmentItemDefinition equipmentDefinition,
            CCS_DurabilityState durabilityState = null)
        {
            Slot = slot;
            EquipmentDefinition = equipmentDefinition;
            Durability = durabilityState;
        }

        #endregion

        #region Properties

        public CCS_EquipmentSlotType Slot { get; }

        public CCS_EquipmentItemDefinition EquipmentDefinition { get; }

        public CCS_ItemDefinition ItemDefinition =>
            EquipmentDefinition != null ? EquipmentDefinition.ItemDefinition : null;

        public CCS_DurabilityState Durability { get; }

        public bool HasDurability => Durability != null;

        #endregion
    }
}
