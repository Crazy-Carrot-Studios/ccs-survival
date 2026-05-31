using System.Collections.Generic;

// =============================================================================
// SCRIPT: CCS_BuildingSaveData
// CATEGORY: Modules / Building / Runtime / Data
// PURPOSE: Versioned save payload for global building catalog and placed instances.
// PLACEMENT: Serialized by CCS_BuildingService.CaptureState().
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Version 3 adds snap occupancy and restore support in 0.8.4.
// =============================================================================

namespace CCS.Modules.Building
{
    [System.Serializable]
    public sealed class CCS_BuildingSaveData
    {
        #region Variables

        public const int CurrentSaveDataVersion = 3;

        public int saveDataVersion = CurrentSaveDataVersion;

        public List<string> registeredPieceIds = new List<string>();

        public List<CCS_BuildingInstanceSaveRecord> placedInstanceRecords = new List<CCS_BuildingInstanceSaveRecord>();

        #endregion
    }
}
