// =============================================================================
// SCRIPT: CCS_GatheringNodeType
// CATEGORY: Modules / Gathering / Runtime / Data
// PURPOSE: Identifies bootstrap gathering node archetypes for reward lookup.
// PLACEMENT: Serialized on CCS_GatheringNode and CCS_GatheringProfile reward tables.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: SmallTree, Rock, and Bush only in 0.9.9 foundation.
// =============================================================================

namespace CCS.Modules.Gathering
{
    public enum CCS_GatheringNodeType
    {
        None = 0,
        SmallTree = 1,
        Rock = 2,
        Bush = 3
    }
}
