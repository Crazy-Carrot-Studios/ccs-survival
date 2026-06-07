// =============================================================================
// SCRIPT: CCS_NpcAffiliationSnapshot
// CATEGORY: Modules / NPCs / Runtime / Affiliations
// PURPOSE: Runtime read model for affiliation labels, debug HUD, and playtest.
// PLACEMENT: Built by CCS_NpcAffiliationService and runtime bridge.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.8.0 — includes resolved display names for dev labels.
// =============================================================================

namespace CCS.Modules.NPCs
{
    public sealed class CCS_NpcAffiliationSnapshot
    {
        public static readonly CCS_NpcAffiliationSnapshot Empty = new CCS_NpcAffiliationSnapshot();

        public string NpcIdentityId { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public string RoleDisplayName { get; set; } = string.Empty;

        public string SettlementId { get; set; } = string.Empty;

        public string SettlementDisplayName { get; set; } = string.Empty;

        public string RegionId { get; set; } = string.Empty;

        public string BusinessId { get; set; } = string.Empty;

        public string BusinessDisplayName { get; set; } = string.Empty;

        public int WorkforceCategory { get; set; }

        public string WorkforceDisplayName { get; set; } = string.Empty;

        public bool IsServiceRepresentative { get; set; }

        public int LoyaltyValue { get; set; } = 50;

        public bool IsValid =>
            !string.IsNullOrWhiteSpace(NpcIdentityId)
            && !string.IsNullOrWhiteSpace(SettlementId);
    }
}
