using System;

// =============================================================================
// SCRIPT: CCS_RegionEconomyUtility
// CATEGORY: Modules / Regions / Runtime / Validation
// PURPOSE: Shared regional economy, contract category, and prosperity helpers.
// PLACEMENT: Used by contracts, world simulation, bootstrap, and validators.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.1.0 regional specialization foundation.
// =============================================================================

namespace CCS.Modules.Regions
{
    public static class CCS_RegionEconomyUtility
    {
        public const string CornItemId = "ccs.survival.farming.item.corn";
        public const string WheatItemId = "ccs.survival.farming.item.wheat";
        public const string BeanItemId = "ccs.survival.farming.item.beans";
        public const string PotatoItemId = "ccs.survival.farming.item.potatoes";
        public const string EggItemId = "ccs.survival.item.ranch.egg";
        public const string MilkItemId = "ccs.survival.item.ranch.milk";
        public const string IronOreItemId = "ccs.survival.item.resource.ironore";
        public const string CoalItemId = "ccs.survival.item.resource.coal";
        public const string RefinedIronItemId = "ccs.survival.item.resource.refinediron";
        public const string LumberItemId = "ccs.survival.item.resource.lumber";
        public const string PolesItemId = "ccs.survival.item.resource.poles";
        public const string CharcoalItemId = "ccs.survival.item.progression.charcoal";

        public static CCS_RegionSpecializationType ResolveDominantIndustry(
            CCS_RegionSpecializationType specializationType,
            float foodPotential,
            float wildlifePotential,
            float miningPotential,
            float industryPotential)
        {
            if (specializationType != CCS_RegionSpecializationType.Unknown)
            {
                return specializationType;
            }

            float timberScore = industryPotential + foodPotential * 0.25f;
            float agricultureScore = foodPotential + wildlifePotential * 0.25f;
            float miningScore = miningPotential + industryPotential * 0.25f;
            float ranchingScore = wildlifePotential + foodPotential * 0.35f;
            float mixedScore = (foodPotential + wildlifePotential + miningPotential + industryPotential) * 0.25f;

            CCS_RegionSpecializationType dominant = CCS_RegionSpecializationType.FrontierMixed;
            float bestScore = mixedScore;
            if (agricultureScore > bestScore)
            {
                bestScore = agricultureScore;
                dominant = CCS_RegionSpecializationType.Agriculture;
            }

            if (ranchingScore > bestScore)
            {
                bestScore = ranchingScore;
                dominant = CCS_RegionSpecializationType.Ranching;
            }

            if (miningScore > bestScore)
            {
                bestScore = miningScore;
                dominant = CCS_RegionSpecializationType.Mining;
            }

            if (timberScore > bestScore)
            {
                dominant = CCS_RegionSpecializationType.Timber;
            }

            return dominant;
        }

        public static void ResolveRegionalSupplyStrengths(
            CCS_RegionSpecializationType specializationType,
            float foodPotential,
            float wildlifePotential,
            float miningPotential,
            float industryPotential,
            out float foodStrength,
            out float industrialStrength,
            out float buildingStrength,
            out float tradeStrength)
        {
            foodStrength = MathfClamp01(foodPotential);
            industrialStrength = MathfClamp01(miningPotential + industryPotential * 0.5f);
            buildingStrength = MathfClamp01(industryPotential + foodPotential * 0.15f);
            tradeStrength = MathfClamp01((foodPotential + miningPotential + industryPotential) / 3f);

            switch (specializationType)
            {
                case CCS_RegionSpecializationType.Agriculture:
                    foodStrength = MathfClamp01(foodStrength + 0.25f);
                    break;
                case CCS_RegionSpecializationType.Ranching:
                    foodStrength = MathfClamp01(foodStrength + 0.15f);
                    tradeStrength = MathfClamp01(tradeStrength + 0.1f);
                    break;
                case CCS_RegionSpecializationType.Mining:
                    industrialStrength = MathfClamp01(industrialStrength + 0.3f);
                    break;
                case CCS_RegionSpecializationType.Timber:
                    buildingStrength = MathfClamp01(buildingStrength + 0.3f);
                    break;
                case CCS_RegionSpecializationType.FrontierMixed:
                    tradeStrength = MathfClamp01(tradeStrength + 0.2f);
                    break;
            }
        }

        public static float ApplyProsperityModifier(float baseProsperity, float prosperityModifier)
        {
            if (baseProsperity <= 0f || prosperityModifier <= 0f)
            {
                return baseProsperity;
            }

            return UnityEngine.Mathf.Clamp(baseProsperity * prosperityModifier, 0f, 100f);
        }

        public static bool TryResolveSpecializationForItem(string itemId, out CCS_RegionSpecializationType specialization)
        {
            specialization = CCS_RegionSpecializationType.Unknown;
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return false;
            }

            if (IsAgricultureItem(itemId))
            {
                specialization = CCS_RegionSpecializationType.Agriculture;
                return true;
            }

            if (IsRanchingItem(itemId))
            {
                specialization = CCS_RegionSpecializationType.Ranching;
                return true;
            }

            if (IsMiningItem(itemId))
            {
                specialization = CCS_RegionSpecializationType.Mining;
                return true;
            }

            if (IsTimberItem(itemId))
            {
                specialization = CCS_RegionSpecializationType.Timber;
                return true;
            }

            return false;
        }

        public static bool ContractMatchesRegionalSpecialization(
            CCS_RegionSpecializationType contractSpecialization,
            CCS_RegionSpecializationType regionSpecialization,
            CCS_RegionProductionModifier regionModifier)
        {
            if (contractSpecialization == CCS_RegionSpecializationType.Unknown)
            {
                return true;
            }

            if (contractSpecialization == regionSpecialization)
            {
                return true;
            }

            return regionModifier != null
                && regionModifier.PrefersContractCategory(contractSpecialization);
        }

        public static int CompareContractRegionalPreference(
            CCS_RegionSpecializationType left,
            CCS_RegionSpecializationType right,
            CCS_RegionSpecializationType preferredRegionSpecialization,
            CCS_RegionProductionModifier regionModifier)
        {
            bool leftPreferred = left != CCS_RegionSpecializationType.Unknown
                && (left == preferredRegionSpecialization
                    || (regionModifier != null && regionModifier.PrefersContractCategory(left)));
            bool rightPreferred = right != CCS_RegionSpecializationType.Unknown
                && (right == preferredRegionSpecialization
                    || (regionModifier != null && regionModifier.PrefersContractCategory(right)));

            if (leftPreferred == rightPreferred)
            {
                return 0;
            }

            return leftPreferred ? -1 : 1;
        }

        public static CCS_RegionSpecializationType[] GetDefaultPreferredCategories(
            CCS_RegionSpecializationType specializationType)
        {
            switch (specializationType)
            {
                case CCS_RegionSpecializationType.Agriculture:
                    return new[]
                    {
                        CCS_RegionSpecializationType.Agriculture,
                        CCS_RegionSpecializationType.FrontierMixed
                    };
                case CCS_RegionSpecializationType.Ranching:
                    return new[]
                    {
                        CCS_RegionSpecializationType.Ranching,
                        CCS_RegionSpecializationType.Agriculture
                    };
                case CCS_RegionSpecializationType.Mining:
                    return new[]
                    {
                        CCS_RegionSpecializationType.Mining,
                        CCS_RegionSpecializationType.FrontierMixed
                    };
                case CCS_RegionSpecializationType.Timber:
                    return new[]
                    {
                        CCS_RegionSpecializationType.Timber,
                        CCS_RegionSpecializationType.FrontierMixed
                    };
                case CCS_RegionSpecializationType.FrontierMixed:
                    return new[]
                    {
                        CCS_RegionSpecializationType.FrontierMixed,
                        CCS_RegionSpecializationType.Agriculture,
                        CCS_RegionSpecializationType.Ranching,
                        CCS_RegionSpecializationType.Mining,
                        CCS_RegionSpecializationType.Timber
                    };
                default:
                    return Array.Empty<CCS_RegionSpecializationType>();
            }
        }

        public static bool IsAgricultureItem(string itemId)
        {
            return string.Equals(itemId, CornItemId, StringComparison.OrdinalIgnoreCase)
                || string.Equals(itemId, WheatItemId, StringComparison.OrdinalIgnoreCase)
                || string.Equals(itemId, BeanItemId, StringComparison.OrdinalIgnoreCase)
                || string.Equals(itemId, PotatoItemId, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsRanchingItem(string itemId)
        {
            return string.Equals(itemId, EggItemId, StringComparison.OrdinalIgnoreCase)
                || string.Equals(itemId, MilkItemId, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsMiningItem(string itemId)
        {
            return string.Equals(itemId, IronOreItemId, StringComparison.OrdinalIgnoreCase)
                || string.Equals(itemId, CoalItemId, StringComparison.OrdinalIgnoreCase)
                || string.Equals(itemId, RefinedIronItemId, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsTimberItem(string itemId)
        {
            return string.Equals(itemId, LumberItemId, StringComparison.OrdinalIgnoreCase)
                || string.Equals(itemId, PolesItemId, StringComparison.OrdinalIgnoreCase)
                || string.Equals(itemId, CharcoalItemId, StringComparison.OrdinalIgnoreCase);
        }

        private static float MathfClamp01(float value)
        {
            return UnityEngine.Mathf.Clamp01(value);
        }
    }
}
