// =============================================================================
// SCRIPT: CCS_PopulationPresenceContentIds
// CATEGORY: Modules / Settlements / Runtime / Data
// PURPOSE: Stable ids for population presence bootstrap and playtest harness.
// PLACEMENT: Shared by bootstrap setup, validators, and playtest service.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 4.0.0 NPC population placeholder foundation.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public static class CCS_PopulationPresenceContentIds
    {
        public const string PresenceProfilesRoot = "Assets/CCS/Survival/Profiles/Settlements/PopulationPresence";
        public const string DefaultPresenceProfilePath =
            PresenceProfilesRoot + "/CCS_DefaultPopulationPresenceProfile.asset";
        public const string DefaultPresenceProfileId = "ccs.survival.profile.populationpresence.default";

        public const string TradingPostMerchantsAnchorId =
            "ccs.survival.populationpresence.tradingpost.merchants";
        public const string TradingPostLaborersAnchorId =
            "ccs.survival.populationpresence.tradingpost.laborers";

        public const string BrokenCreekFarmersAnchorId =
            "ccs.survival.populationpresence.brokencreek.farmers";
        public const string BrokenCreekRanchersAnchorId =
            "ccs.survival.populationpresence.brokencreek.ranchers";

        public const string IronRidgeMinersAnchorId =
            "ccs.survival.populationpresence.ironridge.miners";
        public const string IronRidgeLaborersAnchorId =
            "ccs.survival.populationpresence.ironridge.laborers";

        public const string PineRidgeLumberWorkersAnchorId =
            "ccs.survival.populationpresence.pineridge.lumberworkers";
        public const string PineRidgeLaborersAnchorId =
            "ccs.survival.populationpresence.pineridge.laborers";
    }
}
