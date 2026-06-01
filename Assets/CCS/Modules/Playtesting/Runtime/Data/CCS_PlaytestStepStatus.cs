// =============================================================================
// SCRIPT: CCS_PlaytestStepStatus
// CATEGORY: Modules / Playtesting / Runtime / Data
// PURPOSE: Checklist status values for manual playtest harness steps.
// PLACEMENT: Used by CCS_PlaytestService and CCS_PlaytestHud.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Foundation statuses only for milestone 1.0.2.
// =============================================================================

namespace CCS.Modules.Playtesting
{
    public enum CCS_PlaytestStepStatus
    {
        NotStarted = 0,
        Active = 1,
        Passed = 2,
        Failed = 3,
        Skipped = 4
    }
}
