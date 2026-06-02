using System;
using CCS.Modules.Inventory;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_VendorItemEntry
// CATEGORY: Modules / Economy / Runtime / Vendors
// PURPOSE: Buy/sell catalog entry for a vendor listing.
// PLACEMENT: Embedded in CCS_VendorInventory on CCS_VendorDefinition.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// =============================================================================

namespace CCS.Modules.Economy
{
    [Serializable]
    public sealed class CCS_VendorItemEntry
    {
        [Tooltip("Item sold or purchased at this vendor.")]
        [SerializeField] private CCS_ItemDefinition itemDefinition;

        [Tooltip("Stock available from vendor (-1 = unlimited).")]
        [SerializeField] private int stockQuantity = -1;

        [Tooltip("Player may purchase this item from the vendor.")]
        [SerializeField] private bool allowBuy = true;

        [Tooltip("Player may sell this item to the vendor.")]
        [SerializeField] private bool allowSell = true;

        [Tooltip("Override buy price. Negative uses item buy value.")]
        [SerializeField] private int buyPriceOverride = -1;

        [Tooltip("Override sell price. Negative uses item sell value.")]
        [SerializeField] private int sellPriceOverride = -1;

        public CCS_ItemDefinition ItemDefinition => itemDefinition;

        public int StockQuantity => stockQuantity;

        public bool AllowBuy => allowBuy;

        public bool AllowSell => allowSell;

        public int BuyPriceOverride => buyPriceOverride;

        public int SellPriceOverride => sellPriceOverride;

        public bool HasBuyPriceOverride => buyPriceOverride >= 0;

        public bool HasSellPriceOverride => sellPriceOverride >= 0;
    }
}
