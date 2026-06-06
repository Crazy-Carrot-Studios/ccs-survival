// =============================================================================
// SCRIPT: CCS_SettlementHousingStatus
// CATEGORY: Modules / Settlements / Runtime / Housing
// PURPOSE: Dev-readable housing marker activation states for settlement anchors.
// PLACEMENT: Used by housing markers, labels, and runtime bridge resolution.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.4.0 — Active / Inactive / Locked placeholder visuals only.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public enum CCS_SettlementHousingStatus
    {
        Unknown = 0,
        Locked = 1,
        Inactive = 2,
        Active = 3
    }
}
