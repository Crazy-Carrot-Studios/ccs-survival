using System;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_SettlementGrowthUtility
// CATEGORY: Modules / Settlements / Runtime / Validation
// PURPOSE: Shared settlement growth evaluation, progress, and profile validation.
// PLACEMENT: Used by world simulation, validators, and bootstrap setup.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.6.0 — population thresholds gate settlement growth stages.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public static class CCS_SettlementGrowthUtility
    {
        public static readonly CCS_SettlementGrowthStage[] OrderedActiveStages =
        {
            CCS_SettlementGrowthStage.Outpost,
            CCS_SettlementGrowthStage.TradingPost
        };

        public static CCS_SurvivalValidationResult ValidateProfile(CCS_SettlementGrowthProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail("Settlement growth profile is null.");
            }

            CCS_SurvivalValidationResult baseValidation = CCS_SurvivalProfileValidationUtility.ValidateProfile(profile);
            if (!baseValidation.IsSuccess)
            {
                return baseValidation;
            }

            CCS_SettlementGrowthDefinition[] definitions = profile.GrowthDefinitions;
            if (definitions == null || definitions.Length == 0)
            {
                return CCS_SurvivalValidationResult.Fail("Settlement growth profile requires growth definitions.");
            }

            bool hasOutpost = false;
            bool hasTradingPost = false;
            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_SettlementGrowthDefinition definition = definitions[index];
                if (definition == null)
                {
                    return CCS_SurvivalValidationResult.Fail($"Growth definition at index {index} is null.");
                }

                if (string.IsNullOrWhiteSpace(definition.GrowthDefinitionId))
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Growth definition for stage {definition.GrowthStage} is missing growthDefinitionId.");
                }

                if (definition.GrowthStage == CCS_SettlementGrowthStage.Unknown)
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Growth definition '{definition.GrowthDefinitionId}' has unknown stage.");
                }

                if (definition.GrowthStage == CCS_SettlementGrowthStage.Outpost)
                {
                    hasOutpost = true;
                }

                if (definition.GrowthStage == CCS_SettlementGrowthStage.TradingPost)
                {
                    hasTradingPost = true;
                    if (definition.IsActive && definition.MinimumProsperity < 0f)
                    {
                        return CCS_SurvivalValidationResult.Fail(
                            "TradingPost growth definition has invalid prosperity threshold.");
                    }
                }

                if (!HasUniqueStage(definitions, definition.GrowthStage, index))
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Duplicate growth stage definition: {definition.GrowthStage}.");
                }
            }

            if (!hasOutpost || !hasTradingPost)
            {
                return CCS_SurvivalValidationResult.Fail(
                    "Settlement growth profile requires Outpost and TradingPost definitions.");
            }

            return CCS_SurvivalValidationResult.Pass("Settlement growth profile validated.");
        }

        public static CCS_SettlementGrowthStage ResolveHighestAchievedStage(
            CCS_SettlementGrowthProfile profile,
            CCS_SettlementGrowthStage startingStage,
            float prosperity,
            float foodSupplyPercent,
            float industrialSupplyPercent,
            int completedContractsCount,
            int totalPopulation,
            bool isRegionDiscovered,
            string settlementRegionId)
        {
            CCS_SettlementGrowthStage achievedStage = startingStage;
            if (profile == null)
            {
                return achievedStage;
            }

            CCS_SettlementGrowthDefinition[] definitions = profile.GrowthDefinitions;
            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_SettlementGrowthDefinition definition = definitions[index];
                if (definition == null
                    || !definition.IsActive
                    || definition.GrowthStage == CCS_SettlementGrowthStage.Unknown
                    || definition.GrowthStage <= achievedStage)
                {
                    continue;
                }

                if (!MeetsRequirements(
                        definition,
                        prosperity,
                        foodSupplyPercent,
                        industrialSupplyPercent,
                        completedContractsCount,
                        totalPopulation,
                        isRegionDiscovered,
                        settlementRegionId))
                {
                    continue;
                }

                if (definition.GrowthStage > achievedStage)
                {
                    achievedStage = definition.GrowthStage;
                }
            }

            return achievedStage;
        }

        public static CCS_SettlementGrowthStage ResolveNextActiveStage(CCS_SettlementGrowthStage currentStage)
        {
            for (int index = 0; index < OrderedActiveStages.Length; index++)
            {
                CCS_SettlementGrowthStage stage = OrderedActiveStages[index];
                if (stage > currentStage)
                {
                    return stage;
                }
            }

            return CCS_SettlementGrowthStage.Unknown;
        }

        public static float CalculateProgressToStage(
            CCS_SettlementGrowthDefinition targetDefinition,
            float prosperity,
            float foodSupplyPercent,
            float industrialSupplyPercent,
            int completedContractsCount,
            int totalPopulation,
            bool isRegionDiscovered,
            string settlementRegionId)
        {
            if (targetDefinition == null || !targetDefinition.IsActive)
            {
                return 0f;
            }

            float totalWeight = 0f;
            float earnedWeight = 0f;
            AddRequirementProgress(
                targetDefinition.MinimumProsperity,
                prosperity,
                ref totalWeight,
                ref earnedWeight);
            AddRequirementProgress(
                targetDefinition.MinimumFoodSupplyPercent,
                foodSupplyPercent,
                ref totalWeight,
                ref earnedWeight);
            AddRequirementProgress(
                targetDefinition.MinimumIndustrialSupplyPercent,
                industrialSupplyPercent,
                ref totalWeight,
                ref earnedWeight);
            AddRequirementProgress(
                targetDefinition.MinimumCompletedContracts,
                completedContractsCount,
                ref totalWeight,
                ref earnedWeight);
            AddRequirementProgress(
                targetDefinition.MinimumPopulation,
                totalPopulation,
                ref totalWeight,
                ref earnedWeight);

            if (targetDefinition.RequiresRegionDiscovered)
            {
                totalWeight += 1f;
                bool regionSatisfied = isRegionDiscovered
                    && (string.IsNullOrWhiteSpace(targetDefinition.RequiredRegionId)
                        || string.Equals(
                            targetDefinition.RequiredRegionId,
                            settlementRegionId,
                            StringComparison.OrdinalIgnoreCase));
                if (regionSatisfied)
                {
                    earnedWeight += 1f;
                }
            }

            if (totalWeight <= 0f)
            {
                return 100f;
            }

            return UnityEngine.Mathf.Clamp((earnedWeight / totalWeight) * 100f, 0f, 100f);
        }

        public static CCS_SettlementGrowthSnapshot BuildSnapshot(
            string settlementId,
            CCS_SettlementGrowthStage currentStage,
            CCS_SettlementGrowthStage previousStage,
            CCS_SettlementGrowthStage nextStage,
            float growthProgressPercent,
            int completedContractsCount,
            float prosperity,
            float foodSupplyPercent,
            float industrialSupplyPercent)
        {
            return new CCS_SettlementGrowthSnapshot
            {
                SettlementId = settlementId ?? string.Empty,
                CurrentGrowthStage = currentStage,
                PreviousGrowthStage = previousStage,
                NextGrowthStage = nextStage,
                GrowthProgressPercent = growthProgressPercent,
                CompletedContractsCount = completedContractsCount,
                Prosperity = prosperity,
                FoodSupplyHealthPercent = foodSupplyPercent,
                IndustrialSupplyHealthPercent = industrialSupplyPercent
            };
        }

        public static string GetDisplayName(CCS_SettlementGrowthStage stage)
        {
            switch (stage)
            {
                case CCS_SettlementGrowthStage.Outpost:
                    return "Outpost";
                case CCS_SettlementGrowthStage.TradingPost:
                    return "Trading Post";
                case CCS_SettlementGrowthStage.FrontierTown:
                    return "Frontier Town (Placeholder)";
                case CCS_SettlementGrowthStage.EstablishedTown:
                    return "Established Town (Placeholder)";
                default:
                    return "Unknown";
            }
        }

        private static bool MeetsRequirements(
            CCS_SettlementGrowthDefinition definition,
            float prosperity,
            float foodSupplyPercent,
            float industrialSupplyPercent,
            int completedContractsCount,
            int totalPopulation,
            bool isRegionDiscovered,
            string settlementRegionId)
        {
            if (definition == null || !definition.IsActive)
            {
                return false;
            }

            if (prosperity < definition.MinimumProsperity)
            {
                return false;
            }

            if (foodSupplyPercent < definition.MinimumFoodSupplyPercent)
            {
                return false;
            }

            if (industrialSupplyPercent < definition.MinimumIndustrialSupplyPercent)
            {
                return false;
            }

            if (completedContractsCount < definition.MinimumCompletedContracts)
            {
                return false;
            }

            if (totalPopulation < definition.MinimumPopulation)
            {
                return false;
            }

            if (definition.RequiresRegionDiscovered)
            {
                if (!isRegionDiscovered)
                {
                    return false;
                }

                if (!string.IsNullOrWhiteSpace(definition.RequiredRegionId)
                    && !string.Equals(
                        definition.RequiredRegionId,
                        settlementRegionId,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return true;
        }

        private static void AddRequirementProgress(
            float requiredValue,
            float actualValue,
            ref float totalWeight,
            ref float earnedWeight)
        {
            if (requiredValue <= 0f)
            {
                return;
            }

            totalWeight += 1f;
            earnedWeight += UnityEngine.Mathf.Clamp01(actualValue / requiredValue);
        }

        private static void AddRequirementProgress(
            int requiredValue,
            int actualValue,
            ref float totalWeight,
            ref float earnedWeight)
        {
            if (requiredValue <= 0)
            {
                return;
            }

            totalWeight += 1f;
            earnedWeight += UnityEngine.Mathf.Clamp01((float)actualValue / requiredValue);
        }

        private static bool HasUniqueStage(
            CCS_SettlementGrowthDefinition[] definitions,
            CCS_SettlementGrowthStage stage,
            int skipIndex)
        {
            for (int index = 0; index < definitions.Length; index++)
            {
                if (index == skipIndex)
                {
                    continue;
                }

                CCS_SettlementGrowthDefinition definition = definitions[index];
                if (definition != null && definition.GrowthStage == stage)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
