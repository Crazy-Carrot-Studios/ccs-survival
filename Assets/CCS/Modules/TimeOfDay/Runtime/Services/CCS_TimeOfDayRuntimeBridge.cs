using CCS.Core;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_TimeOfDayRuntimeBridge
// CATEGORY: Modules / TimeOfDay / Runtime / Services
// PURPOSE: Resolves time-of-day service from the runtime registry for HUD and debug tools.
// PLACEMENT: Used by HUD presenters and future schedule systems.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Safe when runtime host or services are missing. No singletons.
// =============================================================================

namespace CCS.Modules.TimeOfDay
{
    public static class CCS_TimeOfDayRuntimeBridge
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

        public static bool TryGetTimeOfDayService(out CCS_TimeOfDayService timeOfDayService)
        {
            timeOfDayService = null;
            if (!TryGetRuntimeHost(out CCS_RuntimeHost runtimeHost))
            {
                return false;
            }

            return runtimeHost.ServiceRegistry.TryGetService(out timeOfDayService);
        }

        #endregion
    }
}
