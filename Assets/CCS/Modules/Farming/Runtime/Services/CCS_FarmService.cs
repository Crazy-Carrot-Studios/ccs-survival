using System;
using System.Collections.Generic;
using CCS.Core;
using CCS.Modules.Inventory;
using CCS.Modules.Shelter;
using CCS.Survival;
using UnityEngine;
using Object = UnityEngine.Object;

// =============================================================================
// SCRIPT: CCS_FarmService
// CATEGORY: Modules / Farming / Runtime / Services
// PURPOSE: Owns farm plots, crop growth timers, planting, and harvesting.
// PLACEMENT: Registered by CCS_SurvivalGameplayServiceRegistration.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-02
// NOTES: Milestone 2.2.0 — timer growth only; no seasons or irrigation.
// =============================================================================

namespace CCS.Modules.Farming
{
    public sealed class CCS_FarmService : CCS_ISurvivalService, CCS_IUpdatable
    {
        private const string LogPrefix = "[CCS_FarmService]";

        private readonly Dictionary<string, CCS_FarmPlotInstance> plotsById =
            new Dictionary<string, CCS_FarmPlotInstance>(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<string, CCS_CropDefinition> cropDefinitionLookup =
            new Dictionary<string, CCS_CropDefinition>(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<string, CCS_FarmPlotDefinition> plotDefinitionLookup =
            new Dictionary<string, CCS_FarmPlotDefinition>(StringComparer.OrdinalIgnoreCase);

        private CCS_CropProfile activeProfile;
        private CCS_PlayerInventoryService inventoryService;
        private CCS_CampService campService;
        private Func<Vector3> playerPositionProvider;
        private GameObject placementPreviewObject;
        private CCS_FarmPlotDefinition pendingPlotDefinition;
        private Vector3 pendingPreviewPosition;
        private float pendingPreviewRotationY;
        private bool pendingPreviewValid;
        private bool isPlacementModeActive;
        private bool isInitialized;

        public event Action<CCS_FarmPlotInstance> FarmPlotPlaced;
        public event Action<CCS_FarmPlotInstance> CropPlanted;
        public event Action<CCS_FarmPlotInstance> CropMature;
        public event Action<CCS_FarmPlotInstance> CropHarvested;

        public bool IsInitialized => isInitialized;

        public bool IsPlacementModeActive => isPlacementModeActive;

        public CCS_CropProfile ActiveProfile => activeProfile;

        public void Initialize()
        {
            isInitialized = true;
        }

        public void InitializeFromProfile(CCS_CropProfile profile)
        {
            activeProfile = profile;
            cropDefinitionLookup.Clear();
            plotDefinitionLookup.Clear();

            if (profile == null)
            {
                isInitialized = false;
                return;
            }

            CCS_SurvivalValidationResult validation = CCS_FarmValidationUtility.ValidateProfile(profile);
            if (!validation.IsSuccess)
            {
                Debug.LogWarning($"{LogPrefix} Profile validation warning: {validation.Message}");
            }

            CCS_CropDefinition[] cropDefinitions = profile.CropDefinitions;
            for (int index = 0; index < cropDefinitions.Length; index++)
            {
                CCS_CropDefinition definition = cropDefinitions[index];
                if (definition == null || string.IsNullOrWhiteSpace(definition.CropId))
                {
                    continue;
                }

                cropDefinitionLookup[definition.CropId] = definition;
            }

            CCS_FarmPlotDefinition[] plotDefinitions = profile.FarmPlotDefinitions;
            for (int index = 0; index < plotDefinitions.Length; index++)
            {
                CCS_FarmPlotDefinition definition = plotDefinitions[index];
                if (definition == null || string.IsNullOrWhiteSpace(definition.PlotDefinitionId))
                {
                    continue;
                }

                plotDefinitionLookup[definition.PlotDefinitionId] = definition;
            }

            isInitialized = validation.IsSuccess || plotDefinitionLookup.Count > 0;
        }

        public void BindInventoryService(CCS_PlayerInventoryService inventory)
        {
            inventoryService = inventory;
        }

        public void BindCampService(CCS_CampService camp)
        {
            campService = camp;
        }

        public void BindPlayerPositionProvider(Func<Vector3> provider)
        {
            playerPositionProvider = provider;
        }

        public void Tick(float deltaTime)
        {
            if (!isInitialized || plotsById.Count == 0)
            {
                return;
            }

            foreach (KeyValuePair<string, CCS_FarmPlotInstance> entry in plotsById)
            {
                CCS_FarmPlotInstance plot = entry.Value;
                if (plot?.Crop == null)
                {
                    continue;
                }

                CCS_CropInstance crop = plot.Crop;
                if (crop.GrowthStage == CCS_CropGrowthStage.Empty
                    || crop.GrowthStage == CCS_CropGrowthStage.Harvested
                    || crop.GrowthStage == CCS_CropGrowthStage.Mature)
                {
                    if (crop.GrowthStage == CCS_CropGrowthStage.Harvested)
                    {
                        plot.ClearCrop();
                        UpdatePlotVisual(plot);
                    }

                    continue;
                }

                crop.GrowthElapsedSeconds += deltaTime;
                CCS_CropGrowthStage previousStage = crop.GrowthStage;
                crop.UpdateGrowthStageFromElapsed();
                UpdateCropVisual(plot, crop);

                if (previousStage != CCS_CropGrowthStage.Mature && crop.GrowthStage == CCS_CropGrowthStage.Mature)
                {
                    CropMature?.Invoke(plot);
                }
            }
        }

        public bool TryResolvePlotDefinitionForItem(
            CCS_ItemDefinition itemDefinition,
            out CCS_FarmPlotDefinition plotDefinition)
        {
            plotDefinition = null;
            if (!EnsureReady() || itemDefinition == null || activeProfile == null)
            {
                return false;
            }

            return activeProfile.TryGetPlotByKitItemId(itemDefinition.ItemId, out plotDefinition);
        }

        public bool TryResolveCropDefinitionForSeedItem(
            CCS_ItemDefinition itemDefinition,
            out CCS_CropDefinition cropDefinition)
        {
            cropDefinition = null;
            if (!EnsureReady() || itemDefinition == null || activeProfile == null)
            {
                return false;
            }

            return activeProfile.TryGetCropBySeedItemId(itemDefinition.ItemId, out cropDefinition);
        }

        public CCS_FarmPlacementResult HandlePlacementRequest(CCS_FarmPlacementRequest request)
        {
            if (!EnsureReady() || request == null)
            {
                return CCS_FarmPlacementResult.Failure("Farm service is unavailable.");
            }

            if (request.ConfirmPlacement)
            {
                return TryConfirmPlacement();
            }

            return UpdatePlacementPreview(request);
        }

        public void CancelPlacementMode()
        {
            isPlacementModeActive = false;
            pendingPlotDefinition = null;
            pendingPreviewValid = false;
            EnsurePlacementPreviewHidden();
        }

        public bool TryPlantSeedInNearestPlot(CCS_ItemDefinition seedItem, float maxDistance = 4f)
        {
            if (!EnsureReady()
                || seedItem == null
                || inventoryService == null
                || !TryResolveCropDefinitionForSeedItem(seedItem, out CCS_CropDefinition cropDefinition))
            {
                return false;
            }

            Vector3 origin = playerPositionProvider != null ? playerPositionProvider() : Vector3.zero;
            CCS_FarmPlotInstance nearestPlot = FindNearestEmptyPlot(origin, maxDistance);
            if (nearestPlot == null)
            {
                return false;
            }

            return TryPlantSeedInPlot(nearestPlot.InstanceId, cropDefinition, seedItem);
        }

        public bool TryPlantSeedInPlot(
            string plotInstanceId,
            CCS_CropDefinition cropDefinition,
            CCS_ItemDefinition seedItem)
        {
            if (!TryGetPlot(plotInstanceId, out CCS_FarmPlotInstance plot)
                || !plot.CanPlant
                || cropDefinition == null
                || inventoryService == null
                || seedItem == null
                || inventoryService.RemoveItem(seedItem, 1) <= 0)
            {
                return false;
            }

            plot.PlantCrop(cropDefinition);
            UpdatePlotVisual(plot);
            CropPlanted?.Invoke(plot);
            return true;
        }

        public bool TryHarvestPlot(string plotInstanceId)
        {
            if (!TryGetPlot(plotInstanceId, out CCS_FarmPlotInstance plot)
                || !plot.CanHarvest
                || inventoryService == null
                || !inventoryService.IsInitialized
                || plot.Crop?.Definition?.HarvestItem == null)
            {
                return false;
            }

            CCS_CropDefinition cropDefinition = plot.Crop.Definition;
            int harvestQuantity = 1;
            int added = inventoryService.AddItem(cropDefinition.HarvestItem, harvestQuantity);
            if (added < harvestQuantity)
            {
                return false;
            }

            if (cropDefinition.SeedReturnQuantity > 0 && cropDefinition.SeedItem != null)
            {
                inventoryService.AddItem(cropDefinition.SeedItem, cropDefinition.SeedReturnQuantity);
            }

            plot.Crop.GrowthStage = CCS_CropGrowthStage.Harvested;
            UpdatePlotVisual(plot);
            CropHarvested?.Invoke(plot);
            return true;
        }

        public bool TryHarvestNearestMaturePlot(float maxDistance = 4f)
        {
            Vector3 origin = playerPositionProvider != null ? playerPositionProvider() : Vector3.zero;
            CCS_FarmPlotInstance nearestPlot = FindNearestMaturePlot(origin, maxDistance);
            return nearestPlot != null && TryHarvestPlot(nearestPlot.InstanceId);
        }

        public bool TryForceFirstCropMatureForPlaytest()
        {
            foreach (KeyValuePair<string, CCS_FarmPlotInstance> entry in plotsById)
            {
                CCS_FarmPlotInstance plot = entry.Value;
                if (plot?.Crop == null || plot.Crop.GrowthStage == CCS_CropGrowthStage.Mature)
                {
                    continue;
                }

                plot.Crop.GrowthElapsedSeconds = plot.Crop.Definition.GrowthDurationSeconds;
                plot.Crop.GrowthStage = CCS_CropGrowthStage.Mature;
                UpdateCropVisual(plot, plot.Crop);
                CropMature?.Invoke(plot);
                return true;
            }

            return false;
        }

        public int GetPlotCount()
        {
            return plotsById.Count;
        }

        public CCS_FarmPlotSnapshot[] CapturePlotState()
        {
            if (plotsById.Count == 0)
            {
                return Array.Empty<CCS_FarmPlotSnapshot>();
            }

            CCS_FarmPlotSnapshot[] snapshots = new CCS_FarmPlotSnapshot[plotsById.Count];
            int index = 0;
            foreach (KeyValuePair<string, CCS_FarmPlotInstance> entry in plotsById)
            {
                snapshots[index++] = entry.Value.CaptureSnapshot();
            }

            return snapshots;
        }

        public void RestoreState(CCS_FarmPlotSnapshot[] plotSnapshots)
        {
            ClearAllPlots();
            if (plotSnapshots == null || plotSnapshots.Length == 0 || activeProfile == null)
            {
                return;
            }

            for (int index = 0; index < plotSnapshots.Length; index++)
            {
                CCS_FarmPlotSnapshot snapshot = plotSnapshots[index];
                if (snapshot == null
                    || string.IsNullOrWhiteSpace(snapshot.instanceId)
                    || !activeProfile.TryGetPlotById(snapshot.plotDefinitionId, out CCS_FarmPlotDefinition plotDefinition))
                {
                    continue;
                }

                Vector3 position = new Vector3(snapshot.positionX, snapshot.positionY, snapshot.positionZ);
                CCS_FarmPlotInstance plot = new CCS_FarmPlotInstance(
                    snapshot.instanceId,
                    plotDefinition,
                    position,
                    snapshot.rotationY,
                    snapshot.campOwnerId);
                plot.ApplySnapshot(snapshot, activeProfile);
                SpawnPlotWorldObject(plot);
                plotsById[plot.InstanceId] = plot;
            }

            campService?.RecalculateCamp();
        }

        public bool TryGetPlot(string instanceId, out CCS_FarmPlotInstance plot)
        {
            plot = null;
            if (string.IsNullOrWhiteSpace(instanceId))
            {
                return false;
            }

            return plotsById.TryGetValue(instanceId, out plot) && plot != null;
        }

        private CCS_FarmPlacementResult UpdatePlacementPreview(CCS_FarmPlacementRequest request)
        {
            if (!activeProfile.TryGetPlotById(request.PlotDefinitionId, out CCS_FarmPlotDefinition definition))
            {
                return CCS_FarmPlacementResult.Failure("Unknown farm plot definition.");
            }

            pendingPlotDefinition = definition;
            isPlacementModeActive = true;

            if (!TryResolvePlacementPose(
                    definition,
                    request.UseOrigin,
                    request.UseDirection,
                    out Vector3 position,
                    out float rotationY,
                    out bool isValid,
                    out string validationMessage))
            {
                return CCS_FarmPlacementResult.Preview(false, validationMessage);
            }

            pendingPreviewPosition = position;
            pendingPreviewRotationY = rotationY;
            pendingPreviewValid = isValid;
            EnsurePlacementPreview(definition, position, rotationY, isValid);
            return CCS_FarmPlacementResult.Preview(isValid, validationMessage);
        }

        private CCS_FarmPlacementResult TryConfirmPlacement()
        {
            if (!isPlacementModeActive || !pendingPreviewValid || pendingPlotDefinition == null)
            {
                return CCS_FarmPlacementResult.Failure("Farm plot placement mode is not active.");
            }

            if (inventoryService == null
                || pendingPlotDefinition.PlaceableKitItem == null
                || inventoryService.RemoveItem(pendingPlotDefinition.PlaceableKitItem, 1) <= 0)
            {
                return CCS_FarmPlacementResult.Failure("Farm plot kit is not available in inventory.");
            }

            CCS_FarmPlotInstance instance = SpawnPlotInstance(
                pendingPlotDefinition,
                pendingPreviewPosition,
                pendingPreviewRotationY,
                GenerateInstanceId(pendingPlotDefinition.PlotDefinitionId),
                CCS_FarmingContentIds.CampOwnerId);
            CancelPlacementMode();
            campService?.RecalculateCamp();
            FarmPlotPlaced?.Invoke(instance);
            return CCS_FarmPlacementResult.Placed($"{pendingPlotDefinition.DisplayName} placed.");
        }

        private CCS_FarmPlotInstance SpawnPlotInstance(
            CCS_FarmPlotDefinition definition,
            Vector3 position,
            float rotationY,
            string instanceId,
            string campOwnerId)
        {
            CCS_FarmPlotInstance instance = new CCS_FarmPlotInstance(
                instanceId,
                definition,
                position,
                rotationY,
                campOwnerId);
            SpawnPlotWorldObject(instance);
            plotsById[instanceId] = instance;
            return instance;
        }

        private void SpawnPlotWorldObject(CCS_FarmPlotInstance plot)
        {
            if (plot?.Definition == null)
            {
                return;
            }

            if (plot.WorldObject != null)
            {
                Object.Destroy(plot.WorldObject);
            }

            GameObject plotObject = GameObject.CreatePrimitive(plot.Definition.PlacementPrimitive);
            plotObject.name = $"CCS_FarmPlot_{plot.InstanceId}";
            plotObject.transform.SetPositionAndRotation(
                plot.WorldPosition,
                Quaternion.Euler(0f, plot.RotationY, 0f));
            plotObject.transform.localScale = plot.Definition.PlacedLocalScale;

            MeshRenderer renderer = plotObject.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial.color = plot.Definition.PlotColor;
            }

            Collider collider = plotObject.GetComponent<Collider>();
            if (collider != null)
            {
                collider.isTrigger = false;
            }

            CCS_FarmPlotInteractable interactable = plotObject.AddComponent<CCS_FarmPlotInteractable>();
            interactable.ConfigureRuntime(plot.InstanceId);
            plot.WorldObject = plotObject;
            UpdatePlotVisual(plot);
        }

        private void UpdatePlotVisual(CCS_FarmPlotInstance plot)
        {
            if (plot?.Crop == null)
            {
                DestroyCropVisual(plot);
                return;
            }

            UpdateCropVisual(plot, plot.Crop);
        }

        private void UpdateCropVisual(CCS_FarmPlotInstance plot, CCS_CropInstance crop)
        {
            if (plot == null || crop?.Definition == null)
            {
                return;
            }

            if (crop.GrowthStage == CCS_CropGrowthStage.Empty
                || crop.GrowthStage == CCS_CropGrowthStage.Harvested)
            {
                DestroyCropVisual(plot);
                return;
            }

            if (crop.VisualObject == null)
            {
                if (crop.Definition.CropVisualPrefab != null)
                {
                    crop.VisualObject = Object.Instantiate(
                        crop.Definition.CropVisualPrefab,
                        plot.WorldObject != null ? plot.WorldObject.transform : null);
                }
                else
                {
                    crop.VisualObject = GameObject.CreatePrimitive(crop.Definition.FallbackCropPrimitive);
                    crop.VisualObject.transform.SetParent(plot.WorldObject != null ? plot.WorldObject.transform : null, false);
                }

                crop.VisualObject.name = $"CCS_CropVisual_{crop.Definition.CropId}";
            }

            Vector3 localScale = ResolveCropScale(crop.GrowthStage);
            crop.VisualObject.transform.localPosition = new Vector3(0f, 0.25f, 0f);
            crop.VisualObject.transform.localRotation = Quaternion.identity;
            crop.VisualObject.transform.localScale = localScale;

            MeshRenderer renderer = crop.VisualObject.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial.color = crop.Definition.MatureCropColor;
            }
        }

        private static Vector3 ResolveCropScale(CCS_CropGrowthStage stage)
        {
            switch (stage)
            {
                case CCS_CropGrowthStage.Planted:
                    return new Vector3(0.15f, 0.15f, 0.15f);
                case CCS_CropGrowthStage.Sprouting:
                    return new Vector3(0.25f, 0.35f, 0.25f);
                case CCS_CropGrowthStage.Growing:
                    return new Vector3(0.35f, 0.65f, 0.35f);
                case CCS_CropGrowthStage.Mature:
                    return new Vector3(0.45f, 0.9f, 0.45f);
                default:
                    return Vector3.one * 0.2f;
            }
        }

        private static void DestroyCropVisual(CCS_FarmPlotInstance plot)
        {
            if (plot?.Crop?.VisualObject != null)
            {
                Object.Destroy(plot.Crop.VisualObject);
                plot.Crop.VisualObject = null;
            }
        }

        private CCS_FarmPlotInstance FindNearestEmptyPlot(Vector3 origin, float maxDistance)
        {
            CCS_FarmPlotInstance nearest = null;
            float nearestDistance = maxDistance * maxDistance;
            foreach (KeyValuePair<string, CCS_FarmPlotInstance> entry in plotsById)
            {
                CCS_FarmPlotInstance plot = entry.Value;
                if (plot == null || !plot.CanPlant)
                {
                    continue;
                }

                float distance = (plot.WorldPosition - origin).sqrMagnitude;
                if (distance > nearestDistance)
                {
                    continue;
                }

                nearestDistance = distance;
                nearest = plot;
            }

            return nearest;
        }

        private CCS_FarmPlotInstance FindNearestMaturePlot(Vector3 origin, float maxDistance)
        {
            CCS_FarmPlotInstance nearest = null;
            float nearestDistance = maxDistance * maxDistance;
            foreach (KeyValuePair<string, CCS_FarmPlotInstance> entry in plotsById)
            {
                CCS_FarmPlotInstance plot = entry.Value;
                if (plot == null || !plot.CanHarvest)
                {
                    continue;
                }

                float distance = (plot.WorldPosition - origin).sqrMagnitude;
                if (distance > nearestDistance)
                {
                    continue;
                }

                nearestDistance = distance;
                nearest = plot;
            }

            return nearest;
        }

        private bool TryResolvePlacementPose(
            CCS_FarmPlotDefinition definition,
            Vector3 useOrigin,
            Vector3 useDirection,
            out Vector3 position,
            out float rotationY,
            out bool isValid,
            out string validationMessage)
        {
            position = useOrigin + useDirection * definition.PlacementForwardDistance;
            rotationY = Mathf.Atan2(useDirection.x, useDirection.z) * Mathf.Rad2Deg;
            isValid = false;
            validationMessage = "Invalid placement.";

            if (Physics.Raycast(
                    useOrigin,
                    Vector3.down,
                    out RaycastHit originHit,
                    definition.PlacementMaxGroundRayDistance,
                    Physics.DefaultRaycastLayers,
                    QueryTriggerInteraction.Ignore)
                && Physics.Raycast(
                    position + Vector3.up * 2f,
                    Vector3.down,
                    out RaycastHit targetHit,
                    definition.PlacementMaxGroundRayDistance + 2f,
                    Physics.DefaultRaycastLayers,
                    QueryTriggerInteraction.Ignore))
            {
                position = targetHit.point;
                float slopeAngle = Vector3.Angle(targetHit.normal, Vector3.up);
                isValid = slopeAngle <= definition.PlacementMaxSlopeAngle;
                validationMessage = isValid
                    ? "Farm plot placement valid."
                    : $"Ground too steep ({slopeAngle:0.#}°).";
                return true;
            }

            validationMessage = "No ground found for farm plot placement.";
            return false;
        }

        private void EnsurePlacementPreview(CCS_FarmPlotDefinition definition, Vector3 position, float rotationY, bool isValid)
        {
            if (placementPreviewObject == null)
            {
                placementPreviewObject = GameObject.CreatePrimitive(definition.PlacementPrimitive);
                placementPreviewObject.name = "CCS_FarmPlotPlacementPreview";
                Collider collider = placementPreviewObject.GetComponent<Collider>();
                if (collider != null)
                {
                    Object.Destroy(collider);
                }
            }

            placementPreviewObject.transform.SetPositionAndRotation(
                position,
                Quaternion.Euler(0f, rotationY, 0f));
            placementPreviewObject.transform.localScale = definition.PlacedLocalScale;
            placementPreviewObject.SetActive(true);

            MeshRenderer renderer = placementPreviewObject.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial.color = isValid
                    ? new Color(0.3f, 0.9f, 0.35f, 0.65f)
                    : new Color(0.9f, 0.25f, 0.25f, 0.65f);
            }
        }

        private void EnsurePlacementPreviewHidden()
        {
            if (placementPreviewObject != null)
            {
                placementPreviewObject.SetActive(false);
            }
        }

        private void ClearAllPlots()
        {
            foreach (KeyValuePair<string, CCS_FarmPlotInstance> entry in plotsById)
            {
                if (entry.Value?.WorldObject != null)
                {
                    Object.Destroy(entry.Value.WorldObject);
                }
            }

            plotsById.Clear();
            EnsurePlacementPreviewHidden();
        }

        private static string GenerateInstanceId(string definitionId)
        {
            return $"{definitionId}.{Guid.NewGuid():N}";
        }

        private bool EnsureReady()
        {
            return isInitialized && activeProfile != null;
        }
    }
}
