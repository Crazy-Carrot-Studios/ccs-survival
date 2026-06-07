// =============================================================================
// SCRIPT: CCS_NpcScheduleBlockType
// CATEGORY: Modules / NPCs / Runtime / Schedules
// PURPOSE: Daily schedule block categories for placeholder NPC routines.
// PLACEMENT: Serialized on schedule blocks and persisted schedule state.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.6.0 — no AI, dialogue, or pathfinding.
// =============================================================================

namespace CCS.Modules.NPCs
{
    public enum CCS_NpcScheduleBlockType
    {
        Unknown = 0,
        Sleep = 1,
        Home = 2,
        Work = 3,
        Break = 4,
        Leisure = 5,
        Service = 6,
        Idle = 7
    }
}
