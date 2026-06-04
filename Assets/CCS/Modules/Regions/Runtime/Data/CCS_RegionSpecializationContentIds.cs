// =============================================================================
// SCRIPT: CCS_RegionSpecializationContentIds
// CATEGORY: Modules / Regions / Runtime / Data
// PURPOSE: Stable ids for regional specialization bootstrap and playtest harness.
// PLACEMENT: Shared by bootstrap setup, validators, and playtest service.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.1.0 regional specialization foundation.
// =============================================================================

namespace CCS.Modules.Regions
{
    public static class CCS_RegionSpecializationContentIds
    {
        public const string RegionalEconomyPlaytestCornContractId =
            "ccs.survival.contract.generalstore.corn";

        public const string RegionalEconomyPlaytestSettlementId =
            "ccs.survival.settlement.frontier.testtradingpost";

        public const string RegionalEconomyPlaytestRegionId =
            "ccs.survival.region.frontier.tradingpost";
    }
}
