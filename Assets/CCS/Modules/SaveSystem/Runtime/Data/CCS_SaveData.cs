using System;
using CCS.Modules.Industry;
using CCS.Modules.Mounts;
using CCS.Modules.Farming;
using CCS.Modules.Land;
using CCS.Modules.Banking;
using CCS.Modules.Upkeep;
using CCS.Modules.Reputation;
using CCS.Modules.Contracts;
using CCS.Modules.Ranching;
using CCS.Modules.Vehicles;
using CCS.Modules.Firearms;
using CCS.Modules.WorldSimulation;

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

        public CCS_SaveSettlementsWorldData settlements = new CCS_SaveSettlementsWorldData();

        public CCS_SaveRegionsWorldData regions = new CCS_SaveRegionsWorldData();

        public CCS_SaveWorldSimulationData worldSimulation = new CCS_SaveWorldSimulationData();

        public CCS_SaveRanchingWorldData ranching = new CCS_SaveRanchingWorldData();

        public CCS_SaveFarmingWorldData farming = new CCS_SaveFarmingWorldData();

        public CCS_SaveLandWorldData land = new CCS_SaveLandWorldData();

        public CCS_SaveBankingWorldData banking = new CCS_SaveBankingWorldData();

        public CCS_SaveUpkeepWorldData upkeep = new CCS_SaveUpkeepWorldData();

        public CCS_SaveReputationWorldData reputation = new CCS_SaveReputationWorldData();

        public CCS_SaveContractsWorldData contracts = new CCS_SaveContractsWorldData();

        #endregion
    }

    [Serializable]
    public sealed class CCS_SaveContractsWorldData
    {
        public CCS_ContractSnapshot[] contractInstances = Array.Empty<CCS_ContractSnapshot>();
    }

    [Serializable]
    public sealed class CCS_SaveReputationWorldData
    {
        public CCS_ReputationSnapshot[] standings = Array.Empty<CCS_ReputationSnapshot>();
    }

    [Serializable]
    public sealed class CCS_SaveUpkeepWorldData
    {
        public CCS_UpkeepEntry[] entries = Array.Empty<CCS_UpkeepEntry>();
    }

    [Serializable]
    public sealed class CCS_SaveBankingWorldData
    {
        public CCS_BankAccountSnapshot[] accounts = Array.Empty<CCS_BankAccountSnapshot>();
        public CCS_LoanSnapshot[] loans = Array.Empty<CCS_LoanSnapshot>();
    }

    [Serializable]
    public sealed class CCS_SaveLandWorldData
    {
        public CCS_LandClaimSnapshot[] claims = Array.Empty<CCS_LandClaimSnapshot>();
    }

    [Serializable]
    public sealed class CCS_SaveFarmingWorldData
    {
        public CCS_FarmPlotSnapshot[] plots = Array.Empty<CCS_FarmPlotSnapshot>();
    }

    [Serializable]
    public sealed class CCS_SaveRanchingWorldData
    {
        public CCS_LivestockSnapshot[] livestock = Array.Empty<CCS_LivestockSnapshot>();
        public CCS_RanchStructureSnapshot[] structures = Array.Empty<CCS_RanchStructureSnapshot>();
    }

    [Serializable]
    public sealed class CCS_SaveWorldSimulationData
    {
        public CCS_SettlementSimulationState[] settlementStates = Array.Empty<CCS_SettlementSimulationState>();
        public CCS_RegionSimulationState[] regionStates = Array.Empty<CCS_RegionSimulationState>();
    }

    [Serializable]
    public sealed class CCS_SaveRegionsWorldData
    {
        public CCS_SaveRegionDiscoveryData[] discoveries = Array.Empty<CCS_SaveRegionDiscoveryData>();
    }

    [Serializable]
    public sealed class CCS_SaveRegionDiscoveryData
    {
        public string regionId = string.Empty;
        public string displayName = string.Empty;
        public int regionType;
        public bool discovered;
        public float positionX;
        public float positionY;
        public float positionZ;
        public int specializationType;
        public int dominantIndustry;
        public float foodSupplyStrength;
        public float industrialSupplyStrength;
        public float buildingSupplyStrength;
        public float tradeSupplyStrength;
    }

    [Serializable]
    public sealed class CCS_SaveSettlementsWorldData
    {
        public CCS_SaveSettlementDiscoveryData[] discoveries = Array.Empty<CCS_SaveSettlementDiscoveryData>();
    }

    [Serializable]
    public sealed class CCS_SaveSettlementDiscoveryData
    {
        public string settlementId = string.Empty;
        public string displayName = string.Empty;
        public int settlementType;
        public bool discovered;
        public float positionX;
        public float positionY;
        public float positionZ;
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
        public string landClaimId = string.Empty;
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
