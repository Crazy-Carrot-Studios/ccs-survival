using System.Collections.Generic;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CraftingRecipeDefinition
// CATEGORY: Modules / Crafting / Runtime / Definitions
// PURPOSE: ScriptableObject recipe identity, requirements, and outputs.
// PLACEMENT: Create assets under project content folders.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: No UI or world station references in 0.5.0 foundation.
// =============================================================================

namespace CCS.Modules.Crafting
{
    [CreateAssetMenu(
        fileName = "CCS_CraftingRecipeDefinition",
        menuName = "CCS/Survival/Crafting/Recipe Definition")]
    public sealed class CCS_CraftingRecipeDefinition : ScriptableObject
    {
        #region Variables

        [Header("Identity")]
        [Tooltip("Stable reverse-DNS recipe ID for save and runtime identity.")]
        [SerializeField] private string recipeId = string.Empty;

        [Tooltip("Player-facing recipe name.")]
        [SerializeField] private string displayName = string.Empty;

        [Tooltip("Short description for future UI and tooltips.")]
        [SerializeField] private string description = string.Empty;

        [Header("Requirements")]
        [Tooltip("Station type required to craft this recipe.")]
        [SerializeField] private CCS_CraftingStationType requiredStationType = CCS_CraftingStationType.Hand;

        [Tooltip("Items consumed when crafting.")]
        [SerializeField] private List<CCS_CraftingIngredientDefinition> ingredients =
            new List<CCS_CraftingIngredientDefinition>();

        [Tooltip("Items granted when crafting succeeds.")]
        [SerializeField] private List<CCS_CraftingResultDefinition> results =
            new List<CCS_CraftingResultDefinition>();

        [Header("Timing")]
        [Tooltip("Base craft duration in seconds before profile multipliers.")]
        [SerializeField] private float craftTimeSeconds;

        [Header("Unlock")]
        [Tooltip("When true, recipe is available without explicit unlock.")]
        [SerializeField] private bool isUnlockedByDefault = true;

        #endregion

        #region Properties

        public string RecipeId => recipeId;

        public string DisplayName => displayName;

        public string Description => description;

        public CCS_CraftingStationType RequiredStationType => requiredStationType;

        public IReadOnlyList<CCS_CraftingIngredientDefinition> Ingredients => ingredients;

        public IReadOnlyList<CCS_CraftingResultDefinition> Results => results;

        public float CraftTimeSeconds => craftTimeSeconds;

        public bool IsUnlockedByDefault => isUnlockedByDefault;

        #endregion
    }
}
