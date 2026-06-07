using System;
using System.Collections.Generic;
using CCS.Modules.Regions;
using CCS.Modules.Settlements;
using CCS.Modules.WorldSimulation;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_DynamicContractValidationUtility
// CATEGORY: Modules / Contracts / Runtime / Dynamic
// PURPOSE: Profile validation, deterministic ids, and runtime contract builders.
// PLACEMENT: Used by CCS_DynamicContractService, validators, and playtest harness.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 5.3.0 dynamic contract generation foundation.
// =============================================================================

namespace CCS.Modules.Contracts
{
    public static class CCS_DynamicContractValidationUtility
    {
        private static readonly CCS_DynamicContractGenerationSource[] ActiveGenerationSources =
        {
            CCS_DynamicContractGenerationSource.LowSettlementSupply,
            CCS_DynamicContractGenerationSource.ActiveSettlementEvent,
            CCS_DynamicContractGenerationSource.RegionalSpecialization
        };

        public static bool IsGeneratedContractId(string contractId)
        {
            return !string.IsNullOrWhiteSpace(contractId)
                && contractId.StartsWith(
                    CCS_DynamicContractContentIds.GeneratedContractIdPrefix,
                    StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsActiveGenerationSource(CCS_DynamicContractGenerationSource source)
        {
            for (int index = 0; index < ActiveGenerationSources.Length; index++)
            {
                if (ActiveGenerationSources[index] == source)
                {
                    return true;
                }
            }

            return false;
        }

        public static CCS_SurvivalValidationResult ValidateProfile(CCS_DynamicContractProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail("Dynamic contract profile is missing.");
            }

            if (string.IsNullOrWhiteSpace(profile.ProfileId))
            {
                return CCS_SurvivalValidationResult.Fail("Dynamic contract profile requires profileId.");
            }

            if (profile.MaxActiveGeneratedContractsPerSettlement < 1)
            {
                return CCS_SurvivalValidationResult.Fail("Dynamic contract profile max active per settlement must be >= 1.");
            }

            CCS_DynamicContractRule[] rules = profile.Rules;
            if (rules.Length == 0)
            {
                return CCS_SurvivalValidationResult.Fail("Dynamic contract profile requires generation rules.");
            }

            bool hasLowSupply = false;
            bool hasEvent = false;
            bool hasRegional = false;
            HashSet<string> ruleIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int index = 0; index < rules.Length; index++)
            {
                CCS_DynamicContractRule rule = rules[index];
                if (rule == null)
                {
                    return CCS_SurvivalValidationResult.Fail("Dynamic contract profile contains null rule.");
                }

                if (string.IsNullOrWhiteSpace(rule.RuleId))
                {
                    return CCS_SurvivalValidationResult.Fail("Dynamic contract rule requires ruleId.");
                }

                if (!ruleIds.Add(rule.RuleId))
                {
                    return CCS_SurvivalValidationResult.Fail($"Duplicate dynamic contract rule id: {rule.RuleId}.");
                }

                if (!rule.Enabled || rule.PlaceholderOnly)
                {
                    continue;
                }

                if (rule.RequiredItemIds.Length == 0)
                {
                    return CCS_SurvivalValidationResult.Fail($"Dynamic contract rule {rule.RuleId} requires item ids.");
                }

                if (rule.ExpirationDays < 1 || rule.CooldownDays < 1)
                {
                    return CCS_SurvivalValidationResult.Fail($"Dynamic contract rule {rule.RuleId} has invalid expiration/cooldown.");
                }

                switch (rule.GenerationSource)
                {
                    case CCS_DynamicContractGenerationSource.LowSettlementSupply:
                        hasLowSupply = true;
                        if (rule.SupplyThresholdPercent <= 0f)
                        {
                            return CCS_SurvivalValidationResult.Fail(
                                $"Dynamic contract rule {rule.RuleId} requires supply threshold percent.");
                        }

                        break;
                    case CCS_DynamicContractGenerationSource.ActiveSettlementEvent:
                        hasEvent = true;
                        if (rule.EventType == CCS_SettlementEventType.Unknown)
                        {
                            return CCS_SurvivalValidationResult.Fail(
                                $"Dynamic contract rule {rule.RuleId} requires event type.");
                        }

                        break;
                    case CCS_DynamicContractGenerationSource.RegionalSpecialization:
                        hasRegional = true;
                        if (rule.RegionSpecialization == CCS_RegionSpecializationType.Unknown)
                        {
                            return CCS_SurvivalValidationResult.Fail(
                                $"Dynamic contract rule {rule.RuleId} requires region specialization.");
                        }

                        break;
                }

                if (rule.ContractKind == CCS_DynamicContractKind.FreightDelivery
                    && string.IsNullOrWhiteSpace(rule.FreightDestinationSettlementId))
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"Dynamic contract rule {rule.RuleId} requires freight destination settlement id.");
                }
            }

            if (!hasLowSupply)
            {
                return CCS_SurvivalValidationResult.Fail("Dynamic contract profile missing active low supply rule.");
            }

            if (!hasEvent)
            {
                return CCS_SurvivalValidationResult.Fail("Dynamic contract profile missing active event rule.");
            }

            if (!hasRegional)
            {
                return CCS_SurvivalValidationResult.Fail("Dynamic contract profile missing active regional rule.");
            }

            return CCS_SurvivalValidationResult.Pass(
                $"Dynamic contract profile valid ({rules.Length} rules, max active {profile.MaxActiveGeneratedContractsPerSettlement}).");
        }

        public static string BuildGeneratedContractId(string ruleId, string settlementId, int dayNumber)
        {
            int safeDay = dayNumber < 1 ? 1 : dayNumber;
            return $"{CCS_DynamicContractContentIds.GeneratedContractIdPrefix}{ruleId}.{settlementId}.d{safeDay}";
        }

        public static bool IsContractExpired(CCS_DynamicContractState state, int currentDayNumber)
        {
            if (state == null || !state.isActive)
            {
                return true;
            }

            int safeDay = currentDayNumber < 1 ? 1 : currentDayNumber;
            return state.expirationDay > 0 && safeDay > state.expirationDay;
        }

        public static bool IsCooldownReady(
            CCS_DynamicContractRule rule,
            string settlementId,
            int currentDayNumber,
            CCS_DynamicContractRuleCooldownState[] cooldownStates)
        {
            if (rule == null || string.IsNullOrWhiteSpace(settlementId))
            {
                return false;
            }

            if (cooldownStates == null || cooldownStates.Length == 0)
            {
                return true;
            }

            for (int index = 0; index < cooldownStates.Length; index++)
            {
                CCS_DynamicContractRuleCooldownState cooldown = cooldownStates[index];
                if (cooldown == null)
                {
                    continue;
                }

                if (!string.Equals(cooldown.ruleId, rule.RuleId, StringComparison.OrdinalIgnoreCase)
                    || !string.Equals(cooldown.settlementId, settlementId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                int safeDay = currentDayNumber < 1 ? 1 : currentDayNumber;
                return safeDay - cooldown.lastGeneratedDay >= rule.CooldownDays;
            }

            return true;
        }

        public static bool HasDuplicateActiveContract(
            CCS_DynamicContractRule rule,
            string settlementId,
            CCS_DynamicContractState[] states)
        {
            if (rule == null || states == null || states.Length == 0)
            {
                return false;
            }

            string primaryItemId = rule.RequiredItemIds.Length > 0 ? rule.RequiredItemIds[0] : string.Empty;
            for (int index = 0; index < states.Length; index++)
            {
                CCS_DynamicContractState state = states[index];
                if (state == null
                    || !state.isActive
                    || state.contractState == (int)CCS_ContractState.Completed)
                {
                    continue;
                }

                if (!string.Equals(state.sourceRuleId, rule.RuleId, StringComparison.OrdinalIgnoreCase)
                    || !string.Equals(state.settlementId, settlementId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(primaryItemId))
                {
                    return true;
                }

                if (state.requirements != null && state.requirements.Length > 0
                    && string.Equals(
                        state.requirements[0].itemId,
                        primaryItemId,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public static int CountActiveGeneratedContracts(string settlementId, CCS_DynamicContractState[] states)
        {
            if (string.IsNullOrWhiteSpace(settlementId) || states == null || states.Length == 0)
            {
                return 0;
            }

            int count = 0;
            for (int index = 0; index < states.Length; index++)
            {
                CCS_DynamicContractState state = states[index];
                if (state == null
                    || !state.isActive
                    || state.contractState == (int)CCS_ContractState.Completed)
                {
                    continue;
                }

                if (string.Equals(state.settlementId, settlementId, StringComparison.OrdinalIgnoreCase))
                {
                    count++;
                }
            }

            return count;
        }

        public static CCS_DynamicContractState CreateStateFromRule(
            CCS_DynamicContractRule rule,
            string settlementId,
            int currentDayNumber,
            string linkedEventId,
            string newsHeadlineReference)
        {
            if (rule == null || string.IsNullOrWhiteSpace(settlementId))
            {
                return null;
            }

            int safeDay = currentDayNumber < 1 ? 1 : currentDayNumber;
            CCS_DynamicContractRequirementEntry[] requirementEntries =
                BuildRequirementEntries(rule.RequiredItemIds, rule.RequiredQuantities);
            CCS_DynamicContractRewardEntry rewardEntry = new CCS_DynamicContractRewardEntry
            {
                tradeDollars = rule.TradeDollars,
                reputationGain = rule.ReputationGain,
                prosperityGain = rule.ProsperityGain,
                supplyType = (int)rule.RewardSupplyType,
                supplyAmount = rule.RewardSupplyAmount
            };

            return new CCS_DynamicContractState
            {
                generatedContractId = BuildGeneratedContractId(rule.RuleId, settlementId, safeDay),
                sourceRuleId = rule.RuleId,
                settlementId = settlementId,
                generationDay = safeDay,
                expirationDay = safeDay + rule.ExpirationDays,
                contractState = (int)CCS_ContractState.Available,
                linkedEventId = linkedEventId ?? string.Empty,
                newsHeadlineReference = newsHeadlineReference ?? string.Empty,
                isActive = true,
                displayName = rule.DisplayName,
                contractType = (int)rule.ContractType,
                contractKind = (int)rule.ContractKind,
                regionSpecialization = (int)rule.RegionSpecialization,
                freightSourceSettlementId = rule.ContractKind == CCS_DynamicContractKind.FreightDelivery
                    ? settlementId
                    : string.Empty,
                freightDestinationSettlementId = rule.FreightDestinationSettlementId ?? string.Empty,
                requirements = requirementEntries,
                reward = rewardEntry
            };
        }

        public static CCS_DynamicContractSnapshot BuildSnapshot(CCS_DynamicContractState state)
        {
            if (state == null)
            {
                return null;
            }

            return new CCS_DynamicContractSnapshot
            {
                GeneratedContractId = state.generatedContractId ?? string.Empty,
                SourceRuleId = state.sourceRuleId ?? string.Empty,
                SettlementId = state.settlementId ?? string.Empty,
                GenerationDay = state.generationDay,
                ExpirationDay = state.expirationDay,
                ContractState = Enum.IsDefined(typeof(CCS_ContractState), state.contractState)
                    ? (CCS_ContractState)state.contractState
                    : CCS_ContractState.Available,
                LinkedEventId = state.linkedEventId ?? string.Empty,
                NewsHeadlineReference = state.newsHeadlineReference ?? string.Empty,
                DisplayName = state.displayName ?? string.Empty,
                IsActive = state.isActive
            };
        }

        public static CCS_ContractDefinition CreateRuntimeDefinition(CCS_DynamicContractState state)
        {
            if (state == null || string.IsNullOrWhiteSpace(state.generatedContractId))
            {
                return null;
            }

            CCS_ContractRuntimeInitData initData = BuildRuntimeInitData(state);
            CCS_ContractDefinition definition = ScriptableObject.CreateInstance<CCS_ContractDefinition>();
            definition.ApplyRuntimeInit(initData);
            return definition;
        }

        public static CCS_ContractRuntimeInitData BuildRuntimeInitData(CCS_DynamicContractState state)
        {
            CCS_ContractRequirement[] requirements = BuildRuntimeRequirements(state);
            CCS_ContractReward reward = BuildRuntimeReward(state.reward);
            bool isFreight = state.contractKind == (int)CCS_DynamicContractKind.FreightDelivery;
            CCS_ContractType contractType = Enum.IsDefined(typeof(CCS_ContractType), state.contractType)
                ? (CCS_ContractType)state.contractType
                : CCS_ContractType.TradingPostSupply;

            if (isFreight)
            {
                contractType = CCS_ContractType.FreightDelivery;
            }

            return new CCS_ContractRuntimeInitData
            {
                ContractId = state.generatedContractId,
                DisplayName = state.displayName,
                ContractType = contractType,
                RegionSpecialization = Enum.IsDefined(typeof(CCS_RegionSpecializationType), state.regionSpecialization)
                    ? (CCS_RegionSpecializationType)state.regionSpecialization
                    : CCS_RegionSpecializationType.Unknown,
                SettlementId = isFreight ? string.Empty : state.settlementId,
                Requirements = requirements,
                Reward = reward,
                FreightSourceSettlementId = state.freightSourceSettlementId ?? string.Empty,
                FreightDestinationSettlementId = state.freightDestinationSettlementId ?? string.Empty,
                PreferWagonCargo = true,
                AllowPlayerInventoryFallback = false,
                Enabled = state.isActive
            };
        }

        public static bool RuleMatchesRequest(CCS_DynamicContractRule rule, CCS_DynamicContractGenerationRequest request)
        {
            if (rule == null || request == null || !rule.Enabled || rule.PlaceholderOnly)
            {
                return false;
            }

            if (rule.GenerationSource != request.GenerationSource)
            {
                return false;
            }

            switch (rule.GenerationSource)
            {
                case CCS_DynamicContractGenerationSource.LowSettlementSupply:
                    return rule.SupplyType == request.SupplyType
                        && (request.ForceGenerationForPlaytest
                            || request.SupplyFillPercent <= rule.SupplyThresholdPercent);
                case CCS_DynamicContractGenerationSource.ActiveSettlementEvent:
                    return rule.EventType == request.EventType;
                case CCS_DynamicContractGenerationSource.RegionalSpecialization:
                    return rule.RegionSpecialization == request.RegionSpecialization;
                default:
                    return false;
            }
        }

        private static CCS_DynamicContractRequirementEntry[] BuildRequirementEntries(
            string[] itemIds,
            int[] quantities)
        {
            if (itemIds == null || itemIds.Length == 0)
            {
                return Array.Empty<CCS_DynamicContractRequirementEntry>();
            }

            CCS_DynamicContractRequirementEntry[] entries =
                new CCS_DynamicContractRequirementEntry[itemIds.Length];
            for (int index = 0; index < itemIds.Length; index++)
            {
                int quantity = quantities != null && index < quantities.Length && quantities[index] > 0
                    ? quantities[index]
                    : 1;
                entries[index] = new CCS_DynamicContractRequirementEntry
                {
                    itemId = itemIds[index] ?? string.Empty,
                    quantity = quantity
                };
            }

            return entries;
        }

        private static CCS_ContractRequirement[] BuildRuntimeRequirements(CCS_DynamicContractState state)
        {
            CCS_DynamicContractRequirementEntry[] entries =
                state?.requirements ?? Array.Empty<CCS_DynamicContractRequirementEntry>();
            if (entries.Length == 0)
            {
                return Array.Empty<CCS_ContractRequirement>();
            }

            CCS_ContractRequirement[] requirements = new CCS_ContractRequirement[entries.Length];
            string settlementRestriction = state.contractKind == (int)CCS_DynamicContractKind.FreightDelivery
                ? string.Empty
                : state.settlementId ?? string.Empty;
            for (int index = 0; index < entries.Length; index++)
            {
                CCS_DynamicContractRequirementEntry entry = entries[index];
                CCS_ContractRequirement requirement = new CCS_ContractRequirement();
                requirement.ApplyRuntimeInit(
                    entry?.itemId ?? string.Empty,
                    entry != null ? entry.quantity : 1,
                    settlementRestriction);
                requirements[index] = requirement;
            }

            return requirements;
        }

        private static CCS_ContractReward BuildRuntimeReward(CCS_DynamicContractRewardEntry rewardEntry)
        {
            CCS_ContractReward reward = new CCS_ContractReward();
            if (rewardEntry == null)
            {
                return reward;
            }

            reward.ApplyRuntimeInit(
                rewardEntry.tradeDollars,
                rewardEntry.reputationGain,
                rewardEntry.prosperityGain,
                Enum.IsDefined(typeof(CCS_SettlementSupplyType), rewardEntry.supplyType)
                    ? (CCS_SettlementSupplyType)rewardEntry.supplyType
                    : CCS_SettlementSupplyType.Food,
                rewardEntry.supplyAmount);
            return reward;
        }
    }
}
