using CCS.Core;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_SurvivalCoreRuntimeBridge
// CATEGORY: Modules / SurvivalCore / Runtime / Services
// PURPOSE: Resolves survival core service from the runtime registry for HUD and debug tools.
// PLACEMENT: Used by influence HUD presenters and development diagnostics.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Safe when runtime host or services are missing. No singletons.
// =============================================================================

namespace CCS.Modules.SurvivalCore
{
    public static class CCS_SurvivalCoreRuntimeBridge
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

        public static bool TryGetSurvivalCoreService(out CCS_SurvivalCoreService survivalCoreService)
        {
            survivalCoreService = null;
            if (!TryGetRuntimeHost(out CCS_RuntimeHost runtimeHost)
                || runtimeHost.ServiceRegistry == null)
            {
                return false;
            }

            return runtimeHost.ServiceRegistry.TryGetService(out survivalCoreService);
        }

        #endregion
    }
}
