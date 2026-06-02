using System;
using CCS.Modules.Inventory;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_StorageContainer
// CATEGORY: Modules / Storage / Runtime / Components
// PURPOSE: World storage container with slot inventory, open state, and save snapshots.
// PLACEMENT: PF_CCS_PrimitiveStorageCrate and dynamically spawned placed crates.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: No direct player input. Interactable hands off open/close to CCS_StorageService.
// =============================================================================

namespace CCS.Modules.Storage
{
    public sealed class CCS_StorageContainer : MonoBehaviour
    {
        #region Variables

        [Header("Identity")]
        [Tooltip("Container definition id copied from CCS_StorageContainerDefinition at configure time.")]
        [SerializeField] private string containerId = string.Empty;

        [Tooltip("Unique runtime instance id for save and service registration.")]
        [SerializeField] private string instanceId = string.Empty;

        [Tooltip("Player-facing container label.")]
        [SerializeField] private string displayName = "Storage Crate";

        [Header("Capacity")]
        [Tooltip("Number of inventory slots in this container instance.")]
        [SerializeField] private int slotCount = 8;

        [Tooltip("Optional max weight for this instance. Zero disables enforcement.")]
        [SerializeField] private float maxWeight;

        [Header("Diagnostics")]
        [Tooltip("Emit container-level debug logs.")]
        [SerializeField] private bool enableDebugLogging;

        private CCS_InventoryContainer inventoryContainer;
        private bool isOpen;

        #endregion

        #region Properties

        public string ContainerId => containerId ?? string.Empty;

        public string InstanceId => instanceId ?? string.Empty;

        public string DisplayName => displayName ?? string.Empty;

        public int SlotCount => inventoryContainer != null ? inventoryContainer.SlotCount : slotCount;

        public float MaxWeight => maxWeight < 0f ? 0f : maxWeight;

        public bool HasMaxWeight => maxWeight > 0f;

        public bool IsOpen => isOpen;

        public CCS_IInventoryContainer ContainerInventory => inventoryContainer;

        #endregion

        #region Unity Callbacks

        private void OnEnable()
        {
            EnsureInventoryInitialized();
            RegisterWithService();
        }

        private void OnDisable()
        {
            UnregisterFromService();
        }

        #endregion

        #region Public Methods

        public void ConfigureFromDefinition(CCS_StorageContainerDefinition definition, string configuredInstanceId)
        {
            if (definition == null)
            {
                return;
            }

            containerId = definition.ContainerId;
            displayName = definition.DisplayName;
            slotCount = definition.SlotCount;
            maxWeight = definition.MaxWeight;
            enableDebugLogging = definition.EnableDebugLogging;
            instanceId = string.IsNullOrWhiteSpace(configuredInstanceId)
                ? GenerateInstanceId()
                : configuredInstanceId;

            EnsureInventoryInitialized();
        }

        public void AssignInstanceId(string configuredInstanceId)
        {
            instanceId = string.IsNullOrWhiteSpace(configuredInstanceId)
                ? GenerateInstanceId()
                : configuredInstanceId;
        }

        public void ConfigureRuntimeInstance(
            string containerDefinitionId,
            string configuredInstanceId,
            string label,
            int slots)
        {
            containerId = containerDefinitionId ?? string.Empty;
            instanceId = string.IsNullOrWhiteSpace(configuredInstanceId)
                ? GenerateInstanceId()
                : configuredInstanceId;
            displayName = string.IsNullOrWhiteSpace(label) ? "Storage" : label;
            slotCount = slots < 1 ? 1 : slots;
            EnsureInventoryInitialized();
        }

        public bool CanOpen()
        {
            return isActiveAndEnabled && inventoryContainer != null;
        }

        public bool Open()
        {
            if (!CanOpen())
            {
                return false;
            }

            isOpen = true;
            LogDebug($"Opened container '{displayName}' ({instanceId}).");
            return true;
        }

        public void Close()
        {
            if (!isOpen)
            {
                return;
            }

            isOpen = false;
            LogDebug($"Closed container '{displayName}' ({instanceId}).");
        }

        public bool TryAddItem(CCS_ItemDefinition itemDefinition, int quantity, out int quantityAdded)
        {
            quantityAdded = 0;
            if (!EnsureInventoryInitialized() || itemDefinition == null || quantity <= 0)
            {
                return false;
            }

            if (HasMaxWeight && !CanAcceptWeight(itemDefinition, quantity))
            {
                return false;
            }

            quantityAdded = inventoryContainer.AddItem(itemDefinition, quantity);
            return quantityAdded > 0;
        }

        public bool TryRemoveItem(CCS_ItemDefinition itemDefinition, int quantity, out int quantityRemoved)
        {
            quantityRemoved = 0;
            if (!EnsureInventoryInitialized() || itemDefinition == null || quantity <= 0)
            {
                return false;
            }

            quantityRemoved = inventoryContainer.RemoveItem(itemDefinition, quantity);
            return quantityRemoved > 0;
        }

        public CCS_StorageContainerSaveState CaptureState()
        {
            CCS_StorageContainerSaveState saveState = new CCS_StorageContainerSaveState
            {
                containerDefinitionId = containerId,
                instanceId = instanceId,
                displayName = displayName
            };

            Vector3 position = transform.position;
            saveState.positionX = position.x;
            saveState.positionY = position.y;
            saveState.positionZ = position.z;

            Quaternion rotation = transform.rotation;
            saveState.rotationX = rotation.x;
            saveState.rotationY = rotation.y;
            saveState.rotationZ = rotation.z;
            saveState.rotationW = rotation.w;

            if (inventoryContainer == null)
            {
                saveState.slots = Array.Empty<CCS_StorageContainerSlotSaveState>();
                return saveState;
            }

            CCS_StorageContainerSlotSaveState[] slotStates =
                new CCS_StorageContainerSlotSaveState[inventoryContainer.SlotCount];
            for (int slotIndex = 0; slotIndex < slotStates.Length; slotIndex++)
            {
                CCS_InventorySlot slot = inventoryContainer.GetSlot(slotIndex);
                CCS_StorageContainerSlotSaveState slotState = new CCS_StorageContainerSlotSaveState();
                if (slot != null && !slot.IsEmpty && slot.Stack.ItemDefinition != null)
                {
                    slotState.itemId = slot.Stack.ItemDefinition.ItemId ?? string.Empty;
                    slotState.quantity = slot.Stack.Quantity;
                }

                slotStates[slotIndex] = slotState;
            }

            saveState.slots = slotStates;
            return saveState;
        }

        public void RestoreState(
            CCS_StorageContainerSaveState saveState,
            CCS_ItemDefinitionLookup itemDefinitionLookup)
        {
            Close();
            EnsureInventoryInitialized();
            inventoryContainer.Clear();

            if (saveState?.slots == null || itemDefinitionLookup == null)
            {
                return;
            }

            CCS_InventorySaveSlotEntry[] slotEntries = new CCS_InventorySaveSlotEntry[saveState.slots.Length];
            for (int index = 0; index < saveState.slots.Length; index++)
            {
                CCS_StorageContainerSlotSaveState source = saveState.slots[index];
                slotEntries[index] = new CCS_InventorySaveSlotEntry
                {
                    itemId = source != null ? source.itemId ?? string.Empty : string.Empty,
                    quantity = source != null ? source.quantity : 0
                };
            }

            inventoryContainer.RestoreFromSaveEntries(
                slotEntries,
                itemDefinitionLookup,
                out int restoredSlotCount,
                out int skippedSlotCount);

            if (enableDebugLogging && skippedSlotCount > 0)
            {
                Debug.Log(
                    $"[CCS_StorageContainer] RestoreState skipped {skippedSlotCount} slot(s) on '{instanceId}'. Restored {restoredSlotCount}.");
            }
        }

        public bool HasAnyItems()
        {
            if (inventoryContainer == null)
            {
                return false;
            }

            for (int slotIndex = 0; slotIndex < inventoryContainer.SlotCount; slotIndex++)
            {
                CCS_InventorySlot slot = inventoryContainer.GetSlot(slotIndex);
                if (slot != null && !slot.IsEmpty)
                {
                    return true;
                }
            }

            return false;
        }

        public bool TryGetFirstOccupiedSlot(out CCS_ItemDefinition itemDefinition, out int quantity)
        {
            itemDefinition = null;
            quantity = 0;

            if (inventoryContainer == null)
            {
                return false;
            }

            for (int slotIndex = 0; slotIndex < inventoryContainer.SlotCount; slotIndex++)
            {
                CCS_InventorySlot slot = inventoryContainer.GetSlot(slotIndex);
                if (slot == null || slot.IsEmpty || slot.Stack.ItemDefinition == null)
                {
                    continue;
                }

                itemDefinition = slot.Stack.ItemDefinition;
                quantity = slot.Stack.Quantity;
                return true;
            }

            return false;
        }

        #endregion

        #region Private Methods

        private bool EnsureInventoryInitialized()
        {
            int resolvedSlotCount = slotCount > 0 ? slotCount : 1;
            if (inventoryContainer == null || inventoryContainer.SlotCount != resolvedSlotCount)
            {
                inventoryContainer = new CCS_InventoryContainer(resolvedSlotCount);
            }

            if (string.IsNullOrWhiteSpace(instanceId))
            {
                instanceId = GenerateInstanceId();
            }

            return inventoryContainer != null;
        }

        private void RegisterWithService()
        {
            if (CCS_StorageRuntimeBridge.TryGetStorageService(out CCS_StorageService storageService)
                && storageService.IsInitialized)
            {
                storageService.RegisterContainer(this);
            }
        }

        private void UnregisterFromService()
        {
            if (CCS_StorageRuntimeBridge.TryGetStorageService(out CCS_StorageService storageService)
                && storageService.IsInitialized)
            {
                storageService.UnregisterContainer(this);
            }
        }

        private bool CanAcceptWeight(CCS_ItemDefinition itemDefinition, int quantity)
        {
            if (!HasMaxWeight || itemDefinition == null || quantity <= 0)
            {
                return true;
            }

            float currentWeight = CalculateCurrentWeight();
            float incomingWeight = itemDefinition.Weight * quantity;
            return currentWeight + incomingWeight <= maxWeight;
        }

        private float CalculateCurrentWeight()
        {
            float totalWeight = 0f;
            if (inventoryContainer == null)
            {
                return totalWeight;
            }

            for (int slotIndex = 0; slotIndex < inventoryContainer.SlotCount; slotIndex++)
            {
                CCS_InventorySlot slot = inventoryContainer.GetSlot(slotIndex);
                if (slot == null || slot.IsEmpty || slot.Stack.ItemDefinition == null)
                {
                    continue;
                }

                totalWeight += slot.Stack.ItemDefinition.Weight * slot.Stack.Quantity;
            }

            return totalWeight;
        }

        private static string GenerateInstanceId()
        {
            return $"ccs.survival.storage.instance.{Guid.NewGuid():N}";
        }

        private void LogDebug(string message)
        {
            if (enableDebugLogging)
            {
                Debug.Log($"[CCS_StorageContainer] {message}");
            }
        }

        #endregion
    }
}
