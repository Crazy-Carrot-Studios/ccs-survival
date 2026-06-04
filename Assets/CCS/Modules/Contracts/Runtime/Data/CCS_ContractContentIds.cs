// =============================================================================
// SCRIPT: CCS_ContractContentIds
// CATEGORY: Modules / Contracts / Runtime / Data
// PURPOSE: Stable content ids for contract bootstrap, validation, and playtest harness.
// PLACEMENT: Shared by bootstrap setup, validators, and debug HUD.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.0.0 frontier contracts foundation.
// =============================================================================

namespace CCS.Modules.Contracts
{
    public static class CCS_ContractContentIds
    {
        public const string DefaultContractProfilePath =
            "Assets/CCS/Survival/Profiles/Contracts/CCS_DefaultContractProfile.asset";

        public const string DefaultContractProfileId = "ccs.survival.profile.contracts.default";

        public const string ContractsContentRoot = "Assets/CCS/Survival/Content/Contracts";

        public const string ContractsProfileRoot = "Assets/CCS/Survival/Profiles/Contracts";

        public const string LumberDeliveryContractPath = ContractsContentRoot + "/CCS_Contract_LumberDelivery.asset";
        public const string CornDeliveryContractPath = ContractsContentRoot + "/CCS_Contract_CornDelivery.asset";
        public const string PotatoDeliveryContractPath = ContractsContentRoot + "/CCS_Contract_PotatoDelivery.asset";
        public const string FeedDeliveryContractPath = ContractsContentRoot + "/CCS_Contract_FeedDelivery.asset";
        public const string MilkDeliveryContractPath = ContractsContentRoot + "/CCS_Contract_MilkDelivery.asset";
        public const string IronOreDeliveryContractPath = ContractsContentRoot + "/CCS_Contract_IronOreDelivery.asset";
        public const string RefinedIronDeliveryContractPath = ContractsContentRoot + "/CCS_Contract_RefinedIronDelivery.asset";
        public const string CharcoalDeliveryContractPath = ContractsContentRoot + "/CCS_Contract_CharcoalDelivery.asset";
        public const string MixedFrontierSupplyContractPath = ContractsContentRoot + "/CCS_Contract_MixedFrontierSupply.asset";

        public const string DefaultTradingPostSettlementId =
            "ccs.survival.settlement.frontier.testtradingpost";

        public const string ContractBoardServicePointId =
            "ccs.survival.settlement.service.contractboard";

        public const string LumberDeliveryContractId = "ccs.survival.contract.generalstore.lumber";
        public const string CornDeliveryContractId = "ccs.survival.contract.generalstore.corn";
        public const string PotatoDeliveryContractId = "ccs.survival.contract.generalstore.potato";
        public const string FeedDeliveryContractId = "ccs.survival.contract.stable.feed";
        public const string MilkDeliveryContractId = "ccs.survival.contract.stable.milk";
        public const string IronOreDeliveryContractId = "ccs.survival.contract.gunsmith.ironore";
        public const string RefinedIronDeliveryContractId = "ccs.survival.contract.gunsmith.refinediron";
        public const string CharcoalDeliveryContractId = "ccs.survival.contract.gunsmith.charcoal";
        public const string MixedFrontierSupplyContractId = "ccs.survival.contract.tradingpost.mixed";

        public const string LumberItemId = "ccs.survival.item.resource.lumber";
        public const string CornItemId = "ccs.survival.farming.item.corn";
        public const string PotatoItemId = "ccs.survival.farming.item.potatoes";
        public const string FeedItemId = "ccs.survival.item.ranch.feed";
        public const string MilkItemId = "ccs.survival.item.ranch.milk";
        public const string IronOreItemId = "ccs.survival.item.resource.ironore";
        public const string RefinedIronItemId = "ccs.survival.item.resource.refinediron";
        public const string CharcoalItemId = "ccs.survival.item.progression.charcoal";
        public const string HideItemId = "ccs.survival.item.resource.hide";
        public const string CordageItemId = "ccs.survival.item.frontier.cordage";
        public const string TradeDollarsCurrencyId = "ccs.survival.currency.tradedollars";
    }
}
