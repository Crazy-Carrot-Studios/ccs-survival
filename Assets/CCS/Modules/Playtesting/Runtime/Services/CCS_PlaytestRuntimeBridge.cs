using CCS.Core;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_PlaytestRuntimeBridge
// CATEGORY: Modules / Playtesting / Runtime / Services
// PURPOSE: Resolves CCS_PlaytestService from the active runtime host registry.
// PLACEMENT: Used by CCS_PlaytestHud and bootstrap dev helpers.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Safe when runtime host or service registry is missing.
// =============================================================================

namespace CCS.Modules.Playtesting
{
    public static class CCS_PlaytestRuntimeBridge
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

        public static bool TryGetPlaytestService(out CCS_PlaytestService playtestService)
        {
            playtestService = null;
            if (!TryGetRuntimeHost(out CCS_RuntimeHost runtimeHost)
                || runtimeHost.ServiceRegistry == null)
            {
                return false;
            }

            return runtimeHost.ServiceRegistry.TryGetService(out playtestService);
        }

        #endregion
    }
}
