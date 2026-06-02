// =============================================================================
// SCRIPT: CCS_CurrencyTransactionResult
// CATEGORY: Modules / Economy / Runtime / Currency
// PURPOSE: Result payload for currency wallet operations.
// PLACEMENT: Returned by CCS_CurrencyService.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// =============================================================================

namespace CCS.Modules.Economy
{
    public sealed class CCS_CurrencyTransactionResult
    {
        public CCS_CurrencyTransactionResult(
            CCS_CurrencyTransactionResultType resultType,
            string currencyId,
            int amount,
            int balanceAfter,
            string message)
        {
            ResultType = resultType;
            CurrencyId = currencyId ?? string.Empty;
            Amount = amount;
            BalanceAfter = balanceAfter;
            Message = message ?? string.Empty;
        }

        public CCS_CurrencyTransactionResultType ResultType { get; }

        public string CurrencyId { get; }

        public int Amount { get; }

        public int BalanceAfter { get; }

        public string Message { get; }

        public bool IsSuccess => ResultType == CCS_CurrencyTransactionResultType.Success;

        public static CCS_CurrencyTransactionResult Success(
            string currencyId,
            int amount,
            int balanceAfter,
            string message)
        {
            return new CCS_CurrencyTransactionResult(
                CCS_CurrencyTransactionResultType.Success,
                currencyId,
                amount,
                balanceAfter,
                message);
        }

        public static CCS_CurrencyTransactionResult Failure(
            CCS_CurrencyTransactionResultType resultType,
            string currencyId,
            string message)
        {
            return new CCS_CurrencyTransactionResult(resultType, currencyId, 0, 0, message);
        }
    }
}
