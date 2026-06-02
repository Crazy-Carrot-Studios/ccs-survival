// =============================================================================
// SCRIPT: CCS_TrapState
// CATEGORY: Modules / Trapping / Runtime / Data
// PURPOSE: Lifecycle states for placed frontier trap instances.
// PLACEMENT: Used by CCS_TrapInstance and CCS_TrapService.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// =============================================================================

namespace CCS.Modules.Trapping
{
    public enum CCS_TrapState
    {
        Unarmed = 0,
        Armed = 1,
        Triggered = 2,
        Harvested = 3,
        Broken = 4
    }
}
