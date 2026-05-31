// =============================================================================
// SCRIPT: CCS_CraftingStationContext
// CATEGORY: Modules / Crafting / Runtime / Stations
// PURPOSE: Runtime-safe description of the station available for crafting.
// PLACEMENT: Passed with crafting requests and stored in snapshots.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: No world MonoBehaviour yet. StationId reserved for future station objects.
// =============================================================================

namespace CCS.Modules.Crafting
{
    public sealed class CCS_CraftingStationContext
    {
        #region Public Methods

        public static CCS_CraftingStationContext CreateHandContext(string displayName = "Hand")
        {
            return new CCS_CraftingStationContext(
                CCS_CraftingStationType.Hand,
                displayName,
                string.Empty);
        }

        public CCS_CraftingStationContext(
            CCS_CraftingStationType stationType,
            string displayName,
            string stationId)
        {
            StationType = stationType;
            DisplayName = displayName ?? string.Empty;
            StationId = stationId ?? string.Empty;
        }

        #endregion

        #region Properties

        public CCS_CraftingStationType StationType { get; }

        public string DisplayName { get; }

        public string StationId { get; }

        #endregion
    }
}
