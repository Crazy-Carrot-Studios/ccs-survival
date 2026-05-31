using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_BuildingPlacementService
// CATEGORY: Modules / Building / Runtime / Services
// PURPOSE: Build mode orchestration with preview updates and piece placement.
// PLACEMENT: Registered as CCS_ISurvivalService by survival gameplay composition wiring.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: No inventory consumption, snapping, or durability in 0.8.1.
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
        private int nextInstanceSequence;
        private bool isInitialized;

        #endregion

        #region Events

        public event PlacementStartedHandler PlacementStarted;
        public event PlacementCancelledHandler PlacementCancelled;
        public event BuildingPlacedHandler BuildingPlaced;

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

        public bool PlaceCurrentPiece()
        {
            if (!EnsureReady()
                || !placementState.IsPlacementModeActive
                || !placementState.IsPlacementValid
                || string.IsNullOrWhiteSpace(placementState.ActivePieceId))
            {
                return false;
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
                return false;
            }

            BuildingPlaced?.Invoke(
                CreateEventArgs(instance, $"Placed building piece '{placementState.ActivePieceId}'."));
            return true;
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
