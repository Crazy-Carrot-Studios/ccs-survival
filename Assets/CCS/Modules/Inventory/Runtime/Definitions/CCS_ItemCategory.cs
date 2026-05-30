// =============================================================================
// SCRIPT: CCS_ItemCategory
// CATEGORY: Modules / Inventory / Runtime / Definitions
// PURPOSE: High-level item classification for inventory data architecture.
// PLACEMENT: Referenced by CCS_ItemDefinition. No gameplay stats in 0.4.0.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Equipment, crafting, and resource categories are placeholders only.
// =============================================================================

namespace CCS.Modules.Inventory
{
    public enum CCS_ItemCategory
    {
        Generic = 0,
        Resource = 1,
        Consumable = 2,
        Tool = 3,
        Material = 4,
        Quest = 5
    }
}
