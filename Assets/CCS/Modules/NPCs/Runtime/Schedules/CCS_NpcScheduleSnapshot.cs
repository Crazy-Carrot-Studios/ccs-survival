// =============================================================================
// SCRIPT: CCS_NpcScheduleSnapshot
// CATEGORY: Modules / NPCs / Runtime / Schedules
// PURPOSE: Runtime read model for schedule evaluation, debug labels, and playtest.
// PLACEMENT: Built by CCS_NpcScheduleService and runtime bridge.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.6.0 — no transform data.
// =============================================================================

namespace CCS.Modules.NPCs
{
    public sealed class CCS_NpcScheduleSnapshot
    {
        public static readonly CCS_NpcScheduleSnapshot Empty = new CCS_NpcScheduleSnapshot();

        public string NpcIdentityId { get; set; } = string.Empty;

        public string SettlementId { get; set; } = string.Empty;

        public string ActiveScheduleId { get; set; } = string.Empty;

        public CCS_NpcScheduleBlockType CurrentBlockType { get; set; } = CCS_NpcScheduleBlockType.Unknown;

        public CCS_NpcScheduleTargetKind CurrentTargetKind { get; set; } = CCS_NpcScheduleTargetKind.Unknown;

        public string CurrentTargetId { get; set; } = string.Empty;

        public int LastEvaluatedHour { get; set; } = -1;

        public bool IsValid =>
            !string.IsNullOrWhiteSpace(NpcIdentityId)
            && !string.IsNullOrWhiteSpace(SettlementId)
            && !string.IsNullOrWhiteSpace(ActiveScheduleId);
    }
}
