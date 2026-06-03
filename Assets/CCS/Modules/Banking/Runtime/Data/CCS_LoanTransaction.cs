// =============================================================================
// SCRIPT: CCS_LoanTransaction
// CATEGORY: Modules / Banking / Runtime / Data
// PURPOSE: Loan transaction history placeholder for borrow and repay operations.
// PLACEMENT: Recorded by CCS_BankingService.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 2.6.0 loans and debt foundation.
// =============================================================================

namespace CCS.Modules.Banking
{
    public sealed class CCS_LoanTransaction
    {
        public CCS_LoanTransaction(
            string loanId,
            string ownerId,
            string currencyId,
            int amount,
            int balanceAfter,
            string transactionKind,
            string reason,
            string timestampUtc,
            string summaryPlaceholder)
        {
            LoanId = loanId ?? string.Empty;
            OwnerId = ownerId ?? string.Empty;
            CurrencyId = currencyId ?? string.Empty;
            Amount = amount;
            BalanceAfter = balanceAfter;
            TransactionKind = transactionKind ?? string.Empty;
            Reason = reason ?? string.Empty;
            TimestampUtc = timestampUtc ?? string.Empty;
            SummaryPlaceholder = summaryPlaceholder ?? string.Empty;
        }

        public string LoanId { get; }

        public string OwnerId { get; }

        public string CurrencyId { get; }

        public int Amount { get; }

        public int BalanceAfter { get; }

        public string TransactionKind { get; }

        public string Reason { get; }

        public string TimestampUtc { get; }

        public string SummaryPlaceholder { get; }
    }
}
