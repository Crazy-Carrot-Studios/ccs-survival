using System;

// =============================================================================
// SCRIPT: CCS_SleepSpotSaveState
// CATEGORY: Modules / Sleep / Runtime / Data
// PURPOSE: Serializable per-instance sleep spot world snapshot for unified save.
// PLACEMENT: Nested in CCS_SaveData and used by CCS_SleepService persistence.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: JsonUtility-compatible fields only.
// =============================================================================

namespace CCS.Modules.Sleep
{
    [Serializable]
    public sealed class CCS_SleepSpotSaveState
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
