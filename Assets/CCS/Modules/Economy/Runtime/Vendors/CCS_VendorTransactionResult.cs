using CCS.Modules.Inventory;

// =============================================================================
// SCRIPT: CCS_VendorTransactionResult
// CATEGORY: Modules / Economy / Runtime / Vendors
// PURPOSE: Result payload for vendor buy/sell operations.
// PLACEMENT: Returned by CCS_VendorService and raised on completion events.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Milestone 2.8.0 adds base/final unit price and reputation modifier fields.
// =============================================================================

namespace CCS.Modules.Economy
{
    public sealed class CCS_VendorTransactionResult
    {
        public CCS_VendorTransactionResult(
            CCS_VendorTransactionResultType resultType,
            string vendorId,
            CCS_ItemDefinition itemDefinition,
            int quantity,
            int currencyAmount,
            int currencyBalanceAfter,
            bool wasSell,
            string message,
            int baseUnitPrice = 0,
            int finalUnitPrice = 0,
            float reputationPriceModifier = 1f,
            string settlementId = "")
        {
            ResultType = resultType;
            VendorId = vendorId ?? string.Empty;
            ItemDefinition = itemDefinition;
            Quantity = quantity;
            CurrencyAmount = currencyAmount;
            CurrencyBalanceAfter = currencyBalanceAfter;
            WasSell = wasSell;
            Message = message ?? string.Empty;
            CurrencyDelta = wasSell ? currencyAmount : -currencyAmount;
            BaseUnitPrice = baseUnitPrice;
            FinalUnitPrice = finalUnitPrice;
            ReputationPriceModifier = reputationPriceModifier;
            SettlementId = settlementId ?? string.Empty;
        }

        public CCS_VendorTransactionResultType ResultType { get; }

        public string VendorId { get; }

        public CCS_ItemDefinition ItemDefinition { get; }

        public int Quantity { get; }

        public int CurrencyAmount { get; }

        /// <summary>Signed wallet change: positive when selling, negative when buying.</summary>
        public int CurrencyDelta { get; }

        public int CurrencyBalanceAfter { get; }

        public bool WasSell { get; }

        public string Message { get; }

        public int BaseUnitPrice { get; }

        public int FinalUnitPrice { get; }

        public float ReputationPriceModifier { get; }

        public string SettlementId { get; }

        public bool IsSuccess => ResultType == CCS_VendorTransactionResultType.Success;

        public string ItemId => ItemDefinition != null ? ItemDefinition.ItemId : string.Empty;
    }
}
