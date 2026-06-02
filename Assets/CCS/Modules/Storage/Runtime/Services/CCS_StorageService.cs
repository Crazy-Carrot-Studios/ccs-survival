using System;
using System.Collections.Generic;
using CCS.Modules.Inventory;
using CCS.Survival;
using CCS.Survival.Player;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_StorageService
// CATEGORY: Modules / Storage / Runtime / Services
// PURPOSE: Registers world storage containers, tracks active container, transfers, and save state.
// PLACEMENT: Registered as CCS_ISurvivalService by survival gameplay composition wiring.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Milestone 1.1.2 storage container foundation. No multiplayer or storage UI.
// =============================================================================

namespace CCS.Modules.Storage
{
    public sealed class CCS_StorageService : CCS_ISurvivalService
    {
        private const string LogPrefix = "[CCS_StorageService]";

        #region Variables

        private readonly Dictionary<string, CCS_StorageContainer> registeredContainers =
            new Dictionary<string, CCS_StorageContainer>(StringComparer.Ordinal);

        private readonly List<CCS_StorageContainer> dynamicallySpawnedContainers =
            new List<CCS_StorageContainer>();

        private CCS_StorageProfile activeProfile;
        private CCS_PlayerInventoryService inventoryService;
        private CCS_ItemDefinitionLookup itemDefinitionLookup;
        private CCS_StorageContainer activeContainer;
        private bool isInitialized;

        #endregion

        #region Events

        public event StorageContainerOpenedHandler StorageContainerOpened;
        public event StorageContainerClosedHandler StorageContainerClosed;
        public event StorageItemAddedHandler StorageItemAdded;
        public event StorageItemRemovedHandler StorageItemRemoved;
        public event StorageStateRestoredHandler StorageStateRestored;

        #endregion

        #region Properties

        public bool IsInitialized => isInitialized;

        public CCS_StorageProfile ActiveProfile => activeProfile;

        public CCS_StorageContainer ActiveContainer => activeContainer;

        public int RegisteredContainerCount => registeredContainers.Count;

        public IReadOnlyList<CCS_StorageContainer> GetRegisteredContainers()
        {
            return new List<CCS_StorageContainer>(registeredContainers.Values);
        }

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

        public void InitializeFromProfile(CCS_StorageProfile profile)
        {
            if (profile == null)
            {
                isInitialized = false;
                return;
            }

            CCS_SurvivalValidationResult validation = CCS_StorageValidationUtility.ValidateProfile(profile);
            if (!validation.IsSuccess)
            {
                Debug.LogWarning($"{LogPrefix} Profile validation warning: {validation.Message}");
            }

            activeProfile = profile;
            isInitialized = true;
        }

        public void BindInventoryService(CCS_PlayerInventoryService inventory)
        {
            inventoryService = inventory;
            if (inventoryService?.ActiveProfile != null)
            {
                itemDefinitionLookup = new CCS_ItemDefinitionLookup(
                    inventoryService.ActiveProfile.SaveRestoreItemDefinitions);
            }
        }

        public void RegisterContainer(CCS_StorageContainer container)
        {
            if (container == null || string.IsNullOrWhiteSpace(container.InstanceId))
            {
                return;
            }

            registeredContainers[container.InstanceId] = container;
            LogDebug($"Registered container {container.InstanceId}.");
        }

        public void UnregisterContainer(CCS_StorageContainer container)
        {
            if (container == null || string.IsNullOrWhiteSpace(container.InstanceId))
            {
                return;
            }

            if (activeContainer == container)
            {
                CloseContainer();
            }

            registeredContainers.Remove(container.InstanceId);
            dynamicallySpawnedContainers.Remove(container);
            LogDebug($"Unregistered container {container.InstanceId}.");
        }

        public bool OpenContainer(CCS_StorageContainer container)
        {
            if (!EnsureReady() || container == null || !container.CanOpen())
            {
                RaiseContainerOpened(container, false, "Container cannot be opened.");
                return false;
            }

            if (activeContainer != null && activeContainer != container)
            {
                activeContainer.Close();
            }

            if (!container.Open())
            {
                RaiseContainerOpened(container, false, "Container open failed.");
                return false;
            }

            activeContainer = container;
            RaiseContainerOpened(container, true, "Container opened.");
            return true;
        }

        public void CloseContainer()
        {
            if (activeContainer == null)
            {
                return;
            }

            CCS_StorageContainer previous = activeContainer;
            previous.Close();
            activeContainer = null;
            RaiseContainerClosed(previous, true, "Container closed.");
        }

        public bool TryMovePlayerItemToContainer()
        {
            if (!EnsureReady() || activeContainer == null || inventoryService == null)
            {
                RaiseItemAdded(activeContainer, false, "No active container or inventory service.");
                return false;
            }

            if (!TryGetFirstPlayerItem(out CCS_ItemDefinition itemDefinition, out int quantity))
            {
                RaiseItemAdded(activeContainer, false, "Player inventory has no transferable items.");
                return false;
            }

            if (!activeContainer.TryAddItem(itemDefinition, quantity, out int quantityAdded))
            {
                RaiseItemAdded(activeContainer, false, "Container rejected player item transfer.");
                return false;
            }

            int removed = inventoryService.RemoveItem(itemDefinition, quantityAdded);
            if (removed < quantityAdded)
            {
                activeContainer.TryRemoveItem(itemDefinition, quantityAdded - removed, out int rollbackRemoved);
                RaiseItemAdded(activeContainer, false, "Player inventory removal failed after container add.");
                return false;
            }

            RaiseItemAdded(activeContainer, true, $"Moved {quantityAdded} x {itemDefinition.DisplayName} into container.");
            return true;
        }

        public bool TryMoveContainerItemToPlayer()
        {
            if (!EnsureReady() || activeContainer == null || inventoryService == null)
            {
                RaiseItemRemoved(activeContainer, false, "No active container or inventory service.");
                return false;
            }

            if (!activeContainer.TryGetFirstOccupiedSlot(out CCS_ItemDefinition itemDefinition, out int quantity))
            {
                RaiseItemRemoved(activeContainer, false, "Container has no transferable items.");
                return false;
            }

            int added = inventoryService.AddItem(itemDefinition, quantity);
            if (added <= 0)
            {
                RaiseItemRemoved(activeContainer, false, "Player inventory could not accept container item.");
                return false;
            }

            if (!activeContainer.TryRemoveItem(itemDefinition, added, out int removed) || removed < added)
            {
                inventoryService.RemoveItem(itemDefinition, added);
                RaiseItemRemoved(activeContainer, false, "Container removal failed after player add.");
                return false;
            }

            RaiseItemRemoved(activeContainer, true, $"Moved {added} x {itemDefinition.DisplayName} to player.");
            return true;
        }

        public CCS_StorageContainerSaveState[] CaptureWorldState()
        {
            if (registeredContainers.Count == 0)
            {
                return Array.Empty<CCS_StorageContainerSaveState>();
            }

            List<CCS_StorageContainerSaveState> records = new List<CCS_StorageContainerSaveState>(registeredContainers.Count);
            foreach (KeyValuePair<string, CCS_StorageContainer> entry in registeredContainers)
            {
                CCS_StorageContainer container = entry.Value;
                if (container == null)
                {
                    continue;
                }

                records.Add(container.CaptureState());
            }

            return records.ToArray();
        }

        public void RestoreWorldState(CCS_StorageContainerSaveState[] saveStates)
        {
            CloseContainer();
            ClearDynamicallySpawnedContainers();

            if (saveStates == null || saveStates.Length == 0)
            {
                RaiseStateRestored(null, true, "No storage containers to restore.");
                return;
            }

            int restoredCount = 0;
            for (int index = 0; index < saveStates.Length; index++)
            {
                CCS_StorageContainerSaveState saveState = saveStates[index];
                if (saveState == null || string.IsNullOrWhiteSpace(saveState.instanceId))
                {
                    continue;
                }

                if (TryFindRegisteredContainer(saveState.instanceId, out CCS_StorageContainer existingContainer))
                {
                    ApplySaveStateToContainer(existingContainer, saveState);
                    restoredCount++;
                    continue;
                }

                CCS_StorageContainerDefinition definition = ResolveDefinition(saveState.containerDefinitionId);
                if (definition == null || definition.PrefabReference == null)
                {
                    continue;
                }

                Vector3 position = new Vector3(saveState.positionX, saveState.positionY, saveState.positionZ);
                Quaternion rotation = new Quaternion(
                    saveState.rotationX,
                    saveState.rotationY,
                    saveState.rotationZ,
                    saveState.rotationW);

                CCS_StorageContainer spawnedContainer = SpawnContainer(
                    definition,
                    position,
                    rotation,
                    saveState.instanceId,
                    markDynamicSpawn: true);
                if (spawnedContainer == null)
                {
                    continue;
                }

                ApplySaveStateToContainer(spawnedContainer, saveState);
                restoredCount++;
            }

            RaiseStateRestored(null, restoredCount > 0, $"Restored {restoredCount} storage container(s).");
        }

        public CCS_StorageContainer TryPlaceDefaultContainerNearPlayer()
        {
            if (!EnsureReady() || activeProfile?.DefaultContainerDefinition == null)
            {
                return null;
            }

            if (!TryResolvePlayerPosition(out Vector3 playerPosition, out Vector3 playerForward))
            {
                return null;
            }

            Vector3 spawnPosition = playerPosition + playerForward * 2f + Vector3.up * 0.25f;
            Quaternion spawnRotation = Quaternion.LookRotation(playerForward, Vector3.up);
            CCS_StorageContainer container = SpawnContainer(
                activeProfile.DefaultContainerDefinition,
                spawnPosition,
                spawnRotation,
                null,
                markDynamicSpawn: true);

            if (container != null)
            {
                OpenContainer(container);
            }

            return container;
        }

        public CCS_StorageContainer SpawnContainer(
            CCS_StorageContainerDefinition definition,
            Vector3 position,
            Quaternion rotation,
            string instanceId,
            bool markDynamicSpawn)
        {
            if (!EnsureReady() || definition == null || definition.PrefabReference == null)
            {
                return null;
            }

            GameObject instance = UnityEngine.Object.Instantiate(
                definition.PrefabReference,
                position,
                rotation);
            if (instance == null)
            {
                return null;
            }

            CCS_StorageContainer container = instance.GetComponent<CCS_StorageContainer>();
            if (container == null)
            {
                container = instance.AddComponent<CCS_StorageContainer>();
            }

            if (instance.GetComponent<CCS_StorageContainerInteractable>() == null)
            {
                instance.AddComponent<CCS_StorageContainerInteractable>();
            }

            container.ConfigureFromDefinition(definition, instanceId);
            RegisterContainer(container);

            if (markDynamicSpawn && !dynamicallySpawnedContainers.Contains(container))
            {
                dynamicallySpawnedContainers.Add(container);
            }

            LogDebug($"Spawned container '{definition.DisplayName}' at {position}.");
            return container;
        }

        public bool TryOpenNearestContainer()
        {
            if (!EnsureReady())
            {
                return false;
            }

            if (activeContainer != null)
            {
                return true;
            }

            CCS_StorageContainer nearest = null;
            float nearestDistance = float.MaxValue;
            if (!TryResolvePlayerPosition(out Vector3 playerPosition, out _))
            {
                return false;
            }

            foreach (KeyValuePair<string, CCS_StorageContainer> entry in registeredContainers)
            {
                CCS_StorageContainer container = entry.Value;
                if (container == null)
                {
                    continue;
                }

                float distance = Vector3.Distance(playerPosition, container.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = container;
                }
            }

            return nearest != null && OpenContainer(nearest);
        }

        #endregion

        #region Private Methods

        private bool EnsureReady()
        {
            return isInitialized
                && inventoryService != null
                && inventoryService.IsInitialized
                && itemDefinitionLookup != null;
        }

        private void ApplySaveStateToContainer(
            CCS_StorageContainer container,
            CCS_StorageContainerSaveState saveState)
        {
            if (container == null || saveState == null)
            {
                return;
            }

            Vector3 position = new Vector3(saveState.positionX, saveState.positionY, saveState.positionZ);
            Quaternion rotation = new Quaternion(
                saveState.rotationX,
                saveState.rotationY,
                saveState.rotationZ,
                saveState.rotationW);
            container.transform.SetPositionAndRotation(position, rotation);
            container.RestoreState(saveState, itemDefinitionLookup);
            container.Close();
        }

        private void ClearDynamicallySpawnedContainers()
        {
            for (int index = dynamicallySpawnedContainers.Count - 1; index >= 0; index--)
            {
                CCS_StorageContainer container = dynamicallySpawnedContainers[index];
                if (container == null)
                {
                    dynamicallySpawnedContainers.RemoveAt(index);
                    continue;
                }

                UnregisterContainer(container);
                UnityEngine.Object.Destroy(container.gameObject);
            }

            dynamicallySpawnedContainers.Clear();
        }

        private bool TryFindRegisteredContainer(string instanceId, out CCS_StorageContainer container)
        {
            container = null;
            if (string.IsNullOrWhiteSpace(instanceId))
            {
                return false;
            }

            return registeredContainers.TryGetValue(instanceId, out container) && container != null;
        }

        private CCS_StorageContainerDefinition ResolveDefinition(string containerDefinitionId)
        {
            if (activeProfile?.DefaultContainerDefinition != null
                && activeProfile.DefaultContainerDefinition.ContainerId == containerDefinitionId)
            {
                return activeProfile.DefaultContainerDefinition;
            }

            return activeProfile?.DefaultContainerDefinition;
        }

        private bool TryGetFirstPlayerItem(out CCS_ItemDefinition itemDefinition, out int quantity)
        {
            itemDefinition = null;
            quantity = 0;

            if (inventoryService == null || !inventoryService.IsInitialized)
            {
                return false;
            }

            CCS_InventorySnapshot snapshot = inventoryService.CreateSnapshot();
            if (snapshot?.SlotStacks == null)
            {
                return false;
            }

            for (int slotIndex = 0; slotIndex < snapshot.SlotStacks.Count; slotIndex++)
            {
                CCS_ItemStack stack = snapshot.SlotStacks[slotIndex];
                if (stack.IsEmpty || stack.ItemDefinition == null)
                {
                    continue;
                }

                itemDefinition = stack.ItemDefinition;
                quantity = stack.Quantity;
                return true;
            }

            return false;
        }

        private static bool TryResolvePlayerPosition(out Vector3 position, out Vector3 forward)
        {
            position = Vector3.zero;
            forward = Vector3.forward;

            CCS_PlayerGameplayController[] players =
                CCS_SurvivalSceneQueryUtility.FindActiveObjectsByType<CCS_PlayerGameplayController>();
            if (players == null || players.Length == 0 || players[0] == null)
            {
                return false;
            }

            Transform playerTransform = players[0].transform;
            position = playerTransform.position;
            forward = playerTransform.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.001f)
            {
                forward = Vector3.forward;
            }
            else
            {
                forward.Normalize();
            }

            return true;
        }

        private void RaiseContainerOpened(CCS_StorageContainer container, bool success, string message)
        {
            StorageContainerOpened?.Invoke(BuildEventArgs(container, success, message));
        }

        private void RaiseContainerClosed(CCS_StorageContainer container, bool success, string message)
        {
            StorageContainerClosed?.Invoke(BuildEventArgs(container, success, message));
        }

        private void RaiseItemAdded(CCS_StorageContainer container, bool success, string message)
        {
            StorageItemAdded?.Invoke(BuildEventArgs(container, success, message));
        }

        private void RaiseItemRemoved(CCS_StorageContainer container, bool success, string message)
        {
            StorageItemRemoved?.Invoke(BuildEventArgs(container, success, message));
        }

        private void RaiseStateRestored(CCS_StorageContainer container, bool success, string message)
        {
            StorageStateRestored?.Invoke(BuildEventArgs(container, success, message));
        }

        private static CCS_StorageEventArgs BuildEventArgs(
            CCS_StorageContainer container,
            bool success,
            string message)
        {
            Vector3 worldPosition = container != null ? container.transform.position : Vector3.zero;
            string containerId = container != null ? container.ContainerId : string.Empty;
            string instanceId = container != null ? container.InstanceId : string.Empty;
            string displayName = container != null ? container.DisplayName : string.Empty;
            return new CCS_StorageEventArgs(containerId, instanceId, displayName, worldPosition, success, message);
        }

        private void LogDebug(string message)
        {
            if (activeProfile != null && activeProfile.EnableDebugLogging)
            {
                Debug.Log($"{LogPrefix} {message}");
            }
        }

        #endregion
    }
}
