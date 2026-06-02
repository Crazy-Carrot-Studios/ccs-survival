using CCS.Core;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_RegionRuntimeBridge
// CATEGORY: Modules / Regions / Runtime / Services
// PURPOSE: Resolves region services from the runtime service registry.
// PLACEMENT: Used by region volumes, playtest harness, and future travel systems.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// =============================================================================

namespace CCS.Modules.Regions
{
    public static class CCS_RegionRuntimeBridge
    {
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

        public static bool TryGetRegionService(out CCS_RegionService regionService)
        {
            regionService = null;
            if (!TryGetRuntimeHost(out CCS_RuntimeHost runtimeHost))
            {
                return false;
            }

            return runtimeHost.ServiceRegistry.TryGetService(out regionService);
        }
    }
}
