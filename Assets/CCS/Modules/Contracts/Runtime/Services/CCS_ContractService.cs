using System;
using System.Collections.Generic;
using CCS.Modules.Economy;
using CCS.Modules.Inventory;
using CCS.Modules.Regions;
using CCS.Modules.Reputation;
using CCS.Modules.Settlements;
using CCS.Modules.Storage;
using CCS.Modules.Vehicles;
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
        private CCS_RegionService regionService;
        private CCS_StorageService storageService;
        private CCS_VehicleService vehicleService;
        private CCS_TradeRouteService tradeRouteService;
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
            CCS_WorldSimulationService worldSimulation,
            CCS_RegionService regions,
            CCS_StorageService storage,
            CCS_VehicleService vehicles,
            CCS_TradeRouteService tradeRoutes)
        {
            inventoryService = inventory;
            currencyService = currency;
            reputationService = reputation;
            worldSimulationService = worldSimulation;
            regionService = regions;
            storageService = storage;
            vehicleService = vehicles;
            tradeRouteService = tradeRoutes;
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
            ResolveRegionalBoardContext(settlementId, out CCS_RegionSpecializationType regionSpecialization, out CCS_RegionProductionModifier regionModifier);

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

            results.Sort((left, right) =>
            {
                if (left == null && right == null)
                {
                    return 0;
                }

                if (left == null)
                {
                    return 1;
                }

                if (right == null)
                {
                    return -1;
                }

                return CCS_RegionEconomyUtility.CompareContractRegionalPreference(
                    left.ResolveRegionSpecialization(),
                    right.ResolveRegionSpecialization(),
                    regionSpecialization,
                    regionModifier);
            });

            return results.ToArray();
        }

        public CCS_ContractDefinition[] GetSettlementBoardContracts(string settlementId)
        {
            List<CCS_ContractDefinition> results = new List<CCS_ContractDefinition>();
            foreach (KeyValuePair<string, CCS_ContractDefinition> entry in definitionLookup)
            {
                CCS_ContractDefinition definition = entry.Value;
                if (definition == null || !definition.Enabled)
                {
                    continue;
                }

                if (ShouldShowOnSettlementBoard(definition, settlementId))
                {
                    results.Add(definition);
                }
            }

            results.Sort((left, right) => string.Compare(
                left?.DisplayName,
                right?.DisplayName,
                StringComparison.OrdinalIgnoreCase));
            return results.ToArray();
        }

        private static bool ShouldShowOnSettlementBoard(CCS_ContractDefinition definition, string settlementId)
        {
            if (definition == null || string.IsNullOrWhiteSpace(settlementId))
            {
                return false;
            }

            if (!definition.IsFreightContract)
            {
                return definition.MatchesSettlement(settlementId);
            }

            if (string.Equals(definition.FreightSourceSettlementId, settlementId, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return string.Equals(definition.FreightDestinationSettlementId, settlementId, StringComparison.OrdinalIgnoreCase);
        }

        private void ResolveRegionalBoardContext(
            string settlementId,
            out CCS_RegionSpecializationType regionSpecialization,
            out CCS_RegionProductionModifier regionModifier)
        {
            regionSpecialization = CCS_RegionSpecializationType.Unknown;
            regionModifier = null;
            if (regionService == null || !regionService.IsInitialized)
            {
                return;
            }

            if (!regionService.TryGetOwningRegionForSettlement(settlementId, out CCS_RegionDefinition definition)
                || definition == null)
            {
                return;
            }

            regionSpecialization = definition.SpecializationType;
            regionModifier = definition.ProductionModifier;
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

            if (!definition.CanAcceptAtSettlement(settlementId))
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
            return TryCompleteContract(contractId, string.Empty);
        }

        public CCS_ContractCompletionResult TryCompleteContract(string contractId, string completionSettlementId)
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

            string acceptedSettlementId = string.IsNullOrWhiteSpace(instance.AcceptedSettlementId)
                ? activeProfile.DefaultSettlementId
                : instance.AcceptedSettlementId;

            string rewardSettlementId = acceptedSettlementId;
            if (definition.IsFreightContract)
            {
                rewardSettlementId = definition.FreightDestinationSettlementId;
                if (string.IsNullOrWhiteSpace(completionSettlementId))
                {
                    return Failure(contractId, "Freight must be completed at the destination contract board.");
                }

                if (!definition.CanCompleteAtSettlement(completionSettlementId))
                {
                    return Failure(contractId, "Freight delivery must be completed at the destination settlement.");
                }
            }

            CCS_ContractRequirement[] requirements = definition.Requirements;
            for (int index = 0; index < requirements.Length; index++)
            {
                CCS_ContractRequirement requirement = requirements[index];
                if (requirement == null)
                {
                    continue;
                }

                if (!TryFindItemDefinition(requirement.ItemId, out CCS_ItemDefinition itemDefinition))
                {
                    return Failure(contractId, $"Required item not found: {requirement.ItemId}.");
                }

                if (definition.IsFreightContract)
                {
                    if (!CCS_ContractFreightUtility.TryGetOwnedQuantity(
                            definition,
                            requirement,
                            itemDefinition,
                            inventoryService,
                            storageService,
                            vehicleService,
                            out int owned,
                            out _))
                    {
                        return Failure(contractId, $"Freight cargo missing: {requirement.ItemId}.");
                    }

                    if (owned < requirement.Quantity)
                    {
                        return Failure(
                            contractId,
                            $"Need {requirement.Quantity}x {requirement.ItemId} in wagon cargo (found {owned}).");
                    }

                    continue;
                }

                if (!requirement.MatchesSettlement(acceptedSettlementId))
                {
                    return Failure(contractId, "Contract requirement settlement restriction failed.");
                }

                int inventoryOwned = inventoryService.GetQuantity(itemDefinition);
                if (inventoryOwned < requirement.Quantity)
                {
                    return Failure(
                        contractId,
                        $"Need {requirement.Quantity}x {requirement.ItemId} (owned {inventoryOwned}).");
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

                if (definition.IsFreightContract)
                {
                    if (!CCS_ContractFreightUtility.TryRemoveFreightGoods(
                            definition,
                            requirement,
                            itemDefinition,
                            inventoryService,
                            storageService,
                            vehicleService,
                            out _))
                    {
                        return Failure(contractId, "Failed to remove required freight goods.");
                    }

                    continue;
                }

                int removed = inventoryService.RemoveItem(itemDefinition, requirement.Quantity);
                if (removed < requirement.Quantity)
                {
                    return Failure(contractId, "Failed to remove required contract goods.");
                }
            }

            CCS_ContractReward reward = definition.Reward;
            int baseTradeDollars = reward.TradeDollars;
            CCS_TradeRouteFreightRewardBreakdown freightRewardBreakdown =
                CCS_TradeRouteFreightRewardBreakdown.Empty;
            if (definition.IsFreightContract && baseTradeDollars > 0)
            {
                freightRewardBreakdown = CCS_TradeRouteRewardModifierUtility.TryCalculateForLinkedRoute(
                    baseTradeDollars,
                    definition.LinkedTradeRouteId,
                    tradeRouteService);
            }

            int grantedTradeDollars = definition.IsFreightContract
                ? freightRewardBreakdown.FinalTradeDollars
                : baseTradeDollars;

            string currencyId = activeProfile?.DefaultCurrencyId ?? CCS_ContractContentIds.TradeDollarsCurrencyId;
            if (grantedTradeDollars > 0)
            {
                currencyService.AddCurrency(
                    currencyId,
                    grantedTradeDollars,
                    $"Contract reward: {definition.DisplayName}");
            }

            int reputationApplied = 0;
            int destinationReputationGain = reward.ReputationGain + freightRewardBreakdown.BonusReputation;
            if (destinationReputationGain != 0
                && reputationService != null
                && reputationService.IsInitialized
                && reputationService.TryApplyContractReward(rewardSettlementId, destinationReputationGain))
            {
                reputationApplied = destinationReputationGain;
            }

            if (definition.IsFreightContract
                && reward.OriginReputationGain != 0
                && reputationService != null
                && reputationService.IsInitialized)
            {
                reputationService.TryApplyContractReward(
                    definition.FreightSourceSettlementId,
                    reward.OriginReputationGain);
            }

            float supplyApplied = 0f;
            float prosperityApplied = 0f;
            if (worldSimulationService != null
                && worldSimulationService.IsInitialized
                && reward.SupplyAmount > 0f)
            {
                worldSimulationService.HandleContractCompleted(
                    rewardSettlementId,
                    reward.SupplyType,
                    reward.SupplyAmount,
                    reward.ProsperityGain);
                supplyApplied = reward.SupplyAmount;
                prosperityApplied = reward.ProsperityGain;
            }

            if (definition.IsFreightContract && tradeRouteService != null && tradeRouteService.IsInitialized)
            {
                if (!string.IsNullOrWhiteSpace(definition.LinkedTradeRouteId))
                {
                    tradeRouteService.RecordFreightUsage(definition.LinkedTradeRouteId);
                }
                else
                {
                    tradeRouteService.RecordFreightUsageForSettlements(
                        definition.FreightSourceSettlementId,
                        definition.FreightDestinationSettlementId);
                }
            }

            instance.State = CCS_ContractState.Completed;
            string completionLabel = definition.IsFreightContract
                ? completionSettlementId
                : acceptedSettlementId;
            CCS_ContractCompletionResult success = new CCS_ContractCompletionResult(
                true,
                contractId,
                BuildCompletionMessage(definition, grantedTradeDollars, freightRewardBreakdown),
                grantedTradeDollars,
                reputationApplied,
                prosperityApplied,
                supplyApplied,
                freightRewardBreakdown.BaseTradeDollars,
                freightRewardBreakdown.RouteMultiplier,
                freightRewardBreakdown.RiskMultiplier,
                freightRewardBreakdown.LinkedRouteId,
                freightRewardBreakdown.RiskLevel);
            LogDebug(success.Message);
            ContractCompleted?.Invoke(success);
            CCS_ContractDebugHud.NotifyContractCompleted(success, definition, completionLabel);
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

        private static string BuildCompletionMessage(
            CCS_ContractDefinition definition,
            int grantedTradeDollars,
            CCS_TradeRouteFreightRewardBreakdown breakdown)
        {
            if (definition == null || !definition.IsFreightContract || !breakdown.UsedRouteModifiers)
            {
                return $"Completed contract: {definition?.DisplayName}.";
            }

            return $"Completed freight: {definition.DisplayName}. Route {breakdown.LinkedRouteId} "
                + $"({breakdown.RiskLevel}) paid {grantedTradeDollars} "
                + $"(base {breakdown.BaseTradeDollars}, route x{breakdown.RouteMultiplier:0.##}, risk x{breakdown.RiskMultiplier:0.##}).";
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
