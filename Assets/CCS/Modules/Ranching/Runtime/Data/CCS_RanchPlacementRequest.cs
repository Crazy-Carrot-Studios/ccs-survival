using UnityEngine;

// =============================================================================
// SCRIPT: CCS_RanchPlacementRequest
// CATEGORY: Modules / Ranching / Runtime / Data
// PURPOSE: Active-item ranch structure placement request payload.
// PLACEMENT: Used by CCS_RanchService and active item routing.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// NOTES: Milestone 2.1.0 ranching foundation.
// =============================================================================

namespace CCS.Modules.Ranching
{
    public sealed class CCS_RanchPlacementRequest
    {
        public CCS_RanchPlacementRequest(
            string structureDefinitionId,
            Vector3 useOrigin,
            Vector3 useDirection,
            bool confirmPlacement)
        {
            StructureDefinitionId = structureDefinitionId ?? string.Empty;
            UseOrigin = useOrigin;
            UseDirection = useDirection.sqrMagnitude > 0.001f ? useDirection.normalized : Vector3.forward;
            ConfirmPlacement = confirmPlacement;
        }

        public string StructureDefinitionId { get; }

        public Vector3 UseOrigin { get; }

        public Vector3 UseDirection { get; }

        public bool ConfirmPlacement { get; }
    }
}
