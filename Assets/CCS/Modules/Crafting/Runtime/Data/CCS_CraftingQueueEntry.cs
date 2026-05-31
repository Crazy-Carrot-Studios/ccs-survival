// =============================================================================
// SCRIPT: CCS_CraftingQueueEntry
// CATEGORY: Modules / Crafting / Runtime / Data
// PURPOSE: Represents a queued crafting job for timed crafting support.
// PLACEMENT: Stored by CCS_CraftingService when queueing is enabled.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Queue processing deferred in 0.5.0 foundation. Data shape only.
// =============================================================================

namespace CCS.Modules.Crafting
{
    public sealed class CCS_CraftingQueueEntry
    {
        #region Public Methods

        public CCS_CraftingQueueEntry(
            CCS_CraftingRecipeDefinition recipe,
            CCS_CraftingStationContext stationContext,
            float durationSeconds,
            int craftCount = 1)
        {
            Recipe = recipe;
            StationContext = stationContext;
            DurationSeconds = durationSeconds < 0f ? 0f : durationSeconds;
            RemainingSeconds = DurationSeconds;
            CraftCount = craftCount < 1 ? 1 : craftCount;
        }

        #endregion

        #region Properties

        public CCS_CraftingRecipeDefinition Recipe { get; }

        public CCS_CraftingStationContext StationContext { get; }

        public float DurationSeconds { get; }

        public float RemainingSeconds { get; set; }

        public int CraftCount { get; }

        #endregion
    }
}
