using System;
using System.Collections.Generic;
using CCS.Core;
using CCS.Modules.Inventory;
using CCS.Survival;
using UnityEngine;

namespace CCS.Modules.Storage
{
    public sealed class CCS_FrontierStoragePlacementService : CCS_ISurvivalService
    {
        private readonly Dictionary<string, CCS_StorageContainer> placedStorageContainers =
            new Dictionary<string, CCS_StorageContainer>(StringComparer.Ordinal);

        private CCS_FrontierStorageCampProfile activeProfile;
        private CCS_StorageService storageService;
        private CCS_PlayerInventoryService inventoryService;
        private Action campStructureChangedCallback;
        private GameObject placementPreviewObject;
        private CCS_StorageContainerDefinition pendingDefinition;
        private Vector3 pendingPreviewPosition;
        private Quaternion pendingPreviewRotation = Quaternion.identity;
        private bool pendingPreviewValid;
        private bool isPlacementModeActive;
        private bool isInitialized;

        public bool IsInitialized => isInitialized;

        public bool IsPlacementModeActive => isPlacementModeActive;

        public void Initialize()
        {
            isInitialized = true;
        }

        public void InitializeFromProfile(CCS_FrontierStorageCampProfile profile)
        {
            activeProfile = profile;
            isInitialized = profile != null;
        }

        public void BindStorageService(CCS_StorageService service)
        {
            storageService = service;
        }

        public void BindInventoryService(CCS_PlayerInventoryService service)
        {
            inventoryService = service;
        }

        public void BindCampStructureChangedCallback(Action callback)
        {
            campStructureChangedCallback = callback;
        }

        public bool TryResolveStorageDefinitionForItem(
            CCS_ItemDefinition itemDefinition,
            out CCS_StorageContainerDefinition storageDefinition)
        {
            storageDefinition = null;
            if (!EnsureReady() || itemDefinition == null)
            {
                return false;
            }

            return activeProfile.TryGetStorageByPlaceableItemId(itemDefinition.ItemId, out storageDefinition)
                && storageDefinition.ContributesToCampTier;
        }

        public CCS_FrontierStoragePlacementResult HandlePlacementRequest(CCS_FrontierStoragePlacementRequest request)
        {
            if (!EnsureReady() || request == null)
            {
                return CCS_FrontierStoragePlacementResult.Failure("Storage placement service is unavailable.");
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
            pendingDefinition = null;
            pendingPreviewValid = false;
            SetPreviewVisible(false);
        }

        public bool HasStorageInRadius(Vector3 origin, float radius)
        {
            foreach (KeyValuePair<string, CCS_StorageContainer> entry in placedStorageContainers)
            {
                CCS_StorageContainer container = entry.Value;
                if (container != null && Vector3.Distance(origin, container.transform.position) <= radius)
                {
                    return true;
                }
            }

            return false;
        }

        public void RebuildPlacedStorageTracking()
        {
            placedStorageContainers.Clear();
            if (storageService == null || !storageService.IsInitialized || !EnsureReady())
            {
                return;
            }

            IReadOnlyList<CCS_StorageContainer> containers = storageService.GetRegisteredContainers();
            for (int index = 0; index < containers.Count; index++)
            {
                CCS_StorageContainer container = containers[index];
                if (container == null)
                {
                    continue;
                }

                if (activeProfile.TryGetStorageById(container.ContainerId, out CCS_StorageContainerDefinition definition)
                    && definition.ContributesToCampTier)
                {
                    placedStorageContainers[container.InstanceId] = container;
                }
            }

            campStructureChangedCallback?.Invoke();
        }

        private CCS_FrontierStoragePlacementResult UpdatePlacementPreview(CCS_FrontierStoragePlacementRequest request)
        {
            if (!activeProfile.TryGetStorageById(request.ContainerDefinitionId, out CCS_StorageContainerDefinition definition))
            {
                return CCS_FrontierStoragePlacementResult.Failure("Unknown storage definition.");
            }

            pendingDefinition = definition;
            isPlacementModeActive = true;

            if (!TryResolvePlacementPose(
                    request.UseOrigin,
                    request.UseDirection,
                    out Vector3 position,
                    out Quaternion rotation,
                    out bool isValid,
                    out string validationMessage))
            {
                return CCS_FrontierStoragePlacementResult.Preview(false, validationMessage);
            }

            pendingPreviewPosition = position;
            pendingPreviewRotation = rotation;
            pendingPreviewValid = isValid;
            EnsurePreview(position, rotation, isValid);
            return CCS_FrontierStoragePlacementResult.Preview(isValid, validationMessage);
        }

        private CCS_FrontierStoragePlacementResult TryConfirmPlacement()
        {
            if (!isPlacementModeActive || !pendingPreviewValid || pendingDefinition == null)
            {
                return CCS_FrontierStoragePlacementResult.Failure("Storage placement mode is not active.");
            }

            if (inventoryService == null
                || pendingDefinition.PlaceableKitItem == null
                || inventoryService.RemoveItem(pendingDefinition.PlaceableKitItem, 1) <= 0)
            {
                return CCS_FrontierStoragePlacementResult.Failure("Storage kit is not available in inventory.");
            }

            CCS_StorageContainer container = storageService.SpawnContainer(
                pendingDefinition,
                pendingPreviewPosition,
                pendingPreviewRotation,
                null,
                markDynamicSpawn: true);
            CancelPlacementMode();
            if (container == null)
            {
                return CCS_FrontierStoragePlacementResult.Failure("Failed to spawn storage container.");
            }

            placedStorageContainers[container.InstanceId] = container;
            campStructureChangedCallback?.Invoke();
            return CCS_FrontierStoragePlacementResult.Placed("Frontier storage placed.");
        }

        private bool TryResolvePlacementPose(
            Vector3 origin,
            Vector3 forward,
            out Vector3 position,
            out Quaternion rotation,
            out bool isValid,
            out string message)
        {
            position = origin + forward * 2f;
            rotation = Quaternion.LookRotation(forward, Vector3.up);
            isValid = false;
            message = "Invalid placement surface.";

            if (Physics.Raycast(
                    origin,
                    Vector3.down,
                    out RaycastHit groundHit,
                    8f,
                    Physics.DefaultRaycastLayers,
                    QueryTriggerInteraction.Ignore))
            {
                position = groundHit.point;
                rotation = Quaternion.LookRotation(forward, groundHit.normal);
            }

            float slopeAngle = Vector3.Angle(rotation * Vector3.up, Vector3.up);
            if (slopeAngle > 35f)
            {
                message = "Ground slope is too steep for storage placement.";
                return false;
            }

            isValid = true;
            message = "Valid storage placement.";
            return true;
        }

        private void EnsurePreview(Vector3 position, Quaternion rotation, bool isValid)
        {
            if (placementPreviewObject == null)
            {
                placementPreviewObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                placementPreviewObject.name = "CCS_FrontierStoragePlacementPreview";
                Collider collider = placementPreviewObject.GetComponent<Collider>();
                if (collider != null)
                {
                    collider.enabled = false;
                }
            }

            placementPreviewObject.transform.SetPositionAndRotation(position, rotation);
            placementPreviewObject.transform.localScale = new Vector3(1.1f, 0.8f, 1.1f);
            placementPreviewObject.SetActive(true);
            Renderer renderer = placementPreviewObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = isValid
                    ? new Color(0.35f, 0.65f, 0.95f, 0.55f)
                    : new Color(0.9f, 0.25f, 0.2f, 0.55f);
            }
        }

        private void SetPreviewVisible(bool visible)
        {
            if (placementPreviewObject != null)
            {
                placementPreviewObject.SetActive(visible);
            }
        }

        private bool EnsureReady()
        {
            return isInitialized && activeProfile != null && storageService != null && storageService.IsInitialized;
        }
    }
}
