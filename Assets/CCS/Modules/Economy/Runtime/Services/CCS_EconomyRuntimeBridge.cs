using CCS.Core;
using CCS.Modules.Inventory;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_EconomyRuntimeBridge
// CATEGORY: Modules / Economy / Runtime / Services
// PURPOSE: Resolves economy services from the runtime service registry.
// PLACEMENT: Used by vendor interactables, debug UI, and playtest harness.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// =============================================================================

namespace CCS.Modules.Economy
{
    public static class CCS_EconomyRuntimeBridge
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

        public static bool TryGetCurrencyService(out CCS_CurrencyService currencyService)
        {
            currencyService = null;
            if (!TryGetRuntimeHost(out CCS_RuntimeHost runtimeHost))
            {
                return false;
            }

            return runtimeHost.ServiceRegistry.TryGetService(out currencyService);
        }

        public static bool TryGetVendorService(out CCS_VendorService vendorService)
        {
            vendorService = null;
            if (!TryGetRuntimeHost(out CCS_RuntimeHost runtimeHost))
            {
                return false;
            }

            return runtimeHost.ServiceRegistry.TryGetService(out vendorService);
        }

        public static bool TryGetInventoryService(out CCS_PlayerInventoryService inventoryService)
        {
            inventoryService = null;
            if (!TryGetRuntimeHost(out CCS_RuntimeHost runtimeHost))
            {
                return false;
            }

            return runtimeHost.ServiceRegistry.TryGetService(out inventoryService);
        }
    }
}
