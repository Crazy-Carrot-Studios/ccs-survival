// =============================================================================
// SCRIPT: CCS_BankingContentIds
// CATEGORY: Modules / Banking / Runtime / Data
// PURPOSE: Stable content ids for banking bootstrap, validation, and playtest harness.
// PLACEMENT: Shared by bootstrap setup, validators, and debug HUD.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 2.4.0 frontier savings account foundation.
// =============================================================================

namespace CCS.Modules.Banking
{
    public static class CCS_BankingContentIds
    {
        public const string DefaultPlayerOwnerId = "ccs.survival.land.player";
        public const string TradeDollarsCurrencyId = "ccs.survival.currency.tradedollars";
        public const string FrontierSavingsAccountDefinitionId = "ccs.survival.banking.account.frontier.savings";
        public const string BankServicePointId = "ccs.survival.settlement.service.bank";
        public const string LandOfficeServicePointId = "ccs.survival.settlement.service.landoffice";
        public const string DefaultBankProfilePath = "Assets/CCS/Survival/Profiles/Banking/CCS_DefaultBankAccountProfile.asset";
        public const string FrontierSavingsAccountDefinitionPath =
            "Assets/CCS/Survival/Content/Banking/Accounts/CCS_BankAccount_FrontierSavings.asset";
    }
}
