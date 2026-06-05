using System;
using System.Collections.Generic;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_PopulationPresenceValidationUtility
// CATEGORY: Modules / Settlements / Runtime / Validation
// PURPOSE: Population presence count resolution, snapshot building, profile validation.
// PLACEMENT: Used by population presence service, validators, and playtest harness.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 4.0.0 — derives actors from CCS_SettlementPopulationSnapshot.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public static class CCS_PopulationPresenceValidationUtility
    {
        public static CCS_SurvivalValidationResult ValidateProfile(CCS_PopulationPresenceProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail("Population presence profile is null.");
            }

            CCS_SurvivalValidationResult baseValidation = CCS_SurvivalProfileValidationUtility.ValidateProfile(profile);
            if (!baseValidation.IsSuccess)
            {
                return baseValidation;
            }

            CCS_PopulationPresenceDefinition[] definitions = profile.AnchorDefinitions;
            if (definitions.Length == 0)
            {
                return CCS_SurvivalValidationResult.Fail("Population presence profile has no anchor definitions.");
            }

            HashSet<string> anchorIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_PopulationPresenceDefinition definition = definitions[index];
                if (definition == null || string.IsNullOrWhiteSpace(definition.AnchorId))
                {
                    return CCS_SurvivalValidationResult.Fail("Population presence definition missing anchor id.");
                }

                if (definition.workforceCategory == CCS_SettlementPopulationCategory.Unknown)
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Population presence anchor '{definition.AnchorId}' has unknown workforce category.");
                }

                if (definition.maxVisibleActors < 1)
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Population presence anchor '{definition.AnchorId}' has invalid maxVisibleActors.");
                }

                if (definition.minimumPopulationCount < 0)
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Population presence anchor '{definition.AnchorId}' has invalid minimumPopulationCount.");
                }

                if (!anchorIds.Add(definition.AnchorId))
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Duplicate population presence anchor id '{definition.AnchorId}'.");
                }
            }

            return CCS_SurvivalValidationResult.Pass("Population presence profile validated.");
        }

        public static int ResolveWorkforceCount(
            CCS_SettlementPopulationSnapshot populationSnapshot,
            CCS_SettlementPopulationCategory category)
        {
            if (populationSnapshot == null || !populationSnapshot.IsValid)
            {
                return 0;
            }

            return category switch
            {
                CCS_SettlementPopulationCategory.Farmers => populationSnapshot.FarmerCount,
                CCS_SettlementPopulationCategory.Ranchers => populationSnapshot.RancherCount,
                CCS_SettlementPopulationCategory.Miners => populationSnapshot.MinerCount,
                CCS_SettlementPopulationCategory.LumberWorkers => populationSnapshot.LumberWorkerCount,
                CCS_SettlementPopulationCategory.Merchants => populationSnapshot.MerchantCount,
                CCS_SettlementPopulationCategory.Laborers => populationSnapshot.LaborerCount,
                _ => 0
            };
        }

        public static int ResolveVisibleActorCount(
            CCS_SettlementPopulationSnapshot populationSnapshot,
            CCS_SettlementPopulationCategory category,
            int minimumPopulationCount,
            int maxVisibleActors,
            bool settlementDiscovered,
            bool growthStageMet)
        {
            if (!settlementDiscovered || !growthStageMet || maxVisibleActors < 1)
            {
                return 0;
            }

            int sourceCount = ResolveWorkforceCount(populationSnapshot, category);
            if (sourceCount < minimumPopulationCount)
            {
                return 0;
            }

            return Math.Min(sourceCount, maxVisibleActors);
        }

        public static bool IsGrowthStageMet(
            CCS_SettlementGrowthSnapshot growthSnapshot,
            CCS_SettlementGrowthStage requiredStage)
        {
            if (requiredStage == CCS_SettlementGrowthStage.Unknown)
            {
                return true;
            }

            if (growthSnapshot == null || !growthSnapshot.IsValid)
            {
                return false;
            }

            return growthSnapshot.CurrentGrowthStage >= requiredStage;
        }

        public static CCS_PopulationPresenceSnapshot BuildSnapshot(
            string settlementId,
            CCS_SettlementPopulationSnapshot populationSnapshot,
            CCS_SettlementGrowthSnapshot growthSnapshot,
            bool settlementDiscovered,
            CCS_PopulationPresenceDefinition[] anchorDefinitions)
        {
            if (string.IsNullOrWhiteSpace(settlementId) || anchorDefinitions == null)
            {
                return CCS_PopulationPresenceSnapshot.Empty;
            }

            List<CCS_PopulationPresenceEntry> entries = new List<CCS_PopulationPresenceEntry>();
            for (int index = 0; index < anchorDefinitions.Length; index++)
            {
                CCS_PopulationPresenceDefinition definition = anchorDefinitions[index];
                if (definition == null
                    || !string.Equals(definition.SettlementId, settlementId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                bool growthStageMet = IsGrowthStageMet(growthSnapshot, definition.requiredGrowthStage);
                int sourceCount = ResolveWorkforceCount(populationSnapshot, definition.workforceCategory);
                int visibleCount = ResolveVisibleActorCount(
                    populationSnapshot,
                    definition.workforceCategory,
                    definition.minimumPopulationCount,
                    definition.maxVisibleActors,
                    settlementDiscovered,
                    growthStageMet);

                entries.Add(new CCS_PopulationPresenceEntry
                {
                    AnchorId = definition.AnchorId,
                    WorkforceCategory = definition.workforceCategory,
                    SourcePopulationCount = sourceCount,
                    VisibleActorCount = visibleCount
                });
            }

            return new CCS_PopulationPresenceSnapshot
            {
                SettlementId = settlementId,
                Entries = entries.ToArray()
            };
        }
    }
}
