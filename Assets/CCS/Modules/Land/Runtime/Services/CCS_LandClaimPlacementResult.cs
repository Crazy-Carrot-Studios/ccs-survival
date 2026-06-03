// =============================================================================
// SCRIPT: CCS_LandClaimPlacementResult
// CATEGORY: Modules / Land / Runtime / Services
// PURPOSE: Result payload for land claim deed preview and placement.
// PLACEMENT: Returned by CCS_LandClaimService.HandlePlacementRequest.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-02
// NOTES: Milestone 2.3.0 land claim placement flow.
// =============================================================================

namespace CCS.Modules.Land
{
    public sealed class CCS_LandClaimPlacementResult
    {
        private CCS_LandClaimPlacementResult(bool isSuccess, bool isPreview, bool isValid, string message)
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

        public static CCS_LandClaimPlacementResult Failure(string message)
        {
            return new CCS_LandClaimPlacementResult(false, false, false, message);
        }

        public static CCS_LandClaimPlacementResult Preview(bool isValid, string message)
        {
            return new CCS_LandClaimPlacementResult(true, true, isValid, message);
        }

        public static CCS_LandClaimPlacementResult Placed(string message)
        {
            return new CCS_LandClaimPlacementResult(true, false, true, message);
        }
    }
}
