// =============================================================================
// SCRIPT: CCS_SettlementContentIds
// CATEGORY: Modules / Settlements / Runtime / Data
// PURPOSE: Stable content ids for frontier settlement bootstrap and validation.
// PLACEMENT: Shared by bootstrap setup, validators, and playtest harness.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// NOTES: Western-specific ids for Survival content assets.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public static class CCS_SettlementContentIds
    {
        public const string TestTradingPostSettlementId = "ccs.survival.settlement.frontier.testtradingpost";
        public const string GeneralStoreServicePointId = "ccs.survival.settlement.service.generalstore";
        public const string StableServicePointId = "ccs.survival.settlement.service.stable";
        public const string GunsmithServicePointId = "ccs.survival.settlement.service.gunsmith";
        public const string BlacksmithServicePointId = "ccs.survival.settlement.service.blacksmith";
        public const string BankServicePointId = "ccs.survival.settlement.service.bank";
        public const string LandOfficeServicePointId = "ccs.survival.settlement.service.landoffice";
        public const string TestTradingPostObjectName = "CCS_TestTradingPost";
    }
}
