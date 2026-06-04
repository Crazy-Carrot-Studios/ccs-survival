using System;
using System.Collections.Generic;
using CCS.Modules.Reputation;
using CCS.Modules.Settlements;
using CCS.Modules.WorldSimulation;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_BusinessValidationUtility
// CATEGORY: Modules / WorldSimulation / Runtime / Validation
// PURPOSE: Business catalog initialization, threshold evaluation, and snapshot building.
// PLACEMENT: Used by CCS_BusinessService, validators, and playtest harness.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.7.0 — population, prosperity, growth stage, optional reputation gates.
// =============================================================================

namespace CCS.Modules.WorldSimulation
{
    public static class CCS_BusinessValidationUtility
    {
        public static CCS_SurvivalValidationResult ValidateProfile(CCS_BusinessProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail("Business profile is null.");
            }

            CCS_SurvivalValidationResult baseValidation = CCS_SurvivalProfileValidationUtility.ValidateProfile(profile);
            if (!baseValidation.IsSuccess)
            {
                return baseValidation;
            }

            CCS_BusinessDefinition[] definitions = profile.BusinessDefinitions;
            if (definitions.Length == 0)
            {
                return CCS_SurvivalValidationResult.Fail("Business profile has no definitions.");
            }

            return CCS_SurvivalValidationResult.Pass("Business profile validated.");
        }

        public static void InitializeBusinessState(
            CCS_SettlementSimulationState settlementState,
            CCS_BusinessProfile profile)
        {
            if (settlementState == null || profile == null)
            {
                return;
            }

            if (!profile.TryGetSettlementCatalog(settlementState.settlementId, out CCS_BusinessSettlementCatalogEntry catalog)
                || catalog == null)
            {
                settlementState.businessStates = Array.Empty<CCS_BusinessState>();
                return;
            }

            CCS_BusinessType[] types = catalog.businessTypes ?? Array.Empty<CCS_BusinessType>();
            CCS_BusinessState[] states = new CCS_BusinessState[types.Length];
            for (int index = 0; index < types.Length; index++)
            {
                states[index] = new CCS_BusinessState
                {
                    businessType = (int)types[index],
                    isActive = false
                };
            }

            settlementState.businessStates = states;
        }

        public static void EnsureBusinessStateMigrated(
            CCS_SettlementSimulationState settlementState,
            CCS_BusinessProfile profile)
        {
            if (settlementState == null || profile == null)
            {
                return;
            }

            if (settlementState.businessStates == null
                || settlementState.businessStates.Length == 0)
            {
                InitializeBusinessState(settlementState, profile);
                return;
            }

            if (!profile.TryGetSettlementCatalog(settlementState.settlementId, out CCS_BusinessSettlementCatalogEntry catalog)
                || catalog == null)
            {
                return;
            }

            CCS_BusinessType[] catalogTypes = catalog.businessTypes ?? Array.Empty<CCS_BusinessType>();
            List<CCS_BusinessState> merged = new List<CCS_BusinessState>(catalogTypes.Length);
            for (int index = 0; index < catalogTypes.Length; index++)
            {
                CCS_BusinessType businessType = catalogTypes[index];
                bool isActive = TryGetPersistedActiveFlag(settlementState.businessStates, businessType);
                merged.Add(new CCS_BusinessState
                {
                    businessType = (int)businessType,
                    isActive = isActive
                });
            }

            settlementState.businessStates = merged.ToArray();
        }

        public static BusinessEvaluationChanges EvaluateSettlementBusinesses(
            CCS_SettlementSimulationState settlementState,
            CCS_BusinessProfile profile,
            CCS_ReputationTier reputationTier)
        {
            BusinessEvaluationChanges changes = new BusinessEvaluationChanges();
            if (settlementState == null
                || profile == null
                || !settlementState.isDiscovered
                || !profile.TryGetSettlementCatalog(settlementState.settlementId, out CCS_BusinessSettlementCatalogEntry catalog)
                || catalog == null)
            {
                return changes;
            }

            EnsureBusinessStateMigrated(settlementState, profile);
            CCS_SettlementGrowthStage growthStage = ResolveGrowthStage(settlementState.currentGrowthStage);
            CCS_BusinessState[] states = settlementState.businessStates ?? Array.Empty<CCS_BusinessState>();
            CCS_BusinessType[] catalogTypes = catalog.businessTypes ?? Array.Empty<CCS_BusinessType>();

            for (int index = 0; index < catalogTypes.Length; index++)
            {
                CCS_BusinessType businessType = catalogTypes[index];
                if (!profile.TryGetDefinition(businessType, out CCS_BusinessDefinition definition)
                    || definition == null)
                {
                    continue;
                }

                bool meetsThresholds = MeetsActivationThresholds(
                    definition,
                    settlementState.population,
                    settlementState.prosperity,
                    growthStage,
                    reputationTier);
                CCS_BusinessState state = FindOrCreateState(states, businessType);
                if (state == null)
                {
                    continue;
                }

                bool wasActive = state.isActive;
                bool shouldBeActive = meetsThresholds;
                if (shouldBeActive && !wasActive)
                {
                    state.isActive = true;
                    changes.Activated.Add(businessType);
                }
                else if (!shouldBeActive && wasActive)
                {
                    state.isActive = false;
                    changes.Deactivated.Add(businessType);
                }
            }

            return changes;
        }

        public static CCS_BusinessSnapshot BuildSnapshot(
            CCS_SettlementSimulationState settlementState,
            CCS_BusinessProfile profile,
            CCS_ReputationTier reputationTier)
        {
            if (settlementState == null || string.IsNullOrWhiteSpace(settlementState.settlementId))
            {
                return CCS_BusinessSnapshot.Empty;
            }

            EnsureBusinessStateMigrated(settlementState, profile);
            CCS_SettlementGrowthStage growthStage = ResolveGrowthStage(settlementState.currentGrowthStage);
            List<CCS_BusinessInstance> active = new List<CCS_BusinessInstance>();
            List<CCS_BusinessInstance> inactive = new List<CCS_BusinessInstance>();
            List<CCS_BusinessInstance> available = new List<CCS_BusinessInstance>();

            if (profile != null
                && profile.TryGetSettlementCatalog(settlementState.settlementId, out CCS_BusinessSettlementCatalogEntry catalog)
                && catalog != null)
            {
                CCS_BusinessType[] catalogTypes = catalog.businessTypes ?? Array.Empty<CCS_BusinessType>();
                for (int index = 0; index < catalogTypes.Length; index++)
                {
                    CCS_BusinessType businessType = catalogTypes[index];
                    profile.TryGetDefinition(businessType, out CCS_BusinessDefinition definition);
                    bool meetsThresholds = definition != null && MeetsActivationThresholds(
                        definition,
                        settlementState.population,
                        settlementState.prosperity,
                        growthStage,
                        reputationTier);
                    bool isActive = TryGetPersistedActiveFlag(settlementState.businessStates, businessType);
                    CCS_BusinessInstance instance = new CCS_BusinessInstance
                    {
                        SettlementId = settlementState.settlementId,
                        BusinessType = businessType,
                        BusinessId = definition?.BusinessId ?? string.Empty,
                        DisplayName = definition?.DisplayName ?? businessType.ToString(),
                        IsActive = isActive,
                        MeetsActivationThresholds = meetsThresholds
                    };

                    if (isActive)
                    {
                        active.Add(instance);
                    }
                    else if (meetsThresholds)
                    {
                        available.Add(instance);
                    }
                    else
                    {
                        inactive.Add(instance);
                    }
                }
            }

            return new CCS_BusinessSnapshot
            {
                SettlementId = settlementState.settlementId,
                ActiveBusinesses = active.ToArray(),
                InactiveBusinesses = inactive.ToArray(),
                AvailableBusinesses = available.ToArray()
            };
        }

        public static bool IsBusinessActive(
            CCS_SettlementSimulationState settlementState,
            CCS_BusinessType businessType)
        {
            if (settlementState == null)
            {
                return false;
            }

            return TryGetPersistedActiveFlag(settlementState.businessStates, businessType);
        }

        public static bool MeetsActivationThresholds(
            CCS_BusinessDefinition definition,
            int population,
            float prosperity,
            CCS_SettlementGrowthStage growthStage,
            CCS_ReputationTier reputationTier)
        {
            if (definition == null || definition.businessType == CCS_BusinessType.Unknown)
            {
                return false;
            }

            if (definition.minimumPopulation > 0 && population < definition.minimumPopulation)
            {
                return false;
            }

            if (prosperity < definition.minimumProsperity)
            {
                return false;
            }

            if ((int)growthStage < definition.minimumGrowthStage)
            {
                return false;
            }

            if (definition.HasReputationGate && (int)reputationTier < definition.minimumReputationTier)
            {
                return false;
            }

            return true;
        }

        private static bool TryGetPersistedActiveFlag(CCS_BusinessState[] states, CCS_BusinessType businessType)
        {
            if (states == null)
            {
                return false;
            }

            for (int index = 0; index < states.Length; index++)
            {
                CCS_BusinessState state = states[index];
                if (state != null && state.ResolvedBusinessType == businessType)
                {
                    return state.isActive;
                }
            }

            return false;
        }

        private static CCS_BusinessState FindOrCreateState(CCS_BusinessState[] states, CCS_BusinessType businessType)
        {
            if (states == null)
            {
                return null;
            }

            for (int index = 0; index < states.Length; index++)
            {
                CCS_BusinessState state = states[index];
                if (state != null && state.ResolvedBusinessType == businessType)
                {
                    return state;
                }
            }

            return null;
        }

        private static CCS_SettlementGrowthStage ResolveGrowthStage(int rawStage)
        {
            return Enum.IsDefined(typeof(CCS_SettlementGrowthStage), rawStage)
                ? (CCS_SettlementGrowthStage)rawStage
                : CCS_SettlementGrowthStage.Outpost;
        }

        public sealed class BusinessEvaluationChanges
        {
            public List<CCS_BusinessType> Activated { get; } = new List<CCS_BusinessType>();

            public List<CCS_BusinessType> Deactivated { get; } = new List<CCS_BusinessType>();

            public bool HasChanges => Activated.Count > 0 || Deactivated.Count > 0;
        }
    }
}
