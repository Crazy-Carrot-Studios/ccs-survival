// =============================================================================
// SCRIPT: CCS_WorldSimulationContentIds
// CATEGORY: Modules / WorldSimulation / Runtime / Data
// PURPOSE: Stable content ids for world simulation bootstrap and validation.
// PLACEMENT: Shared by bootstrap setup, validators, and playtest harness.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// NOTES: Western-specific ids for Survival content assets.
// =============================================================================

using CCS.Modules.Settlements;

namespace CCS.Modules.WorldSimulation
{
    public static class CCS_WorldSimulationContentIds
    {
        public const string DefaultProfileId = "ccs.survival.profile.worldsimulation.default";
        public const string GeneralStoreVendorId = "ccs.survival.vendor.frontier.generalstore";
        public const string TradingPostSettlementId = "ccs.survival.settlement.frontier.testtradingpost";

        public const string DefaultGrowthProfilePath = CCS_SettlementGrowthContentIds.DefaultGrowthProfilePath;
    }
}
