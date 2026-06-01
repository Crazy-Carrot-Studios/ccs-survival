// =============================================================================
// SCRIPT: CCS_FishingEventArgs
// CATEGORY: Modules / Fishing / Runtime / Events
// PURPOSE: Event payload for fishing attempts and catches.
// PLACEMENT: Raised by CCS_FishingService on successful or failed attempts.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Keeps spot id and result type for playtest and future UI listeners.
// =============================================================================

namespace CCS.Modules.Fishing
{
    public sealed class CCS_FishingEventArgs
    {
        public CCS_FishingEventArgs(
            string spotId,
            CCS_FishingResultType resultType,
            string message,
            string grantedItemId,
            int grantedQuantity)
        {
            SpotId = spotId ?? string.Empty;
            ResultType = resultType;
            Message = message ?? string.Empty;
            GrantedItemId = grantedItemId;
            GrantedQuantity = grantedQuantity;
        }

        public string SpotId { get; }

        public CCS_FishingResultType ResultType { get; }

        public string Message { get; }

        public string GrantedItemId { get; }

        public int GrantedQuantity { get; }
    }
}
