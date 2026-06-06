// =============================================================================
// SCRIPT: CCS_NpcMovementSnapshot
// CATEGORY: Modules / NPCs / Runtime / Movement
// PURPOSE: Runtime movement snapshot for playtest, debug, and bridge refresh.
// PLACEMENT: Built by CCS_NpcMovementService from persisted state.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.5.0 — no transform data.
// =============================================================================

namespace CCS.Modules.NPCs
{
    public sealed class CCS_NpcMovementSnapshot
    {
        public string NpcIdentityId { get; set; } = string.Empty;

        public string SettlementId { get; set; } = string.Empty;

        public CCS_NpcMovementStatus Status { get; set; } = CCS_NpcMovementStatus.Unknown;

        public string TargetAnchorId { get; set; } = string.Empty;

        public string WorkplaceAnchorId { get; set; } = string.Empty;

        public string HomeHousingId { get; set; } = string.Empty;

        public bool IsValid => !string.IsNullOrWhiteSpace(NpcIdentityId);

        public static CCS_NpcMovementSnapshot Empty { get; } = new CCS_NpcMovementSnapshot();
    }
}
