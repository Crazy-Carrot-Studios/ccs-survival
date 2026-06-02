// =============================================================================
// SCRIPT: CCS_ItemGameplayKind
// CATEGORY: Modules / Inventory / Runtime / Definitions
// PURPOSE: Distinguishes generic items from tools and weapons for progression systems.
// PLACEMENT: Referenced by CCS_ItemDefinition gameplay classification fields.
// AUTHOR: James Schilz
// CREATED: 2026-05-31
// NOTES: Material and consumable items typically remain Generic in 0.9.2 foundation.
// =============================================================================

namespace CCS.Modules.Inventory
{
    public enum CCS_ItemGameplayKind
    {
        Generic = 0,
        Tool = 1,
        Weapon = 2,
        ToolAndWeapon = 3,
        Placeable = 4
    }
}
