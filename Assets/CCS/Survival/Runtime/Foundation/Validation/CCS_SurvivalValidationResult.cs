using CCS.Core;

// =============================================================================
// SCRIPT: CCS_SurvivalValidationResult
// CATEGORY: Survival / Runtime / Foundation / Validation
// PURPOSE: Lightweight survival validation outcome with optional warning state.
// PLACEMENT: Static validation utilities. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: Wraps CCS_Result semantics without duplicating Core error infrastructure.
// =============================================================================

namespace CCS.Survival
{
    public readonly struct CCS_SurvivalValidationResult
    {
        #region Public Methods

        public CCS_SurvivalValidationResult(bool isSuccess, bool isWarning, string message)
        {
            IsSuccess = isSuccess;
            IsWarning = isWarning;
            Message = message ?? string.Empty;
        }

        public static CCS_SurvivalValidationResult Pass()
        {
            return new CCS_SurvivalValidationResult(true, false, string.Empty);
        }

        public static CCS_SurvivalValidationResult Pass(string message)
        {
            return new CCS_SurvivalValidationResult(true, false, message);
        }

        public static CCS_SurvivalValidationResult Warn(string message)
        {
            return new CCS_SurvivalValidationResult(true, true, message);
        }

        public static CCS_SurvivalValidationResult Fail(string message)
        {
            return new CCS_SurvivalValidationResult(false, false, message);
        }

        public CCS_Result ToCoreResult()
        {
            if (!IsSuccess)
            {
                return CCS_Result.Failure(CCS_CoreErrorCode.ValidationFailed, Message);
            }

            return CCS_Result.Success(Message);
        }

        #endregion

        #region Properties

        public bool IsSuccess { get; }

        public bool IsWarning { get; }

        public string Message { get; }

        #endregion
    }
}
