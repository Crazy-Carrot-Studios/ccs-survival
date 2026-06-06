using System;
using CCS.Modules.Settlements;

// =============================================================================
// SCRIPT: CCS_NpcIdentityState
// CATEGORY: Modules / NPCs / Runtime / Data
// PURPOSE: Persisted NPC identity for one population placeholder slot.
// PLACEMENT: Stored on CCS_SettlementSimulationState.npcIdentityStates.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 4.1.0 — save/load via world simulation; no transform persistence.
// =============================================================================

namespace CCS.Modules.NPCs
{
    [Serializable]
    public sealed class CCS_NpcIdentityState
    {
        public string npcIdentityId = string.Empty;

        public string displayName = string.Empty;

        public int roleType;

        public string settlementId = string.Empty;

        public string businessId = string.Empty;

        public int workforceCategory;

        public string anchorId = string.Empty;

        public int slotIndex;

        public string homeHousingId = string.Empty;

        public CCS_NpcRoleType ResolvedRoleType =>
            Enum.IsDefined(typeof(CCS_NpcRoleType), roleType)
                ? (CCS_NpcRoleType)roleType
                : CCS_NpcRoleType.Unknown;

        public CCS_SettlementPopulationCategory ResolvedWorkforceCategory =>
            Enum.IsDefined(typeof(CCS_SettlementPopulationCategory), workforceCategory)
                ? (CCS_SettlementPopulationCategory)workforceCategory
                : CCS_SettlementPopulationCategory.Unknown;
    }
}
