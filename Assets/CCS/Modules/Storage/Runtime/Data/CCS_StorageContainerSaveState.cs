using System;

// =============================================================================
// SCRIPT: CCS_StorageContainerSaveState
// CATEGORY: Modules / Storage / Runtime / Data
// PURPOSE: Serializable per-instance storage container world and inventory snapshot.
// PLACEMENT: Nested in CCS_SaveData and used by CCS_StorageService persistence.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: JsonUtility-compatible fields only.
// =============================================================================

namespace CCS.Modules.Storage
{
    [Serializable]
    public sealed class CCS_StorageContainerSaveState
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

        public CCS_StorageContainerSlotSaveState[] slots = Array.Empty<CCS_StorageContainerSlotSaveState>();
    }

    [Serializable]
    public sealed class CCS_StorageContainerSlotSaveState
    {
        public string itemId = string.Empty;
        public int quantity;
    }
}
