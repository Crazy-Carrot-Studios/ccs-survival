// =============================================================================
// SCRIPT: CCS_CookingEvents
// CATEGORY: Modules / Cooking / Runtime / Events
// PURPOSE: Event name constants and delegate contracts for cooking systems.
// PLACEMENT: Instance events on cooking services document contracts here.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Subscribers react to cooking flow without UI coupling.
// =============================================================================

namespace CCS.Modules.Cooking
{
    public static class CCS_CookingEvents
    {
        public const string CampfireLitEventName = "Cooking.CampfireLit";
        public const string CookingStartedEventName = "Cooking.Started";
        public const string CookingCompletedEventName = "Cooking.Completed";
        public const string CookingFailedEventName = "Cooking.Failed";
        public const string FoodConsumedEventName = "Cooking.FoodConsumed";
        public const string FoodConsumeFailedEventName = "Cooking.FoodConsumeFailed";
    }

    public delegate void CampfireLitHandler(CCS_CookingEventArgs eventArgs);

    public delegate void CookingStartedHandler(CCS_CookingEventArgs eventArgs);

    public delegate void CookingCompletedHandler(CCS_CookingEventArgs eventArgs);

    public delegate void CookingFailedHandler(CCS_CookingEventArgs eventArgs);

    public delegate void CookingCancelledHandler(CCS_CookingEventArgs eventArgs);

    public delegate void FoodConsumedHandler(CCS_CookingEventArgs eventArgs);

    public delegate void FoodConsumeFailedHandler(CCS_CookingEventArgs eventArgs);
}
