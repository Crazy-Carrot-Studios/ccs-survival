using System;
using CCS.Modules.Inventory;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_FarmValidationUtility
// CATEGORY: Modules / Farming / Runtime / Validation
// PURPOSE: Shared profile/content validation for farming bootstrap and runtime.
// PLACEMENT: Used by CCS_FarmService and editor validators.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-02
// NOTES: Milestone 2.2.0 farming foundation.
// =============================================================================

namespace CCS.Modules.Farming
{
    public static class CCS_FarmValidationUtility
    {
        public static CCS_SurvivalValidationResult ValidateProfile(CCS_CropProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail("Farming profile is null.");
            }

            CCS_CropDefinition[] crops = profile.CropDefinitions;
            if (crops == null || crops.Length == 0)
            {
                return CCS_SurvivalValidationResult.Fail("Farming profile has no crop definitions.");
            }

            for (int index = 0; index < crops.Length; index++)
            {
                CCS_CropDefinition crop = crops[index];
                if (crop == null)
                {
                    return CCS_SurvivalValidationResult.Fail($"Farming crop definition at index {index} is null.");
                }

                if (string.IsNullOrWhiteSpace(crop.CropId))
                {
                    return CCS_SurvivalValidationResult.Fail($"Farming crop at index {index} is missing cropId.");
                }

                if (crop.SeedItem == null || crop.HarvestItem == null)
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Farming crop '{crop.CropId}' is missing seed or harvest item.");
                }

                if (crop.GrowthDurationSeconds <= 0f)
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Farming crop '{crop.CropId}' has invalid growth duration.");
                }
            }

            CCS_FarmPlotDefinition[] plots = profile.FarmPlotDefinitions;
            if (plots == null || plots.Length == 0)
            {
                return CCS_SurvivalValidationResult.Fail("Farming profile has no farm plot definitions.");
            }

            for (int index = 0; index < plots.Length; index++)
            {
                CCS_FarmPlotDefinition plot = plots[index];
                if (plot == null)
                {
                    return CCS_SurvivalValidationResult.Fail($"Farm plot definition at index {index} is null.");
                }

                if (string.IsNullOrWhiteSpace(plot.PlotDefinitionId))
                {
                    return CCS_SurvivalValidationResult.Fail($"Farm plot at index {index} is missing plotDefinitionId.");
                }

                if (plot.PlaceableKitItem == null)
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Farm plot '{plot.PlotDefinitionId}' is missing placeable kit item.");
                }
            }

            return CCS_SurvivalValidationResult.Pass("Farming profile validated.");
        }

        public static CCS_SurvivalValidationResult ValidateItem(CCS_ItemDefinition item, string expectedItemId)
        {
            if (item == null)
            {
                return CCS_SurvivalValidationResult.Fail($"Missing item definition for '{expectedItemId}'.");
            }

            if (!string.Equals(item.ItemId, expectedItemId, StringComparison.OrdinalIgnoreCase))
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Item '{item.name}' has id '{item.ItemId}' but expected '{expectedItemId}'.");
            }

            return CCS_SurvivalValidationResult.Pass($"Item '{expectedItemId}' validated.");
        }
    }
}
