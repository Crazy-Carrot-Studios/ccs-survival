// =============================================================================
// SCRIPT: CCS_HungerState
// CATEGORY: Modules / SurvivalCore / Runtime / Stats
// PURPOSE: Discrete hunger warning states derived from current hunger value.
// PLACEMENT: Resolved by CCS_HungerStateUtility from profile thresholds.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: No health damage or death logic in 0.9.5 foundation.
// =============================================================================

namespace CCS.Modules.SurvivalCore
{
    public enum CCS_HungerState
    {
        Normal = 0,
        Low = 1,
        Critical = 2,
        Empty = 3
    }
}
