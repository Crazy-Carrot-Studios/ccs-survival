// =============================================================================
// SCRIPT: CCS_SurvivalInventorySlot
// CATEGORY: Survival / Runtime / Inventory / Data
// PURPOSE: Single inventory slot holding an optional item stack.
// PLACEMENT: Used by CCS_SurvivalInventoryContainer slot array.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Slots are fixed index containers; no drag/drop in Phase 2B.
// =============================================================================

namespace CCS.Survival.Inventory
{
    public sealed class CCS_SurvivalInventorySlot
    {
        #region Variables

        private readonly CCS_SurvivalItemStack stack = new CCS_SurvivalItemStack();

        #endregion

        #region Properties

        public bool IsEmpty => stack.IsEmpty;

        public CCS_SurvivalItemStack Stack => stack;

        #endregion

        #region Public Methods

        public bool CanAccept(CCS_SurvivalItemDefinition definition, int amount)
        {
            if (definition == null || amount <= 0)
            {
                return false;
            }

            if (IsEmpty)
            {
                return true;
            }

            if (!stack.CanStackWith(definition))
            {
                return false;
            }

            return stack.GetRemainingStackCapacity() > 0 || amount <= definition.GetEffectiveMaxStackSize();
        }

        public void Clear()
        {
            stack.Clear();
        }

        public void SetStack(CCS_SurvivalItemDefinition definition, int amount)
        {
            stack.Set(definition, amount);
        }

        public int AddToStack(CCS_SurvivalItemDefinition definition, int amount)
        {
            if (definition == null || amount <= 0)
            {
                return amount;
            }

            if (IsEmpty)
            {
                int maxStack = definition.GetEffectiveMaxStackSize();
                int placed = amount < maxStack ? amount : maxStack;
                stack.Set(definition, placed);
                return amount - placed;
            }

            if (!stack.CanStackWith(definition))
            {
                return amount;
            }

            return stack.TryAddAmount(amount);
        }

        public int RemoveFromStack(int amount)
        {
            if (amount <= 0 || IsEmpty)
            {
                return amount;
            }

            return stack.TryRemoveAmount(amount);
        }

        #endregion
    }
}
