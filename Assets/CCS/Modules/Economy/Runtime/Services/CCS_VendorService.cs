using System.Collections.Generic;
using CCS.Modules.Inventory;
using CCS.Modules.Reputation;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_VendorService
// CATEGORY: Modules / Economy / Runtime / Services
// PURPOSE: Generic buy/sell vendor transactions using currency wallet and inventory.
// PLACEMENT: Registered by CCS_SurvivalGameplayServiceRegistration.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: No final vendor UI. Debug/playtest flows call TryBuy/TrySell directly.
// =============================================================================

namespace CCS.Modules.Economy
{
    public sealed class CCS_VendorService : CCS_ISurvivalService
    {
        private const string LogPrefix = "[CCS_VendorService]";

        #region Variables

        private readonly Dictionary<string, CCS_VendorDefinition> vendorLookup =
            new Dictionary<string, CCS_VendorDefinition>();

        private CCS_EconomyProfile activeProfile;
        private CCS_CurrencyService currencyService;
        private CCS_PlayerInventoryService inventoryService;
        private CCS_ReputationService reputationService;
        private CCS_VendorDefinition activeVendor;
        private string activeSettlementId = string.Empty;
        private bool isInitialized;

        #endregion

        #region Events

        public event VendorTransactionCompletedHandler VendorTransactionCompleted;

        #endregion

        #region Properties

        public bool IsInitialized => isInitialized;

        public CCS_EconomyProfile ActiveProfile => activeProfile;

        public CCS_VendorDefinition ActiveVendor => activeVendor;

        public string ActiveSettlementId => activeSettlementId ?? string.Empty;

        public bool HasActiveVendor => activeVendor != null;

        #endregion

        #region Public Methods

        public void Initialize()
        {
            if (isInitialized)
            {
                return;
            }

            isInitialized = true;
        }

        public void InitializeFromProfile(CCS_EconomyProfile profile)
        {
            if (profile == null)
            {
                isInitialized = false;
                return;
            }

            CCS_SurvivalValidationResult validation = CCS_EconomyValidationUtility.ValidateProfile(profile);
            if (!validation.IsSuccess)
            {
                Debug.LogWarning($"{LogPrefix} Profile validation warning: {validation.Message}");
            }

            vendorLookup.Clear();
            if (profile.VendorProfile != null)
            {
                CCS_VendorDefinition[] vendors = profile.VendorProfile.VendorDefinitions;
                for (int index = 0; index < vendors.Length; index++)
                {
                    RegisterVendorDefinition(vendors[index]);
                }
            }

            activeProfile = profile;
            isInitialized = true;
        }

        public void BindServices(CCS_CurrencyService currency, CCS_PlayerInventoryService inventory)
        {
            currencyService = currency;
            inventoryService = inventory;
        }

        public void BindReputationService(CCS_ReputationService reputation)
        {
            reputationService = reputation;
        }

        public void RegisterVendorDefinition(CCS_VendorDefinition definition)
        {
            if (definition == null || string.IsNullOrWhiteSpace(definition.VendorId))
            {
                return;
            }

            vendorLookup[definition.VendorId] = definition;
        }

        public bool TryGetVendor(string vendorId, out CCS_VendorDefinition definition)
        {
            definition = null;
            if (string.IsNullOrWhiteSpace(vendorId))
            {
                return false;
            }

            return vendorLookup.TryGetValue(vendorId, out definition);
        }

        public void SetActiveVendor(CCS_VendorDefinition vendorDefinition, string settlementId = "")
        {
            activeVendor = vendorDefinition;
            activeSettlementId = settlementId ?? string.Empty;
            if (activeVendor != null)
            {
                LogDebug(
                    $"Active vendor set: {activeVendor.DisplayName} ({activeVendor.VendorId})"
                    + (string.IsNullOrWhiteSpace(activeSettlementId)
                        ? "."
                        : $" at settlement {activeSettlementId}."));
            }
        }

        public void ClearActiveVendor()
        {
            activeVendor = null;
            activeSettlementId = string.Empty;
        }

        public CCS_VendorTransactionResult TryBuy(CCS_VendorTransactionRequest request)
        {
            return ProcessTransaction(request, isSell: false);
        }

        public CCS_VendorTransactionResult TrySell(CCS_VendorTransactionRequest request)
        {
            return ProcessTransaction(request, isSell: true);
        }

        public CCS_VendorTransactionResult TryBuyActiveVendorItem(CCS_ItemDefinition itemDefinition, int quantity)
        {
            if (activeVendor == null)
            {
                return Failure(
                    CCS_VendorTransactionResultType.VendorNotFound,
                    string.Empty,
                    itemDefinition,
                    quantity,
                    false,
                    "No active vendor.");
            }

            return TryBuy(
                new CCS_VendorTransactionRequest(activeVendor.VendorId, itemDefinition, quantity, false));
        }

        public CCS_VendorTransactionResult TrySellActiveVendorItem(CCS_ItemDefinition itemDefinition, int quantity)
        {
            if (activeVendor == null)
            {
                return Failure(
                    CCS_VendorTransactionResultType.VendorNotFound,
                    string.Empty,
                    itemDefinition,
                    quantity,
                    true,
                    "No active vendor.");
            }

            return TrySell(
                new CCS_VendorTransactionRequest(activeVendor.VendorId, itemDefinition, quantity, true));
        }

        public int ResolveBuyPrice(CCS_VendorDefinition vendor, CCS_VendorItemEntry entry)
        {
            int basePrice = ResolveBaseBuyPrice(vendor, entry);
            if (basePrice <= 0)
            {
                return 0;
            }

            float modifier = ResolveBuyPriceModifier();
            return CCS_ReputationPriceModifierUtility.ApplyModifier(basePrice, modifier);
        }

        public int ResolveSellPrice(CCS_VendorDefinition vendor, CCS_VendorItemEntry entry)
        {
            int basePrice = ResolveBaseSellPrice(vendor, entry);
            if (basePrice <= 0)
            {
                return 0;
            }

            float modifier = ResolveSellPriceModifier();
            return CCS_ReputationPriceModifierUtility.ApplyModifier(basePrice, modifier);
        }

        public int ResolveBaseBuyPrice(CCS_VendorDefinition vendor, CCS_VendorItemEntry entry)
        {
            if (entry == null)
            {
                return 0;
            }

            if (entry.HasBuyPriceOverride)
            {
                return entry.BuyPriceOverride;
            }

            return entry.ItemDefinition != null ? entry.ItemDefinition.BuyValue : 0;
        }

        public int ResolveBaseSellPrice(CCS_VendorDefinition vendor, CCS_VendorItemEntry entry)
        {
            if (entry == null)
            {
                return 0;
            }

            if (entry.HasSellPriceOverride)
            {
                return entry.SellPriceOverride;
            }

            return entry.ItemDefinition != null ? entry.ItemDefinition.SellValue : 0;
        }

        public float ResolveBuyPriceModifier()
        {
            return CCS_ReputationPriceModifierUtility.ResolveBuyPriceModifier(
                reputationService,
                activeSettlementId);
        }

        public float ResolveSellPriceModifier()
        {
            return CCS_ReputationPriceModifierUtility.ResolveSellPriceModifier(
                reputationService,
                activeSettlementId);
        }

        public bool TryFindCatalogEntry(
            CCS_VendorDefinition vendor,
            CCS_ItemDefinition itemDefinition,
            out CCS_VendorItemEntry entry)
        {
            entry = null;
            if (vendor == null || itemDefinition == null)
            {
                return false;
            }

            CCS_VendorItemEntry[] items = vendor.VendorInventory.Items;
            for (int index = 0; index < items.Length; index++)
            {
                CCS_VendorItemEntry candidate = items[index];
                if (candidate?.ItemDefinition == itemDefinition)
                {
                    entry = candidate;
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Private Methods

        private CCS_VendorTransactionResult ProcessTransaction(
            CCS_VendorTransactionRequest request,
            bool isSell)
        {
            if (!isInitialized
                || currencyService == null
                || !currencyService.IsInitialized
                || inventoryService == null
                || !inventoryService.IsInitialized)
            {
                return Failure(
                    CCS_VendorTransactionResultType.UnknownFailure,
                    request?.VendorId,
                    request?.ItemDefinition,
                    request?.Quantity ?? 0,
                    isSell,
                    "Economy services are not ready.");
            }

            if (request == null
                || string.IsNullOrWhiteSpace(request.VendorId)
                || request.ItemDefinition == null
                || request.Quantity <= 0)
            {
                return Failure(
                    CCS_VendorTransactionResultType.InvalidItem,
                    request?.VendorId,
                    request?.ItemDefinition,
                    request?.Quantity ?? 0,
                    isSell,
                    "Invalid vendor transaction request.");
            }

            if (!TryGetVendor(request.VendorId, out CCS_VendorDefinition vendor))
            {
                return Failure(
                    CCS_VendorTransactionResultType.VendorNotFound,
                    request.VendorId,
                    request.ItemDefinition,
                    request.Quantity,
                    isSell,
                    "Vendor not found.");
            }

            if (!TryFindCatalogEntry(vendor, request.ItemDefinition, out CCS_VendorItemEntry entry))
            {
                return Failure(
                    CCS_VendorTransactionResultType.InvalidItem,
                    request.VendorId,
                    request.ItemDefinition,
                    request.Quantity,
                    isSell,
                    "Item not in vendor catalog.");
            }

            CCS_CurrencyDefinition currency = vendor.CurrencyDefinition
                ?? activeProfile?.DefaultCurrencyDefinition;
            if (currency == null || string.IsNullOrWhiteSpace(currency.CurrencyId))
            {
                return Failure(
                    CCS_VendorTransactionResultType.InvalidCurrency,
                    request.VendorId,
                    request.ItemDefinition,
                    request.Quantity,
                    isSell,
                    "Vendor currency is invalid.");
            }

            if (isSell)
            {
                return ProcessSell(vendor, entry, currency.CurrencyId, request.Quantity);
            }

            return ProcessBuy(vendor, entry, currency.CurrencyId, request.Quantity);
        }

        private CCS_VendorTransactionResult ProcessBuy(
            CCS_VendorDefinition vendor,
            CCS_VendorItemEntry entry,
            string currencyId,
            int quantity)
        {
            if (!entry.AllowBuy)
            {
                return Failure(
                    CCS_VendorTransactionResultType.CannotBuy,
                    vendor.VendorId,
                    entry.ItemDefinition,
                    quantity,
                    false,
                    "Item cannot be purchased from this vendor.");
            }

            int baseUnitPrice = ResolveBaseBuyPrice(vendor, entry);
            if (baseUnitPrice <= 0)
            {
                return Failure(
                    CCS_VendorTransactionResultType.CannotBuy,
                    vendor.VendorId,
                    entry.ItemDefinition,
                    quantity,
                    false,
                    "Item has no buy price.");
            }

            float modifier = ResolveBuyPriceModifier();
            int unitPrice = CCS_ReputationPriceModifierUtility.ApplyModifier(baseUnitPrice, modifier);
            if (unitPrice <= 0)
            {
                return Failure(
                    CCS_VendorTransactionResultType.CannotBuy,
                    vendor.VendorId,
                    entry.ItemDefinition,
                    quantity,
                    false,
                    "Item has no buy price.");
            }

            int totalCost = unitPrice * quantity;
            if (!currencyService.CanAfford(currencyId, totalCost))
            {
                return Failure(
                    CCS_VendorTransactionResultType.InsufficientFunds,
                    vendor.VendorId,
                    entry.ItemDefinition,
                    quantity,
                    false,
                    "Insufficient funds.");
            }

            int added = inventoryService.AddItem(entry.ItemDefinition, quantity);
            if (added <= 0)
            {
                return Failure(
                    CCS_VendorTransactionResultType.InventoryFull,
                    vendor.VendorId,
                    entry.ItemDefinition,
                    quantity,
                    false,
                    "Inventory could not accept item.");
            }

            if (added < quantity)
            {
                inventoryService.RemoveItem(entry.ItemDefinition, added);
                return Failure(
                    CCS_VendorTransactionResultType.InventoryFull,
                    vendor.VendorId,
                    entry.ItemDefinition,
                    quantity,
                    false,
                    "Inventory could not accept full purchase quantity.");
            }

            CCS_CurrencyTransactionResult removeResult =
                currencyService.RemoveCurrency(currencyId, totalCost, $"Buy {entry.ItemDefinition.DisplayName}");
            if (!removeResult.IsSuccess)
            {
                inventoryService.RemoveItem(entry.ItemDefinition, added);
                return Failure(
                    CCS_VendorTransactionResultType.InsufficientFunds,
                    vendor.VendorId,
                    entry.ItemDefinition,
                    quantity,
                    false,
                    removeResult.Message);
            }

            CCS_VendorTransactionResult success = Success(
                vendor.VendorId,
                entry.ItemDefinition,
                added,
                totalCost,
                removeResult.BalanceAfter,
                false,
                $"Purchased {added}x {entry.ItemDefinition.DisplayName} for {totalCost} {currencyId}.",
                baseUnitPrice,
                unitPrice,
                modifier,
                activeSettlementId);
            RaiseCompleted(success);
            return success;
        }

        private CCS_VendorTransactionResult ProcessSell(
            CCS_VendorDefinition vendor,
            CCS_VendorItemEntry entry,
            string currencyId,
            int quantity)
        {
            if (!entry.AllowSell)
            {
                return Failure(
                    CCS_VendorTransactionResultType.CannotSell,
                    vendor.VendorId,
                    entry.ItemDefinition,
                    quantity,
                    true,
                    "Item cannot be sold to this vendor.");
            }

            int baseUnitPrice = ResolveBaseSellPrice(vendor, entry);
            if (baseUnitPrice <= 0)
            {
                return Failure(
                    CCS_VendorTransactionResultType.CannotSell,
                    vendor.VendorId,
                    entry.ItemDefinition,
                    quantity,
                    true,
                    "Item has no sell value.");
            }

            float modifier = ResolveSellPriceModifier();
            int unitPrice = CCS_ReputationPriceModifierUtility.ApplyModifier(baseUnitPrice, modifier);
            if (unitPrice <= 0)
            {
                return Failure(
                    CCS_VendorTransactionResultType.CannotSell,
                    vendor.VendorId,
                    entry.ItemDefinition,
                    quantity,
                    true,
                    "Item has no sell value.");
            }

            int owned = inventoryService.GetQuantity(entry.ItemDefinition);
            if (owned < quantity)
            {
                return Failure(
                    CCS_VendorTransactionResultType.NotEnoughItems,
                    vendor.VendorId,
                    entry.ItemDefinition,
                    quantity,
                    true,
                    "Not enough items to sell.");
            }

            int removed = inventoryService.RemoveItem(entry.ItemDefinition, quantity);
            if (removed <= 0)
            {
                return Failure(
                    CCS_VendorTransactionResultType.NotEnoughItems,
                    vendor.VendorId,
                    entry.ItemDefinition,
                    quantity,
                    true,
                    "Failed to remove items from inventory.");
            }

            int payout = unitPrice * removed;
            CCS_CurrencyTransactionResult addResult =
                currencyService.AddCurrency(currencyId, payout, $"Sell {entry.ItemDefinition.DisplayName}");
            if (!addResult.IsSuccess)
            {
                inventoryService.AddItem(entry.ItemDefinition, removed);
                return Failure(
                    CCS_VendorTransactionResultType.UnknownFailure,
                    vendor.VendorId,
                    entry.ItemDefinition,
                    quantity,
                    true,
                    addResult.Message);
            }

            CCS_VendorTransactionResult success = Success(
                vendor.VendorId,
                entry.ItemDefinition,
                removed,
                payout,
                addResult.BalanceAfter,
                true,
                $"Sold {removed}x {entry.ItemDefinition.DisplayName} for {payout} {currencyId}.",
                baseUnitPrice,
                unitPrice,
                modifier,
                activeSettlementId);
            RaiseCompleted(success);
            return success;
        }

        private CCS_VendorTransactionResult Success(
            string vendorId,
            CCS_ItemDefinition item,
            int quantity,
            int currencyAmount,
            int balanceAfter,
            bool wasSell,
            string message,
            int baseUnitPrice = 0,
            int finalUnitPrice = 0,
            float reputationPriceModifier = 1f,
            string settlementId = "")
        {
            LogDebug(message);
            return new CCS_VendorTransactionResult(
                CCS_VendorTransactionResultType.Success,
                vendorId,
                item,
                quantity,
                currencyAmount,
                balanceAfter,
                wasSell,
                message,
                baseUnitPrice,
                finalUnitPrice,
                reputationPriceModifier,
                settlementId);
        }

        private CCS_VendorTransactionResult Failure(
            CCS_VendorTransactionResultType resultType,
            string vendorId,
            CCS_ItemDefinition item,
            int quantity,
            bool wasSell,
            string message)
        {
            CCS_VendorTransactionResult result = new CCS_VendorTransactionResult(
                resultType,
                vendorId,
                item,
                quantity,
                0,
                currencyService != null ? currencyService.GetBalance(GetCurrencyIdForVendor(vendorId)) : 0,
                wasSell,
                message);
            RaiseCompleted(result);
            return result;
        }

        private string GetCurrencyIdForVendor(string vendorId)
        {
            if (TryGetVendor(vendorId, out CCS_VendorDefinition vendor)
                && vendor.CurrencyDefinition != null)
            {
                return vendor.CurrencyDefinition.CurrencyId;
            }

            return activeProfile?.DefaultCurrencyDefinition != null
                ? activeProfile.DefaultCurrencyDefinition.CurrencyId
                : string.Empty;
        }

        private void RaiseCompleted(CCS_VendorTransactionResult result)
        {
            CCS_VendorDebugHud.NotifyTransactionResult(result);
            VendorTransactionCompleted?.Invoke(result);
        }

        private void LogDebug(string message)
        {
            if (activeProfile != null && activeProfile.EnableDebugLogging)
            {
                Debug.Log($"{LogPrefix} {message}");
            }
        }

        #endregion
    }
}
