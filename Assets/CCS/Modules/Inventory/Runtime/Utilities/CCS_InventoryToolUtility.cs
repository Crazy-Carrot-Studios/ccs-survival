using System.Collections.Generic;

// =============================================================================
// SCRIPT: CCS_InventoryToolUtility
// CATEGORY: Modules / Inventory / Runtime / Utilities
// PURPOSE: Resolves whether inventory contains items that satisfy harvest tool requirements.
// PLACEMENT: Used by world resource harvesting and future equipment integration.
// AUTHOR: James Schilz
// CREATED: 2026-05-31
// NOTES: No durability or equipped-slot logic in 0.9.1 foundation.
// =============================================================================

namespace CCS.Modules.Inventory
{
    public static class CCS_InventoryToolUtility
    {
        #region Public Methods

        public static bool InventoryContainsTool(CCS_PlayerInventoryService inventoryService, CCS_ItemToolType requiredTool)
        {
            if (inventoryService == null || !inventoryService.IsInitialized || requiredTool == CCS_ItemToolType.None)
            {
                return requiredTool == CCS_ItemToolType.None;
            }

            CCS_InventorySnapshot snapshot = inventoryService.CreateSnapshot();
            IReadOnlyList<CCS_ItemStack> stacks = snapshot.SlotStacks;
            for (int index = 0; index < stacks.Count; index++)
            {
                CCS_ItemStack stack = stacks[index];
                if (stack.IsEmpty || stack.ItemDefinition == null)
                {
                    continue;
                }

                CCS_ItemDefinition definition = stack.ItemDefinition;
                if (definition.HasToolIdentity && definition.ToolType == requiredTool)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}
