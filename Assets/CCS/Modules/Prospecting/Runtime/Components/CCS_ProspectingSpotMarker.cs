using UnityEngine;

// =============================================================================
// SCRIPT: CCS_ProspectingSpotMarker
// CATEGORY: Modules / Prospecting / Runtime / Components
// PURPOSE: Placeholder marker for future prospecting interactions (1.7.0).
// PLACEMENT: Bootstrap scene prospecting test objects.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// =============================================================================

namespace CCS.Modules.Prospecting
{
    public sealed class CCS_ProspectingSpotMarker : MonoBehaviour
    {
        [SerializeField] private string prospectingSpotId = "ccs.survival.prospecting.spot.frontier";

        public string ProspectingSpotId => prospectingSpotId;
    }
}
