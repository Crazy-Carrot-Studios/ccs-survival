using System.Collections.Generic;

// =============================================================================
// SCRIPT: CCS_BuildingInstanceSaveRecord
// CATEGORY: Modules / Building / Runtime / Data
// PURPOSE: Serializable placed instance record for building persistence restore.
// PLACEMENT: Stored in CCS_BuildingSaveData.placedInstanceRecords.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Snap occupancy and placement order persisted in 0.8.4.
// =============================================================================

namespace CCS.Modules.Building
{
    [System.Serializable]
    public sealed class CCS_BuildingInstanceSaveRecord
    {
        #region Variables

        public string instanceId = string.Empty;

        public string pieceId = string.Empty;

        public float positionX;

        public float positionY;

        public float positionZ;

        public float rotationX;

        public float rotationY;

        public float rotationZ;

        public float rotationW = 1f;

        public float creationTime;

        public int placedOrderIndex;

        public List<string> occupiedSnapPointIds = new List<string>();

        public string targetSnapInstanceId = string.Empty;

        public string targetSnapPointId = string.Empty;

        #endregion
    }
}
