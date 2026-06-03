// =============================================================================
// SCRIPT: CCS_UpkeepState
// CATEGORY: Modules / Upkeep / Runtime / Data
// PURPOSE: Lifecycle state for recurring upkeep and tax entries.
// PLACEMENT: Used by CCS_UpkeepService and save snapshots.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 2.5.0 — no debt, foreclosure, or faction law yet.
// =============================================================================

namespace CCS.Modules.Upkeep
{
    public enum CCS_UpkeepState
    {
        Current = 0,
        Due = 1,
        Paid = 2,
        Failed = 3,
        Disabled = 4
    }
}
