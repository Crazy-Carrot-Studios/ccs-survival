using CCS.Core;
using CCS.Modules.Interaction;
using CCS.Modules.Inventory;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_WorldResourceRuntimeBridge
// CATEGORY: Modules / WorldResources / Runtime / Services
// PURPOSE: Resolves gameplay services from the runtime registry for resource harvesting.
// PLACEMENT: Used by harvestable resources and development test harnesses.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Safe when runtime host or services are missing. No singletons.
// =============================================================================

namespace CCS.Modules.WorldResources
{
    public static class CCS_WorldResourceRuntimeBridge
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

        public static bool TryGetHarvestService(out CCS_ResourceHarvestService harvestService)
        {
            harvestService = null;
            if (!TryGetRuntimeHost(out CCS_RuntimeHost runtimeHost))
            {
                return false;
            }

            return runtimeHost.ServiceRegistry.TryGetService(out harvestService);
        }

        public static bool TryGetRespawnService(out CCS_ResourceRespawnService respawnService)
        {
            respawnService = null;
            if (!TryGetRuntimeHost(out CCS_RuntimeHost runtimeHost))
            {
                return false;
            }

            return runtimeHost.ServiceRegistry.TryGetService(out respawnService);
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

        public static bool TryGetInteractionService(out CCS_InteractionService interactionService)
        {
            interactionService = null;
            if (!TryGetRuntimeHost(out CCS_RuntimeHost runtimeHost))
            {
                return false;
            }

            return runtimeHost.ServiceRegistry.TryGetService(out interactionService);
        }

        #endregion
    }
}
