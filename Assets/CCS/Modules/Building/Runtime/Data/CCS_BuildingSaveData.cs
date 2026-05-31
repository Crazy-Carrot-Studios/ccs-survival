using System.Collections.Generic;

// =============================================================================
// SCRIPT: CCS_BuildingSaveData
// CATEGORY: Modules / Building / Runtime / Data
// PURPOSE: Versioned save payload for global building catalog persistence.
// PLACEMENT: Serialized by CCS_BuildingService.CaptureState().
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Persists registered definition IDs only. No placed structures in 0.8.0.
// =============================================================================

namespace CCS.Modules.Building
{
    [System.Serializable]
    public sealed class CCS_BuildingSaveData
    {
        #region Variables

        public const int CurrentSaveDataVersion = 1;

        public int saveDataVersion = CurrentSaveDataVersion;

        public List<string> registeredPieceIds = new List<string>();

        #endregion
    }
}
