// =============================================================================
// SCRIPT: CCS_BuildingPlacementFailedEventArgs
// CATEGORY: Modules / Building / Runtime / Events
// PURPOSE: Event payload for failed building placement attempts.
// PLACEMENT: Passed to PlacementFailed event subscribers.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Includes validation result for HUD missing-item notifications.
// =============================================================================

namespace CCS.Modules.Building
{
    public sealed class CCS_BuildingPlacementFailedEventArgs
    {
        #region Public Methods

        public CCS_BuildingPlacementFailedEventArgs(
            CCS_BuildingPlacementSnapshot placementSnapshot,
            CCS_BuildingPlacementValidationResult validationResult)
        {
            PlacementSnapshot = placementSnapshot;
            ValidationResult = validationResult;
        }

        #endregion

        #region Properties

        public CCS_BuildingPlacementSnapshot PlacementSnapshot { get; }

        public CCS_BuildingPlacementValidationResult ValidationResult { get; }

        #endregion
    }
}
