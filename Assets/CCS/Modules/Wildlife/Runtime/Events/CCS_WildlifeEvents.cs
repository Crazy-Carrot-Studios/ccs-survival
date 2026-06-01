// =============================================================================
// SCRIPT: CCS_WildlifeEvents
// CATEGORY: Modules / Wildlife / Runtime / Events
// PURPOSE: Event name constants and delegate contracts for wildlife harvest systems.
// PLACEMENT: Instance events on CCS_WildlifeHarvestService document contracts here.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Subscribers react to harvest flow without UI coupling.
// =============================================================================

namespace CCS.Modules.Wildlife
{
    public static class CCS_WildlifeEvents
    {
        public const string WildlifeHarvestStartedEventName = "Wildlife.HarvestStarted";
        public const string WildlifeHarvestCompletedEventName = "Wildlife.HarvestCompleted";
        public const string WildlifeHarvestFailedEventName = "Wildlife.HarvestFailed";
        public const string WildlifeDepletedEventName = "Wildlife.Depleted";
    }

    public delegate void WildlifeHarvestStartedHandler(CCS_WildlifeEventArgs eventArgs);

    public delegate void WildlifeHarvestCompletedHandler(CCS_WildlifeEventArgs eventArgs);

    public delegate void WildlifeHarvestFailedHandler(CCS_WildlifeEventArgs eventArgs);

    public delegate void WildlifeDepletedHandler(CCS_WildlifeEventArgs eventArgs);
}
