using System;
using System.Collections.Generic;
using CCS.Core;
using CCS.Modules.Inventory;
using CCS.Modules.Wildlife;
using CCS.Modules.WorldResources;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_TrapService
// CATEGORY: Modules / Trapping / Runtime / Services
// PURPOSE: Trap placement, timer capture rolls, harvest delegation, and save restore.
// PLACEMENT: Registered by CCS_SurvivalGameplayServiceRegistration from trap profile.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// =============================================================================

namespace CCS.Modules.Trapping
{
    public sealed class CCS_TrapService : CCS_ISurvivalService, CCS_IUpdatable
    {
        private const string LogPrefix = "[CCS_TrapService]";

        #region Variables

        private readonly Dictionary<string, CCS_TrapInstance> registeredTraps =
            new Dictionary<string, CCS_TrapInstance>(StringComparer.Ordinal);

        private readonly List<CCS_TrapInstance> tickList = new List<CCS_TrapInstance>();

        private CCS_TrapProfile activeProfile;
        private CCS_PlayerInventoryService inventoryService;
        private CCS_WildlifeHarvestService wildlifeHarvestService;
        private CCS_TrapPlacementPreview placementPreview;
        private CCS_TrapDefinition pendingTrapDefinition;
        private CCS_ItemDefinition pendingPlaceableItem;
        private Vector3 pendingPreviewPosition;
        private Quaternion pendingPreviewRotation = Quaternion.identity;
        private bool pendingPreviewValid;
        private bool isPlacementModeActive;
        private int nextInstanceSequence = 1;
        private bool isInitialized;

        #endregion

        #region Events

        public event TrapPlacedHandler TrapPlaced;
        public event TrapTriggeredHandler TrapTriggered;
        public event TrapHarvestedHandler TrapHarvested;
        public event TrapBrokenHandler TrapBroken;

        #endregion

        #region Properties

        public bool IsInitialized => isInitialized;

        public bool IsPlacementModeActive => isPlacementModeActive;

        public CCS_TrapProfile ActiveProfile => activeProfile;

        public int RegisteredTrapCount => registeredTraps.Count;

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

        public void InitializeFromProfile(CCS_TrapProfile profile)
        {
            if (profile == null)
            {
                Debug.LogWarning($"{LogPrefix} InitializeFromProfile called with null profile.");
                isInitialized = false;
                return;
            }

            CCS_SurvivalValidationResult validation = CCS_TrapValidationUtility.ValidateProfile(profile);
            if (!validation.IsSuccess)
            {
                Debug.LogWarning($"{LogPrefix} Profile validation warning: {validation.Message}");
            }

            activeProfile = profile;
            isInitialized = true;
        }

        public void BindInventoryService(CCS_PlayerInventoryService service)
        {
            inventoryService = service;
        }

        public void BindWildlifeHarvestService(CCS_WildlifeHarvestService service)
        {
            wildlifeHarvestService = service;
        }

        public void Tick(float deltaTime)
        {
            if (!EnsureReady() || !activeProfile.EnableTrapping)
            {
                return;
            }

            tickList.Clear();
            tickList.AddRange(registeredTraps.Values);

            for (int index = 0; index < tickList.Count; index++)
            {
                CCS_TrapInstance trapInstance = tickList[index];
                if (trapInstance == null)
                {
                    continue;
                }

                if (trapInstance.TrapState == CCS_TrapState.Armed)
                {
                    trapInstance.TickTimer(deltaTime);
                    if (trapInstance.IsTimerReady)
                    {
                        ResolveTimerCapture(trapInstance);
                    }
                }
            }
        }

        public bool TryResolveTrapDefinitionForItem(
            CCS_ItemDefinition itemDefinition,
            out CCS_TrapDefinition trapDefinition)
        {
            trapDefinition = null;
            if (!EnsureReady() || itemDefinition == null)
            {
                return false;
            }

            return activeProfile.TryGetByPlaceableItemId(itemDefinition.ItemId, out trapDefinition);
        }

        public CCS_TrapPlacementResult HandlePlacementRequest(CCS_TrapPlacementRequest request)
        {
            if (!EnsureReady() || request?.TrapDefinition == null)
            {
                return CCS_TrapPlacementResult.Failure("Trap service or definition is unavailable.");
            }

            if (!activeProfile.EnableTrapping)
            {
                return CCS_TrapPlacementResult.Failure("Trapping is disabled.");
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
            pendingTrapDefinition = null;
            pendingPlaceableItem = null;
            pendingPreviewValid = false;
            EnsurePlacementPreviewHidden();
        }

        public CCS_TrapResult TryHarvestTrap(CCS_TrapInstance trapInstance, CCS_RequiredToolType equippedToolType)
        {
            if (!EnsureReady() || trapInstance == null)
            {
                return CCS_TrapResult.Failure(CCS_TrapResultType.ServiceUnavailable, "Trap service unavailable.");
            }

            if (trapInstance.TrapState != CCS_TrapState.Triggered)
            {
                return CCS_TrapResult.Failure(
                    CCS_TrapResultType.TargetUnavailable,
                    "Trap has no capture ready to harvest.");
            }

            if (equippedToolType != CCS_RequiredToolType.Knife)
            {
                return CCS_TrapResult.Failure(CCS_TrapResultType.WrongTool, "Knife required to harvest trap catch.");
            }

            if (wildlifeHarvestService == null || !wildlifeHarvestService.IsInitialized)
            {
                return CCS_TrapResult.Failure(
                    CCS_TrapResultType.ServiceUnavailable,
                    "Wildlife harvest service unavailable.");
            }

            if (inventoryService == null || !inventoryService.IsInitialized)
            {
                return CCS_TrapResult.Failure(
                    CCS_TrapResultType.ServiceUnavailable,
                    "Inventory service unavailable.");
            }

            CCS_WildlifeDefinition wildlifeDefinition = trapInstance.TrapDefinition?.CapturedWildlifeDefinition;
            if (wildlifeDefinition == null)
            {
                return CCS_TrapResult.Failure(
                    CCS_TrapResultType.TargetUnavailable,
                    "Trap has no wildlife harvest definition.");
            }

            CCS_WildlifeState wildlifeState = CCS_WildlifeState.CreateFromDefinition(wildlifeDefinition);
            CCS_WildlifeHarvestRequest harvestRequest = new CCS_WildlifeHarvestRequest(
                wildlifeDefinition,
                wildlifeState,
                equippedToolType,
                trapInstance.InstanceId,
                isDeadCarcass: true);

            CCS_WildlifeHarvestResult harvestResult =
                wildlifeHarvestService.TryHarvest(harvestRequest, inventoryService);

            if (!harvestResult.IsSuccess)
            {
                CCS_TrapResultType mapped = harvestResult.ResultType == CCS_WildlifeHarvestResultType.WrongTool
                    ? CCS_TrapResultType.WrongTool
                    : harvestResult.ResultType == CCS_WildlifeHarvestResultType.InventoryFull
                        ? CCS_TrapResultType.InventoryFull
                        : CCS_TrapResultType.Failed;

                return CCS_TrapResult.Failure(mapped, harvestResult.Message);
            }

            trapInstance.SetTrapState(CCS_TrapState.Harvested);
            CCS_TrapResult success = CCS_TrapResult.Success(harvestResult.Message, trapInstance.InstanceId);
            TrapHarvested?.Invoke(new CCS_TrapEventArgs(trapInstance, success, true, harvestResult.Message));
            return success;
        }

        public CCS_TrapResult TryForceTriggerForPlaytest(string instanceId)
        {
            if (!TryGetTrapInstance(instanceId, out CCS_TrapInstance trapInstance))
            {
                if (registeredTraps.Count == 0)
                {
                    return CCS_TrapResult.Failure(CCS_TrapResultType.TargetUnavailable, "No traps placed.");
                }

                foreach (KeyValuePair<string, CCS_TrapInstance> entry in registeredTraps)
                {
                    trapInstance = entry.Value;
                    break;
                }
            }

            if (trapInstance == null || trapInstance.TrapState != CCS_TrapState.Armed)
            {
                return CCS_TrapResult.Failure(
                    CCS_TrapResultType.TargetUnavailable,
                    "No armed trap available to force trigger.");
            }

            trapInstance.SetTrapState(CCS_TrapState.Armed, 0f);
            return ResolveTimerCapture(trapInstance, forceCaptureRoll: true);
        }

        public void RegisterTrapInstance(CCS_TrapInstance trapInstance)
        {
            if (trapInstance == null || string.IsNullOrWhiteSpace(trapInstance.InstanceId))
            {
                return;
            }

            registeredTraps[trapInstance.InstanceId] = trapInstance;
        }

        public void UnregisterTrapInstance(CCS_TrapInstance trapInstance)
        {
            if (trapInstance == null || string.IsNullOrWhiteSpace(trapInstance.InstanceId))
            {
                return;
            }

            registeredTraps.Remove(trapInstance.InstanceId);
        }

        public bool TryGetTrapInstance(string instanceId, out CCS_TrapInstance trapInstance)
        {
            return registeredTraps.TryGetValue(instanceId, out trapInstance);
        }

        public CCS_TrapInstanceSaveState[] CaptureWorldState()
        {
            if (registeredTraps.Count == 0)
            {
                return Array.Empty<CCS_TrapInstanceSaveState>();
            }

            List<CCS_TrapInstanceSaveState> records = new List<CCS_TrapInstanceSaveState>(registeredTraps.Count);
            foreach (KeyValuePair<string, CCS_TrapInstance> entry in registeredTraps)
            {
                if (entry.Value != null)
                {
                    records.Add(entry.Value.CaptureState());
                }
            }

            return records.ToArray();
        }

        public void RestoreWorldState(CCS_TrapInstanceSaveState[] saveStates)
        {
            ClearSpawnedTraps();

            if (saveStates == null || saveStates.Length == 0)
            {
                return;
            }

            for (int index = 0; index < saveStates.Length; index++)
            {
                CCS_TrapInstanceSaveState saveState = saveStates[index];
                if (saveState == null || string.IsNullOrWhiteSpace(saveState.trapDefinitionId))
                {
                    continue;
                }

                if (!activeProfile.TryGetByTrapId(saveState.trapDefinitionId, out CCS_TrapDefinition definition))
                {
                    continue;
                }

                SpawnTrapInstance(
                    definition,
                    new Vector3(saveState.positionX, saveState.positionY, saveState.positionZ),
                    Quaternion.Euler(0f, saveState.rotationY, 0f),
                    saveState.instanceId,
                    applySaveState: saveState);
            }
        }

        #endregion

        #region Private Methods

        private CCS_TrapPlacementResult UpdatePlacementPreview(CCS_TrapPlacementRequest request)
        {
            pendingTrapDefinition = request.TrapDefinition;
            pendingPlaceableItem = request.TrapDefinition.PlaceableItem;
            isPlacementModeActive = true;

            if (!TryResolvePlacementPose(
                    request.TrapDefinition,
                    request.Origin,
                    request.Forward,
                    out Vector3 position,
                    out Quaternion rotation,
                    out bool isValid,
                    out string validationMessage))
            {
                pendingPreviewValid = false;
                EnsurePlacementPreview(request.TrapDefinition, position, rotation, false);
                return CCS_TrapPlacementResult.Preview(position, false, validationMessage);
            }

            pendingPreviewPosition = position;
            pendingPreviewRotation = rotation;
            pendingPreviewValid = isValid;
            EnsurePlacementPreview(request.TrapDefinition, position, rotation, isValid);
            return CCS_TrapPlacementResult.Preview(
                position,
                isValid,
                isValid ? "Trap placement preview updated. Use again to confirm." : validationMessage);
        }

        private CCS_TrapPlacementResult TryConfirmPlacement()
        {
            if (!isPlacementModeActive || pendingTrapDefinition == null)
            {
                return CCS_TrapPlacementResult.Failure("Trap placement mode is not active.");
            }

            if (!pendingPreviewValid)
            {
                return CCS_TrapPlacementResult.Failure("Trap placement position is invalid.");
            }

            if (inventoryService == null
                || !inventoryService.IsInitialized
                || pendingPlaceableItem == null
                || inventoryService.GetQuantity(pendingPlaceableItem) <= 0)
            {
                return CCS_TrapPlacementResult.Failure("Trap item is not available in inventory.");
            }

            int removed = inventoryService.RemoveItem(pendingPlaceableItem, 1);
            if (removed < 1)
            {
                return CCS_TrapPlacementResult.Failure("Failed to consume trap item from inventory.");
            }

            string instanceId = BuildInstanceId(pendingTrapDefinition);
            CCS_TrapInstance trapInstance = SpawnTrapInstance(
                pendingTrapDefinition,
                pendingPreviewPosition,
                pendingPreviewRotation,
                instanceId,
                applySaveState: null);

            if (trapInstance == null)
            {
                inventoryService.AddItem(pendingPlaceableItem, 1);
                return CCS_TrapPlacementResult.Failure("Failed to spawn trap instance.");
            }

            float delay = pendingTrapDefinition.TriggerDelaySeconds;
            trapInstance.SetTrapState(CCS_TrapState.Armed, delay);

            CancelPlacementMode();

            CCS_TrapResult placedResult = CCS_TrapResult.Success("Trap placed and armed.", instanceId);
            TrapPlaced?.Invoke(new CCS_TrapEventArgs(trapInstance, placedResult, true, placedResult.Message));
            return CCS_TrapPlacementResult.Placed(instanceId, placedResult.Message);
        }

        private CCS_TrapInstance SpawnTrapInstance(
            CCS_TrapDefinition definition,
            Vector3 position,
            Quaternion rotation,
            string instanceId,
            CCS_TrapInstanceSaveState applySaveState)
        {
            GameObject trapObject = GameObject.CreatePrimitive(definition.PlacementPrimitive);
            trapObject.name = $"CCS_Trap_{instanceId}";
            trapObject.transform.SetPositionAndRotation(position, rotation);
            trapObject.transform.localScale = definition.PlacedLocalScale;

            Collider collider = trapObject.GetComponent<Collider>();
            if (collider != null)
            {
                collider.isTrigger = false;
            }

            CCS_TrapInstance trapInstance = trapObject.AddComponent<CCS_TrapInstance>();
            CCS_TrapState initialState = CCS_TrapState.Unarmed;
            float timer = 0f;
            if (applySaveState != null)
            {
                trapInstance.RestoreState(applySaveState);
                initialState = (CCS_TrapState)applySaveState.trapState;
                timer = applySaveState.remainingTimerSeconds;
            }

            trapInstance.Initialize(this, definition, instanceId, initialState, timer);
            RegisterTrapInstance(trapInstance);
            return trapInstance;
        }

        private CCS_TrapResult ResolveTimerCapture(CCS_TrapInstance trapInstance, bool forceCaptureRoll = false)
        {
            CCS_TrapDefinition definition = trapInstance.TrapDefinition;
            if (definition == null)
            {
                return CCS_TrapResult.Failure(CCS_TrapResultType.Failed, "Trap definition missing.");
            }

            float breakRoll = forceCaptureRoll ? 0f : UnityEngine.Random.value;
            if (breakRoll < definition.BreakChance)
            {
                trapInstance.SetTrapState(CCS_TrapState.Broken);
                CCS_TrapResult broken = CCS_TrapResult.Failure(CCS_TrapResultType.BrokenTrap, "Trap broke during capture attempt.");
                TrapBroken?.Invoke(new CCS_TrapEventArgs(trapInstance, broken, false, broken.Message));
                return broken;
            }

            float captureRoll = forceCaptureRoll ? 0f : UnityEngine.Random.value;
            if (!forceCaptureRoll && captureRoll > definition.CaptureChance)
            {
                trapInstance.SetTrapState(CCS_TrapState.Armed, definition.TriggerDelaySeconds);
                return CCS_TrapResult.Failure(CCS_TrapResultType.CaptureFailed, "Trap missed capture. Re-armed.");
            }

            if (!TryCaptureWildlifeNearTrap(trapInstance, definition, out string wildlifeId, out string wildlifeKey))
            {
                trapInstance.SetTrapState(CCS_TrapState.Armed, definition.TriggerDelaySeconds);
                CCS_TrapResult noWildlife = CCS_TrapResult.Failure(
                    CCS_TrapResultType.NoWildlife,
                    "No capturable wildlife near trap.");
                TrapTriggered?.Invoke(new CCS_TrapEventArgs(trapInstance, noWildlife, false, noWildlife.Message));
                return noWildlife;
            }

            trapInstance.SetCaptureData(wildlifeId, wildlifeKey);
            trapInstance.SetTrapState(CCS_TrapState.Triggered);
            CCS_TrapResult success = CCS_TrapResult.CaptureSuccess(
                "Wildlife captured in trap.",
                trapInstance.InstanceId);
            TrapTriggered?.Invoke(new CCS_TrapEventArgs(trapInstance, success, true, success.Message));
            return success;
        }

        private bool TryCaptureWildlifeNearTrap(
            CCS_TrapInstance trapInstance,
            CCS_TrapDefinition definition,
            out string wildlifeId,
            out string wildlifeKey)
        {
            wildlifeId = definition.CapturedWildlifeDefinition != null
                ? definition.CapturedWildlifeDefinition.WildlifeId
                : string.Empty;
            wildlifeKey = string.Empty;

            CCS_WildlifeAgent[] agents = UnityEngine.Object.FindObjectsByType<CCS_WildlifeAgent>();
            float bestDistance = float.MaxValue;
            CCS_WildlifeAgent bestAgent = null;

            for (int index = 0; index < agents.Length; index++)
            {
                CCS_WildlifeAgent agent = agents[index];
                if (agent == null || !agent.gameObject.activeInHierarchy)
                {
                    continue;
                }

                if (!definition.AllowsSpecies(agent.Species))
                {
                    continue;
                }

                float distance = Vector3.Distance(trapInstance.transform.position, agent.transform.position);
                if (distance > definition.CaptureRadius || distance >= bestDistance)
                {
                    continue;
                }

                bestDistance = distance;
                bestAgent = agent;
            }

            if (bestAgent == null)
            {
                return false;
            }

            wildlifeKey = bestAgent.gameObject.name + "_" + bestAgent.GetEntityId();
            bestAgent.gameObject.SetActive(false);
            return true;
        }

        private bool TryResolvePlacementPose(
            CCS_TrapDefinition definition,
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
            message = string.Empty;

            if (Physics.Raycast(
                    position + Vector3.up * 2f,
                    Vector3.down,
                    out RaycastHit hit,
                    definition.PlacementMaxGroundRayDistance + 2f,
                    Physics.DefaultRaycastLayers,
                    QueryTriggerInteraction.Ignore))
            {
                position = hit.point;
                float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
                if (slopeAngle > definition.PlacementMaxSlopeAngle)
                {
                    message = "Ground slope too steep for trap placement.";
                    return true;
                }

                isValid = true;
                message = "Valid trap placement.";
                return true;
            }

            message = "No ground found for trap placement.";
            return true;
        }

        private void EnsurePlacementPreview(
            CCS_TrapDefinition definition,
            Vector3 position,
            Quaternion rotation,
            bool isValid)
        {
            if (placementPreview == null)
            {
                GameObject previewRoot = new GameObject("CCS_TrapPlacementPreviewRoot");
                placementPreview = previewRoot.AddComponent<CCS_TrapPlacementPreview>();
            }

            placementPreview.EnsurePreviewObject(definition.PlacementPrimitive);
            Color validColor = activeProfile != null ? activeProfile.ValidPreviewColor : Color.green;
            Color invalidColor = activeProfile != null ? activeProfile.InvalidPreviewColor : Color.red;
            placementPreview.UpdatePreview(
                position,
                rotation,
                definition.PlacedLocalScale,
                isValid,
                validColor,
                invalidColor);
        }

        private void EnsurePlacementPreviewHidden()
        {
            if (placementPreview != null)
            {
                placementPreview.SetVisible(false);
            }
        }

        private void ClearSpawnedTraps()
        {
            List<CCS_TrapInstance> instances = new List<CCS_TrapInstance>(registeredTraps.Values);
            for (int index = 0; index < instances.Count; index++)
            {
                if (instances[index] != null)
                {
                    UnityEngine.Object.Destroy(instances[index].gameObject);
                }
            }

            registeredTraps.Clear();
        }

        private string BuildInstanceId(CCS_TrapDefinition definition)
        {
            string prefix = definition != null ? definition.TrapDefinitionId : "trap";
            int sequence = nextInstanceSequence++;
            return $"{prefix}.{sequence}";
        }

        private bool EnsureReady()
        {
            return isInitialized && activeProfile != null;
        }

        #endregion
    }
}
