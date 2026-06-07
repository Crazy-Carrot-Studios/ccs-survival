using System;
using CCS.Modules.Settlements;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_NpcScheduleRuntimeBridge
// CATEGORY: Modules / NPCs / Runtime / Schedules
// PURPOSE: Runtime bridge for schedule snapshots, refresh, and playtest forcing.
// PLACEMENT: Wired by CCS_NpcScheduleService and playtest harness.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.6.0 — hosts enumerated via CCS_PopulationPlaceholderIdentityBridge.
// =============================================================================

namespace CCS.Modules.NPCs
{
    public static class CCS_NpcScheduleRuntimeBridge
    {
        public static Func<string, string, CCS_NpcScheduleSnapshot> ResolveScheduleSnapshot;

        public static Action RefreshAllSchedules;

        public static Action<string, string, CCS_NpcScheduleBlockType, int> ForceEvaluateBlock;

        public static bool TryGetScheduleSnapshot(
            string settlementId,
            string npcIdentityId,
            out CCS_NpcScheduleSnapshot snapshot)
        {
            snapshot = CCS_NpcScheduleSnapshot.Empty;
            if (ResolveScheduleSnapshot == null
                || string.IsNullOrWhiteSpace(settlementId)
                || string.IsNullOrWhiteSpace(npcIdentityId))
            {
                return false;
            }

            snapshot = ResolveScheduleSnapshot.Invoke(settlementId, npcIdentityId) ?? CCS_NpcScheduleSnapshot.Empty;
            return snapshot.IsValid;
        }

        public static bool TryGetFirstHostScheduleSnapshot(
            string settlementId,
            out CCS_INpcMovementHost host,
            out CCS_NpcScheduleSnapshot snapshot)
        {
            host = null;
            snapshot = CCS_NpcScheduleSnapshot.Empty;
            bool found = false;
            CCS_INpcMovementHost resolvedHost = null;
            CCS_NpcScheduleSnapshot resolvedSnapshot = CCS_NpcScheduleSnapshot.Empty;
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
                TryGetScheduleSnapshot(settlementId, candidate.NpcIdentityId, out resolvedSnapshot);
                found = true;
            });

            host = resolvedHost;
            snapshot = resolvedSnapshot;
            return found;
        }

        public static void RefreshAllScheduleHosts()
        {
            RefreshAllSchedules?.Invoke();
        }

        public static void ForceEvaluateScheduleBlock(
            string settlementId,
            string npcIdentityId,
            CCS_NpcScheduleBlockType blockType,
            int hour)
        {
            ForceEvaluateBlock?.Invoke(settlementId, npcIdentityId, blockType, hour);
        }
    }
}
