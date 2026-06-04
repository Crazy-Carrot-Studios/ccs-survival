using System;
using System.Collections.Generic;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_TradeRouteService
// CATEGORY: Modules / Settlements / Runtime / Services
// PURPOSE: Runtime trade route discovery, active state, and usage counts for freight.
// PLACEMENT: Registered by CCS_SurvivalGameplayServiceRegistration.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.4.0 — metadata only; no caravan or auto transport simulation.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public sealed class CCS_TradeRouteService : CCS_ISurvivalService
    {
        private const string LogPrefix = "[CCS_TradeRouteService]";

        private sealed class RouteRuntimeState
        {
            public CCS_TradeRouteDefinition Definition;
            public bool IsDiscovered;
            public bool IsActive;
            public int UsageCount;
        }

        private readonly Dictionary<string, RouteRuntimeState> routeLookup =
            new Dictionary<string, RouteRuntimeState>(StringComparer.OrdinalIgnoreCase);

        private CCS_TradeRouteProfile activeProfile;
        private CCS_SettlementService settlementService;
        private bool isInitialized;

        public bool IsInitialized => isInitialized;

        public CCS_TradeRouteProfile ActiveProfile => activeProfile;

        public void Initialize()
        {
            isInitialized = true;
        }

        public void InitializeFromProfile(CCS_TradeRouteProfile profile)
        {
            activeProfile = profile;
            routeLookup.Clear();

            if (profile == null)
            {
                isInitialized = false;
                return;
            }

            CCS_SurvivalValidationResult validation = CCS_TradeRouteUtility.ValidateProfile(profile);
            if (!validation.IsSuccess)
            {
                Debug.LogWarning($"{LogPrefix} Profile validation warning: {validation.Message}");
            }

            CCS_TradeRouteDefinition[] definitions = profile.TradeRouteDefinitions;
            for (int index = 0; index < definitions.Length; index++)
            {
                RegisterDefinition(definitions[index]);
            }

            isInitialized = routeLookup.Count > 0;
        }

        public void BindSettlementService(CCS_SettlementService settlements)
        {
            if (settlementService != null)
            {
                settlementService.SettlementDiscovered -= HandleSettlementDiscovered;
            }

            settlementService = settlements;
            if (settlementService != null)
            {
                settlementService.SettlementDiscovered += HandleSettlementDiscovered;
                EvaluateAllRouteDiscovery();
            }
        }

        public bool TryGetRoute(string routeId, out CCS_TradeRouteDefinition definition)
        {
            definition = null;
            if (string.IsNullOrWhiteSpace(routeId)
                || !routeLookup.TryGetValue(routeId, out RouteRuntimeState state)
                || state?.Definition == null)
            {
                return false;
            }

            definition = state.Definition;
            return true;
        }

        public bool TryGetUsageCount(string routeId, out int usageCount)
        {
            usageCount = 0;
            if (string.IsNullOrWhiteSpace(routeId)
                || !routeLookup.TryGetValue(routeId, out RouteRuntimeState state)
                || state == null)
            {
                return false;
            }

            usageCount = state.UsageCount;
            return true;
        }

        public bool IsRouteDiscovered(string routeId)
        {
            return !string.IsNullOrWhiteSpace(routeId)
                && routeLookup.TryGetValue(routeId, out RouteRuntimeState state)
                && state != null
                && state.IsDiscovered;
        }

        public bool IsRouteActive(string routeId)
        {
            return !string.IsNullOrWhiteSpace(routeId)
                && routeLookup.TryGetValue(routeId, out RouteRuntimeState state)
                && state != null
                && state.IsActive;
        }

        public bool TryFindRouteForSettlements(
            string originSettlementId,
            string destinationSettlementId,
            out string routeId)
        {
            routeId = string.Empty;
            if (string.IsNullOrWhiteSpace(originSettlementId)
                || string.IsNullOrWhiteSpace(destinationSettlementId))
            {
                return false;
            }

            foreach (KeyValuePair<string, RouteRuntimeState> entry in routeLookup)
            {
                RouteRuntimeState state = entry.Value;
                CCS_TradeRouteDefinition definition = state?.Definition;
                if (definition == null)
                {
                    continue;
                }

                if (string.Equals(definition.OriginSettlementId, originSettlementId, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(
                        definition.DestinationSettlementId,
                        destinationSettlementId,
                        StringComparison.OrdinalIgnoreCase))
                {
                    routeId = definition.RouteId;
                    return true;
                }
            }

            return false;
        }

        public void RecordFreightUsage(string routeId)
        {
            if (string.IsNullOrWhiteSpace(routeId)
                || !routeLookup.TryGetValue(routeId, out RouteRuntimeState state)
                || state == null)
            {
                return;
            }

            state.UsageCount++;
            if (!state.IsDiscovered)
            {
                state.IsDiscovered = true;
            }

            if (!state.IsActive)
            {
                state.IsActive = true;
            }
        }

        public void RecordFreightUsageForSettlements(string originSettlementId, string destinationSettlementId)
        {
            if (TryFindRouteForSettlements(originSettlementId, destinationSettlementId, out string routeId))
            {
                RecordFreightUsage(routeId);
            }
        }

        public CCS_TradeRouteSnapshot[] CaptureRouteState()
        {
            if (routeLookup.Count == 0)
            {
                return Array.Empty<CCS_TradeRouteSnapshot>();
            }

            CCS_TradeRouteSnapshot[] snapshots = new CCS_TradeRouteSnapshot[routeLookup.Count];
            int index = 0;
            foreach (KeyValuePair<string, RouteRuntimeState> entry in routeLookup)
            {
                RouteRuntimeState state = entry.Value;
                if (state?.Definition == null)
                {
                    continue;
                }

                snapshots[index++] = CCS_TradeRouteUtility.BuildSnapshot(
                    state.Definition,
                    state.IsDiscovered,
                    state.IsActive,
                    state.UsageCount);
            }

            if (index < snapshots.Length)
            {
                Array.Resize(ref snapshots, index);
            }

            return snapshots;
        }

        public void RestoreRouteState(CCS_TradeRouteSnapshot[] snapshots)
        {
            foreach (KeyValuePair<string, RouteRuntimeState> entry in routeLookup)
            {
                RouteRuntimeState state = entry.Value;
                if (state?.Definition == null)
                {
                    continue;
                }

                state.IsDiscovered = state.Definition.StartsDiscovered;
                state.IsActive = state.Definition.StartsActive && state.IsDiscovered;
                state.UsageCount = 0;
            }

            if (snapshots == null || snapshots.Length == 0)
            {
                EvaluateAllRouteDiscovery();
                return;
            }

            for (int index = 0; index < snapshots.Length; index++)
            {
                CCS_TradeRouteSnapshot snapshot = snapshots[index];
                if (snapshot == null || string.IsNullOrWhiteSpace(snapshot.RouteId))
                {
                    continue;
                }

                if (!routeLookup.TryGetValue(snapshot.RouteId, out RouteRuntimeState state)
                    || state == null)
                {
                    continue;
                }

                state.IsDiscovered = snapshot.IsDiscovered;
                state.IsActive = snapshot.IsActive;
                state.UsageCount = snapshot.UsageCount;
            }

            EvaluateAllRouteDiscovery();
        }

        private void RegisterDefinition(CCS_TradeRouteDefinition definition)
        {
            if (definition == null || string.IsNullOrWhiteSpace(definition.RouteId))
            {
                return;
            }

            routeLookup[definition.RouteId] = new RouteRuntimeState
            {
                Definition = definition,
                IsDiscovered = definition.StartsDiscovered,
                IsActive = definition.StartsActive && definition.StartsDiscovered,
                UsageCount = 0
            };
        }

        private void HandleSettlementDiscovered(CCS_SettlementSnapshot snapshot)
        {
            if (snapshot == null || !snapshot.Discovered)
            {
                return;
            }

            EvaluateAllRouteDiscovery();
        }

        private void EvaluateAllRouteDiscovery()
        {
            foreach (KeyValuePair<string, RouteRuntimeState> entry in routeLookup)
            {
                RouteRuntimeState state = entry.Value;
                CCS_TradeRouteDefinition definition = state?.Definition;
                if (definition == null || state.IsDiscovered)
                {
                    continue;
                }

                if (AreSettlementsDiscovered(definition.OriginSettlementId, definition.DestinationSettlementId))
                {
                    state.IsDiscovered = true;
                    if (!state.IsActive && definition.StartsActive)
                    {
                        state.IsActive = true;
                    }
                }
            }
        }

        private bool AreSettlementsDiscovered(string originSettlementId, string destinationSettlementId)
        {
            return IsSettlementDiscovered(originSettlementId)
                && IsSettlementDiscovered(destinationSettlementId);
        }

        private bool IsSettlementDiscovered(string settlementId)
        {
            if (string.IsNullOrWhiteSpace(settlementId))
            {
                return false;
            }

            if (settlementService == null || !settlementService.IsInitialized)
            {
                return false;
            }

            return settlementService.TryGetSnapshot(settlementId, out CCS_SettlementSnapshot snapshot)
                && snapshot != null
                && snapshot.Discovered;
        }
    }
}
