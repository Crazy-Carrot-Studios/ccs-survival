// =============================================================================
// SCRIPT: CCS_BuildingSnapPointType
// CATEGORY: Modules / Building / Runtime / Snap
// PURPOSE: Categories for building snap point compatibility rules.
// PLACEMENT: Used by snap definitions, runtime snap data, and placement matching.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Basic snap foundation for 0.8.3. No structural integrity validation.
// =============================================================================

namespace CCS.Modules.Building
{
    public enum CCS_BuildingSnapPointType
    {
        FoundationEdge = 0,
        WallBottom = 1,
        WallTop = 2,
        RoofEdge = 3,
        Free = 4
    }
}
