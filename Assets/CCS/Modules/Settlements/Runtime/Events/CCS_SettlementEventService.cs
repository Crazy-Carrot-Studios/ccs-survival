using System;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SettlementEventService
// CATEGORY: Modules / Settlements / Runtime / Events
// PURPOSE: Simulation-driven settlement event generation, modifiers, and presentation.
// PLACEMENT: Registered by CCS_SurvivalGameplayServiceRegistration.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 5.1.0 — metadata and temporary modifiers only; no quests or AI.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public sealed class CCS_SettlementEventService : CCS_ISurvivalService
    {
        private const string LogPrefix = "[CCS_SettlementEventService]";

        private CCS_SettlementEventProfile activeProfile;
        private Func<string, CCS_SettlementEventState> getEventState;
        private Action<string, CCS_SettlementEventState> setEventState;
        private Func<string, CCS_SettlementEventSimulationContext> getSimulationContext;
        private Func<string, CCS_SettlementType> resolveSettlementType;
        private Func<CCS_SettlementEventTimeSnapshot> resolveCurrentTime;
        private bool isInitialized;

        public bool IsInitialized => isInitialized;

        public CCS_SettlementEventProfile ActiveProfile => activeProfile;

        public void Initialize()
        {
            if (isInitialized)
            {
                return;
            }

            isInitialized = true;
        }

        public void InitializeFromProfile(CCS_SettlementEventProfile profile)
        {
            activeProfile = profile;
            isInitialized = profile != null;
            if (profile == null)
            {
                return;
            }

            CCS_SurvivalValidationResult validation = CCS_SettlementEventValidationUtility.ValidateProfile(profile);
            if (!validation.IsSuccess)
            {
                Debug.LogWarning($"{LogPrefix} Profile validation warning: {validation.Message}");
            }

            BindRuntimeBridge();
        }

        public void BindEventStateAccessors(
            Func<string, CCS_SettlementEventState> getter,
            Action<string, CCS_SettlementEventState> setter,
            Func<string, CCS_SettlementEventSimulationContext> simulationContextResolver,
            Func<string, CCS_SettlementType> settlementTypeResolver,
            Func<CCS_SettlementEventTimeSnapshot> currentTimeResolver)
        {
            getEventState = getter;
            setEventState = setter;
            getSimulationContext = simulationContextResolver;
            resolveSettlementType = settlementTypeResolver;
            resolveCurrentTime = currentTimeResolver;
            BindRuntimeBridge();
        }

        public bool TryGetActiveSnapshot(string settlementId, out CCS_SettlementEventSnapshot snapshot)
        {
            snapshot = CCS_SettlementEventSnapshot.Empty;
            if (string.IsNullOrWhiteSpace(settlementId) || activeProfile == null)
            {
                return false;
            }

            CCS_SettlementEventState state = getEventState?.Invoke(settlementId);
            if (state == null || !state.isActive)
            {
                return false;
            }

            if (!activeProfile.TryGetDefinitionById(state.activeEventId, out CCS_SettlementEventDefinition definition)
                || definition == null)
            {
                return false;
            }

            snapshot = CCS_SettlementEventValidationUtility.BuildSnapshot(state, definition);
            return snapshot.IsValid;
        }

        public bool TryForceEvent(string settlementId, CCS_SettlementEventType eventType)
        {
            if (activeProfile == null || string.IsNullOrWhiteSpace(settlementId))
            {
                return false;
            }

            if (!activeProfile.TryGetDefinitionForType(eventType, settlementId, out CCS_SettlementEventDefinition definition)
                || definition == null)
            {
                return false;
            }

            return ActivateEvent(settlementId, definition);
        }

        public void EvaluateSettlement(string settlementId)
        {
            if (activeProfile == null || string.IsNullOrWhiteSpace(settlementId))
            {
                return;
            }

            CCS_SettlementEventTimeSnapshot timeSnapshot =
                resolveCurrentTime?.Invoke() ?? CCS_SettlementEventTimeSnapshot.Default;
            CCS_SettlementEventState currentState = getEventState?.Invoke(settlementId);
            if (currentState != null
                && currentState.isActive
                && !CCS_SettlementEventValidationUtility.IsEventExpired(
                    currentState,
                    timeSnapshot.DayNumber,
                    timeSnapshot.Hour))
            {
                RefreshPresentation(settlementId);
                return;
            }

            if (currentState != null && currentState.isActive)
            {
                ClearEvent(settlementId);
            }

            if (!activeProfile.AllowSimulationGeneration)
            {
                return;
            }

            CCS_SettlementEventSimulationContext simulationContext =
                getSimulationContext?.Invoke(settlementId) ?? CCS_SettlementEventSimulationContext.Empty;
            if (!simulationContext.IsDiscovered)
            {
                return;
            }

            CCS_SettlementType settlementType = resolveSettlementType?.Invoke(settlementId) ?? CCS_SettlementType.Other;
            if (!CCS_SettlementEventValidationUtility.TryResolveEligibleDefinition(
                    activeProfile,
                    settlementId,
                    settlementType,
                    simulationContext.Population,
                    simulationContext.Prosperity,
                    simulationContext.ActiveBusinessCount,
                    simulationContext.TradeRouteUsageCount,
                    out CCS_SettlementEventDefinition definition)
                || definition == null)
            {
                return;
            }

            ActivateEvent(settlementId, definition);
        }

        public void RefreshAllPresentation()
        {
            CCS_SettlementEventAnchorRuntimeBridge.RefreshAllAnchors();
        }

        private bool ActivateEvent(string settlementId, CCS_SettlementEventDefinition definition)
        {
            CCS_SettlementEventTimeSnapshot timeSnapshot =
                resolveCurrentTime?.Invoke() ?? CCS_SettlementEventTimeSnapshot.Default;
            CCS_SettlementEventState state = CCS_SettlementEventValidationUtility.CreateActiveState(
                definition,
                settlementId,
                timeSnapshot.DayNumber,
                timeSnapshot.Hour);
            setEventState?.Invoke(settlementId, state);
            RefreshPresentation(settlementId);
            if (TryGetActiveSnapshot(settlementId, out CCS_SettlementEventSnapshot snapshot))
            {
                CCS_SettlementEventRuntimeBridge.LastEventSnapshot = snapshot;
            }

            return true;
        }

        private void ClearEvent(string settlementId)
        {
            setEventState?.Invoke(
                settlementId,
                CCS_SettlementEventValidationUtility.CreateInactiveState(settlementId));
            RefreshPresentation(settlementId);
        }

        private void RefreshPresentation(string settlementId)
        {
            if (TryGetActiveSnapshot(settlementId, out CCS_SettlementEventSnapshot snapshot))
            {
                CCS_SettlementEventRuntimeBridge.LastEventSnapshot = snapshot;
            }

            RefreshAllPresentation();
        }

        private void BindRuntimeBridge()
        {
            CCS_SettlementEventRuntimeBridge.ResolveActiveEvent = settlementId =>
            {
                TryGetActiveSnapshot(settlementId, out CCS_SettlementEventSnapshot snapshot);
                return snapshot ?? CCS_SettlementEventSnapshot.Empty;
            };
            CCS_SettlementEventRuntimeBridge.TryForceEventForPlaytest = (settlementId) =>
                TryForceEvent(settlementId, CCS_SettlementEventType.MarketDay);
            CCS_SettlementEventRuntimeBridge.RefreshAllEventPresentation = RefreshAllPresentation;
            CCS_SettlementEventRuntimeBridge.ResolveProsperityBonus = settlementId =>
            {
                return TryGetActiveSnapshot(settlementId, out CCS_SettlementEventSnapshot snapshot)
                    && snapshot != null
                    && snapshot.IsValid
                    ? snapshot.ProsperityBonus
                    : 0f;
            };
            CCS_SettlementEventRuntimeBridge.ResolveSupplyBonus = settlementId =>
            {
                return TryGetActiveSnapshot(settlementId, out CCS_SettlementEventSnapshot snapshot)
                    && snapshot != null
                    && snapshot.IsValid
                    ? snapshot.SupplyBonus
                    : 0f;
            };
            CCS_SettlementEventRuntimeBridge.ResolveContractRewardMultiplier = settlementId =>
            {
                return TryGetActiveSnapshot(settlementId, out CCS_SettlementEventSnapshot snapshot)
                    && snapshot != null
                    && snapshot.IsValid
                    ? snapshot.ContractRewardMultiplier
                    : 1f;
            };
            CCS_SettlementEventRuntimeBridge.ResolveReputationGainMultiplier = settlementId =>
            {
                return TryGetActiveSnapshot(settlementId, out CCS_SettlementEventSnapshot snapshot)
                    && snapshot != null
                    && snapshot.IsValid
                    ? snapshot.ReputationGainMultiplier
                    : 1f;
            };
            CCS_SettlementEventRuntimeBridge.ResolveDialogueAppendLine = settlementId =>
            {
                return TryGetActiveSnapshot(settlementId, out CCS_SettlementEventSnapshot snapshot)
                    && snapshot != null
                    && snapshot.IsValid
                    ? snapshot.DialogueAppendLine
                    : string.Empty;
            };
            CCS_SettlementEventRuntimeBridge.ResolvePreferredSocialAnchorId = settlementId =>
            {
                return TryGetActiveSnapshot(settlementId, out CCS_SettlementEventSnapshot snapshot)
                    && snapshot != null
                    && snapshot.IsValid
                    ? snapshot.PreferredSocialAnchorId
                    : string.Empty;
            };
        }
    }
}
