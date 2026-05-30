// =============================================================================
// SCRIPT: CCS_IInventoryContainer
// CATEGORY: Modules / Inventory / Runtime / Containers
// PURPOSE: Contract for slot-based inventory storage with stack merge and split support.
// PLACEMENT: Implemented by CCS_InventoryContainer and future storage containers.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: No UI, interaction, equipment, or save system references.
// =============================================================================

namespace CCS.Modules.Inventory
{
    public interface CCS_IInventoryContainer
    {
        #region Properties

        int SlotCount { get; }

        #endregion

        #region Public Methods

        int AddItem(CCS_ItemDefinition itemDefinition, int quantity);

        int RemoveItem(CCS_ItemDefinition itemDefinition, int quantity);

        bool CanAdd(CCS_ItemDefinition itemDefinition, int quantity);

        bool HasItem(CCS_ItemDefinition itemDefinition, int quantity);

        int GetQuantity(CCS_ItemDefinition itemDefinition);

        void Clear();

        CCS_InventorySnapshot CreateSnapshot();

        #endregion
    }
}
