// =============================================================================
// SCRIPT: CCS_WildlifeHarvestResultType
// CATEGORY: Modules / Wildlife / Runtime / Data
// PURPOSE: Result codes for wildlife harvest attempts.
// PLACEMENT: Returned by CCS_WildlifeHarvestService and active item routing.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Milestone 1.3.2 frontier hunting foundation.
// =============================================================================

namespace CCS.Modules.Wildlife
{
    public enum CCS_WildlifeHarvestResultType
    {
        Success = 0,
        HarvestFailed = 1,
        WildlifeNotDead = 2,
        WildlifeAlreadyHarvested = 3,
        WrongTool = 4,
        InventoryFull = 5,
        ServiceUnavailable = 6,
        InvalidRequest = 7
    }
}
