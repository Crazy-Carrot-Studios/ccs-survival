using CCS.Core;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_MountRuntimeBridge
// CATEGORY: Modules / Mounts / Runtime / Services
// PURPOSE: Scene-safe access to CCS_MountService through the runtime host registry.
// PLACEMENT: Used by interactables and survival player glue.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// =============================================================================

namespace CCS.Modules.Mounts
{
    public static class CCS_MountRuntimeBridge
    {
        private static CCS_MountService registeredService;

        public static void Register(CCS_MountService service)
        {
            registeredService = service;
        }

        public static bool TryGetMountService(out CCS_MountService service)
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
