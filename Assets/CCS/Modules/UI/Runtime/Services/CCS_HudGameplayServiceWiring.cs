using CCS.Core;
using CCS.Modules.Crafting;
using CCS.Modules.Equipment;
using CCS.Modules.Interaction;
using CCS.Modules.Inventory;
using CCS.Modules.SurvivalCore;
using CCS.Modules.EnvironmentEffects;
using CCS.Modules.TimeOfDay;
using CCS.Modules.Weather;
using CCS.Modules.Building;
using CCS.Modules.WorldResources;
using CCS.Modules.Wildlife;
using CCS.Modules.Cooking;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_HudGameplayServiceWiring
// CATEGORY: Modules / UI / Runtime / Services
// PURPOSE: Resolves gameplay services from the runtime registry and binds HUD presentation.
// PLACEMENT: Called by CCS_HudRootPresenter during Start.
// AUTHOR: James Schilz
// CREATED: 2026-05-30
// NOTES: Read-only wiring. Safe when services or runtime host are missing.
// =============================================================================

namespace CCS.Modules.UI
{
    public static class CCS_HudGameplayServiceWiring
    {
        #region Public Methods

        public static bool TryWirePresentationService(CCS_HudPresentationService presentationService)
        {
            if (presentationService == null)
            {
                return false;
            }

            CCS_RuntimeHost[] runtimeHosts = CCS_SurvivalSceneQueryUtility.FindActiveObjectsByType<CCS_RuntimeHost>();
            if (runtimeHosts == null || runtimeHosts.Length == 0)
            {
                return false;
            }

            CCS_RuntimeHost runtimeHost = runtimeHosts[0];
            if (runtimeHost == null)
            {
                return false;
            }

            bool wiredAnyService = false;

            if (runtimeHost.ServiceRegistry.TryGetService(out CCS_SurvivalCoreService survivalCoreService))
            {
                presentationService.BindSurvivalCoreService(survivalCoreService);
                wiredAnyService = true;
            }

            if (runtimeHost.ServiceRegistry.TryGetService(out CCS_InteractionService interactionService))
            {
                presentationService.BindInteractionService(interactionService);
                wiredAnyService = true;
            }

            if (runtimeHost.ServiceRegistry.TryGetService(out CCS_PlayerInventoryService inventoryService))
            {
                presentationService.BindInventoryService(inventoryService);
                wiredAnyService = true;
            }

            if (runtimeHost.ServiceRegistry.TryGetService(out CCS_PlayerEquipmentService equipmentService))
            {
                presentationService.BindEquipmentService(equipmentService);
                wiredAnyService = true;
            }

            if (runtimeHost.ServiceRegistry.TryGetService(out CCS_ResourceHarvestService resourceHarvestService))
            {
                presentationService.BindResourceHarvestService(resourceHarvestService);
                wiredAnyService = true;
            }

            if (runtimeHost.ServiceRegistry.TryGetService(out CCS_ResourceRespawnService resourceRespawnService))
            {
                presentationService.BindResourceRespawnService(resourceRespawnService);
                wiredAnyService = true;
            }

            if (runtimeHost.ServiceRegistry.TryGetService(out CCS_WildlifeHarvestService wildlifeHarvestService))
            {
                presentationService.BindWildlifeHarvestService(wildlifeHarvestService);
                wiredAnyService = true;
            }

            if (runtimeHost.ServiceRegistry.TryGetService(out CCS_CookingService cookingService))
            {
                presentationService.BindCookingService(cookingService);
                wiredAnyService = true;
            }

            if (runtimeHost.ServiceRegistry.TryGetService(out CCS_CampfireService campfireService))
            {
                presentationService.BindCampfireService(campfireService);
                wiredAnyService = true;
            }

            if (runtimeHost.ServiceRegistry.TryGetService(out CCS_ConsumableFoodService consumableFoodService))
            {
                presentationService.BindConsumableFoodService(consumableFoodService);
                wiredAnyService = true;
            }

            if (runtimeHost.ServiceRegistry.TryGetService(out CCS_CraftingService craftingService))
            {
                presentationService.BindCraftingService(craftingService);
                wiredAnyService = true;
            }

            if (runtimeHost.ServiceRegistry.TryGetService(out CCS_TimeOfDayService timeOfDayService))
            {
                presentationService.BindTimeOfDayService(timeOfDayService);
                wiredAnyService = true;
            }

            if (runtimeHost.ServiceRegistry.TryGetService(out CCS_WeatherService weatherService))
            {
                presentationService.BindWeatherService(weatherService);
                wiredAnyService = true;
            }

            if (runtimeHost.ServiceRegistry.TryGetService(out CCS_EnvironmentEffectsService environmentEffectsService))
            {
                presentationService.BindEnvironmentEffectsService(environmentEffectsService);
                wiredAnyService = true;
            }

            if (runtimeHost.ServiceRegistry.TryGetService(out CCS_BuildingPlacementService buildingPlacementService))
            {
                presentationService.BindBuildingPlacementService(buildingPlacementService);
                wiredAnyService = true;
            }

            return wiredAnyService;
        }

        #endregion
    }
}
