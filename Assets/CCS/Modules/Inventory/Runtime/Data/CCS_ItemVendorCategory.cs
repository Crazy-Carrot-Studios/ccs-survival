// =============================================================================
// SCRIPT: CCS_ItemVendorCategory
// CATEGORY: Modules / Inventory / Runtime / Data
// PURPOSE: Optional vendor grouping for economy buy/sell rules and future UI.
// PLACEMENT: Referenced by CCS_ItemDefinition vendor category field.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: None value means item has no vendor category assigned.
// =============================================================================

namespace CCS.Modules.Inventory
{
    public enum CCS_ItemVendorCategory
    {
        None = 0,
        Food = 1,
        Tools = 2,
        Materials = 3,
        Weapons = 4,
        Ammunition = 5,
        Livestock = 6,
        Miscellaneous = 7
    }
}
