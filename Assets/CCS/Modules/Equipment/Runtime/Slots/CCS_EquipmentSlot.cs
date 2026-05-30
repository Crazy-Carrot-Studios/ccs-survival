// =============================================================================
// SCRIPT: CCS_EquipmentSlot
// CATEGORY: Modules / Equipment / Runtime / Slots
// PURPOSE: Single equipment slot with compatibility validation and occupied state.
// PLACEMENT: Managed by CCS_PlayerEquipmentService slot collection.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Validates allowed slot type only. No inventory stack mutation.
// =============================================================================

namespace CCS.Modules.Equipment
{
    public sealed class CCS_EquipmentSlot
    {
        #region Variables

        private CCS_EquippedItem equippedItem;

        #endregion

        #region Public Methods

        public CCS_EquipmentSlot(CCS_EquipmentSlotType slotType)
        {
            SlotType = slotType;
        }

        public bool IsEmpty => equippedItem == null;

        public bool IsOccupied => equippedItem != null;

        public CCS_EquippedItem EquippedItem => equippedItem;

        public bool CanAccept(CCS_EquipmentItemDefinition equipmentDefinition)
        {
            if (equipmentDefinition == null)
            {
                return false;
            }

            if (IsOccupied)
            {
                return false;
            }

            return equipmentDefinition.AllowedSlot == SlotType;
        }

        public bool TryEquip(CCS_EquippedItem item)
        {
            if (item == null || IsOccupied)
            {
                return false;
            }

            if (item.Slot != SlotType)
            {
                return false;
            }

            if (item.EquipmentDefinition == null
                || item.EquipmentDefinition.AllowedSlot != SlotType)
            {
                return false;
            }

            equippedItem = item;
            return true;
        }

        public CCS_EquippedItem TryUnequip()
        {
            if (IsEmpty)
            {
                return null;
            }

            CCS_EquippedItem removed = equippedItem;
            equippedItem = null;
            return removed;
        }

        public void Clear()
        {
            equippedItem = null;
        }

        #endregion

        #region Properties

        public CCS_EquipmentSlotType SlotType { get; }

        #endregion
    }
}
