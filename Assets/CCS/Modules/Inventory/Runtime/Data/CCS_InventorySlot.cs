// =============================================================================
// SCRIPT: CCS_InventorySlot
// CATEGORY: Modules / Inventory / Runtime / Data
// PURPOSE: Single inventory slot with stack validation and capacity helpers.
// PLACEMENT: Used by CCS_InventoryContainer slot arrays.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Supports stack merging within slot max capacity rules.
// =============================================================================

namespace CCS.Modules.Inventory
{
    public sealed class CCS_InventorySlot
    {
        #region Variables

        private CCS_ItemStack stack = CCS_ItemStack.Empty;

        #endregion

        #region Public Methods

        public bool IsEmpty => stack.IsEmpty;

        public CCS_ItemStack Stack => stack;

        public bool CanAccept(CCS_ItemDefinition itemDefinition, int quantity)
        {
            if (itemDefinition == null || quantity <= 0)
            {
                return false;
            }

            if (IsEmpty)
            {
                return quantity <= GetMaxStackSize(itemDefinition);
            }

            if (stack.ItemDefinition != itemDefinition)
            {
                return false;
            }

            return stack.Quantity + quantity <= GetMaxStackSize(itemDefinition);
        }

        public int GetRemainingCapacity(CCS_ItemDefinition itemDefinition)
        {
            if (itemDefinition == null)
            {
                return 0;
            }

            if (IsEmpty)
            {
                return GetMaxStackSize(itemDefinition);
            }

            if (stack.ItemDefinition != itemDefinition)
            {
                return 0;
            }

            return GetMaxStackSize(itemDefinition) - stack.Quantity;
        }

        public int TryAdd(CCS_ItemDefinition itemDefinition, int quantity)
        {
            if (itemDefinition == null || quantity <= 0)
            {
                return 0;
            }

            int remainingCapacity = GetRemainingCapacity(itemDefinition);
            if (remainingCapacity <= 0)
            {
                return 0;
            }

            int amountToAdd = quantity < remainingCapacity ? quantity : remainingCapacity;

            if (IsEmpty)
            {
                stack = new CCS_ItemStack(itemDefinition, amountToAdd);
            }
            else
            {
                stack = stack.WithQuantity(stack.Quantity + amountToAdd);
            }

            return amountToAdd;
        }

        public int TryRemove(int quantity)
        {
            if (IsEmpty || quantity <= 0)
            {
                return 0;
            }

            int amountToRemove = quantity < stack.Quantity ? quantity : stack.Quantity;
            int newQuantity = stack.Quantity - amountToRemove;

            stack = newQuantity <= 0
                ? CCS_ItemStack.Empty
                : stack.WithQuantity(newQuantity);

            return amountToRemove;
        }

        public void Clear()
        {
            stack = CCS_ItemStack.Empty;
        }

        public void SetStack(CCS_ItemStack newStack)
        {
            stack = newStack.IsEmpty ? CCS_ItemStack.Empty : newStack;
        }

        #endregion

        #region Private Methods

        private static int GetMaxStackSize(CCS_ItemDefinition itemDefinition)
        {
            if (itemDefinition == null)
            {
                return 0;
            }

            if (!itemDefinition.IsStackable)
            {
                return 1;
            }

            return itemDefinition.MaxStackSize > 0 ? itemDefinition.MaxStackSize : 1;
        }

        #endregion
    }
}
