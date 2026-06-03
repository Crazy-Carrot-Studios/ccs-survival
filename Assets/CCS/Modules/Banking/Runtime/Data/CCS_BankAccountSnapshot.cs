using System;

// =============================================================================
// SCRIPT: CCS_BankAccountSnapshot
// CATEGORY: Modules / Banking / Runtime / Data
// PURPOSE: Serializable bank account state for save/load and validation.
// PLACEMENT: Used by CCS_BankingService and CCS_SaveBankingWorldData.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 2.4.0 stored-currency foundation.
// =============================================================================

namespace CCS.Modules.Banking
{
    [Serializable]
    public sealed class CCS_BankAccountSnapshot
    {
        public string accountId = string.Empty;
        public string ownerId = string.Empty;
        public string accountDefinitionId = string.Empty;
        public string currencyId = string.Empty;
        public int balance;
        public int accountState;
        public string transactionSummaryPlaceholder = string.Empty;
    }
}
