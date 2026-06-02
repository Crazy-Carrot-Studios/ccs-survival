using UnityEngine;

// =============================================================================
// SCRIPT: CCS_FirearmUseRequest
// CATEGORY: Modules / Firearms / Runtime / Data
// PURPOSE: Origin and direction for a firearm use attempt.
// PLACEMENT: Passed from active item or playtest harness into CCS_FirearmService.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// =============================================================================

namespace CCS.Modules.Firearms
{
    public readonly struct CCS_FirearmUseRequest
    {
        public CCS_FirearmUseRequest(Vector3 useOrigin, Vector3 useDirection)
        {
            UseOrigin = useOrigin;
            UseDirection = useDirection;
        }

        public Vector3 UseOrigin { get; }

        public Vector3 UseDirection { get; }
    }
}
