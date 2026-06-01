// =============================================================================
// SCRIPT: CCS_CraftingProgressionEvents
// CATEGORY: Modules / Crafting / Runtime / Events
// PURPOSE: Delegate contracts for crafting progression recipe lifecycle events.
// PLACEMENT: Instance events on CCS_CraftingRecipeService.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Milestone 1.1.1 crafting progression foundation.
// =============================================================================

namespace CCS.Modules.Crafting
{
    public delegate void CraftingRecipeValidatedHandler(CCS_CraftingProgressionEventArgs eventArgs);

    public delegate void CraftingRecipeFailedHandler(CCS_CraftingProgressionEventArgs eventArgs);

    public delegate void CraftingProgressionStartedHandler(CCS_CraftingProgressionEventArgs eventArgs);

    public delegate void CraftingProgressionCompletedHandler(CCS_CraftingProgressionEventArgs eventArgs);

    public delegate void CraftingResourcesConsumedHandler(CCS_CraftingProgressionEventArgs eventArgs);
}
