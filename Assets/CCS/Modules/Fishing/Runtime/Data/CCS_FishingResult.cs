// =============================================================================
// SCRIPT: CCS_FishingResult
// CATEGORY: Modules / Fishing / Runtime / Data
// PURPOSE: Result payload for fishing attempts including inventory grant status.
// PLACEMENT: Returned by CCS_FishingService.TryFish.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: DidGrantItem distinguishes clean nothing rolls from hard failures.
// =============================================================================

namespace CCS.Modules.Fishing
{
    public sealed class CCS_FishingResult
    {
        public CCS_FishingResult(
            CCS_FishingResultType resultType,
            string message,
            bool didGrantItem,
            string grantedItemId = null,
            int grantedQuantity = 0)
        {
            ResultType = resultType;
            Message = message ?? string.Empty;
            DidGrantItem = didGrantItem;
            GrantedItemId = grantedItemId;
            GrantedQuantity = grantedQuantity;
        }

        public CCS_FishingResultType ResultType { get; }

        public string Message { get; }

        public bool DidGrantItem { get; }

        public string GrantedItemId { get; }

        public int GrantedQuantity { get; }

        public bool DidSucceedAttempt =>
            ResultType == CCS_FishingResultType.FishCaught
            || ResultType == CCS_FishingResultType.SmallFishCaught
            || ResultType == CCS_FishingResultType.JunkCaught
            || ResultType == CCS_FishingResultType.NothingCaught;
    }
}
