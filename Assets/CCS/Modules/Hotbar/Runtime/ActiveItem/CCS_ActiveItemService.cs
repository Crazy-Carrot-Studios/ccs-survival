using System.Collections.Generic;
using CCS.Modules.Combat;
using CCS.Modules.Equipment;
using CCS.Modules.Fishing;
using CCS.Modules.Gathering;
using CCS.Modules.Interaction;
using CCS.Modules.Inventory;
using CCS.Modules.Wildlife;
using CCS.Modules.WorldResources;
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
        private CCS_GatheringService gatheringService;
        private CCS_FishingService fishingService;
        private CCS_InteractionService interactionService;
        private CCS_PlayerInventoryService inventoryService;
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

        public void BindGatheringService(CCS_GatheringService service)
        {
            gatheringService = service;
        }

        public void BindFishingService(CCS_FishingService service)
        {
            fishingService = service;
        }

        public void BindInteractionService(CCS_InteractionService service)
        {
            interactionService = service;
        }

        public void BindInventoryService(CCS_PlayerInventoryService service)
        {
            inventoryService = service;
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
                case CCS_ActiveItemBehaviorType.Bow:
                    return TryUseBow(state, request);
                case CCS_ActiveItemBehaviorType.Tool:
                    return TryUseTool(state, request);
                case CCS_ActiveItemBehaviorType.Consumable:
                case CCS_ActiveItemBehaviorType.Placeable:
                    return CCS_ActiveItemUseResult.NoBehavior(state.ActiveItemId);
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

        private CCS_ActiveItemUseResult TryUseBow(CCS_ActiveItemState state, CCS_ActiveItemUseRequest request)
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
                    "Bow must be equipped in the main hand to attack.",
                    true,
                    state.ActiveItemId);
            }

            CCS_CombatHitResult combatResult =
                combatService.TryRangedAttack(request.UseOrigin, request.UseDirection);

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
                    ? "No wildlife target in bow range."
                    : combatResult.Message,
                true,
                state.ActiveItemId);
        }

        private CCS_ActiveItemUseResult TryUseTool(CCS_ActiveItemState state, CCS_ActiveItemUseRequest request)
        {
            if (!IsActiveToolEquipped(state))
            {
                return new CCS_ActiveItemUseResult(
                    CCS_ActiveItemUseResultType.ToolNotEquipped,
                    "Active tool must be equipped in main hand or tool slot to use.",
                    true,
                    state.ActiveItemId);
            }

            if (CCS_ItemGameplayUtility.IsFishingPoleItem(state.ItemDefinition))
            {
                return TryUseFishingPole(state, request);
            }

            if (!CCS_ActiveItemTargetResolver.TryResolveFromInteraction(
                    interactionService,
                    request.UseOrigin,
                    out CCS_ActiveItemTargetContext targetContext))
            {
                return new CCS_ActiveItemUseResult(
                    CCS_ActiveItemUseResultType.NoTarget,
                    "No harvest target in range. Look at a tree, rock, or resource node.",
                    true,
                    state.ActiveItemId);
            }

            if (targetContext.IsOutOfRange)
            {
                return new CCS_ActiveItemUseResult(
                    CCS_ActiveItemUseResultType.TargetOutOfRange,
                    "Target is out of range.",
                    true,
                    state.ActiveItemId,
                    targetContext.DisplayName,
                    targetContext.TargetTypeLabel);
            }

            switch (targetContext.TargetKind)
            {
                case CCS_ActiveItemTargetKind.GatheringNode:
                    return TryUseToolOnGatheringNode(state, targetContext);
                case CCS_ActiveItemTargetKind.HarvestableResource:
                    return TryUseToolOnHarvestableResource(state, targetContext);
                case CCS_ActiveItemTargetKind.HarvestableWildlife:
                    return TryUseToolOnHarvestableWildlife(state, targetContext);
                case CCS_ActiveItemTargetKind.FishingSpot:
                    return TryUseFishingPoleOnSpot(state, targetContext);
                default:
                    return new CCS_ActiveItemUseResult(
                        CCS_ActiveItemUseResultType.NoBehaviorRegistered,
                        "Active tool has no behavior for the current target.",
                        true,
                        state.ActiveItemId,
                        targetContext.DisplayName,
                        targetContext.TargetTypeLabel);
            }
        }

        private CCS_ActiveItemUseResult TryUseFishingPole(
            CCS_ActiveItemState state,
            CCS_ActiveItemUseRequest request)
        {
            if (!CCS_ActiveItemTargetResolver.TryResolveFromInteraction(
                    interactionService,
                    request.UseOrigin,
                    out CCS_ActiveItemTargetContext targetContext))
            {
                return new CCS_ActiveItemUseResult(
                    CCS_ActiveItemUseResultType.NoTarget,
                    "No fishing spot in range. Look at a river edge, pond, lake, or stream.",
                    true,
                    state.ActiveItemId);
            }

            if (targetContext.IsOutOfRange)
            {
                return new CCS_ActiveItemUseResult(
                    CCS_ActiveItemUseResultType.TargetOutOfRange,
                    "Fishing spot is out of range.",
                    true,
                    state.ActiveItemId,
                    targetContext.DisplayName,
                    targetContext.TargetTypeLabel);
            }

            if (targetContext.TargetKind != CCS_ActiveItemTargetKind.FishingSpot)
            {
                return new CCS_ActiveItemUseResult(
                    CCS_ActiveItemUseResultType.WrongTool,
                    "Fishing pole can only be used on fishable water sources.",
                    true,
                    state.ActiveItemId,
                    targetContext.DisplayName,
                    targetContext.TargetTypeLabel);
            }

            return TryUseFishingPoleOnSpot(state, targetContext);
        }

        private CCS_ActiveItemUseResult TryUseFishingPoleOnSpot(
            CCS_ActiveItemState state,
            CCS_ActiveItemTargetContext targetContext)
        {
            if (activeProfile == null || !activeProfile.EnableFishingRouting)
            {
                return CCS_ActiveItemUseResult.NoBehavior(state.ActiveItemId);
            }

            if (fishingService == null || !fishingService.IsInitialized)
            {
                return new CCS_ActiveItemUseResult(
                    CCS_ActiveItemUseResultType.NoBehaviorRegistered,
                    "Fishing service is unavailable.",
                    false,
                    state.ActiveItemId,
                    targetContext.DisplayName,
                    targetContext.TargetTypeLabel);
            }

            CCS_FishingSpot fishingSpot = targetContext.FishingSpot;
            if (fishingSpot == null || !fishingSpot.CanFish())
            {
                return new CCS_ActiveItemUseResult(
                    CCS_ActiveItemUseResultType.FishingTargetUnavailable,
                    "Fishing spot is unavailable.",
                    true,
                    state.ActiveItemId,
                    targetContext.DisplayName,
                    targetContext.TargetTypeLabel);
            }

            if (fishingSpot.SpotDefinition != null && !fishingSpot.SpotDefinition.SupportsFishing)
            {
                return new CCS_ActiveItemUseResult(
                    CCS_ActiveItemUseResultType.FishingNoWater,
                    "Target does not support fishing.",
                    true,
                    state.ActiveItemId,
                    targetContext.DisplayName,
                    targetContext.TargetTypeLabel);
            }

            CCS_FishingResult fishingResult =
                fishingService.TryFish(new CCS_FishingRequest(fishingSpot, state.ItemDefinition));

            return MapFishingResult(state, targetContext, fishingResult);
        }

        private static CCS_ActiveItemUseResult MapFishingResult(
            CCS_ActiveItemState state,
            CCS_ActiveItemTargetContext targetContext,
            CCS_FishingResult fishingResult)
        {
            if (fishingResult == null)
            {
                return new CCS_ActiveItemUseResult(
                    CCS_ActiveItemUseResultType.FishingFailed,
                    "Fishing result was unavailable.",
                    false,
                    state.ActiveItemId,
                    targetContext.DisplayName,
                    targetContext.TargetTypeLabel);
            }

            switch (fishingResult.ResultType)
            {
                case CCS_FishingResultType.FishCaught:
                case CCS_FishingResultType.SmallFishCaught:
                case CCS_FishingResultType.JunkCaught:
                case CCS_FishingResultType.NothingCaught:
                    return new CCS_ActiveItemUseResult(
                        CCS_ActiveItemUseResultType.FishingSuccess,
                        fishingResult.Message,
                        true,
                        state.ActiveItemId,
                        targetContext.DisplayName,
                        targetContext.TargetTypeLabel);
                case CCS_FishingResultType.NoBait:
                    return new CCS_ActiveItemUseResult(
                        CCS_ActiveItemUseResultType.FishingNoBait,
                        fishingResult.Message,
                        true,
                        state.ActiveItemId,
                        targetContext.DisplayName,
                        targetContext.TargetTypeLabel);
                case CCS_FishingResultType.NoWater:
                    return new CCS_ActiveItemUseResult(
                        CCS_ActiveItemUseResultType.FishingNoWater,
                        fishingResult.Message,
                        true,
                        state.ActiveItemId,
                        targetContext.DisplayName,
                        targetContext.TargetTypeLabel);
                case CCS_FishingResultType.TargetUnavailable:
                    return new CCS_ActiveItemUseResult(
                        CCS_ActiveItemUseResultType.FishingTargetUnavailable,
                        fishingResult.Message,
                        true,
                        state.ActiveItemId,
                        targetContext.DisplayName,
                        targetContext.TargetTypeLabel);
                case CCS_FishingResultType.ServiceUnavailable:
                    return new CCS_ActiveItemUseResult(
                        CCS_ActiveItemUseResultType.NoBehaviorRegistered,
                        fishingResult.Message,
                        false,
                        state.ActiveItemId,
                        targetContext.DisplayName,
                        targetContext.TargetTypeLabel);
                default:
                    return new CCS_ActiveItemUseResult(
                        CCS_ActiveItemUseResultType.FishingFailed,
                        fishingResult.Message,
                        true,
                        state.ActiveItemId,
                        targetContext.DisplayName,
                        targetContext.TargetTypeLabel);
            }
        }

        private CCS_ActiveItemUseResult TryUseToolOnGatheringNode(
            CCS_ActiveItemState state,
            CCS_ActiveItemTargetContext targetContext)
        {
            if (activeProfile == null || !activeProfile.EnableGatheringRouting)
            {
                return CCS_ActiveItemUseResult.NoBehavior(state.ActiveItemId);
            }

            if (gatheringService == null || !gatheringService.IsInitialized)
            {
                return new CCS_ActiveItemUseResult(
                    CCS_ActiveItemUseResultType.ServiceUnavailable,
                    "Gathering service is unavailable.",
                    false,
                    state.ActiveItemId,
                    targetContext.DisplayName,
                    targetContext.TargetTypeLabel);
            }

            CCS_GatheringNode gatheringNode = targetContext.GatheringNode;
            if (gatheringNode == null)
            {
                return new CCS_ActiveItemUseResult(
                    CCS_ActiveItemUseResultType.TargetUnavailable,
                    "Gathering node is unavailable.",
                    true,
                    state.ActiveItemId,
                    targetContext.DisplayName,
                    targetContext.TargetTypeLabel);
            }

            if (!gatheringNode.CanGather())
            {
                return new CCS_ActiveItemUseResult(
                    CCS_ActiveItemUseResultType.TargetUnavailable,
                    "Gathering node is depleted or unavailable.",
                    true,
                    state.ActiveItemId,
                    targetContext.DisplayName,
                    targetContext.TargetTypeLabel);
            }

            CCS_GatheringProfile gatheringProfile =
                gatheringService != null ? gatheringService.ActiveProfile : null;

            if (gatheringProfile != null
                && gatheringProfile.TryGetNodeRewardSettings(
                    gatheringNode.NodeType,
                    out CCS_GatheringNodeRewardSettings nodeSettings)
                && !CCS_ActiveItemGatheringToolUtility.IsHarvestMethodImplementedForActiveUse(
                    nodeSettings.harvestMethod))
            {
                return new CCS_ActiveItemUseResult(
                    CCS_ActiveItemUseResultType.NoBehaviorRegistered,
                    $"Harvest method {nodeSettings.harvestMethod} is not implemented for active use.",
                    true,
                    state.ActiveItemId,
                    targetContext.DisplayName,
                    targetContext.TargetTypeLabel);
            }

            if (!CCS_ActiveItemGatheringToolUtility.ActiveToolMatchesGatheringNode(
                    state.ItemDefinition,
                    gatheringNode.NodeType,
                    gatheringProfile))
            {
                return new CCS_ActiveItemUseResult(
                    CCS_ActiveItemUseResultType.WrongTool,
                    $"Wrong tool for {gatheringNode.NodeType}.",
                    true,
                    state.ActiveItemId,
                    targetContext.DisplayName,
                    targetContext.TargetTypeLabel);
            }

            CCS_GatheringResult gatheringResult = gatheringService.TryGatherNode(gatheringNode);
            if (gatheringResult != null && gatheringResult.DidGather)
            {
                return new CCS_ActiveItemUseResult(
                    CCS_ActiveItemUseResultType.GatheringSuccess,
                    gatheringResult.Message,
                    true,
                    state.ActiveItemId,
                    targetContext.DisplayName,
                    targetContext.TargetTypeLabel);
            }

            string failureMessage = gatheringResult != null && !string.IsNullOrWhiteSpace(gatheringResult.Message)
                ? gatheringResult.Message
                : "Gathering failed.";

            return new CCS_ActiveItemUseResult(
                CCS_ActiveItemUseResultType.GatheringFailed,
                failureMessage,
                true,
                state.ActiveItemId,
                targetContext.DisplayName,
                targetContext.TargetTypeLabel);
        }

        private CCS_ActiveItemUseResult TryUseToolOnHarvestableResource(
            CCS_ActiveItemState state,
            CCS_ActiveItemTargetContext targetContext)
        {
            if (activeProfile == null || !activeProfile.EnableResourceHarvestRouting)
            {
                return CCS_ActiveItemUseResult.NoBehavior(state.ActiveItemId);
            }

            CCS_HarvestableResource harvestableResource = targetContext.HarvestableResource;
            if (harvestableResource == null)
            {
                return new CCS_ActiveItemUseResult(
                    CCS_ActiveItemUseResultType.TargetUnavailable,
                    "Harvestable resource is unavailable.",
                    true,
                    state.ActiveItemId,
                    targetContext.DisplayName,
                    targetContext.TargetTypeLabel);
            }

            CCS_ResourceDefinition resourceDefinition = harvestableResource.ResourceDefinition;
            if (resourceDefinition != null
                && !CCS_ActiveItemGatheringToolUtility.IsHarvestMethodImplementedForActiveUse(
                    resourceDefinition.HarvestMethod))
            {
                return new CCS_ActiveItemUseResult(
                    CCS_ActiveItemUseResultType.NoBehaviorRegistered,
                    $"Harvest method {resourceDefinition.HarvestMethod} is not implemented for active use.",
                    true,
                    state.ActiveItemId,
                    targetContext.DisplayName,
                    targetContext.TargetTypeLabel);
            }

            if (!CCS_ActiveItemGatheringToolUtility.ActiveToolMatchesHarvestableResource(
                    state.ItemDefinition,
                    resourceDefinition))
            {
                return new CCS_ActiveItemUseResult(
                    CCS_ActiveItemUseResultType.WrongTool,
                    "Wrong tool for this resource.",
                    true,
                    state.ActiveItemId,
                    targetContext.DisplayName,
                    targetContext.TargetTypeLabel);
            }

            if (inventoryService == null || !inventoryService.IsInitialized)
            {
                return new CCS_ActiveItemUseResult(
                    CCS_ActiveItemUseResultType.ServiceUnavailable,
                    "Inventory service is unavailable for resource harvest.",
                    false,
                    state.ActiveItemId,
                    targetContext.DisplayName,
                    targetContext.TargetTypeLabel);
            }

            CCS_RequiredToolType equippedToolType =
                CCS_ActiveItemGatheringToolUtility.ResolveEquippedToolType(state.ItemDefinition);

            CCS_HarvestResult harvestResult = harvestableResource.Harvest(equippedToolType, inventoryService);
            if (harvestResult != null && harvestResult.IsSuccess)
            {
                return new CCS_ActiveItemUseResult(
                    CCS_ActiveItemUseResultType.ResourceHarvestSuccess,
                    harvestResult.Message,
                    true,
                    state.ActiveItemId,
                    targetContext.DisplayName,
                    targetContext.TargetTypeLabel);
            }

            string failureMessage = harvestResult != null && !string.IsNullOrWhiteSpace(harvestResult.Message)
                ? harvestResult.Message
                : "Resource harvest failed.";

            return new CCS_ActiveItemUseResult(
                CCS_ActiveItemUseResultType.ResourceHarvestFailed,
                failureMessage,
                true,
                state.ActiveItemId,
                targetContext.DisplayName,
                targetContext.TargetTypeLabel);
        }

        private CCS_ActiveItemUseResult TryUseToolOnHarvestableWildlife(
            CCS_ActiveItemState state,
            CCS_ActiveItemTargetContext targetContext)
        {
            if (activeProfile == null || !activeProfile.EnableWildlifeHarvestRouting)
            {
                return CCS_ActiveItemUseResult.NoBehavior(state.ActiveItemId);
            }

            CCS_HarvestableWildlife harvestableWildlife = targetContext.HarvestableWildlife;
            if (harvestableWildlife == null)
            {
                return new CCS_ActiveItemUseResult(
                    CCS_ActiveItemUseResultType.TargetUnavailable,
                    "Wildlife carcass is unavailable.",
                    true,
                    state.ActiveItemId,
                    targetContext.DisplayName,
                    targetContext.TargetTypeLabel);
            }

            if (!CCS_ItemGameplayUtility.ItemSatisfiesHarvestTool(
                    state.ItemDefinition,
                    CCS_ItemToolType.Knife))
            {
                return new CCS_ActiveItemUseResult(
                    CCS_ActiveItemUseResultType.WrongTool,
                    "Knife required to harvest wildlife.",
                    true,
                    state.ActiveItemId,
                    targetContext.DisplayName,
                    targetContext.TargetTypeLabel);
            }

            if (inventoryService == null || !inventoryService.IsInitialized)
            {
                return new CCS_ActiveItemUseResult(
                    CCS_ActiveItemUseResultType.ServiceUnavailable,
                    "Inventory service is unavailable for wildlife harvest.",
                    false,
                    state.ActiveItemId,
                    targetContext.DisplayName,
                    targetContext.TargetTypeLabel);
            }

            CCS_WildlifeHarvestResult harvestResult =
                harvestableWildlife.Harvest(CCS_RequiredToolType.Knife, inventoryService);

            if (harvestResult != null && harvestResult.IsSuccess)
            {
                return new CCS_ActiveItemUseResult(
                    CCS_ActiveItemUseResultType.HarvestSuccess,
                    harvestResult.Message,
                    true,
                    state.ActiveItemId,
                    targetContext.DisplayName,
                    targetContext.TargetTypeLabel);
            }

            CCS_WildlifeHarvestResultType wildlifeResultType = harvestResult?.ResultType
                ?? CCS_WildlifeHarvestResultType.HarvestFailed;
            CCS_ActiveItemUseResultType mappedType = MapWildlifeHarvestResult(wildlifeResultType);
            string failureMessage = harvestResult != null && !string.IsNullOrWhiteSpace(harvestResult.Message)
                ? harvestResult.Message
                : "Wildlife harvest failed.";

            return new CCS_ActiveItemUseResult(
                mappedType,
                failureMessage,
                true,
                state.ActiveItemId,
                targetContext.DisplayName,
                targetContext.TargetTypeLabel);
        }

        private static CCS_ActiveItemUseResultType MapWildlifeHarvestResult(CCS_WildlifeHarvestResultType resultType)
        {
            switch (resultType)
            {
                case CCS_WildlifeHarvestResultType.WildlifeNotDead:
                    return CCS_ActiveItemUseResultType.WildlifeNotDead;
                case CCS_WildlifeHarvestResultType.WildlifeAlreadyHarvested:
                    return CCS_ActiveItemUseResultType.WildlifeAlreadyHarvested;
                case CCS_WildlifeHarvestResultType.WrongTool:
                    return CCS_ActiveItemUseResultType.WrongTool;
                default:
                    return CCS_ActiveItemUseResultType.HarvestFailed;
            }
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

        private bool IsActiveToolEquipped(CCS_ActiveItemState state)
        {
            if (equipmentService == null || !equipmentService.IsInitialized)
            {
                return state.SourceSlotType == CCS_ActiveItemSlotType.TestHarness;
            }

            if (IsItemEquippedInSlot(state.ActiveItemId, CCS_EquipmentSlotType.MainHand)
                || IsItemEquippedInSlot(state.ActiveItemId, CCS_EquipmentSlotType.Tool)
                || IsItemEquippedInSlot(state.ActiveItemId, CCS_EquipmentSlotType.OffHand))
            {
                return true;
            }

            return false;
        }

        private bool IsItemEquippedInSlot(string itemId, CCS_EquipmentSlotType slotType)
        {
            CCS_EquippedItem equippedItem = equipmentService.GetEquippedItem(slotType);
            return equippedItem?.ItemDefinition != null && equippedItem.ItemDefinition.ItemId == itemId;
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
