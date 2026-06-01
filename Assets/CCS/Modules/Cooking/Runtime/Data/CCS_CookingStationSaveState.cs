using System;

// =============================================================================
// SCRIPT: CCS_CookingStationSaveState
// CATEGORY: Modules / Cooking / Runtime / Data
// PURPOSE: Serializable campfire station state for unified save persistence.
// PLACEMENT: Captured by CCS_CookingStation and mapped by CCS_SaveService.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: JsonUtility-compatible fields only.
// =============================================================================

namespace CCS.Modules.Cooking
{
    [Serializable]
    public sealed class CCS_CookingStationSaveState
    {
        public string stationId = string.Empty;
        public bool isStationActive;
        public bool isCooking;
        public string currentRecipeId = string.Empty;
        public bool hasFuelLoaded;
    }
}
