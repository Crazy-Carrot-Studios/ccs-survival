// =============================================================================
// SCRIPT: CCS_InventoryEventArgs
// CATEGORY: Modules / Inventory / Runtime / Events
// PURPOSE: Event payload for inventory add, remove, change, and full notifications.
// PLACEMENT: Passed to CCS_PlayerInventoryService event subscribers.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: No UI, interaction, equipment, or save data in 0.4.0.
// =============================================================================

namespace CCS.Modules.Inventory
{
    public sealed class CCS_InventoryEventArgs
    {
        #region Public Methods

        public CCS_InventoryEventArgs(
            CCS_ItemDefinition itemDefinition,
            int quantity,
            int slotIndex = -1,
            int remainingQuantity = 0,
            string message = "")
        {
            ItemDefinition = itemDefinition;
            Quantity = quantity;
            SlotIndex = slotIndex;
            RemainingQuantity = remainingQuantity;
            Message = message ?? string.Empty;
        }

        #endregion

        #region Properties

        public CCS_ItemDefinition ItemDefinition { get; }

        public int Quantity { get; }

        public int SlotIndex { get; }

        public int RemainingQuantity { get; }

        public string Message { get; }

        #endregion
    }
}
