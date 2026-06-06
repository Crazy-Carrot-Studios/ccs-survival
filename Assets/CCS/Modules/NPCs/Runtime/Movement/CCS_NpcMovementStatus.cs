// =============================================================================
// SCRIPT: CCS_NpcMovementStatus
// CATEGORY: Modules / NPCs / Runtime / Movement
// PURPOSE: Supported lightweight NPC movement states for placeholder workers and reps.
// PLACEMENT: Serialized on CCS_NpcMovementState and used by movement service.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.5.0 — schedule-driven work/home transitions only.
// =============================================================================

namespace CCS.Modules.NPCs
{
    public enum CCS_NpcMovementStatus
    {
        Unknown = 0,
        Idle = 1,
        TravelingToWork = 2,
        Working = 3,
        TravelingHome = 4,
        AtHome = 5
    }
}
