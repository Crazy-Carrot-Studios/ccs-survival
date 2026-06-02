// =============================================================================
// SCRIPT: CCS_LivestockState
// CATEGORY: Modules / Ranching / Runtime / Data
// PURPOSE: Basic livestock lifecycle states for ranch production.
// PLACEMENT: Used by CCS_LivestockInstance and save payloads.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// NOTES: Milestone 2.1.0 ranching foundation.
// =============================================================================

namespace CCS.Modules.Ranching
{
    public enum CCS_LivestockState
    {
        Idle = 0,
        Assigned = 1,
        Producing = 2,
        ReadyToCollect = 3,
        Unavailable = 4
    }
}
