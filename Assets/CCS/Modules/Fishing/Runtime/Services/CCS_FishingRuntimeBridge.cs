using CCS.Core;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_FishingRuntimeBridge
// CATEGORY: Modules / Fishing / Runtime / Services
// PURPOSE: Resolves fishing services from the runtime registry without null host access.
// PLACEMENT: Used by CCS_FishingSpot and future fishing interactables.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Milestone 2.1.1 — null-safe ServiceRegistry resolution (matches Sleep bridge).
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
            if (!TryGetServiceRegistry(out CCS_ServiceRegistry serviceRegistry))
            {
                return false;
            }

            return serviceRegistry.TryGetService(out fishingService)
                && fishingService != null
                && fishingService.IsInitialized;
        }

        #endregion

        #region Private Methods

        private static bool TryGetServiceRegistry(out CCS_ServiceRegistry serviceRegistry)
        {
            serviceRegistry = null;
            if (!TryGetRuntimeHost(out CCS_RuntimeHost runtimeHost))
            {
                return false;
            }

            serviceRegistry = runtimeHost.ServiceRegistry;
            return serviceRegistry != null;
        }

        #endregion
    }
}
