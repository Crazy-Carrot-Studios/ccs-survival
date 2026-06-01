// =============================================================================
// SCRIPT: CCS_CraftingProgressionEventArgs
// CATEGORY: Modules / Crafting / Runtime / Events
// PURPOSE: Event payload for crafting progression recipe lifecycle notifications.
// PLACEMENT: Raised by CCS_CraftingRecipeService during validation and crafting.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Milestone 1.1.1 crafting progression foundation.
// =============================================================================

namespace CCS.Modules.Crafting
{
    public sealed class CCS_CraftingProgressionEventArgs
    {
        #region Public Methods

        public CCS_CraftingProgressionEventArgs(
            string recipeId,
            CCS_CraftingStationType stationType,
            int craftCount,
            string message)
        {
            RecipeId = recipeId ?? string.Empty;
            StationType = stationType;
            CraftCount = craftCount < 0 ? 0 : craftCount;
            Message = message ?? string.Empty;
        }

        #endregion

        #region Properties

        public string RecipeId { get; }

        public CCS_CraftingStationType StationType { get; }

        public int CraftCount { get; }

        public string Message { get; }

        #endregion
    }
}
