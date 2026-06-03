// =============================================================================
// SCRIPT: CCS_BankTransactionResult
// CATEGORY: Modules / Banking / Runtime / Data
// PURPOSE: Result payload for bank account open, deposit, and withdraw operations.
// PLACEMENT: Returned by CCS_BankingService.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 2.4.0 stored-currency foundation.
// =============================================================================

namespace CCS.Modules.Banking
{
    public sealed class CCS_BankTransactionResult
    {
        public CCS_BankTransactionResult(
            CCS_BankTransactionResultType resultType,
            string accountId,
            string ownerId,
            string currencyId,
            int amount,
            int walletBalanceAfter,
            int bankBalanceAfter,
            string message)
        {
            ResultType = resultType;
            AccountId = accountId ?? string.Empty;
            OwnerId = ownerId ?? string.Empty;
            CurrencyId = currencyId ?? string.Empty;
            Amount = amount;
            WalletBalanceAfter = walletBalanceAfter;
            BankBalanceAfter = bankBalanceAfter;
            Message = message ?? string.Empty;
        }

        public CCS_BankTransactionResultType ResultType { get; }

        public string AccountId { get; }

        public string OwnerId { get; }

        public string CurrencyId { get; }

        public int Amount { get; }

        public int WalletBalanceAfter { get; }

        public int BankBalanceAfter { get; }

        public string Message { get; }

        public bool IsSuccess => ResultType == CCS_BankTransactionResultType.Success;

        public static CCS_BankTransactionResult Success(
            string accountId,
            string ownerId,
            string currencyId,
            int amount,
            int walletBalanceAfter,
            int bankBalanceAfter,
            string message)
        {
            return new CCS_BankTransactionResult(
                CCS_BankTransactionResultType.Success,
                accountId,
                ownerId,
                currencyId,
                amount,
                walletBalanceAfter,
                bankBalanceAfter,
                message);
        }

        public static CCS_BankTransactionResult Failure(
            CCS_BankTransactionResultType resultType,
            string accountId,
            string ownerId,
            string currencyId,
            string message)
        {
            return new CCS_BankTransactionResult(
                resultType,
                accountId,
                ownerId,
                currencyId,
                0,
                0,
                0,
                message);
        }
    }

    public enum CCS_BankTransactionResultType
    {
        Success = 0,
        InvalidAmount = 1,
        InvalidAccount = 2,
        InsufficientWalletFunds = 3,
        InsufficientBankFunds = 4,
        AccountClosed = 5,
        ServiceNotReady = 6,
        UnknownFailure = 7
    }
}
