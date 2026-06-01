using CCS.Core;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_GatheringRuntimeBridge
// CATEGORY: Modules / Gathering / Runtime / Services
// PURPOSE: Resolves gathering and inventory services from the runtime registry.
// PLACEMENT: Used by CCS_GatheringNode and CCS_GatheringInteractable.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Safe when runtime host or services are missing. No singletons.
// =============================================================================

namespace CCS.Modules.Gathering
{
    public static class CCS_GatheringRuntimeBridge
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

        public static bool TryGetGatheringService(out CCS_GatheringService gatheringService)
        {
            gatheringService = null;
            if (!TryGetRuntimeHost(out CCS_RuntimeHost runtimeHost))
            {
                return false;
            }

            return runtimeHost.ServiceRegistry.TryGetService(out gatheringService);
        }

        #endregion
    }
}
