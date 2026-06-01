using System.Collections.Generic;
using CCS.Modules.Inventory;
using CCS.Modules.Resources;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_FishingValidationUtility
// CATEGORY: Modules / Fishing / Runtime / Utilities
// PURPOSE: Validates fishing profiles, spot definitions, and catch table references.
// PLACEMENT: Used by editor validator and CCS_FishingService.InitializeFromProfile.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Runtime-safe validation without UnityEditor dependencies.
// =============================================================================

namespace CCS.Modules.Fishing
{
    public static class CCS_FishingValidationUtility
    {
        #region Public Methods

        public static CCS_SurvivalValidationResult ValidateProfile(CCS_FishingProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail("Fishing profile is null.");
            }

            profile.BuildItemLookup();
            if (profile.DefaultCatchTable == null || profile.DefaultCatchTable.Length == 0)
            {
                return CCS_SurvivalValidationResult.Fail("Fishing profile default catch table is empty.");
            }

            return ValidateCatchTable(profile, profile.DefaultCatchTable, "default catch table");
        }

        public static CCS_SurvivalValidationResult ValidateSpotDefinition(CCS_FishingSpotDefinition spotDefinition)
        {
            if (spotDefinition == null)
            {
                return CCS_SurvivalValidationResult.Fail("Fishing spot definition is null.");
            }

            if (string.IsNullOrWhiteSpace(spotDefinition.SpotId))
            {
                return CCS_SurvivalValidationResult.Fail("Fishing spot definition spotId is empty.");
            }

            if (spotDefinition.ResourceSourceType != CCS_ResourceSourceType.Water)
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Spot {spotDefinition.SpotId} must use ResourceSourceType.Water.");
            }

            if (spotDefinition.HarvestMethod != CCS_HarvestMethodType.Fish)
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Spot {spotDefinition.SpotId} must use HarvestMethodType.Fish.");
            }

            if (spotDefinition.RequiredToolType != CCS_ItemToolType.FishingPole)
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Spot {spotDefinition.SpotId} must require FishingPole tool type.");
            }

            if (spotDefinition.CatchTable != null && spotDefinition.CatchTable.Length > 0)
            {
                return ValidateCatchEntries(spotDefinition.CatchTable, null, $"spot {spotDefinition.SpotId}");
            }

            return CCS_SurvivalValidationResult.Pass(
                $"Fishing spot definition {spotDefinition.SpotId} metadata is valid.");
        }

        public static CCS_SurvivalValidationResult ValidateCatchTable(
            CCS_FishingProfile profile,
            CCS_FishingCatchDefinition[] catchTable,
            string tableLabel)
        {
            if (catchTable == null || catchTable.Length == 0)
            {
                return CCS_SurvivalValidationResult.Fail($"Fishing {tableLabel} is empty.");
            }

            return ValidateCatchEntries(catchTable, profile, tableLabel);
        }

        public static bool IsFishingPoleItemDefinition(CCS_ItemDefinition itemDefinition)
        {
            return CCS_ItemGameplayUtility.IsFishingPoleItem(itemDefinition);
        }

        #endregion

        #region Private Methods

        private static CCS_SurvivalValidationResult ValidateCatchEntries(
            CCS_FishingCatchDefinition[] catchTable,
            CCS_FishingProfile profile,
            string tableLabel)
        {
            int totalWeight = 0;
            for (int index = 0; index < catchTable.Length; index++)
            {
                CCS_FishingCatchDefinition entry = catchTable[index];
                if (entry == null || entry.weight <= 0)
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Fishing {tableLabel} entry {index} has invalid weight.");
                }

                totalWeight += entry.weight;
                if (entry.catchKind == CCS_FishingCatchKind.Nothing)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(entry.itemDefinitionId))
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Fishing {tableLabel} entry {index} is missing itemDefinitionId.");
                }

                if (profile != null && !profile.TryResolveItem(entry.itemDefinitionId, out _))
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Fishing {tableLabel} entry {index} references unknown item {entry.itemDefinitionId}.");
                }
            }

            if (totalWeight <= 0)
            {
                return CCS_SurvivalValidationResult.Fail($"Fishing {tableLabel} has zero total weight.");
            }

            return CCS_SurvivalValidationResult.Pass($"Fishing {tableLabel} is valid ({totalWeight} total weight).");
        }

        #endregion
    }
}
