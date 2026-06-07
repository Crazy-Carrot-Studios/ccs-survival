using System;

// =============================================================================
// SCRIPT: CCS_NpcAffiliationState
// CATEGORY: Modules / NPCs / Runtime / Affiliations
// PURPOSE: Persisted NPC settlement/business/workforce affiliation metadata.
// PLACEMENT: Stored on CCS_SettlementSimulationState.npcAffiliationStates.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.8.0 — transforms are not persisted.
// =============================================================================

namespace CCS.Modules.NPCs
{
    [Serializable]
    public sealed class CCS_NpcAffiliationState
    {
        public string npcIdentityId = string.Empty;

        public string settlementId = string.Empty;

        public string regionId = string.Empty;

        public string businessId = string.Empty;

        public int workforceCategory;

        public bool isServiceRepresentative;

        public int loyaltyValue = 50;
    }
}
