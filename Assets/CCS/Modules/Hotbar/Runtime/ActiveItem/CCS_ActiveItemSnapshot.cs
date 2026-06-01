// =============================================================================
// SCRIPT: CCS_ActiveItemSnapshot
// CATEGORY: Modules / Hotbar / Runtime / ActiveItem
// PURPOSE: Serializable-friendly snapshot of active item selection for save/debug.
// PLACEMENT: Produced by CCS_ActiveItemService.CreateSnapshot.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Save persistence deferred; snapshot supports future authority replication.
// =============================================================================

namespace CCS.Modules.Hotbar
{
    public readonly struct CCS_ActiveItemSnapshot
    {
        public CCS_ActiveItemSnapshot(
            string activeItemId,
            CCS_ActiveItemSlotType sourceSlotType,
            int hotbarSlotIndex,
            CCS_ActiveItemBehaviorType behaviorType,
            bool canUse,
            string activeVisualItemId,
            string lastUseMessage)
        {
            ActiveItemId = activeItemId ?? string.Empty;
            SourceSlotType = sourceSlotType;
            HotbarSlotIndex = hotbarSlotIndex;
            BehaviorType = behaviorType;
            CanUse = canUse;
            ActiveVisualItemId = activeVisualItemId ?? string.Empty;
            LastUseMessage = lastUseMessage ?? string.Empty;
        }

        public string ActiveItemId { get; }

        public CCS_ActiveItemSlotType SourceSlotType { get; }

        public int HotbarSlotIndex { get; }

        public CCS_ActiveItemBehaviorType BehaviorType { get; }

        public bool CanUse { get; }

        public string ActiveVisualItemId { get; }

        public string LastUseMessage { get; }
    }
}
