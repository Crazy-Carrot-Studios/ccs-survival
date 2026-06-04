// =============================================================================
// SCRIPT: CCS_SettlementGrowthContentIds
// CATEGORY: Modules / Settlements / Runtime / Growth
// PURPOSE: Stable content ids for settlement growth bootstrap and playtest harness.
// PLACEMENT: Shared by bootstrap setup, validators, and playtest service.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.2.0 settlement growth foundation.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public static class CCS_SettlementGrowthContentIds
    {
        public const string GrowthContentRoot = "Assets/CCS/Survival/Content/Settlements/Growth";
        public const string GrowthProfilesRoot = "Assets/CCS/Survival/Profiles/Settlements";
        public const string DefaultGrowthProfilePath = GrowthProfilesRoot + "/CCS_DefaultSettlementGrowthProfile.asset";
        public const string DefaultGrowthProfileId = "ccs.survival.profile.settlementgrowth.default";

        public const string OutpostGrowthDefinitionPath = GrowthContentRoot + "/CCS_SettlementGrowth_Outpost.asset";
        public const string TradingPostGrowthDefinitionPath = GrowthContentRoot + "/CCS_SettlementGrowth_TradingPost.asset";
        public const string FrontierTownGrowthDefinitionPath = GrowthContentRoot + "/CCS_SettlementGrowth_FrontierTown.asset";
        public const string EstablishedTownGrowthDefinitionPath = GrowthContentRoot + "/CCS_SettlementGrowth_EstablishedTown.asset";

        public const string TradingPostSettlementId = "ccs.survival.settlement.frontier.testtradingpost";
        public const string PlaytestCornContractId = "ccs.survival.contract.generalstore.corn";
    }
}
