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
using CCS.Modules.Industry;
using CCS.Modules.Mounts;
using CCS.Modules.Ranching;
using CCS.Modules.Farming;
using CCS.Modules.Land;
using CCS.Modules.Banking;
using CCS.Modules.Upkeep;
using CCS.Modules.Vehicles;
using CCS.Modules.Firearms;
using CCS.Modules.NPCs;
using CCS.Modules.Settlements;
using CCS.Modules.Regions;
using CCS.Modules.Reputation;
using CCS.Modules.Contracts;
using CCS.Modules.WorldSimulation;
using CCS.Survival.Player;
using CCS.Survival.Player.Loadout;
using UnityEngine;

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
            CCS_FrontierStorageCampProfile frontierStorageCampProfile,
            CCS_IndustryProfile industryProfile,
            CCS_MountProfile mountProfile,
            CCS_LivestockProfile livestockProfile,
            CCS_CropProfile farmingProfile,
            CCS_LandClaimProfile landClaimProfile,
            CCS_BankAccountProfile bankAccountProfile,
            CCS_UpkeepProfile upkeepProfile,
            CCS_VehicleProfile vehicleProfile,
            CCS_FirearmProfile firearmProfile,
            CCS_SettlementProfile settlementProfile,
            CCS_ReputationProfile reputationProfile,
            CCS_ContractProfile contractsProfile,
            CCS_RegionProfile regionProfile,
            CCS_WorldSimulationProfile worldSimulationProfile,
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

            CCS_FirearmService firearmService = CreateFirearmService(
                runtimeHost,
                firearmProfile,
                inventoryService,
                equipmentService,
                combatService);
            RegisterService(runtimeHost, firearmService, enableDebugLogs);

            if (activeItemService != null && activeItemService.IsInitialized && firearmService.IsInitialized)
            {
                activeItemService.BindFirearmService(firearmService);
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

            CCS_FrontierStoragePlacementService frontierStoragePlacementService = CreateFrontierStoragePlacementService(
                frontierStorageCampProfile,
                storageService,
                campService);
            RegisterService(runtimeHost, frontierStoragePlacementService, enableDebugLogs);

            CCS_IndustryService industryService = CreateIndustryService(
                runtimeHost,
                industryProfile,
                inventoryService,
                craftingService);
            RegisterService(runtimeHost, industryService, enableDebugLogs);

            CCS_MountService mountService = CreateMountService(runtimeHost, mountProfile, inventoryService);
            RegisterService(runtimeHost, mountService, enableDebugLogs);

            CCS_RanchService ranchService = CreateRanchService(runtimeHost, livestockProfile, inventoryService, campService);
            RegisterService(runtimeHost, ranchService, enableDebugLogs);

            CCS_FarmService farmService = CreateFarmService(runtimeHost, farmingProfile, inventoryService, campService);
            RegisterService(runtimeHost, farmService, enableDebugLogs);

            CCS_LandClaimService landClaimService = CreateLandClaimService(
                runtimeHost,
                landClaimProfile,
                inventoryService);
            RegisterService(runtimeHost, landClaimService, enableDebugLogs);
            WireLandClaimStructureIntegration(
                landClaimService,
                farmService,
                ranchService,
                frontierShelterService);

            CCS_VehicleService vehicleService = CreateVehicleService(
                runtimeHost,
                vehicleProfile,
                inventoryService,
                mountService);
            RegisterService(runtimeHost, vehicleService, enableDebugLogs);

            CCS_FrontierHomesteadStructureService homesteadStructureService = CreateFrontierHomesteadStructureService(
                campDefinition,
                campService,
                industryService);
            RegisterService(runtimeHost, homesteadStructureService, enableDebugLogs);

            if (industryService.IsInitialized)
            {
                industryService.BindWorkstationRoleProximityQuery(
                    (origin, radius, roleId) => homesteadStructureService != null
                        && homesteadStructureService.IsInitialized
                        && homesteadStructureService.HasIndustryWorkstationRoleInRadius(origin, radius, roleId));
            }

            if (frontierStoragePlacementService.IsInitialized)
            {
                frontierStoragePlacementService.BindInventoryService(inventoryService);
            }

            if (homesteadStructureService.IsInitialized)
            {
                homesteadStructureService.BindInventoryService(inventoryService);
                if (industryService != null && industryService.IsInitialized)
                {
                    homesteadStructureService.BindIndustryService(industryService);
                }
            }

            if (campService != null && campService.IsInitialized)
            {
                campService.BindHomesteadStructureService(homesteadStructureService);
                campService.BindStorageProximityQuery(
                    (origin, radius) => frontierStoragePlacementService != null
                        && frontierStoragePlacementService.IsInitialized
                        && frontierStoragePlacementService.HasStorageInRadius(origin, radius));
                if (mountService != null && mountService.IsInitialized)
                {
                    campService.BindMountPresenceQuery(
                        (origin, radius) => mountService.HasHorseCampPresence(origin, radius));
                }

                if (ranchService != null && ranchService.IsInitialized)
                {
                    campService.BindRanchStructureProximityQuery(
                        (origin, radius) => ranchService.HasCampContributingStructureInRadius(origin, radius));
                }

                if (landClaimService != null && landClaimService.IsInitialized)
                {
                    campService.BindLandClaimQuery(
                        origin => landClaimService.TryResolveClaimIdContainingPosition(origin));
                }
            }

            if (activeItemService != null && activeItemService.IsInitialized)
            {
                if (landClaimService != null && landClaimService.IsInitialized)
                {
                    activeItemService.BindFrontierLandClaimPlacementHandler(
                        (itemDefinition, placementRequest) =>
                            TryHandleFrontierLandClaimPlacement(
                                landClaimService,
                                itemDefinition,
                                placementRequest));
                }

                if (frontierStoragePlacementService.IsInitialized)
                {
                    activeItemService.BindFrontierStoragePlacementHandler(
                        (itemDefinition, placementRequest) =>
                            TryHandleFrontierStoragePlacement(
                                frontierStoragePlacementService,
                                itemDefinition,
                                placementRequest));
                }

                if (ranchService != null && ranchService.IsInitialized)
                {
                    activeItemService.BindFrontierRanchPlacementHandler(
                        (itemDefinition, placementRequest) =>
                            TryHandleFrontierRanchPlacement(
                                ranchService,
                                itemDefinition,
                                placementRequest));
                }

                if (farmService != null && farmService.IsInitialized)
                {
                    activeItemService.BindFrontierFarmPlotPlacementHandler(
                        (itemDefinition, placementRequest) =>
                            TryHandleFrontierFarmPlotPlacement(
                                farmService,
                                itemDefinition,
                                placementRequest));

                    activeItemService.BindFrontierFarmSeedHandler(
                        (itemDefinition, placementRequest) =>
                            TryHandleFrontierFarmSeed(
                                farmService,
                                itemDefinition,
                                placementRequest));
                }

                if (homesteadStructureService.IsInitialized)
                {
                    activeItemService.BindFrontierHomesteadStructureService(homesteadStructureService);
                }
            }

            CCS_CurrencyService currencyService = CreateCurrencyService(economyProfile, inventoryService);
            RegisterService(runtimeHost, currencyService, enableDebugLogs);
            CCS_BankingService bankingService = CreateBankingService(
                bankAccountProfile,
                currencyService,
                landClaimService);
            RegisterService(runtimeHost, bankingService, enableDebugLogs);
            CCS_UpkeepService upkeepService = CreateUpkeepService(
                upkeepProfile,
                currencyService,
                bankingService,
                timeOfDayService);
            RegisterService(runtimeHost, upkeepService, enableDebugLogs);
            WireUpkeepLandClaimIntegration(upkeepService, landClaimService);
            CCS_VendorService vendorService = CreateVendorService(economyProfile, currencyService, inventoryService);
            RegisterService(runtimeHost, vendorService, enableDebugLogs);

            CCS_SettlementService settlementService = CreateSettlementService(settlementProfile);
            RegisterService(runtimeHost, settlementService, enableDebugLogs);

            CCS_ReputationService reputationService = CreateReputationService(reputationProfile);
            RegisterService(runtimeHost, reputationService, enableDebugLogs);
            if (reputationService.IsInitialized)
            {
                settlementService.BindReputationService(reputationService);
                if (vendorService != null && vendorService.IsInitialized)
                {
                    vendorService.BindReputationService(reputationService);
                }
            }

            CCS_RegionService regionService = CreateRegionService(regionProfile);
            RegisterService(runtimeHost, regionService, enableDebugLogs);

            if (landClaimService != null && landClaimService.IsInitialized && regionService.IsInitialized)
            {
                landClaimService.BindRegionResolver(_ => regionService.CurrentRegionId);
            }

            CCS_BusinessService businessService = CreateBusinessService(worldSimulationProfile?.SettlementBusinessProfile);
            RegisterService(runtimeHost, businessService, enableDebugLogs);

            CCS_BusinessPresenceService businessPresenceService =
                CreateBusinessPresenceService(worldSimulationProfile?.SettlementBusinessPresenceProfile);
            RegisterService(runtimeHost, businessPresenceService, enableDebugLogs);

            CCS_SettlementVisualGrowthService settlementVisualGrowthService =
                CreateSettlementVisualGrowthService(worldSimulationProfile?.SettlementVisualGrowthProfile);
            RegisterService(runtimeHost, settlementVisualGrowthService, enableDebugLogs);

            CCS_PopulationPresenceService populationPresenceService =
                CreatePopulationPresenceService(worldSimulationProfile?.SettlementPopulationPresenceProfile);
            RegisterService(runtimeHost, populationPresenceService, enableDebugLogs);

            CCS_NpcIdentityService npcIdentityService =
                CreateNpcIdentityService(worldSimulationProfile?.SettlementNpcIdentityProfile);
            RegisterService(runtimeHost, npcIdentityService, enableDebugLogs);

            CCS_NpcServiceRepresentativeService npcServiceRepresentativeService =
                CreateNpcServiceRepresentativeService(
                    worldSimulationProfile?.SettlementNpcServiceRepresentativeProfile);
            RegisterService(runtimeHost, npcServiceRepresentativeService, enableDebugLogs);

            CCS_SettlementHousingService settlementHousingService =
                CreateSettlementHousingService(worldSimulationProfile?.SettlementHousingProfile);
            RegisterService(runtimeHost, settlementHousingService, enableDebugLogs);

            CCS_NpcMovementService npcMovementService =
                CreateNpcMovementService(worldSimulationProfile?.SettlementNpcMovementProfile);
            RegisterService(runtimeHost, npcMovementService, enableDebugLogs);

            CCS_NpcScheduleService npcScheduleService =
                CreateNpcScheduleService(worldSimulationProfile?.SettlementNpcScheduleProfile);
            RegisterService(runtimeHost, npcScheduleService, enableDebugLogs);

            CCS_WorldSimulationService worldSimulationService = CreateWorldSimulationService(worldSimulationProfile);
            RegisterService(runtimeHost, worldSimulationService, enableDebugLogs);
            if (worldSimulationService.IsInitialized)
            {
                worldSimulationService.BindGameplayServices(
                    settlementService,
                    regionService,
                    reputationService,
                    businessService);
            }

            WireSettlementGrowth(settlementService, worldSimulationService);
            WireSettlementPopulation(settlementService, worldSimulationService);
            WireSettlementBusinesses(settlementService, worldSimulationService, businessService);
            WireBusinessPresence(
                settlementService,
                businessService,
                businessPresenceService,
                worldSimulationService);
            WireSettlementVisualGrowth(
                settlementService,
                settlementVisualGrowthService,
                worldSimulationService);
            WirePopulationPresence(
                settlementService,
                populationPresenceService,
                worldSimulationService);
            WireNpcIdentity(
                settlementService,
                populationPresenceService,
                npcIdentityService,
                worldSimulationService);
            WireNpcServiceRepresentatives(
                businessService,
                populationPresenceService,
                npcIdentityService,
                npcServiceRepresentativeService,
                worldSimulationService);
            WireSettlementHousing(
                settlementService,
                settlementHousingService,
                worldSimulationService);
            WireNpcSchedule(
                settlementService,
                npcScheduleService,
                worldSimulationService,
                timeOfDayService);
            WireNpcMovement(
                settlementService,
                npcIdentityService,
                npcMovementService,
                npcScheduleService,
                worldSimulationService,
                timeOfDayService,
                worldSimulationProfile?.SettlementHousingProfile);
            RegisterNpcMovementUpdatable(runtimeHost, npcMovementService);

            if (vendorService != null && vendorService.IsInitialized && worldSimulationService.IsInitialized)
            {
                vendorService.VendorTransactionCompleted += worldSimulationService.HandleVendorTransactionCompleted;
            }

            WireReputationEventHooks(
                reputationService,
                vendorService,
                bankingService,
                upkeepService,
                settlementService);

            CCS_TradeRouteService tradeRouteService = CreateTradeRouteService(
                settlementProfile?.TradeRouteProfile,
                settlementService);
            RegisterService(runtimeHost, tradeRouteService, enableDebugLogs);

            CCS_ContractService contractService = CreateContractService(
                contractsProfile,
                inventoryService,
                currencyService,
                reputationService,
                worldSimulationService,
                regionService,
                storageService,
                vehicleService,
                tradeRouteService);
            RegisterService(runtimeHost, contractService, enableDebugLogs);

            if (mountService != null && mountService.IsInitialized && vendorService != null && vendorService.IsInitialized)
            {
                vendorService.VendorTransactionCompleted += result =>
                {
                    if (result == null || !result.IsSuccess || result.WasSell)
                    {
                        return;
                    }

                    mountService.TryConsumeHorsePurchaseItem(result.ItemDefinition);
                    if (vehicleService != null && vehicleService.IsInitialized)
                    {
                        vehicleService.TryConsumeWagonPurchaseItem(result.ItemDefinition);
                    }

                    if (ranchService != null && ranchService.IsInitialized)
                    {
                        ranchService.TryConsumeLivestockPurchaseItem(result.ItemDefinition);
                    }
                };
            }

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
                homesteadStructureService,
                frontierStoragePlacementService,
                industryService,
                mountService,
                vehicleService,
                firearmService,
                settlementService,
                regionService,
                worldSimulationService,
                ranchService,
                farmService,
                landClaimService,
                bankingService,
                upkeepService,
                reputationService,
                contractService,
                tradeRouteService,
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
                    campService,
                    mountService,
                    vehicleService,
                    firearmService,
                    settlementService,
                    regionService,
                    worldSimulationService,
                    ranchService,
                    farmService,
                    landClaimService,
                    bankingService,
                    upkeepService,
                    reputationService,
                    contractService,
                    tradeRouteService);
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

        private static CCS_FrontierStoragePlacementService CreateFrontierStoragePlacementService(
            CCS_FrontierStorageCampProfile frontierStorageCampProfile,
            CCS_StorageService storageService,
            CCS_CampService campService)
        {
            CCS_FrontierStoragePlacementService service = new CCS_FrontierStoragePlacementService();
            service.Initialize();

            if (frontierStorageCampProfile != null)
            {
                service.InitializeFromProfile(frontierStorageCampProfile);
            }

            if (storageService != null && storageService.IsInitialized)
            {
                service.BindStorageService(storageService);
            }

            if (campService != null && campService.IsInitialized)
            {
                service.BindCampStructureChangedCallback(campService.RecalculateCamp);
            }

            return service;
        }

        private static CCS_ActiveItemUseResult TryHandleFrontierStoragePlacement(
            CCS_FrontierStoragePlacementService storagePlacementService,
            CCS_ItemDefinition itemDefinition,
            CCS_ActiveItemUseRequest request)
        {
            if (storagePlacementService == null
                || !storagePlacementService.IsInitialized
                || itemDefinition == null
                || !storagePlacementService.TryResolveStorageDefinitionForItem(
                    itemDefinition,
                    out CCS_StorageContainerDefinition storageDefinition))
            {
                return new CCS_ActiveItemUseResult(
                    CCS_ActiveItemUseResultType.NoBehaviorRegistered,
                    "Item is not a supported frontier storage kit.",
                    true,
                    itemDefinition?.ItemId ?? string.Empty);
            }

            bool confirmPlacement = storagePlacementService.IsPlacementModeActive;
            CCS_FrontierStoragePlacementRequest placementRequest = new CCS_FrontierStoragePlacementRequest(
                storageDefinition.ContainerId,
                request.UseOrigin,
                request.UseDirection,
                confirmPlacement);

            CCS_FrontierStoragePlacementResult placementResult =
                storagePlacementService.HandlePlacementRequest(placementRequest);
            if (!placementResult.IsSuccess)
            {
                return new CCS_ActiveItemUseResult(
                    CCS_ActiveItemUseResultType.HomesteadStructurePlacementFailed,
                    placementResult.Message,
                    true,
                    itemDefinition.ItemId);
            }

            if (placementResult.IsPreview)
            {
                return new CCS_ActiveItemUseResult(
                    placementResult.IsValid
                        ? CCS_ActiveItemUseResultType.HomesteadStructurePlacementPreview
                        : CCS_ActiveItemUseResultType.HomesteadStructurePlacementFailed,
                    placementResult.Message,
                    true,
                    itemDefinition.ItemId);
            }

            return new CCS_ActiveItemUseResult(
                CCS_ActiveItemUseResultType.HomesteadStructurePlaced,
                placementResult.Message,
                true,
                itemDefinition.ItemId);
        }

        private static CCS_RanchService CreateRanchService(
            CCS_RuntimeHost runtimeHost,
            CCS_LivestockProfile livestockProfile,
            CCS_PlayerInventoryService inventoryService,
            CCS_CampService campService)
        {
            CCS_RanchService service = new CCS_RanchService();
            service.Initialize();

            if (livestockProfile != null)
            {
                service.InitializeFromProfile(livestockProfile);
            }

            if (inventoryService != null && inventoryService.IsInitialized)
            {
                service.BindInventoryService(inventoryService);
            }

            if (campService != null && campService.IsInitialized)
            {
                service.BindCampService(campService);
            }

            service.BindPlayerPositionProvider(() =>
            {
                CCS_PlayerGameplayController[] controllers =
                    Object.FindObjectsByType<CCS_PlayerGameplayController>();
                if (controllers != null && controllers.Length > 0 && controllers[0] != null)
                {
                    return controllers[0].transform.position;
                }

                return Vector3.zero;
            });

            CCS_RanchRuntimeBridge.Register(service);
            if (runtimeHost?.RuntimeUpdateLoop != null)
            {
                runtimeHost.RuntimeUpdateLoop.RegisterUpdatable(service);
            }

            return service;
        }

        private static CCS_ActiveItemUseResult TryHandleFrontierRanchPlacement(
            CCS_RanchService ranchService,
            CCS_ItemDefinition itemDefinition,
            CCS_ActiveItemUseRequest request)
        {
            if (ranchService == null
                || !ranchService.IsInitialized
                || itemDefinition == null
                || !ranchService.TryResolveStructureDefinitionForItem(itemDefinition, out CCS_RanchStructureDefinition structureDefinition))
            {
                return new CCS_ActiveItemUseResult(
                    CCS_ActiveItemUseResultType.NoBehaviorRegistered,
                    "Item is not a supported ranch structure kit.",
                    true,
                    itemDefinition?.ItemId ?? string.Empty);
            }

            bool confirmPlacement = ranchService.IsPlacementModeActive;
            CCS_RanchPlacementRequest placementRequest = new CCS_RanchPlacementRequest(
                structureDefinition.StructureDefinitionId,
                request.UseOrigin,
                request.UseDirection,
                confirmPlacement);

            CCS_RanchPlacementResult placementResult = ranchService.HandlePlacementRequest(placementRequest);
            if (!placementResult.IsSuccess)
            {
                return new CCS_ActiveItemUseResult(
                    CCS_ActiveItemUseResultType.HomesteadStructurePlacementFailed,
                    placementResult.Message,
                    true,
                    itemDefinition.ItemId);
            }

            if (placementResult.IsPreview)
            {
                return new CCS_ActiveItemUseResult(
                    placementResult.IsValid
                        ? CCS_ActiveItemUseResultType.HomesteadStructurePlacementPreview
                        : CCS_ActiveItemUseResultType.HomesteadStructurePlacementFailed,
                    placementResult.Message,
                    true,
                    itemDefinition.ItemId);
            }

            return new CCS_ActiveItemUseResult(
                CCS_ActiveItemUseResultType.HomesteadStructurePlaced,
                placementResult.Message,
                true,
                itemDefinition.ItemId);
        }

        private static CCS_FarmService CreateFarmService(
            CCS_RuntimeHost runtimeHost,
            CCS_CropProfile farmingProfile,
            CCS_PlayerInventoryService inventoryService,
            CCS_CampService campService)
        {
            CCS_FarmService service = new CCS_FarmService();
            service.Initialize();

            if (farmingProfile != null)
            {
                service.InitializeFromProfile(farmingProfile);
            }

            if (inventoryService != null && inventoryService.IsInitialized)
            {
                service.BindInventoryService(inventoryService);
            }

            if (campService != null && campService.IsInitialized)
            {
                service.BindCampService(campService);
            }

            service.BindPlayerPositionProvider(() =>
            {
                CCS_PlayerGameplayController[] controllers =
                    Object.FindObjectsByType<CCS_PlayerGameplayController>();
                if (controllers != null && controllers.Length > 0 && controllers[0] != null)
                {
                    return controllers[0].transform.position;
                }

                return Vector3.zero;
            });

            CCS_FarmRuntimeBridge.Register(service);
            if (runtimeHost?.RuntimeUpdateLoop != null)
            {
                runtimeHost.RuntimeUpdateLoop.RegisterUpdatable(service);
            }

            return service;
        }

        private static CCS_ActiveItemUseResult TryHandleFrontierFarmPlotPlacement(
            CCS_FarmService farmService,
            CCS_ItemDefinition itemDefinition,
            CCS_ActiveItemUseRequest request)
        {
            if (farmService == null
                || !farmService.IsInitialized
                || itemDefinition == null
                || !farmService.TryResolvePlotDefinitionForItem(itemDefinition, out CCS_FarmPlotDefinition plotDefinition))
            {
                return new CCS_ActiveItemUseResult(
                    CCS_ActiveItemUseResultType.NoBehaviorRegistered,
                    "Item is not a supported farm plot kit.",
                    true,
                    itemDefinition?.ItemId ?? string.Empty);
            }

            bool confirmPlacement = farmService.IsPlacementModeActive;
            CCS_FarmPlacementRequest placementRequest = new CCS_FarmPlacementRequest(
                plotDefinition.PlotDefinitionId,
                request.UseOrigin,
                request.UseDirection,
                confirmPlacement);

            CCS_FarmPlacementResult placementResult = farmService.HandlePlacementRequest(placementRequest);
            if (!placementResult.IsSuccess)
            {
                return new CCS_ActiveItemUseResult(
                    CCS_ActiveItemUseResultType.HomesteadStructurePlacementFailed,
                    placementResult.Message,
                    true,
                    itemDefinition.ItemId);
            }

            if (placementResult.IsPreview)
            {
                return new CCS_ActiveItemUseResult(
                    placementResult.IsValid
                        ? CCS_ActiveItemUseResultType.HomesteadStructurePlacementPreview
                        : CCS_ActiveItemUseResultType.HomesteadStructurePlacementFailed,
                    placementResult.Message,
                    true,
                    itemDefinition.ItemId);
            }

            return new CCS_ActiveItemUseResult(
                CCS_ActiveItemUseResultType.HomesteadStructurePlaced,
                placementResult.Message,
                true,
                itemDefinition.ItemId);
        }

        private static CCS_ActiveItemUseResult TryHandleFrontierFarmSeed(
            CCS_FarmService farmService,
            CCS_ItemDefinition itemDefinition,
            CCS_ActiveItemUseRequest request)
        {
            if (farmService == null
                || !farmService.IsInitialized
                || itemDefinition == null
                || !farmService.TryResolveCropDefinitionForSeedItem(itemDefinition, out CCS_CropDefinition _))
            {
                return new CCS_ActiveItemUseResult(
                    CCS_ActiveItemUseResultType.NoBehaviorRegistered,
                    "Item is not a supported crop seed.",
                    true,
                    itemDefinition?.ItemId ?? string.Empty);
            }

            if (!farmService.TryPlantSeedInNearestPlot(itemDefinition))
            {
                return new CCS_ActiveItemUseResult(
                    CCS_ActiveItemUseResultType.HomesteadStructurePlacementFailed,
                    "No empty farm plot nearby for planting.",
                    true,
                    itemDefinition.ItemId);
            }

            return new CCS_ActiveItemUseResult(
                CCS_ActiveItemUseResultType.HomesteadStructurePlaced,
                $"Planted {itemDefinition.DisplayName}.",
                true,
                itemDefinition.ItemId);
        }

        private static CCS_LandClaimService CreateLandClaimService(
            CCS_RuntimeHost runtimeHost,
            CCS_LandClaimProfile landClaimProfile,
            CCS_PlayerInventoryService inventoryService)
        {
            CCS_LandClaimService service = new CCS_LandClaimService();
            service.Initialize();

            if (landClaimProfile != null)
            {
                service.InitializeFromProfile(landClaimProfile);
            }

            if (inventoryService != null && inventoryService.IsInitialized)
            {
                service.BindInventoryService(inventoryService);
            }

            CCS_LandClaimRuntimeBridge.Register(service);
            return service;
        }

        private static CCS_BankingService CreateBankingService(
            CCS_BankAccountProfile bankAccountProfile,
            CCS_CurrencyService currencyService,
            CCS_LandClaimService landClaimService)
        {
            CCS_BankingService service = new CCS_BankingService();
            service.Initialize();

            if (bankAccountProfile != null)
            {
                service.InitializeFromProfile(bankAccountProfile);
            }

            if (currencyService != null && currencyService.IsInitialized)
            {
                service.BindCurrencyService(currencyService);
            }

            if (landClaimService != null && landClaimService.IsInitialized)
            {
                service.BindLandClaimService(landClaimService);
            }

            CCS_BankingRuntimeBridge.Register(service);
            return service;
        }

        private static CCS_UpkeepService CreateUpkeepService(
            CCS_UpkeepProfile upkeepProfile,
            CCS_CurrencyService currencyService,
            CCS_BankingService bankingService,
            CCS_TimeOfDayService timeOfDayService)
        {
            CCS_UpkeepService service = new CCS_UpkeepService();
            service.Initialize();

            if (upkeepProfile != null)
            {
                service.InitializeFromProfile(upkeepProfile);
            }

            if (currencyService != null && currencyService.IsInitialized)
            {
                service.BindCurrencyService(currencyService);
            }

            if (bankingService != null && bankingService.IsInitialized)
            {
                service.BindBankingService(bankingService);
            }

            if (timeOfDayService != null && timeOfDayService.IsInitialized)
            {
                service.BindCurrentDayProvider(() => timeOfDayService.CreateSnapshot().DayNumber);
            }

            CCS_UpkeepRuntimeBridge.Register(service);
            WireUpkeepLandOfficeHud(service);
            return service;
        }

        private static void WireUpkeepLandOfficeHud(CCS_UpkeepService service)
        {
            if (service == null)
            {
                return;
            }

            CCS_BankingDebugHud.BindUpkeepHandlers(
                (string targetId, out string statusLabel, out int amountDue) =>
                {
                    statusLabel = "no entry";
                    amountDue = 0;
                    if (!service.TryGetEntryForTarget(targetId, out CCS_UpkeepEntry entry) || entry == null)
                    {
                        return false;
                    }

                    statusLabel = ((CCS_UpkeepState)entry.status).ToString();
                    amountDue = entry.amountDue;
                    return true;
                },
                (string targetId, out string summary) =>
                {
                    summary = string.Empty;
                    if (string.IsNullOrWhiteSpace(targetId))
                    {
                        summary = "Pay upkeep failed: no nearby land claim selected.";
                        return false;
                    }

                    CCS_UpkeepTransactionResult result = service.TryPayUpkeep(targetId);
                    if (result == null)
                    {
                        summary = "Pay upkeep failed: null result.";
                        return false;
                    }

                    summary =
                        $"Upkeep: {result.ResultType} | state {result.EntryState} | amount {result.Amount} | "
                        + $"source {result.PaymentSource} | {result.Message}";
                    return result.IsSuccess;
                });

            service.UpkeepTransactionCompleted += result =>
            {
                if (result == null)
                {
                    CCS_BankingDebugHud.NotifyUpkeepSummary("Last upkeep: null result");
                    return;
                }

                CCS_BankingDebugHud.NotifyUpkeepSummary(
                    $"Upkeep: {result.ResultType} | state {result.EntryState} | amount {result.Amount} | "
                    + $"source {result.PaymentSource} | {result.Message}");
            };
        }

        private static void WireUpkeepLandClaimIntegration(
            CCS_UpkeepService upkeepService,
            CCS_LandClaimService landClaimService)
        {
            if (upkeepService == null
                || !upkeepService.IsInitialized
                || landClaimService == null
                || !landClaimService.IsInitialized)
            {
                return;
            }

            landClaimService.LandClaimPlaced += claim =>
            {
                if (claim != null)
                {
                    upkeepService.TryRegisterLandClaimUpkeep(claim);
                }
            };

            upkeepService.ReconcileLandClaimEntries(landClaimService);
        }

        private static void WireLandClaimStructureIntegration(
            CCS_LandClaimService landClaimService,
            CCS_FarmService farmService,
            CCS_RanchService ranchService,
            CCS_FrontierShelterService frontierShelterService)
        {
            if (landClaimService == null || !landClaimService.IsInitialized)
            {
                return;
            }

            landClaimService.BindStructureScanProvider(() =>
            {
                System.Collections.Generic.List<CCS_LandClaimService.StructureRegistration> registrations =
                    new System.Collections.Generic.List<CCS_LandClaimService.StructureRegistration>();

                if (farmService != null && farmService.IsInitialized)
                {
                    CCS_FarmPlotSnapshot[] plots = farmService.CapturePlotState();
                    for (int index = 0; index < plots.Length; index++)
                    {
                        CCS_FarmPlotSnapshot plot = plots[index];
                        if (plot == null)
                        {
                            continue;
                        }

                        registrations.Add(new CCS_LandClaimService.StructureRegistration(
                            plot.instanceId,
                            new Vector3(plot.positionX, plot.positionY, plot.positionZ),
                            plot.campOwnerId,
                            CCS_LandClaimStructureKind.FarmPlot));
                    }
                }

                if (ranchService != null && ranchService.IsInitialized)
                {
                    CCS_RanchStructureSnapshot[] structures = ranchService.CaptureStructureState();
                    for (int index = 0; index < structures.Length; index++)
                    {
                        CCS_RanchStructureSnapshot structure = structures[index];
                        if (structure == null)
                        {
                            continue;
                        }

                        registrations.Add(new CCS_LandClaimService.StructureRegistration(
                            structure.instanceId,
                            new Vector3(structure.positionX, structure.positionY, structure.positionZ),
                            structure.campOwnerId,
                            CCS_LandClaimStructureKind.RanchStructure));
                    }
                }

                if (frontierShelterService != null && frontierShelterService.IsInitialized)
                {
                    CCS_FrontierShelterInstanceSaveState[] shelters = frontierShelterService.CaptureWorldState();
                    for (int index = 0; index < shelters.Length; index++)
                    {
                        CCS_FrontierShelterInstanceSaveState shelter = shelters[index];
                        if (shelter == null)
                        {
                            continue;
                        }

                        registrations.Add(new CCS_LandClaimService.StructureRegistration(
                            shelter.InstanceId,
                            shelter.Position,
                            shelter.CampOwnerId,
                            CCS_LandClaimStructureKind.Shelter));
                    }
                }

                return registrations;
            });

            if (farmService != null && farmService.IsInitialized)
            {
                farmService.FarmPlotPlaced += plot =>
                {
                    if (plot == null)
                    {
                        return;
                    }

                    landClaimService.TryAssociateStructure(
                        plot.InstanceId,
                        plot.WorldPosition,
                        plot.CampOwnerId,
                        CCS_LandClaimStructureKind.FarmPlot);
                };
            }

            if (ranchService != null && ranchService.IsInitialized)
            {
                ranchService.RanchStructurePlaced += structure =>
                {
                    if (structure == null)
                    {
                        return;
                    }

                    landClaimService.TryAssociateStructure(
                        structure.InstanceId,
                        structure.WorldPosition,
                        CCS_LandContentIds.DefaultCampOwnerId,
                        CCS_LandClaimStructureKind.RanchStructure);
                };
            }

            if (frontierShelterService != null && frontierShelterService.IsInitialized)
            {
                frontierShelterService.ShelterPlaced += shelter =>
                {
                    if (shelter == null)
                    {
                        return;
                    }

                    landClaimService.TryAssociateStructure(
                        shelter.InstanceId,
                        shelter.WorldPosition,
                        CCS_LandContentIds.DefaultCampOwnerId,
                        CCS_LandClaimStructureKind.Shelter);
                };
            }
        }

        private static CCS_ActiveItemUseResult TryHandleFrontierLandClaimPlacement(
            CCS_LandClaimService landClaimService,
            CCS_ItemDefinition itemDefinition,
            CCS_ActiveItemUseRequest request)
        {
            if (landClaimService == null
                || !landClaimService.IsInitialized
                || itemDefinition == null
                || !landClaimService.TryResolveClaimDefinitionForDeedItem(
                    itemDefinition,
                    out CCS_LandClaimDefinition claimDefinition))
            {
                return new CCS_ActiveItemUseResult(
                    CCS_ActiveItemUseResultType.NoBehaviorRegistered,
                    "Item is not a supported land claim deed.",
                    true,
                    itemDefinition?.ItemId ?? string.Empty);
            }

            bool confirmPlacement = landClaimService.IsPlacementModeActive;
            CCS_LandClaimPlacementRequest placementRequest = new CCS_LandClaimPlacementRequest(
                claimDefinition.ClaimDefinitionId,
                request.UseOrigin,
                request.UseDirection,
                confirmPlacement);

            CCS_LandClaimPlacementResult placementResult = landClaimService.HandlePlacementRequest(placementRequest);
            if (!placementResult.IsSuccess)
            {
                return new CCS_ActiveItemUseResult(
                    CCS_ActiveItemUseResultType.HomesteadStructurePlacementFailed,
                    placementResult.Message,
                    true,
                    itemDefinition.ItemId);
            }

            if (placementResult.IsPreview)
            {
                return new CCS_ActiveItemUseResult(
                    placementResult.IsValid
                        ? CCS_ActiveItemUseResultType.HomesteadStructurePlacementPreview
                        : CCS_ActiveItemUseResultType.HomesteadStructurePlacementFailed,
                    placementResult.Message,
                    true,
                    itemDefinition.ItemId);
            }

            return new CCS_ActiveItemUseResult(
                CCS_ActiveItemUseResultType.HomesteadStructurePlaced,
                placementResult.Message,
                true,
                itemDefinition.ItemId);
        }

        private static CCS_MountService CreateMountService(
            CCS_RuntimeHost runtimeHost,
            CCS_MountProfile mountProfile,
            CCS_PlayerInventoryService inventoryService)
        {
            CCS_MountService service = new CCS_MountService();
            service.Initialize();

            if (mountProfile != null)
            {
                service.InitializeFromProfile(mountProfile);
            }

            if (inventoryService != null && inventoryService.IsInitialized)
            {
                service.BindInventoryService(inventoryService);
            }

            CCS_MountRuntimeBridge.Register(service);
            if (runtimeHost?.RuntimeUpdateLoop != null)
            {
                runtimeHost.RuntimeUpdateLoop.RegisterUpdatable(service);
            }

            return service;
        }

        private static CCS_VehicleService CreateVehicleService(
            CCS_RuntimeHost runtimeHost,
            CCS_VehicleProfile vehicleProfile,
            CCS_PlayerInventoryService inventoryService,
            CCS_MountService mountService)
        {
            CCS_VehicleService service = new CCS_VehicleService();
            service.Initialize();

            if (vehicleProfile != null)
            {
                service.InitializeFromProfile(vehicleProfile);
            }

            if (inventoryService != null && inventoryService.IsInitialized)
            {
                service.BindInventoryService(inventoryService);
            }

            if (mountService != null && mountService.IsInitialized)
            {
                service.BindMountService(mountService);
            }

            CCS_VehicleRuntimeBridge.Register(service);
            if (runtimeHost?.RuntimeUpdateLoop != null)
            {
                runtimeHost.RuntimeUpdateLoop.RegisterUpdatable(service);
            }

            return service;
        }

        private static CCS_FirearmService CreateFirearmService(
            CCS_RuntimeHost runtimeHost,
            CCS_FirearmProfile firearmProfile,
            CCS_PlayerInventoryService inventoryService,
            CCS_PlayerEquipmentService equipmentService,
            CCS_CombatService combatService)
        {
            CCS_FirearmService service = new CCS_FirearmService();
            service.Initialize();

            if (firearmProfile != null)
            {
                service.InitializeFromProfile(firearmProfile);
            }

            if (inventoryService != null && inventoryService.IsInitialized)
            {
                service.BindInventoryService(inventoryService);
            }

            if (equipmentService != null && equipmentService.IsInitialized)
            {
                service.BindEquipmentService(equipmentService);
            }

            if (combatService != null && combatService.IsInitialized)
            {
                service.BindCombatService(combatService);
            }

            CCS_FirearmRuntimeBridge.Register(service);
            return service;
        }

        private static CCS_IndustryService CreateIndustryService(
            CCS_RuntimeHost runtimeHost,
            CCS_IndustryProfile industryProfile,
            CCS_PlayerInventoryService inventoryService,
            CCS_CraftingService craftingService)
        {
            CCS_IndustryService service = new CCS_IndustryService();
            service.Initialize();

            if (industryProfile != null)
            {
                service.InitializeFromProfile(industryProfile);
            }

            if (inventoryService != null && inventoryService.IsInitialized)
            {
                service.BindInventoryService(inventoryService);
            }

            if (craftingService != null && craftingService.IsInitialized)
            {
                service.BindCraftingService(craftingService);
            }

            CCS_IndustryRuntimeBridge.Register(service);
            if (runtimeHost?.RuntimeUpdateLoop != null)
            {
                runtimeHost.RuntimeUpdateLoop.RegisterUpdatable(service);
            }

            return service;
        }

        private static CCS_FrontierHomesteadStructureService CreateFrontierHomesteadStructureService(
            CCS_CampDefinition campDefinition,
            CCS_CampService campService,
            CCS_IndustryService industryService)
        {
            CCS_FrontierHomesteadStructureService service = new CCS_FrontierHomesteadStructureService();
            service.Initialize();

            if (campDefinition != null)
            {
                service.InitializeFromProfile(campDefinition);
            }

            if (campService != null && campService.IsInitialized)
            {
                service.BindCampService(campService);
            }

            if (industryService != null && industryService.IsInitialized)
            {
                service.BindIndustryService(industryService);
            }

            return service;
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

        private static CCS_SettlementService CreateSettlementService(CCS_SettlementProfile profile)
        {
            CCS_SettlementService service = new CCS_SettlementService();
            service.Initialize();
            if (profile != null)
            {
                service.InitializeFromProfile(profile);
            }

            return service;
        }

        private static CCS_ReputationService CreateReputationService(CCS_ReputationProfile profile)
        {
            CCS_ReputationService service = new CCS_ReputationService();
            service.Initialize();
            if (profile != null)
            {
                service.InitializeFromProfile(profile);
            }

            CCS_ReputationRuntimeBridge.Register(service);
            return service;
        }

        private static CCS_TradeRouteService CreateTradeRouteService(
            CCS_TradeRouteProfile profile,
            CCS_SettlementService settlementService)
        {
            CCS_TradeRouteService service = new CCS_TradeRouteService();
            service.Initialize();
            if (profile != null)
            {
                service.InitializeFromProfile(profile);
            }

            if (service.IsInitialized)
            {
                service.BindSettlementService(settlementService);
            }

            CCS_TradeRouteRuntimeBridge.Register(service);
            return service;
        }

        private static CCS_ContractService CreateContractService(
            CCS_ContractProfile profile,
            CCS_PlayerInventoryService inventoryService,
            CCS_CurrencyService currencyService,
            CCS_ReputationService reputationService,
            CCS_WorldSimulationService worldSimulationService,
            CCS_RegionService regionService,
            CCS_StorageService storageService,
            CCS_VehicleService vehicleService,
            CCS_TradeRouteService tradeRouteService)
        {
            CCS_ContractService service = new CCS_ContractService();
            service.Initialize();
            if (profile != null)
            {
                service.InitializeFromProfile(profile);
            }

            if (service.IsInitialized)
            {
                service.BindServices(
                    inventoryService,
                    currencyService,
                    reputationService,
                    worldSimulationService,
                    regionService,
                    storageService,
                    vehicleService,
                    tradeRouteService);
            }

            CCS_ContractRuntimeBridge.Register(service);
            WireContractBoardActivation(service);
            return service;
        }

        private static void WireSettlementGrowth(
            CCS_SettlementService settlementService,
            CCS_WorldSimulationService worldSimulationService)
        {
            if (settlementService == null || worldSimulationService == null)
            {
                return;
            }

            settlementService.BindGrowthSnapshotResolver(settlementId =>
            {
                if (worldSimulationService.TryGetGrowthSnapshot(settlementId, out CCS_SettlementGrowthSnapshot snapshot))
                {
                    return snapshot;
                }

                return CCS_SettlementGrowthSnapshot.Empty;
            });

            worldSimulationService.SettlementGrowthChanged += settlementService.NotifySettlementGrowthChanged;
        }

        private static void WireSettlementPopulation(
            CCS_SettlementService settlementService,
            CCS_WorldSimulationService worldSimulationService)
        {
            if (settlementService == null || worldSimulationService == null)
            {
                return;
            }

            settlementService.BindPopulationSnapshotResolver(settlementId =>
            {
                if (worldSimulationService.TryGetPopulationSnapshot(settlementId, out CCS_SettlementPopulationSnapshot snapshot))
                {
                    return snapshot;
                }

                return CCS_SettlementPopulationSnapshot.Empty;
            });

            worldSimulationService.SettlementPopulationChanged += settlementService.NotifySettlementPopulationChanged;
        }

        private static void WireSettlementBusinesses(
            CCS_SettlementService settlementService,
            CCS_WorldSimulationService worldSimulationService,
            CCS_BusinessService businessService)
        {
            if (settlementService == null || worldSimulationService == null || businessService == null)
            {
                return;
            }

            settlementService.BindBusinessSnapshotResolver(settlementId =>
            {
                if (worldSimulationService.TryGetBusinessSnapshot(settlementId, out CCS_BusinessSnapshot snapshot))
                {
                    return snapshot;
                }

                return CCS_BusinessSnapshot.Empty;
            });

            businessService.BindBusinessSnapshotResolver(settlementId =>
            {
                if (worldSimulationService.TryGetBusinessSnapshot(settlementId, out CCS_BusinessSnapshot snapshot))
                {
                    return snapshot;
                }

                return CCS_BusinessSnapshot.Empty;
            });

            businessService.BusinessActivated += settlementService.NotifyBusinessActivated;
            businessService.BusinessDeactivated += settlementService.NotifyBusinessDeactivated;

            CCS_BusinessRuntimeBridge.IsBusinessActiveAtSettlement = (settlementId, businessType) =>
            {
                if (!worldSimulationService.TryGetSettlementState(settlementId, out CCS_SettlementSimulationState state)
                    || state == null)
                {
                    return false;
                }

                return CCS_BusinessValidationUtility.IsBusinessActive(state, businessType);
            };
        }

        private static CCS_BusinessService CreateBusinessService(CCS_BusinessProfile profile)
        {
            CCS_BusinessService service = new CCS_BusinessService();
            service.Initialize();
            if (profile != null)
            {
                service.InitializeFromProfile(profile);
            }

            return service;
        }

        private static CCS_BusinessPresenceService CreateBusinessPresenceService(CCS_BusinessPresenceProfile profile)
        {
            CCS_BusinessPresenceService service = new CCS_BusinessPresenceService();
            service.Initialize();
            if (profile != null)
            {
                service.InitializeFromProfile(profile);
            }

            return service;
        }

        private static void WireBusinessPresence(
            CCS_SettlementService settlementService,
            CCS_BusinessService businessService,
            CCS_BusinessPresenceService businessPresenceService,
            CCS_WorldSimulationService worldSimulationService)
        {
            if (settlementService == null || businessPresenceService == null || worldSimulationService == null)
            {
                return;
            }

            businessPresenceService.BindBusinessSnapshotResolver(settlementId =>
            {
                if (worldSimulationService.TryGetBusinessSnapshot(settlementId, out CCS_BusinessSnapshot snapshot))
                {
                    return snapshot;
                }

                return CCS_BusinessSnapshot.Empty;
            });

            if (businessService != null && businessService.IsInitialized)
            {
                businessService.BusinessActivated += businessPresenceService.HandleBusinessActivated;
                businessService.BusinessDeactivated += businessPresenceService.HandleBusinessDeactivated;
            }

            settlementService.SettlementDiscovered += businessPresenceService.HandleSettlementDiscovered;

            businessPresenceService.RefreshAllAnchors();
        }

        private static CCS_SettlementVisualGrowthService CreateSettlementVisualGrowthService(
            CCS_SettlementVisualGrowthProfile profile)
        {
            CCS_SettlementVisualGrowthService service = new CCS_SettlementVisualGrowthService();
            service.Initialize();
            if (profile != null)
            {
                service.InitializeFromProfile(profile);
            }

            return service;
        }

        private static void WireSettlementVisualGrowth(
            CCS_SettlementService settlementService,
            CCS_SettlementVisualGrowthService visualGrowthService,
            CCS_WorldSimulationService worldSimulationService)
        {
            if (settlementService == null || visualGrowthService == null || worldSimulationService == null)
            {
                return;
            }

            visualGrowthService.BindGrowthSnapshotResolver(settlementId =>
            {
                if (worldSimulationService.TryGetGrowthSnapshot(settlementId, out CCS_SettlementGrowthSnapshot snapshot))
                {
                    return snapshot;
                }

                return CCS_SettlementGrowthSnapshot.Empty;
            });

            visualGrowthService.BindSettlementDiscoveredResolver(settlementId =>
                settlementService.IsInitialized && settlementService.IsDiscovered(settlementId));

            settlementService.SettlementGrowthChanged += visualGrowthService.HandleSettlementGrowthChanged;
            settlementService.SettlementDiscovered += visualGrowthService.HandleSettlementDiscovered;

            visualGrowthService.RefreshAllAnchors();
        }

        private static CCS_PopulationPresenceService CreatePopulationPresenceService(
            CCS_PopulationPresenceProfile profile)
        {
            CCS_PopulationPresenceService service = new CCS_PopulationPresenceService();
            service.Initialize();
            if (profile != null)
            {
                service.InitializeFromProfile(profile);
            }

            return service;
        }

        private static void WirePopulationPresence(
            CCS_SettlementService settlementService,
            CCS_PopulationPresenceService populationPresenceService,
            CCS_WorldSimulationService worldSimulationService)
        {
            if (settlementService == null || populationPresenceService == null || worldSimulationService == null)
            {
                return;
            }

            populationPresenceService.BindPopulationSnapshotResolver(settlementId =>
            {
                if (worldSimulationService.TryGetPopulationSnapshot(settlementId, out CCS_SettlementPopulationSnapshot snapshot))
                {
                    return snapshot;
                }

                return CCS_SettlementPopulationSnapshot.Empty;
            });

            populationPresenceService.BindGrowthSnapshotResolver(settlementId =>
            {
                if (worldSimulationService.TryGetGrowthSnapshot(settlementId, out CCS_SettlementGrowthSnapshot snapshot))
                {
                    return snapshot;
                }

                return CCS_SettlementGrowthSnapshot.Empty;
            });

            populationPresenceService.BindSettlementDiscoveredResolver(settlementId =>
                settlementService.IsInitialized && settlementService.IsDiscovered(settlementId));

            settlementService.SettlementPopulationChanged +=
                populationPresenceService.HandleSettlementPopulationChanged;
            settlementService.SettlementDiscovered += populationPresenceService.HandleSettlementDiscovered;

            populationPresenceService.RefreshAllAnchors();
        }

        private static CCS_NpcIdentityService CreateNpcIdentityService(CCS_NpcIdentityProfile profile)
        {
            CCS_NpcIdentityService service = new CCS_NpcIdentityService();
            service.Initialize();
            if (profile != null)
            {
                service.InitializeFromProfile(profile);
            }

            return service;
        }

        private static void WireNpcIdentity(
            CCS_SettlementService settlementService,
            CCS_PopulationPresenceService populationPresenceService,
            CCS_NpcIdentityService npcIdentityService,
            CCS_WorldSimulationService worldSimulationService)
        {
            if (settlementService == null || populationPresenceService == null
                || npcIdentityService == null || worldSimulationService == null)
            {
                return;
            }

            npcIdentityService.BindSettlementNpcStateAccessors(
                settlementId =>
                {
                    if (worldSimulationService.TryGetSettlementState(settlementId, out CCS_SettlementSimulationState state)
                        && state != null)
                    {
                        return state.npcIdentityStates ?? new CCS_NpcIdentityState[0];
                    }

                    return new CCS_NpcIdentityState[0];
                },
                (settlementId, states) =>
                {
                    if (!worldSimulationService.TryGetSettlementState(settlementId, out CCS_SettlementSimulationState state)
                        || state == null)
                    {
                        return;
                    }

                    state.npcIdentityStates = states ?? new CCS_NpcIdentityState[0];
                });

            settlementService.SettlementPopulationChanged += npcIdentityService.HandleSettlementPopulationChanged;
            settlementService.SettlementDiscovered += npcIdentityService.HandleSettlementDiscovered;

            populationPresenceService.RefreshAllAnchors();
            npcIdentityService.RefreshAllPlaceholderIdentities();
        }

        private static CCS_NpcServiceRepresentativeService CreateNpcServiceRepresentativeService(
            CCS_NpcServiceRepresentativeProfile profile)
        {
            CCS_NpcServiceRepresentativeService service = new CCS_NpcServiceRepresentativeService();
            service.Initialize();
            if (profile != null)
            {
                service.InitializeFromProfile(profile);
            }

            return service;
        }

        private static void WireNpcServiceRepresentatives(
            CCS_BusinessService businessService,
            CCS_PopulationPresenceService populationPresenceService,
            CCS_NpcIdentityService npcIdentityService,
            CCS_NpcServiceRepresentativeService representativeService,
            CCS_WorldSimulationService worldSimulationService)
        {
            if (businessService == null || populationPresenceService == null
                || npcIdentityService == null || representativeService == null || worldSimulationService == null)
            {
                return;
            }

            representativeService.BindIdentityService(npcIdentityService);
            representativeService.BindRepresentativeStateAccessors(
                settlementId =>
                {
                    if (worldSimulationService.TryGetSettlementState(settlementId, out CCS_SettlementSimulationState state)
                        && state != null)
                    {
                        return state.npcServiceRepresentativeStates ?? new CCS_NpcServiceRepresentativeState[0];
                    }

                    return new CCS_NpcServiceRepresentativeState[0];
                },
                (settlementId, states) =>
                {
                    if (!worldSimulationService.TryGetSettlementState(settlementId, out CCS_SettlementSimulationState state)
                        || state == null)
                    {
                        return;
                    }

                    state.npcServiceRepresentativeStates = states ?? new CCS_NpcServiceRepresentativeState[0];
                });

            businessService.BusinessActivated += representativeService.HandleBusinessActivated;
            businessService.BusinessDeactivated += representativeService.HandleBusinessDeactivated;

            populationPresenceService.RefreshAllAnchors();
            representativeService.RefreshAllRepresentatives();
        }

        private static CCS_SettlementHousingService CreateSettlementHousingService(
            CCS_SettlementHousingProfile profile)
        {
            CCS_SettlementHousingService service = new CCS_SettlementHousingService();
            service.Initialize();
            if (profile != null)
            {
                service.InitializeFromProfile(profile);
            }

            return service;
        }

        private static void WireSettlementHousing(
            CCS_SettlementService settlementService,
            CCS_SettlementHousingService housingService,
            CCS_WorldSimulationService worldSimulationService)
        {
            if (settlementService == null || housingService == null || worldSimulationService == null)
            {
                return;
            }

            housingService.BindHousingStateAccessors(
                settlementId =>
                {
                    if (worldSimulationService.TryGetSettlementState(
                            settlementId,
                            out CCS_SettlementSimulationState state)
                        && state != null)
                    {
                        return state.housingStates ?? new CCS_SettlementHousingState[0];
                    }

                    return new CCS_SettlementHousingState[0];
                },
                (settlementId, states) =>
                {
                    worldSimulationService.SetHousingStates(settlementId, states);
                });

            housingService.BindGrowthSnapshotResolver(settlementId =>
            {
                if (worldSimulationService.TryGetGrowthSnapshot(settlementId, out CCS_SettlementGrowthSnapshot snapshot))
                {
                    return snapshot;
                }

                return CCS_SettlementGrowthSnapshot.Empty;
            });

            housingService.BindSettlementDiscoveredResolver(settlementId =>
                settlementService.IsInitialized && settlementService.IsDiscovered(settlementId));

            housingService.BindProsperityResolver(settlementId =>
            {
                if (worldSimulationService.TryGetSettlementState(
                        settlementId,
                        out CCS_SettlementSimulationState state)
                    && state != null)
                {
                    return state.prosperity;
                }

                return 0f;
            });

            housingService.BindPopulationProfileResolver(() => worldSimulationService.SettlementPopulationProfile);
            housingService.BindPopulationMetricsRefreshCallback(
                worldSimulationService.RefreshSettlementPopulationMetrics);

            settlementService.SettlementGrowthChanged += housingService.HandleSettlementGrowthChanged;
            settlementService.SettlementDiscovered += housingService.HandleSettlementDiscovered;

            housingService.RefreshAllAnchors();
        }

        private static CCS_NpcMovementService CreateNpcMovementService(CCS_NpcMovementProfile profile)
        {
            CCS_NpcMovementService service = new CCS_NpcMovementService();
            service.Initialize();
            if (profile != null)
            {
                service.InitializeFromProfile(profile);
            }

            return service;
        }

        private static CCS_NpcScheduleService CreateNpcScheduleService(CCS_NpcScheduleProfile profile)
        {
            CCS_NpcScheduleService service = new CCS_NpcScheduleService();
            service.Initialize();
            if (profile != null)
            {
                service.InitializeFromProfile(profile);
            }

            return service;
        }

        private static void WireNpcSchedule(
            CCS_SettlementService settlementService,
            CCS_NpcScheduleService scheduleService,
            CCS_WorldSimulationService worldSimulationService,
            CCS_TimeOfDayService timeOfDayService)
        {
            if (settlementService == null || scheduleService == null || worldSimulationService == null)
            {
                return;
            }

            scheduleService.BindScheduleStateAccessors(
                settlementId =>
                {
                    if (worldSimulationService.TryGetSettlementState(
                            settlementId,
                            out CCS_SettlementSimulationState state)
                        && state != null)
                    {
                        return state.npcScheduleStates ?? new CCS_NpcScheduleState[0];
                    }

                    return new CCS_NpcScheduleState[0];
                },
                (settlementId, states) =>
                {
                    worldSimulationService.SetScheduleStates(settlementId, states);
                });

            scheduleService.BindIdentityStateAccessors(settlementId =>
            {
                if (worldSimulationService.TryGetSettlementState(
                        settlementId,
                        out CCS_SettlementSimulationState state)
                    && state != null)
                {
                    return state.npcIdentityStates ?? new CCS_NpcIdentityState[0];
                }

                return new CCS_NpcIdentityState[0];
            });

            scheduleService.BindScheduleHourResolver(() =>
            {
                if (timeOfDayService == null || !timeOfDayService.IsInitialized)
                {
                    return 12;
                }

                return timeOfDayService.CreateSnapshot().Hour;
            });

            settlementService.SettlementDiscovered += scheduleService.HandleSettlementDiscovered;
            settlementService.SettlementPopulationChanged += scheduleService.HandleSettlementPopulationChanged;

            scheduleService.RefreshAllSchedules();
        }

        private static void WireNpcMovement(
            CCS_SettlementService settlementService,
            CCS_NpcIdentityService npcIdentityService,
            CCS_NpcMovementService movementService,
            CCS_NpcScheduleService scheduleService,
            CCS_WorldSimulationService worldSimulationService,
            CCS_TimeOfDayService timeOfDayService,
            CCS_SettlementHousingProfile housingProfile)
        {
            if (settlementService == null || npcIdentityService == null
                || movementService == null || worldSimulationService == null)
            {
                return;
            }

            movementService.BindMovementStateAccessors(
                settlementId =>
                {
                    if (worldSimulationService.TryGetSettlementState(
                            settlementId,
                            out CCS_SettlementSimulationState state)
                        && state != null)
                    {
                        return state.npcMovementStates ?? new CCS_NpcMovementState[0];
                    }

                    return new CCS_NpcMovementState[0];
                },
                (settlementId, states) =>
                {
                    worldSimulationService.SetMovementStates(settlementId, states);
                });

            movementService.BindIdentityStateAccessors(
                settlementId =>
                {
                    if (worldSimulationService.TryGetSettlementState(
                            settlementId,
                            out CCS_SettlementSimulationState state)
                        && state != null)
                    {
                        return state.npcIdentityStates ?? new CCS_NpcIdentityState[0];
                    }

                    return new CCS_NpcIdentityState[0];
                },
                (settlementId, states) =>
                {
                    if (!worldSimulationService.TryGetSettlementState(
                            settlementId,
                            out CCS_SettlementSimulationState state)
                        || state == null)
                    {
                        return;
                    }

                    state.npcIdentityStates = states ?? new CCS_NpcIdentityState[0];
                });

            movementService.BindScheduleHourResolver(() =>
            {
                if (timeOfDayService == null || !timeOfDayService.IsInitialized)
                {
                    return 12;
                }

                return timeOfDayService.CreateSnapshot().Hour;
            });

            movementService.BindHousingProfileResolver(() => housingProfile);
            movementService.BindScheduleService(
                scheduleService != null && scheduleService.IsInitialized ? scheduleService : null);

            settlementService.SettlementDiscovered += movementService.HandleSettlementDiscovered;
            settlementService.SettlementPopulationChanged += movementService.HandleSettlementPopulationChanged;

            movementService.ResyncAllFromSchedule();
        }

        private static void RegisterNpcMovementUpdatable(
            CCS_RuntimeHost runtimeHost,
            CCS_NpcMovementService movementService)
        {
            if (runtimeHost == null || movementService == null || !movementService.IsInitialized)
            {
                return;
            }

            runtimeHost.RuntimeUpdateLoop.RegisterUpdatable(movementService);
        }

        private static void WireContractBoardActivation(CCS_ContractService contractService)
        {
            CCS_SettlementContractBoardActivationBridge.ActivateHandler = servicePoint =>
            {
                if (contractService == null || !contractService.IsInitialized || servicePoint == null)
                {
                    return CCS_SettlementServiceActivationResult.Blocked(
                        CCS_SettlementServiceRouteType.ContractBoard,
                        CCS_SettlementServiceActivationStatus.ServiceMissing,
                        "Contract service is not ready.");
                }

                string settlementId = servicePoint.ResolveSettlementId();
                if (servicePoint.ServicePointType == CCS_SettlementServicePointType.ContractBoard)
                {
                    CCS_ContractDebugHud.ShowSettlementBoard(
                        servicePoint.GetInteractionDisplayName(),
                        settlementId);
                }
                else
                {
                    CCS_ContractType boardType = ResolveContractBoardType(servicePoint);
                    CCS_ContractDebugHud.ShowBoard(
                        servicePoint.GetInteractionDisplayName(),
                        settlementId,
                        boardType);
                }
                return CCS_SettlementServiceActivationResult.Success(
                    CCS_SettlementServiceRouteType.ContractBoard,
                    "Contract board debug panel opened.");
            };
        }

        private static CCS_ContractType ResolveContractBoardType(CCS_SettlementServicePoint servicePoint)
        {
            return servicePoint.ServicePointType switch
            {
                CCS_SettlementServicePointType.GeneralStore => CCS_ContractType.GeneralStoreSupply,
                CCS_SettlementServicePointType.Stable => CCS_ContractType.StableSupply,
                CCS_SettlementServicePointType.Gunsmith => CCS_ContractType.GunsmithSupply,
                CCS_SettlementServicePointType.LandOffice => CCS_ContractType.LandOfficeSupply,
                _ => CCS_ContractType.TradingPostSupply
            };
        }

        private static void WireReputationEventHooks(
            CCS_ReputationService reputationService,
            CCS_VendorService vendorService,
            CCS_BankingService bankingService,
            CCS_UpkeepService upkeepService,
            CCS_SettlementService settlementService)
        {
            if (reputationService == null || !reputationService.IsInitialized)
            {
                return;
            }

            string activeSettlementId = reputationService.ActiveProfile != null
                ? reputationService.ActiveProfile.DefaultTradingPostSettlementId
                : CCS_ReputationContentIds.DefaultTradingPostSettlementId;

            if (settlementService != null && settlementService.IsInitialized)
            {
                settlementService.SettlementDiscovered += snapshot =>
                {
                    if (snapshot == null || string.IsNullOrWhiteSpace(snapshot.SettlementId))
                    {
                        return;
                    }

                    reputationService.TryApplySettlementDiscovered(snapshot.SettlementId);
                };

                settlementService.ServicePointActivated += activationArgs =>
                {
                    if (!string.IsNullOrWhiteSpace(activationArgs?.SettlementId))
                    {
                        activeSettlementId = activationArgs.SettlementId;
                    }
                };
            }

            if (vendorService != null && vendorService.IsInitialized)
            {
                vendorService.VendorTransactionCompleted += result =>
                {
                    if (result == null || !result.IsSuccess || !result.WasSell)
                    {
                        return;
                    }

                    reputationService.TryApplyGoodsSold(activeSettlementId);
                };
            }

            if (bankingService != null && bankingService.IsInitialized)
            {
                bankingService.LoanTransactionCompleted += result =>
                {
                    if (result == null
                        || !result.IsSuccess
                        || !result.Message.Contains("Repaid loan", System.StringComparison.OrdinalIgnoreCase))
                    {
                        return;
                    }

                    reputationService.TryApplyLoanRepaid(activeSettlementId);
                };
            }

            if (upkeepService != null && upkeepService.IsInitialized)
            {
                upkeepService.UpkeepTransactionCompleted += result =>
                {
                    if (result == null)
                    {
                        return;
                    }

                    if (result.IsSuccess && result.Amount > 0)
                    {
                        reputationService.TryApplyUpkeepPaid(activeSettlementId);
                        return;
                    }

                    if (result.ResultType == CCS_UpkeepTransactionResultType.InsufficientFunds)
                    {
                        reputationService.TryApplyFailedUpkeep(activeSettlementId);
                    }
                };
            }
        }

        private static CCS_RegionService CreateRegionService(CCS_RegionProfile profile)
        {
            CCS_RegionService service = new CCS_RegionService();
            service.Initialize();
            if (profile != null)
            {
                service.InitializeFromProfile(profile);
            }

            return service;
        }

        private static CCS_WorldSimulationService CreateWorldSimulationService(CCS_WorldSimulationProfile profile)
        {
            CCS_WorldSimulationService service = new CCS_WorldSimulationService();
            service.Initialize();
            if (profile != null)
            {
                service.InitializeFromProfile(profile);
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
