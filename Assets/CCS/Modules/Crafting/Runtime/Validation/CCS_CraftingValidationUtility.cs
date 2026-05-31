using System.Collections.Generic;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_CraftingValidationUtility
// CATEGORY: Modules / Crafting / Runtime / Validation
// PURPOSE: Runtime-safe validation for crafting profiles, recipes, and station types.
// PLACEMENT: Used by editor validators and crafting service preflight checks.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Returns CCS_SurvivalValidationResult. No UnityEditor references.
// =============================================================================

namespace CCS.Modules.Crafting
{
    public static class CCS_CraftingValidationUtility
    {
        #region Public Methods

        public static CCS_SurvivalValidationResult ValidateProfile(CCS_CraftingProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail("Crafting profile is null.");
            }

            CCS_SurvivalValidationResult baseValidation = CCS_SurvivalProfileValidationUtility.ValidateProfile(profile);
            if (!baseValidation.IsSuccess)
            {
                return baseValidation;
            }

            if (profile.MaxQueueSize < 1)
            {
                return CCS_SurvivalValidationResult.Fail("Max queue size must be at least one.");
            }

            if (profile.CraftTimeMultiplier < 0f)
            {
                return CCS_SurvivalValidationResult.Fail("Craft time multiplier cannot be negative.");
            }

            return CCS_SurvivalValidationResult.Pass("Crafting profile validated.");
        }

        public static CCS_SurvivalValidationResult ValidateRecipeDefinition(
            CCS_CraftingRecipeDefinition recipeDefinition)
        {
            if (recipeDefinition == null)
            {
                return CCS_SurvivalValidationResult.Fail("Recipe definition is null.");
            }

            if (string.IsNullOrWhiteSpace(recipeDefinition.RecipeId))
            {
                return CCS_SurvivalValidationResult.Fail("Recipe ID is required.");
            }

            if (recipeDefinition.CraftTimeSeconds < 0f)
            {
                return CCS_SurvivalValidationResult.Fail("Craft time seconds cannot be negative.");
            }

            IReadOnlyList<CCS_CraftingIngredientDefinition> ingredients = recipeDefinition.Ingredients;
            if (ingredients == null || ingredients.Count == 0)
            {
                return CCS_SurvivalValidationResult.Fail("Recipe requires at least one ingredient.");
            }

            for (int i = 0; i < ingredients.Count; i++)
            {
                CCS_CraftingIngredientDefinition ingredient = ingredients[i];
                if (ingredient == null)
                {
                    return CCS_SurvivalValidationResult.Fail("Recipe ingredient entry is null.");
                }

                if (ingredient.ItemDefinition == null)
                {
                    return CCS_SurvivalValidationResult.Fail("Recipe ingredient item definition is null.");
                }

                if (ingredient.Quantity <= 0)
                {
                    return CCS_SurvivalValidationResult.Fail("Recipe ingredient quantity must be greater than zero.");
                }
            }

            IReadOnlyList<CCS_CraftingResultDefinition> results = recipeDefinition.Results;
            if (results == null || results.Count == 0)
            {
                return CCS_SurvivalValidationResult.Fail("Recipe requires at least one result.");
            }

            for (int i = 0; i < results.Count; i++)
            {
                CCS_CraftingResultDefinition result = results[i];
                if (result == null)
                {
                    return CCS_SurvivalValidationResult.Fail("Recipe result entry is null.");
                }

                if (result.ItemDefinition == null)
                {
                    return CCS_SurvivalValidationResult.Fail("Recipe result item definition is null.");
                }

                if (result.Quantity <= 0)
                {
                    return CCS_SurvivalValidationResult.Fail("Recipe result quantity must be greater than zero.");
                }
            }

            return CCS_SurvivalValidationResult.Pass("Recipe definition validated.");
        }

        public static CCS_SurvivalValidationResult ValidateRequiredStationTypes()
        {
            if (!System.Enum.IsDefined(typeof(CCS_CraftingStationType), CCS_CraftingStationType.Hand))
            {
                return CCS_SurvivalValidationResult.Fail("Crafting station type Hand is not defined.");
            }

            if (!System.Enum.IsDefined(typeof(CCS_CraftingStationType), CCS_CraftingStationType.FirePit))
            {
                return CCS_SurvivalValidationResult.Fail("Crafting station type FirePit is not defined.");
            }

            if (!System.Enum.IsDefined(typeof(CCS_CraftingStationType), CCS_CraftingStationType.Workbench))
            {
                return CCS_SurvivalValidationResult.Fail("Crafting station type Workbench is not defined.");
            }

            if (!System.Enum.IsDefined(typeof(CCS_CraftingStationType), CCS_CraftingStationType.Forge))
            {
                return CCS_SurvivalValidationResult.Fail("Crafting station type Forge is not defined.");
            }

            if (!System.Enum.IsDefined(typeof(CCS_CraftingStationType), CCS_CraftingStationType.Apothecary))
            {
                return CCS_SurvivalValidationResult.Fail("Crafting station type Apothecary is not defined.");
            }

            return CCS_SurvivalValidationResult.Pass("Required crafting station types validated.");
        }

        #endregion
    }
}
