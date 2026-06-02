// =============================================================================
// SCRIPT: CCS_CurrencyTransactionResultType
// CATEGORY: Modules / Economy / Runtime / Currency
// PURPOSE: Result codes for currency wallet operations.
// PLACEMENT: Returned by CCS_CurrencyService add/remove/afford checks.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// =============================================================================

namespace CCS.Modules.Economy
{
    public enum CCS_CurrencyTransactionResultType
    {
        Success = 0,
        InsufficientFunds = 1,
        InvalidCurrency = 2,
        InvalidAmount = 3,
        UnknownFailure = 4
    }
}
