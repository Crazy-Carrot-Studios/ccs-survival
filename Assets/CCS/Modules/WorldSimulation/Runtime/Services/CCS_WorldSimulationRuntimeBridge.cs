using CCS.Core;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_WorldSimulationRuntimeBridge
// CATEGORY: Modules / WorldSimulation / Runtime / Services
// PURPOSE: Resolves world simulation services from the runtime service registry.
// PLACEMENT: Used by playtest harness, validation, and future world UI systems.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// NOTES: Milestone 2.0.0 frontier world simulation foundation.
// =============================================================================

namespace CCS.Modules.WorldSimulation
{
    public static class CCS_WorldSimulationRuntimeBridge
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

        public static bool TryGetWorldSimulationService(out CCS_WorldSimulationService worldSimulationService)
        {
            worldSimulationService = null;
            if (!TryGetRuntimeHost(out CCS_RuntimeHost runtimeHost))
            {
                return false;
            }

            return runtimeHost.ServiceRegistry.TryGetService(out worldSimulationService);
        }
    }
}
