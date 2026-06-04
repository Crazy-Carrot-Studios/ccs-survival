using System;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_BusinessPresenceService
// CATEGORY: Modules / Settlements / Runtime / Services
// PURPOSE: Hosts presence profile and refreshes world markers from business activation.
// PLACEMENT: Registered by CCS_SurvivalGameplayServiceRegistration.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.8.0 — visual state derived from business simulation snapshots.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public sealed class CCS_BusinessPresenceService : CCS_ISurvivalService
    {
        private const string LogPrefix = "[CCS_BusinessPresenceService]";

        private CCS_BusinessPresenceProfile activeProfile;
        private Func<string, CCS_BusinessSnapshot> businessSnapshotResolver;
        private bool isInitialized;

        public bool IsInitialized => isInitialized;

        public CCS_BusinessPresenceProfile ActiveProfile => activeProfile;

        public void Initialize()
        {
            if (isInitialized)
            {
                return;
            }

            isInitialized = true;
        }

        public void InitializeFromProfile(CCS_BusinessPresenceProfile profile)
        {
            activeProfile = profile;
            isInitialized = profile != null;
            if (profile == null)
            {
                return;
            }

            CCS_SurvivalValidationResult validation =
                CCS_BusinessPresenceValidationUtility.ValidateProfile(profile);
            if (!validation.IsSuccess)
            {
                Debug.LogWarning($"{LogPrefix} Profile validation warning: {validation.Message}");
            }
        }

        public void BindBusinessSnapshotResolver(Func<string, CCS_BusinessSnapshot> resolver)
        {
            businessSnapshotResolver = resolver;
            CCS_BusinessPresenceRuntimeBridge.ResolveBusinessSnapshot = ResolveSnapshot;
        }

        public bool TryGetPresenceSnapshot(string settlementId, out CCS_BusinessPresenceSnapshot snapshot)
        {
            snapshot = CCS_BusinessPresenceSnapshot.Empty;
            if (activeProfile == null || string.IsNullOrWhiteSpace(settlementId))
            {
                return false;
            }

            if (!TryGetBusinessSnapshot(settlementId, out CCS_BusinessSnapshot businessSnapshot))
            {
                return false;
            }

            snapshot = CCS_BusinessPresenceValidationUtility.BuildSnapshot(
                settlementId,
                businessSnapshot,
                activeProfile.AnchorDefinitions);
            return snapshot.IsValid;
        }

        public void HandleBusinessActivated(CCS_BusinessActivatedEventArgs eventArgs)
        {
            if (eventArgs?.Snapshot == null)
            {
                return;
            }

            RefreshSettlement(eventArgs.Snapshot.SettlementId);
        }

        public void HandleBusinessDeactivated(CCS_BusinessDeactivatedEventArgs eventArgs)
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
            CCS_BusinessPresenceRuntimeBridge.RefreshAllAnchors();
        }

        public void RefreshSettlement(string settlementId)
        {
            CCS_BusinessPresenceRuntimeBridge.RefreshSettlement(settlementId);
        }

        private bool TryGetBusinessSnapshot(string settlementId, out CCS_BusinessSnapshot snapshot)
        {
            snapshot = CCS_BusinessSnapshot.Empty;
            if (string.IsNullOrWhiteSpace(settlementId) || businessSnapshotResolver == null)
            {
                return false;
            }

            snapshot = businessSnapshotResolver.Invoke(settlementId) ?? CCS_BusinessSnapshot.Empty;
            return snapshot.IsValid;
        }

        private CCS_BusinessSnapshot ResolveSnapshot(string settlementId)
        {
            TryGetBusinessSnapshot(settlementId, out CCS_BusinessSnapshot snapshot);
            return snapshot;
        }
    }
}
