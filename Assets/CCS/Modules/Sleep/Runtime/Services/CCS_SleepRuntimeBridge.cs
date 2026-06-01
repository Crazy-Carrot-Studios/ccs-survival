using CCS.Core;
using CCS.Modules.Equipment;
using CCS.Modules.Inventory;
using CCS.Modules.Interaction;
using CCS.Modules.Shelter;
using CCS.Modules.SurvivalCore;
using CCS.Modules.TimeOfDay;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_SleepRuntimeBridge
// CATEGORY: Modules / Sleep / Runtime / Services
// PURPOSE: Resolves gameplay services from the runtime registry for sleep systems.
// PLACEMENT: Used by CCS_BedrollSleepInteractable and development test harnesses.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Safe when runtime host or services are missing. No singletons.
// =============================================================================

namespace CCS.Modules.Sleep
{
    public static class CCS_SleepRuntimeBridge
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

        public static bool TryGetSleepService(out CCS_SleepService sleepService)
        {
            sleepService = null;
            if (!TryGetRuntimeHost(out CCS_RuntimeHost runtimeHost))
            {
                return false;
            }

            return runtimeHost.ServiceRegistry.TryGetService(out sleepService);
        }

        public static bool TryGetShelterService(out CCS_ShelterService shelterService)
        {
            shelterService = null;
            if (!TryGetRuntimeHost(out CCS_RuntimeHost runtimeHost))
            {
                return false;
            }

            return runtimeHost.ServiceRegistry.TryGetService(out shelterService);
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

        public static bool TryGetTimeOfDayService(out CCS_TimeOfDayService timeOfDayService)
        {
            timeOfDayService = null;
            if (!TryGetRuntimeHost(out CCS_RuntimeHost runtimeHost))
            {
                return false;
            }

            return runtimeHost.ServiceRegistry.TryGetService(out timeOfDayService);
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
