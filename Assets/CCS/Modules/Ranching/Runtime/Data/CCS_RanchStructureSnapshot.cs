using System;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_RanchStructureSnapshot
// CATEGORY: Modules / Ranching / Runtime / Data
// PURPOSE: Serializable ranch structure state for save/load.
// PLACEMENT: Used by CCS_RanchService and save payloads.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// NOTES: Milestone 2.1.0 ranching foundation.
// =============================================================================

namespace CCS.Modules.Ranching
{
    [Serializable]
    public sealed class CCS_RanchStructureSnapshot
    {
        public string instanceId = string.Empty;
        public string structureDefinitionId = string.Empty;
        public int structureKind;
        public float positionX;
        public float positionY;
        public float positionZ;
        public float rotationY;
        public string campOwnerId = string.Empty;
    }
}
