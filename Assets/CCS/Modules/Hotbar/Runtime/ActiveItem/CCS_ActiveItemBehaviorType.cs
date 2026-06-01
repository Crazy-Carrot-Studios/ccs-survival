// =============================================================================
// SCRIPT: CCS_ActiveItemBehaviorType
// CATEGORY: Modules / Hotbar / Runtime / ActiveItem
// PURPOSE: Classifies how an active item may be used at runtime.
// PLACEMENT: Resolved from inventory item metadata via CCS_ActiveItemBehaviorUtility.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Only weapon routing is implemented in 1.2.2. Other types fail gracefully.
// =============================================================================

namespace CCS.Modules.Hotbar
{
    public enum CCS_ActiveItemBehaviorType
    {
        None = 0,
        Weapon = 1,
        Tool = 2,
        Consumable = 3,
        Placeable = 4,
        Generic = 5
    }
}
