using CCS.Modules.Settlements;

// =============================================================================
// SCRIPT: CCS_NpcIdentitySnapshot
// CATEGORY: Modules / NPCs / Runtime / Data
// PURPOSE: Runtime read-only NPC identity resolved for a placeholder actor.
// PLACEMENT: Built by CCS_NpcIdentityService and applied to placeholder actors.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 4.1.0 — stable for session; restored from persisted state after load.
// =============================================================================

namespace CCS.Modules.NPCs
{
    public sealed class CCS_NpcIdentitySnapshot
    {
        public static readonly CCS_NpcIdentitySnapshot Empty = new CCS_NpcIdentitySnapshot();

        public string NpcIdentityId { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public string RoleDisplayName { get; set; } = string.Empty;

        public CCS_NpcRoleType Role { get; set; } = CCS_NpcRoleType.Unknown;

        public string SettlementId { get; set; } = string.Empty;

        public string BusinessId { get; set; } = string.Empty;

        public CCS_SettlementPopulationCategory WorkforceCategory { get; set; } =
            CCS_SettlementPopulationCategory.Unknown;

        public bool IsValid =>
            !string.IsNullOrWhiteSpace(NpcIdentityId)
            && !string.IsNullOrWhiteSpace(DisplayName)
            && Role != CCS_NpcRoleType.Unknown
            && WorkforceCategory != CCS_SettlementPopulationCategory.Unknown;
    }
}
