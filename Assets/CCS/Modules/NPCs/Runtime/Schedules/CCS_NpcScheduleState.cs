using System;

// =============================================================================
// SCRIPT: CCS_NpcScheduleState
// CATEGORY: Modules / NPCs / Runtime / Schedules
// PURPOSE: Persisted NPC schedule state for save/load and movement resync.
// PLACEMENT: Stored on CCS_SettlementSimulationState.npcScheduleStates.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.6.0 — transforms are not persisted.
// =============================================================================

namespace CCS.Modules.NPCs
{
    [Serializable]
    public sealed class CCS_NpcScheduleState
    {
        public string npcIdentityId = string.Empty;

        public string settlementId = string.Empty;

        public string activeScheduleId = string.Empty;

        public int currentBlockType = (int)CCS_NpcScheduleBlockType.Unknown;

        public int currentTargetKind = (int)CCS_NpcScheduleTargetKind.Unknown;

        public string currentTargetId = string.Empty;

        public int lastEvaluatedHour = -1;
    }
}
