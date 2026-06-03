using System;

// =============================================================================
// SCRIPT: CCS_UpkeepEntry
// CATEGORY: Modules / Upkeep / Runtime / Data
// PURPOSE: Serializable upkeep entry tracking owner, target, due state, and payment history placeholder.
// PLACEMENT: Used by CCS_UpkeepService and CCS_SaveUpkeepWorldData.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 2.5.0 tax and upkeep foundation.
// =============================================================================

namespace CCS.Modules.Upkeep
{
    [Serializable]
    public sealed class CCS_UpkeepEntry
    {
        public string entryId = string.Empty;
        public string ownerId = string.Empty;
        public string targetId = string.Empty;
        public int targetType;
        public string upkeepDefinitionId = string.Empty;
        public int amountDue;
        public int lastPaidDay;
        public int nextDueDay;
        public int status;
        public string lastTransactionSummary = string.Empty;
    }
}
