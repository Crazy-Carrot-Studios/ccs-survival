using CCS.Modules.Inventory;

// =============================================================================
// SCRIPT: CCS_CookingEventArgs
// CATEGORY: Modules / Cooking / Runtime / Events
// PURPOSE: Event payload for cooking and campfire notifications.
// PLACEMENT: Passed to cooking service event subscribers.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: No UI references in 0.9.4 foundation.
// =============================================================================

namespace CCS.Modules.Cooking
{
    public sealed class CCS_CookingEventArgs
    {
        #region Public Methods

        public CCS_CookingEventArgs(
            CCS_CampfireDefinition campfireDefinition = null,
            CCS_ItemDefinition itemDefinition = null,
            string campfireInstanceKey = "",
            string message = "")
        {
            CampfireDefinition = campfireDefinition;
            ItemDefinition = itemDefinition;
            CampfireInstanceKey = campfireInstanceKey ?? string.Empty;
            Message = message ?? string.Empty;
        }

        #endregion

        #region Properties

        public CCS_CampfireDefinition CampfireDefinition { get; }

        public CCS_ItemDefinition ItemDefinition { get; }

        public string CampfireInstanceKey { get; }

        public string Message { get; }

        #endregion
    }
}
