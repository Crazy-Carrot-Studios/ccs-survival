// =============================================================================
// SCRIPT: CCS_DynamicContractContentIds
// CATEGORY: Modules / Contracts / Runtime / Dynamic
// PURPOSE: Stable ids for dynamic contract bootstrap, validation, and playtest harness.
// PLACEMENT: Shared by bootstrap setup, validators, and debug HUD.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 5.3.0 dynamic contract generation foundation.
// =============================================================================

namespace CCS.Modules.Contracts
{
    public static class CCS_DynamicContractContentIds
    {
        public const string DynamicProfilesRoot = "Assets/CCS/Survival/Profiles/Contracts";

        public const string DefaultDynamicContractProfilePath =
            DynamicProfilesRoot + "/CCS_DefaultDynamicContractProfile.asset";

        public const string DefaultDynamicContractProfileId = "ccs.survival.profile.dynamiccontracts.default";

        public const string GeneratedContractIdPrefix = "ccs.survival.dynamic.contract.";

        public const string LowFoodSupplyRuleId = "ccs.survival.dynamic.rule.lowfood";
        public const string MarketDayMixedGoodsRuleId = "ccs.survival.dynamic.rule.marketday.mixed";
        public const string HarvestFestivalFreightRuleId = "ccs.survival.dynamic.rule.harvestfestival.freight";
        public const string MiningRegionalSupplyRuleId = "ccs.survival.dynamic.rule.regional.mining";
        public const string TimberRegionalSupplyRuleId = "ccs.survival.dynamic.rule.regional.timber";

        public const string WorkforceDemandPlaceholderRuleId = "ccs.survival.dynamic.rule.placeholder.workforce";
        public const string BusinessDemandPlaceholderRuleId = "ccs.survival.dynamic.rule.placeholder.business";
        public const string TradeRouteDemandPlaceholderRuleId = "ccs.survival.dynamic.rule.placeholder.traderoute";

        public const string PolesItemId = "ccs.survival.item.resource.poles";
        public const string BoneHatchetItemId = "ccs.survival.item.tool.hatchet.bone";
        public const string WheatProxyItemId = CCS_ContractContentIds.FeedItemId;
    }
}
