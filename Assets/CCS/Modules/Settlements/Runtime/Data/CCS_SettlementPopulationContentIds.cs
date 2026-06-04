// =============================================================================
// SCRIPT: CCS_SettlementPopulationContentIds
// CATEGORY: Modules / Settlements / Runtime / Data
// PURPOSE: Stable ids and paths for population foundation bootstrap and validation.
// PLACEMENT: Shared by editor bootstrap, validators, and playtest harness.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.6.0 population foundation.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public static class CCS_SettlementPopulationContentIds
    {
        public const string PopulationProfilesRoot = "Assets/CCS/Survival/Profiles/Settlements/Population";
        public const string DefaultPopulationProfilePath =
            PopulationProfilesRoot + "/CCS_DefaultSettlementPopulationProfile.asset";
        public const string DefaultPopulationProfileId = "ccs.survival.profile.settlementpopulation.default";
    }
}
