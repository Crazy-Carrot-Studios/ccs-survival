// =============================================================================
// SCRIPT: CCS_NpcScheduleTargetKind
// CATEGORY: Modules / NPCs / Runtime / Schedules
// PURPOSE: Persisted destination kind for schedule-driven NPC movement.
// PLACEMENT: Stored on CCS_NpcScheduleState; mapped from block types.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.6.0 — transforms are not persisted.
// =============================================================================

namespace CCS.Modules.NPCs
{
    public enum CCS_NpcScheduleTargetKind
    {
        Unknown = 0,
        Housing = 1,
        Workplace = 2,
        ServicePoint = 3,
        SettlementCenter = 4,
        CurrentAnchor = 5,
        Idle = 6
    }
}
