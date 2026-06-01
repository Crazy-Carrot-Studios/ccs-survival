using CCS.Modules.Inventory;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CookingEventArgs
// CATEGORY: Modules / Cooking / Runtime / Events
// PURPOSE: Event payload for cooking station lifecycle and consumable food notifications.
// PLACEMENT: Passed to cooking service event subscribers and HUD notifications.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Includes station, recipe IDs, and world position for 1.0.0 foundation.
// =============================================================================

namespace CCS.Modules.Cooking
{
    public sealed class CCS_CookingEventArgs
    {
        #region Public Methods

        public CCS_CookingEventArgs(
            CCS_CookingStation station,
            CCS_CookingStationType stationType,
            string recipeId,
            string rawItemId,
            string cookedItemId,
            Vector3 worldPosition,
            string message,
            CCS_ItemDefinition itemDefinition = null,
            CCS_CampfireDefinition campfireDefinition = null,
            string campfireInstanceKey = "")
        {
            Station = station;
            StationType = stationType;
            RecipeId = recipeId ?? string.Empty;
            RawItemId = rawItemId ?? string.Empty;
            CookedItemId = cookedItemId ?? string.Empty;
            WorldPosition = worldPosition;
            Message = message ?? string.Empty;
            ItemDefinition = itemDefinition;
            CampfireDefinition = campfireDefinition;
            CampfireInstanceKey = campfireInstanceKey ?? string.Empty;
        }

        public CCS_CookingEventArgs(
            CCS_CampfireDefinition campfireDefinition = null,
            CCS_ItemDefinition itemDefinition = null,
            string campfireInstanceKey = "",
            string message = "")
            : this(
                null,
                CCS_CookingStationType.None,
                string.Empty,
                string.Empty,
                string.Empty,
                Vector3.zero,
                message,
                itemDefinition,
                campfireDefinition,
                campfireInstanceKey)
        {
        }

        #endregion

        #region Properties

        public CCS_CookingStation Station { get; }

        public CCS_CookingStationType StationType { get; }

        public string RecipeId { get; }

        public string RawItemId { get; }

        public string CookedItemId { get; }

        public Vector3 WorldPosition { get; }

        public string Message { get; }

        public CCS_ItemDefinition ItemDefinition { get; }

        public CCS_CampfireDefinition CampfireDefinition { get; }

        public string CampfireInstanceKey { get; }

        #endregion
    }
}
