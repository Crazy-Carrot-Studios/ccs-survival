using System;
using System.Collections.Generic;

// =============================================================================
// SCRIPT: CCS_SettlementVisualGrowthRuntimeBridge
// CATEGORY: Modules / Settlements / Runtime / Services
// PURPOSE: Static bridge for visual growth anchors to resolve growth snapshots safely.
// PLACEMENT: Wired by CCS_SurvivalGameplayServiceRegistration.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.9.0 — markers initialize safely when services are missing.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public static class CCS_SettlementVisualGrowthRuntimeBridge
    {
        private static readonly Dictionary<string, CCS_SettlementVisualGrowthAnchor> AnchorLookup =
            new Dictionary<string, CCS_SettlementVisualGrowthAnchor>(StringComparer.OrdinalIgnoreCase);

        public static Func<string, CCS_SettlementGrowthSnapshot> ResolveGrowthSnapshot;

        public static Func<string, bool> ResolveSettlementDiscovered;

        public static void RegisterAnchor(CCS_SettlementVisualGrowthAnchor anchor)
        {
            if (anchor == null || string.IsNullOrWhiteSpace(anchor.AnchorId))
            {
                return;
            }

            AnchorLookup[anchor.AnchorId] = anchor;
        }

        public static void UnregisterAnchor(CCS_SettlementVisualGrowthAnchor anchor)
        {
            if (anchor == null || string.IsNullOrWhiteSpace(anchor.AnchorId))
            {
                return;
            }

            if (AnchorLookup.TryGetValue(anchor.AnchorId, out CCS_SettlementVisualGrowthAnchor existing)
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

            foreach (KeyValuePair<string, CCS_SettlementVisualGrowthAnchor> pair in AnchorLookup)
            {
                CCS_SettlementVisualGrowthAnchor anchor = pair.Value;
                if (anchor != null
                    && string.Equals(anchor.SettlementId, settlementId, StringComparison.OrdinalIgnoreCase))
                {
                    anchor.RefreshFromGrowthState();
                }
            }
        }

        public static void RefreshAllAnchors()
        {
            foreach (KeyValuePair<string, CCS_SettlementVisualGrowthAnchor> pair in AnchorLookup)
            {
                pair.Value?.RefreshFromGrowthState();
            }
        }

        public static int GetRegisteredAnchorCount()
        {
            return AnchorLookup.Count;
        }

        public static CCS_SettlementVisualGrowthStatus ResolveVisualStatus(
            string settlementId,
            CCS_SettlementGrowthStage requiredStage)
        {
            bool discovered = IsSettlementDiscovered(settlementId);
            if (TryGetGrowthSnapshot(settlementId, out CCS_SettlementGrowthSnapshot snapshot))
            {
                return CCS_SettlementVisualGrowthValidationUtility.ResolveVisualStatus(
                    snapshot,
                    requiredStage,
                    discovered);
            }

            return discovered
                ? CCS_SettlementVisualGrowthStatus.Inactive
                : CCS_SettlementVisualGrowthStatus.Locked;
        }

        public static bool TryGetGrowthSnapshot(string settlementId, out CCS_SettlementGrowthSnapshot snapshot)
        {
            snapshot = CCS_SettlementGrowthSnapshot.Empty;
            if (ResolveGrowthSnapshot == null || string.IsNullOrWhiteSpace(settlementId))
            {
                return false;
            }

            snapshot = ResolveGrowthSnapshot.Invoke(settlementId) ?? CCS_SettlementGrowthSnapshot.Empty;
            return snapshot.IsValid;
        }

        private static bool IsSettlementDiscovered(string settlementId)
        {
            if (ResolveSettlementDiscovered == null || string.IsNullOrWhiteSpace(settlementId))
            {
                return false;
            }

            return ResolveSettlementDiscovered.Invoke(settlementId);
        }
    }
}
