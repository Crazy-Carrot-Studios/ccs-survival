using System;
using System.Collections.Generic;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SettlementHousingValidationUtility
// CATEGORY: Modules / Settlements / Runtime / Validation
// PURPOSE: Housing sync, capacity resolution, snapshot building, and profile validation.
// PLACEMENT: Used by CCS_SettlementHousingService, validators, and playtest harness.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.4.0 — total capacity = base population capacity + active housing.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public static class CCS_SettlementHousingValidationUtility
    {
        public static CCS_SurvivalValidationResult ValidateProfile(CCS_SettlementHousingProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail("Settlement housing profile is null.");
            }

            CCS_SurvivalValidationResult baseValidation = CCS_SurvivalProfileValidationUtility.ValidateProfile(profile);
            if (!baseValidation.IsSuccess)
            {
                return baseValidation;
            }

            CCS_SettlementHousingDefinition[] definitions = profile.HousingDefinitions;
            if (definitions.Length == 0)
            {
                return CCS_SurvivalValidationResult.Fail("Settlement housing profile has no housing definitions.");
            }

            HashSet<string> housingIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            HashSet<string> anchorIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            HashSet<string> settlementCoverage = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_SettlementHousingDefinition definition = definitions[index];
                if (definition == null || string.IsNullOrWhiteSpace(definition.HousingId))
                {
                    return CCS_SurvivalValidationResult.Fail("Settlement housing definition missing housing id.");
                }

                if (!housingIds.Add(definition.HousingId))
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Duplicate settlement housing id '{definition.HousingId}'.");
                }

                if (string.IsNullOrWhiteSpace(definition.SettlementId))
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Settlement housing '{definition.HousingId}' missing settlement id.");
                }

                if (definition.CapacityContribution <= 0)
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Settlement housing '{definition.HousingId}' capacity contribution must be positive.");
                }

                if (!Enum.IsDefined(typeof(CCS_SettlementGrowthStage), definition.RequiredGrowthStage)
                    || definition.RequiredGrowthStage == CCS_SettlementGrowthStage.Unknown)
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Settlement housing '{definition.HousingId}' has invalid required growth stage.");
                }

                if (!string.IsNullOrWhiteSpace(definition.AnchorId) && !anchorIds.Add(definition.AnchorId))
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Duplicate settlement housing anchor id '{definition.AnchorId}'.");
                }

                settlementCoverage.Add(definition.SettlementId);
            }

            if (settlementCoverage.Count < CCS_MultiSettlementContentIds.BootstrapSettlementCount)
            {
                return CCS_SurvivalValidationResult.Fail(
                    "Settlement housing profile must define housing for all active bootstrap settlements.");
            }

            return CCS_SurvivalValidationResult.Pass("Settlement housing profile validated.");
        }

        public static CCS_SettlementHousingState[] SyncHousingStates(
            string settlementId,
            CCS_SettlementHousingState[] persistedStates,
            CCS_SettlementHousingProfile profile,
            CCS_SettlementGrowthStage currentGrowthStage,
            bool isDiscovered)
        {
            if (string.IsNullOrWhiteSpace(settlementId) || profile == null)
            {
                return persistedStates ?? Array.Empty<CCS_SettlementHousingState>();
            }

            List<CCS_SettlementHousingState> merged = new List<CCS_SettlementHousingState>();
            CCS_SettlementHousingDefinition[] definitions = profile.HousingDefinitions;
            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_SettlementHousingDefinition definition = definitions[index];
                if (definition == null
                    || !string.Equals(definition.SettlementId, settlementId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                CCS_SettlementHousingState persisted = TryFindState(persistedStates, definition.HousingId);
                bool isActive = isDiscovered
                    && IsGrowthStageMet(currentGrowthStage, definition.RequiredGrowthStage);
                merged.Add(new CCS_SettlementHousingState
                {
                    housingId = definition.HousingId,
                    settlementId = definition.SettlementId,
                    displayName = definition.DisplayName,
                    housingType = (int)definition.HousingType,
                    capacityContribution = definition.CapacityContribution,
                    requiredGrowthStage = (int)definition.RequiredGrowthStage,
                    workforceAffinity = (int)definition.WorkforceAffinity,
                    isActive = isActive
                });
            }

            return merged.ToArray();
        }

        public static int ResolveActiveHousingCapacity(CCS_SettlementHousingState[] housingStates)
        {
            if (housingStates == null || housingStates.Length == 0)
            {
                return 0;
            }

            int total = 0;
            for (int index = 0; index < housingStates.Length; index++)
            {
                CCS_SettlementHousingState state = housingStates[index];
                if (state != null && state.isActive && state.capacityContribution > 0)
                {
                    total += state.capacityContribution;
                }
            }

            return Mathf.Max(0, total);
        }

        public static int ResolveTotalPopulationCapacity(
            float prosperity,
            CCS_SettlementPopulationProfile populationProfile,
            CCS_SettlementHousingState[] housingStates)
        {
            int baseCapacity = ResolveBasePopulationCapacity(prosperity, populationProfile);
            int housingCapacity = ResolveActiveHousingCapacity(housingStates);
            return Mathf.Max(0, baseCapacity + housingCapacity);
        }

        public static int ResolveBasePopulationCapacity(
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

        public static CCS_SettlementHousingStatus ResolveHousingStatus(
            bool isDiscovered,
            CCS_SettlementGrowthStage currentGrowthStage,
            CCS_SettlementHousingState state)
        {
            if (state == null)
            {
                return CCS_SettlementHousingStatus.Unknown;
            }

            if (!isDiscovered)
            {
                return CCS_SettlementHousingStatus.Locked;
            }

            if (state.isActive)
            {
                return CCS_SettlementHousingStatus.Active;
            }

            CCS_SettlementGrowthStage requiredStage = Enum.IsDefined(typeof(CCS_SettlementGrowthStage), state.requiredGrowthStage)
                ? (CCS_SettlementGrowthStage)state.requiredGrowthStage
                : CCS_SettlementGrowthStage.Unknown;
            return IsGrowthStageMet(currentGrowthStage, requiredStage)
                ? CCS_SettlementHousingStatus.Inactive
                : CCS_SettlementHousingStatus.Locked;
        }

        public static CCS_SettlementHousingSnapshot BuildSnapshot(
            string settlementId,
            float prosperity,
            CCS_SettlementPopulationProfile populationProfile,
            CCS_SettlementHousingState[] housingStates,
            CCS_SettlementHousingProfile housingProfile,
            bool isDiscovered,
            CCS_SettlementGrowthStage currentGrowthStage)
        {
            if (string.IsNullOrWhiteSpace(settlementId))
            {
                return CCS_SettlementHousingSnapshot.Empty;
            }

            int baseCapacity = ResolveBasePopulationCapacity(prosperity, populationProfile);
            int housingCapacity = ResolveActiveHousingCapacity(housingStates);
            List<string> activeNames = new List<string>();
            List<CCS_SettlementHousingEntry> entries = new List<CCS_SettlementHousingEntry>();
            CCS_SettlementHousingDefinition[] definitions = housingProfile?.HousingDefinitions
                ?? Array.Empty<CCS_SettlementHousingDefinition>();
            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_SettlementHousingDefinition definition = definitions[index];
                if (definition == null
                    || !string.Equals(definition.SettlementId, settlementId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                CCS_SettlementHousingState state = TryFindState(housingStates, definition.HousingId);
                CCS_SettlementHousingStatus status = ResolveHousingStatus(isDiscovered, currentGrowthStage, state);
                bool isActive = state != null && state.isActive;
                if (isActive)
                {
                    activeNames.Add(definition.DisplayName);
                }

                entries.Add(new CCS_SettlementHousingEntry
                {
                    HousingId = definition.HousingId,
                    DisplayName = definition.DisplayName,
                    CapacityContribution = definition.CapacityContribution,
                    Status = status,
                    IsActive = isActive
                });
            }

            return new CCS_SettlementHousingSnapshot
            {
                SettlementId = settlementId,
                BasePopulationCapacity = baseCapacity,
                HousingCapacityContribution = housingCapacity,
                TotalPopulationCapacity = baseCapacity + housingCapacity,
                ActiveHousingCount = activeNames.Count,
                ActiveHousingNames = activeNames.ToArray(),
                HousingEntries = entries.ToArray()
            };
        }

        public static CCS_SettlementHousingState[] CloneStates(CCS_SettlementHousingState[] source)
        {
            if (source == null || source.Length == 0)
            {
                return Array.Empty<CCS_SettlementHousingState>();
            }

            CCS_SettlementHousingState[] clone = new CCS_SettlementHousingState[source.Length];
            for (int index = 0; index < source.Length; index++)
            {
                CCS_SettlementHousingState entry = source[index];
                clone[index] = entry == null
                    ? new CCS_SettlementHousingState()
                    : new CCS_SettlementHousingState
                    {
                        housingId = entry.housingId,
                        settlementId = entry.settlementId,
                        displayName = entry.displayName,
                        housingType = entry.housingType,
                        capacityContribution = entry.capacityContribution,
                        requiredGrowthStage = entry.requiredGrowthStage,
                        workforceAffinity = entry.workforceAffinity,
                        isActive = entry.isActive
                    };
            }

            return clone;
        }

        public static CCS_SettlementHousingState TryFindState(
            CCS_SettlementHousingState[] states,
            string housingId)
        {
            if (states == null || string.IsNullOrWhiteSpace(housingId))
            {
                return null;
            }

            for (int index = 0; index < states.Length; index++)
            {
                CCS_SettlementHousingState state = states[index];
                if (state != null
                    && string.Equals(state.housingId, housingId, StringComparison.OrdinalIgnoreCase))
                {
                    return state;
                }
            }

            return null;
        }

        public static bool IsGrowthStageMet(
            CCS_SettlementGrowthStage currentStage,
            CCS_SettlementGrowthStage requiredStage)
        {
            if (requiredStage == CCS_SettlementGrowthStage.Unknown)
            {
                return false;
            }

            return (int)currentStage >= (int)requiredStage;
        }
    }
}
