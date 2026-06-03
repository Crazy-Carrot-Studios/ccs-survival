// =============================================================================
// SCRIPT: CCS_UpkeepTargetType
// CATEGORY: Modules / Upkeep / Runtime / Data
// PURPOSE: Identifies what owned asset an upkeep entry applies to.
// PLACEMENT: Used by CCS_UpkeepDefinition and CCS_UpkeepEntry.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 2.5.0 generic recurring-cost framework.
// =============================================================================

namespace CCS.Modules.Upkeep
{
    public enum CCS_UpkeepTargetType
    {
        Unknown = 0,
        LandClaim = 1,
        Stable = 2,
        Storage = 3,
        License = 4,
        Rental = 5,
        Other = 6
    }
}
