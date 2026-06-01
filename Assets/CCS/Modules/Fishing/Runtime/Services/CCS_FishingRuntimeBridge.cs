using CCS.Core;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_FishingRuntimeBridge
// CATEGORY: Modules / Fishing / Runtime / Services
// PURPOSE: Resolves fishing and inventory services from the runtime registry.
// PLACEMENT: Used by CCS_FishingSpot and future fishing interactables.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Safe when runtime host or services are missing. No singletons.
// =============================================================================

namespace CCS.Modules.Fishing
{
    public static class CCS_FishingRuntimeBridge
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

        public static bool TryGetFishingService(out CCS_FishingService fishingService)
        {
            fishingService = null;
            if (!TryGetRuntimeHost(out CCS_RuntimeHost runtimeHost))
            {
                return false;
            }

            return runtimeHost.ServiceRegistry.TryGetService(out fishingService);
        }

        #endregion
    }
}
