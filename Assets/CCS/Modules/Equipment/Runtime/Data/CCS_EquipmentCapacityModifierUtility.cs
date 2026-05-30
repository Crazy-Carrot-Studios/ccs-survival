using System.Collections.Generic;

// =============================================================================
// SCRIPT: CCS_EquipmentCapacityModifierUtility
// CATEGORY: Modules / Equipment / Runtime / Data
// PURPOSE: Aggregate inventory slot and carry weight modifiers from equipped items.
// PLACEMENT: Used by CCS_PlayerEquipmentService and validation utilities.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Exposes modifiers only. Does not resize inventory containers in 0.4.1a.
// =============================================================================

namespace CCS.Modules.Equipment
{
    public static class CCS_EquipmentCapacityModifierUtility
    {
        #region Public Methods

        public static void CalculateAggregateModifiers(
            IReadOnlyList<CCS_EquippedItem> equippedItems,
            out int totalAdditionalInventorySlots,
            out float totalAdditionalCarryWeight)
        {
            totalAdditionalInventorySlots = 0;
            totalAdditionalCarryWeight = 0f;

            if (equippedItems == null || equippedItems.Count == 0)
            {
                return;
            }

            for (int index = 0; index < equippedItems.Count; index++)
            {
                CCS_EquippedItem equippedItem = equippedItems[index];
                if (equippedItem?.EquipmentDefinition == null)
                {
                    continue;
                }

                AccumulateDefinitionModifiers(
                    equippedItem.EquipmentDefinition,
                    ref totalAdditionalInventorySlots,
                    ref totalAdditionalCarryWeight);
            }
        }

        public static void CalculateAggregateModifiers(
            IEnumerable<CCS_EquippedItem> equippedItems,
            out int totalAdditionalInventorySlots,
            out float totalAdditionalCarryWeight)
        {
            totalAdditionalInventorySlots = 0;
            totalAdditionalCarryWeight = 0f;

            if (equippedItems == null)
            {
                return;
            }

            foreach (CCS_EquippedItem equippedItem in equippedItems)
            {
                if (equippedItem?.EquipmentDefinition == null)
                {
                    continue;
                }

                AccumulateDefinitionModifiers(
                    equippedItem.EquipmentDefinition,
                    ref totalAdditionalInventorySlots,
                    ref totalAdditionalCarryWeight);
            }
        }

        public static bool AffectsInventoryCapacity(CCS_EquipmentItemDefinition equipmentDefinition)
        {
            return equipmentDefinition != null
                && equipmentDefinition.ModifiesInventoryCapacity
                && equipmentDefinition.AdditionalInventorySlots > 0;
        }

        public static bool AffectsCarryWeight(CCS_EquipmentItemDefinition equipmentDefinition)
        {
            return equipmentDefinition != null
                && equipmentDefinition.ModifiesCarryWeight
                && equipmentDefinition.AdditionalCarryWeight > 0f;
        }

        public static bool AffectsCapacity(CCS_EquipmentItemDefinition equipmentDefinition)
        {
            return AffectsInventoryCapacity(equipmentDefinition) || AffectsCarryWeight(equipmentDefinition);
        }

        #endregion

        #region Private Methods

        private static void AccumulateDefinitionModifiers(
            CCS_EquipmentItemDefinition equipmentDefinition,
            ref int totalAdditionalInventorySlots,
            ref float totalAdditionalCarryWeight)
        {
            if (equipmentDefinition.ModifiesInventoryCapacity)
            {
                totalAdditionalInventorySlots += equipmentDefinition.AdditionalInventorySlots;
            }

            if (equipmentDefinition.ModifiesCarryWeight)
            {
                totalAdditionalCarryWeight += equipmentDefinition.AdditionalCarryWeight;
            }
        }

        #endregion
    }
}
