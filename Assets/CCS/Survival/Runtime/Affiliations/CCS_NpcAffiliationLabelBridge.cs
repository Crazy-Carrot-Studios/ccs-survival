using System;

// =============================================================================
// SCRIPT: CCS_NpcAffiliationLabelBridge
// CATEGORY: Survival / Runtime / Affiliations
// PURPOSE: Decouples settlement placeholder labels from NPC affiliation module types.
// PLACEMENT: Wired by CCS_NpcAffiliationService; consumed by placeholder actors.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.8.0 — avoids Settlements -> NPCs assembly reference cycle.
// =============================================================================

namespace CCS.Survival
{
    public static class CCS_NpcAffiliationLabelBridge
    {
        public static Func<string, string, string> ResolveSettlementDisplayLine;

        public static Func<string, string, string> ResolveAffiliationDebugLine;

        public static Func<string, string, string> ResolveAffiliationDetailDebugLine;

        public static string BuildSettlementDisplayLine(string settlementId, string npcIdentityId)
        {
            if (ResolveSettlementDisplayLine == null
                || string.IsNullOrWhiteSpace(settlementId)
                || string.IsNullOrWhiteSpace(npcIdentityId))
            {
                return string.Empty;
            }

            return ResolveSettlementDisplayLine.Invoke(settlementId, npcIdentityId) ?? string.Empty;
        }

        public static string BuildAffiliationDebugLine(string settlementId, string npcIdentityId)
        {
            if (ResolveAffiliationDebugLine == null
                || string.IsNullOrWhiteSpace(settlementId)
                || string.IsNullOrWhiteSpace(npcIdentityId))
            {
                return string.Empty;
            }

            return ResolveAffiliationDebugLine.Invoke(settlementId, npcIdentityId) ?? string.Empty;
        }

        public static string BuildAffiliationDetailDebugLine(string settlementId, string npcIdentityId)
        {
            if (ResolveAffiliationDetailDebugLine == null
                || string.IsNullOrWhiteSpace(settlementId)
                || string.IsNullOrWhiteSpace(npcIdentityId))
            {
                return string.Empty;
            }

            return ResolveAffiliationDetailDebugLine.Invoke(settlementId, npcIdentityId) ?? string.Empty;
        }
    }
}
