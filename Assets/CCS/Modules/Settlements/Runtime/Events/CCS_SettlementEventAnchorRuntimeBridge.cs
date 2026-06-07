using System;
using System.Collections.Generic;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SettlementEventAnchorRuntimeBridge
// CATEGORY: Modules / Settlements / Runtime / Events
// PURPOSE: Tracks registered settlement event anchors for marker presentation.
// PLACEMENT: Used by CCS_SettlementEventService and bootstrap scene anchors.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 5.1.0 — explicit registration only; no scene scanning.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public static class CCS_SettlementEventAnchorRuntimeBridge
    {
        private static readonly Dictionary<string, CCS_SettlementEventAnchor> AnchorLookup =
            new Dictionary<string, CCS_SettlementEventAnchor>(StringComparer.OrdinalIgnoreCase);

        public static void RegisterAnchor(CCS_SettlementEventAnchor anchor)
        {
            if (anchor == null || string.IsNullOrWhiteSpace(anchor.AnchorId))
            {
                return;
            }

            AnchorLookup[anchor.AnchorId] = anchor;
        }

        public static void UnregisterAnchor(CCS_SettlementEventAnchor anchor)
        {
            if (anchor == null || string.IsNullOrWhiteSpace(anchor.AnchorId))
            {
                return;
            }

            if (AnchorLookup.TryGetValue(anchor.AnchorId, out CCS_SettlementEventAnchor existing)
                && existing == anchor)
            {
                AnchorLookup.Remove(anchor.AnchorId);
            }
        }

        public static bool TryFindAnchor(string anchorId, out CCS_SettlementEventAnchor anchor)
        {
            anchor = null;
            if (string.IsNullOrWhiteSpace(anchorId))
            {
                return false;
            }

            return AnchorLookup.TryGetValue(anchorId, out anchor) && anchor != null;
        }

        public static void RefreshAllAnchors()
        {
            foreach (KeyValuePair<string, CCS_SettlementEventAnchor> pair in AnchorLookup)
            {
                pair.Value?.RefreshPresentation();
            }
        }
    }
}
