using System.Collections.Generic;

// =============================================================================
// SCRIPT: CCS_BuildingSnapCompatibilityUtility
// CATEGORY: Modules / Building / Runtime / Snap
// PURPOSE: Explicit snap point compatibility rules for placement matching.
// PLACEMENT: Used by CCS_BuildingPlacementService.FindBestSnapMatch().
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Simple rules only. No angle or structural validation in 0.8.3.
// =============================================================================

namespace CCS.Modules.Building
{
    public static class CCS_BuildingSnapCompatibilityUtility
    {
        #region Public Methods

        public static bool CanSnap(
            CCS_BuildingSnapPointType targetType,
            CCS_BuildingSnapPointType sourceType)
        {
            return CanSnap(targetType, sourceType, null);
        }

        public static bool CanSnap(
            CCS_BuildingSnapPointType targetType,
            CCS_BuildingSnapPointType sourceType,
            IReadOnlyList<CCS_BuildingSnapPointType> explicitCompatibleTargetTypes)
        {
            if (explicitCompatibleTargetTypes != null && explicitCompatibleTargetTypes.Count > 0)
            {
                for (int index = 0; index < explicitCompatibleTargetTypes.Count; index++)
                {
                    if (explicitCompatibleTargetTypes[index] == targetType)
                    {
                        return true;
                    }
                }

                return false;
            }

            switch (targetType)
            {
                case CCS_BuildingSnapPointType.FoundationEdge:
                    return sourceType == CCS_BuildingSnapPointType.WallBottom;
                case CCS_BuildingSnapPointType.WallTop:
                    return sourceType == CCS_BuildingSnapPointType.RoofEdge;
                case CCS_BuildingSnapPointType.Free:
                    return sourceType == CCS_BuildingSnapPointType.Free;
                default:
                    return false;
            }
        }

        #endregion
    }
}
