using System;
using CCS.Modules.Equipment;
using CCS.Modules.Interaction;
using CCS.Modules.Inventory;
using CCS.Modules.SurvivalCore;
using CCS.Modules.WorldResources;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_HudPresentationService
// CATEGORY: Modules / UI / Runtime / Services
// PURPOSE: Read-only bridge between gameplay module services and HUD presenters.
// PLACEMENT: Owned by CCS_HudRootPresenter. No gameplay mutation.
// AUTHOR: James Schilz
// CREATED: 2026-05-30
// NOTES: Safe when gameplay services are missing. Event-driven refresh where possible.
// =============================================================================

namespace CCS.Modules.UI
{
    public sealed class CCS_HudPresentationService : CCS_ISurvivalService
    {
        private const string LogPrefix = "[CCS_HudPresentationService]";

        #region Variables

        private CCS_HudProfile activeProfile;
        private CCS_SurvivalCoreService survivalCoreService;
        private CCS_InteractionService interactionService;
        private CCS_PlayerInventoryService inventoryService;
        private CCS_PlayerEquipmentService equipmentService;
        private CCS_ResourceHarvestService resourceHarvestService;
        private CCS_ResourceRespawnService resourceRespawnService;

        private string currentInteractionPrompt = string.Empty;
        private CCS_InventorySnapshot inventorySnapshot;
        private CCS_EquipmentSnapshot equipmentSnapshot;
        private CCS_SurvivalStatSnapshot healthSnapshot;
        private CCS_SurvivalStatSnapshot staminaSnapshot;
        private CCS_SurvivalStatSnapshot hungerSnapshot;
        private CCS_SurvivalStatSnapshot thirstSnapshot;
        private CCS_SurvivalStatSnapshot fatigueSnapshot;
        private CCS_SurvivalStatSnapshot temperatureSnapshot;

        private bool isInitialized;

        #endregion

        #region Events

        public event HudInitializedHandler HudInitialized;
        public event HudDataRefreshedHandler HudDataRefreshed;
        public event InteractionPromptChangedHandler InteractionPromptChanged;
        public event NotificationQueuedHandler NotificationQueued;
        public event NotificationDismissedHandler NotificationDismissed;

        #endregion

        #region Properties

        public bool IsInitialized => isInitialized;

        public CCS_HudProfile ActiveProfile => activeProfile;

        public string CurrentInteractionPrompt => currentInteractionPrompt;

        public CCS_InventorySnapshot InventorySnapshot => inventorySnapshot;

        public CCS_EquipmentSnapshot EquipmentSnapshot => equipmentSnapshot;

        public CCS_SurvivalStatSnapshot HealthSnapshot => healthSnapshot;

        public CCS_SurvivalStatSnapshot StaminaSnapshot => staminaSnapshot;

        public CCS_SurvivalStatSnapshot HungerSnapshot => hungerSnapshot;

        public CCS_SurvivalStatSnapshot ThirstSnapshot => thirstSnapshot;

        public CCS_SurvivalStatSnapshot FatigueSnapshot => fatigueSnapshot;

        public CCS_SurvivalStatSnapshot TemperatureSnapshot => temperatureSnapshot;

        public int EffectiveInventorySlotCount
        {
            get
            {
                if (inventorySnapshot == null || inventorySnapshot.SlotCount <= 0)
                {
                    return 0;
                }

                int bonusSlots = equipmentSnapshot != null
                    ? equipmentSnapshot.TotalAdditionalInventorySlots
                    : 0;

                return inventorySnapshot.SlotCount + bonusSlots;
            }
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

        public void InitializeFromProfile(CCS_HudProfile profile)
        {
            activeProfile = profile;
            isInitialized = profile != null;

            if (profile == null)
            {
                Debug.LogWarning($"{LogPrefix} InitializeFromProfile called with null profile.");
                return;
            }

            CCS_SurvivalValidationResult validation = CCS_UIValidationUtility.ValidateProfile(profile);
            if (!validation.IsSuccess)
            {
                Debug.LogWarning($"{LogPrefix} Profile validation warning: {validation.Message}");
            }

            RefreshCachedData("HUD initialized.");
            HudInitialized?.Invoke(BuildEventArgs("HUD initialized."));
        }

        public bool TryGetStatSnapshot(CCS_SurvivalStatType statType, out CCS_SurvivalStatSnapshot snapshot)
        {
            snapshot = default;

            if (survivalCoreService == null || !survivalCoreService.IsInitialized)
            {
                return false;
            }

            return survivalCoreService.TryGetSnapshot(statType, out snapshot);
        }

        public void BindSurvivalCoreService(CCS_SurvivalCoreService service)
        {
            UnbindSurvivalCoreService();
            survivalCoreService = service;

            if (survivalCoreService == null)
            {
                return;
            }

            survivalCoreService.StatChanged += HandleStatChanged;
            RefreshSurvivalSnapshots();
        }

        public void BindInteractionService(CCS_InteractionService service)
        {
            UnbindInteractionService();
            interactionService = service;

            if (interactionService == null)
            {
                return;
            }

            interactionService.InteractableFound += HandleInteractableFound;
            interactionService.InteractableLost += HandleInteractableLost;
            interactionService.InteractionFailed += HandleInteractionFailed;
            RefreshInteractionPrompt();
        }

        public void BindInventoryService(CCS_PlayerInventoryService service)
        {
            UnbindInventoryService();
            inventoryService = service;

            if (inventoryService == null)
            {
                return;
            }

            inventoryService.ItemAdded += HandleInventoryItemAdded;
            inventoryService.ItemRemoved += HandleInventoryItemRemoved;
            inventoryService.InventoryChanged += HandleInventoryChanged;
            RefreshInventorySnapshot();
        }

        public void BindEquipmentService(CCS_PlayerEquipmentService service)
        {
            UnbindEquipmentService();
            equipmentService = service;

            if (equipmentService == null)
            {
                return;
            }

            equipmentService.ItemEquipped += HandleEquipmentItemEquipped;
            equipmentService.ItemUnequipped += HandleEquipmentItemUnequipped;
            equipmentService.EquipmentChanged += HandleEquipmentChanged;
            RefreshEquipmentSnapshot();
        }

        public void BindResourceHarvestService(CCS_ResourceHarvestService service)
        {
            UnbindResourceHarvestService();
            resourceHarvestService = service;

            if (resourceHarvestService == null)
            {
                return;
            }

            resourceHarvestService.HarvestCompleted += HandleHarvestCompleted;
            resourceHarvestService.HarvestFailed += HandleHarvestFailed;
            resourceHarvestService.ResourceDepleted += HandleResourceDepleted;
        }

        public void BindResourceRespawnService(CCS_ResourceRespawnService service)
        {
            UnbindResourceRespawnService();
            resourceRespawnService = service;

            if (resourceRespawnService == null)
            {
                return;
            }

            resourceRespawnService.ResourceRespawned += HandleResourceRespawned;
        }

        public void RefreshCachedData(string message)
        {
            RefreshSurvivalSnapshots();
            RefreshInteractionPrompt();
            RefreshInventorySnapshot();
            RefreshEquipmentSnapshot();
            HudDataRefreshed?.Invoke(BuildEventArgs(message));
        }

        public void QueueNotification(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            NotificationQueued?.Invoke(BuildEventArgs(message));
        }

        public void DismissNotification(string message)
        {
            NotificationDismissed?.Invoke(BuildEventArgs(message));
        }

        public void Shutdown()
        {
            UnbindSurvivalCoreService();
            UnbindInteractionService();
            UnbindInventoryService();
            UnbindEquipmentService();
            UnbindResourceHarvestService();
            UnbindResourceRespawnService();
            isInitialized = false;
        }

        #endregion

        #region Private Methods

        private void HandleStatChanged(CCS_SurvivalStatChangedEventArgs eventArgs)
        {
            RefreshSurvivalSnapshots();
            HudDataRefreshed?.Invoke(BuildEventArgs("Survival stat changed."));
        }

        private void HandleInteractableFound(CCS_InteractionEventArgs eventArgs)
        {
            RefreshInteractionPrompt();
        }

        private void HandleInteractableLost(CCS_InteractionEventArgs eventArgs)
        {
            RefreshInteractionPrompt();
        }

        private void HandleInteractionFailed(CCS_InteractionEventArgs eventArgs)
        {
            string message = eventArgs != null && !string.IsNullOrWhiteSpace(eventArgs.Message)
                ? eventArgs.Message
                : "Interaction failed.";

            if (message.IndexOf("Inventory", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                QueueNotification("Harvest failed: Inventory Full");
                return;
            }

            QueueNotification($"Harvest failed: {message}");
        }

        private void HandleInventoryItemAdded(CCS_InventoryEventArgs eventArgs)
        {
            RefreshInventorySnapshot();
            HudDataRefreshed?.Invoke(BuildEventArgs("Inventory item added."));
        }

        private void HandleHarvestCompleted(CCS_ResourceEventArgs eventArgs)
        {
            RefreshInventorySnapshot();
            QueueHarvestNotifications(eventArgs);
            HudDataRefreshed?.Invoke(BuildEventArgs("Resource harvest completed."));
        }

        private void HandleHarvestFailed(CCS_ResourceEventArgs eventArgs)
        {
            string message = eventArgs != null && !string.IsNullOrWhiteSpace(eventArgs.Message)
                ? eventArgs.Message
                : "Harvest failed.";

            QueueNotification(BuildHarvestFailureNotification(message));
        }

        private void HandleResourceDepleted(CCS_ResourceEventArgs eventArgs)
        {
            QueueNotification("Resource depleted");
            HudDataRefreshed?.Invoke(BuildEventArgs("Resource depleted."));
        }

        private void HandleResourceRespawned(CCS_ResourceEventArgs eventArgs)
        {
            QueueNotification("Resource respawned");
            HudDataRefreshed?.Invoke(BuildEventArgs("Resource respawned."));
        }

        private void QueueHarvestNotifications(CCS_ResourceEventArgs eventArgs)
        {
            if (eventArgs?.Drops == null || eventArgs.Drops.Count == 0)
            {
                QueueNotification("Harvest completed.");
                return;
            }

            for (int i = 0; i < eventArgs.Drops.Count; i++)
            {
                CCS_HarvestedItemDrop drop = eventArgs.Drops[i];
                if (drop?.ItemDefinition == null || drop.Quantity <= 0)
                {
                    continue;
                }

                string itemName = string.IsNullOrWhiteSpace(drop.ItemDefinition.DisplayName)
                    ? "Item"
                    : drop.ItemDefinition.DisplayName;

                QueueNotification($"Harvested {itemName} x{drop.Quantity}");
            }
        }

        private static string BuildHarvestFailureNotification(string message)
        {
            if (message.IndexOf("Inventory", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "Harvest failed: Inventory Full";
            }

            return $"Harvest failed: {message}";
        }

        private void HandleInventoryItemRemoved(CCS_InventoryEventArgs eventArgs)
        {
            RefreshInventorySnapshot();
            QueueNotification(BuildInventoryNotification("Item Removed", eventArgs));
            HudDataRefreshed?.Invoke(BuildEventArgs("Inventory item removed."));
        }

        private void HandleInventoryChanged(CCS_InventoryEventArgs eventArgs)
        {
            RefreshInventorySnapshot();
            HudDataRefreshed?.Invoke(BuildEventArgs("Inventory changed."));
        }

        private void HandleEquipmentItemEquipped(CCS_EquipmentEventArgs eventArgs)
        {
            RefreshEquipmentSnapshot();
            QueueNotification(BuildEquipmentNotification("Item Equipped", eventArgs));
            HudDataRefreshed?.Invoke(BuildEventArgs("Equipment item equipped."));
        }

        private void HandleEquipmentItemUnequipped(CCS_EquipmentEventArgs eventArgs)
        {
            RefreshEquipmentSnapshot();
            QueueNotification(BuildEquipmentNotification("Item Unequipped", eventArgs));
            HudDataRefreshed?.Invoke(BuildEventArgs("Equipment item unequipped."));
        }

        private void HandleEquipmentChanged(CCS_EquipmentEventArgs eventArgs)
        {
            RefreshEquipmentSnapshot();
            HudDataRefreshed?.Invoke(BuildEventArgs("Equipment changed."));
        }

        private void RefreshSurvivalSnapshots()
        {
            if (survivalCoreService == null || !survivalCoreService.IsInitialized)
            {
                healthSnapshot = default;
                staminaSnapshot = default;
                hungerSnapshot = default;
                thirstSnapshot = default;
                fatigueSnapshot = default;
                temperatureSnapshot = default;
                return;
            }

            survivalCoreService.TryGetSnapshot(CCS_SurvivalStatType.Health, out healthSnapshot);
            survivalCoreService.TryGetSnapshot(CCS_SurvivalStatType.Stamina, out staminaSnapshot);
            survivalCoreService.TryGetSnapshot(CCS_SurvivalStatType.Hunger, out hungerSnapshot);
            survivalCoreService.TryGetSnapshot(CCS_SurvivalStatType.Thirst, out thirstSnapshot);
            survivalCoreService.TryGetSnapshot(CCS_SurvivalStatType.Fatigue, out fatigueSnapshot);
            survivalCoreService.TryGetSnapshot(CCS_SurvivalStatType.Temperature, out temperatureSnapshot);
        }

        private void RefreshInteractionPrompt()
        {
            string previousPrompt = currentInteractionPrompt;

            if (interactionService == null || !interactionService.IsInitialized || !interactionService.HasCurrentTarget)
            {
                currentInteractionPrompt = string.Empty;
            }
            else
            {
                CCS_IInteractable target = interactionService.CurrentTarget;
                if (target != null && target.CanInteract())
                {
                    string displayName = target.GetInteractionDisplayName();
                    currentInteractionPrompt = string.IsNullOrWhiteSpace(displayName)
                        ? "Interact"
                        : $"Interact: {displayName}";
                }
                else
                {
                    currentInteractionPrompt = string.Empty;
                }
            }

            if (!string.Equals(previousPrompt, currentInteractionPrompt, StringComparison.Ordinal))
            {
                InteractionPromptChanged?.Invoke(BuildEventArgs("Interaction prompt changed."));
            }
        }

        private void RefreshInventorySnapshot()
        {
            inventorySnapshot = inventoryService != null && inventoryService.IsInitialized
                ? inventoryService.CreateSnapshot()
                : new CCS_InventorySnapshot(Array.Empty<CCS_ItemStack>(), 0, 0, 0);
        }

        private void RefreshEquipmentSnapshot()
        {
            equipmentSnapshot = equipmentService != null && equipmentService.IsInitialized
                ? equipmentService.CreateSnapshot()
                : new CCS_EquipmentSnapshot(Array.Empty<CCS_EquippedItem>(), 0, 0, 0, 0f);
        }

        private static string BuildInventoryNotification(string prefix, CCS_InventoryEventArgs eventArgs)
        {
            if (eventArgs == null)
            {
                return prefix;
            }

            string itemName = eventArgs.ItemDefinition != null
                ? eventArgs.ItemDefinition.DisplayName
                : "Item";

            if (string.IsNullOrWhiteSpace(itemName))
            {
                itemName = "Item";
            }

            return eventArgs.Quantity > 0
                ? $"{prefix}: {itemName} x{eventArgs.Quantity}"
                : $"{prefix}: {itemName}";
        }

        private static string BuildEquipmentNotification(string prefix, CCS_EquipmentEventArgs eventArgs)
        {
            if (eventArgs == null)
            {
                return prefix;
            }

            string itemName = ResolveEquipmentDisplayName(eventArgs);
            return $"{prefix}: {itemName}";
        }

        private static string ResolveEquipmentDisplayName(CCS_EquipmentEventArgs eventArgs)
        {
            if (eventArgs.EquipmentDefinition?.ItemDefinition != null &&
                !string.IsNullOrWhiteSpace(eventArgs.EquipmentDefinition.ItemDefinition.DisplayName))
            {
                return eventArgs.EquipmentDefinition.ItemDefinition.DisplayName;
            }

            if (eventArgs.EquipmentDefinition != null &&
                !string.IsNullOrWhiteSpace(eventArgs.EquipmentDefinition.name))
            {
                return eventArgs.EquipmentDefinition.name;
            }

            return "Equipment";
        }

        private CCS_HudEventArgs BuildEventArgs(string message)
        {
            return new CCS_HudEventArgs(
                message,
                currentInteractionPrompt,
                inventorySnapshot,
                equipmentSnapshot,
                EffectiveInventorySlotCount,
                healthSnapshot,
                staminaSnapshot,
                hungerSnapshot,
                thirstSnapshot,
                fatigueSnapshot,
                temperatureSnapshot);
        }

        private void UnbindSurvivalCoreService()
        {
            if (survivalCoreService == null)
            {
                return;
            }

            survivalCoreService.StatChanged -= HandleStatChanged;
            survivalCoreService = null;
        }

        private void UnbindInteractionService()
        {
            if (interactionService == null)
            {
                return;
            }

            interactionService.InteractableFound -= HandleInteractableFound;
            interactionService.InteractableLost -= HandleInteractableLost;
            interactionService.InteractionFailed -= HandleInteractionFailed;
            interactionService = null;
        }

        private void UnbindInventoryService()
        {
            if (inventoryService == null)
            {
                return;
            }

            inventoryService.ItemAdded -= HandleInventoryItemAdded;
            inventoryService.ItemRemoved -= HandleInventoryItemRemoved;
            inventoryService.InventoryChanged -= HandleInventoryChanged;
            inventoryService = null;
        }

        private void UnbindEquipmentService()
        {
            if (equipmentService == null)
            {
                return;
            }

            equipmentService.ItemEquipped -= HandleEquipmentItemEquipped;
            equipmentService.ItemUnequipped -= HandleEquipmentItemUnequipped;
            equipmentService.EquipmentChanged -= HandleEquipmentChanged;
            equipmentService = null;
        }

        private void UnbindResourceHarvestService()
        {
            if (resourceHarvestService == null)
            {
                return;
            }

            resourceHarvestService.HarvestCompleted -= HandleHarvestCompleted;
            resourceHarvestService.HarvestFailed -= HandleHarvestFailed;
            resourceHarvestService.ResourceDepleted -= HandleResourceDepleted;
            resourceHarvestService = null;
        }

        private void UnbindResourceRespawnService()
        {
            if (resourceRespawnService == null)
            {
                return;
            }

            resourceRespawnService.ResourceRespawned -= HandleResourceRespawned;
            resourceRespawnService = null;
        }

        #endregion
    }
}
