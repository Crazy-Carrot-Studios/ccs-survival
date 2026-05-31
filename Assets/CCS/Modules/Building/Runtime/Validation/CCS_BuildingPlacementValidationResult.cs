// =============================================================================
// SCRIPT: CCS_BuildingPlacementValidationResult
// CATEGORY: Modules / Building / Runtime / Validation
// PURPOSE: Result payload for building placement validation and TryPlaceCurrentPiece.
// PLACEMENT: Returned by CCS_BuildingPlacementValidationUtility and placement service.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Includes failure reason and missing item label for HUD notifications.
// =============================================================================

namespace CCS.Modules.Building
{
    public readonly struct CCS_BuildingPlacementValidationResult
    {
        #region Public Methods

        public CCS_BuildingPlacementValidationResult(
            bool success,
            string failureReason,
            string missingItemDisplayName = null)
        {
            Success = success;
            FailureReason = failureReason ?? string.Empty;
            MissingItemDisplayName = missingItemDisplayName ?? string.Empty;
        }

        public static CCS_BuildingPlacementValidationResult Passed =>
            new CCS_BuildingPlacementValidationResult(true, string.Empty);

        public static CCS_BuildingPlacementValidationResult Failed(
            string failureReason,
            string missingItemDisplayName = null)
        {
            return new CCS_BuildingPlacementValidationResult(false, failureReason, missingItemDisplayName);
        }

        #endregion

        #region Properties

        public bool Success { get; }

        public string FailureReason { get; }

        public string MissingItemDisplayName { get; }

        #endregion
    }
}
