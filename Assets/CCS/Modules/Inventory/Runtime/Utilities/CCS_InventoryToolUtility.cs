using System.Collections.Generic;

// =============================================================================
// SCRIPT: CCS_InventoryToolUtility
// CATEGORY: Modules / Inventory / Runtime / Utilities
// PURPOSE: Resolves whether inventory or equipped items satisfy harvest tool requirements.
// PLACEMENT: Used by world resource harvesting and equipment integration.
// AUTHOR: James Schilz
// CREATED: 2026-05-31
// NOTES: Equipped tool checks added at 0.9.2. No durability logic yet.
// =============================================================================

namespace CCS.Modules.Inventory
{
    public static class CCS_InventoryToolUtility
    {
        #region Public Methods

        public static bool InventoryContainsTool(
            CCS_PlayerInventoryService inventoryService,
            CCS_ItemToolType requiredTool)
        {
            if (inventoryService == null || !inventoryService.IsInitialized || requiredTool == CCS_ItemToolType.None)
            {
                return requiredTool == CCS_ItemToolType.None;
            }

            CCS_InventorySnapshot snapshot = inventoryService.CreateSnapshot();
            return ContainsToolInStacks(snapshot.SlotStacks, requiredTool);
        }

        public static bool ContainsToolInStacks(
            IReadOnlyList<CCS_ItemStack> stacks,
            CCS_ItemToolType requiredTool)
        {
            if (stacks == null || requiredTool == CCS_ItemToolType.None)
            {
                return requiredTool == CCS_ItemToolType.None;
            }

            for (int index = 0; index < stacks.Count; index++)
            {
                CCS_ItemStack stack = stacks[index];
                if (stack.IsEmpty || stack.ItemDefinition == null)
                {
                    continue;
                }

                if (CCS_ItemGameplayUtility.ItemSatisfiesHarvestTool(stack.ItemDefinition, requiredTool))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool EquippedItemSatisfiesTool(
            CCS_ItemDefinition equippedItemDefinition,
            CCS_ItemToolType requiredTool)
        {
            return CCS_ItemGameplayUtility.ItemSatisfiesHarvestTool(equippedItemDefinition, requiredTool);
        }

        #endregion
    }
}
