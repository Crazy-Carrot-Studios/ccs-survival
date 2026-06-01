using System.Collections.Generic;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_WildlifeValidationUtility
// CATEGORY: Modules / Wildlife / Runtime / Validation
// PURPOSE: Runtime-safe validation for wildlife profiles and definitions.
// PLACEMENT: Used by editor validators and harvest service preflight checks.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Returns CCS_SurvivalValidationResult. No UnityEditor references.
// =============================================================================

namespace CCS.Modules.Wildlife
{
    public static class CCS_WildlifeValidationUtility
    {
        #region Public Methods

        public static CCS_SurvivalValidationResult ValidateProfile(CCS_WildlifeProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail("Wildlife profile is null.");
            }

            CCS_SurvivalValidationResult baseValidation = CCS_SurvivalProfileValidationUtility.ValidateProfile(profile);
            if (!baseValidation.IsSuccess)
            {
                return baseValidation;
            }

            if (profile.DefaultHarvestCount <= 0)
            {
                return CCS_SurvivalValidationResult.Fail("Default harvest count must be greater than zero.");
            }

            return CCS_SurvivalValidationResult.Pass("Wildlife profile validated.");
        }

        public static CCS_SurvivalValidationResult ValidateWildlifeDefinition(
            CCS_WildlifeDefinition wildlifeDefinition)
        {
            if (wildlifeDefinition == null)
            {
                return CCS_SurvivalValidationResult.Fail("Wildlife definition is null.");
            }

            if (string.IsNullOrWhiteSpace(wildlifeDefinition.WildlifeId))
            {
                return CCS_SurvivalValidationResult.Fail("Wildlife ID is required.");
            }

            if (wildlifeDefinition.MaxHarvestCount <= 0)
            {
                return CCS_SurvivalValidationResult.Fail("Max harvest count must be greater than zero.");
            }

            if (wildlifeDefinition.RespawnTimeSeconds < 0f)
            {
                return CCS_SurvivalValidationResult.Fail("Respawn time seconds cannot be negative.");
            }

            IReadOnlyList<CCS_WildlifeHarvestDropDefinition> harvestDrops = wildlifeDefinition.HarvestDrops;
            if (harvestDrops == null || harvestDrops.Count == 0)
            {
                return CCS_SurvivalValidationResult.Fail("Wildlife requires at least one harvest drop definition.");
            }

            for (int index = 0; index < harvestDrops.Count; index++)
            {
                CCS_WildlifeHarvestDropDefinition dropDefinition = harvestDrops[index];
                if (dropDefinition == null)
                {
                    return CCS_SurvivalValidationResult.Fail("Wildlife harvest drop entry is null.");
                }

                if (dropDefinition.ItemDefinition == null)
                {
                    return CCS_SurvivalValidationResult.Fail("Wildlife harvest drop item definition is null.");
                }

                if (dropDefinition.MinQuantity <= 0 || dropDefinition.MaxQuantity <= 0)
                {
                    return CCS_SurvivalValidationResult.Fail("Wildlife harvest drop quantities must be greater than zero.");
                }
            }

            return CCS_SurvivalValidationResult.Pass("Wildlife definition validated.");
        }

        public static CCS_SurvivalValidationResult ValidateRequiredWildlifeTypes()
        {
            if (!System.Enum.IsDefined(typeof(CCS_WildlifeType), CCS_WildlifeType.SmallGame))
            {
                return CCS_SurvivalValidationResult.Fail("Wildlife type SmallGame is not defined.");
            }

            if (!System.Enum.IsDefined(typeof(CCS_WildlifeType), CCS_WildlifeType.Deer))
            {
                return CCS_SurvivalValidationResult.Fail("Wildlife type Deer is not defined.");
            }

            return CCS_SurvivalValidationResult.Pass("Required wildlife types validated.");
        }

        #endregion
    }
}
