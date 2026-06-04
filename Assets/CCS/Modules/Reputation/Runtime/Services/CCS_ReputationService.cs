using System;
using System.Collections.Generic;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_ReputationService
// CATEGORY: Modules / Reputation / Runtime / Services
// PURPOSE: Owns reputation standings, event-driven trust changes, and save/restore.
// PLACEMENT: Registered by CCS_SurvivalGameplayServiceRegistration.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 2.7.0 — no factions, quests, law, or final UI yet.
// =============================================================================

namespace CCS.Modules.Reputation
{
    public sealed class CCS_ReputationService : CCS_ISurvivalService
    {
        private const string LogPrefix = "[CCS_ReputationService]";

        private sealed class StandingInstance
        {
            public string ReputationDefinitionId = string.Empty;
            public CCS_ReputationScopeType ScopeType = CCS_ReputationScopeType.Settlement;
            public string TargetId = string.Empty;
            public int CurrentValue;
            public int MinValue = -100;
            public int MaxValue = 100;
            public string LastEventSummaryPlaceholder = string.Empty;
        }

        private readonly Dictionary<string, StandingInstance> standingsByKey =
            new Dictionary<string, StandingInstance>(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<string, CCS_ReputationDefinition> definitionLookup =
            new Dictionary<string, CCS_ReputationDefinition>(StringComparer.OrdinalIgnoreCase);

        private CCS_ReputationProfile activeProfile;
        private CCS_ReputationEvent lastEvent;
        private bool isInitialized;

        public event Action<CCS_ReputationChangedEventArgs> ReputationChanged;

        public bool IsInitialized => isInitialized;

        public CCS_ReputationProfile ActiveProfile => activeProfile;

        public CCS_ReputationEvent LastEvent => lastEvent;

        public void Initialize()
        {
            isInitialized = true;
        }

        public void InitializeFromProfile(CCS_ReputationProfile profile)
        {
            activeProfile = profile;
            definitionLookup.Clear();
            standingsByKey.Clear();
            lastEvent = null;

            if (profile == null)
            {
                isInitialized = false;
                return;
            }

            CCS_SurvivalValidationResult validation = CCS_ReputationValidationUtility.ValidateProfile(profile);
            if (!validation.IsSuccess)
            {
                Debug.LogWarning($"{LogPrefix} Profile validation warning: {validation.Message}");
            }

            CCS_ReputationDefinition[] definitions = profile.ReputationDefinitions;
            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_ReputationDefinition definition = definitions[index];
                if (definition == null || string.IsNullOrWhiteSpace(definition.ReputationDefinitionId))
                {
                    continue;
                }

                definitionLookup[definition.ReputationDefinitionId] = definition;
                EnsureStandingForDefinition(definition);
            }

            isInitialized = validation.IsSuccess || definitionLookup.Count > 0;
        }

        public bool TryGetStanding(
            CCS_ReputationScopeType scopeType,
            string targetId,
            out CCS_ReputationStanding standing)
        {
            standing = null;
            if (!TryGetStandingInstance(scopeType, targetId, out StandingInstance instance))
            {
                return false;
            }

            standing = BuildStanding(instance);
            return true;
        }

        public bool TryGetSettlementStanding(string settlementId, out CCS_ReputationStanding standing)
        {
            return TryGetStanding(CCS_ReputationScopeType.Settlement, settlementId, out standing);
        }

        public static CCS_ReputationTier ResolveTier(int value)
        {
            if (value <= -60)
            {
                return CCS_ReputationTier.Hostile;
            }

            if (value <= -20)
            {
                return CCS_ReputationTier.Distrusted;
            }

            if (value < 20)
            {
                return CCS_ReputationTier.Neutral;
            }

            if (value < 60)
            {
                return CCS_ReputationTier.Trusted;
            }

            return CCS_ReputationTier.Honored;
        }

        public bool TryApplyGoodsSold(string settlementId)
        {
            if (!isInitialized || activeProfile == null || !activeProfile.EnableGoodsSoldEvents)
            {
                return false;
            }

            return TryApplyDelta(
                CCS_ReputationEventType.GoodsSold,
                CCS_ReputationScopeType.Settlement,
                ResolveSettlementId(settlementId),
                activeProfile.GoodsSoldDelta,
                "Goods sold increased settlement trust.");
        }

        public bool TryApplyLoanRepaid(string settlementId)
        {
            if (!isInitialized || activeProfile == null || !activeProfile.EnableLoanRepaidEvents)
            {
                return false;
            }

            return TryApplyDelta(
                CCS_ReputationEventType.LoanRepaid,
                CCS_ReputationScopeType.Settlement,
                ResolveSettlementId(settlementId),
                activeProfile.LoanRepaidDelta,
                "Loan repaid increased settlement trust.");
        }

        public bool TryApplyUpkeepPaid(string settlementId)
        {
            if (!isInitialized || activeProfile == null || !activeProfile.EnableUpkeepPaidEvents)
            {
                return false;
            }

            return TryApplyDelta(
                CCS_ReputationEventType.UpkeepPaid,
                CCS_ReputationScopeType.Settlement,
                ResolveSettlementId(settlementId),
                activeProfile.UpkeepPaidDelta,
                "Upkeep paid increased settlement trust.");
        }

        public bool TryApplyFailedUpkeep(string settlementId)
        {
            if (!isInitialized || activeProfile == null || !activeProfile.EnableFailedUpkeepEvents)
            {
                return false;
            }

            int delta = activeProfile.FailedUpkeepDelta;
            if (delta == 0)
            {
                return false;
            }

            return TryApplyDelta(
                CCS_ReputationEventType.FailedUpkeep,
                CCS_ReputationScopeType.Settlement,
                ResolveSettlementId(settlementId),
                delta,
                "Failed upkeep decreased settlement trust.");
        }

        public bool TryApplySettlementDiscovered(string settlementId)
        {
            if (!isInitialized || activeProfile == null || !activeProfile.EnableSettlementDiscoveredEvents)
            {
                return false;
            }

            string resolvedSettlementId = ResolveSettlementId(settlementId);
            EnsureStandingForScope(CCS_ReputationScopeType.Settlement, resolvedSettlementId);
            return TryApplyDelta(
                CCS_ReputationEventType.SettlementDiscovered,
                CCS_ReputationScopeType.Settlement,
                resolvedSettlementId,
                activeProfile.SettlementDiscoveredDelta,
                "Settlement discovery registered trust baseline.");
        }

        public bool TryApplyContractReward(string settlementId, int reputationGain)
        {
            if (!isInitialized
                || activeProfile == null
                || !activeProfile.EnableContractCompletedEvents
                || reputationGain == 0)
            {
                return false;
            }

            string resolvedSettlementId = ResolveSettlementId(settlementId);
            EnsureStandingForScope(CCS_ReputationScopeType.Settlement, resolvedSettlementId);
            return TryApplyDelta(
                CCS_ReputationEventType.ContractCompleted,
                CCS_ReputationScopeType.Settlement,
                resolvedSettlementId,
                reputationGain,
                "Frontier contract completed.");
        }

        public bool TryResolveSettlementForVendor(string vendorId, out string settlementId)
        {
            settlementId = ResolveSettlementId(string.Empty);
            if (string.IsNullOrWhiteSpace(vendorId))
            {
                return false;
            }

            return !string.IsNullOrWhiteSpace(settlementId);
        }

        public CCS_ReputationSnapshot[] CaptureReputationState()
        {
            if (standingsByKey.Count == 0)
            {
                return Array.Empty<CCS_ReputationSnapshot>();
            }

            CCS_ReputationSnapshot[] snapshots = new CCS_ReputationSnapshot[standingsByKey.Count];
            int index = 0;
            foreach (KeyValuePair<string, StandingInstance> entry in standingsByKey)
            {
                StandingInstance instance = entry.Value;
                if (instance == null)
                {
                    continue;
                }

                snapshots[index++] = BuildSnapshot(instance);
            }

            if (index < snapshots.Length)
            {
                Array.Resize(ref snapshots, index);
            }

            return snapshots;
        }

        public void RestoreState(CCS_ReputationSnapshot[] snapshots)
        {
            if (activeProfile == null)
            {
                return;
            }

            CCS_ReputationDefinition[] definitions = activeProfile.ReputationDefinitions;
            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_ReputationDefinition definition = definitions[index];
                if (definition != null)
                {
                    EnsureStandingForDefinition(definition);
                }
            }

            if (snapshots == null || snapshots.Length == 0)
            {
                return;
            }

            for (int index = 0; index < snapshots.Length; index++)
            {
                CCS_ReputationSnapshot snapshot = snapshots[index];
                if (snapshot == null || string.IsNullOrWhiteSpace(snapshot.targetId))
                {
                    continue;
                }

                CCS_ReputationScopeType scopeType = Enum.IsDefined(typeof(CCS_ReputationScopeType), snapshot.scopeType)
                    ? (CCS_ReputationScopeType)snapshot.scopeType
                    : CCS_ReputationScopeType.Settlement;

                StandingInstance instance = GetOrCreateStandingInstance(
                    snapshot.reputationDefinitionId,
                    scopeType,
                    snapshot.targetId,
                    snapshot.minValue,
                    snapshot.maxValue,
                    snapshot.currentValue);
                instance.CurrentValue = Clamp(
                    snapshot.currentValue,
                    instance.MinValue,
                    instance.MaxValue);
                instance.LastEventSummaryPlaceholder = snapshot.lastEventSummaryPlaceholder ?? string.Empty;
            }
        }

        private bool TryApplyDelta(
            CCS_ReputationEventType eventType,
            CCS_ReputationScopeType scopeType,
            string targetId,
            int delta,
            string message)
        {
            if (delta == 0 || string.IsNullOrWhiteSpace(targetId))
            {
                return false;
            }

            if (!EnsureStandingForScope(scopeType, targetId, out StandingInstance instance))
            {
                return false;
            }

            int previousValue = instance.CurrentValue;
            CCS_ReputationTier previousTier = ResolveTier(previousValue);
            instance.CurrentValue = Clamp(previousValue + delta, instance.MinValue, instance.MaxValue);
            CCS_ReputationTier newTier = ResolveTier(instance.CurrentValue);
            instance.LastEventSummaryPlaceholder =
                $"{eventType} {delta:+0;-#} -> {instance.CurrentValue} ({newTier})";

            lastEvent = new CCS_ReputationEvent(
                eventType,
                scopeType,
                targetId,
                delta,
                instance.CurrentValue,
                newTier,
                DateTime.UtcNow.ToString("o"),
                instance.LastEventSummaryPlaceholder);

            if (activeProfile != null && activeProfile.EnableDebugLogging)
            {
                Debug.Log($"{LogPrefix} {instance.LastEventSummaryPlaceholder} target={targetId}");
            }

            ReputationChanged?.Invoke(
                new CCS_ReputationChangedEventArgs(
                    instance.ReputationDefinitionId,
                    scopeType,
                    targetId,
                    previousValue,
                    instance.CurrentValue,
                    previousTier,
                    newTier,
                    eventType,
                    message));

            return instance.CurrentValue != previousValue;
        }

        private bool EnsureStandingForScope(CCS_ReputationScopeType scopeType, string targetId)
        {
            return EnsureStandingForScope(scopeType, targetId, out _);
        }

        private bool EnsureStandingForScope(
            CCS_ReputationScopeType scopeType,
            string targetId,
            out StandingInstance instance)
        {
            instance = null;
            if (activeProfile == null || string.IsNullOrWhiteSpace(targetId))
            {
                return false;
            }

            if (scopeType == CCS_ReputationScopeType.Settlement
                && activeProfile.TryGetDefaultSettlementReputation(out CCS_ReputationDefinition definition))
            {
                EnsureStandingForDefinition(definition);
                return TryGetStandingInstance(scopeType, targetId, out instance);
            }

            return TryGetStandingInstance(scopeType, targetId, out instance);
        }

        private void EnsureStandingForDefinition(CCS_ReputationDefinition definition)
        {
            if (definition == null || !definition.Enabled)
            {
                return;
            }

            string targetId = string.IsNullOrWhiteSpace(definition.TargetId)
                ? ResolveSettlementId(string.Empty)
                : definition.TargetId;

            GetOrCreateStandingInstance(
                definition.ReputationDefinitionId,
                definition.ScopeType,
                targetId,
                definition.MinValue,
                definition.MaxValue,
                definition.DefaultValue);
        }

        private StandingInstance GetOrCreateStandingInstance(
            string reputationDefinitionId,
            CCS_ReputationScopeType scopeType,
            string targetId,
            int minValue,
            int maxValue,
            int defaultValue)
        {
            string key = BuildStandingKey(scopeType, targetId, reputationDefinitionId);
            if (standingsByKey.TryGetValue(key, out StandingInstance existing) && existing != null)
            {
                return existing;
            }

            if (string.IsNullOrWhiteSpace(reputationDefinitionId)
                && activeProfile != null
                && activeProfile.TryGetDefaultSettlementReputation(out CCS_ReputationDefinition definition))
            {
                reputationDefinitionId = definition.ReputationDefinitionId;
                minValue = definition.MinValue;
                maxValue = definition.MaxValue;
                defaultValue = definition.DefaultValue;
            }

            StandingInstance instance = new StandingInstance
            {
                ReputationDefinitionId = reputationDefinitionId ?? string.Empty,
                ScopeType = scopeType,
                TargetId = targetId ?? string.Empty,
                MinValue = minValue,
                MaxValue = maxValue,
                CurrentValue = Clamp(defaultValue, minValue, maxValue)
            };
            standingsByKey[key] = instance;
            return instance;
        }

        private bool TryGetStandingInstance(
            CCS_ReputationScopeType scopeType,
            string targetId,
            out StandingInstance instance)
        {
            instance = null;
            if (string.IsNullOrWhiteSpace(targetId))
            {
                return false;
            }

            if (activeProfile != null
                && activeProfile.TryGetDefaultSettlementReputation(out CCS_ReputationDefinition definition))
            {
                string key = BuildStandingKey(scopeType, targetId, definition.ReputationDefinitionId);
                if (standingsByKey.TryGetValue(key, out instance) && instance != null)
                {
                    return true;
                }
            }

            foreach (KeyValuePair<string, StandingInstance> entry in standingsByKey)
            {
                StandingInstance candidate = entry.Value;
                if (candidate != null
                    && candidate.ScopeType == scopeType
                    && string.Equals(candidate.TargetId, targetId, StringComparison.OrdinalIgnoreCase))
                {
                    instance = candidate;
                    return true;
                }
            }

            return false;
        }

        private string ResolveSettlementId(string settlementId)
        {
            if (!string.IsNullOrWhiteSpace(settlementId))
            {
                return settlementId;
            }

            return activeProfile != null
                ? activeProfile.DefaultTradingPostSettlementId
                : CCS_ReputationContentIds.DefaultTradingPostSettlementId;
        }

        private static string BuildStandingKey(
            CCS_ReputationScopeType scopeType,
            string targetId,
            string reputationDefinitionId)
        {
            return $"{(int)scopeType}:{targetId}:{reputationDefinitionId}";
        }

        private static int Clamp(int value, int minValue, int maxValue)
        {
            return Mathf.Clamp(value, minValue, maxValue);
        }

        private static CCS_ReputationStanding BuildStanding(StandingInstance instance)
        {
            return new CCS_ReputationStanding(
                instance.ReputationDefinitionId,
                instance.ScopeType,
                instance.TargetId,
                instance.CurrentValue,
                instance.MinValue,
                instance.MaxValue,
                ResolveTier(instance.CurrentValue));
        }

        private static CCS_ReputationSnapshot BuildSnapshot(StandingInstance instance)
        {
            return new CCS_ReputationSnapshot
            {
                reputationDefinitionId = instance.ReputationDefinitionId,
                scopeType = (int)instance.ScopeType,
                targetId = instance.TargetId,
                currentValue = instance.CurrentValue,
                minValue = instance.MinValue,
                maxValue = instance.MaxValue,
                displayTier = (int)ResolveTier(instance.CurrentValue),
                lastEventSummaryPlaceholder = instance.LastEventSummaryPlaceholder
            };
        }
    }
}
