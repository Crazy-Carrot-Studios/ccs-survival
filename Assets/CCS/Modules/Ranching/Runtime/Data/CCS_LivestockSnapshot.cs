using System;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_LivestockSnapshot
// CATEGORY: Modules / Ranching / Runtime / Data
// PURPOSE: Serializable livestock state for save/load and queries.
// PLACEMENT: Used by CCS_RanchService and save payloads.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// NOTES: Milestone 2.1.0 ranching foundation.
// =============================================================================

namespace CCS.Modules.Ranching
{
    [Serializable]
    public sealed class CCS_LivestockSnapshot
    {
        public string instanceId = string.Empty;
        public string livestockDefinitionId = string.Empty;
        public int livestockType;
        public int livestockState;
        public string assignedStructureInstanceId = string.Empty;
        public float positionX;
        public float positionY;
        public float positionZ;
        public float productionElapsedSeconds;
        public string lastProducedItemId = string.Empty;
        public int lastProducedQuantity;
        public string campOwnerId = string.Empty;
    }
}
