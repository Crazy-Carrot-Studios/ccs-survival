using CCS.Core;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_InteractionRuntimeBridge
// CATEGORY: Modules / Interaction / Runtime / Services
// PURPOSE: Resolves interaction service from the runtime registry for player drivers.
// PLACEMENT: Used by CCS_InteractionPlayerDriver and test harnesses.
// AUTHOR: James Schilz
// CREATED: 2026-05-31
// NOTES: Safe when runtime host or interaction service is unavailable.
// =============================================================================

namespace CCS.Modules.Interaction
{
    public static class CCS_InteractionRuntimeBridge
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

        public static bool TryGetInteractionService(out CCS_InteractionService interactionService)
        {
            interactionService = null;
            if (!TryGetRuntimeHost(out CCS_RuntimeHost runtimeHost)
                || runtimeHost.ServiceRegistry == null)
            {
                return false;
            }

            return runtimeHost.ServiceRegistry.TryGetService(out interactionService);
        }

        #endregion
    }
}
