using System.Collections.Generic;

// =============================================================================
// SCRIPT: CCS_CraftingSnapshot
// CATEGORY: Modules / Crafting / Runtime / Data
// PURPOSE: Read-only crafting state snapshot for queries and future save hooks.
// PLACEMENT: Returned by CCS_CraftingService snapshot methods.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Immutable after creation. No save serialization in 0.5.0.
// =============================================================================

namespace CCS.Modules.Crafting
{
    public sealed class CCS_CraftingSnapshot
    {
        #region Public Methods

        public CCS_CraftingSnapshot(
            CCS_CraftingStationContext activeStationContext,
            IReadOnlyList<CCS_CraftingQueueEntry> queueEntries,
            IReadOnlyList<string> unlockedRecipeIds)
        {
            ActiveStationContext = activeStationContext;
            QueueEntries = queueEntries;
            UnlockedRecipeIds = unlockedRecipeIds;
        }

        #endregion

        #region Properties

        public CCS_CraftingStationContext ActiveStationContext { get; }

        public IReadOnlyList<CCS_CraftingQueueEntry> QueueEntries { get; }

        public IReadOnlyList<string> UnlockedRecipeIds { get; }

        #endregion
    }
}
