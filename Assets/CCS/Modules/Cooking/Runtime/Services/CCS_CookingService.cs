using System.Collections.Generic;
using CCS.Core;
using CCS.Modules.Inventory;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CookingService
// CATEGORY: Modules / Cooking / Runtime / Services
// PURPOSE: Validates ingredients, queues cooking jobs, and grants cooked outputs.
// PLACEMENT: Registered by CCS_SurvivalGameplayServiceRegistration from cooking profile.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: FirePit station classification without world station UI. No fuel systems.
// =============================================================================

namespace CCS.Modules.Cooking
{
    public sealed class CCS_CookingService : CCS_ISurvivalService, CCS_IUpdatable
    {
        private const string LogPrefix = "[CCS_CookingService]";

        private sealed class ActiveCookJob
        {
            public ActiveCookJob(CCS_CookingRequest request, float remainingSeconds)
            {
                Request = request;
                RemainingSeconds = remainingSeconds;
            }

            public CCS_CookingRequest Request { get; }

            public float RemainingSeconds { get; set; }
        }

        #region Variables

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

        public bool CanCook(CCS_CookingRequest request)
        {
            return ValidateCookingRequest(request).IsSuccess;
        }

        public CCS_CookingResult TryStartCooking(CCS_CookingRequest request)
        {
            if (!EnsureInitialized())
            {
                return FailCooking(request, "Cooking service is not initialized.");
            }

            if (activeProfile != null && !activeProfile.EnableCooking)
            {
                return FailCooking(request, "Cooking is disabled.");
            }

            CCS_SurvivalValidationResult validation = ValidateCookingRequest(request);
            if (!validation.IsSuccess)
            {
                return FailCooking(request, validation.Message);
            }

            if (inventoryService == null || !inventoryService.IsInitialized)
            {
                return FailCooking(request, "Inventory service is not initialized.");
            }

            CCS_ItemDefinition inputItem = request.InputItemDefinition;
            CCS_ItemDefinition outputItem = request.OutputItemDefinition;

            if (inventoryService.RemoveItem(inputItem, 1) < 1)
            {
                return FailCooking(request, "Failed to consume raw meat.");
            }

            if (!inventoryService.CanAdd(outputItem, 1))
            {
                inventoryService.AddItem(inputItem, 1);
                return FailCooking(request, "Inventory cannot hold cooked meat.");
            }

            float cookTimeSeconds = request.CookTimeSeconds;
            if (cookTimeSeconds <= 0f)
            {
                cookTimeSeconds = activeProfile != null ? activeProfile.DefaultCookTimeSeconds : 5f;
            }

            campfireService?.SetCampfireState(request.CampfireInstanceKey, CCS_CampfireState.Cooking);

            ActiveCookJob job = new ActiveCookJob(request, cookTimeSeconds);
            activeCookJobs.Add(job);

            RaiseCookingStarted(request);
            return CCS_CookingResult.Success("Cooking started.");
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

        #endregion

        #region Private Methods

        private bool EnsureInitialized()
        {
            return isInitialized;
        }

        private CCS_SurvivalValidationResult ValidateCookingRequest(CCS_CookingRequest request)
        {
            if (request == null)
            {
                return CCS_SurvivalValidationResult.Fail("Cooking request is null.");
            }

            CCS_SurvivalValidationResult campfireValidation =
                CCS_CookingValidationUtility.ValidateCampfireDefinition(request.CampfireDefinition);

            if (!campfireValidation.IsSuccess)
            {
                return campfireValidation;
            }

            if (request.InputItemDefinition == null)
            {
                return CCS_SurvivalValidationResult.Fail("Cooking input item is null.");
            }

            if (request.OutputItemDefinition == null)
            {
                return CCS_SurvivalValidationResult.Fail("Cooking output item is null.");
            }

            if (string.IsNullOrWhiteSpace(request.CampfireInstanceKey))
            {
                return CCS_SurvivalValidationResult.Fail("Campfire instance key is required.");
            }

            if (inventoryService == null || !inventoryService.IsInitialized)
            {
                return CCS_SurvivalValidationResult.Fail("Inventory service is not initialized.");
            }

            if (inventoryService.GetQuantity(request.InputItemDefinition) <= 0)
            {
                return CCS_SurvivalValidationResult.Fail("Required raw meat is missing.");
            }

            if (!inventoryService.CanAdd(request.OutputItemDefinition, 1))
            {
                return CCS_SurvivalValidationResult.Fail("Inventory cannot hold cooked meat.");
            }

            if (campfireService != null
                && campfireService.TryGetCampfireState(request.CampfireInstanceKey, out CCS_CampfireState campfireState)
                && campfireState != CCS_CampfireState.Lit)
            {
                return CCS_SurvivalValidationResult.Fail("Campfire must be lit before cooking.");
            }

            return CCS_SurvivalValidationResult.Pass("Cooking request validated.");
        }

        private void CompleteCookJob(ActiveCookJob job)
        {
            CCS_CookingRequest request = job.Request;

            if (inventoryService == null || !inventoryService.IsInitialized)
            {
                FailCooking(request, "Inventory service is not initialized.");
                campfireService?.SetCampfireState(request.CampfireInstanceKey, CCS_CampfireState.Lit);
                return;
            }

            int added = inventoryService.AddItem(request.OutputItemDefinition, 1);
            if (added < 1)
            {
                FailCooking(request, "Failed to grant cooked meat.");
                campfireService?.SetCampfireState(request.CampfireInstanceKey, CCS_CampfireState.Lit);
                return;
            }

            campfireService?.SetCampfireState(request.CampfireInstanceKey, CCS_CampfireState.Lit);
            RaiseCookingCompleted(request);
        }

        private CCS_CookingResult FailCooking(CCS_CookingRequest request, string message)
        {
            CCS_CookingResult failure = CCS_CookingResult.Failure(message);
            RaiseCookingFailed(request, message);
            return failure;
        }

        private void RaiseCookingStarted(CCS_CookingRequest request)
        {
            CookingStarted?.Invoke(
                new CCS_CookingEventArgs(
                    request?.CampfireDefinition,
                    request?.InputItemDefinition,
                    request?.CampfireInstanceKey ?? string.Empty,
                    "Cooking started."));
        }

        private void RaiseCookingCompleted(CCS_CookingRequest request)
        {
            CookingCompleted?.Invoke(
                new CCS_CookingEventArgs(
                    request?.CampfireDefinition,
                    request?.OutputItemDefinition,
                    request?.CampfireInstanceKey ?? string.Empty,
                    "Cooking completed."));
        }

        private void RaiseCookingFailed(CCS_CookingRequest request, string message)
        {
            CookingFailed?.Invoke(
                new CCS_CookingEventArgs(
                    request?.CampfireDefinition,
                    request?.InputItemDefinition,
                    request?.CampfireInstanceKey ?? string.Empty,
                    message));
        }

        #endregion
    }
}
