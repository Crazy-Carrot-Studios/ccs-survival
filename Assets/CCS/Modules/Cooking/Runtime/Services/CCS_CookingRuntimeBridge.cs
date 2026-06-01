using CCS.Core;
using CCS.Modules.Interaction;
using CCS.Modules.Inventory;
using CCS.Modules.SurvivalCore;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_CookingRuntimeBridge
// CATEGORY: Modules / Cooking / Runtime / Services
// PURPOSE: Resolves gameplay services from the runtime registry for cooking systems.
// PLACEMENT: Used by CCS_CampfireInteractable and development test harnesses.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Safe when runtime host or services are missing. No singletons.
// =============================================================================

namespace CCS.Modules.Cooking
{
    public static class CCS_CookingRuntimeBridge
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

        public static bool TryGetCookingService(out CCS_CookingService cookingService)
        {
            cookingService = null;
            if (!TryGetRuntimeHost(out CCS_RuntimeHost runtimeHost))
            {
                return false;
            }

            return runtimeHost.ServiceRegistry.TryGetService(out cookingService);
        }

        public static bool TryGetCampfireService(out CCS_CampfireService campfireService)
        {
            campfireService = null;
            if (!TryGetRuntimeHost(out CCS_RuntimeHost runtimeHost))
            {
                return false;
            }

            return runtimeHost.ServiceRegistry.TryGetService(out campfireService);
        }

        public static bool TryGetConsumableFoodService(out CCS_ConsumableFoodService consumableFoodService)
        {
            consumableFoodService = null;
            if (!TryGetRuntimeHost(out CCS_RuntimeHost runtimeHost))
            {
                return false;
            }

            return runtimeHost.ServiceRegistry.TryGetService(out consumableFoodService);
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

        public static bool TryGetSurvivalCoreService(out CCS_SurvivalCoreService survivalCoreService)
        {
            survivalCoreService = null;
            if (!TryGetRuntimeHost(out CCS_RuntimeHost runtimeHost))
            {
                return false;
            }

            return runtimeHost.ServiceRegistry.TryGetService(out survivalCoreService);
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
