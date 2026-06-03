using CCS.Modules.Crafting;
using CCS.Modules.Inventory;
using CCS.Modules.SaveLoad;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_InventoryEquipmentPersistenceTestHarness
// CATEGORY: Modules / Equipment / Runtime / Testing
// PURPOSE: Development-only harvest/craft/equip/save/load persistence verification harness.
// PLACEMENT: Bootstrap verification scenes only. Disable for shipping builds.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-28
// NOTES: Logs pass/fail results. Uses manual save/load debug slot persistence_test.
//        Milestone 2.1.2 — waiting-state logs emit once until state changes.
// =============================================================================

namespace CCS.Modules.Equipment
{
    [DefaultExecutionOrder(290)]
    public sealed class CCS_InventoryEquipmentPersistenceTestHarness : MonoBehaviour
    {
        private const string LogPrefix = "[CCS_InventoryEquipmentPersistenceTestHarness]";
        private const string PersistenceTestSlotId = "persistence_test";

        private enum HarnessWaitingState
        {
            None = 0,
            WaitingForServices = 1,
            WaitingForCampfireKit = 2,
            WaitingToEquip = 3,
            WaitingForSave = 4,
            WaitingForLoad = 5
        }

        #region Variables

        [Header("Development Testing")]
        [Tooltip("When enabled, the harness runs harvest/craft/equip/save/load verification.")]
        [SerializeField] private bool enableHarness = false;

        [Tooltip("When true, repeats waiting-state diagnostics after the check interval.")]
        [SerializeField] private bool verboseWaitingLogs = false;

        [Tooltip("Seconds between harness state checks.")]
        [SerializeField] private float checkIntervalSeconds = 2f;

        [Tooltip("Campfire kit item expected after crafting completes.")]
        [SerializeField] private CCS_ItemDefinition campfireKitItem;

        [Tooltip("Campfire kit equipment definition attempted after craft output exists.")]
        [SerializeField] private CCS_EquipmentItemDefinition campfireKitEquipment;

        [Tooltip("Optional debug controller used for manual save/load calls.")]
        [SerializeField] private CCS_SaveLoadDebugController saveLoadDebugController;

        private float nextCheckTime;
        private bool persistenceTestCompleted;
        private bool baselineCaptured;
        private int baselineCampfireKitQuantity;
        private bool baselineCampfireEquipped;
        private HarnessWaitingState lastLoggedWaitingState = HarnessWaitingState.None;

        #endregion

        #region Unity Callbacks

        private void Update()
        {
            if (!enableHarness || persistenceTestCompleted)
            {
                return;
            }

            if (Time.time < nextCheckTime)
            {
                return;
            }

            nextCheckTime = Time.time + checkIntervalSeconds;
            TryAdvancePersistenceTest();
        }

        #endregion

        #region Private Methods

        private void TryAdvancePersistenceTest()
        {
            if (!TryResolveServices(
                    out CCS_PlayerInventoryService inventoryService,
                    out CCS_PlayerEquipmentService equipmentService,
                    out CCS_CraftingService craftingService,
                    out CCS_SaveLoadService saveLoadService,
                    out CCS_SaveLoadDebugController debugController))
            {
                LogWaitingStateOnce(
                    HarnessWaitingState.WaitingForServices,
                    "Waiting for inventory, equipment, crafting, save/load services.");
                return;
            }

            ClearWaitingStateIfResolved(HarnessWaitingState.WaitingForServices);

            if (campfireKitItem == null || campfireKitEquipment == null)
            {
                Debug.LogWarning($"{LogPrefix} Missing test item or equipment references.");
                return;
            }

            if (!inventoryService.HasItem(campfireKitItem, 1))
            {
                LogWaitingStateOnce(
                    HarnessWaitingState.WaitingForCampfireKit,
                    "Waiting for crafted campfire kit in inventory.");
                return;
            }

            ClearWaitingStateIfResolved(HarnessWaitingState.WaitingForCampfireKit);

            if (!equipmentService.IsSlotOccupied(campfireKitEquipment.AllowedSlot))
            {
                bool equipSucceeded = equipmentService.EquipItem(campfireKitEquipment);
                if (equipSucceeded)
                {
                    ClearWaitingStateIfResolved(HarnessWaitingState.WaitingToEquip);
                    Debug.Log($"{LogPrefix} Equipped test campfire kit equipment.");
                }
                else
                {
                    LogWaitingStateOnce(
                        HarnessWaitingState.WaitingToEquip,
                        "Waiting to equip test campfire kit equipment.");
                    return;
                }
            }

            ClearWaitingStateIfResolved(HarnessWaitingState.WaitingToEquip);

            if (!baselineCaptured)
            {
                baselineCampfireKitQuantity = inventoryService.GetQuantity(campfireKitItem);
                baselineCampfireEquipped = equipmentService.IsSlotOccupied(campfireKitEquipment.AllowedSlot);
                baselineCaptured = true;
                Debug.Log($"{LogPrefix} Baseline captured. Starting persistence save/load cycle.");
            }

            debugController.SetSelectedSlotId(PersistenceTestSlotId);
            CCS_SaveLoadResult saveResult = debugController.ManualSaveSelectedSlot();
            if (!saveResult.IsSuccess)
            {
                Debug.LogWarning($"{LogPrefix} Persistence save failed: {saveResult.Message}");
                return;
            }

            inventoryService.ClearInventory();
            equipmentService.ClearAllEquipment();

            CCS_SaveLoadResult loadResult = debugController.ManualLoadSelectedSlot();
            if (!loadResult.IsSuccess)
            {
                Debug.LogError($"{LogPrefix} FAIL — persistence load failed: {loadResult.Message}");
                persistenceTestCompleted = true;
                return;
            }

            bool quantityMatches = inventoryService.GetQuantity(campfireKitItem) == baselineCampfireKitQuantity;
            bool equipmentMatches =
                equipmentService.IsSlotOccupied(campfireKitEquipment.AllowedSlot) == baselineCampfireEquipped;

            if (quantityMatches && equipmentMatches)
            {
                Debug.Log($"{LogPrefix} PASS — inventory and equipment restored after save/load.");
            }
            else
            {
                Debug.LogError(
                    $"{LogPrefix} FAIL — restored state mismatch. " +
                    $"CampfireQty={inventoryService.GetQuantity(campfireKitItem)} expected {baselineCampfireKitQuantity}, " +
                    $"Equipped={equipmentService.IsSlotOccupied(campfireKitEquipment.AllowedSlot)} expected {baselineCampfireEquipped}.");
            }

            persistenceTestCompleted = true;
        }

        private bool TryResolveServices(
            out CCS_PlayerInventoryService inventoryService,
            out CCS_PlayerEquipmentService equipmentService,
            out CCS_CraftingService craftingService,
            out CCS_SaveLoadService saveLoadService,
            out CCS_SaveLoadDebugController debugController)
        {
            inventoryService = null;
            equipmentService = null;
            craftingService = null;
            saveLoadService = null;
            debugController = saveLoadDebugController;

            if (!CCS_CraftingRuntimeBridge.TryGetInventoryService(out inventoryService)
                || !inventoryService.IsInitialized)
            {
                return false;
            }

            if (!CCS_EquipmentRuntimeBridge.TryGetEquipmentService(out equipmentService)
                || !equipmentService.IsInitialized)
            {
                return false;
            }

            if (!CCS_CraftingRuntimeBridge.TryGetCraftingService(out craftingService)
                || !craftingService.IsInitialized)
            {
                return false;
            }

            if (!CCS_SaveLoadRuntimeBridge.TryGetSaveLoadService(out saveLoadService)
                || !saveLoadService.IsInitialized)
            {
                return false;
            }

            if (debugController == null)
            {
                debugController = Object.FindAnyObjectByType<CCS_SaveLoadDebugController>();
                saveLoadDebugController = debugController;
            }

            return debugController != null && debugController.EnableDebugControls;
        }

        private void LogWaitingStateOnce(HarnessWaitingState waitingState, string message)
        {
            if (!verboseWaitingLogs && lastLoggedWaitingState == waitingState)
            {
                return;
            }

            lastLoggedWaitingState = waitingState;
            Debug.Log($"{LogPrefix} {message}");
        }

        private void ClearWaitingStateIfResolved(HarnessWaitingState resolvedState)
        {
            if (lastLoggedWaitingState == resolvedState)
            {
                lastLoggedWaitingState = HarnessWaitingState.None;
            }
        }

        #endregion
    }
}
