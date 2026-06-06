using System;
using System.Collections.Generic;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SettlementHousingRuntimeBridge
// CATEGORY: Modules / Settlements / Runtime / Services
// PURPOSE: Static bridge for housing anchors to resolve housing snapshots safely.
// PLACEMENT: Wired by CCS_SurvivalGameplayServiceRegistration.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.4.0 — markers initialize safely when services are missing.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public static class CCS_SettlementHousingRuntimeBridge
    {
        private static readonly Dictionary<string, CCS_SettlementHousingAnchor> AnchorLookup =
            new Dictionary<string, CCS_SettlementHousingAnchor>(StringComparer.OrdinalIgnoreCase);

        public static Func<string, CCS_SettlementHousingSnapshot> ResolveHousingSnapshot;

        public static void RegisterAnchor(CCS_SettlementHousingAnchor anchor)
        {
            if (anchor == null || string.IsNullOrWhiteSpace(anchor.AnchorId))
            {
                return;
            }

            AnchorLookup[anchor.AnchorId] = anchor;
        }

        public static void UnregisterAnchor(CCS_SettlementHousingAnchor anchor)
        {
            if (anchor == null || string.IsNullOrWhiteSpace(anchor.AnchorId))
            {
                return;
            }

            if (AnchorLookup.TryGetValue(anchor.AnchorId, out CCS_SettlementHousingAnchor existing)
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

            foreach (KeyValuePair<string, CCS_SettlementHousingAnchor> pair in AnchorLookup)
            {
                CCS_SettlementHousingAnchor anchor = pair.Value;
                if (anchor != null
                    && string.Equals(anchor.SettlementId, settlementId, StringComparison.OrdinalIgnoreCase))
                {
                    anchor.RefreshFromHousingState();
                }
            }
        }

        public static void RefreshAllAnchors()
        {
            foreach (KeyValuePair<string, CCS_SettlementHousingAnchor> pair in AnchorLookup)
            {
                pair.Value?.RefreshFromHousingState();
            }
        }

        public static int GetRegisteredAnchorCount()
        {
            return AnchorLookup.Count;
        }

        public static bool TryGetHousingSnapshot(string settlementId, out CCS_SettlementHousingSnapshot snapshot)
        {
            snapshot = CCS_SettlementHousingSnapshot.Empty;
            if (ResolveHousingSnapshot == null || string.IsNullOrWhiteSpace(settlementId))
            {
                return false;
            }

            snapshot = ResolveHousingSnapshot.Invoke(settlementId) ?? CCS_SettlementHousingSnapshot.Empty;
            return snapshot.IsValid;
        }

        public static CCS_SettlementHousingStatus ResolveHousingStatus(string settlementId, string housingId)
        {
            if (!TryGetHousingSnapshot(settlementId, out CCS_SettlementHousingSnapshot snapshot)
                || snapshot.HousingEntries == null)
            {
                return CCS_SettlementHousingStatus.Locked;
            }

            for (int index = 0; index < snapshot.HousingEntries.Length; index++)
            {
                CCS_SettlementHousingEntry entry = snapshot.HousingEntries[index];
                if (entry != null
                    && string.Equals(entry.HousingId, housingId, StringComparison.OrdinalIgnoreCase))
                {
                    return entry.Status;
                }
            }

            return CCS_SettlementHousingStatus.Locked;
        }

        public static bool TryFindAnchorForHousing(string settlementId, string housingId, out CCS_SettlementHousingAnchor anchor)
        {
            anchor = null;
            if (string.IsNullOrWhiteSpace(settlementId) || string.IsNullOrWhiteSpace(housingId))
            {
                return false;
            }

            foreach (KeyValuePair<string, CCS_SettlementHousingAnchor> pair in AnchorLookup)
            {
                CCS_SettlementHousingAnchor candidate = pair.Value;
                if (candidate != null
                    && string.Equals(candidate.SettlementId, settlementId, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(candidate.HousingId, housingId, StringComparison.OrdinalIgnoreCase))
                {
                    anchor = candidate;
                    return true;
                }
            }

            return false;
        }
    }
}
