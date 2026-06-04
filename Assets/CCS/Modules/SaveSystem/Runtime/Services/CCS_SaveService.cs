using System;
using System.IO;
using CCS.Core;
using CCS.Modules.Building;
using CCS.Modules.Cooking;
using CCS.Modules.Sleep;
using CCS.Modules.Storage;
using CCS.Modules.Trapping;
using CCS.Modules.Industry;
using CCS.Modules.Mounts;
using CCS.Modules.Ranching;
using CCS.Modules.Farming;
using CCS.Modules.Land;
using CCS.Modules.Banking;
using CCS.Modules.Upkeep;
using CCS.Modules.Reputation;
using CCS.Modules.Contracts;
using CCS.Modules.Vehicles;
using CCS.Modules.Firearms;
using CCS.Modules.Shelter;
using CCS.Modules.Gathering;
using CCS.Modules.Economy;
using CCS.Modules.Settlements;
using CCS.Modules.Regions;
using CCS.Modules.WorldSimulation;
using CCS.Modules.Inventory;
using CCS.Modules.SurvivalCore;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SaveService
// CATEGORY: Modules / SaveSystem / Runtime / Services
// PURPOSE: Unified JSON save/load for player, needs, inventory, and world state.
// PLACEMENT: Registered as CCS_ISurvivalService by survival gameplay composition wiring.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: File path Application.persistentDataPath/CCS_Survival_Save.json by default.
// =============================================================================

namespace CCS.Modules.SaveSystem
{
    public sealed class CCS_SaveService : CCS_ISurvivalService, CCS_IUpdatable
    {
        private const string LogPrefix = "[CCS_SaveService]";

        #region Variables

        private CCS_SaveProfile activeProfile;
        private CCS_PlayerInventoryService inventoryService;
        private CCS_SurvivalCoreService survivalCoreService;
        private CCS_GatheringService gatheringService;
        private CCS_BuildingService buildingService;
        private CCS_StorageService storageService;
        private CCS_SleepService sleepService;
        private CCS_TrapService trapService;
        private CCS_FrontierShelterService frontierShelterService;
        private CCS_FrontierHomesteadStructureService homesteadStructureService;
        private CCS_FrontierStoragePlacementService frontierStoragePlacementService;
        private CCS_CampService campService;
        private CCS_IndustryService industryService;
        private CCS_MountService mountService;
        private CCS_VehicleService vehicleService;
        private CCS_FirearmService firearmService;
        private CCS_SettlementService settlementService;
        private CCS_RegionService regionService;
        private CCS_WorldSimulationService worldSimulationService;
        private CCS_RanchService ranchService;
        private CCS_FarmService farmService;
        private CCS_LandClaimService landClaimService;
        private CCS_BankingService bankingService;
        private CCS_UpkeepService upkeepService;
        private CCS_ReputationService reputationService;
        private CCS_ContractService contractService;
        private CCS_CurrencyService currencyService;
        private Transform playerTransform;
        private float autoSaveTimer;
        private bool isInitialized;
        private bool hasLoadedThisSession;

        #endregion

        #region Events

        public event SaveStartedHandler SaveStarted;
        public event SaveCompletedHandler SaveCompleted;
        public event LoadStartedHandler LoadStarted;
        public event LoadCompletedHandler LoadCompleted;

        #endregion

        #region Properties

        public bool IsInitialized => isInitialized;

        public CCS_SaveProfile ActiveProfile => activeProfile;

        public bool HasLoadedThisSession => hasLoadedThisSession;

        public string SaveFilePath => CCS_SaveValidationUtility.ResolveSaveFilePath(activeProfile);

        #endregion

        #region Public Methods

        public void Initialize()
        {
            if (isInitialized)
            {
                return;
            }

            isInitialized = true;
        }

        public void InitializeFromProfile(CCS_SaveProfile profile)
        {
            if (profile == null)
            {
                isInitialized = false;
                return;
            }

            CCS_SurvivalValidationResult validation = CCS_SaveValidationUtility.ValidateProfile(profile);
            if (!validation.IsSuccess)
            {
                Debug.LogWarning($"{LogPrefix} Profile validation warning: {validation.Message}");
            }

            activeProfile = profile;
            isInitialized = true;
        }

        public void BindGameplayServices(
            CCS_PlayerInventoryService inventory,
            CCS_SurvivalCoreService survivalCore,
            CCS_GatheringService gathering,
            CCS_BuildingService building,
            CCS_StorageService storage,
            CCS_SleepService sleep,
            CCS_CurrencyService currency,
            CCS_TrapService trapping,
            CCS_FrontierShelterService frontierShelter,
            CCS_CampService camp,
            CCS_FrontierHomesteadStructureService homesteadStructure,
            CCS_FrontierStoragePlacementService frontierStoragePlacement,
            CCS_IndustryService industry,
            CCS_MountService mounts,
            CCS_VehicleService vehicles,
            CCS_FirearmService firearms,
            CCS_SettlementService settlements,
            CCS_RegionService regions,
            CCS_WorldSimulationService worldSimulation,
            CCS_RanchService ranching,
            CCS_FarmService farming,
            CCS_LandClaimService landClaim,
            CCS_BankingService banking,
            CCS_UpkeepService upkeep,
            CCS_ReputationService reputation,
            CCS_ContractService contracts,
            Transform playerRoot)
        {
            inventoryService = inventory;
            survivalCoreService = survivalCore;
            gatheringService = gathering;
            buildingService = building;
            storageService = storage;
            sleepService = sleep;
            currencyService = currency;
            trapService = trapping;
            frontierShelterService = frontierShelter;
            campService = camp;
            homesteadStructureService = homesteadStructure;
            frontierStoragePlacementService = frontierStoragePlacement;
            industryService = industry;
            mountService = mounts;
            vehicleService = vehicles;
            firearmService = firearms;
            settlementService = settlements;
            regionService = regions;
            worldSimulationService = worldSimulation;
            ranchService = ranching;
            farmService = farming;
            landClaimService = landClaim;
            bankingService = banking;
            upkeepService = upkeep;
            reputationService = reputation;
            contractService = contracts;
            playerTransform = playerRoot;
        }

        public bool HasSave()
        {
            return File.Exists(SaveFilePath);
        }

        public bool SaveGame()
        {
            string savePath = SaveFilePath;
            string timestamp = DateTime.UtcNow.ToString("o");
            CCS_SaveEventArgs startedArgs = new CCS_SaveEventArgs(savePath, timestamp, true, "Save started.");
            SaveStarted?.Invoke(startedArgs);

            if (!EnsureReadyForPersistence())
            {
                RaiseSaveCompleted(savePath, timestamp, false, "Save failed because services are not ready.");
                return false;
            }

            try
            {
                CCS_SaveData saveData = CaptureCurrentState();
                string directory = Path.GetDirectoryName(savePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string json = JsonUtility.ToJson(saveData, true);
                File.WriteAllText(savePath, json);
                LogDebug($"SaveGame wrote {savePath}.");
                RaiseSaveCompleted(savePath, timestamp, true, "Save completed.");
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError($"{LogPrefix} SaveGame failed: {exception.Message}");
                RaiseSaveCompleted(savePath, timestamp, false, exception.Message);
                return false;
            }
        }

        public bool LoadGame()
        {
            string savePath = SaveFilePath;
            string timestamp = DateTime.UtcNow.ToString("o");
            LoadStarted?.Invoke(new CCS_SaveEventArgs(savePath, timestamp, true, "Load started."));

            if (!File.Exists(savePath))
            {
                RaiseLoadCompleted(savePath, timestamp, false, "No save file found.");
                return false;
            }

            if (!EnsureReadyForPersistence())
            {
                RaiseLoadCompleted(savePath, timestamp, false, "Load failed because services are not ready.");
                return false;
            }

            try
            {
                string json = File.ReadAllText(savePath);
                CCS_SaveData saveData = JsonUtility.FromJson<CCS_SaveData>(json);
                if (saveData == null || saveData.saveVersion <= 0)
                {
                    RaiseLoadCompleted(savePath, timestamp, false, "Save payload is invalid.");
                    return false;
                }

                ApplySaveData(saveData);
                hasLoadedThisSession = true;
                LogDebug($"LoadGame restored {savePath}.");
                RaiseLoadCompleted(savePath, timestamp, true, "Load completed.");
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError($"{LogPrefix} LoadGame failed: {exception.Message}");
                RaiseLoadCompleted(savePath, timestamp, false, exception.Message);
                return false;
            }
        }

        public bool TryLoadOnStartup()
        {
            if (hasLoadedThisSession || !HasSave())
            {
                return false;
            }

            return LoadGame();
        }

        public bool DeleteSave()
        {
            string savePath = SaveFilePath;
            if (!File.Exists(savePath))
            {
                return false;
            }

            File.Delete(savePath);
            LogDebug($"DeleteSave removed {savePath}.");
            return true;
        }

        public void Tick(float deltaTime)
        {
            if (!isInitialized || activeProfile == null || !activeProfile.AutoSaveEnabled || deltaTime <= 0f)
            {
                return;
            }

            autoSaveTimer += deltaTime;
            if (autoSaveTimer < activeProfile.AutoSaveIntervalSeconds)
            {
                return;
            }

            autoSaveTimer = 0f;
            SaveGame();
        }

        #endregion

        #region Private Methods

        private bool EnsureReadyForPersistence()
        {
            return isInitialized
                && inventoryService != null
                && inventoryService.IsInitialized
                && survivalCoreService != null
                && survivalCoreService.IsInitialized;
        }

        private CCS_SaveData CaptureCurrentState()
        {
            CCS_SaveData saveData = new CCS_SaveData
            {
                saveVersion = CCS_SaveData.CurrentSaveVersion,
                savedAtUtc = DateTime.UtcNow.ToString("o")
            };

            CapturePlayerTransform(saveData.player);
            CaptureNeeds(saveData.needs);
            CaptureInventory(saveData.inventory);
            CaptureGathering(saveData.gathering);
            CaptureCooking(saveData.cooking);
            CaptureBuilding(saveData.building);
            CaptureStorage(saveData.storage);
            CaptureSleep(saveData.sleep);
            CaptureTrapping(saveData.trapping);
            CaptureCamp(saveData.camp);
            CaptureIndustry(saveData.industry);
            CaptureMounts(saveData.mounts);
            CaptureVehicles(saveData.vehicles);
            CaptureFirearms(saveData.firearms);
            CaptureSettlements(saveData.settlements);
            CaptureRegions(saveData.regions);
            CaptureWorldSimulation(saveData.worldSimulation);
            CaptureRanching(saveData.ranching);
            CaptureFarming(saveData.farming);
            CaptureLand(saveData.land);
            CaptureBanking(saveData.banking);
            CaptureUpkeep(saveData.upkeep);
            CaptureReputation(saveData.reputation);
            CaptureContracts(saveData.contracts);
            CaptureEconomy(saveData.economy);
            return saveData;
        }

        private void CapturePlayerTransform(CCS_SavePlayerData playerData)
        {
            if (playerData == null || playerTransform == null)
            {
                return;
            }

            Vector3 position = playerTransform.position;
            playerData.positionX = position.x;
            playerData.positionY = position.y;
            playerData.positionZ = position.z;
            playerData.rotationY = playerTransform.eulerAngles.y;
        }

        private void CaptureNeeds(CCS_SaveNeedsData needsData)
        {
            if (needsData == null || survivalCoreService == null)
            {
                return;
            }

            if (survivalCoreService.TryGetSnapshot(CCS_SurvivalStatType.Hunger, out CCS_SurvivalStatSnapshot hunger))
            {
                needsData.hunger = hunger.CurrentValue;
            }

            if (survivalCoreService.TryGetSnapshot(CCS_SurvivalStatType.Thirst, out CCS_SurvivalStatSnapshot thirst))
            {
                needsData.thirst = thirst.CurrentValue;
            }

            if (survivalCoreService.TryGetSnapshot(CCS_SurvivalStatType.Stamina, out CCS_SurvivalStatSnapshot stamina))
            {
                needsData.stamina = stamina.CurrentValue;
            }
        }

        private void CaptureInventory(CCS_SaveInventoryData inventoryData)
        {
            if (inventoryData == null || inventoryService == null)
            {
                return;
            }

            string inventoryJson = inventoryService.CaptureState();
            CCS_InventorySaveData inventorySave = JsonUtility.FromJson<CCS_InventorySaveData>(inventoryJson);
            if (inventorySave?.slots == null)
            {
                inventoryData.slots = Array.Empty<CCS_SaveInventorySlotData>();
                return;
            }

            CCS_SaveInventorySlotData[] slots = new CCS_SaveInventorySlotData[inventorySave.slots.Length];
            for (int index = 0; index < inventorySave.slots.Length; index++)
            {
                CCS_InventorySaveSlotEntry source = inventorySave.slots[index];
                slots[index] = new CCS_SaveInventorySlotData
                {
                    itemId = source != null ? source.itemId ?? string.Empty : string.Empty,
                    quantity = source != null ? source.quantity : 0
                };
            }

            inventoryData.slots = slots;
        }

        private void CaptureGathering(CCS_SaveGatheringWorldData gatheringData)
        {
            if (gatheringData == null || gatheringService == null)
            {
                return;
            }

            CCS_GatheringNodeSaveState[] nodeStates = gatheringService.CaptureNodeStates();
            if (nodeStates == null || nodeStates.Length == 0)
            {
                gatheringData.nodes = Array.Empty<CCS_SaveGatheringNodeData>();
                return;
            }

            CCS_SaveGatheringNodeData[] records = new CCS_SaveGatheringNodeData[nodeStates.Length];
            for (int index = 0; index < nodeStates.Length; index++)
            {
                CCS_GatheringNodeSaveState source = nodeStates[index];
                records[index] = new CCS_SaveGatheringNodeData
                {
                    nodeId = source.nodeId,
                    isAvailable = source.isAvailable,
                    respawnTimer = source.respawnTimer
                };
            }

            gatheringData.nodes = records;
        }

        private void CaptureCooking(CCS_SaveCookingWorldData cookingData)
        {
            if (cookingData == null)
            {
                return;
            }

            CCS_CookingStation[] stations =
                CCS_SurvivalSceneQueryUtility.FindActiveObjectsByType<CCS_CookingStation>();
            if (stations == null || stations.Length == 0)
            {
                cookingData.stations = Array.Empty<CCS_SaveCampfireStationData>();
                return;
            }

            System.Collections.Generic.List<CCS_SaveCampfireStationData> records =
                new System.Collections.Generic.List<CCS_SaveCampfireStationData>(stations.Length);
            for (int index = 0; index < stations.Length; index++)
            {
                CCS_CookingStation station = stations[index];
                if (station == null || string.IsNullOrWhiteSpace(station.SaveStationId))
                {
                    continue;
                }

                CCS_CookingStationSaveState source = station.CaptureSaveState();
                records.Add(new CCS_SaveCampfireStationData
                {
                    stationId = source.stationId,
                    isStationActive = source.isStationActive,
                    isCooking = source.isCooking,
                    currentRecipeId = source.currentRecipeId,
                    hasFuelLoaded = source.hasFuelLoaded
                });
            }

            cookingData.stations = records.ToArray();
        }

        private void CaptureBuilding(CCS_SaveBuildingWorldData buildingData)
        {
            if (buildingData == null || buildingService == null || !buildingService.IsInitialized)
            {
                return;
            }

            buildingData.buildingStateJson = buildingService.CaptureState();
        }

        private void CaptureStorage(CCS_SaveStorageWorldData storageData)
        {
            if (storageData == null || storageService == null || !storageService.IsInitialized)
            {
                return;
            }

            CCS_StorageContainerSaveState[] containerStates = storageService.CaptureWorldState();
            if (containerStates == null || containerStates.Length == 0)
            {
                storageData.containers = Array.Empty<CCS_SaveStorageContainerData>();
                return;
            }

            CCS_SaveStorageContainerData[] records = new CCS_SaveStorageContainerData[containerStates.Length];
            for (int index = 0; index < containerStates.Length; index++)
            {
                CCS_StorageContainerSaveState source = containerStates[index];
                records[index] = ConvertStorageSaveState(source);
            }

            storageData.containers = records;
        }

        private static CCS_SaveStorageContainerData ConvertStorageSaveState(CCS_StorageContainerSaveState source)
        {
            if (source == null)
            {
                return new CCS_SaveStorageContainerData();
            }

            CCS_SaveStorageContainerData record = new CCS_SaveStorageContainerData
            {
                containerDefinitionId = source.containerDefinitionId,
                instanceId = source.instanceId,
                displayName = source.displayName,
                positionX = source.positionX,
                positionY = source.positionY,
                positionZ = source.positionZ,
                rotationX = source.rotationX,
                rotationY = source.rotationY,
                rotationZ = source.rotationZ,
                rotationW = source.rotationW
            };

            if (source.slots == null || source.slots.Length == 0)
            {
                record.slots = Array.Empty<CCS_SaveStorageContainerSlotData>();
                return record;
            }

            CCS_SaveStorageContainerSlotData[] slotRecords =
                new CCS_SaveStorageContainerSlotData[source.slots.Length];
            for (int slotIndex = 0; slotIndex < source.slots.Length; slotIndex++)
            {
                CCS_StorageContainerSlotSaveState slotSource = source.slots[slotIndex];
                slotRecords[slotIndex] = new CCS_SaveStorageContainerSlotData
                {
                    itemId = slotSource != null ? slotSource.itemId ?? string.Empty : string.Empty,
                    quantity = slotSource != null ? slotSource.quantity : 0
                };
            }

            record.slots = slotRecords;
            return record;
        }

        private static CCS_StorageContainerSaveState ConvertStorageSaveRecord(CCS_SaveStorageContainerData source)
        {
            if (source == null)
            {
                return new CCS_StorageContainerSaveState();
            }

            CCS_StorageContainerSaveState saveState = new CCS_StorageContainerSaveState
            {
                containerDefinitionId = source.containerDefinitionId,
                instanceId = source.instanceId,
                displayName = source.displayName,
                positionX = source.positionX,
                positionY = source.positionY,
                positionZ = source.positionZ,
                rotationX = source.rotationX,
                rotationY = source.rotationY,
                rotationZ = source.rotationZ,
                rotationW = source.rotationW
            };

            if (source.slots == null || source.slots.Length == 0)
            {
                saveState.slots = Array.Empty<CCS_StorageContainerSlotSaveState>();
                return saveState;
            }

            CCS_StorageContainerSlotSaveState[] slotStates =
                new CCS_StorageContainerSlotSaveState[source.slots.Length];
            for (int slotIndex = 0; slotIndex < source.slots.Length; slotIndex++)
            {
                CCS_SaveStorageContainerSlotData slotSource = source.slots[slotIndex];
                slotStates[slotIndex] = new CCS_StorageContainerSlotSaveState
                {
                    itemId = slotSource != null ? slotSource.itemId ?? string.Empty : string.Empty,
                    quantity = slotSource != null ? slotSource.quantity : 0
                };
            }

            saveState.slots = slotStates;
            return saveState;
        }

        private void CaptureEconomy(CCS_SaveEconomyData economyData)
        {
            if (economyData == null || currencyService == null || !currencyService.IsInitialized)
            {
                return;
            }

            CCS_CurrencyBalance[] balances = currencyService.CaptureBalances();
            CCS_SaveCurrencyBalanceData[] saveBalances = new CCS_SaveCurrencyBalanceData[balances.Length];
            for (int index = 0; index < balances.Length; index++)
            {
                CCS_CurrencyBalance balance = balances[index];
                saveBalances[index] = new CCS_SaveCurrencyBalanceData
                {
                    currencyId = balance?.currencyId ?? string.Empty,
                    amount = balance != null ? balance.amount : 0
                };
            }

            economyData.balances = saveBalances;
        }

        private void ApplyEconomy(CCS_SaveEconomyData economyData)
        {
            if (currencyService == null || !currencyService.IsInitialized)
            {
                return;
            }

            if (economyData?.balances == null || economyData.balances.Length == 0)
            {
                currencyService.ImportBalancesFromInventoryBacking();
                return;
            }

            CCS_CurrencyBalance[] balances = new CCS_CurrencyBalance[economyData.balances.Length];
            for (int index = 0; index < economyData.balances.Length; index++)
            {
                CCS_SaveCurrencyBalanceData source = economyData.balances[index];
                balances[index] = new CCS_CurrencyBalance(
                    source != null ? source.currencyId : string.Empty,
                    source != null ? source.amount : 0);
            }

            currencyService.RestoreBalances(balances, syncInventoryBacking: true);
        }

        private void ApplySaveData(CCS_SaveData saveData)
        {
            ApplyInventory(saveData.inventory);
            ApplyEconomy(saveData.economy);
            ApplyNeeds(saveData.needs);
            ApplyBuilding(saveData.building);
            ApplyStorage(saveData.storage);
            ApplySleep(saveData.sleep);
            ApplyTrapping(saveData.trapping);
            ApplyCamp(saveData.camp);
            ApplyIndustry(saveData.industry);
            ApplyMounts(saveData.mounts);
            ApplyVehicles(saveData.vehicles);
            ApplyFirearms(saveData.firearms);
            ApplySettlements(saveData.settlements);
            ApplyRegions(saveData.regions);
            ApplyWorldSimulation(saveData.worldSimulation);
            ApplyRanching(saveData.ranching);
            ApplyFarming(saveData.farming);
            ApplyLand(saveData.land);
            ApplyBanking(saveData.banking);
            ApplyUpkeep(saveData.upkeep);
            ApplyReputation(saveData.reputation);
            ApplyContracts(saveData.contracts);
            ApplyGathering(saveData.gathering);
            ApplyCooking(saveData.cooking);
            ApplyPlayerTransform(saveData.player);
        }

        private void ApplyInventory(CCS_SaveInventoryData inventoryData)
        {
            if (inventoryData?.slots == null || inventoryService == null)
            {
                return;
            }

            CCS_InventorySaveSlotEntry[] slotEntries = new CCS_InventorySaveSlotEntry[inventoryData.slots.Length];
            for (int index = 0; index < inventoryData.slots.Length; index++)
            {
                CCS_SaveInventorySlotData source = inventoryData.slots[index];
                slotEntries[index] = new CCS_InventorySaveSlotEntry
                {
                    itemId = source != null ? source.itemId ?? string.Empty : string.Empty,
                    quantity = source != null ? source.quantity : 0
                };
            }

            CCS_InventorySaveData inventorySave = new CCS_InventorySaveData
            {
                saveDataVersion = CCS_InventorySaveData.CurrentSaveDataVersion,
                slotCount = slotEntries.Length,
                slots = slotEntries
            };

            inventoryService.RestoreState(JsonUtility.ToJson(inventorySave));
        }

        private void ApplyNeeds(CCS_SaveNeedsData needsData)
        {
            if (needsData == null || survivalCoreService == null)
            {
                return;
            }

            survivalCoreService.TryRestoreSavedNeeds(needsData.hunger, needsData.thirst, needsData.stamina);
        }

        private void ApplyBuilding(CCS_SaveBuildingWorldData buildingData)
        {
            if (buildingData == null
                || buildingService == null
                || string.IsNullOrWhiteSpace(buildingData.buildingStateJson))
            {
                return;
            }

            buildingService.RestoreState(buildingData.buildingStateJson);
        }

        private void ApplyStorage(CCS_SaveStorageWorldData storageData)
        {
            if (storageService == null || !storageService.IsInitialized)
            {
                return;
            }

            storageService.CloseContainer();

            if (storageData?.containers == null || storageData.containers.Length == 0)
            {
                storageService.RestoreWorldState(Array.Empty<CCS_StorageContainerSaveState>());
                return;
            }

            CCS_StorageContainerSaveState[] saveStates =
                new CCS_StorageContainerSaveState[storageData.containers.Length];
            for (int index = 0; index < storageData.containers.Length; index++)
            {
                saveStates[index] = ConvertStorageSaveRecord(storageData.containers[index]);
            }

            storageService.RestoreWorldState(saveStates);
        }

        private void CaptureSleep(CCS_SaveSleepWorldData sleepData)
        {
            if (sleepData == null || sleepService == null || !sleepService.IsInitialized)
            {
                return;
            }

            sleepData.assignedRespawnSpotId = sleepService.AssignedRespawnSpawnId ?? string.Empty;
            CCS_SleepSpotSaveState[] spotStates = sleepService.CaptureWorldState();
            if (spotStates == null || spotStates.Length == 0)
            {
                sleepData.sleepSpots = Array.Empty<CCS_SaveSleepSpotData>();
                return;
            }

            CCS_SaveSleepSpotData[] records = new CCS_SaveSleepSpotData[spotStates.Length];
            for (int index = 0; index < spotStates.Length; index++)
            {
                records[index] = ConvertSleepSaveState(spotStates[index]);
            }

            sleepData.sleepSpots = records;
        }

        private static CCS_SaveSleepSpotData ConvertSleepSaveState(CCS_SleepSpotSaveState source)
        {
            if (source == null)
            {
                return new CCS_SaveSleepSpotData();
            }

            return new CCS_SaveSleepSpotData
            {
                sleepSpotDefinitionId = source.sleepSpotDefinitionId ?? string.Empty,
                instanceId = source.instanceId ?? string.Empty,
                displayName = source.displayName ?? string.Empty,
                positionX = source.positionX,
                positionY = source.positionY,
                positionZ = source.positionZ,
                rotationX = source.rotationX,
                rotationY = source.rotationY,
                rotationZ = source.rotationZ,
                rotationW = source.rotationW,
                assignedRespawnSpotId = source.assignedRespawnSpotId ?? string.Empty,
                isAssignedRespawn = source.isAssignedRespawn
            };
        }

        private static CCS_SleepSpotSaveState ConvertSleepSaveRecord(CCS_SaveSleepSpotData source)
        {
            if (source == null)
            {
                return new CCS_SleepSpotSaveState();
            }

            return new CCS_SleepSpotSaveState
            {
                sleepSpotDefinitionId = source.sleepSpotDefinitionId ?? string.Empty,
                instanceId = source.instanceId ?? string.Empty,
                displayName = source.displayName ?? string.Empty,
                positionX = source.positionX,
                positionY = source.positionY,
                positionZ = source.positionZ,
                rotationX = source.rotationX,
                rotationY = source.rotationY,
                rotationZ = source.rotationZ,
                rotationW = source.rotationW,
                assignedRespawnSpotId = source.assignedRespawnSpotId ?? string.Empty,
                isAssignedRespawn = source.isAssignedRespawn
            };
        }

        private void CaptureTrapping(CCS_SaveTrapWorldData trappingData)
        {
            if (trappingData == null || trapService == null || !trapService.IsInitialized)
            {
                return;
            }

            CCS_TrapInstanceSaveState[] trapStates = trapService.CaptureWorldState();
            if (trapStates == null || trapStates.Length == 0)
            {
                trappingData.instances = Array.Empty<CCS_SaveTrapInstanceData>();
                return;
            }

            CCS_SaveTrapInstanceData[] records = new CCS_SaveTrapInstanceData[trapStates.Length];
            for (int index = 0; index < trapStates.Length; index++)
            {
                records[index] = ConvertTrapSaveRecord(trapStates[index]);
            }

            trappingData.instances = records;
        }

        private static CCS_SaveTrapInstanceData ConvertTrapSaveRecord(CCS_TrapInstanceSaveState source)
        {
            if (source == null)
            {
                return new CCS_SaveTrapInstanceData();
            }

            return new CCS_SaveTrapInstanceData
            {
                instanceId = source.instanceId ?? string.Empty,
                trapDefinitionId = source.trapDefinitionId ?? string.Empty,
                trapState = source.trapState,
                positionX = source.positionX,
                positionY = source.positionY,
                positionZ = source.positionZ,
                rotationY = source.rotationY,
                capturedWildlifeId = source.capturedWildlifeId ?? string.Empty,
                capturedInstanceKey = source.capturedInstanceKey ?? string.Empty,
                remainingTimerSeconds = source.remainingTimerSeconds,
                hasCaptureData = source.hasCaptureData
            };
        }

        private static CCS_TrapInstanceSaveState ConvertTrapSaveRecordToState(CCS_SaveTrapInstanceData source)
        {
            if (source == null)
            {
                return new CCS_TrapInstanceSaveState();
            }

            return new CCS_TrapInstanceSaveState
            {
                instanceId = source.instanceId ?? string.Empty,
                trapDefinitionId = source.trapDefinitionId ?? string.Empty,
                trapState = source.trapState,
                positionX = source.positionX,
                positionY = source.positionY,
                positionZ = source.positionZ,
                rotationY = source.rotationY,
                capturedWildlifeId = source.capturedWildlifeId ?? string.Empty,
                capturedInstanceKey = source.capturedInstanceKey ?? string.Empty,
                remainingTimerSeconds = source.remainingTimerSeconds,
                hasCaptureData = source.hasCaptureData
            };
        }

        private void CaptureCamp(CCS_SaveCampWorldData campData)
        {
            if (campData == null)
            {
                return;
            }

            if (campService != null && campService.IsInitialized)
            {
                CCS_CampSaveState state = campService.CaptureState();
                campData.campState = new CCS_SaveCampStateData
                {
                    campTier = state.campTier,
                    ownsCamp = state.ownsCamp,
                    campOwnerId = state.campOwnerId ?? string.Empty,
                    campCenterX = state.campCenterX,
                    campCenterY = state.campCenterY,
                    campCenterZ = state.campCenterZ,
                    hasShelter = state.hasShelter,
                    hasCampfire = state.hasCampfire,
                    hasBedroll = state.hasBedroll,
                    hasStorage = state.hasStorage,
                    hasWorkArea = state.hasWorkArea,
                    hasSawTable = state.hasSawTable,
                    hasCharcoalKiln = state.hasCharcoalKiln,
                    hasPrimitiveForge = state.hasPrimitiveForge,
                    campCreationTimeUtcTicks = state.campCreationTimeUtcTicks,
                    landClaimId = state.landClaimId ?? string.Empty,
                    structuresPresent = state.structuresPresent ?? Array.Empty<string>()
                };
            }

            if (frontierShelterService == null || !frontierShelterService.IsInitialized)
            {
                campData.shelterInstances = Array.Empty<CCS_SaveFrontierShelterInstanceData>();
            }
            else
            {
            CCS_FrontierShelterInstanceSaveState[] records = frontierShelterService.CaptureWorldState();
            if (records == null || records.Length == 0)
            {
                campData.shelterInstances = Array.Empty<CCS_SaveFrontierShelterInstanceData>();
            }
            else
            {

            CCS_SaveFrontierShelterInstanceData[] saveRecords =
                new CCS_SaveFrontierShelterInstanceData[records.Length];
            for (int index = 0; index < records.Length; index++)
            {
                CCS_FrontierShelterInstanceSaveState source = records[index];
                saveRecords[index] = new CCS_SaveFrontierShelterInstanceData
                {
                    instanceId = source?.InstanceId ?? string.Empty,
                    shelterDefinitionId = source?.ShelterDefinitionId ?? string.Empty,
                    positionX = source != null ? source.Position.x : 0f,
                    positionY = source != null ? source.Position.y : 0f,
                    positionZ = source != null ? source.Position.z : 0f,
                    rotationY = source != null ? source.RotationY : 0f,
                    campOwnerId = source?.CampOwnerId ?? string.Empty
                };
            }

            campData.shelterInstances = saveRecords;
            }
            }

            if (homesteadStructureService == null || !homesteadStructureService.IsInitialized)
            {
                campData.workbenchInstances = Array.Empty<CCS_SaveFrontierWorkbenchInstanceData>();
                return;
            }

            CCS_FrontierWorkbenchInstanceSaveState[] workbenchRecords =
                homesteadStructureService.CaptureWorkbenchWorldState();
            if (workbenchRecords == null || workbenchRecords.Length == 0)
            {
                campData.workbenchInstances = Array.Empty<CCS_SaveFrontierWorkbenchInstanceData>();
                return;
            }

            CCS_SaveFrontierWorkbenchInstanceData[] workbenchSaveRecords =
                new CCS_SaveFrontierWorkbenchInstanceData[workbenchRecords.Length];
            for (int index = 0; index < workbenchRecords.Length; index++)
            {
                CCS_FrontierWorkbenchInstanceSaveState source = workbenchRecords[index];
                workbenchSaveRecords[index] = new CCS_SaveFrontierWorkbenchInstanceData
                {
                    instanceId = source?.InstanceId ?? string.Empty,
                    workbenchDefinitionId = source?.WorkbenchDefinitionId ?? string.Empty,
                    positionX = source != null ? source.Position.x : 0f,
                    positionY = source != null ? source.Position.y : 0f,
                    positionZ = source != null ? source.Position.z : 0f,
                    rotationY = source != null ? source.RotationY : 0f,
                    campOwnerId = source?.CampOwnerId ?? string.Empty
                };
            }

            campData.workbenchInstances = workbenchSaveRecords;
        }

        private void CaptureIndustry(CCS_SaveIndustryWorldData industryData)
        {
            if (industryData == null)
            {
                return;
            }

            industryData.activeJobs = industryService != null && industryService.IsInitialized
                ? industryService.CaptureActiveJobs()
                : Array.Empty<CCS_IndustryJob>();
        }

        private void ApplyIndustry(CCS_SaveIndustryWorldData industryData)
        {
            if (industryService == null || !industryService.IsInitialized)
            {
                return;
            }

            industryService.RestoreActiveJobs(industryData?.activeJobs);
        }

        private void CaptureRanching(CCS_SaveRanchingWorldData ranchingData)
        {
            if (ranchingData == null)
            {
                return;
            }

            ranchingData.livestock = ranchService != null && ranchService.IsInitialized
                ? ranchService.CaptureLivestockState()
                : Array.Empty<CCS_LivestockSnapshot>();
            ranchingData.structures = ranchService != null && ranchService.IsInitialized
                ? ranchService.CaptureStructureState()
                : Array.Empty<CCS_RanchStructureSnapshot>();
        }

        private void ApplyRanching(CCS_SaveRanchingWorldData ranchingData)
        {
            if (ranchService == null || !ranchService.IsInitialized)
            {
                return;
            }

            ranchService.RestoreState(ranchingData?.livestock, ranchingData?.structures);
        }

        private void CaptureFarming(CCS_SaveFarmingWorldData farmingData)
        {
            if (farmingData == null)
            {
                return;
            }

            farmingData.plots = farmService != null && farmService.IsInitialized
                ? farmService.CapturePlotState()
                : Array.Empty<CCS_FarmPlotSnapshot>();
        }

        private void ApplyFarming(CCS_SaveFarmingWorldData farmingData)
        {
            if (farmService == null || !farmService.IsInitialized)
            {
                return;
            }

            farmService.RestoreState(farmingData?.plots);
        }

        private void CaptureLand(CCS_SaveLandWorldData landData)
        {
            if (landData == null)
            {
                return;
            }

            landData.claims = landClaimService != null && landClaimService.IsInitialized
                ? landClaimService.CaptureClaimState()
                : Array.Empty<CCS_LandClaimSnapshot>();
        }

        private void ApplyLand(CCS_SaveLandWorldData landData)
        {
            if (landClaimService == null || !landClaimService.IsInitialized)
            {
                return;
            }

            landClaimService.RestoreState(landData?.claims);
        }

        private void CaptureBanking(CCS_SaveBankingWorldData bankingData)
        {
            if (bankingData == null)
            {
                return;
            }

            bankingData.accounts = bankingService != null && bankingService.IsInitialized
                ? bankingService.CaptureBankingState()
                : Array.Empty<CCS_BankAccountSnapshot>();
            bankingData.loans = bankingService != null && bankingService.IsInitialized
                ? bankingService.CaptureLoanState()
                : Array.Empty<CCS_LoanSnapshot>();
        }

        private void ApplyBanking(CCS_SaveBankingWorldData bankingData)
        {
            if (bankingService == null || !bankingService.IsInitialized)
            {
                return;
            }

            bankingService.RestoreState(bankingData?.accounts);
            bankingService.RestoreLoanState(bankingData?.loans);
        }

        private void CaptureUpkeep(CCS_SaveUpkeepWorldData upkeepData)
        {
            if (upkeepData == null)
            {
                return;
            }

            upkeepData.entries = upkeepService != null && upkeepService.IsInitialized
                ? upkeepService.CaptureUpkeepState()
                : Array.Empty<CCS_UpkeepEntry>();
        }

        private void ApplyUpkeep(CCS_SaveUpkeepWorldData upkeepData)
        {
            if (upkeepService == null || !upkeepService.IsInitialized)
            {
                return;
            }

            upkeepService.RestoreState(upkeepData?.entries);
            if (landClaimService != null && landClaimService.IsInitialized)
            {
                upkeepService.ReconcileLandClaimEntries(landClaimService);
            }
        }

        private void CaptureReputation(CCS_SaveReputationWorldData reputationData)
        {
            if (reputationData == null)
            {
                return;
            }

            reputationData.standings = reputationService != null && reputationService.IsInitialized
                ? reputationService.CaptureReputationState()
                : Array.Empty<CCS_ReputationSnapshot>();
        }

        private void ApplyReputation(CCS_SaveReputationWorldData reputationData)
        {
            if (reputationService == null || !reputationService.IsInitialized)
            {
                return;
            }

            reputationService.RestoreState(reputationData?.standings);
        }

        private void CaptureContracts(CCS_SaveContractsWorldData contractsData)
        {
            if (contractsData == null)
            {
                return;
            }

            contractsData.contractInstances = contractService != null && contractService.IsInitialized
                ? contractService.CaptureContractsState()
                : Array.Empty<CCS_ContractSnapshot>();
        }

        private void ApplyContracts(CCS_SaveContractsWorldData contractsData)
        {
            if (contractService == null || !contractService.IsInitialized)
            {
                return;
            }

            contractService.RestoreState(contractsData?.contractInstances);
        }

        private void CaptureMounts(CCS_SaveMountsWorldData mountsData)
        {
            if (mountsData == null)
            {
                return;
            }

            mountsData.ownedMount = mountService != null && mountService.IsInitialized
                ? mountService.CaptureSnapshot()
                : CCS_MountSnapshot.Empty;
        }

        private void ApplyMounts(CCS_SaveMountsWorldData mountsData)
        {
            if (mountService == null || !mountService.IsInitialized)
            {
                return;
            }

            mountService.RestoreSnapshot(mountsData?.ownedMount);
        }

        private void CaptureVehicles(CCS_SaveVehiclesWorldData vehiclesData)
        {
            if (vehiclesData == null)
            {
                return;
            }

            vehiclesData.ownedVehicle = vehicleService != null && vehicleService.IsInitialized
                ? vehicleService.CaptureSnapshot()
                : CCS_VehicleSnapshot.Empty;
        }

        private void ApplyVehicles(CCS_SaveVehiclesWorldData vehiclesData)
        {
            if (vehicleService == null || !vehicleService.IsInitialized)
            {
                return;
            }

            vehicleService.RestoreSnapshot(vehiclesData?.ownedVehicle);
        }

        private void CaptureFirearms(CCS_SaveFirearmsWorldData firearmsData)
        {
            if (firearmsData == null)
            {
                return;
            }

            firearmsData.firearmState = firearmService != null && firearmService.IsInitialized
                ? firearmService.CurrentSnapshot
                : CCS_FirearmSnapshot.Empty;
        }

        private void ApplyFirearms(CCS_SaveFirearmsWorldData firearmsData)
        {
            if (firearmService == null || !firearmService.IsInitialized)
            {
                return;
            }

            firearmService.RestoreSnapshot(firearmsData?.firearmState);
        }

        private void CaptureSettlements(CCS_SaveSettlementsWorldData settlementsData)
        {
            if (settlementsData == null)
            {
                return;
            }

            if (settlementService == null || !settlementService.IsInitialized)
            {
                settlementsData.discoveries = Array.Empty<CCS_SaveSettlementDiscoveryData>();
                return;
            }

            CCS_SettlementSaveState[] records = settlementService.CaptureState();
            if (records == null || records.Length == 0)
            {
                settlementsData.discoveries = Array.Empty<CCS_SaveSettlementDiscoveryData>();
                return;
            }

            CCS_SaveSettlementDiscoveryData[] saveRecords = new CCS_SaveSettlementDiscoveryData[records.Length];
            for (int index = 0; index < records.Length; index++)
            {
                CCS_SettlementSaveState source = records[index];
                saveRecords[index] = new CCS_SaveSettlementDiscoveryData
                {
                    settlementId = source?.settlementId ?? string.Empty,
                    displayName = source?.displayName ?? string.Empty,
                    settlementType = source != null ? source.settlementType : 0,
                    discovered = source != null && source.discovered,
                    positionX = source != null ? source.positionX : 0f,
                    positionY = source != null ? source.positionY : 0f,
                    positionZ = source != null ? source.positionZ : 0f
                };
            }

            settlementsData.discoveries = saveRecords;
        }

        private void ApplySettlements(CCS_SaveSettlementsWorldData settlementsData)
        {
            if (settlementService == null || !settlementService.IsInitialized)
            {
                return;
            }

            CCS_SaveSettlementDiscoveryData[] discoveries = settlementsData?.discoveries;
            if (discoveries == null || discoveries.Length == 0)
            {
                settlementService.RestoreState(Array.Empty<CCS_SettlementSaveState>());
                return;
            }

            CCS_SettlementSaveState[] records = new CCS_SettlementSaveState[discoveries.Length];
            for (int index = 0; index < discoveries.Length; index++)
            {
                CCS_SaveSettlementDiscoveryData source = discoveries[index];
                records[index] = new CCS_SettlementSaveState
                {
                    settlementId = source?.settlementId ?? string.Empty,
                    displayName = source?.displayName ?? string.Empty,
                    settlementType = source != null ? source.settlementType : 0,
                    discovered = source != null && source.discovered,
                    positionX = source != null ? source.positionX : 0f,
                    positionY = source != null ? source.positionY : 0f,
                    positionZ = source != null ? source.positionZ : 0f
                };
            }

            settlementService.RestoreState(records);
        }

        private void CaptureRegions(CCS_SaveRegionsWorldData regionsData)
        {
            if (regionsData == null)
            {
                return;
            }

            if (regionService == null || !regionService.IsInitialized)
            {
                regionsData.discoveries = Array.Empty<CCS_SaveRegionDiscoveryData>();
                return;
            }

            CCS_RegionSaveState[] records = regionService.CaptureState();
            if (records == null || records.Length == 0)
            {
                regionsData.discoveries = Array.Empty<CCS_SaveRegionDiscoveryData>();
                return;
            }

            CCS_SaveRegionDiscoveryData[] saveRecords = new CCS_SaveRegionDiscoveryData[records.Length];
            for (int index = 0; index < records.Length; index++)
            {
                CCS_RegionSaveState source = records[index];
                saveRecords[index] = new CCS_SaveRegionDiscoveryData
                {
                    regionId = source?.regionId ?? string.Empty,
                    displayName = source?.displayName ?? string.Empty,
                    regionType = source != null ? source.regionType : 0,
                    discovered = source != null && source.discovered,
                    positionX = source != null ? source.positionX : 0f,
                    positionY = source != null ? source.positionY : 0f,
                    positionZ = source != null ? source.positionZ : 0f
                };
            }

            regionsData.discoveries = saveRecords;
        }

        private void ApplyRegions(CCS_SaveRegionsWorldData regionsData)
        {
            if (regionService == null || !regionService.IsInitialized)
            {
                return;
            }

            CCS_SaveRegionDiscoveryData[] discoveries = regionsData?.discoveries;
            if (discoveries == null || discoveries.Length == 0)
            {
                regionService.RestoreState(Array.Empty<CCS_RegionSaveState>());
                return;
            }

            CCS_RegionSaveState[] records = new CCS_RegionSaveState[discoveries.Length];
            for (int index = 0; index < discoveries.Length; index++)
            {
                CCS_SaveRegionDiscoveryData source = discoveries[index];
                records[index] = new CCS_RegionSaveState
                {
                    regionId = source?.regionId ?? string.Empty,
                    displayName = source?.displayName ?? string.Empty,
                    regionType = source != null ? source.regionType : 0,
                    discovered = source != null && source.discovered,
                    positionX = source != null ? source.positionX : 0f,
                    positionY = source != null ? source.positionY : 0f,
                    positionZ = source != null ? source.positionZ : 0f
                };
            }

            regionService.RestoreState(records);
        }

        private void CaptureWorldSimulation(CCS_SaveWorldSimulationData worldSimulationData)
        {
            if (worldSimulationData == null)
            {
                return;
            }

            if (worldSimulationService == null || !worldSimulationService.IsInitialized)
            {
                worldSimulationData.settlementStates = Array.Empty<CCS_SettlementSimulationState>();
                worldSimulationData.regionStates = Array.Empty<CCS_RegionSimulationState>();
                return;
            }

            worldSimulationData.settlementStates = worldSimulationService.CaptureState()
                ?? Array.Empty<CCS_SettlementSimulationState>();
            worldSimulationData.regionStates = worldSimulationService.CaptureRegionState()
                ?? Array.Empty<CCS_RegionSimulationState>();
        }

        private void ApplyWorldSimulation(CCS_SaveWorldSimulationData worldSimulationData)
        {
            if (worldSimulationService == null || !worldSimulationService.IsInitialized)
            {
                return;
            }

            CCS_SettlementSimulationState[] settlementStates = worldSimulationData?.settlementStates;
            CCS_RegionSimulationState[] regionStates = worldSimulationData?.regionStates;
            if ((settlementStates == null || settlementStates.Length == 0)
                && (regionStates == null || regionStates.Length == 0))
            {
                worldSimulationService.RestoreState(
                    Array.Empty<CCS_SettlementSimulationState>(),
                    Array.Empty<CCS_RegionSimulationState>());
                return;
            }

            worldSimulationService.RestoreState(
                settlementStates ?? Array.Empty<CCS_SettlementSimulationState>(),
                regionStates ?? Array.Empty<CCS_RegionSimulationState>());
        }

        private void ApplyCamp(CCS_SaveCampWorldData campData)
        {
            if (campService != null && campService.IsInitialized && campData?.campState != null)
            {
                campService.RestoreState(new CCS_CampSaveState
                {
                    campTier = campData.campState.campTier,
                    ownsCamp = campData.campState.ownsCamp,
                    campOwnerId = campData.campState.campOwnerId,
                    campCenterX = campData.campState.campCenterX,
                    campCenterY = campData.campState.campCenterY,
                    campCenterZ = campData.campState.campCenterZ,
                    hasShelter = campData.campState.hasShelter,
                    hasCampfire = campData.campState.hasCampfire,
                    hasBedroll = campData.campState.hasBedroll,
                    hasStorage = campData.campState.hasStorage,
                    hasWorkArea = campData.campState.hasWorkArea,
                    hasSawTable = campData.campState.hasSawTable,
                    hasCharcoalKiln = campData.campState.hasCharcoalKiln,
                    hasPrimitiveForge = campData.campState.hasPrimitiveForge,
                    campCreationTimeUtcTicks = campData.campState.campCreationTimeUtcTicks,
                    landClaimId = campData.campState.landClaimId,
                    structuresPresent = campData.campState.structuresPresent ?? Array.Empty<string>()
                });
            }

            if (homesteadStructureService != null && homesteadStructureService.IsInitialized)
            {
                if (campData?.workbenchInstances == null || campData.workbenchInstances.Length == 0)
                {
                    homesteadStructureService.RestoreWorkbenchWorldState(
                        Array.Empty<CCS_FrontierWorkbenchInstanceSaveState>());
                }
                else
                {
                    CCS_FrontierWorkbenchInstanceSaveState[] workbenchSaveStates =
                        new CCS_FrontierWorkbenchInstanceSaveState[campData.workbenchInstances.Length];
                    for (int index = 0; index < campData.workbenchInstances.Length; index++)
                    {
                        CCS_SaveFrontierWorkbenchInstanceData source = campData.workbenchInstances[index];
                        workbenchSaveStates[index] = new CCS_FrontierWorkbenchInstanceSaveState
                        {
                            InstanceId = source?.instanceId ?? string.Empty,
                            WorkbenchDefinitionId = source?.workbenchDefinitionId ?? string.Empty,
                            Position = new Vector3(
                                source != null ? source.positionX : 0f,
                                source != null ? source.positionY : 0f,
                                source != null ? source.positionZ : 0f),
                            RotationY = source != null ? source.rotationY : 0f,
                            CampOwnerId = source?.campOwnerId ?? string.Empty
                        };
                    }

                    homesteadStructureService.RestoreWorkbenchWorldState(workbenchSaveStates);
                }

                frontierStoragePlacementService?.RebuildPlacedStorageTracking();
            }

            if (frontierShelterService == null || !frontierShelterService.IsInitialized)
            {
                campService?.RecalculateCamp();
                return;
            }

            if (campData?.shelterInstances == null || campData.shelterInstances.Length == 0)
            {
                frontierShelterService.RestoreWorldState(Array.Empty<CCS_FrontierShelterInstanceSaveState>());
                return;
            }

            CCS_FrontierShelterInstanceSaveState[] saveStates =
                new CCS_FrontierShelterInstanceSaveState[campData.shelterInstances.Length];
            for (int index = 0; index < campData.shelterInstances.Length; index++)
            {
                CCS_SaveFrontierShelterInstanceData source = campData.shelterInstances[index];
                saveStates[index] = new CCS_FrontierShelterInstanceSaveState
                {
                    InstanceId = source?.instanceId ?? string.Empty,
                    ShelterDefinitionId = source?.shelterDefinitionId ?? string.Empty,
                    Position = new Vector3(
                        source != null ? source.positionX : 0f,
                        source != null ? source.positionY : 0f,
                        source != null ? source.positionZ : 0f),
                    RotationY = source != null ? source.rotationY : 0f,
                    CampOwnerId = source?.campOwnerId ?? string.Empty
                };
            }

            frontierShelterService.RestoreWorldState(saveStates);
            campService?.RecalculateCamp();
        }

        private void ApplyTrapping(CCS_SaveTrapWorldData trappingData)
        {
            if (trapService == null || !trapService.IsInitialized)
            {
                return;
            }

            if (trappingData?.instances == null || trappingData.instances.Length == 0)
            {
                trapService.RestoreWorldState(Array.Empty<CCS_TrapInstanceSaveState>());
                return;
            }

            CCS_TrapInstanceSaveState[] saveStates =
                new CCS_TrapInstanceSaveState[trappingData.instances.Length];
            for (int index = 0; index < trappingData.instances.Length; index++)
            {
                saveStates[index] = ConvertTrapSaveRecordToState(trappingData.instances[index]);
            }

            trapService.RestoreWorldState(saveStates);
        }

        private void ApplySleep(CCS_SaveSleepWorldData sleepData)
        {
            if (sleepService == null || !sleepService.IsInitialized)
            {
                return;
            }

            if (sleepData?.sleepSpots == null || sleepData.sleepSpots.Length == 0)
            {
                sleepService.RestoreWorldState(Array.Empty<CCS_SleepSpotSaveState>());
                return;
            }

            CCS_SleepSpotSaveState[] saveStates = new CCS_SleepSpotSaveState[sleepData.sleepSpots.Length];
            for (int index = 0; index < sleepData.sleepSpots.Length; index++)
            {
                saveStates[index] = ConvertSleepSaveRecord(sleepData.sleepSpots[index]);
            }

            sleepService.RestoreWorldState(saveStates);

            if (!string.IsNullOrWhiteSpace(sleepData.assignedRespawnSpotId))
            {
                sleepService.ApplySavedAssignedRespawn(sleepData.assignedRespawnSpotId);
            }
        }

        private void ApplyGathering(CCS_SaveGatheringWorldData gatheringData)
        {
            if (gatheringData?.nodes == null || gatheringService == null)
            {
                return;
            }

            CCS_GatheringNodeSaveState[] nodeStates = new CCS_GatheringNodeSaveState[gatheringData.nodes.Length];
            for (int index = 0; index < gatheringData.nodes.Length; index++)
            {
                CCS_SaveGatheringNodeData source = gatheringData.nodes[index];
                nodeStates[index] = new CCS_GatheringNodeSaveState
                {
                    nodeId = source != null ? source.nodeId : string.Empty,
                    isAvailable = source == null || source.isAvailable,
                    respawnTimer = source != null ? source.respawnTimer : 0f
                };
            }

            gatheringService.ApplyNodeStates(nodeStates);
        }

        private void ApplyCooking(CCS_SaveCookingWorldData cookingData)
        {
            if (cookingData?.stations == null)
            {
                return;
            }

            for (int index = 0; index < cookingData.stations.Length; index++)
            {
                CCS_SaveCampfireStationData record = cookingData.stations[index];
                if (record == null || string.IsNullOrWhiteSpace(record.stationId))
                {
                    continue;
                }

                CCS_CookingStation[] stations =
                    CCS_SurvivalSceneQueryUtility.FindActiveObjectsByType<CCS_CookingStation>();
                for (int stationIndex = 0; stationIndex < stations.Length; stationIndex++)
                {
                    CCS_CookingStation station = stations[stationIndex];
                    if (station != null && station.MatchesSaveId(record.stationId))
                    {
                        station.ApplySaveState(new CCS_CookingStationSaveState
                        {
                            stationId = record.stationId,
                            isStationActive = record.isStationActive,
                            isCooking = record.isCooking,
                            currentRecipeId = record.currentRecipeId,
                            hasFuelLoaded = record.hasFuelLoaded
                        });
                        break;
                    }
                }
            }
        }

        private void ApplyPlayerTransform(CCS_SavePlayerData playerData)
        {
            if (playerData == null || playerTransform == null)
            {
                return;
            }

            Vector3 position = new Vector3(playerData.positionX, playerData.positionY, playerData.positionZ);
            playerTransform.SetPositionAndRotation(
                position,
                Quaternion.Euler(0f, playerData.rotationY, 0f));

            UnityEngine.CharacterController controller = playerTransform.GetComponent<UnityEngine.CharacterController>();
            if (controller != null)
            {
                controller.enabled = false;
                playerTransform.position = position;
                controller.enabled = true;
            }
        }

        private void RaiseSaveCompleted(string savePath, string timestamp, bool success, string message)
        {
            SaveCompleted?.Invoke(new CCS_SaveEventArgs(savePath, timestamp, success, message));
        }

        private void RaiseLoadCompleted(string savePath, string timestamp, bool success, string message)
        {
            LoadCompleted?.Invoke(new CCS_SaveEventArgs(savePath, timestamp, success, message));
        }

        private void LogDebug(string message)
        {
            if (activeProfile != null && activeProfile.EnableDebugLogging)
            {
                Debug.Log($"{LogPrefix} {message}");
            }
        }

        #endregion
    }
}
