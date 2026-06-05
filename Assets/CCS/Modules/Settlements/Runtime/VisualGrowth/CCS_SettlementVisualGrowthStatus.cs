// =============================================================================
// SCRIPT: CCS_SettlementVisualGrowthStatus
// CATEGORY: Modules / Settlements / Runtime / VisualGrowth
// PURPOSE: Visible marker states derived from settlement growth stage.
// PLACEMENT: Used by markers, labels, and validation utilities.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.9.0 settlement visual growth foundation.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public enum CCS_SettlementVisualGrowthStatus
    {
        Locked = 0,
        Inactive = 1,
        Active = 2
    }
}
