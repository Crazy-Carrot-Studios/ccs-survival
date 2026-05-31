using System.Collections.Generic;
using CCS.Modules.Inventory;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CraftingService
// CATEGORY: Modules / Crafting / Runtime / Services
// PURPOSE: Validates and executes crafting against inventory and station context.
// PLACEMENT: Registered as CCS_ISurvivalService by survival gameplay composition wiring.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Inventory integration with rollback on partial failure at 0.5.3.
// =============================================================================

namespace CCS.Modules.Crafting
{
    public sealed class CCS_CraftingService : CCS_ISurvivalService
    {
        private const string LogPrefix = "[CCS_CraftingService]";

        #region Variables

        private readonly HashSet<string> unlockedRecipeIds = new HashSet<string>();
        private readonly List<CCS_CraftingQueueEntry> queueEntries = new List<CCS_CraftingQueueEntry>();

        private CCS_CraftingProfile activeProfile;
        private CCS_PlayerInventoryService inventoryService;
        private CCS_CraftingStationContext activeStationContext;
        private bool isInitialized;

        #endregion

        #region Events

        public event CraftingRequestedHandler CraftingRequested;
        public event CraftingStartedHandler CraftingStarted;
        public event CraftingCompletedHandler CraftingCompleted;
        public event CraftingFailedHandler CraftingFailed;
        public event RecipeUnlockedHandler RecipeUnlocked;

        #endregion

        #region Properties

        public bool IsInitialized => isInitialized;

        public CCS_CraftingProfile ActiveProfile => activeProfile;

        #endregion

        #region Public Methods

        public void Initialize()
        {
            if (isInitialized)
            {
                return;
            }

            activeStationContext = CCS_CraftingStationContext.CreateHandContext();
        }

        public void InitializeFromProfile(
            CCS_CraftingProfile profile,
            CCS_PlayerInventoryService playerInventoryService)
        {
            if (profile == null)
            {
                Debug.LogWarning($"{LogPrefix} InitializeFromProfile called with null profile.");
                isInitialized = false;
                return;
            }

            CCS_SurvivalValidationResult validation = CCS_CraftingValidationUtility.ValidateProfile(profile);
            if (!validation.IsSuccess)
            {
                Debug.LogWarning($"{LogPrefix} Profile validation warning: {validation.Message}");
            }

            activeProfile = profile;
            inventoryService = playerInventoryService;
            activeStationContext = CCS_CraftingStationContext.CreateHandContext();
            isInitialized = inventoryService != null && inventoryService.IsInitialized;

            if (!isInitialized && inventoryService == null)
            {
                Debug.LogWarning($"{LogPrefix} Inventory service is required for crafting.");
            }
            else if (!isInitialized && inventoryService != null && !inventoryService.IsInitialized)
            {
                Debug.LogWarning($"{LogPrefix} Inventory service is not initialized.");
            }
        }

        public void RegisterDefaultUnlockedRecipe(CCS_CraftingRecipeDefinition recipe)
        {
            if (recipe == null || !recipe.IsUnlockedByDefault)
            {
                return;
            }

            UnlockRecipe(recipe, raiseEvent: false);
        }

        public bool IsRecipeUnlocked(CCS_CraftingRecipeDefinition recipe)
        {
            if (recipe == null || string.IsNullOrWhiteSpace(recipe.RecipeId))
            {
                return false;
            }

            return unlockedRecipeIds.Contains(recipe.RecipeId);
        }

        public bool UnlockRecipe(CCS_CraftingRecipeDefinition recipe, bool raiseEvent = true)
        {
            if (recipe == null || string.IsNullOrWhiteSpace(recipe.RecipeId))
            {
                return false;
            }

            if (!unlockedRecipeIds.Add(recipe.RecipeId))
            {
                return false;
            }

            if (raiseEvent)
            {
                RaiseRecipeUnlocked(recipe, activeStationContext, "Recipe unlocked.");
            }

            return true;
        }

        public void SetActiveStationContext(CCS_CraftingStationContext stationContext)
        {
            activeStationContext = stationContext ?? CCS_CraftingStationContext.CreateHandContext();
        }

        public CCS_CraftingSnapshot GetSnapshot()
        {
            return new CCS_CraftingSnapshot(
                activeStationContext,
                queueEntries.AsReadOnly(),
                new List<string>(unlockedRecipeIds));
        }

        public CCS_CraftingResult TryCraft(CCS_CraftingRequest request)
        {
            if (!EnsureInitialized())
            {
                return FailCraft(request?.Recipe, "Crafting service is not initialized.");
            }

            if (request == null)
            {
                return FailCraft(null, "Crafting request is null.");
            }

            CCS_CraftingRecipeDefinition recipe = request.Recipe;
            CCS_CraftingStationContext stationContext = request.StationContext
                ?? activeStationContext
                ?? CCS_CraftingStationContext.CreateHandContext();

            RaiseCraftingRequested(recipe, stationContext, request.CraftCount);

            CCS_SurvivalValidationResult recipeValidation =
                CCS_CraftingValidationUtility.ValidateRecipeDefinition(recipe);

            if (!recipeValidation.IsSuccess)
            {
                return FailCraft(recipe, recipeValidation.Message, stationContext, request.CraftCount);
            }

            if (!IsRecipeUnlocked(recipe) && !recipe.IsUnlockedByDefault)
            {
                return FailCraft(recipe, "Recipe is locked.", stationContext, request.CraftCount);
            }

            CCS_SurvivalValidationResult stationValidation =
                ValidateStationRequirements(recipe, stationContext);

            if (!stationValidation.IsSuccess)
            {
                return FailCraft(recipe, stationValidation.Message, stationContext, request.CraftCount);
            }

            CCS_SurvivalValidationResult inventoryValidation =
                ValidateInventoryRequirements(recipe, request.CraftCount);

            if (!inventoryValidation.IsSuccess)
            {
                return FailCraft(recipe, inventoryValidation.Message, stationContext, request.CraftCount);
            }

            if (activeProfile != null && activeProfile.AllowQueueing && recipe.CraftTimeSeconds > 0f)
            {
                return FailCraft(
                    recipe,
                    "Timed queue crafting is not implemented in 0.5.0 foundation.",
                    stationContext,
                    request.CraftCount);
            }

            RaiseCraftingStarted(recipe, stationContext, request.CraftCount);

            if (!ConsumeIngredients(recipe, request.CraftCount))
            {
                return FailCraft(recipe, "Failed to consume crafting ingredients.", stationContext, request.CraftCount);
            }

            if (!GrantResults(recipe, request.CraftCount))
            {
                RestoreIngredients(recipe, request.CraftCount);
                return FailCraft(recipe, "Failed to grant crafting results.", stationContext, request.CraftCount);
            }

            CCS_CraftingResult success = CCS_CraftingResult.Success(
                recipe,
                request.CraftCount,
                "Crafting completed.");

            RaiseCraftingCompleted(recipe, stationContext, request.CraftCount, success.Message);
            return success;
        }

        #endregion

        #region Private Methods

        private bool EnsureInitialized()
        {
            return isInitialized
                && inventoryService != null
                && inventoryService.IsInitialized
                && activeProfile != null;
        }

        private CCS_SurvivalValidationResult ValidateStationRequirements(
            CCS_CraftingRecipeDefinition recipe,
            CCS_CraftingStationContext stationContext)
        {
            if (stationContext == null)
            {
                return CCS_SurvivalValidationResult.Fail("Station context is null.");
            }

            if (recipe.RequiredStationType != stationContext.StationType)
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Required station type {recipe.RequiredStationType} does not match active station {stationContext.StationType}.");
            }

            if (recipe.RequiredStationType == CCS_CraftingStationType.Hand
                && activeProfile != null
                && !activeProfile.AllowHandCrafting)
            {
                return CCS_SurvivalValidationResult.Fail("Hand crafting is disabled by profile.");
            }

            return CCS_SurvivalValidationResult.Pass("Station requirements validated.");
        }

        private CCS_SurvivalValidationResult ValidateInventoryRequirements(
            CCS_CraftingRecipeDefinition recipe,
            int craftCount)
        {
            if (inventoryService == null)
            {
                return CCS_SurvivalValidationResult.Fail("Inventory service is unavailable.");
            }

            IReadOnlyList<CCS_CraftingIngredientDefinition> ingredients = recipe.Ingredients;
            for (int i = 0; i < ingredients.Count; i++)
            {
                CCS_CraftingIngredientDefinition ingredient = ingredients[i];
                if (ingredient == null)
                {
                    continue;
                }

                int requiredQuantity = ingredient.Quantity * craftCount;
                if (!inventoryService.HasItem(ingredient.ItemDefinition, requiredQuantity))
                {
                    return CCS_SurvivalValidationResult.Fail("Missing required crafting ingredients.");
                }
            }

            IReadOnlyList<CCS_CraftingResultDefinition> results = recipe.Results;
            for (int i = 0; i < results.Count; i++)
            {
                CCS_CraftingResultDefinition result = results[i];
                if (result == null)
                {
                    continue;
                }

                int grantQuantity = result.Quantity * craftCount;
                if (!inventoryService.CanAdd(result.ItemDefinition, grantQuantity))
                {
                    return CCS_SurvivalValidationResult.Fail("Inventory cannot hold crafting results.");
                }
            }

            return CCS_SurvivalValidationResult.Pass("Inventory requirements validated.");
        }

        private bool ConsumeIngredients(CCS_CraftingRecipeDefinition recipe, int craftCount)
        {
            List<(CCS_ItemDefinition itemDefinition, int quantity)> consumedEntries =
                new List<(CCS_ItemDefinition, int)>();

            IReadOnlyList<CCS_CraftingIngredientDefinition> ingredients = recipe.Ingredients;
            for (int i = 0; i < ingredients.Count; i++)
            {
                CCS_CraftingIngredientDefinition ingredient = ingredients[i];
                if (ingredient == null || ingredient.ItemDefinition == null)
                {
                    continue;
                }

                int requiredQuantity = ingredient.Quantity * craftCount;
                int removed = inventoryService.RemoveItem(ingredient.ItemDefinition, requiredQuantity);
                if (removed < requiredQuantity)
                {
                    RestoreConsumedEntries(consumedEntries);
                    return false;
                }

                consumedEntries.Add((ingredient.ItemDefinition, removed));
            }

            return true;
        }

        private bool GrantResults(CCS_CraftingRecipeDefinition recipe, int craftCount)
        {
            IReadOnlyList<CCS_CraftingResultDefinition> results = recipe.Results;
            for (int i = 0; i < results.Count; i++)
            {
                CCS_CraftingResultDefinition result = results[i];
                if (result == null || result.ItemDefinition == null)
                {
                    continue;
                }

                int grantQuantity = result.Quantity * craftCount;
                int added = inventoryService.AddItem(result.ItemDefinition, grantQuantity);
                if (added < grantQuantity)
                {
                    return false;
                }
            }

            return true;
        }

        private void RestoreIngredients(CCS_CraftingRecipeDefinition recipe, int craftCount)
        {
            IReadOnlyList<CCS_CraftingIngredientDefinition> ingredients = recipe.Ingredients;
            for (int i = 0; i < ingredients.Count; i++)
            {
                CCS_CraftingIngredientDefinition ingredient = ingredients[i];
                if (ingredient == null || ingredient.ItemDefinition == null)
                {
                    continue;
                }

                int restoreQuantity = ingredient.Quantity * craftCount;
                inventoryService.AddItem(ingredient.ItemDefinition, restoreQuantity);
            }
        }

        private void RestoreConsumedEntries(List<(CCS_ItemDefinition itemDefinition, int quantity)> consumedEntries)
        {
            for (int i = 0; i < consumedEntries.Count; i++)
            {
                (CCS_ItemDefinition itemDefinition, int quantity) entry = consumedEntries[i];
                if (entry.itemDefinition == null || entry.quantity <= 0)
                {
                    continue;
                }

                inventoryService.AddItem(entry.itemDefinition, entry.quantity);
            }
        }

        private CCS_CraftingResult FailCraft(
            CCS_CraftingRecipeDefinition recipe,
            string message,
            CCS_CraftingStationContext stationContext = null,
            int craftCount = 0)
        {
            CCS_CraftingResult failure = CCS_CraftingResult.Failure(recipe, message);
            RaiseCraftingFailed(recipe, stationContext ?? activeStationContext, craftCount, message);
            return failure;
        }

        private void RaiseCraftingRequested(
            CCS_CraftingRecipeDefinition recipe,
            CCS_CraftingStationContext stationContext,
            int craftCount)
        {
            CraftingRequested?.Invoke(new CCS_CraftingEventArgs(recipe, stationContext, craftCount, "Crafting requested."));
        }

        private void RaiseCraftingStarted(
            CCS_CraftingRecipeDefinition recipe,
            CCS_CraftingStationContext stationContext,
            int craftCount)
        {
            CraftingStarted?.Invoke(new CCS_CraftingEventArgs(recipe, stationContext, craftCount, "Crafting started."));
        }

        private void RaiseCraftingCompleted(
            CCS_CraftingRecipeDefinition recipe,
            CCS_CraftingStationContext stationContext,
            int craftCount,
            string message)
        {
            CraftingCompleted?.Invoke(new CCS_CraftingEventArgs(recipe, stationContext, craftCount, message));
        }

        private void RaiseCraftingFailed(
            CCS_CraftingRecipeDefinition recipe,
            CCS_CraftingStationContext stationContext,
            int craftCount,
            string message)
        {
            CraftingFailed?.Invoke(new CCS_CraftingEventArgs(recipe, stationContext, craftCount, message));
        }

        private void RaiseRecipeUnlocked(
            CCS_CraftingRecipeDefinition recipe,
            CCS_CraftingStationContext stationContext,
            string message)
        {
            RecipeUnlocked?.Invoke(new CCS_CraftingEventArgs(recipe, stationContext, 0, message));
        }

        #endregion
    }
}
