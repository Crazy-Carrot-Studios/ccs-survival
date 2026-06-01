// =============================================================================
// SCRIPT: CCS_SleepFailureReason
// CATEGORY: Modules / Sleep / Runtime / Data
// PURPOSE: Discrete failure reasons for sleep request validation.
// PLACEMENT: Returned by CCS_SleepService and CCS_SleepResult.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: No death, enemy interruption, or dream systems in 0.9.6 foundation.
// =============================================================================

namespace CCS.Modules.Sleep
{
    public enum CCS_SleepFailureReason
    {
        None = 0,
        MissingBedroll = 1,
        UnsafeConditions = 2,
        AlreadyRested = 3,
        TimeServiceUnavailable = 4,
        SurvivalCoreUnavailable = 5,
        ProfileUnavailable = 6
    }
}
