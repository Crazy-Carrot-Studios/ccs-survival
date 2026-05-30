// =============================================================================
// SCRIPT: CCS_ItemStack
// CATEGORY: Modules / Inventory / Runtime / Data
// PURPOSE: Quantity of a single item definition held in an inventory slot.
// PLACEMENT: Stored in CCS_InventorySlot. Immutable-friendly value type.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: No UI or equipment references in 0.4.0.
// =============================================================================

namespace CCS.Modules.Inventory
{
    public readonly struct CCS_ItemStack
    {
        #region Public Methods

        public CCS_ItemStack(CCS_ItemDefinition itemDefinition, int quantity)
        {
            ItemDefinition = itemDefinition;
            Quantity = quantity < 0 ? 0 : quantity;
        }

        public static CCS_ItemStack Empty => new CCS_ItemStack(null, 0);

        public bool IsEmpty => ItemDefinition == null || Quantity <= 0;

        public CCS_ItemStack WithQuantity(int newQuantity)
        {
            return new CCS_ItemStack(ItemDefinition, newQuantity);
        }

        #endregion

        #region Properties

        public CCS_ItemDefinition ItemDefinition { get; }

        public int Quantity { get; }

        #endregion
    }
}
