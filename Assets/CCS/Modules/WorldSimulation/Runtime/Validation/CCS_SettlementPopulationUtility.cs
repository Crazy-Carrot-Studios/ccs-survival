using System;
using CCS.Modules.Reputation;
using CCS.Modules.Regions;
using CCS.Modules.Settlements;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SettlementPopulationUtility
// CATEGORY: Modules / WorldSimulation / Runtime / Validation
// PURPOSE: Population growth, workforce distribution, stability, and profile validation.
// PLACEMENT: Used by CCS_WorldSimulationService, validators, and debug HUD.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.6.0 — integrates prosperity, supply, contracts, and reputation.
// =============================================================================

namespace CCS.Modules.WorldSimulation
{
    public static class CCS_SettlementPopulationUtility
    {
        public static CCS_SurvivalValidationResult ValidateProfile(CCS_SettlementPopulationProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail("Settlement population profile is null.");
            }

            CCS_SurvivalValidationResult baseValidation = CCS_SurvivalProfileValidationUtility.ValidateProfile(profile);
            if (!baseValidation.IsSuccess)
            {
                return baseValidation;
            }

            if (profile.BasePopulationCapacity < 0)
            {
                return CCS_SurvivalValidationResult.Fail("Base population capacity cannot be negative.");
            }

            return CCS_SurvivalValidationResult.Pass("Settlement population profile validated.");
        }

        public static void InitializePopulationState(
            CCS_SettlementSimulationState settlementState,
            CCS_SettlementPopulationProfile profile,
            CCS_RegionSpecializationType regionalSpecialization,
            int profileStartingPopulation,
            int profileStartingCapacity)
        {
            if (settlementState == null)
            {
                return;
            }

            int startingPopulation = profileStartingPopulation < 0 ? 0 : profileStartingPopulation;
            int startingCapacity = profileStartingCapacity > 0
                ? profileStartingCapacity
                : ResolvePopulationCapacity(settlementState.prosperity, profile);

            if (profile != null
                && profile.TryGetSettlementEntry(settlementState.settlementId, out CCS_SettlementPopulationSettlementEntry entry)
                && entry != null)
            {
                startingPopulation = entry.startingPopulation < 0 ? 0 : entry.startingPopulation;
                if (entry.startingCapacity > 0)
                {
                    startingCapacity = entry.startingCapacity;
                }
            }

            settlementState.population = startingPopulation;
            settlementState.populationCapacity = startingCapacity;
            settlementState.populationGrowthRate = 0f;
            settlementState.populationStability = 50f;
            settlementState.farmerCount = 0;
            settlementState.rancherCount = 0;
            settlementState.minerCount = 0;
            settlementState.lumberWorkerCount = 0;
            settlementState.merchantCount = 0;
            settlementState.laborerCount = 0;

            DistributeWorkforce(settlementState, regionalSpecialization);
            RefreshDerivedMetrics(settlementState, profile, CCS_ReputationTier.Neutral, 0f, 0f, false);
        }

        public static int ResolvePopulationCapacity(
            float prosperity,
            CCS_SettlementPopulationProfile profile)
        {
            if (profile == null)
            {
                return Mathf.Max(0, Mathf.RoundToInt(prosperity));
            }

            float capacity = profile.BasePopulationCapacity + prosperity * profile.CapacityPerProsperityPoint;
            return Mathf.Max(0, Mathf.RoundToInt(capacity));
        }

        public static float CalculateGrowthDelta(
            CCS_SettlementPopulationProfile profile,
            float prosperity,
            float foodSupplyPercent,
            bool contractCompletedThisEvaluation,
            CCS_ReputationTier reputationTier)
        {
            if (profile == null)
            {
                return 0f;
            }

            float delta = profile.BasePassiveGrowthRate;
            delta += prosperity * profile.ProsperityGrowthFactor;
            if (contractCompletedThisEvaluation)
            {
                delta += profile.ContractCompletionGrowthBonus;
            }

            if (foodSupplyPercent < profile.PoorSupplyThresholdPercent)
            {
                delta *= profile.PoorSupplyGrowthMultiplier;
            }

            delta *= ResolveReputationGrowthMultiplier(profile, reputationTier);
            return Mathf.Max(0f, delta);
        }

        public static float ResolveReputationGrowthMultiplier(
            CCS_SettlementPopulationProfile profile,
            CCS_ReputationTier reputationTier)
        {
            if (profile == null)
            {
                return 1f;
            }

            switch (reputationTier)
            {
                case CCS_ReputationTier.Hostile:
                    return profile.HostileGrowthMultiplier;
                case CCS_ReputationTier.Distrusted:
                    return profile.DistrustedGrowthMultiplier;
                case CCS_ReputationTier.Trusted:
                    return profile.TrustedGrowthMultiplier;
                case CCS_ReputationTier.Honored:
                    return profile.HonoredGrowthMultiplier;
                default:
                    return 1f;
            }
        }

        public static float CalculatePopulationStability(float foodSupplyPercent, float industrialSupplyPercent)
        {
            return Mathf.Clamp((foodSupplyPercent + industrialSupplyPercent) * 0.5f, 0f, 100f);
        }

        public static void ApplyPopulationGrowth(
            CCS_SettlementSimulationState settlementState,
            float growthDelta,
            CCS_SettlementPopulationProfile profile,
            CCS_RegionSpecializationType regionalSpecialization)
        {
            if (settlementState == null || growthDelta <= 0f)
            {
                return;
            }

            int capacity = ResolvePopulationCapacity(settlementState.prosperity, profile);
            if (settlementState.populationCapacity > 0)
            {
                capacity = Mathf.Max(capacity, settlementState.populationCapacity);
            }

            settlementState.populationCapacity = capacity;
            int nextPopulation = Mathf.Min(capacity, settlementState.population + Mathf.Max(1, Mathf.RoundToInt(growthDelta)));
            settlementState.population = Mathf.Max(0, nextPopulation);
            DistributeWorkforce(settlementState, regionalSpecialization);
            ClampPopulationNonNegative(settlementState);
        }

        public static void RefreshDerivedMetrics(
            CCS_SettlementSimulationState settlementState,
            CCS_SettlementPopulationProfile profile,
            CCS_ReputationTier reputationTier,
            float foodSupplyPercent,
            float industrialSupplyPercent,
            bool contractCompletedThisEvaluation)
        {
            if (settlementState == null)
            {
                return;
            }

            settlementState.populationCapacity = ResolvePopulationCapacity(settlementState.prosperity, profile);
            settlementState.populationGrowthRate = CalculateGrowthDelta(
                profile,
                settlementState.prosperity,
                foodSupplyPercent,
                contractCompletedThisEvaluation,
                reputationTier);
            settlementState.populationStability = CalculatePopulationStability(
                foodSupplyPercent,
                industrialSupplyPercent);
            ClampPopulationNonNegative(settlementState);
        }

        public static void DistributeWorkforce(
            CCS_SettlementSimulationState settlementState,
            CCS_RegionSpecializationType regionalSpecialization)
        {
            if (settlementState == null)
            {
                return;
            }

            int total = Mathf.Max(0, settlementState.population);
            if (total <= 0)
            {
                settlementState.farmerCount = 0;
                settlementState.rancherCount = 0;
                settlementState.minerCount = 0;
                settlementState.lumberWorkerCount = 0;
                settlementState.merchantCount = 0;
                settlementState.laborerCount = 0;
                return;
            }

            float farmerWeight;
            float rancherWeight;
            float minerWeight;
            float lumberWeight;
            float merchantWeight;
            float laborerWeight;
            ResolveWorkforceWeights(
                regionalSpecialization,
                out farmerWeight,
                out rancherWeight,
                out minerWeight,
                out lumberWeight,
                out merchantWeight,
                out laborerWeight);

            int farmers = Mathf.RoundToInt(total * farmerWeight);
            int ranchers = Mathf.RoundToInt(total * rancherWeight);
            int miners = Mathf.RoundToInt(total * minerWeight);
            int lumberWorkers = Mathf.RoundToInt(total * lumberWeight);
            int merchants = Mathf.RoundToInt(total * merchantWeight);
            int laborers = Mathf.Max(0, total - farmers - ranchers - miners - lumberWorkers - merchants);

            settlementState.farmerCount = farmers;
            settlementState.rancherCount = ranchers;
            settlementState.minerCount = miners;
            settlementState.lumberWorkerCount = lumberWorkers;
            settlementState.merchantCount = merchants;
            settlementState.laborerCount = laborers;
        }

        public static void ClampPopulationNonNegative(CCS_SettlementSimulationState settlementState)
        {
            if (settlementState == null)
            {
                return;
            }

            settlementState.population = Mathf.Max(0, settlementState.population);
            settlementState.populationCapacity = Mathf.Max(0, settlementState.populationCapacity);
            settlementState.populationGrowthRate = Mathf.Max(0f, settlementState.populationGrowthRate);
            settlementState.populationStability = Mathf.Clamp(settlementState.populationStability, 0f, 100f);
            settlementState.farmerCount = Mathf.Max(0, settlementState.farmerCount);
            settlementState.rancherCount = Mathf.Max(0, settlementState.rancherCount);
            settlementState.minerCount = Mathf.Max(0, settlementState.minerCount);
            settlementState.lumberWorkerCount = Mathf.Max(0, settlementState.lumberWorkerCount);
            settlementState.merchantCount = Mathf.Max(0, settlementState.merchantCount);
            settlementState.laborerCount = Mathf.Max(0, settlementState.laborerCount);
        }

        public static CCS_SettlementPopulationSnapshot BuildSnapshot(CCS_SettlementSimulationState settlementState)
        {
            if (settlementState == null)
            {
                return CCS_SettlementPopulationSnapshot.Empty;
            }

            ClampPopulationNonNegative(settlementState);
            return new CCS_SettlementPopulationSnapshot
            {
                SettlementId = settlementState.settlementId ?? string.Empty,
                TotalPopulation = settlementState.population,
                PopulationCapacity = settlementState.populationCapacity,
                PopulationGrowthRate = settlementState.populationGrowthRate,
                PopulationStability = settlementState.populationStability,
                FarmerCount = settlementState.farmerCount,
                RancherCount = settlementState.rancherCount,
                MinerCount = settlementState.minerCount,
                LumberWorkerCount = settlementState.lumberWorkerCount,
                MerchantCount = settlementState.merchantCount,
                LaborerCount = settlementState.laborerCount
            };
        }

        public static CCS_RegionSpecializationType ResolveRegionalSpecialization(
            CCS_SettlementSimulationState settlementState,
            CCS_RegionSimulationState regionState)
        {
            if (regionState != null
                && Enum.IsDefined(typeof(CCS_RegionSpecializationType), regionState.specializationType))
            {
                return (CCS_RegionSpecializationType)regionState.specializationType;
            }

            if (regionState != null
                && Enum.IsDefined(typeof(CCS_RegionSpecializationType), regionState.dominantIndustry))
            {
                return (CCS_RegionSpecializationType)regionState.dominantIndustry;
            }

            return CCS_RegionSpecializationType.FrontierMixed;
        }

        private static void ResolveWorkforceWeights(
            CCS_RegionSpecializationType specialization,
            out float farmerWeight,
            out float rancherWeight,
            out float minerWeight,
            out float lumberWeight,
            out float merchantWeight,
            out float laborerWeight)
        {
            switch (specialization)
            {
                case CCS_RegionSpecializationType.Agriculture:
                    farmerWeight = 0.4f;
                    rancherWeight = 0.15f;
                    minerWeight = 0.05f;
                    lumberWeight = 0.05f;
                    merchantWeight = 0.15f;
                    laborerWeight = 0.2f;
                    return;
                case CCS_RegionSpecializationType.Ranching:
                    farmerWeight = 0.15f;
                    rancherWeight = 0.4f;
                    minerWeight = 0.05f;
                    lumberWeight = 0.05f;
                    merchantWeight = 0.15f;
                    laborerWeight = 0.2f;
                    return;
                case CCS_RegionSpecializationType.Mining:
                    farmerWeight = 0.05f;
                    rancherWeight = 0.05f;
                    minerWeight = 0.45f;
                    lumberWeight = 0.05f;
                    merchantWeight = 0.15f;
                    laborerWeight = 0.25f;
                    return;
                case CCS_RegionSpecializationType.Timber:
                    farmerWeight = 0.05f;
                    rancherWeight = 0.05f;
                    minerWeight = 0.05f;
                    lumberWeight = 0.4f;
                    merchantWeight = 0.15f;
                    laborerWeight = 0.3f;
                    return;
                default:
                    farmerWeight = 0.15f;
                    rancherWeight = 0.1f;
                    minerWeight = 0.1f;
                    lumberWeight = 0.1f;
                    merchantWeight = 0.25f;
                    laborerWeight = 0.3f;
                    return;
            }
        }
    }
}
