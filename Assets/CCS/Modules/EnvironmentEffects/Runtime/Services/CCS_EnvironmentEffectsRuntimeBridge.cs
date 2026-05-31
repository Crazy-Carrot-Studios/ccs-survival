using CCS.Core;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_EnvironmentEffectsRuntimeBridge
// CATEGORY: Modules / EnvironmentEffects / Runtime / Services
// PURPOSE: Resolves environment service from the runtime registry for HUD and debug tools.
// PLACEMENT: Used by HUD presenters and future Survival Core integration.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Safe when runtime host or services are missing. No singletons.
// =============================================================================

namespace CCS.Modules.EnvironmentEffects
{
    public static class CCS_EnvironmentEffectsRuntimeBridge
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

        public static bool TryGetEnvironmentEffectsService(out CCS_EnvironmentEffectsService environmentService)
        {
            environmentService = null;
            if (!TryGetRuntimeHost(out CCS_RuntimeHost runtimeHost)
                || runtimeHost.ServiceRegistry == null)
            {
                return false;
            }

            return runtimeHost.ServiceRegistry.TryGetService(out environmentService);
        }

        #endregion
    }
}
