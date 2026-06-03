using CCS.Core;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_BankingRuntimeBridge
// CATEGORY: Modules / Banking / Runtime / Services
// PURPOSE: Resolves banking service from the runtime registry.
// PLACEMENT: Used by settlement routing, debug HUD, and playtest harness.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 2.4.0 banking and land office foundation.
// =============================================================================

namespace CCS.Modules.Banking
{
    public static class CCS_BankingRuntimeBridge
    {
        private static CCS_BankingService registeredService;

        public static void Register(CCS_BankingService service)
        {
            registeredService = service;
        }

        public static bool TryGetBankingService(out CCS_BankingService bankingService)
        {
            bankingService = registeredService;
            if (bankingService != null && bankingService.IsInitialized)
            {
                return true;
            }

            bankingService = null;
            if (!TryGetServiceRegistry(out CCS_ServiceRegistry serviceRegistry))
            {
                return false;
            }

            return serviceRegistry.TryGetService(out bankingService)
                && bankingService != null
                && bankingService.IsInitialized;
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
