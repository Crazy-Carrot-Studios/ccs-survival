using System;
using System.IO;
using CCS.Core;
using CCS.Modules.Building;
using CCS.Modules.Cooking;
using CCS.Modules.Sleep;
using CCS.Modules.Storage;
using CCS.Modules.Gathering;
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
            Transform playerRoot)
        {
            inventoryService = inventory;
            survivalCoreService = survivalCore;
            gatheringService = gathering;
            buildingService = building;
            storageService = storage;
            sleepService = sleep;
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

        private void ApplySaveData(CCS_SaveData saveData)
        {
            ApplyInventory(saveData.inventory);
            ApplyNeeds(saveData.needs);
            ApplyBuilding(saveData.building);
            ApplyStorage(saveData.storage);
            ApplySleep(saveData.sleep);
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
