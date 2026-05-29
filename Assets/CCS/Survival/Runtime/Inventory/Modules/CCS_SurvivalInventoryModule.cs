using System;
using System.Collections.Generic;
using CCS.Core;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_SurvivalInventoryModule
// CATEGORY: Survival / Runtime / Inventory / Modules
// PURPOSE: Survival inventory module owning a fixed slot container and service registration.
// PLACEMENT: Installed by CCS_SurvivalInventoryModuleInstaller via survival bootstrap sequencing.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Module ID ccs.survival.inventory. Pickup integration deferred to Phase 2C.
// =============================================================================

namespace CCS.Survival.Inventory
{
    public sealed class CCS_SurvivalInventoryModule : CCS_SurvivalModuleBase, CCS_ISurvivalInventoryService
    {
        private const int DefaultSlotCount = 16;

        #region Variables

        private readonly int slotCount;
        private readonly bool enableInventoryDebugLogs;
        private readonly CCS_SurvivalInventoryContainer inventoryContainer;
        private readonly List<(string displayName, int amount)> summaryScratch = new List<(string, int)>(8);
        private CCS_RuntimeHost registeredHost;

        #endregion

        #region Events

        public event Action InventoryChanged;

        public event Action<string, string, int, int> ItemAdded;

        public event Action<string, string, int, int> ItemRemoved;

        #endregion

        #region Properties

        public int SlotCount => inventoryContainer.SlotCount;

        public int OccupiedSlotCount => inventoryContainer.GetOccupiedSlotCount();

        #endregion

        #region Public Methods

        public CCS_SurvivalInventoryModule(int slotCount, bool enableDebugLogs)
            : base(
                new CCS_ModuleMetadata(
                    CCS_SurvivalRuntimeConstants.InventoryModuleId,
                    "CCS Survival Inventory Module",
                    "0.7.0",
                    "Phase 2B inventory container and service foundation."),
                CCS_SurvivalRuntimeConstants.InventoryLogCategory,
                enableDebugLogs)
        {
            enableInventoryDebugLogs = enableDebugLogs;
            this.slotCount = slotCount < 1 ? DefaultSlotCount : slotCount;
            inventoryContainer = new CCS_SurvivalInventoryContainer(this.slotCount);
        }

        public CCS_Result TryAddItem(CCS_SurvivalItemDefinition definition, int amount, out int remainingAmount)
        {
            remainingAmount = amount;

            CCS_SurvivalValidationResult definitionValidation =
                CCS_SurvivalItemDefinitionValidationUtility.ValidateDefinition(definition);
            if (!definitionValidation.IsSuccess)
            {
                return definitionValidation.ToCoreResult();
            }

            if (amount <= 0)
            {
                return CCS_Result.Failure(CCS_CoreErrorCode.ValidationFailed, "Add amount must be greater than zero.");
            }

            int previousCount = inventoryContainer.GetItemCount(definition.ItemId);
            int accepted = inventoryContainer.AddItem(definition, amount, out remainingAmount);
            if (accepted <= 0)
            {
                return CCS_Result.Failure(CCS_CoreErrorCode.ValidationFailed, "Inventory is full or cannot accept item.");
            }

            int newTotal = previousCount + accepted;
            NotifyItemAdded(definition, accepted, newTotal);
            return CCS_Result.Success();
        }

        public CCS_Result TryRemoveItem(string itemId, int amount, out int removedAmount)
        {
            removedAmount = 0;

            if (string.IsNullOrWhiteSpace(itemId))
            {
                return CCS_Result.Failure(CCS_CoreErrorCode.ValidationFailed, "Item ID is required.");
            }

            if (amount <= 0)
            {
                return CCS_Result.Failure(CCS_CoreErrorCode.ValidationFailed, "Remove amount must be greater than zero.");
            }

            int previousCount = inventoryContainer.GetItemCount(itemId);
            if (previousCount <= 0)
            {
                return CCS_Result.Failure(CCS_CoreErrorCode.ValidationFailed, $"Item not found: {itemId}");
            }

            removedAmount = inventoryContainer.RemoveItem(itemId, amount);
            if (removedAmount <= 0)
            {
                return CCS_Result.Failure(CCS_CoreErrorCode.ValidationFailed, "No items removed.");
            }

            int newTotal = previousCount - removedAmount;
            NotifyItemRemoved(itemId, removedAmount, newTotal);
            return CCS_Result.Success();
        }

        public bool HasItem(string itemId, int amount)
        {
            return inventoryContainer.HasItem(itemId, amount);
        }

        public int GetItemCount(string itemId)
        {
            return inventoryContainer.GetItemCount(itemId);
        }

        public bool TryGetSlotSnapshot(int slotIndex, out CCS_SurvivalInventorySlotSnapshot snapshot)
        {
            return inventoryContainer.TryGetSlotSnapshot(slotIndex, out snapshot);
        }

        public string BuildCompactItemSummary(int maxEntries)
        {
            inventoryContainer.BuildItemSummary(summaryScratch, maxEntries);
            if (summaryScratch.Count == 0)
            {
                return "None";
            }

            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            for (int i = 0; i < summaryScratch.Count; i++)
            {
                (string displayName, int amount) = summaryScratch[i];
                if (i > 0)
                {
                    builder.Append(", ");
                }

                builder.Append(displayName);
                builder.Append(" x");
                builder.Append(amount);
            }

            return builder.ToString();
        }

        #endregion

        #region Protected Methods

        protected override CCS_Result OnInitialize()
        {
            LogSurvival("Inventory module initialize.");
            return CCS_Result.Success();
        }

        protected override CCS_Result OnInstall(CCS_RuntimeHost runtimeHost)
        {
            CCS_SurvivalValidationResult moduleValidation = CCS_SurvivalModuleValidationUtility.ValidateModule(this);
            if (!moduleValidation.IsSuccess)
            {
                CCS_Logger.LogWarning(SurvivalLogCategory, moduleValidation.Message);
                return moduleValidation.ToCoreResult();
            }

            if (!runtimeHost.ServiceRegistry.RegisterService<CCS_ISurvivalInventoryService>(this))
            {
                CCS_Logger.LogWarning(SurvivalLogCategory, "Inventory service registration failed or was already registered.");
                return CCS_Result.Failure(
                    CCS_CoreErrorCode.ValidationFailed,
                    "Failed to register CCS_ISurvivalInventoryService.");
            }

            registeredHost = runtimeHost;
            LogSurvival($"Inventory module installed. Slots={SlotCount}.");

            if (enableInventoryDebugLogs)
            {
                CCS_SurvivalInventoryBootstrapSelfTest.RunContainerSmokeTest(true);
            }

            return CCS_Result.Success();
        }

        protected override CCS_Result OnUninstall(CCS_RuntimeHost runtimeHost)
        {
            if (CCS_Validation.IsObjectValid(runtimeHost))
            {
                runtimeHost.ServiceRegistry.UnregisterService<CCS_ISurvivalInventoryService>();
            }

            registeredHost = null;
            inventoryContainer.Clear();
            LogSurvival("Inventory module uninstalled.");
            return CCS_Result.Success();
        }

        #endregion

        #region Private Methods

        private void NotifyItemAdded(CCS_SurvivalItemDefinition definition, int amountAdded, int newTotalCount)
        {
            ItemAdded?.Invoke(definition.ItemId, definition.DisplayName, amountAdded, newTotalCount);
            DispatchInventoryEvents(definition.ItemId, definition.DisplayName, amountAdded, 0, newTotalCount);
            NotifyInventoryChanged();
        }

        private void NotifyItemRemoved(string itemId, int amountRemoved, int newTotalCount)
        {
            string displayName = itemId;
            ItemRemoved?.Invoke(itemId, displayName, amountRemoved, newTotalCount);
            DispatchInventoryEvents(itemId, displayName, 0, amountRemoved, newTotalCount);
            NotifyInventoryChanged();
        }

        private void NotifyInventoryChanged()
        {
            InventoryChanged?.Invoke();
            DispatchInventoryChangedEvent();
        }

        private void DispatchInventoryChangedEvent()
        {
            if (!CCS_Validation.IsObjectValid(registeredHost) || !registeredHost.IsRuntimeInitialized)
            {
                return;
            }

            registeredHost.EventDispatcher.Dispatch(
                new CCS_SurvivalInventoryChangedEvent(OccupiedSlotCount, SlotCount));
        }

        private void DispatchInventoryEvents(
            string itemId,
            string displayName,
            int amountAdded,
            int amountRemoved,
            int newTotalCount)
        {
            if (!CCS_Validation.IsObjectValid(registeredHost) || !registeredHost.IsRuntimeInitialized)
            {
                return;
            }

            if (amountAdded > 0)
            {
                registeredHost.EventDispatcher.Dispatch(
                    new CCS_SurvivalItemAddedEvent(itemId, displayName, amountAdded, newTotalCount));
            }

            if (amountRemoved > 0)
            {
                registeredHost.EventDispatcher.Dispatch(
                    new CCS_SurvivalItemRemovedEvent(itemId, displayName, amountRemoved, newTotalCount));
            }
        }

        #endregion
    }
}
