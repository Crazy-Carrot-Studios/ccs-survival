// =============================================================================
// SCRIPT: CCS_ResourceNodeType
// CATEGORY: Modules / WorldResources / Runtime / Definitions
// PURPOSE: High-level classification for harvestable world resource nodes.
// PLACEMENT: Referenced by CCS_ResourceDefinition assets.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: No world art or terrain references in 0.5.1 foundation.
// =============================================================================

namespace CCS.Modules.WorldResources
{
    public enum CCS_ResourceNodeType
    {
        Tree = 0,
        Rock = 1,
        Plant = 2,
        Gatherable = 3,
        Custom = 4
    }
}
