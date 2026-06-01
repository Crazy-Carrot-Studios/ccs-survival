// =============================================================================
// SCRIPT: CCS_FishingEvents
// CATEGORY: Modules / Fishing / Runtime / Events
// PURPOSE: Delegate types for CCS_FishingService events.
// PLACEMENT: Used by playtest harness and future UI without direct service polling.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Foundation event surface only.
// =============================================================================

namespace CCS.Modules.Fishing
{
    public delegate void FishingAttemptedHandler(CCS_FishingEventArgs eventArgs);

    public delegate void FishingCatchGrantedHandler(CCS_FishingEventArgs eventArgs);
}
