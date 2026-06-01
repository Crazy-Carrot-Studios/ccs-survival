using System.Collections.Generic;
using CCS.Modules.Building;
using CCS.Modules.Crafting;
using CCS.Modules.Inventory;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_CookingValidationUtility
// CATEGORY: Modules / Cooking / Runtime / Validation
// PURPOSE: Runtime-safe validation for cooking profiles, recipes, and campfire content.
// PLACEMENT: Used by editor validators and cooking service preflight checks.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Returns CCS_SurvivalValidationResult. No UnityEditor references.
// =============================================================================

namespace CCS.Modules.Cooking
{
    public static class CCS_CookingValidationUtility
    {
        private const string CookRabbitRecipeId = "ccs.survival.cooking.recipe.cookrabbit";
        private const string CookVenisonRecipeId = "ccs.survival.cooking.recipe.cookvenison";

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

            if (profile.DefaultInteractDistance <= 0f)
            {
                return CCS_SurvivalValidationResult.Fail("Default interact distance must be greater than zero.");
            }

            if (profile.DefaultCookDurationSeconds <= 0f)
            {
                return CCS_SurvivalValidationResult.Fail("Default cook duration must be greater than zero.");
            }

            if (profile.DefaultFuelBurnDurationSeconds <= 0f)
            {
                return CCS_SurvivalValidationResult.Fail("Default fuel burn duration must be greater than zero.");
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

            profile.BuildRecipeLookup();
            CCS_SurvivalValidationResult rabbitRecipeValidation = ValidateRecipeExists(profile, CookRabbitRecipeId);
            if (!rabbitRecipeValidation.IsSuccess)
            {
                return rabbitRecipeValidation;
            }

            CCS_SurvivalValidationResult venisonRecipeValidation = ValidateRecipeExists(profile, CookVenisonRecipeId);
            if (!venisonRecipeValidation.IsSuccess)
            {
                return venisonRecipeValidation;
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

            if (!System.Enum.IsDefined(typeof(CCS_CookingStationType), CCS_CookingStationType.Campfire))
            {
                return CCS_SurvivalValidationResult.Fail("Campfire cooking station type is missing.");
            }

            return CCS_SurvivalValidationResult.Pass("Cooking station types validated.");
        }

        #endregion

        #region Private Methods

        private static CCS_SurvivalValidationResult ValidateRecipeExists(CCS_CookingProfile profile, string recipeId)
        {
            if (!profile.TryGetRecipe(recipeId, out CCS_CookingRecipe recipe) || recipe == null)
            {
                return CCS_SurvivalValidationResult.Fail($"Cooking recipe '{recipeId}' is missing.");
            }

            if (string.IsNullOrWhiteSpace(recipe.RawItemDefinitionId)
                || !profile.TryResolveItemDefinition(recipe.RawItemDefinitionId, out _))
            {
                return CCS_SurvivalValidationResult.Fail($"Recipe '{recipeId}' raw item is not configured.");
            }

            if (string.IsNullOrWhiteSpace(recipe.CookedItemDefinitionId)
                || !profile.TryResolveItemDefinition(recipe.CookedItemDefinitionId, out _))
            {
                return CCS_SurvivalValidationResult.Fail($"Recipe '{recipeId}' cooked item is not configured.");
            }

            if (recipe.AcceptedFuelItemIds == null || recipe.AcceptedFuelItemIds.Count == 0)
            {
                return CCS_SurvivalValidationResult.Fail($"Recipe '{recipeId}' must accept fuel item IDs.");
            }

            return CCS_SurvivalValidationResult.Pass($"Recipe '{recipeId}' validated.");
        }

        #endregion
    }
}
