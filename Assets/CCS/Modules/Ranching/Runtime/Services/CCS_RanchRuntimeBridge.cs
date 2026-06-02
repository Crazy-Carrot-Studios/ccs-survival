using CCS.Core;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_RanchRuntimeBridge
// CATEGORY: Modules / Ranching / Runtime / Services
// PURPOSE: Scene-safe access to CCS_RanchService through the runtime host registry.
// PLACEMENT: Used by playtest harness and survival player glue.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// NOTES: Milestone 2.1.0 ranching foundation.
// =============================================================================

namespace CCS.Modules.Ranching
{
    public static class CCS_RanchRuntimeBridge
    {
        private static CCS_RanchService registeredService;

        public static void Register(CCS_RanchService service)
        {
            registeredService = service;
        }

        public static bool TryGetRanchService(out CCS_RanchService service)
        {
            service = registeredService;
            if (service != null && service.IsInitialized)
            {
                return true;
            }

            CCS_RuntimeHost[] runtimeHosts = CCS_SurvivalSceneQueryUtility.FindActiveObjectsByType<CCS_RuntimeHost>();
            if (runtimeHosts != null
                && runtimeHosts.Length > 0
                && runtimeHosts[0]?.ServiceRegistry != null
                && runtimeHosts[0].ServiceRegistry.TryGetService(out service)
                && service != null
                && service.IsInitialized)
            {
                registeredService = service;
                return true;
            }

            service = null;
            return false;
        }
    }
}
