using CCS.Modules.Inventory;

// =============================================================================
// SCRIPT: CCS_ActiveItemBehaviorUtility
// CATEGORY: Modules / Hotbar / Runtime / ActiveItem
// PURPOSE: Maps inventory item metadata to active item behavior classification.
// PLACEMENT: Used by CCS_ActiveItemService when selecting or using items.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Does not hard-code item IDs. Uses existing inventory gameplay metadata.
// =============================================================================

namespace CCS.Modules.Hotbar
{
    public static class CCS_ActiveItemBehaviorUtility
    {
        public static CCS_ActiveItemBehaviorType ResolveBehaviorType(CCS_ItemDefinition itemDefinition)
        {
            if (itemDefinition == null)
            {
                return CCS_ActiveItemBehaviorType.None;
            }

            if (CCS_ItemGameplayUtility.IsWeaponItem(itemDefinition))
            {
                return CCS_ActiveItemBehaviorType.Weapon;
            }

            if (CCS_ItemGameplayUtility.IsToolItem(itemDefinition))
            {
                return CCS_ActiveItemBehaviorType.Tool;
            }

            if (itemDefinition.Category == CCS_ItemCategory.Consumable)
            {
                return CCS_ActiveItemBehaviorType.Consumable;
            }

            return CCS_ActiveItemBehaviorType.Generic;
        }

        public static bool CanUseBehavior(CCS_ActiveItemBehaviorType behaviorType)
        {
            return behaviorType == CCS_ActiveItemBehaviorType.Weapon
                || behaviorType == CCS_ActiveItemBehaviorType.Tool;
        }
    }
}
