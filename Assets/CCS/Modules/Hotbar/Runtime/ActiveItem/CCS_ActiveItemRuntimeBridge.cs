using CCS.Core;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_ActiveItemRuntimeBridge
// CATEGORY: Modules / Hotbar / Runtime / ActiveItem
// PURPOSE: Resolves CCS_ActiveItemService from the runtime service registry.
// PLACEMENT: Used by player drivers, playtest harness, and validation.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Safe when runtime host or service is missing. No singletons.
// =============================================================================

namespace CCS.Modules.Hotbar
{
    public static class CCS_ActiveItemRuntimeBridge
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

        public static bool TryGetActiveItemService(out CCS_ActiveItemService activeItemService)
        {
            activeItemService = null;
            if (!TryGetRuntimeHost(out CCS_RuntimeHost runtimeHost))
            {
                return false;
            }

            return runtimeHost.ServiceRegistry.TryGetService(out activeItemService);
        }
    }
}
