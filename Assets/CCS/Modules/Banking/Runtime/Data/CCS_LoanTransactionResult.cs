// =============================================================================
// SCRIPT: CCS_LoanTransactionResult
// CATEGORY: Modules / Banking / Runtime / Data
// PURPOSE: Result payload for loan borrow and repay operations.
// PLACEMENT: Returned by CCS_BankingService.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 2.6.0 loans and debt foundation.
// =============================================================================

namespace CCS.Modules.Banking
{
    public sealed class CCS_LoanTransactionResult
    {
        public CCS_LoanTransactionResult(
            CCS_LoanTransactionResultType resultType,
            string loanId,
            string ownerId,
            string currencyId,
            int amount,
            int walletBalanceAfter,
            int bankBalanceAfter,
            int loanBalanceAfter,
            CCS_LoanState loanStateAfter,
            string message)
        {
            ResultType = resultType;
            LoanId = loanId ?? string.Empty;
            OwnerId = ownerId ?? string.Empty;
            CurrencyId = currencyId ?? string.Empty;
            Amount = amount;
            WalletBalanceAfter = walletBalanceAfter;
            BankBalanceAfter = bankBalanceAfter;
            LoanBalanceAfter = loanBalanceAfter;
            LoanStateAfter = loanStateAfter;
            Message = message ?? string.Empty;
        }

        public CCS_LoanTransactionResultType ResultType { get; }

        public string LoanId { get; }

        public string OwnerId { get; }

        public string CurrencyId { get; }

        public int Amount { get; }

        public int WalletBalanceAfter { get; }

        public int BankBalanceAfter { get; }

        public int LoanBalanceAfter { get; }

        public CCS_LoanState LoanStateAfter { get; }

        public string Message { get; }

        public bool IsSuccess => ResultType == CCS_LoanTransactionResultType.Success;

        public static CCS_LoanTransactionResult Success(
            string loanId,
            string ownerId,
            string currencyId,
            int amount,
            int walletBalanceAfter,
            int bankBalanceAfter,
            int loanBalanceAfter,
            CCS_LoanState loanStateAfter,
            string message)
        {
            return new CCS_LoanTransactionResult(
                CCS_LoanTransactionResultType.Success,
                loanId,
                ownerId,
                currencyId,
                amount,
                walletBalanceAfter,
                bankBalanceAfter,
                loanBalanceAfter,
                loanStateAfter,
                message);
        }

        public static CCS_LoanTransactionResult Failure(
            CCS_LoanTransactionResultType resultType,
            string loanId,
            string ownerId,
            string currencyId,
            string message)
        {
            return new CCS_LoanTransactionResult(
                resultType,
                loanId,
                ownerId,
                currencyId,
                0,
                0,
                0,
                0,
                CCS_LoanState.None,
                message);
        }
    }

    public enum CCS_LoanTransactionResultType
    {
        Success = 0,
        InvalidLoan = 1,
        InvalidAmount = 2,
        MaxLoansReached = 3,
        LoanDisabled = 4,
        InsufficientFunds = 5,
        ServiceNotReady = 6,
        UnknownFailure = 7
    }
}
