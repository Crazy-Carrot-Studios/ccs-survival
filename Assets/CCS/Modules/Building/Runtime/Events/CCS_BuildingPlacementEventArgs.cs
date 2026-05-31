// =============================================================================
// SCRIPT: CCS_BuildingPlacementEventArgs
// CATEGORY: Modules / Building / Runtime / Events
// PURPOSE: Event payload for building placement lifecycle.
// PLACEMENT: Passed to placement event subscribers.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Includes placement snapshot and placed instance count for HUD refresh.
// =============================================================================

namespace CCS.Modules.Building
{
    public sealed class CCS_BuildingPlacementEventArgs
    {
        #region Public Methods

        public CCS_BuildingPlacementEventArgs(
            CCS_BuildingPlacementSnapshot placementSnapshot,
            int placedInstanceCount,
            CCS_BuildingInstance placedInstance,
            string message)
        {
            PlacementSnapshot = placementSnapshot;
            PlacedInstanceCount = placedInstanceCount < 0 ? 0 : placedInstanceCount;
            PlacedInstance = placedInstance;
            Message = message ?? string.Empty;
        }

        #endregion

        #region Properties

        public CCS_BuildingPlacementSnapshot PlacementSnapshot { get; }

        public int PlacedInstanceCount { get; }

        public CCS_BuildingInstance PlacedInstance { get; }

        public string Message { get; }

        #endregion
    }
}
