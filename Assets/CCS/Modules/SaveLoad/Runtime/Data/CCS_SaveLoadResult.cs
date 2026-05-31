// =============================================================================
// SCRIPT: CCS_SaveLoadResult
// CATEGORY: Modules / SaveLoad / Runtime / Data
// PURPOSE: Represents the outcome of a save/load operation.
// PLACEMENT: Returned by CCS_SaveLoadService save and load methods.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Safe failure results instead of exceptions for expected validation failures.
// =============================================================================

namespace CCS.Modules.SaveLoad
{
    public sealed class CCS_SaveLoadResult
    {
        #region Public Methods

        public static CCS_SaveLoadResult Success(string message, string slotId = null)
        {
            return new CCS_SaveLoadResult(true, message ?? string.Empty, slotId);
        }

        public static CCS_SaveLoadResult Failure(string message, string slotId = null)
        {
            return new CCS_SaveLoadResult(false, message ?? string.Empty, slotId);
        }

        private CCS_SaveLoadResult(bool isSuccess, string message, string slotId)
        {
            IsSuccess = isSuccess;
            Message = message ?? string.Empty;
            SlotId = slotId ?? string.Empty;
        }

        #endregion

        #region Properties

        public bool IsSuccess { get; }

        public string Message { get; }

        public string SlotId { get; }

        #endregion
    }
}
