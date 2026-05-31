using CCS.Modules.Inventory;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_BuildingPlacementService
// CATEGORY: Modules / Building / Runtime / Services
// PURPOSE: Build mode orchestration with preview updates and piece placement.
// PLACEMENT: Registered as CCS_ISurvivalService by survival gameplay composition wiring.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Inventory cost validation and consumption added in 0.8.2.
// =============================================================================

namespace CCS.Modules.Building
{
    public sealed class CCS_BuildingPlacementService : CCS_ISurvivalService
    {
        private const string LogPrefix = "[CCS_BuildingPlacementService]";

        #region Variables

        private readonly CCS_BuildingPlacementState placementState = new CCS_BuildingPlacementState();

        private CCS_BuildingProfile activeProfile;
        private CCS_BuildingService buildingService;
        private CCS_PlayerInventoryService inventoryService;
        private int nextInstanceSequence;
        private bool isInitialized;

        #endregion

        #region Events

        public event PlacementStartedHandler PlacementStarted;
        public event PlacementCancelledHandler PlacementCancelled;
        public event BuildingPlacedHandler BuildingPlaced;
        public event PlacementFailedHandler PlacementFailed;

        #endregion

        #region Properties

        public bool IsInitialized => isInitialized;

        public bool IsPlacementModeActive => placementState.IsPlacementModeActive;

        #endregion

        #region Public Methods

        public void Initialize()
        {
            if (isInitialized)
            {
                return;
            }

            // Profile binding via InitializeFromProfile sets isInitialized when ready.
        }

        public void InitializeFromProfile(CCS_BuildingProfile profile)
        {
            if (profile == null)
            {
                Debug.LogWarning($"{LogPrefix} InitializeFromProfile called with null profile.");
                isInitialized = false;
                return;
            }

            if (!profile.AllowPlacement)
            {
                Debug.LogWarning($"{LogPrefix} Profile has placement disabled.");
            }

            activeProfile = profile;
            placementState.Clear();
            isInitialized = true;
        }

        public void BindBuildingService(CCS_BuildingService service)
        {
            buildingService = service;
        }

        public void BindInventoryService(CCS_PlayerInventoryService service)
        {
            inventoryService = service;
        }

        public bool EnterPlacementMode(string message = null)
        {
            if (!EnsureReady())
            {
                return false;
            }

            if (activeProfile != null && !activeProfile.AllowPlacement)
            {
                Debug.LogWarning($"{LogPrefix} EnterPlacementMode rejected because profile disables placement.");
                return false;
            }

            placementState.IsPlacementModeActive = true;
            PlacementStarted?.Invoke(CreateEventArgs(null, message ?? "Placement mode entered."));
            return true;
        }

        public bool ExitPlacementMode(string message = null)
        {
            if (!EnsureReady())
            {
                return false;
            }

            if (!placementState.IsPlacementModeActive)
            {
                return false;
            }

            placementState.Clear();
            PlacementCancelled?.Invoke(CreateEventArgs(null, message ?? "Placement mode exited."));
            return true;
        }

        public bool SetActiveDefinition(CCS_BuildingPieceDefinition definition)
        {
            if (!EnsureReady() || definition == null)
            {
                return false;
            }

            if (!buildingService.TryGetDefinition(definition.PieceId, out _))
            {
                Debug.LogWarning($"{LogPrefix} SetActiveDefinition rejected unknown piece '{definition.PieceId}'.");
                return false;
            }

            if (!placementState.IsPlacementModeActive)
            {
                EnterPlacementMode($"Placement mode entered for '{definition.DisplayName}'.");
            }

            placementState.ActivatePlacement(definition.PieceId, definition.BuildingPieceType);
            PlacementStarted?.Invoke(
                CreateEventArgs(null, $"Active definition set to '{definition.DisplayName}'."));
            return true;
        }

        public bool SetActiveDefinitionByPieceId(string pieceId)
        {
            if (!EnsureReady() || !buildingService.TryGetDefinition(pieceId, out CCS_BuildingPieceDefinition definition))
            {
                return false;
            }

            return SetActiveDefinition(definition);
        }

        public bool UpdatePreview(Vector3 position, Quaternion rotation)
        {
            if (!EnsureReady() || !placementState.IsPlacementModeActive)
            {
                return false;
            }

            bool isValid = ValidatePreviewPosition(position);
            placementState.UpdatePreview(position, rotation, isValid);
            return isValid;
        }

        public CCS_BuildingPlacementValidationResult TryPlaceCurrentPiece()
        {
            CCS_BuildingPlacementValidationResult validation =
                CCS_BuildingPlacementValidationUtility.ValidatePlacementAttempt(
                    this,
                    buildingService,
                    inventoryService,
                    placementState);

            if (!validation.Success)
            {
                RaisePlacementFailed(validation);
                return validation;
            }

            if (!buildingService.TryGetDefinition(
                    placementState.ActivePieceId,
                    out CCS_BuildingPieceDefinition definition))
            {
                validation = CCS_BuildingPlacementValidationResult.Failed("Building definition was not found.");
                RaisePlacementFailed(validation);
                return validation;
            }

            if (!CCS_BuildingPlacementValidationUtility.TryConsumeBuildCosts(
                    inventoryService,
                    definition,
                    out CCS_BuildingPlacementValidationResult consumeResult))
            {
                RaisePlacementFailed(consumeResult);
                return consumeResult;
            }

            string instanceId = GenerateInstanceId();
            CCS_BuildingInstance instance = new CCS_BuildingInstance(
                instanceId,
                placementState.ActivePieceId,
                placementState.PreviewPosition,
                placementState.PreviewRotation,
                Time.time);

            if (!buildingService.TryAddPlacedInstance(instance))
            {
                CCS_BuildingPlacementValidationUtility.RestoreBuildCosts(inventoryService, definition);
                validation = CCS_BuildingPlacementValidationResult.Failed("Failed to register placed building instance.");
                RaisePlacementFailed(validation);
                return validation;
            }

            BuildingPlaced?.Invoke(
                CreateEventArgs(instance, $"Placed building piece '{placementState.ActivePieceId}'."));
            return CCS_BuildingPlacementValidationResult.Passed;
        }

        public bool PlaceCurrentPiece()
        {
            return TryPlaceCurrentPiece().Success;
        }

        public CCS_BuildingPlacementSnapshot GetSnapshot()
        {
            if (!EnsureReady())
            {
                return CCS_BuildingPlacementSnapshot.Empty;
            }

            return placementState.CreateSnapshot();
        }

        #endregion

        #region Private Methods

        private bool EnsureReady()
        {
            if (isInitialized && activeProfile != null && buildingService != null && buildingService.IsInitialized)
            {
                return true;
            }

            Debug.LogWarning($"{LogPrefix} Service is not ready.");
            return false;
        }

        private static bool ValidatePreviewPosition(Vector3 position)
        {
            return !float.IsNaN(position.x)
                && !float.IsNaN(position.y)
                && !float.IsNaN(position.z);
        }

        private string GenerateInstanceId()
        {
            nextInstanceSequence++;
            return $"ccs.survival.building.instance.{nextInstanceSequence}";
        }

        private void RaisePlacementFailed(CCS_BuildingPlacementValidationResult validation)
        {
            PlacementFailed?.Invoke(
                new CCS_BuildingPlacementFailedEventArgs(placementState.CreateSnapshot(), validation));
        }

        private CCS_BuildingPlacementEventArgs CreateEventArgs(
            CCS_BuildingInstance placedInstance,
            string message)
        {
            int placedCount = buildingService != null ? buildingService.PlacedInstanceCount : 0;
            return new CCS_BuildingPlacementEventArgs(
                placementState.CreateSnapshot(),
                placedCount,
                placedInstance,
                message);
        }

        #endregion
    }
}
