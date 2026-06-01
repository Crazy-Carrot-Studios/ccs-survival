using CCS.Core;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_PlayerDeathRuntimeBridge
// CATEGORY: Modules / PlayerDeath / Runtime / Services
// PURPOSE: Resolves CCS_PlayerDeathService from the active runtime host registry.
// PLACEMENT: Used by player controllers and future UI hooks.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Safe when runtime host or service registry is missing.
// =============================================================================

namespace CCS.Modules.PlayerDeath
{
    public static class CCS_PlayerDeathRuntimeBridge
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

        public static bool TryGetPlayerDeathService(out CCS_PlayerDeathService playerDeathService)
        {
            playerDeathService = null;
            if (!TryGetRuntimeHost(out CCS_RuntimeHost runtimeHost)
                || runtimeHost.ServiceRegistry == null)
            {
                return false;
            }

            return runtimeHost.ServiceRegistry.TryGetService(out playerDeathService);
        }

        #endregion
    }
}
