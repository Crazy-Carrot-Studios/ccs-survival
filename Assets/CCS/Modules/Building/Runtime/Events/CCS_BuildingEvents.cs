// =============================================================================
// SCRIPT: CCS_BuildingEvents
// CATEGORY: Modules / Building / Runtime / Events
// PURPOSE: Event delegate definitions for building catalog and placement lifecycle.
// PLACEMENT: Subscribed by HUD/debug presenters and future build mode systems.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Placement events added in 0.8.1 foundation.
// =============================================================================

namespace CCS.Modules.Building
{
    public static class CCS_BuildingEvents
    {
        #region Variables

        public const string BuildingDefinitionRegistered = "BuildingDefinitionRegistered";

        public const string BuildingStateChanged = "BuildingStateChanged";

        public const string PlacementStarted = "PlacementStarted";

        public const string PlacementCancelled = "PlacementCancelled";

        public const string BuildingPlaced = "BuildingPlaced";

        public const string PlacementFailed = "PlacementFailed";

        #endregion
    }

    public delegate void BuildingDefinitionRegisteredHandler(CCS_BuildingEventArgs eventArgs);

    public delegate void BuildingStateChangedHandler(CCS_BuildingEventArgs eventArgs);

    public delegate void PlacementStartedHandler(CCS_BuildingPlacementEventArgs eventArgs);

    public delegate void PlacementCancelledHandler(CCS_BuildingPlacementEventArgs eventArgs);

    public delegate void BuildingPlacedHandler(CCS_BuildingPlacementEventArgs eventArgs);

    public delegate void PlacementFailedHandler(CCS_BuildingPlacementFailedEventArgs eventArgs);
}
