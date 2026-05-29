using System;
using System.Collections.Generic;

// =============================================================================
// SCRIPT: CCS_SurvivalInventoryContainer
// CATEGORY: Survival / Runtime / Inventory / Containers
// PURPOSE: Fixed-size slot container with stack merge and overflow handling.
// PLACEMENT: Owned by CCS_SurvivalInventoryModule. Not a MonoBehaviour.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Merge compatible stacks first, then fill empty slots. Returns leftover amount.
// =============================================================================

namespace CCS.Survival.Inventory
{
    public sealed class CCS_SurvivalInventoryContainer
    {
        #region Variables

        private readonly CCS_SurvivalInventorySlot[] slots;

        #endregion

        #region Properties

        public int SlotCount => slots.Length;

        #endregion

        #region Public Methods

        public CCS_SurvivalInventoryContainer(int slotCount)
        {
            if (slotCount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(slotCount), "Inventory slot count must be at least 1.");
            }

            slots = new CCS_SurvivalInventorySlot[slotCount];
            for (int i = 0; i < slots.Length; i++)
            {
                slots[i] = new CCS_SurvivalInventorySlot();
            }
        }

        public int GetOccupiedSlotCount()
        {
            int occupied = 0;
            for (int i = 0; i < slots.Length; i++)
            {
                if (!slots[i].IsEmpty)
                {
                    occupied++;
                }
            }

            return occupied;
        }

        public bool TryGetSlotSnapshot(int slotIndex, out CCS_SurvivalInventorySlotSnapshot snapshot)
        {
            if (slotIndex < 0 || slotIndex >= slots.Length)
            {
                snapshot = default;
                return false;
            }

            CCS_SurvivalInventorySlot slot = slots[slotIndex];
            if (slot.IsEmpty)
            {
                snapshot = new CCS_SurvivalInventorySlotSnapshot(slotIndex, true, string.Empty, string.Empty, 0);
                return true;
            }

            snapshot = new CCS_SurvivalInventorySlotSnapshot(
                slotIndex,
                false,
                slot.Stack.ItemDefinition.ItemId,
                slot.Stack.ItemDefinition.DisplayName,
                slot.Stack.Amount);
            return true;
        }

        public int AddItem(CCS_SurvivalItemDefinition definition, int amount, out int remainingAmount)
        {
            remainingAmount = amount;

            CCS_SurvivalValidationResult validation = CCS_SurvivalItemDefinitionValidationUtility.ValidateDefinition(definition);
            if (!validation.IsSuccess || amount <= 0)
            {
                return 0;
            }

            remainingAmount = TryMergeIntoExistingStacks(definition, remainingAmount);
            remainingAmount = TryFillEmptySlots(definition, remainingAmount);

            int accepted = amount - remainingAmount;
            return accepted < 0 ? 0 : accepted;
        }

        public int RemoveItem(string itemId, int amount)
        {
            if (string.IsNullOrWhiteSpace(itemId) || amount <= 0)
            {
                return 0;
            }

            int remainingToRemove = amount;
            for (int i = 0; i < slots.Length && remainingToRemove > 0; i++)
            {
                CCS_SurvivalInventorySlot slot = slots[i];
                if (slot.IsEmpty || slot.Stack.ItemDefinition.ItemId != itemId)
                {
                    continue;
                }

                remainingToRemove = slot.RemoveFromStack(remainingToRemove);
            }

            int removed = amount - remainingToRemove;
            return removed < 0 ? 0 : removed;
        }

        public bool HasItem(string itemId, int amount)
        {
            return GetItemCount(itemId) >= amount;
        }

        public int GetItemCount(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return 0;
            }

            int total = 0;
            for (int i = 0; i < slots.Length; i++)
            {
                CCS_SurvivalInventorySlot slot = slots[i];
                if (slot.IsEmpty || slot.Stack.ItemDefinition.ItemId != itemId)
                {
                    continue;
                }

                total += slot.Stack.Amount;
            }

            return total;
        }

        public void Clear()
        {
            for (int i = 0; i < slots.Length; i++)
            {
                slots[i].Clear();
            }
        }

        public void BuildItemSummary(List<(string displayName, int amount)> summaryEntries, int maxEntries)
        {
            summaryEntries.Clear();
            if (maxEntries < 1)
            {
                return;
            }

            for (int i = 0; i < slots.Length; i++)
            {
                CCS_SurvivalInventorySlot slot = slots[i];
                if (slot.IsEmpty)
                {
                    continue;
                }

                string displayName = slot.Stack.ItemDefinition.DisplayName;
                int amount = slot.Stack.Amount;
                bool merged = false;

                for (int entryIndex = 0; entryIndex < summaryEntries.Count; entryIndex++)
                {
                    (string existingDisplayName, int existingAmount) = summaryEntries[entryIndex];
                    if (existingDisplayName != displayName)
                    {
                        continue;
                    }

                    summaryEntries[entryIndex] = (existingDisplayName, existingAmount + amount);
                    merged = true;
                    break;
                }

                if (!merged)
                {
                    summaryEntries.Add((displayName, amount));
                }
            }

            if (summaryEntries.Count > maxEntries)
            {
                summaryEntries.RemoveRange(maxEntries, summaryEntries.Count - maxEntries);
            }
        }

        #endregion

        #region Private Methods

        private int TryMergeIntoExistingStacks(CCS_SurvivalItemDefinition definition, int amountToAdd)
        {
            int remaining = amountToAdd;
            for (int i = 0; i < slots.Length && remaining > 0; i++)
            {
                remaining = slots[i].AddToStack(definition, remaining);
            }

            return remaining;
        }

        private int TryFillEmptySlots(CCS_SurvivalItemDefinition definition, int amountToAdd)
        {
            int remaining = amountToAdd;
            for (int i = 0; i < slots.Length && remaining > 0; i++)
            {
                if (!slots[i].IsEmpty)
                {
                    continue;
                }

                remaining = slots[i].AddToStack(definition, remaining);
            }

            return remaining;
        }

        #endregion
    }
}
