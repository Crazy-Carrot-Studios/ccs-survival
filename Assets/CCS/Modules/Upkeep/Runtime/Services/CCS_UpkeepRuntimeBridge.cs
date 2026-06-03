using CCS.Core;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_UpkeepRuntimeBridge
// CATEGORY: Modules / Upkeep / Runtime / Services
// PURPOSE: Resolves upkeep service from the runtime registry.
// PLACEMENT: Used by land office debug HUD and playtest harness.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 2.5.0 tax and upkeep foundation.
// =============================================================================

namespace CCS.Modules.Upkeep
{
    public static class CCS_UpkeepRuntimeBridge
    {
        private static CCS_UpkeepService registeredService;

        public static void Register(CCS_UpkeepService service)
        {
            registeredService = service;
        }

        public static bool TryGetUpkeepService(out CCS_UpkeepService upkeepService)
        {
            upkeepService = registeredService;
            if (upkeepService != null && upkeepService.IsInitialized)
            {
                return true;
            }

            upkeepService = null;
            if (!TryGetServiceRegistry(out CCS_ServiceRegistry serviceRegistry))
            {
                return false;
            }

            return serviceRegistry.TryGetService(out upkeepService)
                && upkeepService != null
                && upkeepService.IsInitialized;
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
