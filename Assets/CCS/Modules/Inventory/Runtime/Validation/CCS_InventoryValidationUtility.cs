using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_InventoryValidationUtility
// CATEGORY: Modules / Inventory / Runtime / Validation
// PURPOSE: Runtime-safe validation for inventory profiles and tuning values.
// PLACEMENT: Used by editor validators and future bootstrap checks.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Returns CCS_SurvivalValidationResult. No UnityEditor references.
// =============================================================================

namespace CCS.Modules.Inventory
{
    public static class CCS_InventoryValidationUtility
    {
        #region Public Methods

        public static CCS_SurvivalValidationResult ValidateProfile(CCS_InventoryProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail("Inventory profile is null.");
            }

            CCS_SurvivalValidationResult baseValidation = CCS_SurvivalProfileValidationUtility.ValidateProfile(profile);
            if (!baseValidation.IsSuccess)
            {
                return baseValidation;
            }

            if (profile.InventorySlotCount <= 0)
            {
                return CCS_SurvivalValidationResult.Fail("Inventory slot count must be greater than zero.");
            }

            if (profile.EnableWeightLimit && profile.MaxCarryWeight <= 0f)
            {
                return CCS_SurvivalValidationResult.Fail("Max carry weight must be greater than zero when weight limit is enabled.");
            }

            return CCS_SurvivalValidationResult.Pass("Inventory profile validated.");
        }

        #endregion
    }
}
