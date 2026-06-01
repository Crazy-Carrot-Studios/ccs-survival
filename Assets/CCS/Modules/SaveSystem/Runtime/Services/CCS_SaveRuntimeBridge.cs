using CCS.Core;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_SaveRuntimeBridge
// CATEGORY: Modules / SaveSystem / Runtime / Services
// PURPOSE: Resolves CCS_SaveService from the active runtime host registry.
// PLACEMENT: Used by debug controllers, startup loaders, and future UI hooks.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Safe when runtime host or service registry is missing. No singletons.
// =============================================================================

namespace CCS.Modules.SaveSystem
{
    public static class CCS_SaveRuntimeBridge
    {
        #region Public Methods

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

        public static bool TryGetSaveService(out CCS_SaveService saveService)
        {
            saveService = null;
            if (!TryGetRuntimeHost(out CCS_RuntimeHost runtimeHost)
                || runtimeHost.ServiceRegistry == null)
            {
                return false;
            }

            return runtimeHost.ServiceRegistry.TryGetService(out saveService);
        }

        #endregion
    }
}
