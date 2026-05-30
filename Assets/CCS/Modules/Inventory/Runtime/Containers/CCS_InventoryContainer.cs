using System.Collections.Generic;

// =============================================================================
// SCRIPT: CCS_InventoryContainer
// CATEGORY: Modules / Inventory / Runtime / Containers
// PURPOSE: Variable-slot inventory storage with stack merging and partial removal.
// PLACEMENT: Owned by CCS_PlayerInventoryService or future storage/chest systems.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Merge into existing stacks first, then empty slots. No UI dependencies.
// =============================================================================

namespace CCS.Modules.Inventory
{
    public sealed class CCS_InventoryContainer : CCS_IInventoryContainer
    {
        #region Variables

        private readonly CCS_InventorySlot[] slots;

        #endregion

        #region Public Methods

        public CCS_InventoryContainer(int slotCount)
        {
            int resolvedCount = slotCount > 0 ? slotCount : 0;
            slots = new CCS_InventorySlot[resolvedCount];

            for (int index = 0; index < resolvedCount; index++)
            {
                slots[index] = new CCS_InventorySlot();
            }
        }

        public int AddItem(CCS_ItemDefinition itemDefinition, int quantity)
        {
            if (itemDefinition == null || quantity <= 0 || slots.Length == 0)
            {
                return 0;
            }

            int remaining = quantity;
            remaining -= AddToMatchingStacks(itemDefinition, remaining);
            remaining -= AddToEmptySlots(itemDefinition, remaining);
            return quantity - remaining;
        }

        public int RemoveItem(CCS_ItemDefinition itemDefinition, int quantity)
        {
            if (itemDefinition == null || quantity <= 0)
            {
                return 0;
            }

            int remaining = quantity;

            for (int index = 0; index < slots.Length && remaining > 0; index++)
            {
                CCS_InventorySlot slot = slots[index];
                if (slot.IsEmpty || slot.Stack.ItemDefinition != itemDefinition)
                {
                    continue;
                }

                remaining -= slot.TryRemove(remaining);
            }

            return quantity - remaining;
        }

        public bool CanAdd(CCS_ItemDefinition itemDefinition, int quantity)
        {
            if (itemDefinition == null || quantity <= 0 || slots.Length == 0)
            {
                return false;
            }

            int remaining = quantity;

            for (int index = 0; index < slots.Length && remaining > 0; index++)
            {
                remaining -= slots[index].GetRemainingCapacity(itemDefinition);
            }

            return remaining <= 0;
        }

        public bool HasItem(CCS_ItemDefinition itemDefinition, int quantity)
        {
            if (itemDefinition == null || quantity <= 0)
            {
                return quantity <= 0;
            }

            return GetQuantity(itemDefinition) >= quantity;
        }

        public int GetQuantity(CCS_ItemDefinition itemDefinition)
        {
            if (itemDefinition == null)
            {
                return 0;
            }

            int total = 0;

            for (int index = 0; index < slots.Length; index++)
            {
                CCS_InventorySlot slot = slots[index];
                if (!slot.IsEmpty && slot.Stack.ItemDefinition == itemDefinition)
                {
                    total += slot.Stack.Quantity;
                }
            }

            return total;
        }

        public void Clear()
        {
            for (int index = 0; index < slots.Length; index++)
            {
                slots[index].Clear();
            }
        }

        public CCS_InventorySnapshot CreateSnapshot()
        {
            List<CCS_ItemStack> slotStacks = new List<CCS_ItemStack>(slots.Length);
            int usedSlotCount = 0;
            int totalItemQuantity = 0;

            for (int index = 0; index < slots.Length; index++)
            {
                CCS_ItemStack stack = slots[index].Stack;
                slotStacks.Add(stack);

                if (!stack.IsEmpty)
                {
                    usedSlotCount++;
                    totalItemQuantity += stack.Quantity;
                }
            }

            return new CCS_InventorySnapshot(slotStacks, slots.Length, usedSlotCount, totalItemQuantity);
        }

        public CCS_InventorySlot GetSlot(int index)
        {
            if (index < 0 || index >= slots.Length)
            {
                return null;
            }

            return slots[index];
        }

        #endregion

        #region Private Methods

        private int AddToMatchingStacks(CCS_ItemDefinition itemDefinition, int quantity)
        {
            int added = 0;

            for (int index = 0; index < slots.Length && quantity > 0; index++)
            {
                CCS_InventorySlot slot = slots[index];
                if (slot.IsEmpty || slot.Stack.ItemDefinition != itemDefinition)
                {
                    continue;
                }

                int amountAdded = slot.TryAdd(itemDefinition, quantity);
                added += amountAdded;
                quantity -= amountAdded;
            }

            return added;
        }

        private int AddToEmptySlots(CCS_ItemDefinition itemDefinition, int quantity)
        {
            int added = 0;

            for (int index = 0; index < slots.Length && quantity > 0; index++)
            {
                CCS_InventorySlot slot = slots[index];
                if (!slot.IsEmpty)
                {
                    continue;
                }

                int amountAdded = slot.TryAdd(itemDefinition, quantity);
                added += amountAdded;
                quantity -= amountAdded;
            }

            return added;
        }

        #endregion

        #region Properties

        public int SlotCount => slots.Length;

        #endregion
    }
}
