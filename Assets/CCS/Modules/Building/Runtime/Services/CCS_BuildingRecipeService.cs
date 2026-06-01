using System.Collections.Generic;
using CCS.Modules.Inventory;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_BuildingRecipeService
// CATEGORY: Modules / Building / Runtime / Services
// PURPOSE: Recipe lookup, placement authorization, and inventory cost handling for building.
// PLACEMENT: Registered as CCS_ISurvivalService by survival gameplay composition wiring.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Milestone 1.1.0 building progression foundation.
// =============================================================================

namespace CCS.Modules.Building
{
    public sealed class CCS_BuildingRecipeService : CCS_ISurvivalService
    {
        private const string LogPrefix = "[CCS_BuildingRecipeService]";

        #region Variables

        private readonly Dictionary<string, CCS_BuildingRecipe> recipesById =
            new Dictionary<string, CCS_BuildingRecipe>();

        private readonly Dictionary<string, CCS_BuildingRecipe> recipesByPieceId =
            new Dictionary<string, CCS_BuildingRecipe>();

        private CCS_BuildingProgressionProfile activeProfile;
        private CCS_BuildingService buildingService;
        private CCS_PlayerInventoryService inventoryService;
        private bool isInitialized;

        #endregion

        #region Events

        public event BuildingRecipeValidatedHandler BuildingRecipeValidated;
        public event BuildingRecipeFailedHandler BuildingRecipeFailed;
        public event BuildingPiecePlacedHandler BuildingPiecePlaced;
        public event BuildingResourcesConsumedHandler BuildingResourcesConsumed;

        #endregion

        #region Properties

        public bool IsInitialized => isInitialized;

        public bool ProgressionEnabled => activeProfile != null && activeProfile.ProgressionEnabled;

        public CCS_BuildingProgressionProfile ActiveProfile => activeProfile;

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

        public void InitializeFromProfile(CCS_BuildingProgressionProfile profile)
        {
            activeProfile = profile;
            recipesById.Clear();
            recipesByPieceId.Clear();
            isInitialized = profile != null;

            if (profile?.RecipeDefinitions == null)
            {
                return;
            }

            for (int index = 0; index < profile.RecipeDefinitions.Count; index++)
            {
                RegisterRecipe(profile.RecipeDefinitions[index]);
            }
        }

        public void BindBuildingService(CCS_BuildingService service)
        {
            buildingService = service;
        }

        public void BindInventoryService(CCS_PlayerInventoryService service)
        {
            inventoryService = service;
        }

        public bool TryGetRecipeById(string recipeId, out CCS_BuildingRecipe recipe)
        {
            recipe = null;
            if (string.IsNullOrWhiteSpace(recipeId))
            {
                return false;
            }

            return recipesById.TryGetValue(recipeId, out recipe);
        }

        public bool TryGetRecipeForPiece(string pieceDefinitionId, out CCS_BuildingRecipe recipe)
        {
            recipe = null;
            if (string.IsNullOrWhiteSpace(pieceDefinitionId))
            {
                return false;
            }

            return recipesByPieceId.TryGetValue(pieceDefinitionId, out recipe);
        }

        public bool TryAuthorizePlacement(
            CCS_BuildingPieceDefinition definition,
            CCS_BuildingPlacementState placementState,
            out CCS_BuildingRecipe recipe,
            out string failureMessage)
        {
            recipe = null;
            failureMessage = string.Empty;

            if (!isInitialized || definition == null)
            {
                failureMessage = "Building recipe service is not ready.";
                return false;
            }

            if (!ProgressionEnabled)
            {
                return true;
            }

            if (!TryGetRecipeForPiece(definition.PieceId, out recipe))
            {
                failureMessage = $"No building recipe registered for '{definition.PieceId}'.";
                RaiseRecipeFailed(string.Empty, definition.PieceId, definition.PieceCategory, failureMessage);
                return false;
            }

            if (!CCS_BuildingProgressionPlacementUtility.TryValidatePlacementRules(
                    buildingService,
                    definition,
                    recipe,
                    placementState,
                    out failureMessage))
            {
                RaiseRecipeFailed(recipe.RecipeId, recipe.PieceDefinitionId, recipe.PieceCategory, failureMessage);
                return false;
            }

            if (!TryValidateInventoryCosts(recipe, out failureMessage))
            {
                RaiseRecipeFailed(recipe.RecipeId, recipe.PieceDefinitionId, recipe.PieceCategory, failureMessage);
                return false;
            }

            RaiseRecipeValidated(recipe, "Recipe validated for placement.");
            return true;
        }

        public bool TryConsumeRecipeCosts(
            CCS_BuildingRecipe recipe,
            out string failureMessage)
        {
            failureMessage = string.Empty;

            if (!ProgressionEnabled || recipe == null)
            {
                return true;
            }

            if (inventoryService == null || !inventoryService.IsInitialized)
            {
                failureMessage = "Inventory service is unavailable.";
                return false;
            }

            IReadOnlyList<CCS_BuildingRecipeRequiredItem> requiredItems = recipe.RequiredItems;
            List<(CCS_ItemDefinition itemDefinition, int quantity)> consumedEntries =
                new List<(CCS_ItemDefinition, int)>();

            for (int index = 0; index < requiredItems.Count; index++)
            {
                CCS_BuildingRecipeRequiredItem requiredItem = requiredItems[index];
                CCS_ItemDefinition itemDefinition = ResolveItemDefinition(requiredItem);
                if (itemDefinition == null || requiredItem.quantity <= 0)
                {
                    continue;
                }

                int removed = inventoryService.RemoveItem(itemDefinition, requiredItem.quantity);
                if (removed < requiredItem.quantity)
                {
                    RestoreConsumedEntries(inventoryService, consumedEntries);
                    failureMessage = $"Failed to consume '{itemDefinition.DisplayName}'.";
                    RaiseRecipeFailed(recipe.RecipeId, recipe.PieceDefinitionId, recipe.PieceCategory, failureMessage);
                    return false;
                }

                consumedEntries.Add((itemDefinition, removed));
            }

            RaiseResourcesConsumed(recipe, "Building resources consumed.");
            return true;
        }

        public void NotifyPiecePlaced(CCS_BuildingPieceDefinition definition)
        {
            if (definition == null)
            {
                return;
            }

            TryGetRecipeForPiece(definition.PieceId, out CCS_BuildingRecipe recipe);
            string recipeId = recipe != null ? recipe.RecipeId : string.Empty;
            BuildingPiecePlaced?.Invoke(
                new CCS_BuildingProgressionEventArgs(
                    recipeId,
                    definition.PieceId,
                    definition.PieceCategory,
                    $"Placed {definition.DisplayName}."));
        }

        public bool MeetsMinimumShelter()
        {
            return CCS_BuildingProgressionPlacementUtility.MeetsMinimumShelter(buildingService, activeProfile);
        }

        public void RestoreRecipeCosts(CCS_BuildingRecipe recipe)
        {
            if (recipe == null || inventoryService == null || !inventoryService.IsInitialized)
            {
                return;
            }

            IReadOnlyList<CCS_BuildingRecipeRequiredItem> requiredItems = recipe.RequiredItems;
            for (int index = 0; index < requiredItems.Count; index++)
            {
                CCS_BuildingRecipeRequiredItem requiredItem = requiredItems[index];
                CCS_ItemDefinition itemDefinition = ResolveItemDefinition(requiredItem);
                if (itemDefinition == null || requiredItem.quantity <= 0)
                {
                    continue;
                }

                inventoryService.AddItem(itemDefinition, requiredItem.quantity);
            }
        }

        #endregion

        #region Private Methods

        private void RegisterRecipe(CCS_BuildingRecipe recipe)
        {
            if (recipe == null || string.IsNullOrWhiteSpace(recipe.RecipeId))
            {
                return;
            }

            recipesById[recipe.RecipeId] = recipe;
            if (!string.IsNullOrWhiteSpace(recipe.PieceDefinitionId))
            {
                recipesByPieceId[recipe.PieceDefinitionId] = recipe;
            }
        }

        private bool TryValidateInventoryCosts(CCS_BuildingRecipe recipe, out string failureMessage)
        {
            failureMessage = string.Empty;

            if (inventoryService == null || !inventoryService.IsInitialized)
            {
                failureMessage = "Inventory service is unavailable.";
                return false;
            }

            IReadOnlyList<CCS_BuildingRecipeRequiredItem> requiredItems = recipe.RequiredItems;
            for (int index = 0; index < requiredItems.Count; index++)
            {
                CCS_BuildingRecipeRequiredItem requiredItem = requiredItems[index];
                CCS_ItemDefinition itemDefinition = ResolveItemDefinition(requiredItem);
                if (itemDefinition == null || requiredItem.quantity <= 0)
                {
                    continue;
                }

                if (inventoryService.HasItem(itemDefinition, requiredItem.quantity))
                {
                    continue;
                }

                failureMessage = $"Missing required item '{itemDefinition.DisplayName}'.";
                return false;
            }

            return true;
        }

        private static CCS_ItemDefinition ResolveItemDefinition(CCS_BuildingRecipeRequiredItem requiredItem)
        {
            if (requiredItem.itemDefinition != null)
            {
                return requiredItem.itemDefinition;
            }

            return null;
        }

        private static void RestoreConsumedEntries(
            CCS_PlayerInventoryService inventoryService,
            List<(CCS_ItemDefinition itemDefinition, int quantity)> consumedEntries)
        {
            for (int index = 0; index < consumedEntries.Count; index++)
            {
                (CCS_ItemDefinition itemDefinition, int quantity) entry = consumedEntries[index];
                inventoryService.AddItem(entry.itemDefinition, entry.quantity);
            }
        }

        private void RaiseRecipeValidated(CCS_BuildingRecipe recipe, string message)
        {
            BuildingRecipeValidated?.Invoke(
                new CCS_BuildingProgressionEventArgs(
                    recipe.RecipeId,
                    recipe.PieceDefinitionId,
                    recipe.PieceCategory,
                    message));
            LogDebug(message);
        }

        private void RaiseRecipeFailed(
            string recipeId,
            string pieceDefinitionId,
            CCS_BuildingPieceCategory category,
            string message)
        {
            BuildingRecipeFailed?.Invoke(
                new CCS_BuildingProgressionEventArgs(recipeId, pieceDefinitionId, category, message));
            LogDebug(message);
        }

        private void RaiseResourcesConsumed(CCS_BuildingRecipe recipe, string message)
        {
            BuildingResourcesConsumed?.Invoke(
                new CCS_BuildingProgressionEventArgs(
                    recipe.RecipeId,
                    recipe.PieceDefinitionId,
                    recipe.PieceCategory,
                    message));
            LogDebug(message);
        }

        private void LogDebug(string message)
        {
            if (activeProfile != null && activeProfile.EnableDebugLogging)
            {
                Debug.Log($"{LogPrefix} {message}");
            }
        }

        #endregion
    }
}
