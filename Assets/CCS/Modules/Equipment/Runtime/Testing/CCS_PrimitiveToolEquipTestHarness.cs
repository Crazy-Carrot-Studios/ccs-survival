using CCS.Modules.Crafting;
using CCS.Modules.Inventory;
using CCS.Modules.SaveLoad;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_PrimitiveToolEquipTestHarness
// CATEGORY: Modules / Equipment / Runtime / Testing
// PURPOSE: Verifies primitive tool equip, HUD refresh, capacity, and save/load persistence.
// PLACEMENT: Bootstrap verification scenes only. Disabled by default.
// AUTHOR: James Schilz
// CREATED: 2026-05-31
// NOTES: No combat or wildlife logic in 0.9.2 foundation.
// =============================================================================

namespace CCS.Modules.Equipment
{
    [DefaultExecutionOrder(285)]
    public sealed class CCS_PrimitiveToolEquipTestHarness : MonoBehaviour
    {
        private const string LogPrefix = "[CCS_PrimitiveToolEquipTestHarness]";
        private const string PersistenceTestSlotId = "primitive_tool_equip_test";

        private enum HarnessPhase
        {
            WaitingForServices = 0,
            EquipKnife = 1,
            VerifyKnifePersistence = 2,
            EquipBoneHatchet = 3,
            EquipBonePick = 4,
            Complete = 5
        }

        #region Variables

        [Header("Development Testing")]
        [Tooltip("When enabled, verifies knife, bone hatchet, and bone pick equip paths.")]
        [SerializeField] private bool enableHarness = false;

        [Tooltip("Seconds between harness state checks.")]
        [SerializeField] private float checkIntervalSeconds = 2f;

        [Tooltip("Starter knife equipment definition for MainHand verification.")]
        [SerializeField] private CCS_EquipmentItemDefinition knifeEquipment;

        [Tooltip("Bone hatchet equipment definition for Tool slot verification.")]
        [SerializeField] private CCS_EquipmentItemDefinition boneHatchetEquipment;

        [Tooltip("Bone pick equipment definition for Tool slot verification.")]
        [SerializeField] private CCS_EquipmentItemDefinition bonePickEquipment;

        [Tooltip("Optional debug controller used for save/load persistence verification.")]
        [SerializeField] private CCS_SaveLoadDebugController saveLoadDebugController;

        private float nextCheckTime;
        private HarnessPhase currentPhase = HarnessPhase.WaitingForServices;
        private int baselineInventorySlotCount;

        #endregion

        #region Unity Callbacks

        private void Update()
        {
            if (!enableHarness || currentPhase == HarnessPhase.Complete)
            {
                return;
            }

            if (Time.time < nextCheckTime)
            {
                return;
            }

            nextCheckTime = Time.time + checkIntervalSeconds;
            AdvanceHarness();
        }

        #endregion

        #region Private Methods

        private void AdvanceHarness()
        {
            if (!TryResolveServices(
                    out CCS_PlayerInventoryService inventoryService,
                    out CCS_PlayerEquipmentService equipmentService,
                    out CCS_SaveLoadDebugController debugController))
            {
                return;
            }

            if (knifeEquipment == null || boneHatchetEquipment == null || bonePickEquipment == null)
            {
                Debug.LogWarning($"{LogPrefix} Missing primitive tool equipment references.");
                currentPhase = HarnessPhase.Complete;
                return;
            }

            switch (currentPhase)
            {
                case HarnessPhase.WaitingForServices:
                    baselineInventorySlotCount = inventoryService.CreateSnapshot().SlotCount;
                    currentPhase = HarnessPhase.EquipKnife;
                    break;

                case HarnessPhase.EquipKnife:
                    if (!TryEquipIfNeeded(equipmentService, knifeEquipment))
                    {
                        return;
                    }

                    if (!ValidateInventoryCapacity(inventoryService, "knife equip"))
                    {
                        currentPhase = HarnessPhase.Complete;
                        return;
                    }

                    currentPhase = HarnessPhase.VerifyKnifePersistence;
                    break;

                case HarnessPhase.VerifyKnifePersistence:
                    debugController.SetSelectedSlotId(PersistenceTestSlotId);
                    if (!debugController.ManualSaveSelectedSlot().IsSuccess)
                    {
                        return;
                    }

                    equipmentService.ClearAllEquipment();
                    if (!debugController.ManualLoadSelectedSlot().IsSuccess)
                    {
                        Debug.LogError($"{LogPrefix} FAIL — knife persistence load failed.");
                        currentPhase = HarnessPhase.Complete;
                        return;
                    }

                    if (!equipmentService.IsSlotOccupied(knifeEquipment.AllowedSlot))
                    {
                        Debug.LogError($"{LogPrefix} FAIL — knife was not restored after save/load.");
                        currentPhase = HarnessPhase.Complete;
                        return;
                    }

                    Debug.Log($"{LogPrefix} Knife equip persistence verified.");
                    currentPhase = HarnessPhase.EquipBoneHatchet;
                    break;

                case HarnessPhase.EquipBoneHatchet:
                    if (!TryEquipIfNeeded(equipmentService, boneHatchetEquipment))
                    {
                        return;
                    }

                    if (!ValidateInventoryCapacity(inventoryService, "bone hatchet equip"))
                    {
                        currentPhase = HarnessPhase.Complete;
                        return;
                    }

                    currentPhase = HarnessPhase.EquipBonePick;
                    break;

                case HarnessPhase.EquipBonePick:
                    if (!TryEquipIfNeeded(equipmentService, bonePickEquipment))
                    {
                        return;
                    }

                    if (!ValidateInventoryCapacity(inventoryService, "bone pick equip"))
                    {
                        currentPhase = HarnessPhase.Complete;
                        return;
                    }

                    Debug.Log($"{LogPrefix} PASS — knife, bone hatchet, and bone pick equip verified.");
                    currentPhase = HarnessPhase.Complete;
                    break;
            }
        }

        private static bool TryEquipIfNeeded(
            CCS_PlayerEquipmentService equipmentService,
            CCS_EquipmentItemDefinition equipmentDefinition)
        {
            if (equipmentService.IsSlotOccupied(equipmentDefinition.AllowedSlot))
            {
                CCS_EquippedItem equippedItem = equipmentService.GetEquippedItem(equipmentDefinition.AllowedSlot);
                if (equippedItem?.EquipmentDefinition == equipmentDefinition)
                {
                    return true;
                }

                equipmentService.UnequipItem(equipmentDefinition.AllowedSlot);
            }

            bool equipSucceeded = equipmentService.EquipItem(equipmentDefinition);
            if (equipSucceeded)
            {
                Debug.Log($"{LogPrefix} Equipped {equipmentDefinition.ItemDefinition.DisplayName}.");
            }

            return equipSucceeded;
        }

        private bool ValidateInventoryCapacity(
            CCS_PlayerInventoryService inventoryService,
            string stepLabel)
        {
            int currentSlotCount = inventoryService.CreateSnapshot().SlotCount;
            if (currentSlotCount == baselineInventorySlotCount)
            {
                return true;
            }

            Debug.LogError(
                $"{LogPrefix} FAIL — inventory capacity changed after {stepLabel}. " +
                $"Current={currentSlotCount}, Baseline={baselineInventorySlotCount}.");
            return false;
        }

        private bool TryResolveServices(
            out CCS_PlayerInventoryService inventoryService,
            out CCS_PlayerEquipmentService equipmentService,
            out CCS_SaveLoadDebugController debugController)
        {
            inventoryService = null;
            equipmentService = null;
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

            if (!CCS_SaveLoadRuntimeBridge.TryGetSaveLoadService(out CCS_SaveLoadService saveLoadService)
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

        #endregion
    }
}
