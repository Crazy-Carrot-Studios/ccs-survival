using System.Collections.Generic;
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
// NOTES: Inventory costs in 0.8.2. Basic snap matching and occupancy in 0.8.3.
// =============================================================================

namespace CCS.Modules.Building
{
    public sealed class CCS_BuildingPlacementService : CCS_ISurvivalService
    {
        private const string LogPrefix = "[CCS_BuildingPlacementService]";
        private const float DefaultSnapSearchRadius = 8f;

        #region Variables

        private readonly CCS_BuildingPlacementState placementState = new CCS_BuildingPlacementState();

        private CCS_BuildingProfile activeProfile;
        private CCS_BuildingService buildingService;
        private CCS_BuildingRecipeService recipeService;
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

        public void BindRecipeService(CCS_BuildingRecipeService service)
        {
            recipeService = service;
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
            return UpdatePreviewWithSnap(position, rotation);
        }

        public bool FindBestSnapMatch(Vector3 hintPosition, out CCS_BuildingSnapMatch snapMatch)
        {
            snapMatch = CCS_BuildingSnapMatch.Empty;

            if (!EnsureReady()
                || !placementState.IsPlacementModeActive
                || !buildingService.TryGetDefinition(placementState.ActivePieceId, out CCS_BuildingPieceDefinition definition))
            {
                return false;
            }

            IReadOnlyList<CCS_BuildingSnapPoint> sourceSnapPoints = definition.SnapPoints;
            if (sourceSnapPoints == null || sourceSnapPoints.Count == 0)
            {
                return false;
            }

            IReadOnlyList<CCS_BuildingInstance> placedInstances = buildingService.GetPlacedInstances();
            if (placedInstances == null || placedInstances.Count == 0)
            {
                return false;
            }

            float bestDistance = float.MaxValue;
            CCS_BuildingSnapMatch bestMatch = CCS_BuildingSnapMatch.Empty;

            for (int instanceIndex = 0; instanceIndex < placedInstances.Count; instanceIndex++)
            {
                CCS_BuildingInstance placedInstance = placedInstances[instanceIndex];
                IReadOnlyList<CCS_BuildingRuntimeSnapPoint> targetSnapPoints = placedInstance.RuntimeSnapPoints;

                for (int targetIndex = 0; targetIndex < targetSnapPoints.Count; targetIndex++)
                {
                    CCS_BuildingRuntimeSnapPoint targetSnapPoint = targetSnapPoints[targetIndex];
                    if (targetSnapPoint.IsOccupied)
                    {
                        continue;
                    }

                    for (int sourceIndex = 0; sourceIndex < sourceSnapPoints.Count; sourceIndex++)
                    {
                        CCS_BuildingSnapPoint sourceSnapPoint = sourceSnapPoints[sourceIndex];
                        if (sourceSnapPoint == null)
                        {
                            continue;
                        }

                        if (!CCS_BuildingSnapCompatibilityUtility.CanSnap(
                                targetSnapPoint.SnapPointType,
                                sourceSnapPoint.SnapPointType,
                                sourceSnapPoint.CompatibleTargetTypes))
                        {
                            continue;
                        }

                        ComputeSnappedTransform(
                            targetSnapPoint.WorldPosition,
                            targetSnapPoint.WorldRotation,
                            sourceSnapPoint.LocalPosition,
                            sourceSnapPoint.LocalRotation,
                            out Vector3 snappedPosition,
                            out Quaternion snappedRotation);

                        float distance = Vector3.Distance(hintPosition, snappedPosition);
                        if (distance > DefaultSnapSearchRadius || distance >= bestDistance)
                        {
                            continue;
                        }

                        bestDistance = distance;
                        bestMatch = new CCS_BuildingSnapMatch(
                            true,
                            placedInstance.InstanceId,
                            targetSnapPoint.SnapPointId,
                            targetSnapPoint.SnapPointType,
                            sourceSnapPoint.SnapPointId,
                            sourceSnapPoint.SnapPointType,
                            snappedPosition,
                            snappedRotation);
                    }
                }
            }

            if (!bestMatch.HasMatch)
            {
                return false;
            }

            snapMatch = bestMatch;
            return true;
        }

        public bool UpdatePreviewWithSnap(Vector3 hintPosition, Quaternion hintRotation)
        {
            if (!EnsureReady() || !placementState.IsPlacementModeActive)
            {
                return false;
            }

            if (!buildingService.TryGetDefinition(placementState.ActivePieceId, out CCS_BuildingPieceDefinition definition))
            {
                return false;
            }

            bool hasSnapMatch = FindBestSnapMatch(hintPosition, out CCS_BuildingSnapMatch snapMatch);

            if (definition.RequiresSnapPoint)
            {
                if (!hasSnapMatch)
                {
                    placementState.UpdatePreviewWithSnap(hintPosition, hintRotation, false, CCS_BuildingSnapMatch.Empty, false);
                    return false;
                }

                placementState.UpdatePreviewWithSnap(
                    snapMatch.SnappedPosition,
                    snapMatch.SnappedRotation,
                    true,
                    snapMatch,
                    true);
                return true;
            }

            if (hasSnapMatch)
            {
                placementState.UpdatePreviewWithSnap(
                    snapMatch.SnappedPosition,
                    snapMatch.SnappedRotation,
                    ValidatePreviewPosition(snapMatch.SnappedPosition),
                    snapMatch,
                    true);
                return placementState.IsPlacementValid;
            }

            if (definition.AllowsFreePlacement)
            {
                bool isValid = ValidatePreviewPosition(hintPosition);
                placementState.UpdatePreviewWithSnap(hintPosition, hintRotation, isValid, CCS_BuildingSnapMatch.Empty, false);
                return isValid;
            }

            placementState.UpdatePreviewWithSnap(hintPosition, hintRotation, false, CCS_BuildingSnapMatch.Empty, false);
            return false;
        }

        public CCS_BuildingPlacementValidationResult TryPlaceCurrentPiece()
        {
            return PlaceCurrentPieceUsingSnap();
        }

        public CCS_BuildingPlacementValidationResult PlaceCurrentPieceUsingSnap()
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

            CCS_BuildingRecipe activeRecipe = null;
            if (recipeService != null
                && recipeService.IsInitialized
                && recipeService.ProgressionEnabled)
            {
                if (!recipeService.TryAuthorizePlacement(
                        definition,
                        placementState,
                        out activeRecipe,
                        out string authorizationMessage))
                {
                    validation = CCS_BuildingPlacementValidationResult.Failed(authorizationMessage);
                    RaisePlacementFailed(validation);
                    return validation;
                }

                if (!recipeService.TryConsumeRecipeCosts(activeRecipe, out string consumeMessage))
                {
                    validation = CCS_BuildingPlacementValidationResult.Failed(consumeMessage);
                    RaisePlacementFailed(validation);
                    return validation;
                }
            }
            else if (!CCS_BuildingPlacementValidationUtility.TryConsumeBuildCosts(
                    inventoryService,
                    definition,
                    out CCS_BuildingPlacementValidationResult consumeResult))
            {
                RaisePlacementFailed(consumeResult);
                return consumeResult;
            }

            CCS_BuildingSnapMatch consumedSnapMatch = placementState.ActiveSnapMatch;
            string instanceId = GenerateInstanceId();
            CCS_BuildingInstance instance = new CCS_BuildingInstance(
                instanceId,
                placementState.ActivePieceId,
                placementState.PreviewPosition,
                placementState.PreviewRotation,
                Time.time);

            if (!buildingService.TryAddPlacedInstance(instance, consumedSnapMatch))
            {
                RestorePlacementCosts(definition, activeRecipe);
                validation = CCS_BuildingPlacementValidationResult.Failed("Failed to register placed building instance.");
                RaisePlacementFailed(validation);
                return validation;
            }

            if (consumedSnapMatch.HasMatch
                && !buildingService.TryMarkSnapPointOccupied(
                    consumedSnapMatch.TargetInstanceId,
                    consumedSnapMatch.TargetSnapPointId,
                    true))
            {
                RestorePlacementCosts(definition, activeRecipe);
                validation = CCS_BuildingPlacementValidationResult.Failed("Failed to mark target snap point occupied.");
                RaisePlacementFailed(validation);
                return validation;
            }

            recipeService?.NotifyPiecePlaced(definition);
            BuildingPlaced?.Invoke(
                CreateEventArgs(instance, $"Placed building piece '{placementState.ActivePieceId}'."));
            return CCS_BuildingPlacementValidationResult.Passed;
        }

        public bool PlaceCurrentPiece()
        {
            return PlaceCurrentPieceUsingSnap().Success;
        }

        public CCS_BuildingPlacementSnapshot GetSnapshot()
        {
            if (!EnsureReady())
            {
                return CCS_BuildingPlacementSnapshot.Empty;
            }

            return placementState.CreateSnapshot();
        }

        public void SyncNextInstanceSequenceFromRestoredInstances(IReadOnlyList<CCS_BuildingInstance> instances)
        {
            if (instances == null || instances.Count == 0)
            {
                return;
            }

            const string instanceIdPrefix = "ccs.survival.building.instance.";
            int maxSequence = nextInstanceSequence;

            for (int index = 0; index < instances.Count; index++)
            {
                string instanceId = instances[index]?.InstanceId;
                if (string.IsNullOrWhiteSpace(instanceId)
                    || !instanceId.StartsWith(instanceIdPrefix, System.StringComparison.Ordinal))
                {
                    continue;
                }

                string sequenceText = instanceId.Substring(instanceIdPrefix.Length);
                if (int.TryParse(sequenceText, out int parsedSequence) && parsedSequence > maxSequence)
                {
                    maxSequence = parsedSequence;
                }
            }

            nextInstanceSequence = maxSequence;
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

        private static void ComputeSnappedTransform(
            Vector3 targetWorldPosition,
            Quaternion targetWorldRotation,
            Vector3 sourceLocalPosition,
            Quaternion sourceLocalRotation,
            out Vector3 snappedPosition,
            out Quaternion snappedRotation)
        {
            snappedRotation = targetWorldRotation * Quaternion.Inverse(sourceLocalRotation);
            snappedPosition = targetWorldPosition - (snappedRotation * sourceLocalPosition);
        }

        private string GenerateInstanceId()
        {
            nextInstanceSequence++;
            return $"ccs.survival.building.instance.{nextInstanceSequence}";
        }

        private void RestorePlacementCosts(CCS_BuildingPieceDefinition definition, CCS_BuildingRecipe recipe)
        {
            if (recipeService != null && recipe != null && recipeService.ProgressionEnabled)
            {
                recipeService.RestoreRecipeCosts(recipe);
                return;
            }

            CCS_BuildingPlacementValidationUtility.RestoreBuildCosts(inventoryService, definition);
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
