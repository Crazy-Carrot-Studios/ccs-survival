// =============================================================================
// SCRIPT: CCS_EquipmentEventArgs
// CATEGORY: Modules / Equipment / Runtime / Events
// PURPOSE: Event payload for equip, unequip, change, durability, and failure notifications.
// PLACEMENT: Passed to CCS_PlayerEquipmentService event subscribers.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: No UI, combat, or visual data in 0.4.1.
// =============================================================================

namespace CCS.Modules.Equipment
{
    public sealed class CCS_EquipmentEventArgs
    {
        #region Public Methods

        public CCS_EquipmentEventArgs(
            CCS_EquipmentSlotType slot,
            CCS_EquipmentItemDefinition equipmentDefinition,
            CCS_EquippedItem equippedItem = null,
            float durabilityValue = 0f,
            string message = "")
        {
            Slot = slot;
            EquipmentDefinition = equipmentDefinition;
            EquippedItem = equippedItem;
            DurabilityValue = durabilityValue;
            Message = message ?? string.Empty;
        }

        #endregion

        #region Properties

        public CCS_EquipmentSlotType Slot { get; }

        public CCS_EquipmentItemDefinition EquipmentDefinition { get; }

        public CCS_EquippedItem EquippedItem { get; }

        public float DurabilityValue { get; }

        public string Message { get; }

        #endregion
    }
}
