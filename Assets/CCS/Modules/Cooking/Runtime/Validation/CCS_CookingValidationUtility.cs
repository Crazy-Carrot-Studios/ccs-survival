using System.Collections.Generic;
using CCS.Modules.Building;
using CCS.Modules.Crafting;
using CCS.Modules.Inventory;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_CookingValidationUtility
// CATEGORY: Modules / Cooking / Runtime / Validation
// PURPOSE: Runtime-safe validation for cooking profiles, campfires, and food definitions.
// PLACEMENT: Used by editor validators and cooking service preflight checks.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Returns CCS_SurvivalValidationResult. No UnityEditor references.
// =============================================================================

namespace CCS.Modules.Cooking
{
    public static class CCS_CookingValidationUtility
    {
        #region Public Methods

        public static CCS_SurvivalValidationResult ValidateProfile(CCS_CookingProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail("Cooking profile is null.");
            }

            CCS_SurvivalValidationResult baseValidation = CCS_SurvivalProfileValidationUtility.ValidateProfile(profile);
            if (!baseValidation.IsSuccess)
            {
                return baseValidation;
            }

            if (profile.DefaultCookTimeSeconds <= 0f)
            {
                return CCS_SurvivalValidationResult.Fail("Default cook time must be greater than zero.");
            }

            if (profile.DefaultCampfireDefinition == null)
            {
                return CCS_SurvivalValidationResult.Fail("Default campfire definition is required.");
            }

            CCS_SurvivalValidationResult campfireValidation =
                ValidateCampfireDefinition(profile.DefaultCampfireDefinition);

            if (!campfireValidation.IsSuccess)
            {
                return campfireValidation;
            }

            if (profile.CampfireBuildingPiece == null)
            {
                return CCS_SurvivalValidationResult.Fail("Campfire building piece is required.");
            }

            CCS_SurvivalValidationResult buildingValidation =
                ValidateCampfireBuildingPiece(profile.CampfireBuildingPiece);

            if (!buildingValidation.IsSuccess)
            {
                return buildingValidation;
            }

            if (profile.RawMeatItemDefinition == null)
            {
                return CCS_SurvivalValidationResult.Fail("Raw meat item definition is required.");
            }

            if (profile.CookedMeatItemDefinition == null)
            {
                return CCS_SurvivalValidationResult.Fail("Cooked meat item definition is required.");
            }

            IReadOnlyList<CCS_ConsumableFoodDefinition> consumableDefinitions = profile.ConsumableFoodDefinitions;
            if (consumableDefinitions == null || consumableDefinitions.Count == 0)
            {
                return CCS_SurvivalValidationResult.Fail("At least one consumable food definition is required.");
            }

            for (int index = 0; index < consumableDefinitions.Count; index++)
            {
                CCS_ConsumableFoodDefinition consumableDefinition = consumableDefinitions[index];
                if (consumableDefinition == null)
                {
                    return CCS_SurvivalValidationResult.Fail("Consumable food definition entry is null.");
                }

                if (consumableDefinition.ItemDefinition == null)
                {
                    return CCS_SurvivalValidationResult.Fail("Consumable food item definition is null.");
                }

                if (consumableDefinition.HungerRestoreAmount <= 0f)
                {
                    return CCS_SurvivalValidationResult.Fail("Consumable food hunger restore must be greater than zero.");
                }
            }

            return CCS_SurvivalValidationResult.Pass("Cooking profile validated.");
        }

        public static CCS_SurvivalValidationResult ValidateCampfireDefinition(CCS_CampfireDefinition campfireDefinition)
        {
            if (campfireDefinition == null)
            {
                return CCS_SurvivalValidationResult.Fail("Campfire definition is null.");
            }

            if (string.IsNullOrWhiteSpace(campfireDefinition.CampfireId))
            {
                return CCS_SurvivalValidationResult.Fail("Campfire ID is required.");
            }

            if (campfireDefinition.CookTimeSeconds <= 0f)
            {
                return CCS_SurvivalValidationResult.Fail("Campfire cook time must be greater than zero.");
            }

            if (campfireDefinition.MaxQueueCount <= 0)
            {
                return CCS_SurvivalValidationResult.Fail("Campfire max queue count must be greater than zero.");
            }

            return CCS_SurvivalValidationResult.Pass("Campfire definition validated.");
        }

        public static CCS_SurvivalValidationResult ValidateCampfireBuildingPiece(
            CCS_BuildingPieceDefinition buildingPieceDefinition)
        {
            if (buildingPieceDefinition == null)
            {
                return CCS_SurvivalValidationResult.Fail("Campfire building piece is null.");
            }

            if (string.IsNullOrWhiteSpace(buildingPieceDefinition.PieceId))
            {
                return CCS_SurvivalValidationResult.Fail("Campfire building piece ID is required.");
            }

            if (buildingPieceDefinition.BuildingPieceType != CCS_BuildingPieceType.CampStructure)
            {
                return CCS_SurvivalValidationResult.Fail("Campfire building piece must use CampStructure type.");
            }

            IReadOnlyList<CCS_BuildingCostEntry> buildCostEntries = buildingPieceDefinition.BuildCostEntries;
            if (buildCostEntries == null || buildCostEntries.Count == 0)
            {
                return CCS_SurvivalValidationResult.Fail("Campfire building piece requires at least one build cost entry.");
            }

            return CCS_SurvivalValidationResult.Pass("Campfire building piece validated.");
        }

        public static CCS_SurvivalValidationResult ValidateCookingStationType()
        {
            if (!System.Enum.IsDefined(typeof(CCS_CraftingStationType), CCS_CraftingStationType.FirePit))
            {
                return CCS_SurvivalValidationResult.Fail("FirePit crafting station type is missing.");
            }

            return CCS_SurvivalValidationResult.Pass("FirePit station type validated.");
        }

        #endregion
    }
}
