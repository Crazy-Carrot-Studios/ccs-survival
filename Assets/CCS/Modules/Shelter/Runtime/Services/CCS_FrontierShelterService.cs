using System;
using System.Collections.Generic;
using CCS.Core;
using CCS.Modules.Building;
using CCS.Modules.Inventory;
using CCS.Survival;
using UnityEngine;

namespace CCS.Modules.Shelter
{
    public sealed class CCS_FrontierShelterService : CCS_ISurvivalService, CCS_IUpdatable
    {
        private const string LogPrefix = "[CCS_FrontierShelterService]";

        private readonly Dictionary<string, CCS_FrontierShelterInstance> registeredShelters =
            new Dictionary<string, CCS_FrontierShelterInstance>(StringComparer.Ordinal);

        private readonly List<CCS_BuildingShelterContribution> contributionBuffer = new List<CCS_BuildingShelterContribution>();

        private CCS_CampDefinition activeProfile;
        private CCS_ShelterService shelterService;
        private CCS_CampService campService;
        private CCS_PlayerInventoryService inventoryService;
        private CCS_FrontierShelterPlacementPreview placementPreview;
        private CCS_ShelterDefinition pendingShelterDefinition;
        private CCS_ItemDefinition pendingPlaceableItem;
        private Vector3 pendingPreviewPosition;
        private Quaternion pendingPreviewRotation = Quaternion.identity;
        private bool pendingPreviewValid;
        private bool isPlacementModeActive;
        private bool isInitialized;

        public event Action<CCS_FrontierShelterInstance> ShelterPlaced;

        public bool IsInitialized => isInitialized;

        public bool IsPlacementModeActive => isPlacementModeActive;

        public CCS_CampDefinition ActiveProfile => activeProfile;

        public int RegisteredShelterCount => registeredShelters.Count;

        public void Initialize()
        {
            isInitialized = true;
        }

        public void InitializeFromProfile(CCS_CampDefinition profile)
        {
            activeProfile = profile;
            isInitialized = profile != null;
        }

        public void BindShelterService(CCS_ShelterService service)
        {
            shelterService = service;
            RebuildShelterContributions();
        }

        public void BindCampService(CCS_CampService service)
        {
            campService = service;
        }

        public void BindInventoryService(CCS_PlayerInventoryService service)
        {
            inventoryService = service;
        }

        public void Tick(float deltaTime)
        {
            if (campService != null && campService.IsInitialized)
            {
                campService.Tick(deltaTime);
            }
        }

        public bool TryResolveShelterDefinitionForItem(
            CCS_ItemDefinition itemDefinition,
            out CCS_ShelterDefinition shelterDefinition)
        {
            shelterDefinition = null;
            if (!EnsureReady() || itemDefinition == null)
            {
                return false;
            }

            return activeProfile.TryGetByPlaceableItemId(itemDefinition.ItemId, out shelterDefinition)
                && shelterDefinition.IsFunctional;
        }

        public CCS_FrontierShelterPlacementResult HandlePlacementRequest(CCS_FrontierShelterPlacementRequest request)
        {
            if (!EnsureReady() || request?.ShelterDefinition == null)
            {
                return CCS_FrontierShelterPlacementResult.Failure("Shelter service or definition is unavailable.");
            }

            if (!request.ShelterDefinition.IsFunctional)
            {
                return CCS_FrontierShelterPlacementResult.Failure("Shelter definition is not functional in this milestone.");
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
            pendingShelterDefinition = null;
            pendingPlaceableItem = null;
            pendingPreviewValid = false;
            EnsurePlacementPreviewHidden();
        }

        public bool TryGetShelterInstance(string instanceId, out CCS_FrontierShelterInstance instance)
        {
            return registeredShelters.TryGetValue(instanceId, out instance);
        }

        public IReadOnlyList<CCS_FrontierShelterInstance> GetRegisteredShelters()
        {
            return new List<CCS_FrontierShelterInstance>(registeredShelters.Values);
        }

        public CCS_FrontierShelterInstanceSaveState[] CaptureWorldState()
        {
            if (registeredShelters.Count == 0)
            {
                return Array.Empty<CCS_FrontierShelterInstanceSaveState>();
            }

            List<CCS_FrontierShelterInstanceSaveState> records =
                new List<CCS_FrontierShelterInstanceSaveState>(registeredShelters.Count);
            foreach (KeyValuePair<string, CCS_FrontierShelterInstance> entry in registeredShelters)
            {
                if (entry.Value != null)
                {
                    records.Add(entry.Value.CaptureState());
                }
            }

            return records.ToArray();
        }

        public void RestoreWorldState(CCS_FrontierShelterInstanceSaveState[] saveStates)
        {
            ClearSpawnedShelters();
            if (saveStates == null || saveStates.Length == 0)
            {
                return;
            }

            for (int index = 0; index < saveStates.Length; index++)
            {
                CCS_FrontierShelterInstanceSaveState saveState = saveStates[index];
                if (saveState == null
                    || string.IsNullOrWhiteSpace(saveState.ShelterDefinitionId)
                    || !activeProfile.TryGetShelterById(saveState.ShelterDefinitionId, out CCS_ShelterDefinition definition))
                {
                    continue;
                }

                SpawnShelterInstance(
                    definition,
                    saveState.Position,
                    Quaternion.Euler(0f, saveState.RotationY, 0f),
                    saveState.InstanceId,
                    saveState);
            }
        }

        public void UnregisterShelterInstance(CCS_FrontierShelterInstance instance)
        {
            if (instance == null || string.IsNullOrWhiteSpace(instance.InstanceId))
            {
                return;
            }

            registeredShelters.Remove(instance.InstanceId);
            RebuildShelterContributions();
            campService?.RecalculateCamp();
        }

        private CCS_FrontierShelterPlacementResult UpdatePlacementPreview(CCS_FrontierShelterPlacementRequest request)
        {
            pendingShelterDefinition = request.ShelterDefinition;
            pendingPlaceableItem = request.ShelterDefinition.PlaceableKitItem;
            isPlacementModeActive = true;

            if (!TryResolvePlacementPose(
                    request.ShelterDefinition,
                    request.UseOrigin,
                    request.UseDirection,
                    out Vector3 position,
                    out Quaternion rotation,
                    out bool isValid,
                    out string validationMessage))
            {
                pendingPreviewValid = false;
                EnsurePlacementPreview(request.ShelterDefinition, position, rotation, false);
                return CCS_FrontierShelterPlacementResult.Preview(false, validationMessage);
            }

            pendingPreviewPosition = position;
            pendingPreviewRotation = rotation;
            pendingPreviewValid = isValid;
            EnsurePlacementPreview(request.ShelterDefinition, position, rotation, isValid);
            return CCS_FrontierShelterPlacementResult.Preview(
                isValid,
                isValid ? "Shelter placement preview updated. Use again to confirm." : validationMessage);
        }

        private CCS_FrontierShelterPlacementResult TryConfirmPlacement()
        {
            if (!isPlacementModeActive || pendingShelterDefinition == null)
            {
                return CCS_FrontierShelterPlacementResult.Failure("Shelter placement mode is not active.");
            }

            if (!pendingPreviewValid)
            {
                return CCS_FrontierShelterPlacementResult.Failure("Shelter placement position is invalid.");
            }

            if (inventoryService == null
                || !inventoryService.IsInitialized
                || pendingPlaceableItem == null
                || inventoryService.GetQuantity(pendingPlaceableItem) <= 0)
            {
                return CCS_FrontierShelterPlacementResult.Failure("Shelter kit is not available in inventory.");
            }

            if (inventoryService.RemoveItem(pendingPlaceableItem, 1) < 1)
            {
                return CCS_FrontierShelterPlacementResult.Failure("Failed to consume shelter kit from inventory.");
            }

            string instanceId = BuildInstanceId(pendingShelterDefinition);
            CCS_FrontierShelterInstance instance = SpawnShelterInstance(
                pendingShelterDefinition,
                pendingPreviewPosition,
                pendingPreviewRotation,
                instanceId,
                null);

            if (instance == null)
            {
                inventoryService.AddItem(pendingPlaceableItem, 1);
                return CCS_FrontierShelterPlacementResult.Failure("Failed to spawn shelter instance.");
            }

            CancelPlacementMode();
            ShelterPlaced?.Invoke(instance);
            campService?.RecalculateCamp();
            return CCS_FrontierShelterPlacementResult.Placed("Frontier shelter placed.");
        }

        private CCS_FrontierShelterInstance SpawnShelterInstance(
            CCS_ShelterDefinition definition,
            Vector3 position,
            Quaternion rotation,
            string instanceId,
            CCS_FrontierShelterInstanceSaveState saveState)
        {
            GameObject shelterObject = GameObject.CreatePrimitive(definition.PlacementPrimitive);
            shelterObject.name = $"CCS_Shelter_{instanceId}";
            shelterObject.transform.SetPositionAndRotation(position, rotation);
            shelterObject.transform.localScale = definition.PlacedLocalScale;

            Collider collider = shelterObject.GetComponent<Collider>();
            if (collider != null)
            {
                collider.isTrigger = true;
            }

            CCS_FrontierShelterInstance instance = shelterObject.AddComponent<CCS_FrontierShelterInstance>();
            instance.Initialize(this, definition, instanceId);
            if (saveState != null)
            {
                instance.ApplySaveState(saveState);
            }

            registeredShelters[instanceId] = instance;
            RebuildShelterContributions();
            return instance;
        }

        private void RebuildShelterContributions()
        {
            if (shelterService == null || !shelterService.IsInitialized)
            {
                return;
            }

            contributionBuffer.Clear();
            foreach (KeyValuePair<string, CCS_FrontierShelterInstance> entry in registeredShelters)
            {
                CCS_FrontierShelterInstance instance = entry.Value;
                CCS_ShelterDefinition definition = instance?.ShelterDefinition;
                if (instance == null || definition == null)
                {
                    continue;
                }

                contributionBuffer.Add(new CCS_BuildingShelterContribution(
                    instance.InstanceId,
                    definition.ShelterDefinitionId,
                    instance.WorldPosition,
                    definition.ShelterCoverageRadius,
                    definition.WetnessProtection,
                    definition.ExposureProtection,
                    definition.TemperatureProtection));
            }

            shelterService.SetFrontierShelterContributions(contributionBuffer);
        }

        private void ClearSpawnedShelters()
        {
            foreach (KeyValuePair<string, CCS_FrontierShelterInstance> entry in registeredShelters)
            {
                if (entry.Value != null)
                {
                    UnityEngine.Object.Destroy(entry.Value.gameObject);
                }
            }

            registeredShelters.Clear();
            RebuildShelterContributions();
        }

        private bool TryResolvePlacementPose(
            CCS_ShelterDefinition definition,
            Vector3 origin,
            Vector3 forward,
            out Vector3 position,
            out Quaternion rotation,
            out bool isValid,
            out string message)
        {
            position = origin + forward * definition.PlacementForwardDistance;
            rotation = Quaternion.LookRotation(forward, Vector3.up);
            isValid = false;
            message = "Invalid placement surface.";

            if (Physics.Raycast(
                    origin,
                    Vector3.down,
                    out RaycastHit groundHit,
                    definition.PlacementMaxGroundRayDistance,
                    Physics.DefaultRaycastLayers,
                    QueryTriggerInteraction.Ignore))
            {
                position = groundHit.point;
                rotation = Quaternion.LookRotation(forward, groundHit.normal);
            }

            float slopeAngle = Vector3.Angle(rotation * Vector3.up, Vector3.up);
            if (slopeAngle > definition.PlacementMaxSlopeAngle)
            {
                message = "Ground slope is too steep for shelter placement.";
                return false;
            }

            isValid = true;
            message = "Valid shelter placement.";
            return true;
        }

        private void EnsurePlacementPreview(CCS_ShelterDefinition definition, Vector3 position, Quaternion rotation, bool isValid)
        {
            if (placementPreview == null)
            {
                GameObject previewRoot = new GameObject("CCS_FrontierShelterPlacementPreviewRoot");
                placementPreview = previewRoot.AddComponent<CCS_FrontierShelterPlacementPreview>();
            }

            placementPreview.EnsurePreviewObject(definition.PlacementPrimitive);
            placementPreview.UpdatePreview(
                position,
                rotation,
                definition.PlacedLocalScale,
                isValid,
                new Color(0.3f, 0.8f, 0.35f, 0.55f),
                new Color(0.9f, 0.25f, 0.2f, 0.55f));
        }

        private void EnsurePlacementPreviewHidden()
        {
            placementPreview?.SetVisible(false);
        }

        private static string BuildInstanceId(CCS_ShelterDefinition definition)
        {
            return $"{definition.ShelterDefinitionId}.{Guid.NewGuid():N}";
        }

        private bool EnsureReady()
        {
            return isInitialized && activeProfile != null;
        }
    }
}
