using CCS.Core;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_BuildingRuntimeBridge
// CATEGORY: Modules / Building / Runtime / Services
// PURPOSE: Resolves building service from the runtime registry for HUD and tooling.
// PLACEMENT: Used by environment HUD presenter and future placement systems.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Safe when runtime host or services are missing. No singletons.
// =============================================================================

namespace CCS.Modules.Building
{
    public static class CCS_BuildingRuntimeBridge
    {
        #region Public Methods

        public static bool TryGetRuntimeHost(out CCS_RuntimeHost runtimeHost)
        {
            runtimeHost = null;
            CCS_RuntimeHost[] runtimeHosts = CCS_SurvivalSceneQueryUtility.FindActiveObjectsByType<CCS_RuntimeHost>();
            if (runtimeHosts == null || runtimeHosts.Length == 0)
            {
                return false;
            }

            runtimeHost = runtimeHosts[0];
            return runtimeHost != null;
        }

        public static bool TryGetBuildingService(out CCS_BuildingService buildingService)
        {
            buildingService = null;
            if (!TryGetRuntimeHost(out CCS_RuntimeHost runtimeHost)
                || runtimeHost.ServiceRegistry == null)
            {
                return false;
            }

            return runtimeHost.ServiceRegistry.TryGetService(out buildingService);
        }

        #endregion
    }
}
