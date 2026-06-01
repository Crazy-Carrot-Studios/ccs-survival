using System.Collections.Generic;
using CCS.Core;
using CCS.Modules.Inventory;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CookingService
// CATEGORY: Modules / Cooking / Runtime / Services
// PURPOSE: Registers cooking stations, validates fuel and ingredients, and grants outputs.
// PLACEMENT: Registered by CCS_SurvivalGameplayServiceRegistration from cooking profile.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Inventory integration uses CCS_PlayerInventoryService public APIs only.
// =============================================================================

namespace CCS.Modules.Cooking
{
    public sealed class CCS_CookingService : CCS_ISurvivalService, CCS_IUpdatable
    {
        private const string LogPrefix = "[CCS_CookingService]";

        private sealed class ActiveCookJob
        {
            public ActiveCookJob(
                CCS_CookingStation station,
                CCS_CookingRecipe recipe,
                float remainingSeconds,
                CCS_CookingRequest legacyRequest = null)
            {
                Station = station;
                Recipe = recipe;
                LegacyRequest = legacyRequest;
                RemainingSeconds = remainingSeconds;
            }

            public CCS_CookingStation Station { get; }

            public CCS_CookingRecipe Recipe { get; }

            public CCS_CookingRequest LegacyRequest { get; }

            public float RemainingSeconds { get; set; }
        }

        #region Variables

        private readonly HashSet<CCS_CookingStation> registeredStations = new HashSet<CCS_CookingStation>();
        private readonly List<ActiveCookJob> activeCookJobs = new List<ActiveCookJob>();

        private CCS_CookingProfile activeProfile;
        private CCS_PlayerInventoryService inventoryService;
        private CCS_CampfireService campfireService;
        private bool isInitialized;

        #endregion

        #region Events

        public event CookingStartedHandler CookingStarted;
        public event CookingCompletedHandler CookingCompleted;
        public event CookingFailedHandler CookingFailed;
        public event CookingCancelledHandler CookingCancelled;

        #endregion

        #region Properties

        public bool IsInitialized => isInitialized;

        public CCS_CookingProfile ActiveProfile => activeProfile;

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
            CCS_CookingProfile profile,
            CCS_PlayerInventoryService inventoryServiceOverride = null)
        {
            if (profile == null)
            {
                Debug.LogWarning($"{LogPrefix} InitializeFromProfile called with null profile.");
                isInitialized = false;
                return;
            }

            CCS_SurvivalValidationResult validation = CCS_CookingValidationUtility.ValidateProfile(profile);
            if (!validation.IsSuccess)
            {
                Debug.LogWarning($"{LogPrefix} Profile validation warning: {validation.Message}");
            }

            profile.BuildRecipeLookup();
            activeProfile = profile;
            if (inventoryServiceOverride != null)
            {
                inventoryService = inventoryServiceOverride;
            }

            isInitialized = true;
        }

        public void BindInventoryService(CCS_PlayerInventoryService service)
        {
            inventoryService = service;
        }

        public void BindCampfireService(CCS_CampfireService service)
        {
            campfireService = service;
        }

        public void RegisterStation(CCS_CookingStation station)
        {
            if (station == null)
            {
                return;
            }

            registeredStations.Add(station);
        }

        public void UnregisterStation(CCS_CookingStation station)
        {
            if (station == null)
            {
                return;
            }

            registeredStations.Remove(station);
        }

        public bool TryFindFirstCookableRecipe(
            CCS_CookingStation station,
            out CCS_CookingRecipe recipe,
            out string failureMessage)
        {
            recipe = null;
            failureMessage = string.Empty;

            if (!EnsureInitialized() || activeProfile == null || station == null)
            {
                failureMessage = "Cooking service is unavailable.";
                return false;
            }

            if (!station.CanCook())
            {
                failureMessage = "Cooking station is not ready.";
                return false;
            }

            IReadOnlyList<CCS_CookingRecipe> recipes = activeProfile.Recipes;
            if (recipes == null || recipes.Count == 0)
            {
                failureMessage = "No cooking recipes are configured.";
                return false;
            }

            for (int index = 0; index < recipes.Count; index++)
            {
                CCS_CookingRecipe candidate = recipes[index];
                if (candidate == null)
                {
                    continue;
                }

                if (ValidateRecipeRequirements(candidate, out _))
                {
                    recipe = candidate;
                    return true;
                }
            }

            failureMessage = "Missing raw food or fuel for campfire cooking.";
            return false;
        }

        public CCS_CookingResult TryStartCooking(CCS_CookingStation station, string recipeId)
        {
            if (!EnsureInitialized() || activeProfile == null)
            {
                return FailCooking(station, null, "Cooking service is not initialized.");
            }

            if (activeProfile != null && !activeProfile.EnableCooking)
            {
                return FailCooking(station, null, "Cooking is disabled.");
            }

            if (station == null)
            {
                return FailCooking(null, null, "Cooking station is null.");
            }

            if (!station.CanCook())
            {
                return FailCooking(station, null, "Cooking station is not ready.");
            }

            if (!activeProfile.TryGetRecipe(recipeId, out CCS_CookingRecipe recipe))
            {
                return FailCooking(station, null, $"Cooking recipe '{recipeId}' was not found.");
            }

            if (!ValidateRecipeRequirements(recipe, out string validationMessage))
            {
                return FailCooking(station, recipe, validationMessage);
            }

            if (!TryConsumeRecipeIngredients(recipe, out string consumeMessage))
            {
                return FailCooking(station, recipe, consumeMessage);
            }

            float cookDuration = recipe.CookDurationSeconds > 0f
                ? recipe.CookDurationSeconds
                : activeProfile.DefaultCookDurationSeconds;

            station.ApplyCookingStarted(recipe.RecipeId);
            activeCookJobs.Add(new ActiveCookJob(station, recipe, cookDuration));

            if (activeProfile.EnableDebugLogs)
            {
                Debug.Log($"{LogPrefix} Started {recipe.DisplayName} on {station.StationType}.");
            }

            RaiseCookingStarted(station, recipe, "Cooking started.");
            return CCS_CookingResult.Success("Cooking started.");
        }

        public CCS_CookingResult TryStartCooking(CCS_CookingRequest request)
        {
            if (!EnsureInitialized())
            {
                return CCS_CookingResult.Failure("Cooking service is not initialized.");
            }

            if (request == null)
            {
                return CCS_CookingResult.Failure("Cooking request is null.");
            }

            CCS_CookingStation matchingStation = FindStationForCampfireKey(request.CampfireInstanceKey);
            if (matchingStation != null
                && TryFindFirstCookableRecipe(matchingStation, out CCS_CookingRecipe stationRecipe, out _))
            {
                return TryStartCooking(matchingStation, stationRecipe.RecipeId);
            }

            return TryStartLegacyRequest(request);
        }

        public bool CanCook(CCS_CookingRequest request)
        {
            return request != null
                && request.InputItemDefinition != null
                && request.OutputItemDefinition != null
                && inventoryService != null
                && inventoryService.IsInitialized
                && inventoryService.GetQuantity(request.InputItemDefinition) > 0
                && inventoryService.CanAdd(request.OutputItemDefinition, 1);
        }

        public void Tick(float deltaTime)
        {
            if (!EnsureInitialized() || activeCookJobs.Count == 0)
            {
                return;
            }

            for (int index = activeCookJobs.Count - 1; index >= 0; index--)
            {
                ActiveCookJob job = activeCookJobs[index];
                job.RemainingSeconds -= deltaTime;

                if (job.RemainingSeconds > 0f)
                {
                    continue;
                }

                activeCookJobs.RemoveAt(index);
                CompleteCookJob(job);
            }
        }

        public void CancelCooking(CCS_CookingStation station)
        {
            if (station == null)
            {
                return;
            }

            for (int index = activeCookJobs.Count - 1; index >= 0; index--)
            {
                ActiveCookJob job = activeCookJobs[index];
                if (job.Station != station)
                {
                    continue;
                }

                activeCookJobs.RemoveAt(index);
                station.CancelCooking();
                RaiseCookingCancelled(station, job.Recipe, "Cooking cancelled.");
                return;
            }
        }

        #endregion

        #region Private Methods

        private bool EnsureInitialized()
        {
            return isInitialized;
        }

        private bool ValidateRecipeRequirements(CCS_CookingRecipe recipe, out string failureMessage)
        {
            failureMessage = string.Empty;
            if (recipe == null)
            {
                failureMessage = "Cooking recipe is null.";
                return false;
            }

            if (inventoryService == null || !inventoryService.IsInitialized)
            {
                failureMessage = "Inventory service is not initialized.";
                return false;
            }

            if (!activeProfile.TryResolveItemDefinition(recipe.RawItemDefinitionId, out CCS_ItemDefinition rawItem))
            {
                failureMessage = $"Raw item '{recipe.RawItemDefinitionId}' could not be resolved.";
                return false;
            }

            if (!activeProfile.TryResolveItemDefinition(recipe.CookedItemDefinitionId, out CCS_ItemDefinition cookedItem))
            {
                failureMessage = $"Cooked item '{recipe.CookedItemDefinitionId}' could not be resolved.";
                return false;
            }

            if (inventoryService.GetQuantity(rawItem) < recipe.RawAmount)
            {
                failureMessage = $"Missing {recipe.DisplayName} ingredients.";
                return false;
            }

            if (!inventoryService.CanAdd(cookedItem, recipe.CookedAmount))
            {
                failureMessage = "Inventory cannot hold cooked food.";
                return false;
            }

            if (!TryResolveFuelItem(recipe, out CCS_ItemDefinition fuelItem, out int fuelAmount))
            {
                failureMessage = "Missing stick or wood fuel.";
                return false;
            }

            if (inventoryService.GetQuantity(fuelItem) < fuelAmount)
            {
                failureMessage = "Missing stick or wood fuel.";
                return false;
            }

            return true;
        }

        private bool TryResolveFuelItem(
            CCS_CookingRecipe recipe,
            out CCS_ItemDefinition fuelItem,
            out int fuelAmount)
        {
            fuelItem = null;
            fuelAmount = recipe != null ? recipe.RequiredFuelAmount : 0;
            if (recipe == null || fuelAmount <= 0)
            {
                return false;
            }

            IReadOnlyList<string> acceptedFuelIds = recipe.AcceptedFuelItemIds;
            if (acceptedFuelIds == null || acceptedFuelIds.Count == 0)
            {
                return false;
            }

            for (int index = 0; index < acceptedFuelIds.Count; index++)
            {
                string fuelId = acceptedFuelIds[index];
                if (!activeProfile.TryResolveItemDefinition(fuelId, out CCS_ItemDefinition candidate)
                    || candidate == null)
                {
                    continue;
                }

                if (inventoryService.GetQuantity(candidate) >= fuelAmount)
                {
                    fuelItem = candidate;
                    return true;
                }
            }

            for (int index = 0; index < acceptedFuelIds.Count; index++)
            {
                if (activeProfile.TryResolveItemDefinition(acceptedFuelIds[index], out CCS_ItemDefinition candidate)
                    && candidate != null)
                {
                    fuelItem = candidate;
                    return true;
                }
            }

            return fuelItem != null;
        }

        private bool TryConsumeRecipeIngredients(CCS_CookingRecipe recipe, out string failureMessage)
        {
            failureMessage = string.Empty;
            if (!activeProfile.TryResolveItemDefinition(recipe.RawItemDefinitionId, out CCS_ItemDefinition rawItem)
                || !activeProfile.TryResolveItemDefinition(recipe.CookedItemDefinitionId, out CCS_ItemDefinition cookedItem))
            {
                failureMessage = "Cooking recipe items could not be resolved.";
                return false;
            }

            if (!TryResolveFuelItem(recipe, out CCS_ItemDefinition fuelItem, out int fuelAmount))
            {
                failureMessage = "Missing stick or wood fuel.";
                return false;
            }

            if (inventoryService.RemoveItem(rawItem, recipe.RawAmount) < recipe.RawAmount)
            {
                failureMessage = "Failed to consume raw ingredients.";
                return false;
            }

            if (inventoryService.RemoveItem(fuelItem, fuelAmount) < fuelAmount)
            {
                inventoryService.AddItem(rawItem, recipe.RawAmount);
                failureMessage = "Failed to consume fuel.";
                return false;
            }

            if (!inventoryService.CanAdd(cookedItem, recipe.CookedAmount))
            {
                inventoryService.AddItem(rawItem, recipe.RawAmount);
                inventoryService.AddItem(fuelItem, fuelAmount);
                failureMessage = "Inventory cannot hold cooked food.";
                return false;
            }

            return true;
        }

        private void CompleteCookJob(ActiveCookJob job)
        {
            if (job == null)
            {
                return;
            }

            if (job.LegacyRequest != null)
            {
                CompleteLegacyCookJob(job);
                return;
            }

            if (job.Recipe == null)
            {
                return;
            }

            if (!activeProfile.TryResolveItemDefinition(job.Recipe.CookedItemDefinitionId, out CCS_ItemDefinition cookedItem))
            {
                FailCooking(job.Station, job.Recipe, "Cooked item could not be resolved.");
                job.Station?.CompleteCooking();
                return;
            }

            int added = inventoryService.AddItem(cookedItem, job.Recipe.CookedAmount);
            if (added < job.Recipe.CookedAmount)
            {
                FailCooking(job.Station, job.Recipe, "Failed to grant cooked food.");
                job.Station?.CompleteCooking();
                return;
            }

            job.Station?.CompleteCooking();
            RaiseCookingCompleted(job.Station, job.Recipe, "Cooking completed.");
        }

        private void CompleteLegacyCookJob(ActiveCookJob job)
        {
            CCS_CookingRequest request = job.LegacyRequest;
            if (request == null)
            {
                return;
            }

            if (inventoryService == null || !inventoryService.IsInitialized)
            {
                FailCooking(null, null, "Inventory service is not initialized.");
                campfireService?.SetCampfireState(request.CampfireInstanceKey, CCS_CampfireState.Lit);
                return;
            }

            int added = inventoryService.AddItem(request.OutputItemDefinition, 1);
            if (added < 1)
            {
                FailCooking(null, null, "Failed to grant cooked meat.");
                campfireService?.SetCampfireState(request.CampfireInstanceKey, CCS_CampfireState.Lit);
                return;
            }

            campfireService?.SetCampfireState(request.CampfireInstanceKey, CCS_CampfireState.Lit);
            RaiseCookingCompleted(
                null,
                null,
                $"Cooked {request.OutputItemDefinition.DisplayName}.",
                request.OutputItemDefinition,
                request.CampfireDefinition,
                request.CampfireInstanceKey);
        }

        private CCS_CookingResult TryStartLegacyRequest(CCS_CookingRequest request)
        {
            if (!CanCook(request))
            {
                return FailCooking(null, null, "Cooking request is invalid.");
            }

            if (inventoryService.RemoveItem(request.InputItemDefinition, 1) < 1)
            {
                return FailCooking(null, null, "Failed to consume raw meat.");
            }

            float cookTimeSeconds = request.CookTimeSeconds > 0f
                ? request.CookTimeSeconds
                : activeProfile.DefaultCookDurationSeconds;

            campfireService?.SetCampfireState(request.CampfireInstanceKey, CCS_CampfireState.Cooking);
            activeCookJobs.Add(new ActiveCookJob(null, null, cookTimeSeconds, request));
            RaiseCookingStarted(
                null,
                null,
                "Cooking started.",
                request.InputItemDefinition,
                request.CampfireDefinition,
                request.CampfireInstanceKey);
            return CCS_CookingResult.Success("Cooking started.");
        }

        private CCS_CookingStation FindStationForCampfireKey(string campfireInstanceKey)
        {
            if (string.IsNullOrWhiteSpace(campfireInstanceKey))
            {
                return null;
            }

            foreach (CCS_CookingStation station in registeredStations)
            {
                if (station != null && station.name.Contains(campfireInstanceKey))
                {
                    return station;
                }
            }

            return null;
        }

        private CCS_CookingResult FailCooking(
            CCS_CookingStation station,
            CCS_CookingRecipe recipe,
            string message)
        {
            RaiseCookingFailed(station, recipe, message);
            return CCS_CookingResult.Failure(message);
        }

        private void RaiseCookingStarted(
            CCS_CookingStation station,
            CCS_CookingRecipe recipe,
            string message,
            CCS_ItemDefinition itemDefinition = null,
            CCS_CampfireDefinition campfireDefinition = null,
            string campfireInstanceKey = "")
        {
            CookingStarted?.Invoke(
                BuildEventArgs(station, recipe, message, itemDefinition, campfireDefinition, campfireInstanceKey));
        }

        private void RaiseCookingCompleted(
            CCS_CookingStation station,
            CCS_CookingRecipe recipe,
            string message,
            CCS_ItemDefinition itemDefinition = null,
            CCS_CampfireDefinition campfireDefinition = null,
            string campfireInstanceKey = "")
        {
            CookingCompleted?.Invoke(
                BuildEventArgs(station, recipe, message, itemDefinition, campfireDefinition, campfireInstanceKey));
        }

        private void RaiseCookingFailed(CCS_CookingStation station, CCS_CookingRecipe recipe, string message)
        {
            CookingFailed?.Invoke(BuildEventArgs(station, recipe, message));
        }

        private void RaiseCookingCancelled(CCS_CookingStation station, CCS_CookingRecipe recipe, string message)
        {
            CookingCancelled?.Invoke(BuildEventArgs(station, recipe, message));
        }

        private static CCS_CookingEventArgs BuildEventArgs(
            CCS_CookingStation station,
            CCS_CookingRecipe recipe,
            string message,
            CCS_ItemDefinition itemDefinition = null,
            CCS_CampfireDefinition campfireDefinition = null,
            string campfireInstanceKey = "")
        {
            if (station == null && recipe == null)
            {
                return new CCS_CookingEventArgs(
                    campfireDefinition,
                    itemDefinition,
                    campfireInstanceKey,
                    message);
            }

            return new CCS_CookingEventArgs(
                station,
                station != null ? station.StationType : CCS_CookingStationType.Campfire,
                recipe?.RecipeId ?? string.Empty,
                recipe?.RawItemDefinitionId ?? string.Empty,
                recipe?.CookedItemDefinitionId ?? string.Empty,
                station != null ? station.WorldPosition : Vector3.zero,
                message,
                itemDefinition,
                campfireDefinition,
                campfireInstanceKey);
        }

        #endregion
    }
}
