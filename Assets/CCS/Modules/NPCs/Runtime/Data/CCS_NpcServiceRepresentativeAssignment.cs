using System;
using CCS.Modules.Settlements;

// =============================================================================
// SCRIPT: CCS_NpcServiceRepresentativeAssignment
// CATEGORY: Modules / NPCs / Runtime / Data
// PURPOSE: Runtime assignment payload linking business, identity, and service point.
// PLACEMENT: Built by CCS_NpcServiceRepresentativeUtility during business activation.
// AUTHOR: James Schilz
// CREATED: 2026-06-05
// NOTES: Milestone 4.3.0 — no AI movement or duplicate service routing.
// =============================================================================

namespace CCS.Modules.NPCs
{
    [Serializable]
    public sealed class CCS_NpcServiceRepresentativeAssignment
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

        public CCS_SettlementServiceRouteType ResolvedRouteType =>
            CCS_NpcServiceRepresentativeUtility.ResolveRouteType(ResolvedRequiredRole);
    }
}
