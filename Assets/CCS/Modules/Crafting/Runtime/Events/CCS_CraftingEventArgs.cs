// =============================================================================
// SCRIPT: CCS_CraftingEventArgs
// CATEGORY: Modules / Crafting / Runtime / Events
// PURPOSE: Event payload for crafting request, progress, completion, and unlock notifications.
// PLACEMENT: Passed to CCS_CraftingService event subscribers.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: No UI, interaction, or save data in 0.5.0.
// =============================================================================

namespace CCS.Modules.Crafting
{
    public sealed class CCS_CraftingEventArgs
    {
        #region Public Methods

        public CCS_CraftingEventArgs(
            CCS_CraftingRecipeDefinition recipe,
            CCS_CraftingStationContext stationContext,
            int craftCount = 1,
            string message = "")
        {
            Recipe = recipe;
            StationContext = stationContext;
            CraftCount = craftCount < 0 ? 0 : craftCount;
            Message = message ?? string.Empty;
        }

        #endregion

        #region Properties

        public CCS_CraftingRecipeDefinition Recipe { get; }

        public CCS_CraftingStationContext StationContext { get; }

        public int CraftCount { get; }

        public string Message { get; }

        #endregion
    }
}
