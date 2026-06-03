using System;

// =============================================================================
// SCRIPT: CCS_FarmPlotSnapshot
// CATEGORY: Modules / Farming / Runtime / Data
// PURPOSE: Serializable farm plot instance state for save/load.
// PLACEMENT: Used by CCS_FarmService and CCS_SaveFarmingWorldData.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-02
// NOTES: One planted crop per plot. Milestone 2.2.0.
// =============================================================================

namespace CCS.Modules.Farming
{
    [Serializable]
    public sealed class CCS_FarmPlotSnapshot
    {
        public string instanceId = string.Empty;
        public string plotDefinitionId = string.Empty;
        public float positionX;
        public float positionY;
        public float positionZ;
        public float rotationY;
        public string campOwnerId = string.Empty;
        public CCS_CropSnapshot crop = new CCS_CropSnapshot();
    }
}
