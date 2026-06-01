using CCS.Modules.Equipment;
using CCS.Modules.Inventory;

// =============================================================================
// SCRIPT: CCS_ActiveItemState
// CATEGORY: Modules / Hotbar / Runtime / ActiveItem
// PURPOSE: Runtime snapshot of the currently selected active item.
// PLACEMENT: Owned by CCS_ActiveItemService.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Visual item ID mirrors equipment visuals; no duplicate visual spawn here.
// =============================================================================

namespace CCS.Modules.Hotbar
{
    public sealed class CCS_ActiveItemState
    {
        public CCS_ActiveItemState(
            string activeItemId,
            CCS_ActiveItemSlotType sourceSlotType,
            CCS_EquipmentSlotType equipmentSlot,
            int hotbarSlotIndex,
            CCS_ItemDefinition itemDefinition,
            CCS_ActiveItemBehaviorType behaviorType,
            bool canUse,
            string activeVisualItemId)
        {
            ActiveItemId = activeItemId ?? string.Empty;
            SourceSlotType = sourceSlotType;
            EquipmentSlot = equipmentSlot;
            HotbarSlotIndex = hotbarSlotIndex;
            ItemDefinition = itemDefinition;
            BehaviorType = behaviorType;
            CanUse = canUse;
            ActiveVisualItemId = activeVisualItemId ?? string.Empty;
        }

        public static CCS_ActiveItemState Empty { get; } = new CCS_ActiveItemState(
            string.Empty,
            CCS_ActiveItemSlotType.None,
            CCS_EquipmentSlotType.Head,
            -1,
            null,
            CCS_ActiveItemBehaviorType.None,
            false,
            string.Empty);

        public string ActiveItemId { get; }

        public CCS_ActiveItemSlotType SourceSlotType { get; }

        public CCS_EquipmentSlotType EquipmentSlot { get; }

        public int HotbarSlotIndex { get; }

        public CCS_ItemDefinition ItemDefinition { get; }

        public CCS_ActiveItemBehaviorType BehaviorType { get; }

        public bool CanUse { get; }

        public string ActiveVisualItemId { get; }

        public bool HasActiveItem => !string.IsNullOrWhiteSpace(ActiveItemId);
    }
}
