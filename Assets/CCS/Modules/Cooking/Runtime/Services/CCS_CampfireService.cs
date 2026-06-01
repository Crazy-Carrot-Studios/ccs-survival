using System.Collections.Generic;
using CCS.Modules.Building;
using CCS.Modules.Inventory;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CampfireService
// CATEGORY: Modules / Cooking / Runtime / Services
// PURPOSE: Tracks campfire state, placement from kits, and cooking orchestration hooks.
// PLACEMENT: Registered by CCS_SurvivalGameplayServiceRegistration from cooking profile.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: No fuel system in 0.9.4 foundation.
// =============================================================================

namespace CCS.Modules.Cooking
{
    public sealed class CCS_CampfireService : CCS_ISurvivalService
    {
        private const string LogPrefix = "[CCS_CampfireService]";

        #region Variables

        private readonly Dictionary<string, CCS_CampfireInstanceState> campfiresByInstanceKey =
            new Dictionary<string, CCS_CampfireInstanceState>();

        private CCS_CookingProfile activeProfile;
        private CCS_BuildingService buildingService;
        private CCS_BuildingPlacementService placementService;
        private CCS_CookingService cookingService;
        private CCS_PlayerInventoryService inventoryService;
        private bool isInitialized;

        #endregion

        #region Events

        public event CampfireLitHandler CampfireLit;

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

        public void InitializeFromProfile(CCS_CookingProfile profile)
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
            isInitialized = true;
        }

        public void BindBuildingService(CCS_BuildingService service)
        {
            buildingService = service;
        }

        public void BindPlacementService(CCS_BuildingPlacementService service)
        {
            if (placementService != null)
            {
                placementService.BuildingPlaced -= HandleBuildingPlaced;
            }

            placementService = service;

            if (placementService != null)
            {
                placementService.BuildingPlaced += HandleBuildingPlaced;
            }
        }

        public void BindCookingService(CCS_CookingService service)
        {
            cookingService = service;
        }

        public void BindInventoryService(CCS_PlayerInventoryService service)
        {
            inventoryService = service;
        }

        public bool RegisterCampfire(
            string instanceKey,
            CCS_CampfireDefinition campfireDefinition,
            bool autoLightOverride = false)
        {
            if (!EnsureInitialized() || string.IsNullOrWhiteSpace(instanceKey) || campfireDefinition == null)
            {
                return false;
            }

            bool shouldAutoLight = autoLightOverride
                || (activeProfile != null && activeProfile.AutoLightCampfiresOnPlacement);

            CCS_CampfireState initialState = shouldAutoLight
                ? CCS_CampfireState.Lit
                : CCS_CampfireState.Unlit;

            campfiresByInstanceKey[instanceKey] =
                new CCS_CampfireInstanceState(campfireDefinition, instanceKey, initialState);

            if (initialState == CCS_CampfireState.Lit)
            {
                RaiseCampfireLit(campfireDefinition, instanceKey);
            }

            return true;
        }

        public bool TryGetCampfireState(string instanceKey, out CCS_CampfireState campfireState)
        {
            campfireState = CCS_CampfireState.Unlit;

            if (string.IsNullOrWhiteSpace(instanceKey)
                || !campfiresByInstanceKey.TryGetValue(instanceKey, out CCS_CampfireInstanceState instanceState)
                || instanceState == null)
            {
                return false;
            }

            campfireState = instanceState.CampfireState;
            return true;
        }

        public void SetCampfireState(string instanceKey, CCS_CampfireState campfireState)
        {
            if (string.IsNullOrWhiteSpace(instanceKey)
                || !campfiresByInstanceKey.TryGetValue(instanceKey, out CCS_CampfireInstanceState instanceState)
                || instanceState == null)
            {
                return;
            }

            instanceState.SetState(campfireState);
        }

        public bool TryLightCampfire(string instanceKey)
        {
            if (!EnsureInitialized()
                || string.IsNullOrWhiteSpace(instanceKey)
                || !campfiresByInstanceKey.TryGetValue(instanceKey, out CCS_CampfireInstanceState instanceState)
                || instanceState == null)
            {
                return false;
            }

            if (instanceState.CampfireState == CCS_CampfireState.Cooking)
            {
                return false;
            }

            if (instanceState.CampfireState == CCS_CampfireState.Lit)
            {
                return true;
            }

            instanceState.SetState(CCS_CampfireState.Lit);
            RaiseCampfireLit(instanceState.CampfireDefinition, instanceKey);
            return true;
        }

        public CCS_CookingResult TryCookMeatAtCampfire(string instanceKey)
        {
            if (!EnsureInitialized()
                || cookingService == null
                || !cookingService.IsInitialized
                || activeProfile == null)
            {
                return CCS_CookingResult.Failure("Campfire cooking is not ready.");
            }

            if (string.IsNullOrWhiteSpace(instanceKey)
                || !campfiresByInstanceKey.TryGetValue(instanceKey, out CCS_CampfireInstanceState instanceState)
                || instanceState == null)
            {
                return CCS_CookingResult.Failure("Campfire instance was not found.");
            }

            if (instanceState.CampfireState != CCS_CampfireState.Lit)
            {
                return CCS_CookingResult.Failure("Campfire must be lit before cooking.");
            }

            CCS_CampfireDefinition campfireDefinition = instanceState.CampfireDefinition;
            float cookTimeSeconds = campfireDefinition != null
                ? campfireDefinition.CookTimeSeconds
                : activeProfile.DefaultCookTimeSeconds;

            CCS_CookingRequest request = new CCS_CookingRequest(
                campfireDefinition ?? activeProfile.DefaultCampfireDefinition,
                activeProfile.RawMeatItemDefinition,
                activeProfile.CookedMeatItemDefinition,
                instanceKey,
                cookTimeSeconds);

            return cookingService.TryStartCooking(request);
        }

        public bool TryPlaceCampfireFromKit(Vector3 position, Quaternion rotation)
        {
            if (!EnsureInitialized()
                || placementService == null
                || !placementService.IsInitialized
                || activeProfile?.CampfireBuildingPiece == null)
            {
                return false;
            }

            CCS_BuildingPieceDefinition campfirePiece = activeProfile.CampfireBuildingPiece;
            if (!placementService.SetActiveDefinition(campfirePiece))
            {
                return false;
            }

            placementService.UpdatePreview(position, rotation);
            return placementService.TryPlaceCurrentPiece().Success;
        }

        public CCS_CampfireSnapshot GetSnapshot(string instanceKey)
        {
            if (string.IsNullOrWhiteSpace(instanceKey)
                || !campfiresByInstanceKey.TryGetValue(instanceKey, out CCS_CampfireInstanceState instanceState)
                || instanceState == null)
            {
                return new CCS_CampfireSnapshot(null, CCS_CampfireState.Unlit, instanceKey);
            }

            return new CCS_CampfireSnapshot(
                instanceState.CampfireDefinition,
                instanceState.CampfireState,
                instanceState.InstanceKey);
        }

        #endregion

        #region Private Methods

        private bool EnsureInitialized()
        {
            return isInitialized;
        }

        private void HandleBuildingPlaced(CCS_BuildingPlacementEventArgs eventArgs)
        {
            if (activeProfile?.CampfireBuildingPiece == null || eventArgs?.PlacedInstance == null)
            {
                return;
            }

            CCS_BuildingInstance placedInstance = eventArgs.PlacedInstance;
            if (!string.Equals(
                    placedInstance.PieceId,
                    activeProfile.CampfireBuildingPiece.PieceId,
                    System.StringComparison.Ordinal))
            {
                return;
            }

            RegisterCampfire(
                placedInstance.InstanceId,
                activeProfile.DefaultCampfireDefinition,
                autoLightOverride: activeProfile.AutoLightCampfiresOnPlacement);

            SpawnCampfireInteractable(placedInstance);
        }

        private void SpawnCampfireInteractable(CCS_BuildingInstance placedInstance)
        {
            if (placedInstance == null || activeProfile?.DefaultCampfireDefinition == null)
            {
                return;
            }

            GameObject campfireObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            campfireObject.name = $"CCS_Campfire_{placedInstance.InstanceId}";
            campfireObject.transform.SetPositionAndRotation(placedInstance.Position, placedInstance.Rotation);
            campfireObject.transform.localScale = new Vector3(0.8f, 0.25f, 0.8f);

            Collider collider = campfireObject.GetComponent<Collider>();
            if (collider != null)
            {
                collider.isTrigger = false;
                collider.enabled = true;
            }

            CCS_CampfireInteractable interactable = campfireObject.AddComponent<CCS_CampfireInteractable>();
            interactable.ConfigureRuntime(
                activeProfile.DefaultCampfireDefinition,
                activeProfile,
                placedInstance.InstanceId,
                assumeLitOnStartOverride: activeProfile.AutoLightCampfiresOnPlacement);
        }

        private void RaiseCampfireLit(CCS_CampfireDefinition campfireDefinition, string instanceKey)
        {
            CampfireLit?.Invoke(
                new CCS_CookingEventArgs(
                    campfireDefinition,
                    message: "Campfire lit.",
                    campfireInstanceKey: instanceKey));
        }

        #endregion
    }
}
