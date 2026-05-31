using CCS.Core;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_SaveLoadRuntimeBridge
// CATEGORY: Modules / SaveLoad / Runtime / Services
// PURPOSE: Resolves save/load service from the runtime registry for dev components.
// PLACEMENT: Used by development test saveables and future save UI.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Safe when runtime host or services are missing. No singletons.
// =============================================================================

namespace CCS.Modules.SaveLoad
{
    public static class CCS_SaveLoadRuntimeBridge
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

        public static bool TryGetSaveLoadService(out CCS_SaveLoadService saveLoadService)
        {
            saveLoadService = null;
            if (!TryGetRuntimeHost(out CCS_RuntimeHost runtimeHost))
            {
                return false;
            }

            return runtimeHost.ServiceRegistry.TryGetService(out saveLoadService);
        }

        #endregion
    }
}
