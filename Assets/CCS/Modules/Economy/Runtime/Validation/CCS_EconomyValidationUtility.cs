using CCS.Modules.Inventory;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_EconomyValidationUtility
// CATEGORY: Modules / Economy / Runtime / Validation
// PURPOSE: Shared validation helpers for economy profiles and definitions.
// PLACEMENT: Used by services and editor validators.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// =============================================================================

namespace CCS.Modules.Economy
{
    public static class CCS_EconomyValidationUtility
    {
        public static CCS_SurvivalValidationResult ValidateProfile(CCS_EconomyProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail("Economy profile is null.");
            }

            CCS_CurrencyDefinition[] currencies = profile.CurrencyDefinitions;
            if (currencies == null || currencies.Length == 0)
            {
                return CCS_SurvivalValidationResult.Fail("Economy profile has no currency definitions.");
            }

            for (int index = 0; index < currencies.Length; index++)
            {
                CCS_SurvivalValidationResult currencyResult = ValidateCurrencyDefinition(currencies[index]);
                if (!currencyResult.IsSuccess)
                {
                    return currencyResult;
                }
            }

            if (profile.VendorProfile == null)
            {
                return CCS_SurvivalValidationResult.Fail("Economy profile is missing vendor profile.");
            }

            CCS_VendorDefinition[] vendors = profile.VendorProfile.VendorDefinitions;
            for (int index = 0; index < vendors.Length; index++)
            {
                CCS_SurvivalValidationResult vendorResult = ValidateVendorDefinition(vendors[index]);
                if (!vendorResult.IsSuccess)
                {
                    return vendorResult;
                }
            }

            return CCS_SurvivalValidationResult.Pass("Economy profile validated.");
        }

        public static CCS_SurvivalValidationResult ValidateCurrencyDefinition(CCS_CurrencyDefinition definition)
        {
            if (definition == null)
            {
                return CCS_SurvivalValidationResult.Fail("Currency definition is null.");
            }

            if (string.IsNullOrWhiteSpace(definition.CurrencyId))
            {
                return CCS_SurvivalValidationResult.Fail("Currency definition is missing currencyId.");
            }

            return CCS_SurvivalValidationResult.Pass($"Currency '{definition.CurrencyId}' validated.");
        }

        public static CCS_SurvivalValidationResult ValidateVendorDefinition(CCS_VendorDefinition definition)
        {
            if (definition == null)
            {
                return CCS_SurvivalValidationResult.Fail("Vendor definition is null.");
            }

            if (string.IsNullOrWhiteSpace(definition.VendorId))
            {
                return CCS_SurvivalValidationResult.Fail("Vendor definition is missing vendorId.");
            }

            if (definition.CurrencyDefinition == null)
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Vendor '{definition.VendorId}' is missing currency definition.");
            }

            CCS_VendorItemEntry[] entries = definition.VendorInventory?.Items;
            if (entries == null || entries.Length == 0)
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Vendor '{definition.VendorId}' has no catalog entries.");
            }

            for (int index = 0; index < entries.Length; index++)
            {
                CCS_VendorItemEntry entry = entries[index];
                if (entry?.ItemDefinition == null)
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Vendor '{definition.VendorId}' has null item at index {index}.");
                }

                if (entry.BuyPriceOverride < -1 || entry.SellPriceOverride < -1)
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Vendor '{definition.VendorId}' has invalid price override at index {index}.");
                }

                if (!CCS_EconomyValidationUtility.HasValidEconomyValues(entry.ItemDefinition)
                    && !entry.HasBuyPriceOverride
                    && !entry.HasSellPriceOverride)
                {
                    return CCS_SurvivalValidationResult.Pass(
                        $"Vendor entry '{entry.ItemDefinition.ItemId}' uses runtime overrides only.");
                }
            }

            return CCS_SurvivalValidationResult.Pass($"Vendor '{definition.VendorId}' validated.");
        }

        public static void ValidateItemEconomyValues(CCS_ItemDefinition itemDefinition)
        {
            if (itemDefinition == null)
            {
                return;
            }

            if (itemDefinition.BuyValue < 0 || itemDefinition.SellValue < 0)
            {
                return;
            }
        }

        public static bool HasValidEconomyValues(CCS_ItemDefinition itemDefinition)
        {
            return itemDefinition != null
                && itemDefinition.BuyValue >= 0
                && itemDefinition.SellValue >= 0;
        }
    }
}
