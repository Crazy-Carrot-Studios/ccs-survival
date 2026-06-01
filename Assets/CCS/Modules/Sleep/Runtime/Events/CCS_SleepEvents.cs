// =============================================================================
// SCRIPT: CCS_SleepEvents
// CATEGORY: Modules / Sleep / Runtime / Events
// PURPOSE: Event handler delegates for sleep spot lifecycle notifications.
// PLACEMENT: Subscribed by playtest harness, HUD wiring, and diagnostics.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Milestone 1.1.3 sleep and bedroll foundation.
// =============================================================================

namespace CCS.Modules.Sleep
{
    public delegate void SleepStartedHandler(CCS_SleepEventArgs eventArgs);

    public delegate void SleepCompletedHandler(CCS_SleepEventArgs eventArgs);

    public delegate void SleepFailedHandler(CCS_SleepEventArgs eventArgs);

    public delegate void SleepRespawnPointAssignedHandler(CCS_SleepEventArgs eventArgs);

    public delegate void SleepStateRestoredHandler(CCS_SleepEventArgs eventArgs);
}
