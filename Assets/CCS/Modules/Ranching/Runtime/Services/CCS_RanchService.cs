using System;
using System.Collections.Generic;
using CCS.Core;
using CCS.Modules.Inventory;
using CCS.Modules.Shelter;
using CCS.Survival;
using UnityEngine;
using Object = UnityEngine.Object;

// =============================================================================
// SCRIPT: CCS_RanchService
// CATEGORY: Modules / Ranching / Runtime / Services
// PURPOSE: Owns livestock, ranch structures, timer production, and collection.
// PLACEMENT: Registered by CCS_SurvivalGameplayServiceRegistration.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// NOTES: Milestone 2.1.0 ranching foundation. No advanced animal AI.
// =============================================================================

namespace CCS.Modules.Ranching
{
    public sealed class CCS_RanchService : CCS_ISurvivalService, CCS_IUpdatable
    {
        private const string LogPrefix = "[CCS_RanchService]";

        private readonly Dictionary<string, CCS_LivestockInstance> livestockById =
            new Dictionary<string, CCS_LivestockInstance>(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<string, CCS_RanchStructureInstance> structuresById =
            new Dictionary<string, CCS_RanchStructureInstance>(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<string, CCS_LivestockDefinition> livestockDefinitionLookup =
            new Dictionary<string, CCS_LivestockDefinition>(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<string, CCS_RanchStructureDefinition> structureDefinitionLookup =
            new Dictionary<string, CCS_RanchStructureDefinition>(StringComparer.OrdinalIgnoreCase);

        private CCS_LivestockProfile activeProfile;
        private CCS_PlayerInventoryService inventoryService;
        private CCS_CampService campService;
        private Func<Vector3> playerPositionProvider;
        private GameObject placementPreviewObject;
        private CCS_RanchStructureDefinition pendingStructureDefinition;
        private Vector3 pendingPreviewPosition;
        private float pendingPreviewRotationY;
        private bool pendingPreviewValid;
        private bool isPlacementModeActive;
        private bool isInitialized;

        public event Action<CCS_LivestockInstance> LivestockOwnershipChanged;
        public event Action<CCS_LivestockInstance> LivestockProductionReady;
        public event Action<CCS_LivestockInstance> LivestockProductCollected;
        public event Action<CCS_RanchStructureInstance> RanchStructurePlaced;

        public bool IsInitialized => isInitialized;

        public bool IsPlacementModeActive => isPlacementModeActive;

        public CCS_LivestockProfile ActiveProfile => activeProfile;

        public void Initialize()
        {
            isInitialized = true;
        }

        public void InitializeFromProfile(CCS_LivestockProfile profile)
        {
            activeProfile = profile;
            livestockDefinitionLookup.Clear();
            structureDefinitionLookup.Clear();

            if (profile == null)
            {
                isInitialized = false;
                return;
            }

            CCS_SurvivalValidationResult validation = CCS_RanchValidationUtility.ValidateProfile(profile);
            if (!validation.IsSuccess)
            {
                Debug.LogWarning($"{LogPrefix} Profile validation warning: {validation.Message}");
            }

            CCS_LivestockDefinition[] livestockDefinitions = profile.LivestockDefinitions;
            for (int index = 0; index < livestockDefinitions.Length; index++)
            {
                CCS_LivestockDefinition definition = livestockDefinitions[index];
                if (definition == null || string.IsNullOrWhiteSpace(definition.LivestockId))
                {
                    continue;
                }

                livestockDefinitionLookup[definition.LivestockId] = definition;
            }

            CCS_RanchStructureDefinition[] structureDefinitions = profile.RanchStructureDefinitions;
            for (int index = 0; index < structureDefinitions.Length; index++)
            {
                CCS_RanchStructureDefinition definition = structureDefinitions[index];
                if (definition == null || string.IsNullOrWhiteSpace(definition.StructureDefinitionId))
                {
                    continue;
                }

                structureDefinitionLookup[definition.StructureDefinitionId] = definition;
            }

            isInitialized = validation.IsSuccess || livestockDefinitionLookup.Count > 0;
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

        public bool TryConsumeLivestockPurchaseItem(CCS_ItemDefinition itemDefinition)
        {
            Vector3 spawnPosition = playerPositionProvider != null
                ? playerPositionProvider() + Vector3.forward * 2f
                : Vector3.forward * 2f;
            return TryConsumeLivestockPurchaseItem(itemDefinition, spawnPosition);
        }

        public void Tick(float deltaTime)
        {
            if (!isInitialized || livestockById.Count == 0)
            {
                return;
            }

            foreach (KeyValuePair<string, CCS_LivestockInstance> entry in livestockById)
            {
                CCS_LivestockInstance livestock = entry.Value;
                if (livestock == null || livestock.Definition == null)
                {
                    continue;
                }

                UpdateProductionState(livestock);
                if (livestock.State != CCS_LivestockState.Producing)
                {
                    continue;
                }

                livestock.ProductionElapsedSeconds += deltaTime;
                if (livestock.ProductionElapsedSeconds >= livestock.Definition.ProductionIntervalSeconds)
                {
                    livestock.State = CCS_LivestockState.ReadyToCollect;
                    livestock.LastProducedItemId = livestock.Definition.ProductionItemId;
                    livestock.LastProducedQuantity = livestock.Definition.ProductionQuantity;
                    LivestockProductionReady?.Invoke(livestock);
                }
            }
        }

        public bool TryResolveStructureDefinitionForItem(
            CCS_ItemDefinition itemDefinition,
            out CCS_RanchStructureDefinition structureDefinition)
        {
            structureDefinition = null;
            if (!EnsureReady() || itemDefinition == null)
            {
                return false;
            }

            return activeProfile.TryGetStructureByKitItemId(itemDefinition.ItemId, out structureDefinition);
        }

        public CCS_RanchPlacementResult HandlePlacementRequest(CCS_RanchPlacementRequest request)
        {
            if (!EnsureReady() || request == null)
            {
                return CCS_RanchPlacementResult.Failure("Ranch service is unavailable.");
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
            pendingStructureDefinition = null;
            pendingPreviewValid = false;
            EnsurePlacementPreviewHidden();
        }

        public bool TryConsumeLivestockPurchaseItem(CCS_ItemDefinition itemDefinition, Vector3 spawnPosition)
        {
            if (!EnsureReady() || itemDefinition == null)
            {
                return false;
            }

            if (!activeProfile.TryGetLivestockByPurchaseItemId(itemDefinition.ItemId, out CCS_LivestockDefinition definition))
            {
                return false;
            }

            if (inventoryService == null || inventoryService.RemoveItem(itemDefinition, 1) <= 0)
            {
                return false;
            }

            string instanceId = GenerateInstanceId(definition.LivestockId);
            CCS_LivestockInstance instance = new CCS_LivestockInstance(
                instanceId,
                definition,
                spawnPosition,
                CCS_RanchingContentIds.CampOwnerId);
            SpawnLivestockWorldObject(instance);
            livestockById[instanceId] = instance;
            LivestockOwnershipChanged?.Invoke(instance);
            return true;
        }

        public bool TryAssignLivestockToStructure(string livestockInstanceId, string structureInstanceId)
        {
            if (!TryGetLivestock(livestockInstanceId, out CCS_LivestockInstance livestock)
                || !TryGetStructure(structureInstanceId, out CCS_RanchStructureInstance structure))
            {
                return false;
            }

            if (structure.StructureKind != livestock.Definition.RequiredStructureKind)
            {
                return false;
            }

            livestock.AssignedStructureInstanceId = structureInstanceId;
            livestock.WorldPosition = structure.WorldPosition;
            livestock.State = CCS_LivestockState.Assigned;
            UpdateLivestockTransform(livestock);
            UpdateProductionState(livestock);
            return true;
        }

        public bool TryAssignNearestLivestockToNearestStructure(CCS_LivestockType livestockType, CCS_RanchStructureKind structureKind)
        {
            CCS_LivestockInstance nearestLivestock = null;
            foreach (KeyValuePair<string, CCS_LivestockInstance> entry in livestockById)
            {
                CCS_LivestockInstance livestock = entry.Value;
                if (livestock == null
                    || livestock.LivestockType != livestockType
                    || livestock.State == CCS_LivestockState.Unavailable
                    || !string.IsNullOrWhiteSpace(livestock.AssignedStructureInstanceId))
                {
                    continue;
                }

                nearestLivestock = livestock;
                break;
            }

            CCS_RanchStructureInstance nearestStructure = null;
            foreach (KeyValuePair<string, CCS_RanchStructureInstance> entry in structuresById)
            {
                CCS_RanchStructureInstance structure = entry.Value;
                if (structure == null || structure.StructureKind != structureKind)
                {
                    continue;
                }

                nearestStructure = structure;
                break;
            }

            if (nearestLivestock == null || nearestStructure == null)
            {
                return false;
            }

            return TryAssignLivestockToStructure(nearestLivestock.InstanceId, nearestStructure.InstanceId);
        }

        public bool TryCollectProduction(string livestockInstanceId)
        {
            if (!TryGetLivestock(livestockInstanceId, out CCS_LivestockInstance livestock)
                || livestock.State != CCS_LivestockState.ReadyToCollect
                || inventoryService == null
                || !inventoryService.IsInitialized)
            {
                return false;
            }

            CCS_ItemDefinition itemDefinition = livestock.Definition?.ProductionItem;
            if (itemDefinition == null)
            {
                return false;
            }

            int added = inventoryService.AddItem(itemDefinition, livestock.LastProducedQuantity);
            if (added < livestock.LastProducedQuantity)
            {
                return false;
            }
            livestock.ProductionElapsedSeconds = 0f;
            livestock.State = CCS_LivestockState.Assigned;
            UpdateProductionState(livestock);
            LivestockProductCollected?.Invoke(livestock);
            return true;
        }

        public bool TryCollectFirstReadyProduction()
        {
            foreach (KeyValuePair<string, CCS_LivestockInstance> entry in livestockById)
            {
                CCS_LivestockInstance livestock = entry.Value;
                if (livestock != null
                    && livestock.State == CCS_LivestockState.ReadyToCollect
                    && TryCollectProduction(livestock.InstanceId))
                {
                    return true;
                }
            }

            return false;
        }

        public bool TryForceProductionForPlaytest(string livestockInstanceId)
        {
            if (!TryGetLivestock(livestockInstanceId, out CCS_LivestockInstance livestock)
                || livestock.Definition == null)
            {
                return false;
            }

            UpdateProductionState(livestock);
            if (livestock.State != CCS_LivestockState.Producing && livestock.State != CCS_LivestockState.Assigned)
            {
                return false;
            }

            livestock.State = CCS_LivestockState.ReadyToCollect;
            livestock.LastProducedItemId = livestock.Definition.ProductionItemId;
            livestock.LastProducedQuantity = livestock.Definition.ProductionQuantity;
            LivestockProductionReady?.Invoke(livestock);
            return true;
        }

        public bool TryForceFirstLivestockProductionForPlaytest()
        {
            foreach (KeyValuePair<string, CCS_LivestockInstance> entry in livestockById)
            {
                if (entry.Value != null && TryForceProductionForPlaytest(entry.Value.InstanceId))
                {
                    return true;
                }
            }

            return false;
        }

        public bool HasCampContributingStructureInRadius(Vector3 origin, float radius)
        {
            foreach (KeyValuePair<string, CCS_RanchStructureInstance> entry in structuresById)
            {
                CCS_RanchStructureInstance structure = entry.Value;
                if (structure?.Definition != null
                    && structure.Definition.ContributesToCampTier
                    && Vector3.Distance(origin, structure.WorldPosition) <= radius)
                {
                    return true;
                }
            }

            return false;
        }

        public bool HasRanchStructureInRadius(Vector3 origin, float radius, CCS_RanchStructureKind structureKind)
        {
            foreach (KeyValuePair<string, CCS_RanchStructureInstance> entry in structuresById)
            {
                CCS_RanchStructureInstance structure = entry.Value;
                if (structure != null
                    && structure.StructureKind == structureKind
                    && Vector3.Distance(origin, structure.WorldPosition) <= radius)
                {
                    return true;
                }
            }

            return false;
        }

        public bool TryGetLivestock(string instanceId, out CCS_LivestockInstance livestock)
        {
            livestock = null;
            return !string.IsNullOrWhiteSpace(instanceId)
                && livestockById.TryGetValue(instanceId, out livestock)
                && livestock != null;
        }

        public bool TryGetStructure(string instanceId, out CCS_RanchStructureInstance structure)
        {
            structure = null;
            return !string.IsNullOrWhiteSpace(instanceId)
                && structuresById.TryGetValue(instanceId, out structure)
                && structure != null;
        }

        public int GetOwnedLivestockCount(CCS_LivestockType livestockType)
        {
            int count = 0;
            foreach (KeyValuePair<string, CCS_LivestockInstance> entry in livestockById)
            {
                if (entry.Value != null && entry.Value.LivestockType == livestockType)
                {
                    count++;
                }
            }

            return count;
        }

        public CCS_LivestockSnapshot[] CaptureLivestockState()
        {
            if (livestockById.Count == 0)
            {
                return Array.Empty<CCS_LivestockSnapshot>();
            }

            CCS_LivestockSnapshot[] snapshots = new CCS_LivestockSnapshot[livestockById.Count];
            int writeIndex = 0;
            foreach (KeyValuePair<string, CCS_LivestockInstance> entry in livestockById)
            {
                if (entry.Value != null)
                {
                    snapshots[writeIndex++] = entry.Value.ToSnapshot();
                }
            }

            if (writeIndex == snapshots.Length)
            {
                return snapshots;
            }

            CCS_LivestockSnapshot[] trimmed = new CCS_LivestockSnapshot[writeIndex];
            Array.Copy(snapshots, trimmed, writeIndex);
            return trimmed;
        }

        public CCS_RanchStructureSnapshot[] CaptureStructureState()
        {
            if (structuresById.Count == 0)
            {
                return Array.Empty<CCS_RanchStructureSnapshot>();
            }

            CCS_RanchStructureSnapshot[] snapshots = new CCS_RanchStructureSnapshot[structuresById.Count];
            int writeIndex = 0;
            foreach (KeyValuePair<string, CCS_RanchStructureInstance> entry in structuresById)
            {
                if (entry.Value != null)
                {
                    snapshots[writeIndex++] = entry.Value.ToSnapshot();
                }
            }

            if (writeIndex == snapshots.Length)
            {
                return snapshots;
            }

            CCS_RanchStructureSnapshot[] trimmed = new CCS_RanchStructureSnapshot[writeIndex];
            Array.Copy(snapshots, trimmed, writeIndex);
            return trimmed;
        }

        public void RestoreState(CCS_LivestockSnapshot[] livestockStates, CCS_RanchStructureSnapshot[] structureStates)
        {
            ClearRuntimeState();
            if (structureStates != null)
            {
                for (int index = 0; index < structureStates.Length; index++)
                {
                    CCS_RanchStructureSnapshot snapshot = structureStates[index];
                    if (snapshot == null
                        || string.IsNullOrWhiteSpace(snapshot.structureDefinitionId)
                        || !structureDefinitionLookup.TryGetValue(
                            snapshot.structureDefinitionId,
                            out CCS_RanchStructureDefinition definition))
                    {
                        continue;
                    }

                    CCS_RanchStructureInstance instance = CCS_RanchStructureInstance.FromSnapshot(snapshot, definition);
                    SpawnStructureWorldObject(instance);
                    structuresById[instance.InstanceId] = instance;
                }
            }

            if (livestockStates != null)
            {
                for (int index = 0; index < livestockStates.Length; index++)
                {
                    CCS_LivestockSnapshot snapshot = livestockStates[index];
                    if (snapshot == null
                        || string.IsNullOrWhiteSpace(snapshot.livestockDefinitionId)
                        || !livestockDefinitionLookup.TryGetValue(
                            snapshot.livestockDefinitionId,
                            out CCS_LivestockDefinition definition))
                    {
                        continue;
                    }

                    CCS_LivestockInstance instance = CCS_LivestockInstance.FromSnapshot(snapshot, definition);
                    SpawnLivestockWorldObject(instance);
                    livestockById[instance.InstanceId] = instance;
                }
            }

            campService?.RecalculateCamp();
        }

        private void UpdateProductionState(CCS_LivestockInstance livestock)
        {
            if (livestock?.Definition == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(livestock.AssignedStructureInstanceId)
                || !TryGetStructure(livestock.AssignedStructureInstanceId, out CCS_RanchStructureInstance structure)
                || structure.StructureKind != livestock.Definition.RequiredStructureKind)
            {
                livestock.State = string.IsNullOrWhiteSpace(livestock.AssignedStructureInstanceId)
                    ? CCS_LivestockState.Idle
                    : CCS_LivestockState.Unavailable;
                return;
            }

            if (livestock.State == CCS_LivestockState.ReadyToCollect)
            {
                return;
            }

            Vector3 origin = structure.WorldPosition;
            float radius = livestock.Definition.SupportStructureProximityRadius;
            if (livestock.Definition.RequiresFeed
                && !HasRanchStructureInRadius(origin, radius, CCS_RanchStructureKind.FeedTrough))
            {
                livestock.State = CCS_LivestockState.Assigned;
                return;
            }

            if (livestock.Definition.RequiresWater
                && !HasRanchStructureInRadius(origin, radius, CCS_RanchStructureKind.WaterTrough))
            {
                livestock.State = CCS_LivestockState.Assigned;
                return;
            }

            livestock.State = CCS_LivestockState.Producing;
        }

        private CCS_RanchPlacementResult UpdatePlacementPreview(CCS_RanchPlacementRequest request)
        {
            if (!activeProfile.TryGetStructureById(request.StructureDefinitionId, out CCS_RanchStructureDefinition definition))
            {
                return CCS_RanchPlacementResult.Failure("Unknown ranch structure definition.");
            }

            pendingStructureDefinition = definition;
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
                return CCS_RanchPlacementResult.Preview(false, validationMessage);
            }

            pendingPreviewPosition = position;
            pendingPreviewRotationY = rotationY;
            pendingPreviewValid = isValid;
            EnsurePlacementPreview(definition, position, rotationY, isValid);
            return CCS_RanchPlacementResult.Preview(isValid, validationMessage);
        }

        private CCS_RanchPlacementResult TryConfirmPlacement()
        {
            if (!isPlacementModeActive || !pendingPreviewValid || pendingStructureDefinition == null)
            {
                return CCS_RanchPlacementResult.Failure("Ranch structure placement mode is not active.");
            }

            if (inventoryService == null
                || pendingStructureDefinition.PlaceableKitItem == null
                || inventoryService.RemoveItem(pendingStructureDefinition.PlaceableKitItem, 1) <= 0)
            {
                return CCS_RanchPlacementResult.Failure("Ranch structure kit is not available in inventory.");
            }

            CCS_RanchStructureInstance instance = SpawnStructureInstance(
                pendingStructureDefinition,
                pendingPreviewPosition,
                pendingPreviewRotationY,
                GenerateInstanceId(pendingStructureDefinition.StructureDefinitionId),
                CCS_RanchingContentIds.CampOwnerId);
            CancelPlacementMode();
            campService?.RecalculateCamp();
            RanchStructurePlaced?.Invoke(instance);
            return CCS_RanchPlacementResult.Placed($"{pendingStructureDefinition.DisplayName} placed.");
        }

        private CCS_RanchStructureInstance SpawnStructureInstance(
            CCS_RanchStructureDefinition definition,
            Vector3 position,
            float rotationY,
            string instanceId,
            string campOwnerId)
        {
            CCS_RanchStructureInstance instance = new CCS_RanchStructureInstance(
                instanceId,
                definition,
                position,
                rotationY,
                campOwnerId);
            SpawnStructureWorldObject(instance);
            structuresById[instanceId] = instance;
            return instance;
        }

        private void SpawnStructureWorldObject(CCS_RanchStructureInstance instance)
        {
            if (instance?.Definition == null)
            {
                return;
            }

            GameObject structureObject = GameObject.CreatePrimitive(instance.Definition.PlacementPrimitive);
            structureObject.name = $"CCS_RanchStructure_{instance.InstanceId}";
            structureObject.transform.SetPositionAndRotation(
                instance.WorldPosition,
                Quaternion.Euler(0f, instance.RotationY, 0f));
            structureObject.transform.localScale = instance.Definition.PlacedLocalScale;
            instance.WorldObject = structureObject;
        }

        private void SpawnLivestockWorldObject(CCS_LivestockInstance instance)
        {
            if (instance?.Definition == null)
            {
                return;
            }

            GameObject livestockObject;
            if (instance.Definition.WorldPrefab != null)
            {
                livestockObject = Object.Instantiate(instance.Definition.WorldPrefab, instance.WorldPosition, Quaternion.identity);
            }
            else
            {
                livestockObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                livestockObject.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
                livestockObject.transform.position = instance.WorldPosition;
            }

            livestockObject.name = $"CCS_Livestock_{instance.InstanceId}";
            instance.WorldObject = livestockObject;
        }

        private void UpdateLivestockTransform(CCS_LivestockInstance livestock)
        {
            if (livestock?.WorldObject != null)
            {
                livestock.WorldObject.transform.position = livestock.WorldPosition;
            }
        }

        private bool TryResolvePlacementPose(
            CCS_RanchStructureDefinition definition,
            Vector3 useOrigin,
            Vector3 useDirection,
            out Vector3 position,
            out float rotationY,
            out bool isValid,
            out string validationMessage)
        {
            position = useOrigin;
            rotationY = 0f;
            isValid = false;
            validationMessage = "Invalid placement.";

            if (definition == null)
            {
                validationMessage = "Missing ranch structure definition.";
                return false;
            }

            Vector3 target = useOrigin + useDirection * definition.PlacementForwardDistance;
            if (!Physics.Raycast(
                    target + Vector3.up * definition.PlacementMaxGroundRayDistance,
                    Vector3.down,
                    out RaycastHit hit,
                    definition.PlacementMaxGroundRayDistance * 2f,
                    Physics.DefaultRaycastLayers,
                    QueryTriggerInteraction.Ignore))
            {
                validationMessage = "No valid ground found for ranch structure.";
                return true;
            }

            float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
            if (slopeAngle > definition.PlacementMaxSlopeAngle)
            {
                validationMessage = "Ground slope is too steep for ranch structure.";
                return true;
            }

            position = hit.point;
            rotationY = Mathf.Atan2(useDirection.x, useDirection.z) * Mathf.Rad2Deg;
            isValid = true;
            validationMessage = "Ranch structure placement valid.";
            return true;
        }

        private void EnsurePlacementPreview(
            CCS_RanchStructureDefinition definition,
            Vector3 position,
            float rotationY,
            bool isValid)
        {
            if (placementPreviewObject == null)
            {
                placementPreviewObject = GameObject.CreatePrimitive(definition.PlacementPrimitive);
                Collider collider = placementPreviewObject.GetComponent<Collider>();
                if (collider != null)
                {
                    collider.enabled = false;
                }
            }

            placementPreviewObject.name = "CCS_RanchStructurePreview";
            placementPreviewObject.transform.SetPositionAndRotation(
                position,
                Quaternion.Euler(0f, rotationY, 0f));
            placementPreviewObject.transform.localScale = definition.PlacedLocalScale;
            placementPreviewObject.SetActive(true);

            Renderer renderer = placementPreviewObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = isValid ? new Color(0.2f, 0.85f, 0.3f, 0.55f) : new Color(0.85f, 0.2f, 0.2f, 0.55f);
            }
        }

        private void EnsurePlacementPreviewHidden()
        {
            if (placementPreviewObject != null)
            {
                placementPreviewObject.SetActive(false);
            }
        }

        private void ClearRuntimeState()
        {
            foreach (KeyValuePair<string, CCS_LivestockInstance> entry in livestockById)
            {
                if (entry.Value?.WorldObject != null)
                {
                    Object.Destroy(entry.Value.WorldObject);
                }
            }

            foreach (KeyValuePair<string, CCS_RanchStructureInstance> entry in structuresById)
            {
                if (entry.Value?.WorldObject != null)
                {
                    Object.Destroy(entry.Value.WorldObject);
                }
            }

            livestockById.Clear();
            structuresById.Clear();
            CancelPlacementMode();
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
