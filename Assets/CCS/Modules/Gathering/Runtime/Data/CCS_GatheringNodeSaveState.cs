using System;

// =============================================================================
// SCRIPT: CCS_GatheringNodeSaveState
// CATEGORY: Modules / Gathering / Runtime / Data
// PURPOSE: Serializable gathering node state for unified save persistence.
// PLACEMENT: Captured by CCS_GatheringService and mapped by CCS_SaveService.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: JsonUtility-compatible fields only.
// =============================================================================

namespace CCS.Modules.Gathering
{
    [Serializable]
    public sealed class CCS_GatheringNodeSaveState
    {
        public string nodeId = string.Empty;
        public bool isAvailable = true;
        public float respawnTimer;
    }
}
