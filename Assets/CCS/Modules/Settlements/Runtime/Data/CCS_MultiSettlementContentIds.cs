// =============================================================================
// SCRIPT: CCS_MultiSettlementContentIds
// CATEGORY: Modules / Settlements / Runtime / Data
// PURPOSE: Stable ids for multi-settlement bootstrap, validation, and playtest harness.
// PLACEMENT: Shared by bootstrap setup, validators, and playtest service.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.3.0 multi-settlement foundation.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public static class CCS_MultiSettlementContentIds
    {
        public const string SettlementContentRoot = "Assets/CCS/Survival/Content/Settlements";
        public const string TradeRoutesContentRoot = "Assets/CCS/Survival/Content/TradeRoutes";
        public const string TradeRoutesProfilePath =
            "Assets/CCS/Survival/Profiles/Settlements/CCS_DefaultTradeRouteProfile.asset";

        public const string PineRidgeCampDefinitionPath =
            SettlementContentRoot + "/CCS_Settlement_PineRidgeCamp.asset";
        public const string BrokenCreekFarmsteadDefinitionPath =
            SettlementContentRoot + "/CCS_Settlement_BrokenCreekFarmstead.asset";
        public const string IronRidgeMiningCampDefinitionPath =
            SettlementContentRoot + "/CCS_Settlement_IronRidgeMiningCamp.asset";

        public const string PineRidgeCampSettlementId = "ccs.survival.settlement.frontier.pineridgecamp";
        public const string BrokenCreekFarmsteadSettlementId = "ccs.survival.settlement.frontier.brokencreekfarmstead";
        public const string IronRidgeMiningCampSettlementId = "ccs.survival.settlement.frontier.ironridgeminingcamp";

        public const string PineRidgeCampObjectName = "CCS_PineRidgeCamp";
        public const string BrokenCreekFarmsteadObjectName = "CCS_BrokenCreekFarmstead";
        public const string IronRidgeMiningCampObjectName = "CCS_IronRidgeMiningCamp";

        public const string PineRidgeContractBoardObjectName = "CCS_PineRidgeCamp_ContractBoard";
        public const string BrokenCreekContractBoardObjectName = "CCS_BrokenCreekFarmstead_ContractBoard";
        public const string IronRidgeContractBoardObjectName = "CCS_IronRidgeMiningCamp_ContractBoard";

        public const string MultiSettlementPlaytestContractId = "ccs.survival.contract.pineridge.lumber";
        public const int BootstrapSettlementCount = 4;
    }
}
