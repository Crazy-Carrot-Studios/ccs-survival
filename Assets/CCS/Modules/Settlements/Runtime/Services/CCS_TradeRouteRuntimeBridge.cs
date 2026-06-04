using CCS.Core;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_TradeRouteRuntimeBridge
// CATEGORY: Modules / Settlements / Runtime / Services
// PURPOSE: Resolves CCS_TradeRouteService from the active runtime service registry.
// PLACEMENT: Used by contract freight flows and playtest harness.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.4.0 trade routes and freight contracts.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public static class CCS_TradeRouteRuntimeBridge
    {
        private static CCS_TradeRouteService registeredService;

        public static void Register(CCS_TradeRouteService service)
        {
            registeredService = service;
        }

        public static bool TryGetTradeRouteService(out CCS_TradeRouteService tradeRouteService)
        {
            tradeRouteService = registeredService;
            if (tradeRouteService != null && tradeRouteService.IsInitialized)
            {
                return true;
            }

            tradeRouteService = null;
            if (!TryGetServiceRegistry(out CCS_ServiceRegistry serviceRegistry))
            {
                return false;
            }

            return serviceRegistry.TryGetService(out tradeRouteService)
                && tradeRouteService != null
                && tradeRouteService.IsInitialized;
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
