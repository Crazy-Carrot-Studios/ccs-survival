using System.Collections.Generic;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CraftingProgressionProfile
// CATEGORY: Modules / Crafting / Runtime / Profiles
// PURPOSE: Tiered primitive crafting progression recipes grouped by station context.
// PLACEMENT: Assets/CCS/Survival/Profiles/Crafting/CCS_DefaultCraftingProgressionProfile.asset
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Milestone 1.1.1 crafting progression and workstation foundation.
// =============================================================================

namespace CCS.Modules.Crafting
{
    [CreateAssetMenu(
        fileName = "CCS_CraftingProgressionProfile",
        menuName = "CCS/Survival/Crafting/Crafting Progression Profile")]
    public sealed class CCS_CraftingProgressionProfile : CCS_SurvivalProfileBase
    {
        #region Variables

        [Header("Feature")]
        [Tooltip("When false, progression recipe registration and filtering are skipped.")]
        [SerializeField] private bool progressionEnabled = true;

        [Tooltip("Emit categorized crafting progression debug logs.")]
        [SerializeField] private bool enableDebugLogging = true;

        [Header("Recipes")]
        [Tooltip("Primitive hand, campfire (FirePit), and workbench progression recipes.")]
        [SerializeField] private List<CCS_CraftingProgressionRecipeEntry> progressionRecipes =
            new List<CCS_CraftingProgressionRecipeEntry>();

        [Header("Workbench Playtest")]
        [Tooltip("Recipe id used to complete the bootstrap workbench playtest step.")]
        [SerializeField] private string workbenchPlaytestRecipeId = "ccs.survival.recipe.progression.storagecrate";

        #endregion

        #region Properties

        public bool ProgressionEnabled => progressionEnabled;

        public bool EnableDebugLogging => enableDebugLogging;

        public IReadOnlyList<CCS_CraftingProgressionRecipeEntry> ProgressionRecipes => progressionRecipes;

        public string WorkbenchPlaytestRecipeId => workbenchPlaytestRecipeId;

        #endregion
    }
}
