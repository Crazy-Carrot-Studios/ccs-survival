using System;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_PopulationPresenceService
// CATEGORY: Modules / Settlements / Runtime / Services
// PURPOSE: Hosts population presence profile and refreshes placeholder actors.
// PLACEMENT: Registered by CCS_SurvivalGameplayServiceRegistration.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 4.0.0 — visual actors derived from population snapshots.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public sealed class CCS_PopulationPresenceService : CCS_ISurvivalService
    {
        private const string LogPrefix = "[CCS_PopulationPresenceService]";

        private CCS_PopulationPresenceProfile activeProfile;
        private Func<string, CCS_SettlementPopulationSnapshot> populationSnapshotResolver;
        private Func<string, CCS_SettlementGrowthSnapshot> growthSnapshotResolver;
        private Func<string, bool> settlementDiscoveredResolver;
        private bool isInitialized;

        public bool IsInitialized => isInitialized;

        public CCS_PopulationPresenceProfile ActiveProfile => activeProfile;

        public void Initialize()
        {
            if (isInitialized)
            {
                return;
            }

            isInitialized = true;
        }

        public void InitializeFromProfile(CCS_PopulationPresenceProfile profile)
        {
            activeProfile = profile;
            isInitialized = profile != null;
            if (profile == null)
            {
                return;
            }

            CCS_SurvivalValidationResult validation =
                CCS_PopulationPresenceValidationUtility.ValidateProfile(profile);
            if (!validation.IsSuccess)
            {
                Debug.LogWarning($"{LogPrefix} Profile validation warning: {validation.Message}");
            }
        }

        public void BindPopulationSnapshotResolver(Func<string, CCS_SettlementPopulationSnapshot> resolver)
        {
            populationSnapshotResolver = resolver;
            CCS_PopulationPresenceRuntimeBridge.ResolvePopulationSnapshot = ResolveSnapshot;
        }

        public void BindGrowthSnapshotResolver(Func<string, CCS_SettlementGrowthSnapshot> resolver)
        {
            growthSnapshotResolver = resolver;
            CCS_PopulationPresenceRuntimeBridge.ResolveGrowthSnapshot = ResolveGrowthSnapshot;
        }

        public void BindSettlementDiscoveredResolver(Func<string, bool> resolver)
        {
            settlementDiscoveredResolver = resolver;
            CCS_PopulationPresenceRuntimeBridge.ResolveSettlementDiscovered = resolver;
        }

        public bool TryGetPresenceSnapshot(string settlementId, out CCS_PopulationPresenceSnapshot snapshot)
        {
            snapshot = CCS_PopulationPresenceSnapshot.Empty;
            if (activeProfile == null || string.IsNullOrWhiteSpace(settlementId))
            {
                return false;
            }

            if (!TryGetPopulationSnapshot(settlementId, out CCS_SettlementPopulationSnapshot populationSnapshot))
            {
                populationSnapshot = CCS_SettlementPopulationSnapshot.Empty;
            }

            bool discovered = IsSettlementDiscovered(settlementId);
            CCS_SettlementGrowthSnapshot growthSnapshot = ResolveGrowthSnapshot(settlementId);
            snapshot = CCS_PopulationPresenceValidationUtility.BuildSnapshot(
                settlementId,
                populationSnapshot,
                growthSnapshot,
                discovered,
                activeProfile.AnchorDefinitions);
            return snapshot.IsValid;
        }

        public void HandleSettlementPopulationChanged(CCS_SettlementPopulationChangedEventArgs eventArgs)
        {
            if (eventArgs?.Snapshot == null)
            {
                return;
            }

            RefreshSettlement(eventArgs.Snapshot.SettlementId);
        }

        public void HandleSettlementDiscovered(CCS_SettlementSnapshot settlementSnapshot)
        {
            if (settlementSnapshot == null)
            {
                return;
            }

            RefreshSettlement(settlementSnapshot.SettlementId);
        }

        public void RefreshAllAnchors()
        {
            CCS_PopulationPresenceRuntimeBridge.RefreshAllAnchors();
        }

        public void RefreshSettlement(string settlementId)
        {
            CCS_PopulationPresenceRuntimeBridge.RefreshSettlement(settlementId);
        }

        private bool TryGetPopulationSnapshot(string settlementId, out CCS_SettlementPopulationSnapshot snapshot)
        {
            snapshot = CCS_SettlementPopulationSnapshot.Empty;
            if (string.IsNullOrWhiteSpace(settlementId) || populationSnapshotResolver == null)
            {
                return false;
            }

            snapshot = populationSnapshotResolver.Invoke(settlementId) ?? CCS_SettlementPopulationSnapshot.Empty;
            return snapshot.IsValid;
        }

        private bool IsSettlementDiscovered(string settlementId)
        {
            if (string.IsNullOrWhiteSpace(settlementId) || settlementDiscoveredResolver == null)
            {
                return false;
            }

            return settlementDiscoveredResolver.Invoke(settlementId);
        }

        private CCS_SettlementPopulationSnapshot ResolveSnapshot(string settlementId)
        {
            TryGetPopulationSnapshot(settlementId, out CCS_SettlementPopulationSnapshot snapshot);
            return snapshot;
        }

        private CCS_SettlementGrowthSnapshot ResolveGrowthSnapshot(string settlementId)
        {
            if (growthSnapshotResolver == null || string.IsNullOrWhiteSpace(settlementId))
            {
                return CCS_SettlementGrowthSnapshot.Empty;
            }

            return growthSnapshotResolver.Invoke(settlementId) ?? CCS_SettlementGrowthSnapshot.Empty;
        }
    }
}
