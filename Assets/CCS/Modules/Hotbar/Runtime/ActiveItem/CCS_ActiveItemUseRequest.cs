using UnityEngine;

// =============================================================================
// SCRIPT: CCS_ActiveItemUseRequest
// CATEGORY: Modules / Hotbar / Runtime / ActiveItem
// PURPOSE: Payload for an active item use attempt from player input.
// PLACEMENT: Passed to CCS_ActiveItemService.TryUseActiveItem.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Origin/direction support combat routing without camera coupling in the service.
// =============================================================================

namespace CCS.Modules.Hotbar
{
    public readonly struct CCS_ActiveItemUseRequest
    {
        public CCS_ActiveItemUseRequest(Vector3 useOrigin, Vector3 useDirection)
        {
            UseOrigin = useOrigin;
            UseDirection = useDirection;
        }

        public Vector3 UseOrigin { get; }

        public Vector3 UseDirection { get; }
    }
}
