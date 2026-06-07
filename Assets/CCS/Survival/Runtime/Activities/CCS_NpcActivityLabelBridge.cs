using System;

// =============================================================================
// SCRIPT: CCS_NpcActivityLabelBridge
// CATEGORY: Survival / Runtime / Activities
// PURPOSE: Decouples settlement placeholder labels from NPC activity module types.
// PLACEMENT: Wired by CCS_NpcActivityService; consumed by placeholder actors.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.7.0 — avoids Settlements -> NPCs assembly reference cycle.
// =============================================================================

namespace CCS.Survival
{
    public static class CCS_NpcActivityLabelBridge
    {
        public static Func<string, string, string> ResolveActivityDisplayLine;

        public static Func<string, string, string> ResolveActivityDebugLine;

        public static string BuildActivityDisplayLine(string settlementId, string npcIdentityId)
        {
            if (ResolveActivityDisplayLine == null
                || string.IsNullOrWhiteSpace(settlementId)
                || string.IsNullOrWhiteSpace(npcIdentityId))
            {
                return string.Empty;
            }

            return ResolveActivityDisplayLine.Invoke(settlementId, npcIdentityId) ?? string.Empty;
        }

        public static string BuildActivityDebugLine(string settlementId, string npcIdentityId)
        {
            if (ResolveActivityDebugLine == null
                || string.IsNullOrWhiteSpace(settlementId)
                || string.IsNullOrWhiteSpace(npcIdentityId))
            {
                return string.Empty;
            }

            return ResolveActivityDebugLine.Invoke(settlementId, npcIdentityId) ?? string.Empty;
        }
    }
}
