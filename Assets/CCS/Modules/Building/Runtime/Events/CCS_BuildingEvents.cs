// =============================================================================
// SCRIPT: CCS_BuildingEvents
// CATEGORY: Modules / Building / Runtime / Events
// PURPOSE: Event delegate definitions for building catalog lifecycle.
// PLACEMENT: Subscribed by HUD/debug presenters and future placement systems.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Foundation only. No placement or construction events in 0.8.0.
// =============================================================================

namespace CCS.Modules.Building
{
    public static class CCS_BuildingEvents
    {
        #region Variables

        public const string BuildingDefinitionRegistered = "BuildingDefinitionRegistered";

        public const string BuildingStateChanged = "BuildingStateChanged";

        #endregion
    }

    public delegate void BuildingDefinitionRegisteredHandler(CCS_BuildingEventArgs eventArgs);

    public delegate void BuildingStateChangedHandler(CCS_BuildingEventArgs eventArgs);
}
