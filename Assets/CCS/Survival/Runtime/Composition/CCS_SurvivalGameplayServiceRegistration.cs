using CCS.Core;
using CCS.Modules.Crafting;
using CCS.Modules.Equipment;
using CCS.Modules.Interaction;
using CCS.Modules.Inventory;
using CCS.Modules.SaveLoad;
using CCS.Modules.SurvivalCore;
using CCS.Modules.EnvironmentEffects;
using CCS.Modules.TimeOfDay;
using CCS.Modules.Weather;
using CCS.Modules.Shelter;
using CCS.Modules.Building;
using CCS.Modules.WorldResources;

// =============================================================================
// SCRIPT: CCS_SurvivalGameplayServiceRegistration
// CATEGORY: Survival / Runtime / Composition
// PURPOSE: Registers gameplay module services on the runtime service registry from profiles.
// PLACEMENT: Invoked by CCS_SurvivalBootstrap after survival install pipeline completes.
// AUTHOR: James Schilz
// CREATED: 2026-05-30
// NOTES: Safe when profiles are missing. No singletons or scene name lookups.
// =============================================================================

namespace CCS.Survival.Composition
{
    public static class CCS_SurvivalGameplayServiceRegistration
    {
        private const string LogCategory = CCS_SurvivalRuntimeConstants.SurvivalBootstrapLogCategory;

        #region Public Methods

        public static void RegisterGameplayServices(
            CCS_RuntimeHost runtimeHost,
            CCS_SurvivalCoreProfile survivalCoreProfile,
            CCS_InteractionProfile interactionProfile,
            CCS_InventoryProfile inventoryProfile,
            CCS_EquipmentProfile equipmentProfile,
            CCS_WorldResourceProfile worldResourceProfile,
            CCS_CraftingProfile craftingProfile,
            CCS_SaveLoadProfile saveLoadProfile,
            CCS_TimeOfDayProfile timeOfDayProfile,
            CCS_WeatherProfile weatherProfile,
            CCS_ShelterProfile shelterProfile,
            CCS_EnvironmentEffectsProfile environmentEffectsProfile,
            CCS_BuildingProfile buildingProfile,
            bool enableDebugLogs = false)
        {
            if (runtimeHost == null)
            {
                return;
            }

            CCS_SurvivalCoreService survivalCoreService = CreateSurvivalCoreService(survivalCoreProfile);
            RegisterService(runtimeHost, survivalCoreService, enableDebugLogs);
            RegisterService(runtimeHost, CreateInteractionService(interactionProfile), enableDebugLogs);

            CCS_PlayerInventoryService inventoryService = CreateInventoryService(inventoryProfile);
            RegisterService(runtimeHost, inventoryService, enableDebugLogs);

            CCS_PlayerEquipmentService equipmentService = CreateEquipmentService(equipmentProfile);
            RegisterService(runtimeHost, equipmentService, enableDebugLogs);

            RegisterService(runtimeHost, CreateResourceHarvestService(worldResourceProfile), enableDebugLogs);
            RegisterService(runtimeHost, CreateResourceRespawnService(worldResourceProfile), enableDebugLogs);
            RegisterService(runtimeHost, CreateCraftingService(craftingProfile, inventoryService), enableDebugLogs);

            CCS_SaveLoadService saveLoadService = CreateSaveLoadService(saveLoadProfile);
            RegisterService(runtimeHost, saveLoadService, enableDebugLogs);

            CCS_TimeOfDayService timeOfDayService = CreateTimeOfDayService(timeOfDayProfile);
            RegisterService(runtimeHost, timeOfDayService, enableDebugLogs);
            RegisterTimeOfDayUpdatable(runtimeHost, timeOfDayService);

            CCS_WeatherService weatherService = CreateWeatherService(weatherProfile, timeOfDayService);
            RegisterService(runtimeHost, weatherService, enableDebugLogs);
            RegisterWeatherUpdatable(runtimeHost, weatherService);

            CCS_ShelterService shelterService = CreateShelterService(shelterProfile);
            RegisterService(runtimeHost, shelterService, enableDebugLogs);

            CCS_EnvironmentEffectsService environmentEffectsService = CreateEnvironmentEffectsService(
                environmentEffectsProfile,
                timeOfDayService,
                weatherService,
                shelterService,
                equipmentService);
            RegisterService(runtimeHost, environmentEffectsService, enableDebugLogs);
            RegisterEnvironmentEffectsUpdatable(runtimeHost, environmentEffectsService);

            CCS_BuildingService buildingService = CreateBuildingService(buildingProfile);
            RegisterService(runtimeHost, buildingService, enableDebugLogs);

            CCS_BuildingPlacementService placementService = CreateBuildingPlacementService(
                buildingProfile,
                buildingService,
                inventoryService);
            RegisterService(runtimeHost, placementService, enableDebugLogs);

            BindBuildingShelterIntegration(shelterService, buildingService);

            BindSurvivalCoreEnvironmentEffects(survivalCoreService, environmentEffectsService);
            RegisterSurvivalCoreUpdatable(runtimeHost, survivalCoreService);

            RegisterGameplaySaveables(
                saveLoadService,
                inventoryService,
                equipmentService,
                timeOfDayService,
                weatherService,
                shelterService,
                environmentEffectsService,
                buildingService);
        }

        #endregion

        #region Private Methods

        private static CCS_SurvivalCoreService CreateSurvivalCoreService(CCS_SurvivalCoreProfile profile)
        {
            CCS_SurvivalCoreService service = new CCS_SurvivalCoreService();
            service.Initialize();

            if (profile == null)
            {
                return service;
            }

            service.InitializeFromProfile(profile);
            return service;
        }

        private static CCS_InteractionService CreateInteractionService(CCS_InteractionProfile profile)
        {
            CCS_InteractionService service = new CCS_InteractionService();
            service.Initialize();

            if (profile == null)
            {
                return service;
            }

            service.InitializeFromProfile(profile);
            return service;
        }

        private static CCS_PlayerInventoryService CreateInventoryService(CCS_InventoryProfile profile)
        {
            CCS_PlayerInventoryService service = new CCS_PlayerInventoryService();
            service.Initialize();

            if (profile == null)
            {
                return service;
            }

            service.InitializeFromProfile(profile);
            return service;
        }

        private static CCS_PlayerEquipmentService CreateEquipmentService(CCS_EquipmentProfile profile)
        {
            CCS_PlayerEquipmentService service = new CCS_PlayerEquipmentService();
            service.Initialize();

            if (profile == null)
            {
                return service;
            }

            service.InitializeFromProfile(profile);
            return service;
        }

        private static CCS_ResourceHarvestService CreateResourceHarvestService(CCS_WorldResourceProfile profile)
        {
            CCS_ResourceHarvestService service = new CCS_ResourceHarvestService();
            service.Initialize();

            if (profile == null)
            {
                return service;
            }

            service.InitializeFromProfile(profile);
            return service;
        }

        private static CCS_ResourceRespawnService CreateResourceRespawnService(CCS_WorldResourceProfile profile)
        {
            CCS_ResourceRespawnService service = new CCS_ResourceRespawnService();
            service.Initialize();

            if (profile == null)
            {
                return service;
            }

            service.InitializeFromProfile(profile);
            return service;
        }

        private static CCS_CraftingService CreateCraftingService(
            CCS_CraftingProfile profile,
            CCS_PlayerInventoryService inventoryService)
        {
            CCS_CraftingService service = new CCS_CraftingService();
            service.Initialize();

            if (profile == null || inventoryService == null)
            {
                return service;
            }

            service.InitializeFromProfile(profile, inventoryService);
            return service;
        }

        private static CCS_SaveLoadService CreateSaveLoadService(CCS_SaveLoadProfile profile)
        {
            CCS_SaveLoadService service = new CCS_SaveLoadService();
            service.Initialize();

            if (profile == null)
            {
                return service;
            }

            service.InitializeFromProfile(profile);
            return service;
        }

        private static CCS_TimeOfDayService CreateTimeOfDayService(CCS_TimeOfDayProfile profile)
        {
            CCS_TimeOfDayService service = new CCS_TimeOfDayService();
            service.Initialize();

            if (profile == null)
            {
                return service;
            }

            service.InitializeFromProfile(profile);
            return service;
        }

        private static void RegisterTimeOfDayUpdatable(
            CCS_RuntimeHost runtimeHost,
            CCS_TimeOfDayService timeOfDayService)
        {
            if (runtimeHost == null || timeOfDayService == null || !timeOfDayService.IsInitialized)
            {
                return;
            }

            runtimeHost.RuntimeUpdateLoop.RegisterUpdatable(timeOfDayService);
        }

        private static CCS_WeatherService CreateWeatherService(
            CCS_WeatherProfile profile,
            CCS_TimeOfDayService timeOfDayService)
        {
            CCS_WeatherService service = new CCS_WeatherService();
            service.Initialize();

            if (profile == null)
            {
                return service;
            }

            service.InitializeFromProfile(profile);

            if (timeOfDayService != null && timeOfDayService.IsInitialized)
            {
                service.BindTimeOfDayService(timeOfDayService);
            }

            return service;
        }

        private static void RegisterWeatherUpdatable(
            CCS_RuntimeHost runtimeHost,
            CCS_WeatherService weatherService)
        {
            if (runtimeHost == null || weatherService == null || !weatherService.IsInitialized)
            {
                return;
            }

            runtimeHost.RuntimeUpdateLoop.RegisterUpdatable(weatherService);
        }

        private static CCS_ShelterService CreateShelterService(CCS_ShelterProfile profile)
        {
            CCS_ShelterService service = new CCS_ShelterService();
            service.Initialize();

            if (profile == null)
            {
                return service;
            }

            service.InitializeFromProfile(profile);
            return service;
        }

        private static CCS_BuildingService CreateBuildingService(CCS_BuildingProfile profile)
        {
            CCS_BuildingService service = new CCS_BuildingService();
            service.Initialize();

            if (profile == null)
            {
                return service;
            }

            service.InitializeFromProfile(profile);
            return service;
        }

        private static CCS_BuildingPlacementService CreateBuildingPlacementService(
            CCS_BuildingProfile profile,
            CCS_BuildingService buildingService,
            CCS_PlayerInventoryService inventoryService)
        {
            CCS_BuildingPlacementService service = new CCS_BuildingPlacementService();
            service.Initialize();

            if (profile == null || buildingService == null)
            {
                return service;
            }

            service.InitializeFromProfile(profile);

            if (buildingService.IsInitialized)
            {
                service.BindBuildingService(buildingService);
                buildingService.BindPlacementService(service);
            }

            if (inventoryService != null && inventoryService.IsInitialized)
            {
                service.BindInventoryService(inventoryService);
            }

            return service;
        }

        private static CCS_EnvironmentEffectsService CreateEnvironmentEffectsService(
            CCS_EnvironmentEffectsProfile profile,
            CCS_TimeOfDayService timeOfDayService,
            CCS_WeatherService weatherService,
            CCS_ShelterService shelterService,
            CCS_PlayerEquipmentService equipmentService)
        {
            CCS_EnvironmentEffectsService service = new CCS_EnvironmentEffectsService();
            service.Initialize();

            if (profile == null)
            {
                return service;
            }

            if (timeOfDayService != null && timeOfDayService.IsInitialized)
            {
                service.BindTimeOfDayService(timeOfDayService);
            }

            if (weatherService != null && weatherService.IsInitialized)
            {
                service.BindWeatherService(weatherService);
            }

            if (shelterService != null && shelterService.IsInitialized)
            {
                service.BindShelterService(shelterService);
            }

            if (equipmentService != null && equipmentService.IsInitialized)
            {
                service.BindEquipmentService(equipmentService);
            }

            service.InitializeFromProfile(profile);
            return service;
        }

        private static void RegisterEnvironmentEffectsUpdatable(
            CCS_RuntimeHost runtimeHost,
            CCS_EnvironmentEffectsService environmentEffectsService)
        {
            if (runtimeHost == null || environmentEffectsService == null || !environmentEffectsService.IsInitialized)
            {
                return;
            }

            runtimeHost.RuntimeUpdateLoop.RegisterUpdatable(environmentEffectsService);
        }

        private static void BindBuildingShelterIntegration(
            CCS_ShelterService shelterService,
            CCS_BuildingService buildingService)
        {
            if (shelterService == null
                || !shelterService.IsInitialized
                || buildingService == null
                || !buildingService.IsInitialized)
            {
                return;
            }

            shelterService.BindBuildingService(buildingService);
        }

        private static void BindSurvivalCoreEnvironmentEffects(
            CCS_SurvivalCoreService survivalCoreService,
            CCS_EnvironmentEffectsService environmentEffectsService)
        {
            if (survivalCoreService == null
                || !survivalCoreService.IsInitialized
                || environmentEffectsService == null
                || !environmentEffectsService.IsInitialized)
            {
                return;
            }

            survivalCoreService.BindEnvironmentEffectsService(environmentEffectsService);
        }

        private static void RegisterSurvivalCoreUpdatable(
            CCS_RuntimeHost runtimeHost,
            CCS_SurvivalCoreService survivalCoreService)
        {
            if (runtimeHost == null || survivalCoreService == null || !survivalCoreService.IsInitialized)
            {
                return;
            }

            runtimeHost.RuntimeUpdateLoop.RegisterUpdatable(survivalCoreService);
        }

        private static void RegisterGameplaySaveables(
            CCS_SaveLoadService saveLoadService,
            CCS_PlayerInventoryService inventoryService,
            CCS_PlayerEquipmentService equipmentService,
            CCS_TimeOfDayService timeOfDayService,
            CCS_WeatherService weatherService,
            CCS_ShelterService shelterService,
            CCS_EnvironmentEffectsService environmentEffectsService,
            CCS_BuildingService buildingService)
        {
            if (saveLoadService == null || !saveLoadService.IsInitialized)
            {
                return;
            }

            if (inventoryService != null && inventoryService.IsInitialized)
            {
                inventoryService.SetCapacityModifierSource(() =>
                {
                    if (equipmentService == null || !equipmentService.IsInitialized)
                    {
                        return CCS_InventoryCapacityModifierSnapshot.Empty;
                    }

                    return new CCS_InventoryCapacityModifierSnapshot(
                        equipmentService.GetAdditionalInventorySlots(),
                        equipmentService.GetAdditionalCarryWeight());
                });

                saveLoadService.RegisterSaveable(inventoryService);
            }

            if (equipmentService != null && equipmentService.IsInitialized)
            {
                saveLoadService.RegisterSaveable(equipmentService);
            }

            if (timeOfDayService != null && timeOfDayService.IsInitialized)
            {
                saveLoadService.RegisterSaveable(timeOfDayService);
            }

            if (weatherService != null && weatherService.IsInitialized)
            {
                saveLoadService.RegisterSaveable(weatherService);
            }

            if (shelterService != null && shelterService.IsInitialized)
            {
                saveLoadService.RegisterSaveable(shelterService);
            }

            if (environmentEffectsService != null && environmentEffectsService.IsInitialized)
            {
                saveLoadService.RegisterSaveable(environmentEffectsService);
            }

            if (buildingService != null && buildingService.IsInitialized)
            {
                saveLoadService.RegisterSaveable(buildingService);
            }
        }

        private static void RegisterService<TService>(
            CCS_RuntimeHost runtimeHost,
            TService service,
            bool enableDebugLogs)
            where TService : class, CCS_IService
        {
            if (service == null)
            {
                return;
            }

            if (runtimeHost.ServiceRegistry.RegisterService(service))
            {
                CCS_Logger.Log(
                    LogCategory,
                    $"Registered gameplay service: {typeof(TService).Name}",
                    enableDebugLogs);
                return;
            }

            CCS_Logger.LogWarning(
                LogCategory,
                $"Failed to register gameplay service: {typeof(TService).Name}");
        }

        #endregion
    }
}
