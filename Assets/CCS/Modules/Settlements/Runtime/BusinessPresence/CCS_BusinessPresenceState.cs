using System;

// =============================================================================
// SCRIPT: CCS_BusinessPresenceState
// CATEGORY: Modules / Settlements / Runtime / BusinessPresence
// PURPOSE: Optional runtime visibility override for a presence anchor.
// PLACEMENT: Derived from business simulation by default; overrides are optional.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.8.0 — prefer deriving marker state from BusinessService snapshots.
// =============================================================================

namespace CCS.Modules.Settlements
{
    [Serializable]
    public sealed class CCS_BusinessPresenceState
    {
        public string anchorId = string.Empty;

        public string businessId = string.Empty;

        public bool hasVisibilityOverride;

        public int visibilityOverrideStatus;
    }
}
