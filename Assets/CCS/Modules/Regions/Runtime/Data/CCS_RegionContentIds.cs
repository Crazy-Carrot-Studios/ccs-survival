// =============================================================================
// SCRIPT: CCS_RegionContentIds
// CATEGORY: Modules / Regions / Runtime / Data
// PURPOSE: Stable content ids for frontier region bootstrap and validation.
// PLACEMENT: Shared by bootstrap setup, validators, and playtest harness.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// NOTES: Western-specific ids for Survival content assets.
// =============================================================================

namespace CCS.Modules.Regions
{
    public static class CCS_RegionContentIds
    {
        public const string PineRidgeForestRegionId = "ccs.survival.region.frontier.pineridgeforest";
        public const string BrokenCreekRegionId = "ccs.survival.region.frontier.brokencreek";
        public const string IronRidgeMineRegionId = "ccs.survival.region.frontier.ironridgemine";
        public const string FrontierTradingPostRegionId = "ccs.survival.region.frontier.tradingpost";

        public const string PineRidgeForestVolumeObjectName = "CCS_RegionVolume_PineRidgeForest";
        public const string BrokenCreekVolumeObjectName = "CCS_RegionVolume_BrokenCreek";
        public const string IronRidgeMineVolumeObjectName = "CCS_RegionVolume_IronRidgeMine";
        public const string FrontierTradingPostRegionVolumeObjectName = "CCS_RegionVolume_FrontierTradingPost";

        public const int BootstrapRegionCount = 4;
    }
}
