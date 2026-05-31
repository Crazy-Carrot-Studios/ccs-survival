using System;
using CCS.Modules.SaveLoad;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_PlayerInventoryService
// CATEGORY: Modules / Inventory / Runtime / Services
// PURPOSE: Runtime owner of the player inventory container and inventory events.
// PLACEMENT: Registered as CCS_ISurvivalService by future inventory module installer.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Implements CCS_ISaveable at 0.6.2. Restores before equipment save payloads.
// =============================================================================

namespace CCS.Modules.Inventory
{
    public sealed class CCS_PlayerInventoryService : CCS_ISurvivalService, CCS_ISaveable
    {
        private const string LogPrefix = "[CCS_PlayerInventoryService]";

        #region Variables

        private CCS_InventoryContainer inventoryContainer;
        private CCS_InventoryProfile activeProfile;
        private CCS_ItemDefinitionLookup itemDefinitionLookup;
        private Func<CCS_InventoryCapacityModifierSnapshot> capacityModifierSource;
        private bool isInitialized;

        #endregion

        #region Events

        public event InventoryItemAddedHandler ItemAdded;
        public event InventoryItemRemovedHandler ItemRemoved;
        public event InventoryChangedHandler InventoryChanged;
        public event InventoryFullHandler InventoryFull;

        #endregion

        #region Properties

        public bool IsInitialized => isInitialized;

        public CCS_InventoryProfile ActiveProfile => activeProfile;

        public CCS_IInventoryContainer Container => inventoryContainer;

        public string SaveableId => CCS_SaveLoadSaveableIds.PlayerInventory;

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

        public void InitializeFromProfile(CCS_InventoryProfile profile)
        {
            if (profile == null)
            {
                Debug.LogWarning($"{LogPrefix} InitializeFromProfile called with null profile.");
                isInitialized = false;
                return;
            }

            CCS_SurvivalValidationResult validation = CCS_InventoryValidationUtility.ValidateProfile(profile);
            if (!validation.IsSuccess)
            {
                Debug.LogWarning($"{LogPrefix} Profile validation warning: {validation.Message}");
            }

            activeProfile = profile;
            itemDefinitionLookup = new CCS_ItemDefinitionLookup(profile.SaveRestoreItemDefinitions);
            inventoryContainer = new CCS_InventoryContainer(profile.InventorySlotCount);
            isInitialized = true;
        }

        public void SetCapacityModifierSource(Func<CCS_InventoryCapacityModifierSnapshot> source)
        {
            capacityModifierSource = source;
        }

        public int AddItem(CCS_ItemDefinition itemDefinition, int quantity)
        {
            if (!EnsureInitialized() || itemDefinition == null || quantity <= 0)
            {
                return 0;
            }

            int added = inventoryContainer.AddItem(itemDefinition, quantity);
            int remaining = quantity - added;

            if (added > 0)
            {
                RaiseItemAdded(itemDefinition, added);
                RaiseInventoryChanged(itemDefinition, added, "Item added.");
            }

            if (remaining > 0)
            {
                RaiseInventoryFull(itemDefinition, remaining);
            }

            return added;
        }

        public int RemoveItem(CCS_ItemDefinition itemDefinition, int quantity)
        {
            if (!EnsureInitialized() || itemDefinition == null || quantity <= 0)
            {
                return 0;
            }

            int removed = inventoryContainer.RemoveItem(itemDefinition, quantity);

            if (removed > 0)
            {
                RaiseItemRemoved(itemDefinition, removed);
                RaiseInventoryChanged(itemDefinition, removed, "Item removed.");
            }

            return removed;
        }

        public bool CanAdd(CCS_ItemDefinition itemDefinition, int quantity)
        {
            return EnsureInitialized()
                && inventoryContainer.CanAdd(itemDefinition, quantity);
        }

        public bool HasItem(CCS_ItemDefinition itemDefinition, int quantity)
        {
            return EnsureInitialized()
                && inventoryContainer.HasItem(itemDefinition, quantity);
        }

        public int GetQuantity(CCS_ItemDefinition itemDefinition)
        {
            return EnsureInitialized()
                ? inventoryContainer.GetQuantity(itemDefinition)
                : 0;
        }

        public void ClearInventory()
        {
            if (!EnsureInitialized())
            {
                return;
            }

            inventoryContainer.Clear();
            RaiseInventoryChanged(null, 0, "Inventory cleared.");
        }

        public CCS_InventorySnapshot CreateSnapshot()
        {
            return EnsureInitialized()
                ? inventoryContainer.CreateSnapshot()
                : new CCS_InventorySnapshot(System.Array.Empty<CCS_ItemStack>(), 0, 0, 0);
        }

        public string CaptureState()
        {
            if (!EnsureInitialized())
            {
                return JsonUtility.ToJson(new CCS_InventorySaveData());
            }

            CCS_InventoryCapacityModifierSnapshot capacityModifiers = ResolveCapacityModifierSnapshot();
            CCS_InventorySaveSlotEntry[] slotEntries = BuildSaveSlotEntries();
            CCS_InventorySaveData saveData = new CCS_InventorySaveData
            {
                saveDataVersion = CCS_InventorySaveData.CurrentSaveDataVersion,
                slotCount = inventoryContainer.SlotCount,
                additionalInventorySlots = capacityModifiers.AdditionalInventorySlots,
                additionalCarryWeight = capacityModifiers.AdditionalCarryWeight,
                slots = slotEntries
            };

            return JsonUtility.ToJson(saveData);
        }

        public void RestoreState(string stateJson)
        {
            if (!EnsureInitialized())
            {
                Debug.LogWarning($"{LogPrefix} RestoreState skipped because service is not initialized.");
                return;
            }

            if (string.IsNullOrWhiteSpace(stateJson))
            {
                ClearInventory();
                return;
            }

            CCS_InventorySaveData saveData = JsonUtility.FromJson<CCS_InventorySaveData>(stateJson);
            if (saveData == null)
            {
                Debug.LogWarning($"{LogPrefix} RestoreState skipped because payload could not be parsed.");
                return;
            }

            if (saveData.saveDataVersion <= 0)
            {
                Debug.LogWarning($"{LogPrefix} RestoreState skipped because saveDataVersion is missing.");
                return;
            }

            inventoryContainer.RestoreFromSaveEntries(
                saveData.slots,
                itemDefinitionLookup,
                out int restoredSlotCount,
                out int skippedSlotCount);

            if (skippedSlotCount > 0)
            {
                Debug.LogWarning(
                    $"{LogPrefix} RestoreState skipped {skippedSlotCount} slot(s) due to missing or invalid item definitions.");
            }

            Debug.Log($"{LogPrefix} RestoreState restored {restoredSlotCount} occupied slot(s).");
            RaiseInventoryChanged(null, 0, "Inventory restored from save.");
        }

        #endregion

        #region Private Methods

        private CCS_InventorySaveSlotEntry[] BuildSaveSlotEntries()
        {
            CCS_InventorySaveSlotEntry[] slotEntries =
                new CCS_InventorySaveSlotEntry[inventoryContainer.SlotCount];

            for (int slotIndex = 0; slotIndex < slotEntries.Length; slotIndex++)
            {
                CCS_InventorySlot slot = inventoryContainer.GetSlot(slotIndex);
                CCS_InventorySaveSlotEntry saveEntry = new CCS_InventorySaveSlotEntry();

                if (slot != null && !slot.IsEmpty && slot.Stack.ItemDefinition != null)
                {
                    saveEntry.itemId = slot.Stack.ItemDefinition.ItemId ?? string.Empty;
                    saveEntry.quantity = slot.Stack.Quantity;
                }

                slotEntries[slotIndex] = saveEntry;
            }

            return slotEntries;
        }

        private CCS_InventoryCapacityModifierSnapshot ResolveCapacityModifierSnapshot()
        {
            if (capacityModifierSource == null)
            {
                return CCS_InventoryCapacityModifierSnapshot.Empty;
            }

            try
            {
                return capacityModifierSource.Invoke();
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"{LogPrefix} Capacity modifier source failed: {exception.Message}");
                return CCS_InventoryCapacityModifierSnapshot.Empty;
            }
        }

        private bool EnsureInitialized()
        {
            if (isInitialized && inventoryContainer != null)
            {
                return true;
            }

            Debug.LogWarning($"{LogPrefix} Service is not initialized.");
            return false;
        }

        private void RaiseItemAdded(CCS_ItemDefinition itemDefinition, int quantity)
        {
            ItemAdded?.Invoke(new CCS_InventoryEventArgs(itemDefinition, quantity));
        }

        private void RaiseItemRemoved(CCS_ItemDefinition itemDefinition, int quantity)
        {
            ItemRemoved?.Invoke(new CCS_InventoryEventArgs(itemDefinition, quantity));
        }

        private void RaiseInventoryChanged(CCS_ItemDefinition itemDefinition, int quantity, string message)
        {
            InventoryChanged?.Invoke(new CCS_InventoryEventArgs(itemDefinition, quantity, message: message));
        }

        private void RaiseInventoryFull(CCS_ItemDefinition itemDefinition, int remainingQuantity)
        {
            InventoryFull?.Invoke(new CCS_InventoryEventArgs(
                itemDefinition,
                remainingQuantity,
                remainingQuantity: remainingQuantity,
                message: "Inventory could not accept remaining quantity."));
        }

        #endregion
    }
}
