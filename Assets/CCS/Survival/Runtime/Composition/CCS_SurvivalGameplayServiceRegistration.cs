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
using CCS.Modules.Wildlife;
using CCS.Modules.Cooking;
using CCS.Modules.Sleep;
using CCS.Modules.Combat;
using CCS.Modules.Fishing;
using CCS.Modules.Economy;
using CCS.Modules.Gathering;
using CCS.Modules.Hotbar;
using CCS.Modules.CharacterController;
using CCS.Modules.SaveSystem;
using CCS.Modules.PlayerDeath;
using CCS.Modules.Playtesting;
using CCS.Modules.Storage;
using CCS.Modules.Trapping;
using CCS.Survival.Player.Loadout;

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
            CCS_WildlifeProfile wildlifeProfile,
            CCS_WildlifeAiProfile wildlifeAiProfile,
            CCS_CookingProfile cookingProfile,
            CCS_SleepProfile sleepProfile,
            CCS_CombatProfile combatProfile,
            CCS_ActiveItemProfile activeItemProfile,
            CCS_GatheringProfile gatheringProfile,
            CCS_FishingProfile fishingProfile,
            CCS_TrapProfile trapProfile,
            CCS_CampDefinition campDefinition,
            CCS_EconomyProfile economyProfile,
            CCS_CraftingProfile craftingProfile,
            CCS_CraftingProgressionProfile craftingProgressionProfile,
            CCS_SaveLoadProfile saveLoadProfile,
            CCS_SaveProfile saveProfile,
            CCS_PlayerDeathProfile playerDeathProfile,
            CCS_PlaytestProfile playtestProfile,
            CCS_TimeOfDayProfile timeOfDayProfile,
            CCS_WeatherProfile weatherProfile,
            CCS_ShelterProfile shelterProfile,
            CCS_EnvironmentEffectsProfile environmentEffectsProfile,
            CCS_BuildingProfile buildingProfile,
            CCS_BuildingProgressionProfile buildingProgressionProfile,
            CCS_StorageProfile storageProfile,
            CCS_CharacterControllerProfile characterControllerProfile,
            CCS_StarterLoadoutProfile starterLoadoutProfile,
            bool enableDebugLogs = false)
        {
            if (runtimeHost == null)
            {
                return;
            }

            CCS_SurvivalCoreService survivalCoreService = CreateSurvivalCoreService(survivalCoreProfile);
            RegisterService(runtimeHost, survivalCoreService, enableDebugLogs);
            CCS_InteractionService interactionService = CreateInteractionService(interactionProfile);
            RegisterService(runtimeHost, interactionService, enableDebugLogs);

            CCS_PlayerInventoryService inventoryService = CreateInventoryService(inventoryProfile);
            RegisterService(runtimeHost, inventoryService, enableDebugLogs);

            CCS_PlayerEquipmentService equipmentService = CreateEquipmentService(equipmentProfile);
            RegisterService(runtimeHost, equipmentService, enableDebugLogs);

            CCS_ActiveItemService activeItemService =
                CreateActiveItemService(activeItemProfile, equipmentService);
            RegisterService(runtimeHost, activeItemService, enableDebugLogs);

            RegisterService(runtimeHost, CreateResourceHarvestService(worldResourceProfile), enableDebugLogs);
            RegisterService(runtimeHost, CreateResourceRespawnService(worldResourceProfile), enableDebugLogs);
            CCS_WildlifeHarvestService wildlifeHarvestService = CreateWildlifeHarvestService(wildlifeProfile);
            RegisterService(runtimeHost, wildlifeHarvestService, enableDebugLogs);
            RegisterService(runtimeHost, CreateWildlifeAiService(wildlifeAiProfile), enableDebugLogs);

            CCS_CraftingService craftingService = CreateCraftingService(craftingProfile, inventoryService);
            RegisterService(runtimeHost, craftingService, enableDebugLogs);

            CCS_CraftingRecipeService craftingRecipeService =
                CreateCraftingRecipeService(craftingProgressionProfile, craftingService);
            RegisterService(runtimeHost, craftingRecipeService, enableDebugLogs);
            if (craftingRecipeService.IsInitialized)
            {
                craftingRecipeService.BindCraftingService(craftingService);
            }

            CCS_StarterLoadoutService starterLoadoutService =
                CreateStarterLoadoutService(starterLoadoutProfile, inventoryService, craftingService);
            RegisterService(runtimeHost, starterLoadoutService, enableDebugLogs);

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

            CCS_BuildingRecipeService recipeService = CreateBuildingRecipeService(buildingProgressionProfile);
            RegisterService(runtimeHost, recipeService, enableDebugLogs);
            if (recipeService.IsInitialized)
            {
                recipeService.BindBuildingService(buildingService);
                recipeService.BindInventoryService(inventoryService);
                placementService.BindRecipeService(recipeService);
            }

            BindBuildingShelterIntegration(shelterService, buildingService);

            CCS_CookingService cookingService = CreateCookingService(cookingProfile, inventoryService);
            RegisterService(runtimeHost, cookingService, enableDebugLogs);
            RegisterCookingUpdatable(runtimeHost, cookingService);

            CCS_CampfireService campfireService = CreateCampfireService(
                cookingProfile,
                buildingService,
                placementService,
                cookingService,
                inventoryService);
            RegisterService(runtimeHost, campfireService, enableDebugLogs);

            CCS_ConsumableFoodService consumableFoodService =
                CreateConsumableFoodService(cookingProfile, inventoryService, survivalCoreService);
            RegisterService(runtimeHost, consumableFoodService, enableDebugLogs);

            CCS_SleepService sleepService = CreateSleepService(
                sleepProfile,
                survivalCoreService,
                timeOfDayService,
                shelterService,
                inventoryService,
                equipmentService,
                craftingService);
            RegisterService(runtimeHost, sleepService, enableDebugLogs);

            CCS_CombatService combatService = CreateCombatService(combatProfile, equipmentService);
            RegisterService(runtimeHost, combatService, enableDebugLogs);

            if (activeItemService != null && activeItemService.IsInitialized)
            {
                activeItemService.BindCombatService(combatService);
            }

            CCS_GatheringService gatheringService = CreateGatheringService(gatheringProfile, inventoryService);
            RegisterService(runtimeHost, gatheringService, enableDebugLogs);

            CCS_FishingService fishingService = CreateFishingService(fishingProfile, inventoryService);
            RegisterService(runtimeHost, fishingService, enableDebugLogs);

            CCS_TrapService trapService = CreateTrapService(
                trapProfile,
                inventoryService,
                wildlifeHarvestService);
            RegisterService(runtimeHost, trapService, enableDebugLogs);
            RegisterTrapUpdatable(runtimeHost, trapService);

            CCS_FrontierShelterService frontierShelterService = CreateFrontierShelterService(
                campDefinition,
                shelterService);
            RegisterService(runtimeHost, frontierShelterService, enableDebugLogs);
            RegisterFrontierShelterUpdatable(runtimeHost, frontierShelterService);

            CCS_CampService campService = CreateCampService(
                campDefinition,
                frontierShelterService,
                buildingService,
                sleepService,
                shelterService);
            RegisterService(runtimeHost, campService, enableDebugLogs);

            if (frontierShelterService.IsInitialized)
            {
                frontierShelterService.BindInventoryService(inventoryService);
                frontierShelterService.BindCampService(campService);
            }

            if (sleepService != null && sleepService.IsInitialized)
            {
                sleepService.BindCampService(campService);
                campService.BindBedrollProximityQuery(
                    (origin, radius) => sleepService.TryGetNearestSleepSpotWithinRadius(origin, radius, out _));
            }

            if (activeItemService != null && activeItemService.IsInitialized)
            {
                activeItemService.BindGatheringService(gatheringService);
                activeItemService.BindFishingService(fishingService);
                activeItemService.BindInteractionService(interactionService);
                activeItemService.BindInventoryService(inventoryService);
                activeItemService.BindTrapService(trapService);
                activeItemService.BindFrontierShelterService(frontierShelterService);
            }

            CCS_CharacterMovementService characterMovementService =
                CreateCharacterMovementService(characterControllerProfile);
            RegisterService(runtimeHost, characterMovementService, enableDebugLogs);
            RegisterCharacterMovementUpdatable(runtimeHost, characterMovementService);
            BindCharacterStaminaIntegration(survivalCoreService, characterMovementService);

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

            CCS_StorageService storageService = CreateStorageService(storageProfile, inventoryService);
            RegisterService(runtimeHost, storageService, enableDebugLogs);

            CCS_CurrencyService currencyService = CreateCurrencyService(economyProfile, inventoryService);
            RegisterService(runtimeHost, currencyService, enableDebugLogs);
            CCS_VendorService vendorService = CreateVendorService(economyProfile, currencyService, inventoryService);
            RegisterService(runtimeHost, vendorService, enableDebugLogs);

            CCS_SaveService saveService = CreateSaveService(saveProfile);
            RegisterService(runtimeHost, saveService, enableDebugLogs);
            saveService.BindGameplayServices(
                inventoryService,
                survivalCoreService,
                gatheringService,
                buildingService,
                storageService,
                sleepService,
                currencyService,
                trapService,
                frontierShelterService,
                campService,
                null);
            RegisterSaveSystemUpdatable(runtimeHost, saveService);

            CCS_PlayerDeathService playerDeathService = CreatePlayerDeathService(playerDeathProfile);
            playerDeathService.BindGameplayServices(
                survivalCoreService,
                characterMovementService,
                null);
            playerDeathService.BindAssignedRespawnSpawnIdProvider(
                () => sleepService != null && sleepService.IsInitialized
                    ? sleepService.AssignedRespawnSpawnId
                    : string.Empty);
            RegisterService(runtimeHost, playerDeathService, enableDebugLogs);
            RegisterPlayerDeathUpdatable(runtimeHost, playerDeathService);

            CCS_PlaytestService playtestService = CreatePlaytestService(playtestProfile);
            RegisterService(runtimeHost, playtestService, enableDebugLogs);
            if (playtestService.HarnessEnabled)
            {
                playtestService.BindEventListeners(
                    gatheringService,
                    combatService,
                    wildlifeHarvestService,
                    trapService,
                    cookingService,
                    consumableFoodService,
                    saveService,
                    playerDeathService,
                    placementService,
                    equipmentService,
                    activeItemService,
                    craftingRecipeService,
                    storageService,
                    sleepService,
                    survivalCoreService,
                    interactionService,
                    currencyService,
                    vendorService,
                    campService);
                RegisterPlaytestUpdatable(runtimeHost, playtestService);
            }
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

        private static CCS_WildlifeHarvestService CreateWildlifeHarvestService(CCS_WildlifeProfile profile)
        {
            CCS_WildlifeHarvestService service = new CCS_WildlifeHarvestService();
            service.Initialize();

            if (profile == null)
            {
                return service;
            }

            service.InitializeFromProfile(profile);
            return service;
        }

        private static CCS_WildlifeAiService CreateWildlifeAiService(CCS_WildlifeAiProfile profile)
        {
            CCS_WildlifeAiService service = new CCS_WildlifeAiService();
            service.Initialize();

            if (profile == null)
            {
                return service;
            }

            service.InitializeFromProfile(profile);
            return service;
        }

        private static CCS_CookingService CreateCookingService(
            CCS_CookingProfile profile,
            CCS_PlayerInventoryService inventoryService)
        {
            CCS_CookingService service = new CCS_CookingService();
            service.Initialize();

            if (profile == null)
            {
                return service;
            }

            service.InitializeFromProfile(profile, inventoryService);
            return service;
        }

        private static CCS_CampfireService CreateCampfireService(
            CCS_CookingProfile profile,
            CCS_BuildingService buildingService,
            CCS_BuildingPlacementService placementService,
            CCS_CookingService cookingService,
            CCS_PlayerInventoryService inventoryService)
        {
            CCS_CampfireService service = new CCS_CampfireService();
            service.Initialize();

            if (profile == null)
            {
                return service;
            }

            service.InitializeFromProfile(profile);

            if (buildingService != null && buildingService.IsInitialized)
            {
                service.BindBuildingService(buildingService);
            }

            if (placementService != null && placementService.IsInitialized)
            {
                service.BindPlacementService(placementService);
            }

            if (cookingService != null && cookingService.IsInitialized)
            {
                service.BindCookingService(cookingService);
                cookingService.BindCampfireService(service);
            }

            if (inventoryService != null && inventoryService.IsInitialized)
            {
                service.BindInventoryService(inventoryService);
            }

            return service;
        }

        private static CCS_ConsumableFoodService CreateConsumableFoodService(
            CCS_CookingProfile profile,
            CCS_PlayerInventoryService inventoryService,
            CCS_SurvivalCoreService survivalCoreService)
        {
            CCS_ConsumableFoodService service = new CCS_ConsumableFoodService();
            service.Initialize();

            if (profile == null)
            {
                return service;
            }

            service.InitializeFromProfile(profile);

            if (inventoryService != null && inventoryService.IsInitialized)
            {
                service.BindInventoryService(inventoryService);
            }

            if (survivalCoreService != null && survivalCoreService.IsInitialized)
            {
                service.BindSurvivalCoreService(survivalCoreService);
            }

            return service;
        }

        private static CCS_ActiveItemService CreateActiveItemService(
            CCS_ActiveItemProfile profile,
            CCS_PlayerEquipmentService equipmentService)
        {
            CCS_ActiveItemService service = new CCS_ActiveItemService();
            service.Initialize();

            if (profile == null)
            {
                return service;
            }

            service.InitializeFromProfile(profile);

            if (equipmentService != null && equipmentService.IsInitialized)
            {
                service.BindEquipmentService(equipmentService);
            }

            return service;
        }

        private static CCS_CombatService CreateCombatService(
            CCS_CombatProfile profile,
            CCS_PlayerEquipmentService equipmentService)
        {
            CCS_CombatService service = new CCS_CombatService();
            service.Initialize();

            if (profile == null)
            {
                return service;
            }

            service.InitializeFromProfile(profile);

            if (equipmentService != null && equipmentService.IsInitialized)
            {
                service.BindEquipmentService(equipmentService);
            }

            return service;
        }

        private static CCS_GatheringService CreateGatheringService(
            CCS_GatheringProfile profile,
            CCS_PlayerInventoryService inventoryService)
        {
            CCS_GatheringService service = new CCS_GatheringService();
            service.Initialize();

            if (profile == null)
            {
                return service;
            }

            service.InitializeFromProfile(profile);

            if (inventoryService != null && inventoryService.IsInitialized)
            {
                service.BindInventoryService(inventoryService);
            }

            return service;
        }

        private static CCS_FishingService CreateFishingService(
            CCS_FishingProfile profile,
            CCS_PlayerInventoryService inventoryService)
        {
            CCS_FishingService service = new CCS_FishingService();
            service.Initialize();

            if (profile == null)
            {
                return service;
            }

            service.InitializeFromProfile(profile);

            if (inventoryService != null && inventoryService.IsInitialized)
            {
                service.BindInventoryService(inventoryService);
            }

            return service;
        }

        private static CCS_TrapService CreateTrapService(
            CCS_TrapProfile profile,
            CCS_PlayerInventoryService inventoryService,
            CCS_WildlifeHarvestService wildlifeHarvestService)
        {
            CCS_TrapService service = new CCS_TrapService();
            service.Initialize();

            if (profile != null)
            {
                service.InitializeFromProfile(profile);
            }

            if (inventoryService != null && inventoryService.IsInitialized)
            {
                service.BindInventoryService(inventoryService);
            }

            if (wildlifeHarvestService != null && wildlifeHarvestService.IsInitialized)
            {
                service.BindWildlifeHarvestService(wildlifeHarvestService);
            }

            return service;
        }

        private static void RegisterTrapUpdatable(CCS_RuntimeHost runtimeHost, CCS_TrapService trapService)
        {
            if (runtimeHost?.RuntimeUpdateLoop == null || trapService == null || !trapService.IsInitialized)
            {
                return;
            }

            runtimeHost.RuntimeUpdateLoop.RegisterUpdatable(trapService);
        }

        private static CCS_FrontierShelterService CreateFrontierShelterService(
            CCS_CampDefinition campDefinition,
            CCS_ShelterService shelterService)
        {
            CCS_FrontierShelterService service = new CCS_FrontierShelterService();
            service.Initialize();

            if (campDefinition != null)
            {
                service.InitializeFromProfile(campDefinition);
            }

            if (shelterService != null && shelterService.IsInitialized)
            {
                service.BindShelterService(shelterService);
            }

            return service;
        }

        private static CCS_CampService CreateCampService(
            CCS_CampDefinition campDefinition,
            CCS_FrontierShelterService frontierShelterService,
            CCS_BuildingService buildingService,
            CCS_SleepService sleepService,
            CCS_ShelterService shelterService)
        {
            CCS_CampService service = new CCS_CampService();
            service.Initialize();

            if (campDefinition != null)
            {
                service.InitializeFromProfile(campDefinition);
            }

            if (frontierShelterService != null && frontierShelterService.IsInitialized)
            {
                service.BindFrontierShelterService(frontierShelterService);
            }

            if (buildingService != null && buildingService.IsInitialized)
            {
                service.BindBuildingService(buildingService);
            }

            if (shelterService != null && shelterService.IsInitialized)
            {
                service.BindShelterService(shelterService);
            }

            return service;
        }

        private static void RegisterFrontierShelterUpdatable(
            CCS_RuntimeHost runtimeHost,
            CCS_FrontierShelterService frontierShelterService)
        {
            if (runtimeHost?.RuntimeUpdateLoop == null
                || frontierShelterService == null
                || !frontierShelterService.IsInitialized)
            {
                return;
            }

            runtimeHost.RuntimeUpdateLoop.RegisterUpdatable(frontierShelterService);
        }

        private static CCS_SleepService CreateSleepService(
            CCS_SleepProfile profile,
            CCS_SurvivalCoreService survivalCoreService,
            CCS_TimeOfDayService timeOfDayService,
            CCS_ShelterService shelterService,
            CCS_PlayerInventoryService inventoryService,
            CCS_PlayerEquipmentService equipmentService,
            CCS_CraftingService craftingService)
        {
            CCS_SleepService service = new CCS_SleepService();
            service.Initialize();

            if (profile == null)
            {
                return service;
            }

            service.InitializeFromProfile(profile);

            if (survivalCoreService != null && survivalCoreService.IsInitialized)
            {
                service.BindSurvivalCoreService(survivalCoreService);
            }

            if (timeOfDayService != null && timeOfDayService.IsInitialized)
            {
                service.BindTimeOfDayService(timeOfDayService);
            }

            if (shelterService != null && shelterService.IsInitialized)
            {
                service.BindShelterService(shelterService);
            }

            if (inventoryService != null && inventoryService.IsInitialized)
            {
                service.BindInventoryService(inventoryService);
            }

            if (equipmentService != null && equipmentService.IsInitialized)
            {
                service.BindEquipmentService(equipmentService);
            }

            if (craftingService != null && craftingService.IsInitialized)
            {
                service.BindCraftingService(craftingService);
            }

            return service;
        }

        private static void RegisterCookingUpdatable(
            CCS_RuntimeHost runtimeHost,
            CCS_CookingService cookingService)
        {
            if (runtimeHost == null || cookingService == null || !cookingService.IsInitialized)
            {
                return;
            }

            runtimeHost.RuntimeUpdateLoop.RegisterUpdatable(cookingService);
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

        private static CCS_CraftingRecipeService CreateCraftingRecipeService(
            CCS_CraftingProgressionProfile profile,
            CCS_CraftingService craftingService)
        {
            CCS_CraftingRecipeService service = new CCS_CraftingRecipeService();
            service.Initialize();

            if (profile == null || craftingService == null)
            {
                return service;
            }

            service.InitializeFromProfile(profile, craftingService);
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

        private static CCS_BuildingRecipeService CreateBuildingRecipeService(CCS_BuildingProgressionProfile profile)
        {
            CCS_BuildingRecipeService service = new CCS_BuildingRecipeService();
            service.Initialize();

            if (profile == null)
            {
                return service;
            }

            service.InitializeFromProfile(profile);
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

        private static CCS_StarterLoadoutService CreateStarterLoadoutService(
            CCS_StarterLoadoutProfile profile,
            CCS_PlayerInventoryService inventoryService,
            CCS_CraftingService craftingService)
        {
            CCS_StarterLoadoutService service = new CCS_StarterLoadoutService();
            service.Initialize();

            if (profile == null)
            {
                return service;
            }

            service.InitializeFromProfile(profile);
            service.RegisterPrimitiveRecipes(craftingService);
            return service;
        }

        private static CCS_CurrencyService CreateCurrencyService(
            CCS_EconomyProfile profile,
            CCS_PlayerInventoryService inventoryService)
        {
            CCS_CurrencyService service = new CCS_CurrencyService();
            service.Initialize();

            if (profile != null)
            {
                service.InitializeFromProfile(profile);
            }

            if (service.IsInitialized && inventoryService != null)
            {
                service.BindInventoryService(inventoryService);
            }

            return service;
        }

        private static CCS_VendorService CreateVendorService(
            CCS_EconomyProfile profile,
            CCS_CurrencyService currencyService,
            CCS_PlayerInventoryService inventoryService)
        {
            CCS_VendorService service = new CCS_VendorService();
            service.Initialize();

            if (profile != null)
            {
                service.InitializeFromProfile(profile);
            }

            if (service.IsInitialized)
            {
                service.BindServices(currencyService, inventoryService);
            }

            return service;
        }

        private static CCS_StorageService CreateStorageService(
            CCS_StorageProfile profile,
            CCS_PlayerInventoryService inventoryService)
        {
            CCS_StorageService service = new CCS_StorageService();
            service.Initialize();

            if (profile != null)
            {
                service.InitializeFromProfile(profile);
            }

            if (service.IsInitialized && inventoryService != null)
            {
                service.BindInventoryService(inventoryService);
            }

            return service;
        }

        private static CCS_SaveService CreateSaveService(CCS_SaveProfile profile)
        {
            CCS_SaveService service = new CCS_SaveService();
            service.Initialize();

            if (profile == null)
            {
                return service;
            }

            service.InitializeFromProfile(profile);
            return service;
        }

        private static CCS_PlayerDeathService CreatePlayerDeathService(CCS_PlayerDeathProfile profile)
        {
            CCS_PlayerDeathService service = new CCS_PlayerDeathService();
            service.Initialize();

            if (profile == null)
            {
                return service;
            }

            service.InitializeFromProfile(profile);
            return service;
        }

        private static void RegisterSaveSystemUpdatable(CCS_RuntimeHost runtimeHost, CCS_SaveService saveService)
        {
            if (runtimeHost == null || saveService == null || !saveService.IsInitialized)
            {
                return;
            }

            runtimeHost.RuntimeUpdateLoop.RegisterUpdatable(saveService);
        }

        private static void RegisterPlayerDeathUpdatable(
            CCS_RuntimeHost runtimeHost,
            CCS_PlayerDeathService playerDeathService)
        {
            if (runtimeHost == null || playerDeathService == null || !playerDeathService.IsInitialized)
            {
                return;
            }

            runtimeHost.RuntimeUpdateLoop.RegisterUpdatable(playerDeathService);
        }

        private static CCS_PlaytestService CreatePlaytestService(CCS_PlaytestProfile profile)
        {
            CCS_PlaytestService service = new CCS_PlaytestService();
            service.Initialize();

            if (profile == null)
            {
                return service;
            }

            service.InitializeFromProfile(profile);
            return service;
        }

        private static void RegisterPlaytestUpdatable(
            CCS_RuntimeHost runtimeHost,
            CCS_PlaytestService playtestService)
        {
            if (runtimeHost == null || playtestService == null || !playtestService.IsInitialized)
            {
                return;
            }

            runtimeHost.RuntimeUpdateLoop.RegisterUpdatable(playtestService);
        }

        private static void BindCharacterStaminaIntegration(
            CCS_SurvivalCoreService survivalCoreService,
            CCS_CharacterMovementService characterMovementService)
        {
            if (survivalCoreService == null
                || !survivalCoreService.IsInitialized
                || characterMovementService == null)
            {
                return;
            }

            characterMovementService.SprintStateChanged += isSprinting =>
            {
                survivalCoreService.StaminaDrainActive = isSprinting;
            };

            characterMovementService.Jumped += _ =>
            {
                float jumpCost = characterMovementService.ActiveProfile != null
                    ? characterMovementService.ActiveProfile.Movement.JumpStaminaCost
                    : 0f;

                if (jumpCost <= 0f)
                {
                    return;
                }

                survivalCoreService.TryApplyModifier(
                    CCS_SurvivalStatType.Stamina,
                    CCS_SurvivalStatModifier.Add(-jumpCost));
            };
        }

        private static CCS_CharacterMovementService CreateCharacterMovementService(
            CCS_CharacterControllerProfile profile)
        {
            CCS_CharacterMovementService service = new CCS_CharacterMovementService();
            service.Initialize();
            return service;
        }

        private static void RegisterCharacterMovementUpdatable(
            CCS_RuntimeHost runtimeHost,
            CCS_CharacterMovementService characterMovementService)
        {
            if (runtimeHost == null || characterMovementService == null)
            {
                return;
            }

            runtimeHost.RuntimeUpdateLoop.RegisterUpdatable(characterMovementService);
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
