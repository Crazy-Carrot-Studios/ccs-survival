using UnityEngine;

// =============================================================================
// SCRIPT: CCS_LandClaimPlacementRequest
// CATEGORY: Modules / Land / Runtime / Services
// PURPOSE: Active-item placement request payload for land claim deeds.
// PLACEMENT: Built by composition active item handler.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-02
// NOTES: Milestone 2.3.0 land claim placement flow.
// =============================================================================

namespace CCS.Modules.Land
{
    public sealed class CCS_LandClaimPlacementRequest
    {
        public CCS_LandClaimPlacementRequest(
            string claimDefinitionId,
            Vector3 useOrigin,
            Vector3 useDirection,
            bool confirmPlacement)
        {
            ClaimDefinitionId = claimDefinitionId ?? string.Empty;
            UseOrigin = useOrigin;
            UseDirection = useDirection.sqrMagnitude > 0.001f ? useDirection.normalized : Vector3.forward;
            ConfirmPlacement = confirmPlacement;
        }

        public string ClaimDefinitionId { get; }

        public Vector3 UseOrigin { get; }

        public Vector3 UseDirection { get; }

        public bool ConfirmPlacement { get; }
    }
}
