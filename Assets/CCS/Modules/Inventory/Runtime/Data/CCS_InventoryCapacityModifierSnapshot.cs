// =============================================================================
// SCRIPT: CCS_InventoryCapacityModifierSnapshot
// CATEGORY: Modules / Inventory / Runtime / Data
// PURPOSE: Lightweight capacity modifier data for future bootstrap composition.
// PLACEMENT: Populated by composition layer from equipment modifiers in a later milestone.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Inventory does not reference Equipment in 0.4.1a. Placeholder hook only.
// =============================================================================

namespace CCS.Modules.Inventory
{
    public readonly struct CCS_InventoryCapacityModifierSnapshot
    {
        #region Public Methods

        public CCS_InventoryCapacityModifierSnapshot(int additionalInventorySlots, float additionalCarryWeight)
        {
            AdditionalInventorySlots = additionalInventorySlots < 0 ? 0 : additionalInventorySlots;
            AdditionalCarryWeight = additionalCarryWeight < 0f ? 0f : additionalCarryWeight;
        }

        public static CCS_InventoryCapacityModifierSnapshot Empty =>
            new CCS_InventoryCapacityModifierSnapshot(0, 0f);

        #endregion

        #region Properties

        public int AdditionalInventorySlots { get; }

        public float AdditionalCarryWeight { get; }

        #endregion
    }
}
