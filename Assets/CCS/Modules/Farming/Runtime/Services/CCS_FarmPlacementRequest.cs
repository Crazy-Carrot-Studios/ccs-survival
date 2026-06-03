using UnityEngine;

// =============================================================================
// SCRIPT: CCS_FarmPlacementRequest
// CATEGORY: Modules / Farming / Runtime / Services
// PURPOSE: Active-item farm plot placement request payload.
// PLACEMENT: Used by CCS_FarmService and active item routing.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-02
// NOTES: Milestone 2.2.0.
// =============================================================================

namespace CCS.Modules.Farming
{
    public sealed class CCS_FarmPlacementRequest
    {
        public CCS_FarmPlacementRequest(
            string plotDefinitionId,
            Vector3 useOrigin,
            Vector3 useDirection,
            bool confirmPlacement)
        {
            PlotDefinitionId = plotDefinitionId ?? string.Empty;
            UseOrigin = useOrigin;
            UseDirection = useDirection.sqrMagnitude > 0.001f ? useDirection.normalized : Vector3.forward;
            ConfirmPlacement = confirmPlacement;
        }

        public string PlotDefinitionId { get; }

        public Vector3 UseOrigin { get; }

        public Vector3 UseDirection { get; }

        public bool ConfirmPlacement { get; }
    }
}
