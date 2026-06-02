// =============================================================================
// SCRIPT: CCS_VendorTransactionResultType
// CATEGORY: Modules / Economy / Runtime / Vendors
// PURPOSE: Result codes for vendor buy/sell transactions.
// PLACEMENT: Returned by CCS_VendorService transaction methods.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// =============================================================================

namespace CCS.Modules.Economy
{
    public enum CCS_VendorTransactionResultType
    {
        Success = 0,
        InsufficientFunds = 1,
        InvalidCurrency = 2,
        InvalidAmount = 3,
        InvalidItem = 4,
        VendorNotFound = 5,
        InventoryFull = 6,
        NotEnoughStock = 7,
        CannotBuy = 8,
        CannotSell = 9,
        NotEnoughItems = 10,
        UnknownFailure = 11
    }
}
