using CCS.Modules.Inventory;

// =============================================================================
// SCRIPT: CCS_HarvestedItemDrop
// CATEGORY: Modules / WorldResources / Runtime / Data
// PURPOSE: Represents a single harvested item quantity result.
// PLACEMENT: Returned inside CCS_HarvestResult drop lists.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Inventory integration is optional and applied outside the drop payload.
// =============================================================================

namespace CCS.Modules.WorldResources
{
    public sealed class CCS_HarvestedItemDrop
    {
        #region Public Methods

        public CCS_HarvestedItemDrop(CCS_ItemDefinition itemDefinition, int quantity)
        {
            ItemDefinition = itemDefinition;
            Quantity = quantity < 0 ? 0 : quantity;
        }

        #endregion

        #region Properties

        public CCS_ItemDefinition ItemDefinition { get; }

        public int Quantity { get; }

        #endregion
    }
}
