// =============================================================================
// SCRIPT: CCS_CraftingRequest
// CATEGORY: Modules / Crafting / Runtime / Data
// PURPOSE: Represents a single crafting attempt request.
// PLACEMENT: Passed to CCS_CraftingService craft methods.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Immediate crafting supported. Queueing deferred until profile enables it.
// =============================================================================

namespace CCS.Modules.Crafting
{
    public sealed class CCS_CraftingRequest
    {
        #region Public Methods

        public CCS_CraftingRequest(
            CCS_CraftingRecipeDefinition recipe,
            CCS_CraftingStationContext stationContext,
            int craftCount = 1)
        {
            Recipe = recipe;
            StationContext = stationContext;
            CraftCount = craftCount < 1 ? 1 : craftCount;
        }

        #endregion

        #region Properties

        public CCS_CraftingRecipeDefinition Recipe { get; }

        public CCS_CraftingStationContext StationContext { get; }

        public int CraftCount { get; }

        #endregion
    }
}
