using System.Collections.Generic;

// =============================================================================
// SCRIPT: CCS_EquipmentEnvironmentalModifierUtility
// CATEGORY: Modules / Equipment / Runtime / Data
// PURPOSE: Aggregate environmental survival modifiers from equipped items.
// PLACEMENT: Used by CCS_PlayerEquipmentService and validation utilities.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Allocation-safe aggregation. Survival modifiers only.
// =============================================================================

namespace CCS.Modules.Equipment
{
    public static class CCS_EquipmentEnvironmentalModifierUtility
    {
        #region Public Methods

        public static CCS_EquipmentEnvironmentalModifierSnapshot CalculateAggregateModifiers(
            IReadOnlyList<CCS_EquippedItem> equippedItems)
        {
            float totalTemperatureResistance = 0f;
            float totalWetnessResistance = 0f;
            float totalExposureResistance = 0f;

            if (equippedItems == null || equippedItems.Count == 0)
            {
                return CCS_EquipmentEnvironmentalModifierSnapshot.Empty;
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
                    ref totalTemperatureResistance,
                    ref totalWetnessResistance,
                    ref totalExposureResistance);
            }

            return new CCS_EquipmentEnvironmentalModifierSnapshot(
                totalTemperatureResistance,
                totalWetnessResistance,
                totalExposureResistance);
        }

        public static CCS_EquipmentEnvironmentalModifierSnapshot CalculateAggregateModifiers(
            IEnumerable<CCS_EquippedItem> equippedItems)
        {
            float totalTemperatureResistance = 0f;
            float totalWetnessResistance = 0f;
            float totalExposureResistance = 0f;

            if (equippedItems == null)
            {
                return CCS_EquipmentEnvironmentalModifierSnapshot.Empty;
            }

            foreach (CCS_EquippedItem equippedItem in equippedItems)
            {
                if (equippedItem?.EquipmentDefinition == null)
                {
                    continue;
                }

                AccumulateDefinitionModifiers(
                    equippedItem.EquipmentDefinition,
                    ref totalTemperatureResistance,
                    ref totalWetnessResistance,
                    ref totalExposureResistance);
            }

            return new CCS_EquipmentEnvironmentalModifierSnapshot(
                totalTemperatureResistance,
                totalWetnessResistance,
                totalExposureResistance);
        }

        public static bool AffectsEnvironment(CCS_EquipmentItemDefinition equipmentDefinition)
        {
            return equipmentDefinition != null
                && (equipmentDefinition.TemperatureResistance > 0f
                    || equipmentDefinition.WetnessResistance > 0f
                    || equipmentDefinition.ExposureResistance > 0f);
        }

        #endregion

        #region Private Methods

        private static void AccumulateDefinitionModifiers(
            CCS_EquipmentItemDefinition equipmentDefinition,
            ref float totalTemperatureResistance,
            ref float totalWetnessResistance,
            ref float totalExposureResistance)
        {
            totalTemperatureResistance += equipmentDefinition.TemperatureResistance;
            totalWetnessResistance += equipmentDefinition.WetnessResistance;
            totalExposureResistance += equipmentDefinition.ExposureResistance;
        }

        #endregion
    }
}
