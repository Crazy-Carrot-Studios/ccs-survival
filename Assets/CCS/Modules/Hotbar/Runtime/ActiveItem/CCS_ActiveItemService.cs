using System.Collections.Generic;
using CCS.Modules.Combat;
using CCS.Modules.Equipment;
using CCS.Modules.Inventory;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_ActiveItemService
// CATEGORY: Modules / Hotbar / Runtime / ActiveItem
// PURPOSE: Tracks and uses the player's active item selection in a service-driven flow.
// PLACEMENT: Registered by CCS_SurvivalGameplayServiceRegistration.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Does not spawn duplicate visuals. Equipment visuals remain authoritative.
// =============================================================================

namespace CCS.Modules.Hotbar
{
    public sealed class CCS_ActiveItemService : CCS_ISurvivalService
    {
        private const string LogPrefix = "[CCS_ActiveItemService]";

        #region Variables

        private CCS_ActiveItemProfile activeProfile;
        private CCS_PlayerEquipmentService equipmentService;
        private CCS_CombatService combatService;
        private CCS_ActiveItemState activeState = CCS_ActiveItemState.Empty;
        private CCS_ActiveItemUseResult lastUseResult;
        private float lastUseTime = -999f;
        private bool isInitialized;
        private readonly List<CCS_EquipmentSlotType> cycleSlotBuffer = new List<CCS_EquipmentSlotType>();

        #endregion

        #region Events

        public event ActiveItemChangedHandler ActiveItemChanged;
        public event ActiveItemUsedHandler ActiveItemUsed;

        #endregion

        #region Properties

        public bool IsInitialized => isInitialized;

        public CCS_ActiveItemProfile ActiveProfile => activeProfile;

        public CCS_ActiveItemState ActiveState => activeState;

        public CCS_ActiveItemUseResult LastUseResult => lastUseResult;

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

        public void InitializeFromProfile(CCS_ActiveItemProfile profile)
        {
            if (profile == null)
            {
                isInitialized = false;
                return;
            }

            CCS_SurvivalValidationResult validation = CCS_ActiveItemValidationUtility.ValidateProfile(profile);
            if (!validation.IsSuccess)
            {
                Debug.LogWarning($"{LogPrefix} Profile validation warning: {validation.Message}");
            }

            activeProfile = profile;
            isInitialized = true;
        }

        public void BindEquipmentService(CCS_PlayerEquipmentService service)
        {
            UnbindEquipmentService();
            equipmentService = service;

            if (equipmentService != null && equipmentService.IsInitialized)
            {
                equipmentService.ItemEquipped += OnItemEquipped;
                equipmentService.ItemUnequipped += OnItemUnequipped;
                equipmentService.EquipmentChanged += OnEquipmentChanged;
            }
        }

        public void BindCombatService(CCS_CombatService service)
        {
            combatService = service;
        }

        public void UnbindEquipmentService()
        {
            if (equipmentService == null)
            {
                return;
            }

            equipmentService.ItemEquipped -= OnItemEquipped;
            equipmentService.ItemUnequipped -= OnItemUnequipped;
            equipmentService.EquipmentChanged -= OnEquipmentChanged;
            equipmentService = null;
        }

        public bool SelectActiveFromEquipped(CCS_EquipmentSlotType equipmentSlot)
        {
            if (!EnsureInitialized() || equipmentService == null || !equipmentService.IsInitialized)
            {
                return false;
            }

            CCS_EquippedItem equippedItem = equipmentService.GetEquippedItem(equipmentSlot);
            if (equippedItem?.ItemDefinition == null)
            {
                return false;
            }

            return SelectActiveItem(
                equippedItem.ItemDefinition.ItemId,
                CCS_ActiveItemSlotType.Equipped,
                equipmentSlot,
                -1,
                equippedItem.ItemDefinition);
        }

        public bool SelectActiveItem(
            string itemId,
            CCS_ActiveItemSlotType sourceSlotType,
            CCS_EquipmentSlotType equipmentSlot = CCS_EquipmentSlotType.MainHand,
            int hotbarSlotIndex = -1,
            CCS_ItemDefinition itemDefinition = null)
        {
            if (!EnsureInitialized() || string.IsNullOrWhiteSpace(itemId))
            {
                return false;
            }

            CCS_ActiveItemBehaviorType behaviorType =
                CCS_ActiveItemBehaviorUtility.ResolveBehaviorType(itemDefinition);
            bool canUse = CCS_ActiveItemBehaviorUtility.CanUseBehavior(behaviorType);

            CCS_ActiveItemState previousState = activeState;
            activeState = new CCS_ActiveItemState(
                itemId,
                sourceSlotType,
                equipmentSlot,
                hotbarSlotIndex,
                itemDefinition,
                behaviorType,
                canUse,
                itemId);

            if (activeProfile != null && activeProfile.EnableDebugLogs)
            {
                Debug.Log($"{LogPrefix} Active item set to {itemId} ({behaviorType}).");
            }

            ActiveItemChanged?.Invoke(previousState, activeState);
            return true;
        }

        public bool SelectActiveFromTestHarness(string itemId, CCS_ItemDefinition itemDefinition)
        {
            return SelectActiveItem(
                itemId,
                CCS_ActiveItemSlotType.TestHarness,
                CCS_EquipmentSlotType.MainHand,
                -1,
                itemDefinition);
        }

        public bool CycleActiveEquippedItem()
        {
            if (!EnsureInitialized()
                || equipmentService == null
                || !equipmentService.IsInitialized
                || activeProfile == null
                || !activeProfile.EnableEquipmentSlotCycling)
            {
                return false;
            }

            BuildOccupiedEquipmentSlots();
            if (cycleSlotBuffer.Count == 0)
            {
                ClearActiveItem();
                return false;
            }

            int currentIndex = cycleSlotBuffer.IndexOf(activeState.EquipmentSlot);
            int nextIndex = currentIndex < 0 ? 0 : (currentIndex + 1) % cycleSlotBuffer.Count;
            return SelectActiveFromEquipped(cycleSlotBuffer[nextIndex]);
        }

        public void ClearActiveItem()
        {
            if (!activeState.HasActiveItem)
            {
                return;
            }

            CCS_ActiveItemState previousState = activeState;
            activeState = CCS_ActiveItemState.Empty;
            ActiveItemChanged?.Invoke(previousState, activeState);

            if (activeProfile != null && activeProfile.EnableDebugLogs)
            {
                Debug.Log($"{LogPrefix} Active item cleared.");
            }
        }

        public CCS_ActiveItemUseResult TryUseActiveItem(CCS_ActiveItemUseRequest request)
        {
            if (!EnsureInitialized())
            {
                lastUseResult = new CCS_ActiveItemUseResult(
                    CCS_ActiveItemUseResultType.ServiceUnavailable,
                    "Active item service is unavailable.",
                    false);
                RaiseUsed(lastUseResult);
                return lastUseResult;
            }

            if (!activeState.HasActiveItem)
            {
                lastUseResult = CCS_ActiveItemUseResult.NoActiveItem();
                RaiseUsed(lastUseResult);
                return lastUseResult;
            }

            if (activeProfile != null
                && activeProfile.UseCooldownSeconds > 0f
                && Time.time < lastUseTime + activeProfile.UseCooldownSeconds)
            {
                lastUseResult = new CCS_ActiveItemUseResult(
                    CCS_ActiveItemUseResultType.OnCooldown,
                    "Active item use is on cooldown.",
                    true,
                    activeState.ActiveItemId);
                RaiseUsed(lastUseResult);
                return lastUseResult;
            }

            lastUseResult = RouteUse(activeState, request);
            lastUseTime = Time.time;
            RaiseUsed(lastUseResult);
            return lastUseResult;
        }

        public CCS_ActiveItemSnapshot CreateSnapshot()
        {
            return new CCS_ActiveItemSnapshot(
                activeState.ActiveItemId,
                activeState.SourceSlotType,
                activeState.HotbarSlotIndex,
                activeState.BehaviorType,
                activeState.CanUse,
                activeState.ActiveVisualItemId,
                lastUseResult.Message);
        }

        #endregion

        #region Event Handlers

        private void OnItemEquipped(CCS_EquipmentEventArgs eventArgs)
        {
            if (activeProfile == null || !activeProfile.AutoSelectMainHandOnEquip)
            {
                return;
            }

            if (eventArgs?.Slot == CCS_EquipmentSlotType.MainHand
                && eventArgs.EquippedItem?.ItemDefinition != null)
            {
                SelectActiveFromEquipped(CCS_EquipmentSlotType.MainHand);
            }
        }

        private void OnItemUnequipped(CCS_EquipmentEventArgs eventArgs)
        {
            if (!activeState.HasActiveItem || eventArgs?.EquippedItem?.ItemDefinition == null)
            {
                return;
            }

            if (eventArgs.EquippedItem.ItemDefinition.ItemId == activeState.ActiveItemId)
            {
                ClearActiveItem();
            }
        }

        private void OnEquipmentChanged(CCS_EquipmentEventArgs eventArgs)
        {
            string message = eventArgs?.Message ?? string.Empty;
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            if (message.Contains("cleared", System.StringComparison.OrdinalIgnoreCase)
                || message.Contains("restored", System.StringComparison.OrdinalIgnoreCase))
            {
                ClearActiveItem();
                if (activeProfile != null && activeProfile.AutoSelectMainHandOnEquip)
                {
                    SelectActiveFromEquipped(CCS_EquipmentSlotType.MainHand);
                }
            }
        }

        #endregion

        #region Private Methods

        private CCS_ActiveItemUseResult RouteUse(CCS_ActiveItemState state, CCS_ActiveItemUseRequest request)
        {
            switch (state.BehaviorType)
            {
                case CCS_ActiveItemBehaviorType.Weapon:
                    return TryUseWeapon(state, request);
                case CCS_ActiveItemBehaviorType.Tool:
                case CCS_ActiveItemBehaviorType.Consumable:
                case CCS_ActiveItemBehaviorType.Placeable:
                case CCS_ActiveItemBehaviorType.Generic:
                default:
                    return CCS_ActiveItemUseResult.NoBehavior(state.ActiveItemId);
            }
        }

        private CCS_ActiveItemUseResult TryUseWeapon(CCS_ActiveItemState state, CCS_ActiveItemUseRequest request)
        {
            if (combatService == null || !combatService.IsInitialized)
            {
                return new CCS_ActiveItemUseResult(
                    CCS_ActiveItemUseResultType.ServiceUnavailable,
                    "Combat service is unavailable.",
                    false,
                    state.ActiveItemId);
            }

            if (!IsActiveWeaponEquippedInMainHand(state))
            {
                return new CCS_ActiveItemUseResult(
                    CCS_ActiveItemUseResultType.WeaponNotEquipped,
                    "Active weapon must be equipped in the main hand to attack.",
                    true,
                    state.ActiveItemId);
            }

            CCS_CombatHitResult combatResult =
                combatService.TryMeleeAttack(request.UseOrigin, request.UseDirection);

            if (combatResult == null)
            {
                return new CCS_ActiveItemUseResult(
                    CCS_ActiveItemUseResultType.ServiceUnavailable,
                    "Combat result was unavailable.",
                    false,
                    state.ActiveItemId);
            }

            if (combatResult.DidHitWildlife)
            {
                return new CCS_ActiveItemUseResult(
                    CCS_ActiveItemUseResultType.CombatHit,
                    combatResult.Message,
                    true,
                    state.ActiveItemId);
            }

            if (combatResult.Message != null
                && combatResult.Message.Contains("cooldown", System.StringComparison.OrdinalIgnoreCase))
            {
                return new CCS_ActiveItemUseResult(
                    CCS_ActiveItemUseResultType.OnCooldown,
                    combatResult.Message,
                    true,
                    state.ActiveItemId);
            }

            return new CCS_ActiveItemUseResult(
                CCS_ActiveItemUseResultType.NoTarget,
                string.IsNullOrWhiteSpace(combatResult.Message)
                    ? "No wildlife target in range."
                    : combatResult.Message,
                true,
                state.ActiveItemId);
        }

        private bool IsActiveWeaponEquippedInMainHand(CCS_ActiveItemState state)
        {
            if (equipmentService == null || !equipmentService.IsInitialized)
            {
                return state.SourceSlotType == CCS_ActiveItemSlotType.TestHarness;
            }

            CCS_EquippedItem mainHand = equipmentService.GetEquippedItem(CCS_EquipmentSlotType.MainHand);
            return mainHand?.ItemDefinition != null
                && mainHand.ItemDefinition.ItemId == state.ActiveItemId;
        }

        private void BuildOccupiedEquipmentSlots()
        {
            cycleSlotBuffer.Clear();
            foreach (CCS_EquipmentSlotType slotType in System.Enum.GetValues(typeof(CCS_EquipmentSlotType)))
            {
                if (equipmentService.IsSlotOccupied(slotType))
                {
                    cycleSlotBuffer.Add(slotType);
                }
            }
        }

        private void RaiseUsed(CCS_ActiveItemUseResult result)
        {
            ActiveItemUsed?.Invoke(result);
        }

        private bool EnsureInitialized()
        {
            return isInitialized;
        }

        #endregion
    }
}
