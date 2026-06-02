using CCS.Core;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_SettlementRuntimeBridge
// CATEGORY: Modules / Settlements / Runtime / Services
// PURPOSE: Resolves settlement services from the runtime service registry.
// PLACEMENT: Used by settlement locations, service points, and playtest harness.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// =============================================================================

namespace CCS.Modules.Settlements
{
    public static class CCS_SettlementRuntimeBridge
    {
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

        public static bool TryGetSettlementService(out CCS_SettlementService settlementService)
        {
            settlementService = null;
            if (!TryGetRuntimeHost(out CCS_RuntimeHost runtimeHost))
            {
                return false;
            }

            return runtimeHost.ServiceRegistry.TryGetService(out settlementService);
        }
    }
}
