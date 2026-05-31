using CCS.Core;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_WeatherRuntimeBridge
// CATEGORY: Modules / Weather / Runtime / Services
// PURPOSE: Resolves weather service from the runtime registry for HUD and debug tools.
// PLACEMENT: Used by HUD presenters and future environment effect systems.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Safe when runtime host or services are missing. No singletons.
// =============================================================================

namespace CCS.Modules.Weather
{
    public static class CCS_WeatherRuntimeBridge
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

        public static bool TryGetWeatherService(out CCS_WeatherService weatherService)
        {
            weatherService = null;
            if (!TryGetRuntimeHost(out CCS_RuntimeHost runtimeHost))
            {
                return false;
            }

            return runtimeHost.ServiceRegistry.TryGetService(out weatherService);
        }

        #endregion
    }
}
