using System.Collections.Generic;
using CCS.Modules.Inventory;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CraftingRecipeService
// CATEGORY: Modules / Crafting / Runtime / Services
// PURPOSE: Progression recipe lookup, station filtering, and crafting orchestration.
// PLACEMENT: Registered as CCS_ISurvivalService by survival gameplay composition wiring.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Wraps CCS_CraftingService without replacing legacy crafting flows.
// =============================================================================

namespace CCS.Modules.Crafting
{
    public sealed class CCS_CraftingRecipeService : CCS_ISurvivalService
    {
        private const string LogPrefix = "[CCS_CraftingRecipeService]";

        #region Variables

        private readonly Dictionary<string, CCS_CraftingRecipeDefinition> recipesById =
            new Dictionary<string, CCS_CraftingRecipeDefinition>();

        private readonly List<CCS_CraftingRecipeDefinition> recipesByStationCache =
            new List<CCS_CraftingRecipeDefinition>();

        private CCS_CraftingProgressionProfile activeProfile;
        private CCS_CraftingService craftingService;
        private bool isInitialized;

        #endregion

        #region Events

        public event CraftingRecipeValidatedHandler CraftingRecipeValidated;
        public event CraftingRecipeFailedHandler CraftingRecipeFailed;
        public event CraftingProgressionStartedHandler CraftingStarted;
        public event CraftingProgressionCompletedHandler CraftingCompleted;
        public event CraftingResourcesConsumedHandler CraftingResourcesConsumed;

        #endregion

        #region Properties

        public bool IsInitialized => isInitialized;

        public bool ProgressionEnabled => activeProfile != null && activeProfile.ProgressionEnabled;

        public CCS_CraftingProgressionProfile ActiveProfile => activeProfile;

        #endregion

        #region Public Methods

        public void Initialize()
        {
            if (isInitialized)
            {
                return;
            }

            isInitialized = true;
        }

        public void InitializeFromProfile(
            CCS_CraftingProgressionProfile profile,
            CCS_CraftingService boundCraftingService)
        {
            activeProfile = profile;
            craftingService = boundCraftingService;
            recipesById.Clear();
            isInitialized = profile != null && craftingService != null;

            if (!ProgressionEnabled || profile?.ProgressionRecipes == null)
            {
                return;
            }

            IReadOnlyList<CCS_CraftingProgressionRecipeEntry> entries = profile.ProgressionRecipes;
            for (int index = 0; index < entries.Count; index++)
            {
                RegisterProgressionRecipe(entries[index]);
            }
        }

        public void BindCraftingService(CCS_CraftingService service)
        {
            craftingService = service;
            isInitialized = activeProfile != null && craftingService != null;
        }

        public bool TryGetRecipeById(string recipeId, out CCS_CraftingRecipeDefinition recipe)
        {
            recipe = null;
            if (string.IsNullOrWhiteSpace(recipeId))
            {
                return false;
            }

            return recipesById.TryGetValue(recipeId, out recipe) && recipe != null;
        }

        public IReadOnlyList<CCS_CraftingRecipeDefinition> GetRecipesForStation(CCS_CraftingStationType stationType)
        {
            recipesByStationCache.Clear();
            foreach (KeyValuePair<string, CCS_CraftingRecipeDefinition> entry in recipesById)
            {
                CCS_CraftingRecipeDefinition recipe = entry.Value;
                if (recipe != null && recipe.RequiredStationType == stationType)
                {
                    recipesByStationCache.Add(recipe);
                }
            }

            return recipesByStationCache;
        }

        public bool IsWorkbenchRecipe(CCS_CraftingRecipeDefinition recipe)
        {
            return recipe != null && recipe.RequiredStationType == CCS_CraftingStationType.Workbench;
        }

        public CCS_CraftingResult TryCraftProgressionRecipe(
            CCS_CraftingRecipeDefinition recipe,
            CCS_CraftingStationContext stationContext,
            int craftCount = 1)
        {
            if (!EnsureReady())
            {
                return CCS_CraftingResult.Failure(recipe, "Crafting recipe service is not initialized.");
            }

            if (recipe == null)
            {
                return Fail(recipe, CCS_CraftingStationType.Hand, craftCount, "Recipe is null.");
            }

            CCS_CraftingStationContext resolvedContext = stationContext
                ?? craftingService.GetSnapshot().ActiveStationContext
                ?? CCS_CraftingStationContext.CreateHandContext();

            if (!TryAuthorizeRecipe(recipe, resolvedContext, out string failureMessage))
            {
                return Fail(recipe, resolvedContext.StationType, craftCount, failureMessage);
            }

            RaiseValidated(recipe, resolvedContext, craftCount);
            RaiseStarted(recipe, resolvedContext, craftCount);

            CCS_CraftingResult craftResult = craftingService.TryCraft(
                new CCS_CraftingRequest(recipe, resolvedContext, craftCount));

            if (!craftResult.IsSuccess)
            {
                return Fail(recipe, resolvedContext.StationType, craftCount, craftResult.Message);
            }

            RaiseResourcesConsumed(recipe, resolvedContext, craftCount);
            RaiseCompleted(recipe, resolvedContext, craftCount, craftResult.Message);
            return craftResult;
        }

        public void ApplyActiveStationContext(CCS_CraftingStationContext stationContext)
        {
            if (craftingService == null || stationContext == null)
            {
                return;
            }

            craftingService.SetActiveStationContext(stationContext);
        }

        #endregion

        #region Private Methods

        private bool EnsureReady()
        {
            return isInitialized
                && craftingService != null
                && craftingService.IsInitialized;
        }

        private void RegisterProgressionRecipe(CCS_CraftingProgressionRecipeEntry entry)
        {
            if (entry?.RecipeDefinition == null || string.IsNullOrWhiteSpace(entry.RecipeDefinition.RecipeId))
            {
                return;
            }

            recipesById[entry.RecipeDefinition.RecipeId] = entry.RecipeDefinition;
            craftingService.RegisterDefaultUnlockedRecipe(entry.RecipeDefinition);
        }

        private bool TryAuthorizeRecipe(
            CCS_CraftingRecipeDefinition recipe,
            CCS_CraftingStationContext stationContext,
            out string failureMessage)
        {
            failureMessage = string.Empty;

            if (!ProgressionEnabled)
            {
                failureMessage = "Crafting progression is disabled.";
                return false;
            }

            if (!recipesById.ContainsKey(recipe.RecipeId))
            {
                failureMessage = $"Recipe '{recipe.RecipeId}' is not registered in the progression profile.";
                return false;
            }

            if (recipe.RequiredStationType != stationContext.StationType)
            {
                failureMessage =
                    $"Recipe requires {recipe.RequiredStationType} but active station is {stationContext.StationType}.";
                return false;
            }

            CCS_SurvivalValidationResult recipeValidation =
                CCS_CraftingValidationUtility.ValidateRecipeDefinition(recipe);

            if (!recipeValidation.IsSuccess)
            {
                failureMessage = recipeValidation.Message;
                return false;
            }

            return true;
        }

        private CCS_CraftingResult Fail(
            CCS_CraftingRecipeDefinition recipe,
            CCS_CraftingStationType stationType,
            int craftCount,
            string message)
        {
            string recipeId = recipe != null ? recipe.RecipeId : string.Empty;
            RaiseFailed(recipeId, stationType, craftCount, message);
            return CCS_CraftingResult.Failure(recipe, message);
        }

        private void LogDebug(string message)
        {
            if (activeProfile != null && activeProfile.EnableDebugLogging)
            {
                Debug.Log($"{LogPrefix} {message}");
            }
        }

        private void RaiseValidated(
            CCS_CraftingRecipeDefinition recipe,
            CCS_CraftingStationContext stationContext,
            int craftCount)
        {
            CraftingRecipeValidated?.Invoke(
                new CCS_CraftingProgressionEventArgs(
                    recipe.RecipeId,
                    stationContext.StationType,
                    craftCount,
                    "Crafting recipe validated."));
            LogDebug($"Validated {recipe.RecipeId} at {stationContext.StationType}.");
        }

        private void RaiseFailed(string recipeId, CCS_CraftingStationType stationType, int craftCount, string message)
        {
            CraftingRecipeFailed?.Invoke(
                new CCS_CraftingProgressionEventArgs(recipeId, stationType, craftCount, message));
            LogDebug($"Failed {recipeId}: {message}");
        }

        private void RaiseStarted(
            CCS_CraftingRecipeDefinition recipe,
            CCS_CraftingStationContext stationContext,
            int craftCount)
        {
            CraftingStarted?.Invoke(
                new CCS_CraftingProgressionEventArgs(
                    recipe.RecipeId,
                    stationContext.StationType,
                    craftCount,
                    "Crafting started."));
        }

        private void RaiseResourcesConsumed(
            CCS_CraftingRecipeDefinition recipe,
            CCS_CraftingStationContext stationContext,
            int craftCount)
        {
            CraftingResourcesConsumed?.Invoke(
                new CCS_CraftingProgressionEventArgs(
                    recipe.RecipeId,
                    stationContext.StationType,
                    craftCount,
                    "Crafting resources consumed."));
        }

        private void RaiseCompleted(
            CCS_CraftingRecipeDefinition recipe,
            CCS_CraftingStationContext stationContext,
            int craftCount,
            string message)
        {
            CraftingCompleted?.Invoke(
                new CCS_CraftingProgressionEventArgs(
                    recipe.RecipeId,
                    stationContext.StationType,
                    craftCount,
                    message));
            LogDebug($"Completed {recipe.RecipeId}: {message}");
        }

        #endregion
    }
}
