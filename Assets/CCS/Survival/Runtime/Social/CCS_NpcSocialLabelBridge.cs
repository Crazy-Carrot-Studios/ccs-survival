using System;

// =============================================================================
// SCRIPT: CCS_NpcSocialLabelBridge
// CATEGORY: Survival / Runtime / Social
// PURPOSE: Decouples settlement placeholder labels from NPC social module types.
// PLACEMENT: Wired by CCS_NpcSocialService; consumed by placeholder actors.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 5.0.0 — avoids Settlements -> NPCs assembly reference cycle.
// =============================================================================

namespace CCS.Survival
{
    public static class CCS_NpcSocialLabelBridge
    {
        public static Func<string, string, string> ResolveSocialDisplayLine;

        public static Func<string, string, string> ResolveSocialDebugLine;

        public static string BuildSocialDisplayLine(string settlementId, string npcIdentityId)
        {
            if (ResolveSocialDisplayLine == null
                || string.IsNullOrWhiteSpace(settlementId)
                || string.IsNullOrWhiteSpace(npcIdentityId))
            {
                return string.Empty;
            }

            return ResolveSocialDisplayLine.Invoke(settlementId, npcIdentityId) ?? string.Empty;
        }

        public static string BuildSocialDebugLine(string settlementId, string npcIdentityId)
        {
            if (ResolveSocialDebugLine == null
                || string.IsNullOrWhiteSpace(settlementId)
                || string.IsNullOrWhiteSpace(npcIdentityId))
            {
                return string.Empty;
            }

            return ResolveSocialDebugLine.Invoke(settlementId, npcIdentityId) ?? string.Empty;
        }
    }
}
