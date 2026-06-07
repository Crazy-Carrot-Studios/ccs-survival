using System;
using System.Collections.Generic;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SettlementSocialRuntimeBridge
// CATEGORY: Modules / Settlements / Runtime / Social
// PURPOSE: Tracks registered settlement social anchors for leisure target resolution.
// PLACEMENT: Used by schedule validation and NPC social service.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 5.0.0 — no scene scanning; explicit anchor registration only.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public static class CCS_SettlementSocialRuntimeBridge
    {
        private static readonly Dictionary<string, CCS_SettlementSocialAnchor> AnchorLookup =
            new Dictionary<string, CCS_SettlementSocialAnchor>(StringComparer.OrdinalIgnoreCase);

        public static void RegisterAnchor(CCS_SettlementSocialAnchor anchor)
        {
            if (anchor == null || string.IsNullOrWhiteSpace(anchor.AnchorId))
            {
                return;
            }

            AnchorLookup[anchor.AnchorId] = anchor;
        }

        public static void UnregisterAnchor(CCS_SettlementSocialAnchor anchor)
        {
            if (anchor == null || string.IsNullOrWhiteSpace(anchor.AnchorId))
            {
                return;
            }

            if (AnchorLookup.TryGetValue(anchor.AnchorId, out CCS_SettlementSocialAnchor existing)
                && existing == anchor)
            {
                AnchorLookup.Remove(anchor.AnchorId);
            }
        }

        public static bool TryFindAnchor(string anchorId, out CCS_SettlementSocialAnchor anchor)
        {
            anchor = null;
            if (string.IsNullOrWhiteSpace(anchorId))
            {
                return false;
            }

            return AnchorLookup.TryGetValue(anchorId, out anchor) && anchor != null;
        }

        public static bool TryGetFirstAnchorForSettlement(
            string settlementId,
            out CCS_SettlementSocialAnchor anchor)
        {
            anchor = null;
            if (string.IsNullOrWhiteSpace(settlementId))
            {
                return false;
            }

            foreach (KeyValuePair<string, CCS_SettlementSocialAnchor> pair in AnchorLookup)
            {
                CCS_SettlementSocialAnchor candidate = pair.Value;
                if (candidate != null
                    && string.Equals(candidate.SettlementId, settlementId, StringComparison.OrdinalIgnoreCase))
                {
                    anchor = candidate;
                    return true;
                }
            }

            return false;
        }

        public static bool TryGetAnchorPosition(string anchorId, out Vector3 position)
        {
            position = Vector3.zero;
            if (!TryFindAnchor(anchorId, out CCS_SettlementSocialAnchor anchor) || anchor == null)
            {
                return false;
            }

            position = anchor.transform.position;
            return true;
        }

        public static void RefreshAllAnchors()
        {
            foreach (KeyValuePair<string, CCS_SettlementSocialAnchor> pair in AnchorLookup)
            {
                pair.Value?.RefreshPresentation();
            }
        }

        public static int GetRegisteredAnchorCount()
        {
            return AnchorLookup.Count;
        }
    }
}
