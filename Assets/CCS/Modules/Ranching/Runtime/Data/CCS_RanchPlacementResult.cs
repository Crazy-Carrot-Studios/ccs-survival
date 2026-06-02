// =============================================================================
// SCRIPT: CCS_RanchPlacementResult
// CATEGORY: Modules / Ranching / Runtime / Data
// PURPOSE: Result payload for ranch structure placement attempts.
// PLACEMENT: Returned by CCS_RanchService placement handlers.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// NOTES: Milestone 2.1.0 ranching foundation.
// =============================================================================

namespace CCS.Modules.Ranching
{
    public sealed class CCS_RanchPlacementResult
    {
        private CCS_RanchPlacementResult(bool isSuccess, bool isPreview, bool isValid, string message)
        {
            IsSuccess = isSuccess;
            IsPreview = isPreview;
            IsValid = isValid;
            Message = message ?? string.Empty;
        }

        public bool IsSuccess { get; }

        public bool IsPreview { get; }

        public bool IsValid { get; }

        public string Message { get; }

        public static CCS_RanchPlacementResult Failure(string message)
        {
            return new CCS_RanchPlacementResult(false, false, false, message);
        }

        public static CCS_RanchPlacementResult Preview(bool isValid, string message)
        {
            return new CCS_RanchPlacementResult(true, true, isValid, message);
        }

        public static CCS_RanchPlacementResult Placed(string message)
        {
            return new CCS_RanchPlacementResult(true, false, true, message);
        }
    }
}
