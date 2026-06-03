// =============================================================================
// SCRIPT: CCS_LandClaimState
// CATEGORY: Modules / Land / Runtime / Data
// PURPOSE: Lifecycle states for frontier land claim instances.
// PLACEMENT: Used by CCS_LandClaimInstance and CCS_LandClaimService.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-02
// NOTES: Milestone 2.3.0 — single-player ownership only.
// =============================================================================

namespace CCS.Modules.Land
{
    public enum CCS_LandClaimState
    {
        Unclaimed = 0,
        Claimed = 1,
        Abandoned = 2,
        Invalid = 3
    }
}
