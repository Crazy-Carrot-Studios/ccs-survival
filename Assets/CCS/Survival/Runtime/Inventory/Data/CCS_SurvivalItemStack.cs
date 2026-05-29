// =============================================================================
// SCRIPT: CCS_SurvivalItemStack
// CATEGORY: Survival / Runtime / Inventory / Data
// PURPOSE: Runtime stack of a single item definition and quantity.
// PLACEMENT: Used by CCS_SurvivalInventorySlot and CCS_SurvivalInventoryContainer.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Mutable runtime data. Not a ScriptableObject.
// =============================================================================

namespace CCS.Survival.Inventory
{
    public sealed class CCS_SurvivalItemStack
    {
        #region Variables

        private CCS_SurvivalItemDefinition itemDefinition;
        private int amount;

        #endregion

        #region Properties

        public CCS_SurvivalItemDefinition ItemDefinition => itemDefinition;

        public int Amount => amount;

        public bool IsValid =>
            itemDefinition != null && amount > 0;

        public bool IsEmpty => !IsValid;

        #endregion

        #region Public Methods

        public void Clear()
        {
            itemDefinition = null;
            amount = 0;
        }

        public void Set(CCS_SurvivalItemDefinition definition, int stackAmount)
        {
            itemDefinition = definition;
            amount = ClampAmount(definition, stackAmount);
        }

        public bool CanStackWith(CCS_SurvivalItemDefinition definition)
        {
            if (itemDefinition == null || definition == null)
            {
                return false;
            }

            if (!ReferenceEquals(itemDefinition, definition)
                && itemDefinition.ItemId != definition.ItemId)
            {
                return false;
            }

            return itemDefinition.IsStackable && definition.IsStackable;
        }

        public int GetRemainingStackCapacity()
        {
            if (itemDefinition == null)
            {
                return 0;
            }

            return itemDefinition.GetEffectiveMaxStackSize() - amount;
        }

        public int TryAddAmount(int addAmount)
        {
            if (addAmount <= 0 || itemDefinition == null)
            {
                return addAmount;
            }

            int capacity = GetRemainingStackCapacity();
            if (capacity <= 0)
            {
                return addAmount;
            }

            int accepted = addAmount < capacity ? addAmount : capacity;
            amount += accepted;
            return addAmount - accepted;
        }

        public int TryRemoveAmount(int removeAmount)
        {
            if (removeAmount <= 0 || amount <= 0)
            {
                return removeAmount;
            }

            int removed = removeAmount < amount ? removeAmount : amount;
            amount -= removed;

            if (amount <= 0)
            {
                Clear();
            }

            return removeAmount - removed;
        }

        #endregion

        #region Private Methods

        private static int ClampAmount(CCS_SurvivalItemDefinition definition, int stackAmount)
        {
            if (definition == null)
            {
                return 0;
            }

            if (stackAmount <= 0)
            {
                return 0;
            }

            int maxStack = definition.GetEffectiveMaxStackSize();
            return stackAmount > maxStack ? maxStack : stackAmount;
        }

        #endregion
    }
}
