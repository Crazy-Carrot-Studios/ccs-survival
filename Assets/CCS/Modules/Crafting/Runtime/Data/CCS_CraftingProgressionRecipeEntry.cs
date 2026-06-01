using System;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CraftingProgressionRecipeEntry
// CATEGORY: Modules / Crafting / Runtime / Data
// PURPOSE: Links a crafting recipe asset to progression unlock tier metadata.
// PLACEMENT: Serialized on CCS_CraftingProgressionProfile recipe lists.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Recipe identity and costs live on CCS_CraftingRecipeDefinition assets.
// =============================================================================

namespace CCS.Modules.Crafting
{
    [Serializable]
    public sealed class CCS_CraftingProgressionRecipeEntry
    {
        #region Variables

        [Header("Recipe")]
        [Tooltip("Authoritative recipe definition for this progression entry.")]
        [SerializeField] private CCS_CraftingRecipeDefinition recipeDefinition;

        [Tooltip("Progression tier required before this recipe is treated as unlocked.")]
        [SerializeField] private int unlockTier = 1;

        #endregion

        #region Properties

        public CCS_CraftingRecipeDefinition RecipeDefinition => recipeDefinition;

        public int UnlockTier => unlockTier < 1 ? 1 : unlockTier;

        #endregion
    }
}
