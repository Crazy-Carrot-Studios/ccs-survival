using System;
using System.Collections.Generic;

// =============================================================================
// SCRIPT: CCS_PopulationPresenceRuntimeBridge
// CATEGORY: Modules / Settlements / Runtime / Services
// PURPOSE: Static bridge for population presence anchors to resolve snapshots safely.
// PLACEMENT: Wired by CCS_SurvivalGameplayServiceRegistration.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 4.0.0 — anchors initialize safely when services are missing.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public static class CCS_PopulationPresenceRuntimeBridge
    {
        private static readonly Dictionary<string, CCS_PopulationPresenceAnchor> AnchorLookup =
            new Dictionary<string, CCS_PopulationPresenceAnchor>(StringComparer.OrdinalIgnoreCase);

        public static Func<string, CCS_SettlementPopulationSnapshot> ResolvePopulationSnapshot;

        public static Func<string, CCS_SettlementGrowthSnapshot> ResolveGrowthSnapshot;

        public static Func<string, bool> ResolveSettlementDiscovered;

        public static void RegisterAnchor(CCS_PopulationPresenceAnchor anchor)
        {
            if (anchor == null || string.IsNullOrWhiteSpace(anchor.AnchorId))
            {
                return;
            }

            AnchorLookup[anchor.AnchorId] = anchor;
        }

        public static void UnregisterAnchor(CCS_PopulationPresenceAnchor anchor)
        {
            if (anchor == null || string.IsNullOrWhiteSpace(anchor.AnchorId))
            {
                return;
            }

            if (AnchorLookup.TryGetValue(anchor.AnchorId, out CCS_PopulationPresenceAnchor existing)
                && existing == anchor)
            {
                AnchorLookup.Remove(anchor.AnchorId);
            }
        }

        public static void RefreshSettlement(string settlementId)
        {
            if (string.IsNullOrWhiteSpace(settlementId))
            {
                return;
            }

            foreach (KeyValuePair<string, CCS_PopulationPresenceAnchor> pair in AnchorLookup)
            {
                CCS_PopulationPresenceAnchor anchor = pair.Value;
                if (anchor != null
                    && string.Equals(anchor.SettlementId, settlementId, StringComparison.OrdinalIgnoreCase))
                {
                    anchor.RefreshFromPopulationState();
                }
            }
        }

        public static void RefreshAllAnchors()
        {
            foreach (KeyValuePair<string, CCS_PopulationPresenceAnchor> pair in AnchorLookup)
            {
                pair.Value?.RefreshFromPopulationState();
            }
        }

        public static int GetRegisteredAnchorCount()
        {
            return AnchorLookup.Count;
        }

        public static int GetSpawnedActorCount(
            string settlementId,
            CCS_SettlementPopulationCategory category)
        {
            foreach (KeyValuePair<string, CCS_PopulationPresenceAnchor> pair in AnchorLookup)
            {
                CCS_PopulationPresenceAnchor anchor = pair.Value;
                if (anchor != null
                    && string.Equals(anchor.SettlementId, settlementId, StringComparison.OrdinalIgnoreCase)
                    && anchor.WorkforceCategory == category)
                {
                    return anchor.GetSpawnedActorCount();
                }
            }

            return 0;
        }

        public static int ResolveWorkforceCount(
            string settlementId,
            CCS_SettlementPopulationCategory category)
        {
            CCS_SettlementPopulationSnapshot snapshot = GetPopulationSnapshotForSettlement(settlementId);
            return CCS_PopulationPresenceValidationUtility.ResolveWorkforceCount(snapshot, category);
        }

        public static int ResolveVisibleActorCount(
            string settlementId,
            CCS_SettlementPopulationCategory category,
            int minimumPopulationCount,
            int maxVisibleActors,
            CCS_SettlementGrowthStage requiredGrowthStage)
        {
            return CCS_PopulationPresenceValidationUtility.ResolveVisibleActorCount(
                GetPopulationSnapshotForSettlement(settlementId),
                category,
                minimumPopulationCount,
                maxVisibleActors,
                IsSettlementDiscovered(settlementId),
                IsGrowthStageMet(settlementId, requiredGrowthStage));
        }

        public static bool IsSettlementDiscovered(string settlementId)
        {
            if (ResolveSettlementDiscovered == null || string.IsNullOrWhiteSpace(settlementId))
            {
                return false;
            }

            return ResolveSettlementDiscovered.Invoke(settlementId);
        }

        public static bool IsGrowthStageMet(string settlementId, CCS_SettlementGrowthStage requiredStage)
        {
            if (requiredStage == CCS_SettlementGrowthStage.Unknown)
            {
                return true;
            }

            return CCS_PopulationPresenceValidationUtility.IsGrowthStageMet(
                ResolveGrowthSnapshotForSettlement(settlementId),
                requiredStage);
        }

        public static CCS_SettlementPopulationSnapshot GetPopulationSnapshotForSettlement(string settlementId)
        {
            if (ResolvePopulationSnapshot == null || string.IsNullOrWhiteSpace(settlementId))
            {
                return CCS_SettlementPopulationSnapshot.Empty;
            }

            return ResolvePopulationSnapshot.Invoke(settlementId) ?? CCS_SettlementPopulationSnapshot.Empty;
        }

        public static CCS_SettlementGrowthSnapshot ResolveGrowthSnapshotForSettlement(string settlementId)
        {
            if (ResolveGrowthSnapshot == null || string.IsNullOrWhiteSpace(settlementId))
            {
                return CCS_SettlementGrowthSnapshot.Empty;
            }

            return ResolveGrowthSnapshot.Invoke(settlementId) ?? CCS_SettlementGrowthSnapshot.Empty;
        }

        public static bool TryFindAnchor(string anchorId, out CCS_PopulationPresenceAnchor anchor)
        {
            anchor = null;
            if (string.IsNullOrWhiteSpace(anchorId))
            {
                return false;
            }

            return AnchorLookup.TryGetValue(anchorId, out anchor) && anchor != null;
        }
    }
}
