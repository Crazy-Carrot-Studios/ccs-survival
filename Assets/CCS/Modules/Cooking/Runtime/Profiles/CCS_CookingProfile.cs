using System;
using System.Collections.Generic;
using CCS.Modules.Building;
using CCS.Modules.Inventory;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CookingProfile
// CATEGORY: Modules / Cooking / Runtime / Profiles
// PURPOSE: Tuning profile for campfire cooking recipes, fuel rules, and consumable food.
// PLACEMENT: Assets/CCS/Survival/Profiles/Cooking/ (project shell configuration).
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: 1.0.0 adds recipe list, fuel rules, and species-specific meat cooking.
// =============================================================================

namespace CCS.Modules.Cooking
{
    [CreateAssetMenu(
        fileName = "CCS_CookingProfile",
        menuName = "CCS/Survival/Cooking/Cooking Profile")]
    public sealed class CCS_CookingProfile : CCS_SurvivalProfileBase
    {
        #region Variables

        [Header("Interaction Defaults")]
        [Tooltip("Default interaction distance used by CCS_CookingInteractable.")]
        [SerializeField] private float defaultInteractDistance = 3f;

        [Tooltip("Default seconds required to cook when a recipe omits duration.")]
        [SerializeField] private float defaultCookDurationSeconds = 5f;

        [Tooltip("Default fuel burn duration placeholder for future fuel simulation.")]
        [SerializeField] private float defaultFuelBurnDurationSeconds = 30f;

        [Header("Cooking")]
        [Tooltip("When enabled, cooking stations may process recipes through interaction.")]
        [SerializeField] private bool enableCooking = true;

        [Tooltip("When enabled, newly registered campfires start lit.")]
        [SerializeField] private bool autoLightCampfiresOnPlacement = true;

        [Header("Campfire")]
        [Tooltip("Default campfire definition used by legacy campfire service flows.")]
        [SerializeField] private CCS_CampfireDefinition defaultCampfireDefinition;

        [Tooltip("Building piece placed when consuming a campfire kit.")]
        [SerializeField] private CCS_BuildingPieceDefinition campfireBuildingPiece;

        [Header("Recipes")]
        [Tooltip("Item definitions used to resolve recipe itemDefinitionId strings.")]
        [SerializeField] private CCS_ItemDefinition[] recipeItemCatalog;

        [Tooltip("Campfire cooking recipes available in 1.0.0 foundation.")]
        [SerializeField] private CCS_CookingRecipe[] recipes;

        [Header("Legacy Cooking Items")]
        [Tooltip("Legacy generic raw meat used by older campfire flows.")]
        [SerializeField] private CCS_ItemDefinition rawMeatItemDefinition;

        [Tooltip("Legacy generic cooked meat used by older campfire flows.")]
        [SerializeField] private CCS_ItemDefinition cookedMeatItemDefinition;

        [Header("Consumable Food")]
        [Tooltip("Food items that restore hunger when consumed.")]
        [SerializeField] private List<CCS_ConsumableFoodDefinition> consumableFoodDefinitions =
            new List<CCS_ConsumableFoodDefinition>();

        private Dictionary<string, CCS_ItemDefinition> recipeDefinitionsById;

        #endregion

        #region Properties

        public float DefaultInteractDistance => defaultInteractDistance;

        public float DefaultCookDurationSeconds => defaultCookDurationSeconds;

        public float DefaultFuelBurnDurationSeconds => defaultFuelBurnDurationSeconds;

        public bool EnableCooking => enableCooking;

        public float DefaultCookTimeSeconds => defaultCookDurationSeconds;

        public bool AutoLightCampfiresOnPlacement => autoLightCampfiresOnPlacement;

        public CCS_CampfireDefinition DefaultCampfireDefinition => defaultCampfireDefinition;

        public CCS_BuildingPieceDefinition CampfireBuildingPiece => campfireBuildingPiece;

        public IReadOnlyList<CCS_CookingRecipe> Recipes => recipes;

        public CCS_ItemDefinition RawMeatItemDefinition => rawMeatItemDefinition;

        public CCS_ItemDefinition CookedMeatItemDefinition => cookedMeatItemDefinition;

        public IReadOnlyList<CCS_ConsumableFoodDefinition> ConsumableFoodDefinitions => consumableFoodDefinitions;

        #endregion

        #region Public Methods

        public void BuildRecipeLookup()
        {
            recipeDefinitionsById = new Dictionary<string, CCS_ItemDefinition>(StringComparer.OrdinalIgnoreCase);
            if (recipeItemCatalog == null)
            {
                return;
            }

            for (int index = 0; index < recipeItemCatalog.Length; index++)
            {
                CCS_ItemDefinition itemDefinition = recipeItemCatalog[index];
                if (itemDefinition == null || string.IsNullOrWhiteSpace(itemDefinition.ItemId))
                {
                    continue;
                }

                recipeDefinitionsById[itemDefinition.ItemId] = itemDefinition;
            }
        }

        public bool TryGetRecipe(string recipeId, out CCS_CookingRecipe recipe)
        {
            recipe = null;
            if (string.IsNullOrWhiteSpace(recipeId) || recipes == null)
            {
                return false;
            }

            for (int index = 0; index < recipes.Length; index++)
            {
                CCS_CookingRecipe candidate = recipes[index];
                if (candidate != null
                    && string.Equals(candidate.RecipeId, recipeId, StringComparison.OrdinalIgnoreCase))
                {
                    recipe = candidate;
                    return true;
                }
            }

            return false;
        }

        public bool TryResolveItemDefinition(string itemDefinitionId, out CCS_ItemDefinition itemDefinition)
        {
            itemDefinition = null;
            if (string.IsNullOrWhiteSpace(itemDefinitionId))
            {
                return false;
            }

            if (recipeDefinitionsById == null)
            {
                BuildRecipeLookup();
            }

            return recipeDefinitionsById != null
                && recipeDefinitionsById.TryGetValue(itemDefinitionId, out itemDefinition)
                && itemDefinition != null;
        }

        #endregion
    }
}
