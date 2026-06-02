// =============================================================================
// SCRIPT: CCS_TrapPlacementRequest
// CATEGORY: Modules / Trapping / Runtime / Data
// PURPOSE: Request payload for trap placement preview and confirmation.
// PLACEMENT: Built by CCS_ActiveItemService when using a placeable trap item.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// =============================================================================

using UnityEngine;

namespace CCS.Modules.Trapping
{
    public sealed class CCS_TrapPlacementRequest
    {
        public CCS_TrapPlacementRequest(
            CCS_TrapDefinition trapDefinition,
            Vector3 origin,
            Vector3 forward,
            bool confirmPlacement)
        {
            TrapDefinition = trapDefinition;
            Origin = origin;
            Forward = forward.sqrMagnitude > 0.0001f ? forward.normalized : Vector3.forward;
            ConfirmPlacement = confirmPlacement;
        }

        public CCS_TrapDefinition TrapDefinition { get; }

        public Vector3 Origin { get; }

        public Vector3 Forward { get; }

        public bool ConfirmPlacement { get; }
    }
}
