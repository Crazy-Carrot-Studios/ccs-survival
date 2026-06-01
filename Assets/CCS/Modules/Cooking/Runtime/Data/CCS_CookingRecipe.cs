using System;
using System.Collections.Generic;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CookingRecipe
// CATEGORY: Modules / Cooking / Runtime / Data
// PURPOSE: Serializable campfire recipe mapping raw items, fuel, and cooked outputs.
// PLACEMENT: Embedded on CCS_CookingProfile recipe list.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Fuel resolved through acceptedFuelItemIds inventory item ID strings.
// =============================================================================

namespace CCS.Modules.Cooking
{
    [Serializable]
    public sealed class CCS_CookingRecipe
    {
        #region Variables

        [Tooltip("Stable recipe identifier used by CCS_CookingService.TryStartCooking.")]
        [SerializeField] private string recipeId = string.Empty;

        [Tooltip("Display name used by interaction prompts and notifications.")]
        [SerializeField] private string displayName = string.Empty;

        [Tooltip("Inventory item ID consumed as the raw ingredient.")]
        [SerializeField] private string rawItemDefinitionId = string.Empty;

        [Tooltip("Inventory item ID granted when cooking completes.")]
        [SerializeField] private string cookedItemDefinitionId = string.Empty;

        [Tooltip("Raw ingredient quantity consumed per cook.")]
        [SerializeField] private int rawAmount = 1;

        [Tooltip("Cooked output quantity granted per cook.")]
        [SerializeField] private int cookedAmount = 1;

        [Tooltip("Seconds required to finish this recipe. Zero uses profile default.")]
        [SerializeField] private float cookDurationSeconds;

        [Tooltip("Inventory item IDs accepted as fuel for this recipe.")]
        [SerializeField] private List<string> acceptedFuelItemIds = new List<string>();

        [Tooltip("Fuel quantity consumed when cooking starts.")]
        [SerializeField] private int requiredFuelAmount = 1;

        #endregion

        #region Properties

        public string RecipeId => recipeId;

        public string DisplayName => displayName;

        public string RawItemDefinitionId => rawItemDefinitionId;

        public string CookedItemDefinitionId => cookedItemDefinitionId;

        public int RawAmount => rawAmount;

        public int CookedAmount => cookedAmount;

        public float CookDurationSeconds => cookDurationSeconds;

        public IReadOnlyList<string> AcceptedFuelItemIds => acceptedFuelItemIds;

        public int RequiredFuelAmount => requiredFuelAmount;

        #endregion
    }
}
