using System;

// =============================================================================
// SCRIPT: CCS_SettlementNewsRuntimeBridge
// CATEGORY: Modules / Settlements / Runtime / News
// PURPOSE: Runtime bridge for news queries, playtest hooks, and UI integration.
// PLACEMENT: Wired by CCS_SettlementNewsService.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 5.2.0 — used by dialogue, contract boards, and playtest harness.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public static class CCS_SettlementNewsRuntimeBridge
    {
        public static Func<string, int, CCS_SettlementNewsEntry[]> ResolveRecentNewsEntries;

        public static Func<string, string> ResolveRumorDialogueAppendLine;

        public static Func<string, CCS_SettlementEventType, bool> TryForceNewsFromEventForPlaytest;

        public static Func<bool> TryForcePropagationForPlaytest;

        public static Action RefreshNewsPresentation;

        public static CCS_SettlementNewsSnapshot LastNewsSnapshot { get; set; } = CCS_SettlementNewsSnapshot.Empty;

        public static bool TryGetRecentNews(string settlementId, int maxCount, out CCS_SettlementNewsEntry[] entries)
        {
            entries = ResolveRecentNewsEntries?.Invoke(settlementId, maxCount) ?? Array.Empty<CCS_SettlementNewsEntry>();
            return entries != null && entries.Length > 0;
        }

        public static string ResolveDialogueRumorLine(string settlementId)
        {
            return ResolveRumorDialogueAppendLine?.Invoke(settlementId) ?? string.Empty;
        }
    }
}
