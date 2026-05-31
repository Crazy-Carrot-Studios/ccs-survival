// =============================================================================
// SCRIPT: CCS_CraftingEvents
// CATEGORY: Modules / Crafting / Runtime / Events
// PURPOSE: Event name constants and delegate contracts for crafting systems.
// PLACEMENT: Instance events on CCS_CraftingService document contracts here.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Subscribers react to crafting flow without UI or interaction coupling.
// =============================================================================

namespace CCS.Modules.Crafting
{
    public static class CCS_CraftingEvents
    {
        public const string CraftingRequestedEventName = "Crafting.Requested";
        public const string CraftingStartedEventName = "Crafting.Started";
        public const string CraftingCompletedEventName = "Crafting.Completed";
        public const string CraftingFailedEventName = "Crafting.Failed";
        public const string RecipeUnlockedEventName = "Crafting.RecipeUnlocked";
    }

    public delegate void CraftingRequestedHandler(CCS_CraftingEventArgs eventArgs);

    public delegate void CraftingStartedHandler(CCS_CraftingEventArgs eventArgs);

    public delegate void CraftingCompletedHandler(CCS_CraftingEventArgs eventArgs);

    public delegate void CraftingFailedHandler(CCS_CraftingEventArgs eventArgs);

    public delegate void RecipeUnlockedHandler(CCS_CraftingEventArgs eventArgs);
}
