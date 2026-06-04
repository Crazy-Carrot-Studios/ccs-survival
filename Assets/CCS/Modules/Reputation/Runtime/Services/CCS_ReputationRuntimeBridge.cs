using CCS.Core;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_ReputationRuntimeBridge
// CATEGORY: Modules / Reputation / Runtime / Services
// PURPOSE: Resolves CCS_ReputationService from the active runtime service registry.
// PLACEMENT: Used by debug HUD and settlement integration.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 2.7.0 reputation and service trust foundation.
// =============================================================================

namespace CCS.Modules.Reputation
{
    public static class CCS_ReputationRuntimeBridge
    {
        private static CCS_ReputationService registeredService;

        public static void Register(CCS_ReputationService service)
        {
            registeredService = service;
        }

        public static bool TryGetReputationService(out CCS_ReputationService reputationService)
        {
            reputationService = registeredService;
            if (reputationService != null && reputationService.IsInitialized)
            {
                return true;
            }

            reputationService = null;
            if (!TryGetServiceRegistry(out CCS_ServiceRegistry serviceRegistry))
            {
                return false;
            }

            return serviceRegistry.TryGetService(out reputationService)
                && reputationService != null
                && reputationService.IsInitialized;
        }

        private static bool TryGetServiceRegistry(out CCS_ServiceRegistry serviceRegistry)
        {
            serviceRegistry = null;
            CCS_RuntimeHost[] runtimeHosts = CCS_SurvivalSceneQueryUtility.FindActiveObjectsByType<CCS_RuntimeHost>();
            if (runtimeHosts == null || runtimeHosts.Length == 0)
            {
                return false;
            }

            serviceRegistry = runtimeHosts[0].ServiceRegistry;
            return serviceRegistry != null;
        }
    }
}
