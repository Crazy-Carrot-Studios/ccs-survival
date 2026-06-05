using System;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SettlementVisualGrowthService
// CATEGORY: Modules / Settlements / Runtime / Services
// PURPOSE: Hosts visual growth profile and refreshes world markers from growth stage.
// PLACEMENT: Registered by CCS_SurvivalGameplayServiceRegistration.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.9.0 — visual state derived from settlement growth snapshots.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public sealed class CCS_SettlementVisualGrowthService : CCS_ISurvivalService
    {
        private const string LogPrefix = "[CCS_SettlementVisualGrowthService]";

        private CCS_SettlementVisualGrowthProfile activeProfile;
        private Func<string, CCS_SettlementGrowthSnapshot> growthSnapshotResolver;
        private Func<string, bool> settlementDiscoveredResolver;
        private bool isInitialized;

        public bool IsInitialized => isInitialized;

        public CCS_SettlementVisualGrowthProfile ActiveProfile => activeProfile;

        public void Initialize()
        {
            if (isInitialized)
            {
                return;
            }

            isInitialized = true;
        }

        public void InitializeFromProfile(CCS_SettlementVisualGrowthProfile profile)
        {
            activeProfile = profile;
            isInitialized = profile != null;
            if (profile == null)
            {
                return;
            }

            CCS_SurvivalValidationResult validation =
                CCS_SettlementVisualGrowthValidationUtility.ValidateProfile(profile);
            if (!validation.IsSuccess)
            {
                Debug.LogWarning($"{LogPrefix} Profile validation warning: {validation.Message}");
            }
        }

        public void BindGrowthSnapshotResolver(Func<string, CCS_SettlementGrowthSnapshot> resolver)
        {
            growthSnapshotResolver = resolver;
            CCS_SettlementVisualGrowthRuntimeBridge.ResolveGrowthSnapshot = ResolveSnapshot;
        }

        public void BindSettlementDiscoveredResolver(Func<string, bool> resolver)
        {
            settlementDiscoveredResolver = resolver;
            CCS_SettlementVisualGrowthRuntimeBridge.ResolveSettlementDiscovered = resolver;
        }

        public bool TryGetVisualGrowthSnapshot(
            string settlementId,
            out CCS_SettlementVisualGrowthSnapshot snapshot)
        {
            snapshot = CCS_SettlementVisualGrowthSnapshot.Empty;
            if (activeProfile == null || string.IsNullOrWhiteSpace(settlementId))
            {
                return false;
            }

            if (!TryGetGrowthSnapshot(settlementId, out CCS_SettlementGrowthSnapshot growthSnapshot))
            {
                growthSnapshot = CCS_SettlementGrowthSnapshot.Empty;
            }

            bool discovered = IsSettlementDiscovered(settlementId);
            snapshot = CCS_SettlementVisualGrowthValidationUtility.BuildSnapshot(
                settlementId,
                growthSnapshot,
                discovered,
                activeProfile.AnchorDefinitions);
            return snapshot.IsValid;
        }

        public void HandleSettlementGrowthChanged(CCS_SettlementGrowthChangedEventArgs eventArgs)
        {
            if (eventArgs?.Snapshot == null)
            {
                return;
            }

            RefreshSettlement(eventArgs.Snapshot.SettlementId);
            ApplySettlementLocationVisual(eventArgs.Snapshot);
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
            CCS_SettlementVisualGrowthRuntimeBridge.RefreshAllAnchors();
        }

        public void RefreshSettlement(string settlementId)
        {
            CCS_SettlementVisualGrowthRuntimeBridge.RefreshSettlement(settlementId);
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

        private CCS_SettlementGrowthSnapshot ResolveSnapshot(string settlementId)
        {
            TryGetGrowthSnapshot(settlementId, out CCS_SettlementGrowthSnapshot snapshot);
            return snapshot;
        }

        private static void ApplySettlementLocationVisual(CCS_SettlementGrowthSnapshot snapshot)
        {
            if (snapshot == null || !snapshot.IsValid)
            {
                return;
            }

            CCS_SettlementLocation[] locations =
                CCS_SurvivalSceneQueryUtility.FindActiveObjectsByType<CCS_SettlementLocation>();
            if (locations == null)
            {
                return;
            }

            for (int index = 0; index < locations.Length; index++)
            {
                CCS_SettlementLocation location = locations[index];
                if (location?.SettlementDefinition == null
                    || !string.Equals(
                        location.SettlementDefinition.SettlementId,
                        snapshot.SettlementId,
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                location.ApplyGrowthStageVisual(snapshot.CurrentGrowthStage);
            }
        }
    }
}
