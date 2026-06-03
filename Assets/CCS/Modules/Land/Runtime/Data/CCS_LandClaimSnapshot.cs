using System;

// =============================================================================
// SCRIPT: CCS_LandClaimSnapshot
// CATEGORY: Modules / Land / Runtime / Data
// PURPOSE: Serializable land claim instance state for save/load.
// PLACEMENT: Used by CCS_LandClaimService and CCS_SaveLandWorldData.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-02
// NOTES: Milestone 2.3.0 land ownership foundation.
// =============================================================================

namespace CCS.Modules.Land
{
    [Serializable]
    public sealed class CCS_LandClaimSnapshot
    {
        public string instanceId = string.Empty;
        public string claimDefinitionId = string.Empty;
        public string ownerId = string.Empty;
        public string regionId = string.Empty;
        public float positionX;
        public float positionY;
        public float positionZ;
        public float claimRadius;
        public int claimState;
        public string[] associatedStructureIds = Array.Empty<string>();
    }
}
