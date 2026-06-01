using System;

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

        #endregion
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
}
