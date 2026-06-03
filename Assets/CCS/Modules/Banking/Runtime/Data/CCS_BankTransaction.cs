using System;

// =============================================================================
// SCRIPT: CCS_BankTransaction
// CATEGORY: Modules / Banking / Runtime / Data
// PURPOSE: Placeholder transaction record for bank deposit and withdraw history.
// PLACEMENT: Stored in CCS_BankingService transaction history ring buffer.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 2.4.0 stored-currency foundation.
// =============================================================================

namespace CCS.Modules.Banking
{
    [Serializable]
    public sealed class CCS_BankTransaction
    {
        public string accountId = string.Empty;
        public string ownerId = string.Empty;
        public string currencyId = string.Empty;
        public int deltaAmount;
        public int balanceAfter;
        public string transactionKind = string.Empty;
        public string reason = string.Empty;
        public string timestampUtc = string.Empty;
        public string summaryPlaceholder = string.Empty;

        public CCS_BankTransaction()
        {
        }

        public CCS_BankTransaction(
            string accountId,
            string ownerId,
            string currencyId,
            int deltaAmount,
            int balanceAfter,
            string transactionKind,
            string reason,
            string timestampUtc,
            string summaryPlaceholder)
        {
            this.accountId = accountId ?? string.Empty;
            this.ownerId = ownerId ?? string.Empty;
            this.currencyId = currencyId ?? string.Empty;
            this.deltaAmount = deltaAmount;
            this.balanceAfter = balanceAfter;
            this.transactionKind = transactionKind ?? string.Empty;
            this.reason = reason ?? string.Empty;
            this.timestampUtc = timestampUtc ?? string.Empty;
            this.summaryPlaceholder = summaryPlaceholder ?? string.Empty;
        }
    }
}
