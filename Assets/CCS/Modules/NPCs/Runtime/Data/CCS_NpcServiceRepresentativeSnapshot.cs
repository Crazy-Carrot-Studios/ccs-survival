using CCS.Modules.Settlements;

// =============================================================================
// SCRIPT: CCS_NpcServiceRepresentativeSnapshot
// CATEGORY: Modules / NPCs / Runtime / Data
// PURPOSE: Runtime read-only service representative resolved for interaction/debug HUD.
// PLACEMENT: Built by CCS_NpcServiceRepresentativeService.
// AUTHOR: James Schilz
// CREATED: 2026-06-05
// NOTES: Milestone 4.3.0 — restored from persisted state after load.
// =============================================================================

namespace CCS.Modules.NPCs
{
    public sealed class CCS_NpcServiceRepresentativeSnapshot
    {
        public static readonly CCS_NpcServiceRepresentativeSnapshot Empty = new CCS_NpcServiceRepresentativeSnapshot();

        public string RepresentativeId { get; set; } = string.Empty;

        public string SettlementId { get; set; } = string.Empty;

        public string BusinessId { get; set; } = string.Empty;

        public string ServicePointId { get; set; } = string.Empty;

        public CCS_NpcRoleType RequiredRole { get; set; } = CCS_NpcRoleType.Unknown;

        public string AssignedNpcIdentityId { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public string DisplayTitle { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public bool FallbackToServicePoint { get; set; } = true;

        public CCS_SettlementServiceRouteType RouteType { get; set; } = CCS_SettlementServiceRouteType.Unknown;

        public bool IsValid =>
            !string.IsNullOrWhiteSpace(RepresentativeId)
            && !string.IsNullOrWhiteSpace(SettlementId)
            && !string.IsNullOrWhiteSpace(BusinessId)
            && !string.IsNullOrWhiteSpace(ServicePointId)
            && RequiredRole != CCS_NpcRoleType.Unknown
            && !string.IsNullOrWhiteSpace(AssignedNpcIdentityId)
            && !string.IsNullOrWhiteSpace(DisplayName);
    }
}
