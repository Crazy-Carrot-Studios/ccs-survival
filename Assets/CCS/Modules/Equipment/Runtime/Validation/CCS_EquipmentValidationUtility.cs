using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_EquipmentValidationUtility
// CATEGORY: Modules / Equipment / Runtime / Validation
// PURPOSE: Runtime-safe validation for equipment profiles and definitions.
// PLACEMENT: Used by editor validators and future bootstrap checks.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Returns CCS_SurvivalValidationResult. No UnityEditor references.
// =============================================================================

namespace CCS.Modules.Equipment
{
    public static class CCS_EquipmentValidationUtility
    {
        #region Public Methods

        public static CCS_SurvivalValidationResult ValidateProfile(CCS_EquipmentProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail("Equipment profile is null.");
            }

            CCS_SurvivalValidationResult baseValidation = CCS_SurvivalProfileValidationUtility.ValidateProfile(profile);
            if (!baseValidation.IsSuccess)
            {
                return baseValidation;
            }

            return CCS_SurvivalValidationResult.Pass("Equipment profile validated.");
        }

        public static CCS_SurvivalValidationResult ValidateEquipmentDefinition(
            CCS_EquipmentItemDefinition equipmentDefinition)
        {
            if (equipmentDefinition == null)
            {
                return CCS_SurvivalValidationResult.Fail("Equipment item definition is null.");
            }

            if (equipmentDefinition.ItemDefinition == null)
            {
                return CCS_SurvivalValidationResult.Fail("Equipment item definition requires an inventory item reference.");
            }

            if (equipmentDefinition.DurabilityEnabled && equipmentDefinition.MaxDurability <= 0f)
            {
                return CCS_SurvivalValidationResult.Fail("Max durability must be greater than zero when durability is enabled.");
            }

            return CCS_SurvivalValidationResult.Pass("Equipment item definition validated.");
        }

        public static bool IsSlotCompatible(
            CCS_EquipmentSlotType slot,
            CCS_EquipmentItemDefinition equipmentDefinition)
        {
            return equipmentDefinition != null && equipmentDefinition.AllowedSlot == slot;
        }

        #endregion
    }
}
