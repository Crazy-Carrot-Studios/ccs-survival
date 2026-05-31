// =============================================================================
// SCRIPT: CCS_BuildingPieceType
// CATEGORY: Modules / Building / Runtime / Definitions
// PURPOSE: Enumerates building piece categories for structure definitions.
// PLACEMENT: Referenced by CCS_BuildingPieceDefinition and building snapshots.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Architecture only. No placement, snapping, or build mode in 0.8.0.
// =============================================================================

namespace CCS.Modules.Building
{
    public enum CCS_BuildingPieceType
    {
        Foundation = 0,
        Floor = 1,
        Wall = 2,
        Doorway = 3,
        Door = 4,
        WindowWall = 5,
        Roof = 6,
        Stair = 7,
        Pillar = 8,
        CampStructure = 9,
        Custom = 10
    }
}
