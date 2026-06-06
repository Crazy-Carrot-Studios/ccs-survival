using System;
using CCS.Modules.Settlements;

// =============================================================================
// SCRIPT: CCS_NpcServiceRepresentativeState
// CATEGORY: Modules / NPCs / Runtime / Data
// PURPOSE: Persisted service representative assignment on settlement simulation state.
// PLACEMENT: Stored on CCS_SettlementSimulationState.npcServiceRepresentativeStates.
// AUTHOR: James Schilz
// CREATED: 2026-06-05
// NOTES: Milestone 4.3.0 — save/load via world simulation; no transform persistence.
// =============================================================================

namespace CCS.Modules.NPCs
{
    [Serializable]
    public sealed class CCS_NpcServiceRepresentativeState
    {
        public string representativeId = string.Empty;

        public string settlementId = string.Empty;

        public string businessId = string.Empty;

        public string servicePointId = string.Empty;

        public int requiredRole;

        public string assignedNpcIdentityId = string.Empty;

        public string displayTitle = string.Empty;

        public bool isActive;

        public bool fallbackToServicePoint = true;

        public CCS_NpcRoleType ResolvedRequiredRole =>
            Enum.IsDefined(typeof(CCS_NpcRoleType), requiredRole)
                ? (CCS_NpcRoleType)requiredRole
                : CCS_NpcRoleType.Unknown;
    }
}
