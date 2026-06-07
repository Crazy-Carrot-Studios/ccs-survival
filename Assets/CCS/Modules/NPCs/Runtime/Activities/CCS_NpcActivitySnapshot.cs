// =============================================================================
// SCRIPT: CCS_NpcActivitySnapshot
// CATEGORY: Modules / NPCs / Runtime / Activities
// PURPOSE: Runtime read model for activity evaluation, labels, and playtest.
// PLACEMENT: Built by CCS_NpcActivityService and runtime bridge.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.7.0 — includes schedule block and movement context for debug.
// =============================================================================

namespace CCS.Modules.NPCs
{
    public sealed class CCS_NpcActivitySnapshot
    {
        public static readonly CCS_NpcActivitySnapshot Empty = new CCS_NpcActivitySnapshot();

        public string NpcIdentityId { get; set; } = string.Empty;

        public string SettlementId { get; set; } = string.Empty;

        public CCS_NpcActivityType CurrentActivityType { get; set; } = CCS_NpcActivityType.None;

        public CCS_NpcScheduleBlockType ScheduleBlockType { get; set; } = CCS_NpcScheduleBlockType.Unknown;

        public CCS_NpcMovementStatus MovementStatus { get; set; } = CCS_NpcMovementStatus.Unknown;

        public int LastEvaluatedHour { get; set; } = -1;

        public bool IsValid =>
            !string.IsNullOrWhiteSpace(NpcIdentityId)
            && !string.IsNullOrWhiteSpace(SettlementId)
            && CurrentActivityType != CCS_NpcActivityType.None;
    }
}
