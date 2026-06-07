using System;

// =============================================================================
// SCRIPT: CCS_SettlementEventState
// CATEGORY: Modules / Settlements / Runtime / Events
// PURPOSE: Persisted active settlement event metadata for save/load restore.
// PLACEMENT: Stored on CCS_SettlementSimulationState.activeSettlementEvent.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 5.1.0 — no transform persistence; markers refresh after load.
// =============================================================================

namespace CCS.Modules.Settlements
{
    [Serializable]
    public sealed class CCS_SettlementEventState
    {
        public string activeEventId = string.Empty;

        public int eventType = (int)CCS_SettlementEventType.Unknown;

        public int startDayNumber = 1;

        public int startHour = 0;

        public int durationHours = 24;

        public string settlementId = string.Empty;

        public bool isActive;
    }
}
