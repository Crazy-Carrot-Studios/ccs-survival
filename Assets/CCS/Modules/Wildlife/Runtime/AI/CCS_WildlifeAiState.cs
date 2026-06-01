// =============================================================================
// SCRIPT: CCS_WildlifeAiState
// CATEGORY: Modules / Wildlife / Runtime / AI
// PURPOSE: Passive wildlife behavior states for 0.9.7 foundation.
// PLACEMENT: Used by CCS_WildlifeStateMachine and CCS_WildlifeAiSnapshot.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: No attack, combat, or death states in this milestone.
// =============================================================================

namespace CCS.Modules.Wildlife
{
    public enum CCS_WildlifeAiState
    {
        Idle = 0,
        Wander = 1,
        Alert = 2,
        Flee = 3
    }
}
