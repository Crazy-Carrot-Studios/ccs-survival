using System;
using CCS.Modules.Settlements;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_NpcServiceRepresentativeDefinition
// CATEGORY: Modules / NPCs / Runtime / Data
// PURPOSE: Maps an active business to a service point and representative role/title.
// PLACEMENT: Serialized on CCS_NpcServiceRepresentativeProfile.
// AUTHOR: James Schilz
// CREATED: 2026-06-05
// NOTES: Milestone 4.3.0 — business/service routing without duplicate service logic.
// =============================================================================

namespace CCS.Modules.NPCs
{
    [Serializable]
    public sealed class CCS_NpcServiceRepresentativeDefinition
    {
        [SerializeField] private string representativeId = string.Empty;

        [SerializeField] private string settlementId = string.Empty;

        [SerializeField] private string businessId = string.Empty;

        [SerializeField] private string servicePointId = string.Empty;

        [SerializeField] private CCS_NpcRoleType requiredRole = CCS_NpcRoleType.Unknown;

        [SerializeField] private string displayTitle = string.Empty;

        [SerializeField] private bool fallbackToServicePoint = true;

        [SerializeField] private string populationPresenceAnchorId = string.Empty;

        public string RepresentativeId => representativeId ?? string.Empty;

        public string SettlementId => settlementId ?? string.Empty;

        public string BusinessId => businessId ?? string.Empty;

        public string ServicePointId => servicePointId ?? string.Empty;

        public CCS_NpcRoleType RequiredRole => requiredRole;

        public string DisplayTitle => displayTitle ?? string.Empty;

        public bool FallbackToServicePoint => fallbackToServicePoint;

        public string PopulationPresenceAnchorId => populationPresenceAnchorId ?? string.Empty;

        public bool MatchesBusiness(string candidateSettlementId, string candidateBusinessId)
        {
            if (string.IsNullOrWhiteSpace(candidateBusinessId))
            {
                return false;
            }

            if (!string.Equals(businessId, candidateBusinessId, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(settlementId)
                && !string.Equals(settlementId, candidateSettlementId, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return true;
        }
    }
}
