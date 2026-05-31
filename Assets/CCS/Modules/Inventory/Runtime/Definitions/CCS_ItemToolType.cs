// =============================================================================
// SCRIPT: CCS_ItemToolType
// CATEGORY: Modules / Inventory / Runtime / Definitions
// PURPOSE: Lightweight tool identity for harvest requirement matching.
// PLACEMENT: Referenced by CCS_ItemDefinition for primitive tool items.
// AUTHOR: James Schilz
// CREATED: 2026-05-31
// NOTES: Numeric values align with CCS_RequiredToolType for composition mapping.
// =============================================================================

namespace CCS.Modules.Inventory
{
    public enum CCS_ItemToolType
    {
        None = 0,
        Axe = 1,
        Pickaxe = 2,
        Knife = 3,
        Shovel = 4
    }
}
