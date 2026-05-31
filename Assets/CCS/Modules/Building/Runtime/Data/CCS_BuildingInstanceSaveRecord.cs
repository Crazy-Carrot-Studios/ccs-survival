// =============================================================================
// SCRIPT: CCS_BuildingInstanceSaveRecord
// CATEGORY: Modules / Building / Runtime / Data
// PURPOSE: Serializable placed instance record for future building persistence.
// PLACEMENT: Stored in CCS_BuildingSaveData.placedInstanceRecords.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Serialization model only. Full restore deferred beyond 0.8.1.
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

        #endregion
    }
}
