using CCS.Core;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_VehicleRuntimeBridge
// CATEGORY: Modules / Vehicles / Runtime / Services
// PURPOSE: Scene-safe access to CCS_VehicleService through the runtime host registry.
// PLACEMENT: Used by interactables and survival player glue.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// =============================================================================

namespace CCS.Modules.Vehicles
{
    public static class CCS_VehicleRuntimeBridge
    {
        private static CCS_VehicleService registeredService;

        public static void Register(CCS_VehicleService service)
        {
            registeredService = service;
        }

        public static bool TryGetVehicleService(out CCS_VehicleService service)
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
