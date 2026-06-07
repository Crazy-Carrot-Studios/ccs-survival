using System;
using CCS.Modules.Settlements;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_NpcScheduleRoleMapping
// CATEGORY: Modules / NPCs / Runtime / Schedules
// PURPOSE: Maps role, business, workforce, or representative flag to a schedule id.
// PLACEMENT: Serialized on CCS_NpcScheduleProfile.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.6.0 — first matching mapping wins during evaluation.
// =============================================================================

namespace CCS.Modules.NPCs
{
    [Serializable]
    public sealed class CCS_NpcScheduleRoleMapping
    {
        [SerializeField] private CCS_NpcRoleType roleType = CCS_NpcRoleType.Unknown;

        [SerializeField] private string businessId = string.Empty;

        [SerializeField] private CCS_SettlementPopulationCategory workforceCategory =
            CCS_SettlementPopulationCategory.Unknown;

        [SerializeField] private bool requiresServiceRepresentative;

        [SerializeField] private string scheduleId = string.Empty;

        public CCS_NpcRoleType RoleType => roleType;

        public string BusinessId => businessId ?? string.Empty;

        public CCS_SettlementPopulationCategory WorkforceCategory => workforceCategory;

        public bool RequiresServiceRepresentative => requiresServiceRepresentative;

        public string ScheduleId => scheduleId ?? string.Empty;

        public bool Matches(
            CCS_NpcRoleType candidateRole,
            string candidateBusinessId,
            CCS_SettlementPopulationCategory candidateCategory,
            bool isServiceRepresentative)
        {
            if (requiresServiceRepresentative && !isServiceRepresentative)
            {
                return false;
            }

            if (roleType != CCS_NpcRoleType.Unknown && roleType != candidateRole)
            {
                return false;
            }

            if (workforceCategory != CCS_SettlementPopulationCategory.Unknown
                && workforceCategory != candidateCategory)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(businessId)
                && !string.Equals(businessId, candidateBusinessId, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return !string.IsNullOrWhiteSpace(scheduleId);
        }
    }
}
