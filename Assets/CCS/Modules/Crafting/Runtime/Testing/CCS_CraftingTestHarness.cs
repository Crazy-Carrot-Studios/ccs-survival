using CCS.Modules.Inventory;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CraftingTestHarness
// CATEGORY: Modules / Crafting / Runtime / Testing
// PURPOSE: Development-only harness that attempts test recipes when inventory is ready.
// PLACEMENT: Bootstrap verification scenes only. Disable for shipping builds.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Uses CCS_CraftingService through the runtime registry. Not final player input.
// =============================================================================

namespace CCS.Modules.Crafting
{
    [DefaultExecutionOrder(260)]
    public sealed class CCS_CraftingTestHarness : MonoBehaviour
    {
        #region Variables

        [Header("Development Testing")]
        [Tooltip("When enabled, the harness attempts configured test recipes after harvest resources are available.")]
        [SerializeField] private bool enableHarness = true;

        [Tooltip("Seconds between automated craft attempts.")]
        [SerializeField] private float craftAttemptIntervalSeconds = 5f;

        [Tooltip("First test recipe attempted when ingredients are available.")]
        [SerializeField] private CCS_CraftingRecipeDefinition bandageRecipe;

        [Tooltip("Second test recipe attempted when ingredients are available.")]
        [SerializeField] private CCS_CraftingRecipeDefinition campfireRecipe;

        private float nextCraftAttemptTime;
        private bool bandageCraftAttempted;
        private bool campfireCraftAttempted;

        #endregion

        #region Unity Callbacks

        private void Update()
        {
            if (!enableHarness)
            {
                return;
            }

            if (Time.time < nextCraftAttemptTime)
            {
                return;
            }

            if (!CCS_CraftingRuntimeBridge.TryGetCraftingService(out CCS_CraftingService craftingService)
                || !craftingService.IsInitialized)
            {
                return;
            }

            if (!CCS_CraftingRuntimeBridge.TryGetInventoryService(out CCS_PlayerInventoryService inventoryService)
                || !inventoryService.IsInitialized)
            {
                return;
            }

            nextCraftAttemptTime = Time.time + craftAttemptIntervalSeconds;

            if (!bandageCraftAttempted && TryAttemptRecipe(craftingService, inventoryService, bandageRecipe))
            {
                bandageCraftAttempted = true;
                return;
            }

            if (!campfireCraftAttempted && TryAttemptRecipe(craftingService, inventoryService, campfireRecipe))
            {
                campfireCraftAttempted = true;
            }
        }

        #endregion

        #region Private Methods

        private static bool TryAttemptRecipe(
            CCS_CraftingService craftingService,
            CCS_PlayerInventoryService inventoryService,
            CCS_CraftingRecipeDefinition recipe)
        {
            if (recipe == null)
            {
                return false;
            }

            if (!HasRequiredIngredients(inventoryService, recipe))
            {
                Debug.Log($"[CCS_CraftingTestHarness] Waiting for ingredients: {recipe.DisplayName}.");
                return false;
            }

            CCS_CraftingRequest request = new CCS_CraftingRequest(
                recipe,
                CCS_CraftingStationContext.CreateHandContext());

            CCS_CraftingResult result = craftingService.TryCraft(request);
            Debug.Log(result.IsSuccess
                ? $"[CCS_CraftingTestHarness] Craft succeeded: {recipe.DisplayName} — {result.Message}"
                : $"[CCS_CraftingTestHarness] Craft failed: {recipe.DisplayName} — {result.Message}");

            return result.IsSuccess;
        }

        private static bool HasRequiredIngredients(
            CCS_PlayerInventoryService inventoryService,
            CCS_CraftingRecipeDefinition recipe)
        {
            for (int i = 0; i < recipe.Ingredients.Count; i++)
            {
                CCS_CraftingIngredientDefinition ingredient = recipe.Ingredients[i];
                if (ingredient == null || ingredient.ItemDefinition == null)
                {
                    continue;
                }

                if (!inventoryService.HasItem(ingredient.ItemDefinition, ingredient.Quantity))
                {
                    return false;
                }
            }

            return true;
        }

        #endregion
    }
}
