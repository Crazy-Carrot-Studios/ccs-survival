// =============================================================================
// SCRIPT: CCS_TradeRoutesFreightContentIds
// CATEGORY: Modules / Settlements / Runtime / Data
// PURPOSE: Stable ids for trade route freight bootstrap, validation, and playtest harness.
// PLACEMENT: Shared by bootstrap setup, validators, and playtest service.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.4.0 trade routes and freight contracts.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public static class CCS_TradeRoutesFreightContentIds
    {
        public const string PineRidgeToTradingPostRouteId =
            "ccs.survival.traderoute.frontier.pineridge.tradingpost";
        public const string BrokenCreekToTradingPostRouteId =
            "ccs.survival.traderoute.frontier.brokencreek.tradingpost";
        public const string IronRidgeToTradingPostRouteId =
            "ccs.survival.traderoute.frontier.ironridge.tradingpost";

        public const string TradingPostToPineRidgeMixedRouteId =
            "ccs.survival.traderoute.frontier.tradingpost.pineridge.mixed";
        public const string TradingPostToBrokenCreekMixedRouteId =
            "ccs.survival.traderoute.frontier.tradingpost.brokencreek.mixed";
        public const string TradingPostToIronRidgeMixedRouteId =
            "ccs.survival.traderoute.frontier.tradingpost.ironridge.mixed";

        public const string PineRidgeLumberFreightContractId =
            "ccs.survival.contract.freight.pineridge.lumber";
        public const string PineRidgeCharcoalFreightContractId =
            "ccs.survival.contract.freight.pineridge.charcoal";
        public const string BrokenCreekCornFreightContractId =
            "ccs.survival.contract.freight.brokencreek.corn";
        public const string BrokenCreekWheatFreightContractId =
            "ccs.survival.contract.freight.brokencreek.wheat";
        public const string IronRidgeIronOreFreightContractId =
            "ccs.survival.contract.freight.ironridge.ironore";
        public const string IronRidgeCoalFreightContractId =
            "ccs.survival.contract.freight.ironridge.coal";

        public const string TradingPostPineMixedFreightContractId =
            "ccs.survival.contract.freight.tradingpost.pineridge.mixed";
        public const string TradingPostBrokenCreekMixedFreightContractId =
            "ccs.survival.contract.freight.tradingpost.brokencreek.mixed";
        public const string TradingPostIronRidgeMixedFreightContractId =
            "ccs.survival.contract.freight.tradingpost.ironridge.mixed";

        public const string FreightContractsContentRoot = "Assets/CCS/Survival/Content/Contracts/Freight";
    }
}
