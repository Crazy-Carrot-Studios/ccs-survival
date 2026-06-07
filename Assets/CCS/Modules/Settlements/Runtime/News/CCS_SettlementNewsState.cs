using System;

// =============================================================================
// SCRIPT: CCS_SettlementNewsState
// CATEGORY: Modules / Settlements / Runtime / News
// PURPOSE: Persisted settlement news entry and rumor propagation metadata.
// PLACEMENT: Stored in world simulation save payload newsEntries array.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 5.2.0 — information propagation only; no quests or politics.
// =============================================================================

namespace CCS.Modules.Settlements
{
    [Serializable]
    public sealed class CCS_SettlementNewsState
    {
        public string newsId = string.Empty;

        public string originSettlementId = string.Empty;

        public int eventType = (int)CCS_SettlementNewsType.Unknown;

        public string headline = string.Empty;

        public string rumorLine = string.Empty;

        public int dayNumber = 1;

        public int expirationDay = 1;

        public int propagationReadyDay = 1;

        public string[] knownSettlementIds = Array.Empty<string>();

        public bool isActive;
    }
}
