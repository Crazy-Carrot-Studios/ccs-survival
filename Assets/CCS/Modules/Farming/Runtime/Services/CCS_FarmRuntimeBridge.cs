using CCS.Core;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_FarmRuntimeBridge
// CATEGORY: Modules / Farming / Runtime / Services
// PURPOSE: Resolves farm service from runtime registry without null host access.
// PLACEMENT: Used by interactables and playtest harness.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-02
// NOTES: Milestone 2.2.0 — null-safe ServiceRegistry resolution.
// =============================================================================

namespace CCS.Modules.Farming
{
    public static class CCS_FarmRuntimeBridge
    {
        private static CCS_FarmService registeredService;

        public static void Register(CCS_FarmService service)
        {
            registeredService = service;
        }

        public static bool TryGetFarmService(out CCS_FarmService farmService)
        {
            farmService = registeredService;
            if (farmService != null && farmService.IsInitialized)
            {
                return true;
            }

            farmService = null;
            if (!TryGetServiceRegistry(out CCS_ServiceRegistry serviceRegistry))
            {
                return false;
            }

            return serviceRegistry.TryGetService(out farmService)
                && farmService != null
                && farmService.IsInitialized;
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
