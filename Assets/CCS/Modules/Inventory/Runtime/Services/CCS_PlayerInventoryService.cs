using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_PlayerInventoryService
// CATEGORY: Modules / Inventory / Runtime / Services
// PURPOSE: Runtime owner of the player inventory container and inventory events.
// PLACEMENT: Registered as CCS_ISurvivalService by future inventory module installer.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: No interaction, save, equipment, crafting, or UI references in 0.4.0.
// =============================================================================

namespace CCS.Modules.Inventory
{
    public sealed class CCS_PlayerInventoryService : CCS_ISurvivalService
    {
        private const string LogPrefix = "[CCS_PlayerInventoryService]";

        #region Variables

        private CCS_InventoryContainer inventoryContainer;
        private CCS_InventoryProfile activeProfile;
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
            inventoryContainer = new CCS_InventoryContainer(profile.InventorySlotCount);
            isInitialized = true;
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

        #endregion

        #region Private Methods

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
