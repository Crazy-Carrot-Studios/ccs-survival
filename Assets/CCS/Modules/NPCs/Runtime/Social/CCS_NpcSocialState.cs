using System;

// =============================================================================
// SCRIPT: CCS_NpcSocialState
// CATEGORY: Modules / NPCs / Runtime / Social
// PURPOSE: Persisted NPC social gathering metadata for leisure-period groups.
// PLACEMENT: Stored on CCS_SettlementSimulationState.npcSocialStates.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 5.0.0 — transforms are not persisted; groups rebuilt after load.
// =============================================================================

namespace CCS.Modules.NPCs
{
    [Serializable]
    public sealed class CCS_NpcSocialState
    {
        public string npcIdentityId = string.Empty;

        public string settlementId = string.Empty;

        public string groupId = string.Empty;

        public string anchorId = string.Empty;

        public int lastEvaluatedHour = -1;

        public bool isSocializing;
    }
}
