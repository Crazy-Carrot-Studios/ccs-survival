using System;
using System.Collections.Generic;
using CCS.Modules.Economy;
using CCS.Modules.Inventory;
using CCS.Modules.Reputation;
using CCS.Modules.WorldSimulation;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_ContractService
// CATEGORY: Modules / Contracts / Runtime / Services
// PURPOSE: Accepts and completes frontier settlement contracts for item delivery rewards.
// PLACEMENT: Registered by CCS_SurvivalGameplayServiceRegistration.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.0.0 — no final UI; debug panel drives accept/complete flows.
// =============================================================================

namespace CCS.Modules.Contracts
{
    public sealed class CCS_ContractService : CCS_ISurvivalService
    {
        private const string LogPrefix = "[CCS_ContractService]";

        private sealed class ContractInstance
        {
            public CCS_ContractState State = CCS_ContractState.Available;
            public string AcceptedSettlementId = string.Empty;
        }

        private readonly Dictionary<string, CCS_ContractDefinition> definitionLookup =
            new Dictionary<string, CCS_ContractDefinition>(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<string, ContractInstance> instanceLookup =
            new Dictionary<string, ContractInstance>(StringComparer.OrdinalIgnoreCase);

        private CCS_ContractProfile activeProfile;
        private CCS_PlayerInventoryService inventoryService;
        private CCS_CurrencyService currencyService;
        private CCS_ReputationService reputationService;
        private CCS_WorldSimulationService worldSimulationService;
        private bool isInitialized;

        public event Action<CCS_ContractCompletionResult> ContractAccepted;

        public event Action<CCS_ContractCompletionResult> ContractCompleted;

        public bool IsInitialized => isInitialized;

        public CCS_ContractProfile ActiveProfile => activeProfile;

        public void Initialize()
        {
            isInitialized = true;
        }

        public void InitializeFromProfile(CCS_ContractProfile profile)
        {
            activeProfile = profile;
            definitionLookup.Clear();
            instanceLookup.Clear();

            if (profile == null)
            {
                isInitialized = false;
                return;
            }

            CCS_SurvivalValidationResult validation = CCS_ContractValidationUtility.ValidateProfile(profile);
            if (!validation.IsSuccess)
            {
                Debug.LogWarning($"{LogPrefix} Profile validation warning: {validation.Message}");
            }

            CCS_ContractDefinition[] definitions = profile.ContractDefinitions;
            for (int index = 0; index < definitions.Length; index++)
            {
                RegisterDefinition(definitions[index]);
            }

            isInitialized = definitionLookup.Count > 0;
        }

        public void BindServices(
            CCS_PlayerInventoryService inventory,
            CCS_CurrencyService currency,
            CCS_ReputationService reputation,
            CCS_WorldSimulationService worldSimulation)
        {
            inventoryService = inventory;
            currencyService = currency;
            reputationService = reputation;
            worldSimulationService = worldSimulation;
        }

        public void RegisterDefinition(CCS_ContractDefinition definition)
        {
            if (definition == null || string.IsNullOrWhiteSpace(definition.ContractId))
            {
                return;
            }

            definitionLookup[definition.ContractId] = definition;
            if (!instanceLookup.ContainsKey(definition.ContractId))
            {
                instanceLookup[definition.ContractId] = new ContractInstance();
            }
        }

        public bool TryGetDefinition(string contractId, out CCS_ContractDefinition definition)
        {
            definition = null;
            if (string.IsNullOrWhiteSpace(contractId))
            {
                return false;
            }

            return definitionLookup.TryGetValue(contractId, out definition);
        }

        public CCS_ContractState GetContractState(string contractId)
        {
            if (string.IsNullOrWhiteSpace(contractId)
                || !instanceLookup.TryGetValue(contractId, out ContractInstance instance)
                || instance == null)
            {
                return CCS_ContractState.Available;
            }

            return instance.State;
        }

        public CCS_ContractDefinition[] GetBoardContracts(string settlementId, CCS_ContractType contractType)
        {
            List<CCS_ContractDefinition> results = new List<CCS_ContractDefinition>();
            foreach (KeyValuePair<string, CCS_ContractDefinition> entry in definitionLookup)
            {
                CCS_ContractDefinition definition = entry.Value;
                if (definition == null
                    || !definition.Enabled
                    || definition.ContractType != contractType
                    || !definition.MatchesSettlement(settlementId))
                {
                    continue;
                }

                results.Add(definition);
            }

            return results.ToArray();
        }

        public CCS_ContractCompletionResult TryAcceptContract(string contractId, string settlementId)
        {
            if (!isInitialized)
            {
                return Failure(contractId, "Contract service is not ready.");
            }

            if (!TryGetDefinition(contractId, out CCS_ContractDefinition definition))
            {
                return Failure(contractId, "Contract not found.");
            }

            if (!definition.Enabled)
            {
                return Failure(contractId, "Contract is disabled.");
            }

            if (!definition.MatchesSettlement(settlementId))
            {
                return Failure(contractId, "Contract is not available at this settlement.");
            }

            ContractInstance instance = GetOrCreateInstance(contractId);
            if (instance.State == CCS_ContractState.Completed)
            {
                return Failure(contractId, "Contract already completed.");
            }

            if (instance.State == CCS_ContractState.Accepted)
            {
                return Failure(contractId, "Contract already accepted.");
            }

            instance.State = CCS_ContractState.Accepted;
            instance.AcceptedSettlementId = settlementId ?? string.Empty;
            CCS_ContractCompletionResult result = Success(
                contractId,
                $"Accepted contract: {definition.DisplayName}.");
            ContractAccepted?.Invoke(result);
            CCS_ContractDebugHud.NotifyContractAccepted(definition, settlementId);
            return result;
        }

        public CCS_ContractCompletionResult TryCompleteContract(string contractId)
        {
            if (!isInitialized
                || inventoryService == null
                || !inventoryService.IsInitialized
                || currencyService == null
                || !currencyService.IsInitialized)
            {
                return Failure(contractId, "Contract services are not ready.");
            }

            if (!TryGetDefinition(contractId, out CCS_ContractDefinition definition))
            {
                return Failure(contractId, "Contract not found.");
            }

            ContractInstance instance = GetOrCreateInstance(contractId);
            if (instance.State != CCS_ContractState.Accepted)
            {
                return Failure(contractId, "Contract must be accepted before completion.");
            }

            string settlementId = string.IsNullOrWhiteSpace(instance.AcceptedSettlementId)
                ? activeProfile.DefaultSettlementId
                : instance.AcceptedSettlementId;

            CCS_ContractRequirement[] requirements = definition.Requirements;
            for (int index = 0; index < requirements.Length; index++)
            {
                CCS_ContractRequirement requirement = requirements[index];
                if (requirement == null)
                {
                    continue;
                }

                if (!requirement.MatchesSettlement(settlementId))
                {
                    return Failure(contractId, "Contract requirement settlement restriction failed.");
                }

                if (!TryFindItemDefinition(requirement.ItemId, out CCS_ItemDefinition itemDefinition))
                {
                    return Failure(contractId, $"Required item not found: {requirement.ItemId}.");
                }

                int owned = inventoryService.GetQuantity(itemDefinition);
                if (owned < requirement.Quantity)
                {
                    return Failure(
                        contractId,
                        $"Need {requirement.Quantity}x {requirement.ItemId} (owned {owned}).");
                }
            }

            for (int index = 0; index < requirements.Length; index++)
            {
                CCS_ContractRequirement requirement = requirements[index];
                if (requirement == null
                    || !TryFindItemDefinition(requirement.ItemId, out CCS_ItemDefinition itemDefinition))
                {
                    continue;
                }

                int removed = inventoryService.RemoveItem(itemDefinition, requirement.Quantity);
                if (removed < requirement.Quantity)
                {
                    return Failure(contractId, "Failed to remove required contract goods.");
                }
            }

            CCS_ContractReward reward = definition.Reward;
            string currencyId = activeProfile?.DefaultCurrencyId ?? CCS_ContractContentIds.TradeDollarsCurrencyId;
            if (reward.TradeDollars > 0)
            {
                currencyService.AddCurrency(
                    currencyId,
                    reward.TradeDollars,
                    $"Contract reward: {definition.DisplayName}");
            }

            int reputationApplied = 0;
            if (reward.ReputationGain != 0
                && reputationService != null
                && reputationService.IsInitialized
                && reputationService.TryApplyContractReward(settlementId, reward.ReputationGain))
            {
                reputationApplied = reward.ReputationGain;
            }

            float supplyApplied = 0f;
            float prosperityApplied = 0f;
            if (worldSimulationService != null
                && worldSimulationService.IsInitialized
                && reward.SupplyAmount > 0f)
            {
                worldSimulationService.HandleContractCompleted(
                    settlementId,
                    reward.SupplyType,
                    reward.SupplyAmount,
                    reward.ProsperityGain);
                supplyApplied = reward.SupplyAmount;
                prosperityApplied = reward.ProsperityGain;
            }

            instance.State = CCS_ContractState.Completed;
            CCS_ContractCompletionResult success = new CCS_ContractCompletionResult(
                true,
                contractId,
                $"Completed contract: {definition.DisplayName}.",
                reward.TradeDollars,
                reputationApplied,
                prosperityApplied,
                supplyApplied);
            LogDebug(success.Message);
            ContractCompleted?.Invoke(success);
            CCS_ContractDebugHud.NotifyContractCompleted(success, definition, settlementId);
            return success;
        }

        public CCS_ContractSnapshot[] CaptureContractsState()
        {
            if (instanceLookup.Count == 0)
            {
                return Array.Empty<CCS_ContractSnapshot>();
            }

            CCS_ContractSnapshot[] snapshots = new CCS_ContractSnapshot[instanceLookup.Count];
            int index = 0;
            foreach (KeyValuePair<string, ContractInstance> entry in instanceLookup)
            {
                ContractInstance instance = entry.Value;
                if (instance == null || instance.State == CCS_ContractState.Available)
                {
                    continue;
                }

                snapshots[index++] = new CCS_ContractSnapshot
                {
                    contractDefinitionId = entry.Key,
                    contractState = (int)instance.State,
                    acceptedSettlementId = instance.AcceptedSettlementId ?? string.Empty
                };
            }

            if (index < snapshots.Length)
            {
                Array.Resize(ref snapshots, index);
            }

            return snapshots;
        }

        public void RestoreState(CCS_ContractSnapshot[] snapshots)
        {
            foreach (KeyValuePair<string, ContractInstance> entry in instanceLookup)
            {
                if (entry.Value != null)
                {
                    entry.Value.State = CCS_ContractState.Available;
                    entry.Value.AcceptedSettlementId = string.Empty;
                }
            }

            if (snapshots == null || snapshots.Length == 0)
            {
                return;
            }

            for (int index = 0; index < snapshots.Length; index++)
            {
                CCS_ContractSnapshot snapshot = snapshots[index];
                if (snapshot == null || string.IsNullOrWhiteSpace(snapshot.contractDefinitionId))
                {
                    continue;
                }

                if (!definitionLookup.ContainsKey(snapshot.contractDefinitionId))
                {
                    continue;
                }

                ContractInstance instance = GetOrCreateInstance(snapshot.contractDefinitionId);
                instance.State = (CCS_ContractState)snapshot.contractState;
                instance.AcceptedSettlementId = snapshot.acceptedSettlementId ?? string.Empty;
            }
        }

        private ContractInstance GetOrCreateInstance(string contractId)
        {
            if (!instanceLookup.TryGetValue(contractId, out ContractInstance instance) || instance == null)
            {
                instance = new ContractInstance();
                instanceLookup[contractId] = instance;
            }

            return instance;
        }

        private bool TryFindItemDefinition(string itemId, out CCS_ItemDefinition itemDefinition)
        {
            itemDefinition = null;
            if (string.IsNullOrWhiteSpace(itemId)
                || inventoryService?.ActiveProfile?.SaveRestoreItemDefinitions == null)
            {
                return false;
            }

            CCS_ItemDefinition[] definitions = inventoryService.ActiveProfile.SaveRestoreItemDefinitions;
            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_ItemDefinition candidate = definitions[index];
                if (candidate != null
                    && string.Equals(candidate.ItemId, itemId, StringComparison.OrdinalIgnoreCase))
                {
                    itemDefinition = candidate;
                    return true;
                }
            }

            return false;
        }

        private static CCS_ContractCompletionResult Success(string contractId, string message)
        {
            return new CCS_ContractCompletionResult(true, contractId, message);
        }

        private static CCS_ContractCompletionResult Failure(string contractId, string message)
        {
            return new CCS_ContractCompletionResult(false, contractId, message);
        }

        private void LogDebug(string message)
        {
            if (activeProfile != null && activeProfile.EnableDebugLogging)
            {
                Debug.Log($"{LogPrefix} {message}");
            }
        }
    }
}
