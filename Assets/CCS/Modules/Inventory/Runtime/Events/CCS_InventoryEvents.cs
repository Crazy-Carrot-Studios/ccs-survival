// =============================================================================
// SCRIPT: CCS_InventoryEvents
// CATEGORY: Modules / Inventory / Runtime / Events
// PURPOSE: Event name constants and delegate contracts for inventory systems.
// PLACEMENT: Instance events on CCS_PlayerInventoryService document contracts here.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Subscribers react to data changes without UI or save coupling.
// =============================================================================

namespace CCS.Modules.Inventory
{
    public static class CCS_InventoryEvents
    {
        public const string ItemAddedEventName = "Inventory.ItemAdded";
        public const string ItemRemovedEventName = "Inventory.ItemRemoved";
        public const string InventoryChangedEventName = "Inventory.Changed";
        public const string InventoryFullEventName = "Inventory.Full";
    }

    public delegate void InventoryItemAddedHandler(CCS_InventoryEventArgs eventArgs);

    public delegate void InventoryItemRemovedHandler(CCS_InventoryEventArgs eventArgs);

    public delegate void InventoryChangedHandler(CCS_InventoryEventArgs eventArgs);

    public delegate void InventoryFullHandler(CCS_InventoryEventArgs eventArgs);
}
