// =============================================================================
// SCRIPT: CCS_CraftingResult
// CATEGORY: Modules / Crafting / Runtime / Data
// PURPOSE: Represents the outcome of a crafting attempt.
// PLACEMENT: Returned by CCS_CraftingService craft methods.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Safe failure results instead of exceptions. Not a recipe output definition.
// =============================================================================

namespace CCS.Modules.Crafting
{
    public sealed class CCS_CraftingResult
    {
        #region Public Methods

        public static CCS_CraftingResult Success(
            CCS_CraftingRecipeDefinition recipe,
            int craftCount,
            string message = "Crafting completed.")
        {
            return new CCS_CraftingResult(true, recipe, craftCount, message ?? string.Empty);
        }

        public static CCS_CraftingResult Failure(
            CCS_CraftingRecipeDefinition recipe,
            string message)
        {
            return new CCS_CraftingResult(false, recipe, 0, message ?? string.Empty);
        }

        private CCS_CraftingResult(
            bool isSuccess,
            CCS_CraftingRecipeDefinition recipe,
            int craftCount,
            string message)
        {
            IsSuccess = isSuccess;
            Recipe = recipe;
            CraftCount = craftCount < 0 ? 0 : craftCount;
            Message = message ?? string.Empty;
        }

        #endregion

        #region Properties

        public bool IsSuccess { get; }

        public CCS_CraftingRecipeDefinition Recipe { get; }

        public int CraftCount { get; }

        public string Message { get; }

        #endregion
    }
}
