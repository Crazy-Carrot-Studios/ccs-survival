// =============================================================================
// SCRIPT: CCS_EconomyEvents
// CATEGORY: Modules / Economy / Runtime / Events
// PURPOSE: Event contracts for currency and vendor transactions.
// PLACEMENT: Raised by CCS_CurrencyService and CCS_VendorService.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// =============================================================================

namespace CCS.Modules.Economy
{
    public delegate void CurrencyBalanceChangedHandler(
        string currencyId,
        int previousBalance,
        int newBalance,
        string reason);

    public delegate void VendorTransactionCompletedHandler(CCS_VendorTransactionResult result);
}
