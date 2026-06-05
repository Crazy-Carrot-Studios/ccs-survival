using System;
using CCS.Modules.Settlements;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_NpcRoleAssignment
// CATEGORY: Modules / NPCs / Runtime / Data
// PURPOSE: Maps workforce category and optional business id to an NPC role.
// PLACEMENT: Serialized on CCS_NpcIdentityProfile role mappings.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 4.1.0 — business-aware role overrides for placeholder workers.
// =============================================================================

namespace CCS.Modules.NPCs
{
    [Serializable]
    public sealed class CCS_NpcRoleAssignment
    {
        [SerializeField] private string settlementId = string.Empty;

        [SerializeField] private CCS_SettlementPopulationCategory workforceCategory =
            CCS_SettlementPopulationCategory.Unknown;

        [SerializeField] private string businessId = string.Empty;

        [SerializeField] private CCS_NpcRoleType roleType = CCS_NpcRoleType.Unknown;

        public string SettlementId => settlementId ?? string.Empty;

        public CCS_SettlementPopulationCategory WorkforceCategory => workforceCategory;

        public string BusinessId => businessId ?? string.Empty;

        public CCS_NpcRoleType RoleType => roleType;

        public bool Matches(
            string candidateSettlementId,
            CCS_SettlementPopulationCategory candidateCategory,
            string candidateBusinessId)
        {
            if (workforceCategory != candidateCategory)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(settlementId)
                && !string.Equals(settlementId, candidateSettlementId, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(businessId))
            {
                return string.Equals(businessId, candidateBusinessId, StringComparison.OrdinalIgnoreCase);
            }

            return string.IsNullOrWhiteSpace(candidateBusinessId);
        }
    }
}
