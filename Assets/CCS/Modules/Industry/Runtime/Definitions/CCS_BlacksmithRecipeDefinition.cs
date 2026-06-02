using CCS.Modules.Crafting;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_BlacksmithRecipeDefinition
// CATEGORY: Modules / Industry / Runtime / Definitions
// PURPOSE: Maps forge crafting recipes to blacksmith categories for validation.
// PLACEMENT: Assets/CCS/Survival/Content/Industry/Blacksmith/
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// =============================================================================

namespace CCS.Modules.Industry
{
    [CreateAssetMenu(
        fileName = "CCS_BlacksmithRecipeDefinition",
        menuName = "CCS/Survival/Industry/Blacksmith Recipe Definition")]
    public sealed class CCS_BlacksmithRecipeDefinition : ScriptableObject
    {
        [SerializeField] private string blacksmithRecipeId = "ccs.survival.industry.blacksmith.example";
        [SerializeField] private CCS_BlacksmithRecipeCategory category = CCS_BlacksmithRecipeCategory.Tool;
        [SerializeField] private CCS_CraftingRecipeDefinition craftingRecipe;

        public string BlacksmithRecipeId => blacksmithRecipeId ?? string.Empty;

        public CCS_BlacksmithRecipeCategory Category => category;

        public CCS_CraftingRecipeDefinition CraftingRecipe => craftingRecipe;
    }
}
