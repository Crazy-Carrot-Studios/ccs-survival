using System;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SettlementHousingService
// CATEGORY: Modules / Settlements / Runtime / Services
// PURPOSE: Hosts housing profile, syncs persisted states, and refreshes world markers.
// PLACEMENT: Registered by CCS_SurvivalGameplayServiceRegistration.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.4.0 — housing capacity contributes to settlement population cap.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public sealed class CCS_SettlementHousingService : CCS_ISurvivalService
    {
        private const string LogPrefix = "[CCS_SettlementHousingService]";

        private CCS_SettlementHousingProfile activeProfile;
        private Func<string, CCS_SettlementHousingState[]> getHousingStates;
        private Action<string, CCS_SettlementHousingState[]> setHousingStates;
        private Func<string, CCS_SettlementGrowthSnapshot> growthSnapshotResolver;
        private Func<string, bool> settlementDiscoveredResolver;
        private Func<string, float> prosperityResolver;
        private Func<CCS_SettlementPopulationProfile> populationProfileResolver;
        private Action<string> populationMetricsRefreshCallback;
        private bool isInitialized;

        public bool IsInitialized => isInitialized;

        public CCS_SettlementHousingProfile ActiveProfile => activeProfile;

        public void Initialize()
        {
            if (isInitialized)
            {
                return;
            }

            isInitialized = true;
        }

        public void InitializeFromProfile(CCS_SettlementHousingProfile profile)
        {
            activeProfile = profile;
            isInitialized = profile != null;
            if (profile == null)
            {
                return;
            }

            CCS_SurvivalValidationResult validation =
                CCS_SettlementHousingValidationUtility.ValidateProfile(profile);
            if (!validation.IsSuccess)
            {
                Debug.LogWarning($"{LogPrefix} Profile validation warning: {validation.Message}");
            }
        }

        public void BindHousingStateAccessors(
            Func<string, CCS_SettlementHousingState[]> getter,
            Action<string, CCS_SettlementHousingState[]> setter)
        {
            getHousingStates = getter;
            setHousingStates = setter;
            BindRuntimeBridge();
        }

        public void BindGrowthSnapshotResolver(Func<string, CCS_SettlementGrowthSnapshot> resolver)
        {
            growthSnapshotResolver = resolver;
        }

        public void BindSettlementDiscoveredResolver(Func<string, bool> resolver)
        {
            settlementDiscoveredResolver = resolver;
        }

        public void BindProsperityResolver(Func<string, float> resolver)
        {
            prosperityResolver = resolver;
        }

        public void BindPopulationProfileResolver(Func<CCS_SettlementPopulationProfile> resolver)
        {
            populationProfileResolver = resolver;
        }

        public void BindPopulationMetricsRefreshCallback(Action<string> callback)
        {
            populationMetricsRefreshCallback = callback;
        }

        public bool TryGetHousingSnapshot(string settlementId, out CCS_SettlementHousingSnapshot snapshot)
        {
            snapshot = CCS_SettlementHousingSnapshot.Empty;
            if (activeProfile == null || string.IsNullOrWhiteSpace(settlementId))
            {
                return false;
            }

            if (!TryGetGrowthSnapshot(settlementId, out CCS_SettlementGrowthSnapshot growthSnapshot))
            {
                growthSnapshot = CCS_SettlementGrowthSnapshot.Empty;
            }

            bool discovered = IsSettlementDiscovered(settlementId);
            float prosperity = ResolveProsperity(settlementId);
            CCS_SettlementHousingState[] states =
                getHousingStates?.Invoke(settlementId) ?? Array.Empty<CCS_SettlementHousingState>();
            snapshot = CCS_SettlementHousingValidationUtility.BuildSnapshot(
                settlementId,
                prosperity,
                populationProfileResolver?.Invoke(),
                states,
                activeProfile,
                discovered,
                growthSnapshot.CurrentGrowthStage);
            return snapshot.IsValid;
        }

        public void HandleSettlementGrowthChanged(CCS_SettlementGrowthChangedEventArgs eventArgs)
        {
            if (eventArgs?.Snapshot == null)
            {
                return;
            }

            SyncSettlementHousing(eventArgs.Snapshot.SettlementId);
        }

        public void HandleSettlementDiscovered(CCS_SettlementSnapshot settlementSnapshot)
        {
            if (settlementSnapshot == null)
            {
                return;
            }

            SyncSettlementHousing(settlementSnapshot.SettlementId);
        }

        public void SyncSettlementHousing(string settlementId)
        {
            if (activeProfile == null || string.IsNullOrWhiteSpace(settlementId) || setHousingStates == null)
            {
                return;
            }

            if (!TryGetGrowthSnapshot(settlementId, out CCS_SettlementGrowthSnapshot growthSnapshot))
            {
                growthSnapshot = CCS_SettlementGrowthSnapshot.Empty;
            }

            bool discovered = IsSettlementDiscovered(settlementId);
            CCS_SettlementHousingState[] states =
                getHousingStates?.Invoke(settlementId) ?? Array.Empty<CCS_SettlementHousingState>();
            CCS_SettlementHousingState[] synced = CCS_SettlementHousingValidationUtility.SyncHousingStates(
                settlementId,
                states,
                activeProfile,
                growthSnapshot.CurrentGrowthStage,
                discovered);
            setHousingStates.Invoke(settlementId, synced);
            populationMetricsRefreshCallback?.Invoke(settlementId);
            RefreshSettlement(settlementId);
        }

        public void RefreshAllAnchors()
        {
            CCS_SettlementHousingRuntimeBridge.RefreshAllAnchors();
        }

        public void RefreshSettlement(string settlementId)
        {
            CCS_SettlementHousingRuntimeBridge.RefreshSettlement(settlementId);
        }

        private void BindRuntimeBridge()
        {
            CCS_SettlementHousingRuntimeBridge.ResolveHousingSnapshot = ResolveSnapshot;
        }

        private CCS_SettlementHousingSnapshot ResolveSnapshot(string settlementId)
        {
            TryGetHousingSnapshot(settlementId, out CCS_SettlementHousingSnapshot snapshot);
            return snapshot;
        }

        private bool TryGetGrowthSnapshot(string settlementId, out CCS_SettlementGrowthSnapshot snapshot)
        {
            snapshot = CCS_SettlementGrowthSnapshot.Empty;
            if (string.IsNullOrWhiteSpace(settlementId) || growthSnapshotResolver == null)
            {
                return false;
            }

            snapshot = growthSnapshotResolver.Invoke(settlementId) ?? CCS_SettlementGrowthSnapshot.Empty;
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

        private float ResolveProsperity(string settlementId)
        {
            if (string.IsNullOrWhiteSpace(settlementId) || prosperityResolver == null)
            {
                return 0f;
            }

            return prosperityResolver.Invoke(settlementId);
        }
    }
}
