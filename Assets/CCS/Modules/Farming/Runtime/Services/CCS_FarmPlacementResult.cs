// =============================================================================
// SCRIPT: CCS_FarmPlacementResult
// CATEGORY: Modules / Farming / Runtime / Services
// PURPOSE: Result payload for farm plot placement preview/confirm.
// PLACEMENT: Returned by CCS_FarmService.HandlePlacementRequest.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-02
// NOTES: Milestone 2.2.0.
// =============================================================================

namespace CCS.Modules.Farming
{
    public sealed class CCS_FarmPlacementResult
    {
        private CCS_FarmPlacementResult(
            bool isSuccess,
            bool isPreview,
            bool isValid,
            string message)
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

        public static CCS_FarmPlacementResult Failure(string message)
        {
            return new CCS_FarmPlacementResult(false, false, false, message);
        }

        public static CCS_FarmPlacementResult Preview(bool isValid, string message)
        {
            return new CCS_FarmPlacementResult(true, true, isValid, message);
        }

        public static CCS_FarmPlacementResult Placed(string message)
        {
            return new CCS_FarmPlacementResult(true, false, true, message);
        }
    }
}
