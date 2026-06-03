using System;

// =============================================================================
// SCRIPT: CCS_CropSnapshot
// CATEGORY: Modules / Farming / Runtime / Data
// PURPOSE: Serializable crop state for save/load and farm plot persistence.
// PLACEMENT: Nested in CCS_FarmPlotSnapshot and used by CCS_FarmService.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-02
// NOTES: Milestone 2.2.0 farming foundation.
// =============================================================================

namespace CCS.Modules.Farming
{
    [Serializable]
    public sealed class CCS_CropSnapshot
    {
        public string cropDefinitionId = string.Empty;
        public int growthStage;
        public float growthElapsedSeconds;
        public bool hasCrop;
    }
}
