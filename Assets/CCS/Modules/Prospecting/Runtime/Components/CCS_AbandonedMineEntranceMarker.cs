using UnityEngine;

// =============================================================================
// SCRIPT: CCS_AbandonedMineEntranceMarker
// CATEGORY: Modules / Prospecting / Runtime / Components
// PURPOSE: Placeholder for abandoned mine entrance (no cave systems in 1.7.0).
// PLACEMENT: Bootstrap scene mining test area.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// =============================================================================

namespace CCS.Modules.Prospecting
{
    public sealed class CCS_AbandonedMineEntranceMarker : MonoBehaviour
    {
        [SerializeField] private string entranceId = "ccs.survival.mining.entrance.abandoned";

        public string EntranceId => entranceId;
    }
}
