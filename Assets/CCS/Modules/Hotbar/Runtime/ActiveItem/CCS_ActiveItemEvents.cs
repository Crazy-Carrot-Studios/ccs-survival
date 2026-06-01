// =============================================================================
// SCRIPT: CCS_ActiveItemEvents
// CATEGORY: Modules / Hotbar / Runtime / ActiveItem
// PURPOSE: Event contracts for active item selection and use.
// PLACEMENT: Instance events on CCS_ActiveItemService.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Playtest and future UI subscribe without coupling to player drivers.
// =============================================================================

namespace CCS.Modules.Hotbar
{
    public delegate void ActiveItemChangedHandler(CCS_ActiveItemState previousState, CCS_ActiveItemState newState);

    public delegate void ActiveItemUsedHandler(CCS_ActiveItemUseResult useResult);
}
