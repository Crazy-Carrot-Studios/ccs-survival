using System;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_NpcActivityRuntimeBridge
// CATEGORY: Modules / NPCs / Runtime / Activities
// PURPOSE: Runtime bridge for activity snapshots, refresh, and playtest forcing.
// PLACEMENT: Wired by CCS_NpcActivityService and playtest harness.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.7.0 — hosts enumerated via CCS_PopulationPlaceholderIdentityBridge.
// =============================================================================

namespace CCS.Modules.NPCs
{
    public static class CCS_NpcActivityRuntimeBridge
    {
        public static Func<string, string, CCS_NpcActivitySnapshot> ResolveActivitySnapshot;

        public static Action RefreshAllActivities;

        public static Action<string, string, CCS_NpcActivityType, int> ForceEvaluateActivity;

        public static bool TryGetActivitySnapshot(
            string settlementId,
            string npcIdentityId,
            out CCS_NpcActivitySnapshot snapshot)
        {
            snapshot = CCS_NpcActivitySnapshot.Empty;
            if (ResolveActivitySnapshot == null
                || string.IsNullOrWhiteSpace(settlementId)
                || string.IsNullOrWhiteSpace(npcIdentityId))
            {
                return false;
            }

            snapshot = ResolveActivitySnapshot.Invoke(settlementId, npcIdentityId) ?? CCS_NpcActivitySnapshot.Empty;
            return snapshot.IsValid;
        }

        public static bool TryGetFirstHostActivitySnapshot(
            string settlementId,
            out CCS_INpcMovementHost host,
            out CCS_NpcActivitySnapshot snapshot)
        {
            host = null;
            snapshot = CCS_NpcActivitySnapshot.Empty;
            bool found = false;
            CCS_INpcMovementHost resolvedHost = null;
            CCS_NpcActivitySnapshot resolvedSnapshot = CCS_NpcActivitySnapshot.Empty;
            CCS_PopulationPlaceholderIdentityBridge.ForEachMovementHost(candidate =>
            {
                if (found
                    || candidate == null
                    || !candidate.HasIdentity
                    || !string.Equals(candidate.SettlementId, settlementId, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                resolvedHost = candidate;
                TryGetActivitySnapshot(settlementId, candidate.NpcIdentityId, out resolvedSnapshot);
                found = true;
            });

            host = resolvedHost;
            snapshot = resolvedSnapshot;
            return found;
        }

        public static void RefreshAllActivityHosts()
        {
            RefreshAllActivities?.Invoke();
        }
    }
}
