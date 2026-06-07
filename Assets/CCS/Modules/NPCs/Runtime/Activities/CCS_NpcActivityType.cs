// =============================================================================
// SCRIPT: CCS_NpcActivityType
// CATEGORY: Modules / NPCs / Runtime / Activities
// PURPOSE: Lightweight visible activity states for placeholder NPCs.
// PLACEMENT: Serialized on activity state and derived from schedule/movement.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.7.0 — no animations, dialogue, or pathfinding.
// =============================================================================

namespace CCS.Modules.NPCs
{
    public enum CCS_NpcActivityType
    {
        None = 0,
        Traveling = 1,
        Working = 2,
        Serving = 3,
        Resting = 4,
        Leisure = 5,
        Sleeping = 6,
        Idle = 7
    }
}
