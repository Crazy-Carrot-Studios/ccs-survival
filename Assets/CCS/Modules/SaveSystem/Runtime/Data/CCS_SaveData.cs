using System;
using CCS.Modules.Industry;
using CCS.Modules.Mounts;
using CCS.Modules.Vehicles;
using CCS.Modules.Firearms;

// =============================================================================
// SCRIPT: CCS_SaveData
// CATEGORY: Modules / SaveSystem / Runtime / Data
// PURPOSE: Root serializable save payload for unified survival persistence.
// PLACEMENT: Serialized to JSON by CCS_SaveService.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: saveVersion reserved for future migration. JsonUtility-compatible fields only.
// =============================================================================

namespace CCS.Modules.SaveSystem
{
    [Serializable]
    public sealed class CCS_SaveData
    {
        #region Variables

        public const int CurrentSaveVersion = 1;

        public int saveVersion = CurrentSaveVersion;

        public string savedAtUtc = string.Empty;

        public CCS_SavePlayerData player = new CCS_SavePlayerData();

        public CCS_SaveNeedsData needs = new CCS_SaveNeedsData();

        public CCS_SaveInventoryData inventory = new CCS_SaveInventoryData();

        public CCS_SaveGatheringWorldData gathering = new CCS_SaveGatheringWorldData();

        public CCS_SaveCookingWorldData cooking = new CCS_SaveCookingWorldData();

        public CCS_SaveBuildingWorldData building = new CCS_SaveBuildingWorldData();

        public CCS_SaveStorageWorldData storage = new CCS_SaveStorageWorldData();

        public CCS_SaveSleepWorldData sleep = new CCS_SaveSleepWorldData();

        public CCS_SaveEconomyData economy = new CCS_SaveEconomyData();

        public CCS_SaveTrapWorldData trapping = new CCS_SaveTrapWorldData();

        public CCS_SaveCampWorldData camp = new CCS_SaveCampWorldData();

        public CCS_SaveIndustryWorldData industry = new CCS_SaveIndustryWorldData();

        public CCS_SaveMountsWorldData mounts = new CCS_SaveMountsWorldData();

        public CCS_SaveVehiclesWorldData vehicles = new CCS_SaveVehiclesWorldData();

        public CCS_SaveFirearmsWorldData firearms = new CCS_SaveFirearmsWorldData();

        #endregion
    }

    [Serializable]
    public sealed class CCS_SaveMountsWorldData
    {
        public CCS_MountSnapshot ownedMount = new CCS_MountSnapshot();
    }

    [Serializable]
    public sealed class CCS_SaveVehiclesWorldData
    {
        public CCS_VehicleSnapshot ownedVehicle = new CCS_VehicleSnapshot();
    }

    [Serializable]
    public sealed class CCS_SaveFirearmsWorldData
    {
        public CCS_FirearmSnapshot firearmState = new CCS_FirearmSnapshot();
    }

    [Serializable]
    public sealed class CCS_SaveIndustryWorldData
    {
        public CCS_IndustryJob[] activeJobs = Array.Empty<CCS_IndustryJob>();
    }

    [Serializable]
    public sealed class CCS_SaveCampWorldData
    {
        public CCS_SaveCampStateData campState = new CCS_SaveCampStateData();
        public CCS_SaveFrontierShelterInstanceData[] shelterInstances = Array.Empty<CCS_SaveFrontierShelterInstanceData>();
        public CCS_SaveFrontierWorkbenchInstanceData[] workbenchInstances = Array.Empty<CCS_SaveFrontierWorkbenchInstanceData>();
    }

    [Serializable]
    public sealed class CCS_SaveCampStateData
    {
        public int campTier;
        public bool ownsCamp;
        public string campOwnerId = string.Empty;
        public float campCenterX;
        public float campCenterY;
        public float campCenterZ;
        public bool hasShelter;
        public bool hasCampfire;
        public bool hasBedroll;
        public bool hasStorage;
        public bool hasWorkArea;
        public bool hasSawTable;
        public bool hasCharcoalKiln;
        public bool hasPrimitiveForge;
        public long campCreationTimeUtcTicks;
        public string[] structuresPresent = Array.Empty<string>();
    }

    [Serializable]
    public sealed class CCS_SaveFrontierWorkbenchInstanceData
    {
        public string instanceId = string.Empty;
        public string workbenchDefinitionId = string.Empty;
        public float positionX;
        public float positionY;
        public float positionZ;
        public float rotationY;
        public string campOwnerId = string.Empty;
    }

    [Serializable]
    public sealed class CCS_SaveFrontierShelterInstanceData
    {
        public string instanceId = string.Empty;
        public string shelterDefinitionId = string.Empty;
        public float positionX;
        public float positionY;
        public float positionZ;
        public float rotationY;
        public string campOwnerId = string.Empty;
    }

    [Serializable]
    public sealed class CCS_SaveTrapWorldData
    {
        public CCS_SaveTrapInstanceData[] instances = Array.Empty<CCS_SaveTrapInstanceData>();
    }

    [Serializable]
    public sealed class CCS_SaveTrapInstanceData
    {
        public string instanceId = string.Empty;
        public string trapDefinitionId = string.Empty;
        public int trapState;
        public float positionX;
        public float positionY;
        public float positionZ;
        public float rotationY;
        public string capturedWildlifeId = string.Empty;
        public string capturedInstanceKey = string.Empty;
        public float remainingTimerSeconds;
        public bool hasCaptureData;
    }

    [Serializable]
    public sealed class CCS_SaveEconomyData
    {
        public CCS_SaveCurrencyBalanceData[] balances = Array.Empty<CCS_SaveCurrencyBalanceData>();
    }

    [Serializable]
    public sealed class CCS_SaveCurrencyBalanceData
    {
        public string currencyId = string.Empty;
        public int amount;
    }

    [Serializable]
    public sealed class CCS_SavePlayerData
    {
        public float positionX;
        public float positionY;
        public float positionZ;
        public float rotationY;
    }

    [Serializable]
    public sealed class CCS_SaveNeedsData
    {
        public float hunger;
        public float thirst;
        public float stamina;
    }

    [Serializable]
    public sealed class CCS_SaveInventoryData
    {
        public CCS_SaveInventorySlotData[] slots = Array.Empty<CCS_SaveInventorySlotData>();
    }

    [Serializable]
    public sealed class CCS_SaveInventorySlotData
    {
        public string itemId = string.Empty;
        public int quantity;
    }

    [Serializable]
    public sealed class CCS_SaveGatheringWorldData
    {
        public CCS_SaveGatheringNodeData[] nodes = Array.Empty<CCS_SaveGatheringNodeData>();
    }

    [Serializable]
    public sealed class CCS_SaveGatheringNodeData
    {
        public string nodeId = string.Empty;
        public bool isAvailable = true;
        public float respawnTimer;
    }

    [Serializable]
    public sealed class CCS_SaveCookingWorldData
    {
        public CCS_SaveCampfireStationData[] stations = Array.Empty<CCS_SaveCampfireStationData>();
    }

    [Serializable]
    public sealed class CCS_SaveCampfireStationData
    {
        public string stationId = string.Empty;
        public bool isStationActive;
        public bool isCooking;
        public string currentRecipeId = string.Empty;
        public bool hasFuelLoaded;
    }

    [Serializable]
    public sealed class CCS_SaveBuildingWorldData
    {
        public string buildingStateJson = string.Empty;
    }

    [Serializable]
    public sealed class CCS_SaveStorageWorldData
    {
        public CCS_SaveStorageContainerData[] containers = System.Array.Empty<CCS_SaveStorageContainerData>();
    }

    [Serializable]
    public sealed class CCS_SaveStorageContainerData
    {
        public string containerDefinitionId = string.Empty;

        public string instanceId = string.Empty;

        public string displayName = string.Empty;

        public float positionX;
        public float positionY;
        public float positionZ;

        public float rotationX;
        public float rotationY;
        public float rotationZ;
        public float rotationW = 1f;

        public CCS_SaveStorageContainerSlotData[] slots = System.Array.Empty<CCS_SaveStorageContainerSlotData>();
    }

    [Serializable]
    public sealed class CCS_SaveStorageContainerSlotData
    {
        public string itemId = string.Empty;
        public int quantity;
    }

    [Serializable]
    public sealed class CCS_SaveSleepWorldData
    {
        public string assignedRespawnSpotId = string.Empty;

        public CCS_SaveSleepSpotData[] sleepSpots = Array.Empty<CCS_SaveSleepSpotData>();
    }

    [Serializable]
    public sealed class CCS_SaveSleepSpotData
    {
        public string sleepSpotDefinitionId = string.Empty;

        public string instanceId = string.Empty;

        public string displayName = string.Empty;

        public float positionX;
        public float positionY;
        public float positionZ;

        public float rotationX;
        public float rotationY;
        public float rotationZ;
        public float rotationW = 1f;

        public string assignedRespawnSpotId = string.Empty;

        public bool isAssignedRespawn;
    }
}
