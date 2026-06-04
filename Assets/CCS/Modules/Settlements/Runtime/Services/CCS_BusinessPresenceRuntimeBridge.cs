using System;
using System.Collections.Generic;
// =============================================================================
// SCRIPT: CCS_BusinessPresenceRuntimeBridge
// CATEGORY: Modules / Settlements / Runtime / Services
// PURPOSE: Static bridge for presence anchors to resolve business snapshots safely.
// PLACEMENT: Wired by CCS_SurvivalGameplayServiceRegistration.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.8.0 — markers initialize safely when services are missing.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public static class CCS_BusinessPresenceRuntimeBridge
    {
        private static readonly Dictionary<string, CCS_BusinessPresenceAnchor> AnchorLookup =
            new Dictionary<string, CCS_BusinessPresenceAnchor>(StringComparer.OrdinalIgnoreCase);

        public static Func<string, CCS_BusinessSnapshot> ResolveBusinessSnapshot;

        public static void RegisterAnchor(CCS_BusinessPresenceAnchor anchor)
        {
            if (anchor == null || string.IsNullOrWhiteSpace(anchor.AnchorId))
            {
                return;
            }

            AnchorLookup[anchor.AnchorId] = anchor;
        }

        public static void UnregisterAnchor(CCS_BusinessPresenceAnchor anchor)
        {
            if (anchor == null || string.IsNullOrWhiteSpace(anchor.AnchorId))
            {
                return;
            }

            if (AnchorLookup.TryGetValue(anchor.AnchorId, out CCS_BusinessPresenceAnchor existing)
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

            foreach (KeyValuePair<string, CCS_BusinessPresenceAnchor> pair in AnchorLookup)
            {
                CCS_BusinessPresenceAnchor anchor = pair.Value;
                if (anchor != null
                    && string.Equals(anchor.SettlementId, settlementId, StringComparison.OrdinalIgnoreCase))
                {
                    anchor.RefreshFromBusinessState();
                }
            }
        }

        public static void RefreshAllAnchors()
        {
            foreach (KeyValuePair<string, CCS_BusinessPresenceAnchor> pair in AnchorLookup)
            {
                pair.Value?.RefreshFromBusinessState();
            }
        }

        public static int GetRegisteredAnchorCount()
        {
            return AnchorLookup.Count;
        }

        public static CCS_BusinessPresenceStatus ResolvePresenceStatus(
            string settlementId,
            CCS_BusinessType businessType)
        {
            if (TryGetBusinessSnapshot(settlementId, out CCS_BusinessSnapshot snapshot))
            {
                return CCS_BusinessPresenceValidationUtility.ResolvePresenceStatus(snapshot, businessType);
            }

            return CCS_BusinessPresenceStatus.Locked;
        }

        public static bool TryGetBusinessSnapshot(string settlementId, out CCS_BusinessSnapshot snapshot)
        {
            snapshot = CCS_BusinessSnapshot.Empty;
            if (ResolveBusinessSnapshot == null || string.IsNullOrWhiteSpace(settlementId))
            {
                return false;
            }

            snapshot = ResolveBusinessSnapshot.Invoke(settlementId) ?? CCS_BusinessSnapshot.Empty;
            return snapshot.IsValid;
        }
    }
}
