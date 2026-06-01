using CCS.Modules.Inventory;

// =============================================================================
// SCRIPT: CCS_ConsumableFoodResult
// CATEGORY: Modules / Cooking / Runtime / Data
// PURPOSE: Represents the outcome of a food consumption attempt.
// PLACEMENT: Returned by CCS_ConsumableFoodService consumption methods.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Safe failure results instead of exceptions.
// =============================================================================

namespace CCS.Modules.Cooking
{
    public sealed class CCS_ConsumableFoodResult
    {
        #region Public Methods

        public static CCS_ConsumableFoodResult Success(
            CCS_ItemDefinition itemDefinition,
            float hungerRestored,
            string message = "Food consumed.")
        {
            return new CCS_ConsumableFoodResult(true, itemDefinition, hungerRestored, message ?? string.Empty);
        }

        public static CCS_ConsumableFoodResult Failure(string message)
        {
            return new CCS_ConsumableFoodResult(false, null, 0f, message ?? string.Empty);
        }

        private CCS_ConsumableFoodResult(
            bool isSuccess,
            CCS_ItemDefinition itemDefinition,
            float hungerRestored,
            string message)
        {
            IsSuccess = isSuccess;
            ItemDefinition = itemDefinition;
            HungerRestored = hungerRestored;
            Message = message ?? string.Empty;
        }

        #endregion

        #region Properties

        public bool IsSuccess { get; }

        public CCS_ItemDefinition ItemDefinition { get; }

        public float HungerRestored { get; }

        public string Message { get; }

        #endregion
    }
}
