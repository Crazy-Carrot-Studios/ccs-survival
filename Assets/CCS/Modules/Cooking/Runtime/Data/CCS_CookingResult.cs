// =============================================================================
// SCRIPT: CCS_CookingResult
// CATEGORY: Modules / Cooking / Runtime / Data
// PURPOSE: Represents the outcome of a cooking attempt.
// PLACEMENT: Returned by CCS_CookingService cooking methods.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Safe failure results instead of exceptions.
// =============================================================================

namespace CCS.Modules.Cooking
{
    public sealed class CCS_CookingResult
    {
        #region Public Methods

        public static CCS_CookingResult Success(string message = "Cooking completed.")
        {
            return new CCS_CookingResult(true, message ?? string.Empty);
        }

        public static CCS_CookingResult Failure(string message)
        {
            return new CCS_CookingResult(false, message ?? string.Empty);
        }

        private CCS_CookingResult(bool isSuccess, string message)
        {
            IsSuccess = isSuccess;
            Message = message ?? string.Empty;
        }

        #endregion

        #region Properties

        public bool IsSuccess { get; }

        public string Message { get; }

        #endregion
    }
}
