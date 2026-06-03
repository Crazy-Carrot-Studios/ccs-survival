using System;

// =============================================================================
// SCRIPT: CCS_LoanSnapshot
// CATEGORY: Modules / Banking / Runtime / Data
// PURPOSE: Serializable loan state for save/load and validation.
// PLACEMENT: Used by CCS_BankingService and CCS_SaveBankingWorldData.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 2.6.0 loans and debt foundation.
// =============================================================================

namespace CCS.Modules.Banking
{
    [Serializable]
    public sealed class CCS_LoanSnapshot
    {
        public string loanId = string.Empty;
        public string ownerId = string.Empty;
        public string loanDefinitionId = string.Empty;
        public string currencyId = string.Empty;
        public int principalAmount;
        public int repaymentAmount;
        public int balance;
        public int loanState;
        public string transactionSummaryPlaceholder = string.Empty;
    }
}
