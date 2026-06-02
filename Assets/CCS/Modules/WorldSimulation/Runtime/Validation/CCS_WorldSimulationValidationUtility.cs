using System;
using CCS.Modules.Economy;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_WorldSimulationValidationUtility
// CATEGORY: Modules / WorldSimulation / Runtime / Validation
// PURPOSE: Profile validation for world simulation module startup.
// PLACEMENT: Used by editor validators and runtime service initialization.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// NOTES: Milestone 2.0.0 frontier world simulation foundation.
// =============================================================================

namespace CCS.Modules.WorldSimulation
{
    public static class CCS_WorldSimulationValidationUtility
    {
        public static CCS_SurvivalValidationResult ValidateProfile(CCS_WorldSimulationProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail("World simulation profile is null.");
            }

            CCS_SurvivalValidationResult baseValidation = CCS_SurvivalProfileValidationUtility.ValidateProfile(profile);
            if (!baseValidation.IsSuccess)
            {
                return baseValidation;
            }

            CCS_WorldSimulationSettlementProfileEntry[] settlementEntries = profile.SettlementEntries;
            if (settlementEntries == null || settlementEntries.Length == 0)
            {
                return CCS_SurvivalValidationResult.Fail("World simulation profile requires at least one settlement entry.");
            }

            for (int index = 0; index < settlementEntries.Length; index++)
            {
                CCS_WorldSimulationSettlementProfileEntry entry = settlementEntries[index];
                if (entry == null || string.IsNullOrWhiteSpace(entry.settlementId))
                {
                    return CCS_SurvivalValidationResult.Fail($"Settlement simulation entry at index {index} is invalid.");
                }

                CCS_SurvivalValidationResult supplyValidation = ValidateUniqueSupplyCategories(entry.supplies, entry.settlementId);
                if (!supplyValidation.IsSuccess)
                {
                    return supplyValidation;
                }
            }

            CCS_WorldSimulationRegionProfileEntry[] regionEntries = profile.RegionEntries;
            if (regionEntries == null || regionEntries.Length == 0)
            {
                return CCS_SurvivalValidationResult.Fail("World simulation profile requires at least one region entry.");
            }

            for (int index = 0; index < regionEntries.Length; index++)
            {
                CCS_WorldSimulationRegionProfileEntry entry = regionEntries[index];
                if (entry == null || string.IsNullOrWhiteSpace(entry.regionId))
                {
                    return CCS_SurvivalValidationResult.Fail($"Region simulation entry at index {index} is invalid.");
                }
            }

            return CCS_SurvivalValidationResult.Pass("World simulation profile validated.");
        }

        public static CCS_SurvivalValidationResult ValidateSettlementReferences(
            CCS_WorldSimulationProfile profile,
            string[] knownSettlementIds)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail("World simulation profile is null.");
            }

            for (int index = 0; index < profile.SettlementEntries.Length; index++)
            {
                CCS_WorldSimulationSettlementProfileEntry entry = profile.SettlementEntries[index];
                if (entry == null || string.IsNullOrWhiteSpace(entry.settlementId))
                {
                    continue;
                }

                if (!ContainsId(knownSettlementIds, entry.settlementId))
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"World simulation references unknown settlement id '{entry.settlementId}'.");
                }
            }

            return CCS_SurvivalValidationResult.Pass("World simulation settlement references validated.");
        }

        public static CCS_SurvivalValidationResult ValidateRegionReferences(
            CCS_WorldSimulationProfile profile,
            string[] knownRegionIds)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail("World simulation profile is null.");
            }

            for (int index = 0; index < profile.RegionEntries.Length; index++)
            {
                CCS_WorldSimulationRegionProfileEntry entry = profile.RegionEntries[index];
                if (entry == null || string.IsNullOrWhiteSpace(entry.regionId))
                {
                    continue;
                }

                if (!ContainsId(knownRegionIds, entry.regionId))
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"World simulation references unknown region id '{entry.regionId}'.");
                }
            }

            return CCS_SurvivalValidationResult.Pass("World simulation region references validated.");
        }

        public static bool TryResolveSupplyImpact(
            CCS_VendorTransactionResult transaction,
            out CCS_SettlementSupplyType supplyType,
            out float amountDelta)
        {
            supplyType = CCS_SettlementSupplyType.TradeGoods;
            amountDelta = 0f;
            if (transaction == null || !transaction.IsSuccess)
            {
                return false;
            }

            string itemId = transaction.ItemId;
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return false;
            }

            float quantity = transaction.Quantity <= 0 ? 1f : transaction.Quantity;
            if (TryResolveFoodItem(itemId, out supplyType))
            {
                amountDelta = transaction.WasSell ? quantity : -quantity;
                return true;
            }

            if (TryResolveFuelOrBuildingItem(itemId, out supplyType))
            {
                amountDelta = transaction.WasSell ? quantity : -quantity;
                return true;
            }

            if (TryResolveIndustrialItem(itemId, out supplyType))
            {
                amountDelta = transaction.WasSell ? quantity : -quantity;
                return true;
            }

            if (TryResolveToolItem(itemId))
            {
                supplyType = transaction.WasSell ? CCS_SettlementSupplyType.Tools : CCS_SettlementSupplyType.TradeGoods;
                amountDelta = transaction.WasSell ? quantity : -quantity;
                return true;
            }

            supplyType = CCS_SettlementSupplyType.TradeGoods;
            amountDelta = transaction.WasSell ? quantity * 0.5f : -quantity * 0.5f;
            return true;
        }

        private static CCS_SurvivalValidationResult ValidateUniqueSupplyCategories(
            CCS_SettlementSupplyEntry[] supplies,
            string settlementId)
        {
            if (supplies == null || supplies.Length == 0)
            {
                return CCS_SurvivalValidationResult.Pass("Settlement supplies optional.");
            }

            bool[] seen = new bool[Enum.GetValues(typeof(CCS_SettlementSupplyType)).Length];
            for (int index = 0; index < supplies.Length; index++)
            {
                CCS_SettlementSupplyEntry supply = supplies[index];
                if (supply == null)
                {
                    continue;
                }

                int typeIndex = (int)supply.SupplyType;
                if (typeIndex < 0 || typeIndex >= seen.Length || seen[typeIndex])
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Duplicate supply category '{supply.SupplyType}' on settlement '{settlementId}'.");
                }

                seen[typeIndex] = true;
            }

            return CCS_SurvivalValidationResult.Pass("Settlement supply categories validated.");
        }

        private static bool ContainsId(string[] ids, string candidate)
        {
            if (ids == null || ids.Length == 0)
            {
                return false;
            }

            for (int index = 0; index < ids.Length; index++)
            {
                if (string.Equals(ids[index], candidate, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool TryResolveFoodItem(string itemId, out CCS_SettlementSupplyType supplyType)
        {
            supplyType = CCS_SettlementSupplyType.Food;
            return itemId.Contains("rawfish", StringComparison.OrdinalIgnoreCase)
                || itemId.Contains("cookedfish", StringComparison.OrdinalIgnoreCase)
                || itemId.Contains("driedfish", StringComparison.OrdinalIgnoreCase)
                || itemId.Contains("rawmeat", StringComparison.OrdinalIgnoreCase)
                || itemId.Contains("cookedmeat", StringComparison.OrdinalIgnoreCase)
                || itemId.Contains("jerky", StringComparison.OrdinalIgnoreCase);
        }

        private static bool TryResolveFuelOrBuildingItem(string itemId, out CCS_SettlementSupplyType supplyType)
        {
            if (itemId.Contains("charcoal", StringComparison.OrdinalIgnoreCase))
            {
                supplyType = CCS_SettlementSupplyType.Fuel;
                return true;
            }

            if (itemId.Contains("wood", StringComparison.OrdinalIgnoreCase))
            {
                supplyType = CCS_SettlementSupplyType.BuildingMaterials;
                return true;
            }

            if (itemId.Contains("lumber", StringComparison.OrdinalIgnoreCase))
            {
                supplyType = CCS_SettlementSupplyType.BuildingMaterials;
                return true;
            }

            supplyType = CCS_SettlementSupplyType.BuildingMaterials;
            return false;
        }

        private static bool TryResolveIndustrialItem(string itemId, out CCS_SettlementSupplyType supplyType)
        {
            supplyType = CCS_SettlementSupplyType.IndustrialMaterials;
            return itemId.Contains("ironore", StringComparison.OrdinalIgnoreCase)
                || itemId.Contains("refinediron", StringComparison.OrdinalIgnoreCase)
                || itemId.Contains("ironbar", StringComparison.OrdinalIgnoreCase)
                || itemId.Contains("nails", StringComparison.OrdinalIgnoreCase)
                || itemId.Contains("coal", StringComparison.OrdinalIgnoreCase)
                || itemId.Contains("stone", StringComparison.OrdinalIgnoreCase);
        }

        private static bool TryResolveToolItem(string itemId)
        {
            return itemId.Contains(".tool.", StringComparison.OrdinalIgnoreCase)
                || itemId.Contains("hatchet", StringComparison.OrdinalIgnoreCase)
                || itemId.Contains("pick", StringComparison.OrdinalIgnoreCase)
                || itemId.Contains("knife", StringComparison.OrdinalIgnoreCase);
        }
    }
}
