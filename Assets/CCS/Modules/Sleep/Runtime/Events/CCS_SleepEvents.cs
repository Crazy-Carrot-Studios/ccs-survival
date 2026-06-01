// =============================================================================
// SCRIPT: CCS_SleepEvents
// CATEGORY: Modules / Sleep / Runtime / Events
// PURPOSE: Event name constants and delegate contracts for sleep systems.
// PLACEMENT: Instance events on CCS_SleepService document contracts here.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Subscribers react to sleep flow without UI coupling.
// =============================================================================

namespace CCS.Modules.Sleep
{
    public static class CCS_SleepEvents
    {
        public const string SleepCompletedEventName = "Sleep.Completed";
        public const string SleepFailedEventName = "Sleep.Failed";
    }

    public delegate void SleepCompletedHandler(CCS_SleepEventArgs eventArgs);

    public delegate void SleepFailedHandler(CCS_SleepEventArgs eventArgs);
}
