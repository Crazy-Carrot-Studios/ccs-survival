using System;
using System.Collections.Generic;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SettlementNewsService
// CATEGORY: Modules / Settlements / Runtime / News
// PURPOSE: Event-driven settlement news generation and trade-route rumor propagation.
// PLACEMENT: Registered by CCS_SurvivalGameplayServiceRegistration.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 5.2.0 — information propagation only; no quests or politics.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public sealed class CCS_SettlementNewsService : CCS_ISurvivalService
    {
        private const string LogPrefix = "[CCS_SettlementNewsService]";

        private CCS_SettlementNewsProfile activeProfile;
        private Func<CCS_SettlementNewsState[]> getNewsStates;
        private Action<CCS_SettlementNewsState[]> setNewsStates;
        private Func<string, string> resolveSettlementDisplayName;
        private Func<CCS_TradeRouteDefinition[]> resolveTradeRouteDefinitions;
        private Func<CCS_SettlementEventTimeSnapshot> resolveCurrentTime;
        private bool isInitialized;

        public bool IsInitialized => isInitialized;

        public CCS_SettlementNewsProfile ActiveProfile => activeProfile;

        public void Initialize()
        {
            if (isInitialized)
            {
                return;
            }

            isInitialized = true;
        }

        public void InitializeFromProfile(CCS_SettlementNewsProfile profile)
        {
            activeProfile = profile;
            isInitialized = profile != null;
            if (profile == null)
            {
                return;
            }

            CCS_SurvivalValidationResult validation = CCS_SettlementNewsValidationUtility.ValidateProfile(profile);
            if (!validation.IsSuccess)
            {
                Debug.LogWarning($"{LogPrefix} Profile validation warning: {validation.Message}");
            }

            BindRuntimeBridge();
        }

        public void BindNewsStateAccessors(
            Func<CCS_SettlementNewsState[]> getter,
            Action<CCS_SettlementNewsState[]> setter,
            Func<string, string> settlementDisplayNameResolver,
            Func<CCS_TradeRouteDefinition[]> tradeRouteDefinitionsResolver,
            Func<CCS_SettlementEventTimeSnapshot> currentTimeResolver)
        {
            getNewsStates = getter;
            setNewsStates = setter;
            resolveSettlementDisplayName = settlementDisplayNameResolver;
            resolveTradeRouteDefinitions = tradeRouteDefinitionsResolver;
            resolveCurrentTime = currentTimeResolver;
            BindRuntimeBridge();
        }

        public void HandleSettlementEventActivated(string settlementId, CCS_SettlementEventSnapshot eventSnapshot)
        {
            if (activeProfile == null
                || string.IsNullOrWhiteSpace(settlementId)
                || eventSnapshot == null
                || !eventSnapshot.IsValid)
            {
                return;
            }

            CCS_SettlementNewsType newsType =
                CCS_SettlementNewsValidationUtility.MapEventTypeToNewsType(eventSnapshot.EventType);
            if (!CCS_SettlementNewsValidationUtility.IsActiveNewsType(newsType))
            {
                return;
            }

            if (!activeProfile.TryGetDefinitionForType(newsType, out CCS_SettlementNewsDefinition definition)
                || definition == null)
            {
                return;
            }

            if (HasActiveNewsForOrigin(settlementId, newsType))
            {
                return;
            }

            CCS_SettlementEventTimeSnapshot timeSnapshot =
                resolveCurrentTime?.Invoke() ?? CCS_SettlementEventTimeSnapshot.Default;
            string displayName = resolveSettlementDisplayName?.Invoke(settlementId) ?? settlementId;
            CCS_SettlementNewsState newsState = CCS_SettlementNewsValidationUtility.CreateNewsState(
                definition,
                settlementId,
                displayName,
                timeSnapshot.DayNumber);

            AddNewsState(newsState);
            EvaluatePropagation(timeSnapshot.DayNumber);
            BindRuntimeBridge();
        }

        public void EvaluatePropagation(int currentDayNumber)
        {
            if (activeProfile == null)
            {
                return;
            }

            CCS_SettlementNewsState[] states = getNewsStates?.Invoke() ?? Array.Empty<CCS_SettlementNewsState>();
            if (states.Length == 0)
            {
                return;
            }

            CCS_TradeRouteDefinition[] routeDefinitions =
                resolveTradeRouteDefinitions?.Invoke() ?? Array.Empty<CCS_TradeRouteDefinition>();
            bool changed = false;
            int safeDay = currentDayNumber < 1 ? 1 : currentDayNumber;

            for (int index = 0; index < states.Length; index++)
            {
                CCS_SettlementNewsState state = states[index];
                if (state == null || !state.isActive)
                {
                    continue;
                }

                if (CCS_SettlementNewsValidationUtility.IsNewsExpired(state, safeDay))
                {
                    state.isActive = false;
                    changed = true;
                    continue;
                }

                if (safeDay < state.propagationReadyDay)
                {
                    continue;
                }

                string[] connectedSettlementIds = CCS_SettlementNewsValidationUtility.ResolveConnectedSettlementIds(
                    state.originSettlementId,
                    routeDefinitions);
                for (int connectedIndex = 0; connectedIndex < connectedSettlementIds.Length; connectedIndex++)
                {
                    string connectedSettlementId = connectedSettlementIds[connectedIndex];
                    if (CCS_SettlementNewsValidationUtility.IsSettlementAwareOfNews(state, connectedSettlementId))
                    {
                        continue;
                    }

                    CCS_SettlementNewsValidationUtility.AddKnownSettlement(state, connectedSettlementId);
                    changed = true;
                }
            }

            if (changed)
            {
                setNewsStates?.Invoke(states);
            }
        }

        public CCS_SettlementNewsEntry[] GetRecentNewsEntries(string settlementId, int maxCount)
        {
            if (activeProfile == null || string.IsNullOrWhiteSpace(settlementId) || maxCount < 1)
            {
                return Array.Empty<CCS_SettlementNewsEntry>();
            }

            CCS_SettlementEventTimeSnapshot timeSnapshot =
                resolveCurrentTime?.Invoke() ?? CCS_SettlementEventTimeSnapshot.Default;
            CCS_SettlementNewsState[] states = getNewsStates?.Invoke() ?? Array.Empty<CCS_SettlementNewsState>();
            List<CCS_SettlementNewsEntry> entries = new List<CCS_SettlementNewsEntry>(maxCount);

            for (int index = 0; index < states.Length; index++)
            {
                CCS_SettlementNewsState state = states[index];
                if (state == null
                    || !state.isActive
                    || CCS_SettlementNewsValidationUtility.IsNewsExpired(state, timeSnapshot.DayNumber)
                    || !CCS_SettlementNewsValidationUtility.IsSettlementAwareOfNews(state, settlementId))
                {
                    continue;
                }

                CCS_SettlementNewsSnapshot snapshot =
                    CCS_SettlementNewsValidationUtility.BuildSnapshot(state, settlementId);
                CCS_SettlementNewsEntry entry = CCS_SettlementNewsEntry.FromSnapshot(snapshot);
                if (entry != null)
                {
                    entries.Add(entry);
                }
            }

            entries.Sort((left, right) => right.DayNumber.CompareTo(left.DayNumber));
            if (entries.Count > maxCount)
            {
                return entries.GetRange(0, maxCount).ToArray();
            }

            return entries.ToArray();
        }

        public string ResolveRumorDialogueLine(string settlementId)
        {
            if (string.IsNullOrWhiteSpace(settlementId))
            {
                return string.Empty;
            }

            CCS_SettlementNewsEntry[] entries = GetRecentNewsEntries(
                settlementId,
                activeProfile != null ? activeProfile.MaxRecentNewsEntries : 3);
            for (int index = 0; index < entries.Length; index++)
            {
                CCS_SettlementNewsEntry entry = entries[index];
                if (entry == null
                    || string.IsNullOrWhiteSpace(entry.RumorLine)
                    || string.Equals(entry.OriginSettlementId, settlementId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                return entry.RumorLine;
            }

            return string.Empty;
        }

        public bool TryForceNewsFromEvent(string settlementId, CCS_SettlementEventType eventType)
        {
            if (string.IsNullOrWhiteSpace(settlementId))
            {
                return false;
            }

            CCS_SettlementNewsType newsType = CCS_SettlementNewsValidationUtility.MapEventTypeToNewsType(eventType);
            if (!CCS_SettlementNewsValidationUtility.IsActiveNewsType(newsType)
                || !activeProfile.TryGetDefinitionForType(newsType, out CCS_SettlementNewsDefinition definition)
                || definition == null)
            {
                return false;
            }

            if (HasActiveNewsForOrigin(settlementId, newsType))
            {
                return true;
            }

            CCS_SettlementEventTimeSnapshot timeSnapshot =
                resolveCurrentTime?.Invoke() ?? CCS_SettlementEventTimeSnapshot.Default;
            string displayName = resolveSettlementDisplayName?.Invoke(settlementId) ?? settlementId;
            CCS_SettlementNewsState newsState = CCS_SettlementNewsValidationUtility.CreateNewsState(
                definition,
                settlementId,
                displayName,
                timeSnapshot.DayNumber);
            AddNewsState(newsState);
            CCS_SettlementNewsRuntimeBridge.LastNewsSnapshot =
                CCS_SettlementNewsValidationUtility.BuildSnapshot(newsState, settlementId);
            return true;
        }

        public bool TryForcePropagationForPlaytest()
        {
            CCS_SettlementEventTimeSnapshot timeSnapshot =
                resolveCurrentTime?.Invoke() ?? CCS_SettlementEventTimeSnapshot.Default;
            CCS_SettlementNewsState[] states = getNewsStates?.Invoke() ?? Array.Empty<CCS_SettlementNewsState>();
            if (states.Length == 0)
            {
                return false;
            }

            bool changed = false;
            for (int index = 0; index < states.Length; index++)
            {
                CCS_SettlementNewsState state = states[index];
                if (state == null || !state.isActive)
                {
                    continue;
                }

                if (state.propagationReadyDay > timeSnapshot.DayNumber)
                {
                    state.propagationReadyDay = timeSnapshot.DayNumber;
                    changed = true;
                }
            }

            if (changed)
            {
                setNewsStates?.Invoke(states);
            }

            EvaluatePropagation(timeSnapshot.DayNumber);
            return true;
        }

        private void AddNewsState(CCS_SettlementNewsState newsState)
        {
            if (newsState == null || !newsState.isActive)
            {
                return;
            }

            CCS_SettlementNewsState[] existing = getNewsStates?.Invoke() ?? Array.Empty<CCS_SettlementNewsState>();
            CCS_SettlementNewsState[] expanded = new CCS_SettlementNewsState[existing.Length + 1];
            Array.Copy(existing, expanded, existing.Length);
            expanded[existing.Length] = newsState;
            setNewsStates?.Invoke(expanded);
            CCS_SettlementNewsRuntimeBridge.LastNewsSnapshot =
                CCS_SettlementNewsValidationUtility.BuildSnapshot(newsState, newsState.originSettlementId);
        }

        private bool HasActiveNewsForOrigin(string originSettlementId, CCS_SettlementNewsType newsType)
        {
            CCS_SettlementEventTimeSnapshot timeSnapshot =
                resolveCurrentTime?.Invoke() ?? CCS_SettlementEventTimeSnapshot.Default;
            CCS_SettlementNewsState[] states = getNewsStates?.Invoke() ?? Array.Empty<CCS_SettlementNewsState>();
            for (int index = 0; index < states.Length; index++)
            {
                CCS_SettlementNewsState state = states[index];
                if (state == null
                    || !state.isActive
                    || CCS_SettlementNewsValidationUtility.IsNewsExpired(state, timeSnapshot.DayNumber))
                {
                    continue;
                }

                if (string.Equals(state.originSettlementId, originSettlementId, StringComparison.OrdinalIgnoreCase)
                    && state.eventType == (int)newsType)
                {
                    return true;
                }
            }

            return false;
        }

        private void BindRuntimeBridge()
        {
            CCS_SettlementNewsRuntimeBridge.ResolveRecentNewsEntries = (settlementId, maxCount) =>
                GetRecentNewsEntries(settlementId, maxCount);
            CCS_SettlementNewsRuntimeBridge.ResolveRumorDialogueAppendLine = ResolveRumorDialogueLine;
            CCS_SettlementNewsRuntimeBridge.TryForceNewsFromEventForPlaytest = TryForceNewsFromEvent;
            CCS_SettlementNewsRuntimeBridge.TryForcePropagationForPlaytest = TryForcePropagationForPlaytest;
            CCS_SettlementNewsRuntimeBridge.RefreshNewsPresentation = () => { };
        }
    }
}
