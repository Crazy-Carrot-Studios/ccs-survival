using CCS.Core;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_ContractRuntimeBridge
// CATEGORY: Modules / Contracts / Runtime / Services
// PURPOSE: Resolves CCS_ContractService from the active runtime service registry.
// PLACEMENT: Used by settlement routing and debug HUD.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.0.0 frontier contracts foundation.
// =============================================================================

namespace CCS.Modules.Contracts
{
    public static class CCS_ContractRuntimeBridge
    {
        private static CCS_ContractService registeredService;

        public static void Register(CCS_ContractService service)
        {
            registeredService = service;
        }

        public static bool TryGetContractService(out CCS_ContractService contractService)
        {
            contractService = registeredService;
            if (contractService != null && contractService.IsInitialized)
            {
                return true;
            }

            contractService = null;
            if (!TryGetServiceRegistry(out CCS_ServiceRegistry serviceRegistry))
            {
                return false;
            }

            return serviceRegistry.TryGetService(out contractService)
                && contractService != null
                && contractService.IsInitialized;
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
