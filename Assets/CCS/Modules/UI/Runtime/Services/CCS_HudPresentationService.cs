using System;
using CCS.Modules.Equipment;
using CCS.Modules.Interaction;
using CCS.Modules.Inventory;
using CCS.Modules.SurvivalCore;
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

        private string currentInteractionPrompt = string.Empty;
        private CCS_InventorySnapshot inventorySnapshot;
        private CCS_EquipmentSnapshot equipmentSnapshot;
        private CCS_SurvivalStatSnapshot healthSnapshot;
        private CCS_SurvivalStatSnapshot staminaSnapshot;
        private CCS_SurvivalStatSnapshot hungerSnapshot;
        private CCS_SurvivalStatSnapshot thirstSnapshot;

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

            equipmentService.EquipmentChanged += HandleEquipmentChanged;
            RefreshEquipmentSnapshot();
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

        private void HandleInventoryChanged(CCS_InventoryEventArgs eventArgs)
        {
            RefreshInventorySnapshot();
            HudDataRefreshed?.Invoke(BuildEventArgs("Inventory changed."));
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
                return;
            }

            survivalCoreService.TryGetSnapshot(CCS_SurvivalStatType.Health, out healthSnapshot);
            survivalCoreService.TryGetSnapshot(CCS_SurvivalStatType.Stamina, out staminaSnapshot);
            survivalCoreService.TryGetSnapshot(CCS_SurvivalStatType.Hunger, out hungerSnapshot);
            survivalCoreService.TryGetSnapshot(CCS_SurvivalStatType.Thirst, out thirstSnapshot);
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
                currentInteractionPrompt = target != null && target.CanInteract()
                    ? $"[E] {target.GetInteractionDisplayName()}"
                    : string.Empty;
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

        private CCS_HudEventArgs BuildEventArgs(string message)
        {
            return new CCS_HudEventArgs(
                message,
                currentInteractionPrompt,
                inventorySnapshot,
                equipmentSnapshot,
                healthSnapshot,
                staminaSnapshot,
                hungerSnapshot,
                thirstSnapshot);
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
            interactionService = null;
        }

        private void UnbindInventoryService()
        {
            if (inventoryService == null)
            {
                return;
            }

            inventoryService.InventoryChanged -= HandleInventoryChanged;
            inventoryService = null;
        }

        private void UnbindEquipmentService()
        {
            if (equipmentService == null)
            {
                return;
            }

            equipmentService.EquipmentChanged -= HandleEquipmentChanged;
            equipmentService = null;
        }

        #endregion
    }
}
