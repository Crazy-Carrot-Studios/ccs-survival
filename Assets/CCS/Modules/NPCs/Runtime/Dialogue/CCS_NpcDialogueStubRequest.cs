using CCS.Modules.Settlements;

// =============================================================================
// SCRIPT: CCS_NpcDialogueStubRequest
// CATEGORY: Modules / NPCs / Runtime / Dialogue
// PURPOSE: Input payload for resolving profile-driven NPC dialogue stub lines.
// PLACEMENT: Built from interactables and playtest harness.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.9.0 — identity and affiliation resolved before matching.
// =============================================================================

namespace CCS.Modules.NPCs
{
    public sealed class CCS_NpcDialogueStubRequest
    {
        public string NpcIdentityId { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public string RoleDisplayName { get; set; } = string.Empty;

        public string SettlementId { get; set; } = string.Empty;

        public string SettlementDisplayName { get; set; } = string.Empty;

        public string BusinessId { get; set; } = string.Empty;

        public string BusinessDisplayName { get; set; } = string.Empty;

        public string RegionId { get; set; } = string.Empty;

        public CCS_NpcRoleType RoleType { get; set; } = CCS_NpcRoleType.Unknown;

        public CCS_NpcAffiliationType PrimaryAffiliationType { get; set; } = CCS_NpcAffiliationType.None;

        public bool IsServiceRepresentative { get; set; }

        public CCS_SettlementServiceRouteType ServiceRoute { get; set; } = CCS_SettlementServiceRouteType.Unknown;

        public bool HasIdentity =>
            !string.IsNullOrWhiteSpace(NpcIdentityId)
            && !string.IsNullOrWhiteSpace(DisplayName);

        public bool HasSettlement => !string.IsNullOrWhiteSpace(SettlementId);

        public bool HasRole => RoleType != CCS_NpcRoleType.Unknown;
    }
}
