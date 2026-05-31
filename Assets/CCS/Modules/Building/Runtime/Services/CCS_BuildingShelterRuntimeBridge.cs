using System.Collections.Generic;
using CCS.Core;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_BuildingShelterRuntimeBridge
// CATEGORY: Modules / Building / Runtime / Services
// PURPOSE: Resolves building service for shelter and environment integrations.
// PLACEMENT: Used by shelter service, harnesses, and HUD presenters.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Safe when runtime host or building service is unavailable.
// =============================================================================

namespace CCS.Modules.Building
{
    public static class CCS_BuildingShelterRuntimeBridge
    {
        #region Public Methods

        public static bool TryGetBuildingService(out CCS_BuildingService buildingService)
        {
            return CCS_BuildingRuntimeBridge.TryGetBuildingService(out buildingService);
        }

        public static bool TryGetShelterContributions(
            out IReadOnlyList<CCS_BuildingShelterContribution> shelterContributions)
        {
            shelterContributions = System.Array.Empty<CCS_BuildingShelterContribution>();

            if (!TryGetBuildingService(out CCS_BuildingService buildingService)
                || buildingService == null
                || !buildingService.IsInitialized)
            {
                return false;
            }

            shelterContributions = buildingService.GetShelterContributions();
            return true;
        }

        #endregion
    }
}
