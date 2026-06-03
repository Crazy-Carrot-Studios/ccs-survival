using CCS.Core;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_LandClaimRuntimeBridge
// CATEGORY: Modules / Land / Runtime / Services
// PURPOSE: Resolves land claim service from the runtime registry.
// PLACEMENT: Used by interactables and development test harnesses.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-02
// NOTES: Milestone 2.3.0 land ownership foundation.
// =============================================================================

namespace CCS.Modules.Land
{
    public static class CCS_LandClaimRuntimeBridge
    {
        private static CCS_LandClaimService registeredService;

        public static void Register(CCS_LandClaimService service)
        {
            registeredService = service;
        }

        public static bool TryGetLandClaimService(out CCS_LandClaimService landClaimService)
        {
            landClaimService = registeredService;
            if (landClaimService != null && landClaimService.IsInitialized)
            {
                return true;
            }

            landClaimService = null;
            if (!TryGetServiceRegistry(out CCS_ServiceRegistry serviceRegistry))
            {
                return false;
            }

            return serviceRegistry.TryGetService(out landClaimService)
                && landClaimService != null
                && landClaimService.IsInitialized;
        }

        private static bool TryGetServiceRegistry(out CCS_ServiceRegistry serviceRegistry)
        {
            serviceRegistry = null;
            CCS_RuntimeHost[] runtimeHosts = CCS_SurvivalSceneQueryUtility.FindActiveObjectsByType<CCS_RuntimeHost>();
            if (runtimeHosts == null || runtimeHosts.Length == 0)
            {
                return false;
            }

            serviceRegistry = runtimeHosts[0].ServiceRegistry;
            return serviceRegistry != null;
        }
    }
}
