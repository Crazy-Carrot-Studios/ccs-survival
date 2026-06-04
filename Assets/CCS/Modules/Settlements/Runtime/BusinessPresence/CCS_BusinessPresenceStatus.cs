// =============================================================================
// SCRIPT: CCS_BusinessPresenceStatus
// CATEGORY: Modules / Settlements / Runtime / BusinessPresence
// PURPOSE: Dev-readable business presence states for world markers and labels.
// PLACEMENT: Used by presence markers, labels, and validation utilities.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.8.0 — visible business presence foundation.
// =============================================================================

namespace CCS.Modules.Settlements
{
    public enum CCS_BusinessPresenceStatus
    {
        Locked = 0,
        Inactive = 1,
        Active = 2
    }
}
