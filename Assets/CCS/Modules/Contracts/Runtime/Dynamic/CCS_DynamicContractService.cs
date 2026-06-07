using System;
using System.Collections.Generic;
using CCS.Modules.Regions;
using CCS.Modules.Settlements;
using CCS.Modules.WorldSimulation;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_DynamicContractService
// CATEGORY: Modules / Contracts / Runtime / Dynamic
// PURPOSE: Generates temporary settlement contracts from simulation state and rules.
// PLACEMENT: Registered by CCS_SurvivalGameplayServiceRegistration.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 5.3.0 — uses existing CCS_ContractService accept/complete path.
// =============================================================================

namespace CCS.Modules.Contracts
{
    public sealed class CCS_DynamicContractService : CCS_ISurvivalService
    {
        private const string LogPrefix = "[CCS_DynamicContractService]";

        private readonly List<CCS_DynamicContractState> activeStates = new List<CCS_DynamicContractState>();
        private readonly List<CCS_DynamicContractRuleCooldownState> cooldownStates =
            new List<CCS_DynamicContractRuleCooldownState>();
        private readonly Dictionary<string, CCS_ContractDefinition> runtimeDefinitions =
            new Dictionary<string, CCS_ContractDefinition>(StringComparer.OrdinalIgnoreCase);

        private CCS_DynamicContractProfile activeProfile;
        private CCS_ContractService contractService;
        private CCS_WorldSimulationService worldSimulationService;
        private CCS_RegionService regionService;
        private Func<CCS_SettlementEventTimeSnapshot> resolveCurrentTime;
        private Func<string, int, CCS_SettlementNewsEntry[]> resolveRecentNews;
        private bool isInitialized;

        public bool IsInitialized => isInitialized;

        public CCS_DynamicContractProfile ActiveProfile => activeProfile;

        public void Initialize()
        {
            isInitialized = true;
        }

        public void InitializeFromProfile(CCS_DynamicContractProfile profile)
        {
            activeProfile = profile;
            isInitialized = profile != null;
            if (profile == null)
            {
                return;
            }

            CCS_SurvivalValidationResult validation = CCS_DynamicContractValidationUtility.ValidateProfile(profile);
            if (!validation.IsSuccess)
            {
                Debug.LogWarning($"{LogPrefix} Profile validation warning: {validation.Message}");
            }

            BindRuntimeBridge();
        }

        public void BindServices(
            CCS_ContractService contracts,
            CCS_WorldSimulationService worldSimulation,
            CCS_RegionService regions,
            Func<CCS_SettlementEventTimeSnapshot> currentTimeResolver,
            Func<string, int, CCS_SettlementNewsEntry[]> recentNewsResolver)
        {
            contractService = contracts;
            worldSimulationService = worldSimulation;
            regionService = regions;
            resolveCurrentTime = currentTimeResolver;
            resolveRecentNews = recentNewsResolver;
            BindRuntimeBridge();
        }

        public void EvaluateSettlementSupply(string settlementId)
        {
            if (!CanEvaluate(settlementId) || worldSimulationService == null)
            {
                return;
            }

            CCS_SettlementEventTimeSnapshot timeSnapshot =
                resolveCurrentTime?.Invoke() ?? CCS_SettlementEventTimeSnapshot.Default;
            EvaluateExpirations(timeSnapshot.DayNumber);

            CCS_DynamicContractRule[] rules = activeProfile.Rules;
            for (int index = 0; index < rules.Length; index++)
            {
                CCS_DynamicContractRule rule = rules[index];
                if (rule == null
                    || !rule.Enabled
                    || rule.PlaceholderOnly
                    || rule.GenerationSource != CCS_DynamicContractGenerationSource.LowSettlementSupply)
                {
                    continue;
                }

                float fillPercent = ResolveSupplyFillPercent(settlementId, rule.SupplyType);
                CCS_DynamicContractGenerationRequest request = new CCS_DynamicContractGenerationRequest
                {
                    SettlementId = settlementId,
                    GenerationSource = CCS_DynamicContractGenerationSource.LowSettlementSupply,
                    SupplyType = rule.SupplyType,
                    SupplyFillPercent = fillPercent,
                    CurrentDayNumber = timeSnapshot.DayNumber,
                    NewsHeadlineReference = ResolveRecentNewsHeadline(settlementId)
                };
                TryGenerateFromMatchingRules(request);
            }
        }

        public void EvaluateRegionalSpecialization(string settlementId)
        {
            if (!CanEvaluate(settlementId) || regionService == null || !regionService.IsInitialized)
            {
                return;
            }

            if (!regionService.TryGetOwningRegionForSettlement(settlementId, out CCS_RegionDefinition regionDefinition)
                || regionDefinition == null)
            {
                return;
            }

            CCS_SettlementEventTimeSnapshot timeSnapshot =
                resolveCurrentTime?.Invoke() ?? CCS_SettlementEventTimeSnapshot.Default;
            EvaluateExpirations(timeSnapshot.DayNumber);

            CCS_DynamicContractGenerationRequest request = new CCS_DynamicContractGenerationRequest
            {
                SettlementId = settlementId,
                GenerationSource = CCS_DynamicContractGenerationSource.RegionalSpecialization,
                RegionSpecialization = regionDefinition.SpecializationType,
                CurrentDayNumber = timeSnapshot.DayNumber,
                NewsHeadlineReference = ResolveRecentNewsHeadline(settlementId)
            };
            TryGenerateFromMatchingRules(request);
        }

        public void HandleSettlementEventActivated(string settlementId, CCS_SettlementEventSnapshot eventSnapshot)
        {
            if (!CanEvaluate(settlementId) || eventSnapshot == null || !eventSnapshot.IsValid)
            {
                return;
            }

            CCS_SettlementEventTimeSnapshot timeSnapshot =
                resolveCurrentTime?.Invoke() ?? CCS_SettlementEventTimeSnapshot.Default;
            EvaluateExpirations(timeSnapshot.DayNumber);

            CCS_DynamicContractGenerationRequest request = new CCS_DynamicContractGenerationRequest
            {
                SettlementId = settlementId,
                GenerationSource = CCS_DynamicContractGenerationSource.ActiveSettlementEvent,
                EventType = eventSnapshot.EventType,
                LinkedEventId = eventSnapshot.ActiveEventId ?? string.Empty,
                CurrentDayNumber = timeSnapshot.DayNumber,
                NewsHeadlineReference = ResolveRecentNewsHeadline(settlementId)
            };
            TryGenerateFromMatchingRules(request);
        }

        public CCS_DynamicContractGenerationResult TryGenerateForRequest(CCS_DynamicContractGenerationRequest request)
        {
            if (request == null || !CanEvaluate(request.SettlementId))
            {
                return CCS_DynamicContractGenerationResult.Failure("Dynamic contract service is not ready.");
            }

            if (request.CurrentDayNumber < 1)
            {
                request.CurrentDayNumber = resolveCurrentTime?.Invoke().DayNumber ?? 1;
            }

            EvaluateExpirations(request.CurrentDayNumber);
            return TryGenerateFromMatchingRules(request);
        }

        public void HandleContractCompleted(string contractId)
        {
            if (string.IsNullOrWhiteSpace(contractId)
                || !CCS_DynamicContractValidationUtility.IsGeneratedContractId(contractId))
            {
                return;
            }

            for (int index = 0; index < activeStates.Count; index++)
            {
                CCS_DynamicContractState state = activeStates[index];
                if (state == null
                    || !string.Equals(state.generatedContractId, contractId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                state.contractState = (int)CCS_ContractState.Completed;
                state.isActive = false;
                UnregisterRuntimeDefinition(contractId);
                BindRuntimeBridge();
                return;
            }
        }

        public void HandleContractAccepted(string contractId, string settlementId)
        {
            if (string.IsNullOrWhiteSpace(contractId)
                || !CCS_DynamicContractValidationUtility.IsGeneratedContractId(contractId))
            {
                return;
            }

            for (int index = 0; index < activeStates.Count; index++)
            {
                CCS_DynamicContractState state = activeStates[index];
                if (state == null
                    || !string.Equals(state.generatedContractId, contractId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                state.contractState = (int)CCS_ContractState.Accepted;
                state.acceptedSettlementId = settlementId ?? string.Empty;
                BindRuntimeBridge();
                return;
            }
        }

        public void EvaluateExpirations(int currentDayNumber)
        {
            if (activeStates.Count == 0)
            {
                return;
            }

            bool changed = false;
            for (int index = activeStates.Count - 1; index >= 0; index--)
            {
                CCS_DynamicContractState state = activeStates[index];
                if (state == null)
                {
                    activeStates.RemoveAt(index);
                    changed = true;
                    continue;
                }

                if (state.contractState == (int)CCS_ContractState.Completed)
                {
                    activeStates.RemoveAt(index);
                    UnregisterRuntimeDefinition(state.generatedContractId);
                    changed = true;
                    continue;
                }

                if (!CCS_DynamicContractValidationUtility.IsContractExpired(state, currentDayNumber))
                {
                    continue;
                }

                state.isActive = false;
                UnregisterRuntimeDefinition(state.generatedContractId);
                activeStates.RemoveAt(index);
                changed = true;
            }

            if (changed)
            {
                BindRuntimeBridge();
            }
        }

        public CCS_DynamicContractSnapshot[] GetSettlementSnapshots(string settlementId)
        {
            if (string.IsNullOrWhiteSpace(settlementId) || activeStates.Count == 0)
            {
                return Array.Empty<CCS_DynamicContractSnapshot>();
            }

            List<CCS_DynamicContractSnapshot> snapshots = new List<CCS_DynamicContractSnapshot>();
            for (int index = 0; index < activeStates.Count; index++)
            {
                CCS_DynamicContractState state = activeStates[index];
                if (state == null
                    || !state.isActive
                    || !string.Equals(state.settlementId, settlementId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                CCS_DynamicContractSnapshot snapshot = CCS_DynamicContractValidationUtility.BuildSnapshot(state);
                if (snapshot != null && snapshot.IsValid)
                {
                    snapshots.Add(snapshot);
                }
            }

            return snapshots.ToArray();
        }

        public CCS_DynamicContractState[] CaptureDynamicContractStates()
        {
            if (activeStates.Count == 0)
            {
                return Array.Empty<CCS_DynamicContractState>();
            }

            return activeStates.ToArray();
        }

        public CCS_DynamicContractRuleCooldownState[] CaptureRuleCooldownStates()
        {
            if (cooldownStates.Count == 0)
            {
                return Array.Empty<CCS_DynamicContractRuleCooldownState>();
            }

            return cooldownStates.ToArray();
        }

        public void RestorePersistedState(
            CCS_DynamicContractState[] states,
            CCS_DynamicContractRuleCooldownState[] cooldowns)
        {
            ClearRuntimeGenerated();
            activeStates.Clear();
            cooldownStates.Clear();

            if (cooldowns != null)
            {
                for (int index = 0; index < cooldowns.Length; index++)
                {
                    CCS_DynamicContractRuleCooldownState cooldown = cooldowns[index];
                    if (cooldown != null)
                    {
                        cooldownStates.Add(cooldown);
                    }
                }
            }

            if (states == null || states.Length == 0)
            {
                BindRuntimeBridge();
                return;
            }

            int currentDay = resolveCurrentTime?.Invoke().DayNumber ?? 1;
            for (int index = 0; index < states.Length; index++)
            {
                CCS_DynamicContractState state = states[index];
                if (state == null
                    || string.IsNullOrWhiteSpace(state.generatedContractId)
                    || !state.isActive)
                {
                    continue;
                }

                if (CCS_DynamicContractValidationUtility.IsContractExpired(state, currentDay)
                    && state.contractState != (int)CCS_ContractState.Accepted)
                {
                    continue;
                }

                RegisterRuntimeState(state);
            }

            BindRuntimeBridge();
        }

        public void SyncContractInstanceStates()
        {
            if (contractService == null || !contractService.IsInitialized)
            {
                return;
            }

            for (int index = 0; index < activeStates.Count; index++)
            {
                CCS_DynamicContractState state = activeStates[index];
                if (state == null || string.IsNullOrWhiteSpace(state.generatedContractId))
                {
                    continue;
                }

                contractService.SetInstanceState(
                    state.generatedContractId,
                    (CCS_ContractState)state.contractState,
                    state.acceptedSettlementId);
            }
        }

        private CCS_DynamicContractGenerationResult TryGenerateFromMatchingRules(
            CCS_DynamicContractGenerationRequest request)
        {
            if (request == null || activeProfile == null)
            {
                return CCS_DynamicContractGenerationResult.Failure("Dynamic contract profile unavailable.");
            }

            if (!request.ForceGenerationForPlaytest
                && !CCS_DynamicContractValidationUtility.IsActiveGenerationSource(request.GenerationSource))
            {
                return CCS_DynamicContractGenerationResult.Failure("Generation source is inactive for this milestone.");
            }

            if (CountActiveForSettlement(request.SettlementId) >= activeProfile.MaxActiveGeneratedContractsPerSettlement
                && !request.ForceGenerationForPlaytest)
            {
                return CCS_DynamicContractGenerationResult.Failure("Settlement reached max active generated contracts.");
            }

            CCS_DynamicContractRule[] rules = activeProfile.Rules;
            for (int index = 0; index < rules.Length; index++)
            {
                CCS_DynamicContractRule rule = rules[index];
                if (!CCS_DynamicContractValidationUtility.RuleMatchesRequest(rule, request))
                {
                    continue;
                }

                if (!request.ForceGenerationForPlaytest
                    && !CCS_DynamicContractValidationUtility.IsCooldownReady(
                        rule,
                        request.SettlementId,
                        request.CurrentDayNumber,
                        cooldownStates.ToArray()))
                {
                    continue;
                }

                if (CCS_DynamicContractValidationUtility.HasDuplicateActiveContract(
                        rule,
                        request.SettlementId,
                        activeStates.ToArray()))
                {
                    continue;
                }

                CCS_DynamicContractState state = CCS_DynamicContractValidationUtility.CreateStateFromRule(
                    rule,
                    request.SettlementId,
                    request.CurrentDayNumber,
                    request.LinkedEventId,
                    request.NewsHeadlineReference);
                if (state == null)
                {
                    continue;
                }

                RegisterRuntimeState(state);
                RecordCooldown(rule.RuleId, request.SettlementId, request.CurrentDayNumber);
                CCS_DynamicContractSnapshot snapshot = CCS_DynamicContractValidationUtility.BuildSnapshot(state);
                BindRuntimeBridge();
                CCS_DynamicContractGenerationResult success = CCS_DynamicContractGenerationResult.Success(
                    state.generatedContractId,
                    rule.RuleId,
                    snapshot,
                    $"Generated dynamic contract: {state.displayName}.");
                CCS_DynamicContractRuntimeBridge.LastGenerationResult = success;
                return success;
            }

            return CCS_DynamicContractGenerationResult.Failure("No matching dynamic contract rule passed validation.");
        }

        private void RegisterRuntimeState(CCS_DynamicContractState state)
        {
            if (state == null || contractService == null || !contractService.IsInitialized)
            {
                return;
            }

            CCS_ContractDefinition definition = CCS_DynamicContractValidationUtility.CreateRuntimeDefinition(state);
            if (definition == null)
            {
                return;
            }

            UnregisterRuntimeDefinition(state.generatedContractId);
            runtimeDefinitions[state.generatedContractId] = definition;
            contractService.RegisterDefinition(definition);
            contractService.SetInstanceState(
                state.generatedContractId,
                (CCS_ContractState)state.contractState,
                state.acceptedSettlementId);
            activeStates.Add(state);
        }

        private void RecordCooldown(string ruleId, string settlementId, int dayNumber)
        {
            for (int index = 0; index < cooldownStates.Count; index++)
            {
                CCS_DynamicContractRuleCooldownState cooldown = cooldownStates[index];
                if (cooldown == null)
                {
                    continue;
                }

                if (string.Equals(cooldown.ruleId, ruleId, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(cooldown.settlementId, settlementId, StringComparison.OrdinalIgnoreCase))
                {
                    cooldown.lastGeneratedDay = dayNumber;
                    return;
                }
            }

            cooldownStates.Add(new CCS_DynamicContractRuleCooldownState
            {
                ruleId = ruleId ?? string.Empty,
                settlementId = settlementId ?? string.Empty,
                lastGeneratedDay = dayNumber
            });
        }

        private void ClearRuntimeGenerated()
        {
            foreach (KeyValuePair<string, CCS_ContractDefinition> entry in runtimeDefinitions)
            {
                contractService?.UnregisterDefinition(entry.Key);
            }

            runtimeDefinitions.Clear();
        }

        private void UnregisterRuntimeDefinition(string contractId)
        {
            if (string.IsNullOrWhiteSpace(contractId))
            {
                return;
            }

            runtimeDefinitions.Remove(contractId);
            contractService?.UnregisterDefinition(contractId);
        }

        private bool CanEvaluate(string settlementId)
        {
            return isInitialized
                && activeProfile != null
                && contractService != null
                && contractService.IsInitialized
                && !string.IsNullOrWhiteSpace(settlementId);
        }

        private int CountActiveForSettlement(string settlementId)
        {
            return CCS_DynamicContractValidationUtility.CountActiveGeneratedContracts(
                settlementId,
                activeStates.ToArray());
        }

        private float ResolveSupplyFillPercent(string settlementId, CCS_SettlementSupplyType supplyType)
        {
            if (worldSimulationService == null || !worldSimulationService.IsInitialized)
            {
                return 100f;
            }

            if (!worldSimulationService.TryGetSettlementState(settlementId, out CCS_SettlementSimulationState state)
                || state?.supplies == null)
            {
                return 100f;
            }

            for (int index = 0; index < state.supplies.Length; index++)
            {
                CCS_SettlementSupplyEntry entry = state.supplies[index];
                if (entry == null || entry.supplyType != (int)supplyType)
                {
                    continue;
                }

                return Mathf.Clamp01(entry.FillRatio) * 100f;
            }

            return 100f;
        }

        private string ResolveRecentNewsHeadline(string settlementId)
        {
            if (resolveRecentNews == null || string.IsNullOrWhiteSpace(settlementId))
            {
                return string.Empty;
            }

            CCS_SettlementNewsEntry[] entries = resolveRecentNews.Invoke(settlementId, 1);
            if (entries == null || entries.Length == 0 || entries[0] == null)
            {
                return string.Empty;
            }

            return entries[0].Headline ?? string.Empty;
        }

        private void BindRuntimeBridge()
        {
            CCS_DynamicContractRuntimeBridge.ResolveSettlementSnapshots = GetSettlementSnapshots;
            CCS_DynamicContractRuntimeBridge.TryGenerateForRequest = TryGenerateForRequest;
            CCS_DynamicContractRuntimeBridge.TryEvaluateSettlementSupply = EvaluateSettlementSupply;
            CCS_DynamicContractRuntimeBridge.TryEvaluateRegionalSpecialization = EvaluateRegionalSpecialization;
            CCS_DynamicContractRuntimeBridge.TryHandleEventActivated = HandleSettlementEventActivated;
        }
    }
}
