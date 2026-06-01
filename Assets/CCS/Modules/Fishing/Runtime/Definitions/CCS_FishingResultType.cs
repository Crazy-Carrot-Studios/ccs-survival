// =============================================================================
// SCRIPT: CCS_FishingResultType
// CATEGORY: Modules / Fishing / Runtime / Definitions
// PURPOSE: Outcome codes for fishing attempts through CCS_FishingService.
// PLACEMENT: Returned by CCS_FishingService.TryFish and mapped to active item results.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Foundation outcomes only. No minigame scoring types yet.
// =============================================================================

namespace CCS.Modules.Fishing
{
    public enum CCS_FishingResultType
    {
        None = 0,
        FishCaught = 1,
        SmallFishCaught = 2,
        JunkCaught = 3,
        NothingCaught = 4,
        Failed = 5,
        NoBait = 6,
        NoWater = 7,
        TargetUnavailable = 8,
        ServiceUnavailable = 9
    }
}
