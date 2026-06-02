using CCS.Modules.Inventory;

// =============================================================================
// SCRIPT: CCS_VendorTransactionRequest
// CATEGORY: Modules / Economy / Runtime / Vendors
// PURPOSE: Request payload for vendor buy/sell operations.
// PLACEMENT: Passed to CCS_VendorService.TryBuy / TrySell.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// =============================================================================

namespace CCS.Modules.Economy
{
    public sealed class CCS_VendorTransactionRequest
    {
        public CCS_VendorTransactionRequest(
            string vendorId,
            CCS_ItemDefinition itemDefinition,
            int quantity,
            bool isSellTransaction)
        {
            VendorId = vendorId ?? string.Empty;
            ItemDefinition = itemDefinition;
            Quantity = quantity < 1 ? 1 : quantity;
            IsSellTransaction = isSellTransaction;
        }

        public string VendorId { get; }

        public CCS_ItemDefinition ItemDefinition { get; }

        public int Quantity { get; }

        public bool IsSellTransaction { get; }
    }
}
