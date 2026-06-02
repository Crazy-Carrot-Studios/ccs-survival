using System;
using System.Collections.Generic;
using CCS.Core;
using CCS.Modules.Industry;
using CCS.Modules.Inventory;
using CCS.Survival;
using UnityEngine;

namespace CCS.Modules.Shelter
{
    public sealed class CCS_FrontierHomesteadStructureService : CCS_ISurvivalService, CCS_IUpdatable
    {
        private readonly Dictionary<string, CCS_FrontierWorkbenchInstance> placedWorkbenches =
            new Dictionary<string, CCS_FrontierWorkbenchInstance>(StringComparer.Ordinal);

        private CCS_CampDefinition activeProfile;
        private CCS_CampService campService;
        private CCS_IndustryService industryService;
        private CCS_PlayerInventoryService inventoryService;
        private CCS_FrontierShelterPlacementPreview placementPreview;
        private CCS_WorkbenchDefinition pendingWorkbenchDefinition;
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

        public void InitializeFromProfile(CCS_CampDefinition profile)
        {
            activeProfile = profile;
            isInitialized = profile != null;
        }

        public void BindCampService(CCS_CampService service)
        {
            campService = service;
        }

        public void BindIndustryService(CCS_IndustryService service)
        {
            industryService = service;
        }

        public void BindInventoryService(CCS_PlayerInventoryService service)
        {
            inventoryService = service;
        }

        public void Tick(float deltaTime)
        {
        }

        public bool TryResolveWorkbenchDefinitionForItem(
            CCS_ItemDefinition itemDefinition,
            out CCS_WorkbenchDefinition workbenchDefinition)
        {
            workbenchDefinition = null;
            if (!EnsureReady() || itemDefinition == null)
            {
                return false;
            }

            return activeProfile.TryGetWorkbenchByPlaceableItemId(itemDefinition.ItemId, out workbenchDefinition)
                && workbenchDefinition.ContributesToCampTier;
        }

        public CCS_FrontierHomesteadPlacementResult HandlePlacementRequest(CCS_FrontierHomesteadPlacementRequest request)
        {
            if (!EnsureReady() || request == null)
            {
                return CCS_FrontierHomesteadPlacementResult.Failure("Homestead structure service is unavailable.");
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
            pendingWorkbenchDefinition = null;
            pendingPreviewValid = false;
            EnsurePlacementPreviewHidden();
        }

        public bool HasWorkbenchInRadius(Vector3 origin, float radius)
        {
            return HasCampStructureInRadius(origin, radius, CCS_CampStructureKind.WorkArea);
        }

        public bool HasCampStructureInRadius(Vector3 origin, float radius, CCS_CampStructureKind structureKind)
        {
            foreach (KeyValuePair<string, CCS_FrontierWorkbenchInstance> entry in placedWorkbenches)
            {
                CCS_FrontierWorkbenchInstance workbench = entry.Value;
                CCS_WorkbenchDefinition definition = workbench?.WorkbenchDefinition;
                if (workbench == null
                    || definition == null
                    || !definition.ContributesToCampTier
                    || definition.CampStructureKind != structureKind)
                {
                    continue;
                }

                if (Vector3.Distance(origin, workbench.WorldPosition) <= radius)
                {
                    return true;
                }
            }

            return false;
        }

        public bool HasIndustryWorkstationRoleInRadius(Vector3 origin, float radius, string workstationRoleId)
        {
            if (string.IsNullOrWhiteSpace(workstationRoleId))
            {
                return false;
            }

            foreach (KeyValuePair<string, CCS_FrontierWorkbenchInstance> entry in placedWorkbenches)
            {
                CCS_FrontierWorkbenchInstance workbench = entry.Value;
                CCS_WorkbenchDefinition definition = workbench?.WorkbenchDefinition;
                if (workbench == null
                    || definition == null
                    || !string.Equals(
                        definition.IndustryWorkstationRoleId,
                        workstationRoleId,
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (Vector3.Distance(origin, workbench.WorldPosition) <= radius)
                {
                    return true;
                }
            }

            return false;
        }

        public CCS_FrontierWorkbenchInstanceSaveState[] CaptureWorkbenchWorldState()
        {
            if (placedWorkbenches.Count == 0)
            {
                return Array.Empty<CCS_FrontierWorkbenchInstanceSaveState>();
            }

            List<CCS_FrontierWorkbenchInstanceSaveState> records =
                new List<CCS_FrontierWorkbenchInstanceSaveState>(placedWorkbenches.Count);
            foreach (KeyValuePair<string, CCS_FrontierWorkbenchInstance> entry in placedWorkbenches)
            {
                if (entry.Value != null)
                {
                    records.Add(entry.Value.CaptureState());
                }
            }

            return records.ToArray();
        }

        public void RestoreWorkbenchWorldState(CCS_FrontierWorkbenchInstanceSaveState[] saveStates)
        {
            ClearWorkbenches();
            if (saveStates == null || saveStates.Length == 0)
            {
                return;
            }

            for (int index = 0; index < saveStates.Length; index++)
            {
                CCS_FrontierWorkbenchInstanceSaveState saveState = saveStates[index];
                if (saveState == null
                    || string.IsNullOrWhiteSpace(saveState.WorkbenchDefinitionId)
                    || !activeProfile.TryGetWorkbenchById(saveState.WorkbenchDefinitionId, out CCS_WorkbenchDefinition definition))
                {
                    continue;
                }

                SpawnWorkbenchInstance(
                    definition,
                    saveState.Position,
                    Quaternion.Euler(0f, saveState.RotationY, 0f),
                    saveState.InstanceId,
                    saveState.CampOwnerId);
            }
        }

        public void UnregisterWorkbenchInstance(CCS_FrontierWorkbenchInstance instance)
        {
            if (instance == null || string.IsNullOrWhiteSpace(instance.InstanceId))
            {
                return;
            }

            placedWorkbenches.Remove(instance.InstanceId);
            campService?.RecalculateCamp();
        }

        private CCS_FrontierHomesteadPlacementResult UpdatePlacementPreview(CCS_FrontierHomesteadPlacementRequest request)
        {
            if (!activeProfile.TryGetWorkbenchById(request.DefinitionId, out CCS_WorkbenchDefinition definition))
            {
                return CCS_FrontierHomesteadPlacementResult.Failure("Unknown workbench definition.");
            }

            pendingWorkbenchDefinition = definition;
            isPlacementModeActive = true;

            if (!TryResolvePlacementPose(
                    definition,
                    request.UseOrigin,
                    request.UseDirection,
                    out Vector3 position,
                    out Quaternion rotation,
                    out bool isValid,
                    out string validationMessage))
            {
                return CCS_FrontierHomesteadPlacementResult.Preview(false, validationMessage);
            }

            pendingPreviewPosition = position;
            pendingPreviewRotation = rotation;
            pendingPreviewValid = isValid;
            EnsurePlacementPreview(definition, position, rotation, isValid);
            return CCS_FrontierHomesteadPlacementResult.Preview(isValid, validationMessage);
        }

        private CCS_FrontierHomesteadPlacementResult TryConfirmPlacement()
        {
            if (!isPlacementModeActive || !pendingPreviewValid || pendingWorkbenchDefinition == null)
            {
                return CCS_FrontierHomesteadPlacementResult.Failure("Workbench placement mode is not active.");
            }

            if (inventoryService == null
                || pendingWorkbenchDefinition.PlaceableKitItem == null
                || inventoryService.RemoveItem(pendingWorkbenchDefinition.PlaceableKitItem, 1) <= 0)
            {
                return CCS_FrontierHomesteadPlacementResult.Failure("Workbench kit is not available in inventory.");
            }

            SpawnWorkbenchInstance(
                pendingWorkbenchDefinition,
                pendingPreviewPosition,
                pendingPreviewRotation,
                BuildInstanceId(pendingWorkbenchDefinition.WorkbenchDefinitionId),
                "ccs.survival.camp.player");
            CancelPlacementMode();
            campService?.RecalculateCamp();
            return CCS_FrontierHomesteadPlacementResult.Placed("Frontier workbench placed.");
        }

        private CCS_FrontierWorkbenchInstance SpawnWorkbenchInstance(
            CCS_WorkbenchDefinition definition,
            Vector3 position,
            Quaternion rotation,
            string instanceId,
            string campOwnerId)
        {
            GameObject workbenchObject = GameObject.CreatePrimitive(definition.PlacementPrimitive);
            workbenchObject.name = $"CCS_Workbench_{instanceId}";
            workbenchObject.transform.SetPositionAndRotation(position, rotation);
            workbenchObject.transform.localScale = definition.PlacedLocalScale;

            Collider collider = workbenchObject.GetComponent<Collider>();
            if (collider != null)
            {
                collider.isTrigger = false;
            }

            CCS_FrontierWorkbenchInstance instance = workbenchObject.AddComponent<CCS_FrontierWorkbenchInstance>();
            instance.Initialize(this, definition, instanceId, campOwnerId);
            placedWorkbenches[instanceId] = instance;

            if (industryService != null
                && industryService.IsInitialized
                && CCS_IndustryWorkstationRole.IsIndustryRole(definition.IndustryWorkstationRoleId))
            {
                CCS_IndustryWorkstation industryWorkstation = workbenchObject.AddComponent<CCS_IndustryWorkstation>();
                industryWorkstation.Initialize(
                    industryService,
                    instanceId,
                    definition.IndustryWorkstationRoleId,
                    campOwnerId);
            }

            return instance;
        }

        private void ClearWorkbenches()
        {
            foreach (KeyValuePair<string, CCS_FrontierWorkbenchInstance> entry in placedWorkbenches)
            {
                if (entry.Value != null)
                {
                    UnityEngine.Object.Destroy(entry.Value.gameObject);
                }
            }

            placedWorkbenches.Clear();
        }

        private bool TryResolvePlacementPose(
            CCS_WorkbenchDefinition definition,
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
                message = "Ground slope is too steep for workbench placement.";
                return false;
            }

            isValid = true;
            message = "Valid workbench placement.";
            return true;
        }

        private void EnsurePlacementPreview(
            CCS_WorkbenchDefinition definition,
            Vector3 position,
            Quaternion rotation,
            bool isValid)
        {
            if (placementPreview == null)
            {
                GameObject previewRoot = new GameObject("CCS_FrontierWorkbenchPlacementPreviewRoot");
                placementPreview = previewRoot.AddComponent<CCS_FrontierShelterPlacementPreview>();
            }

            placementPreview.EnsurePreviewObject(definition.PlacementPrimitive);
            placementPreview.UpdatePreview(
                position,
                rotation,
                definition.PlacedLocalScale,
                isValid,
                new Color(0.35f, 0.65f, 0.95f, 0.55f),
                new Color(0.9f, 0.25f, 0.2f, 0.55f));
        }

        private void EnsurePlacementPreviewHidden()
        {
            placementPreview?.SetVisible(false);
        }

        private static string BuildInstanceId(string definitionId)
        {
            return $"{definitionId}.{Guid.NewGuid():N}";
        }

        private bool EnsureReady()
        {
            return isInitialized && activeProfile != null;
        }
    }
}
