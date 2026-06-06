using System;

// =============================================================================
// SCRIPT: CCS_NpcMovementState
// CATEGORY: Modules / NPCs / Runtime / Movement
// PURPOSE: Persisted NPC movement state for save/load and schedule resync.
// PLACEMENT: Stored on CCS_SettlementSimulationState.npcMovementStates.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.5.0 — transforms are not persisted.
// =============================================================================

namespace CCS.Modules.NPCs
{
    [Serializable]
    public sealed class CCS_NpcMovementState
    {
        public string npcIdentityId = string.Empty;

        public string settlementId = string.Empty;

        public int movementStatus = (int)CCS_NpcMovementStatus.Idle;

        public string targetAnchorId = string.Empty;

        public string workplaceAnchorId = string.Empty;

        public string homeHousingId = string.Empty;
    }
}
