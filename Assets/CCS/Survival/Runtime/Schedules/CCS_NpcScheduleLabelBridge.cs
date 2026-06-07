using System;

// =============================================================================
// SCRIPT: CCS_NpcScheduleLabelBridge
// CATEGORY: Survival / Runtime / Schedules
// PURPOSE: Decouples settlement placeholder labels from NPC schedule module types.
// PLACEMENT: Wired by CCS_NpcScheduleService; consumed by placeholder actors.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.6.0 — avoids Settlements -> NPCs assembly reference cycle.
// =============================================================================

namespace CCS.Survival
{
    public static class CCS_NpcScheduleLabelBridge
    {
        public static Func<string, string, string> ResolveScheduleDebugLine;

        public static string BuildScheduleDebugLine(string settlementId, string npcIdentityId)
        {
            if (ResolveScheduleDebugLine == null
                || string.IsNullOrWhiteSpace(settlementId)
                || string.IsNullOrWhiteSpace(npcIdentityId))
            {
                return string.Empty;
            }

            return ResolveScheduleDebugLine.Invoke(settlementId, npcIdentityId) ?? string.Empty;
        }
    }
}
