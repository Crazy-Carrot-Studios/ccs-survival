// =============================================================================
// SCRIPT: CCS_ActiveItemSlotType
// CATEGORY: Modules / Hotbar / Runtime / ActiveItem
// PURPOSE: Identifies where the active item selection originated.
// PLACEMENT: Used by CCS_ActiveItemState and CCS_ActiveItemService.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Hotbar slot indices are placeholders until final hotbar UI exists.
// =============================================================================

namespace CCS.Modules.Hotbar
{
    public enum CCS_ActiveItemSlotType
    {
        None = 0,
        Equipped = 1,
        Hotbar = 2,
        TestHarness = 3
    }
}
