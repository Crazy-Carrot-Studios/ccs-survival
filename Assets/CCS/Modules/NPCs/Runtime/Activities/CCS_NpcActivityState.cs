using System;

// =============================================================================
// SCRIPT: CCS_NpcActivityState
// CATEGORY: Modules / NPCs / Runtime / Activities
// PURPOSE: Persisted NPC activity state for save/load and label refresh.
// PLACEMENT: Stored on CCS_SettlementSimulationState.npcActivityStates.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.7.0 — transforms are not persisted.
// =============================================================================

namespace CCS.Modules.NPCs
{
    [Serializable]
    public sealed class CCS_NpcActivityState
    {
        public string npcIdentityId = string.Empty;

        public string settlementId = string.Empty;

        public int currentActivityType = (int)CCS_NpcActivityType.None;

        public int lastEvaluatedHour = -1;
    }
}
