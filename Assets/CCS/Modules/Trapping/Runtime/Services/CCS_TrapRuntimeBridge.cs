using CCS.Core;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_TrapRuntimeBridge
// CATEGORY: Modules / Trapping / Runtime / Services
// PURPOSE: Resolves CCS_TrapService from the runtime service registry.
// PLACEMENT: Used by trap components, active item routing, and playtest harness.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// =============================================================================

namespace CCS.Modules.Trapping
{
    public static class CCS_TrapRuntimeBridge
    {
        public static bool TryGetTrapService(out CCS_TrapService trapService)
        {
            trapService = null;
            CCS_RuntimeHost[] runtimeHosts =
                CCS_SurvivalSceneQueryUtility.FindActiveObjectsByType<CCS_RuntimeHost>();
            if (runtimeHosts == null || runtimeHosts.Length == 0)
            {
                return false;
            }

            CCS_RuntimeHost runtimeHost = runtimeHosts[0];
            if (runtimeHost?.ServiceRegistry == null)
            {
                return false;
            }

            return runtimeHost.ServiceRegistry.TryGetService(out trapService)
                && trapService != null
                && trapService.IsInitialized;
        }
    }
}
