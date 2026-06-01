// =============================================================================
// SCRIPT: CCS_GatheringResourceType
// CATEGORY: Modules / Gathering / Runtime / Data
// PURPOSE: Identifies primitive resource categories granted by gathering nodes.
// PLACEMENT: Used by CCS_GatheringReward and CCS_GatheringProfile reward tables.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Foundation enum for 0.9.9 resource gathering milestone.
// =============================================================================

namespace CCS.Modules.Gathering
{
    public enum CCS_GatheringResourceType
    {
        None = 0,
        Stick = 1,
        Stone = 2,
        Wood = 3,
        PlantFiber = 4
    }
}
