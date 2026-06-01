// =============================================================================
// SCRIPT: CCS_CampfireState
// CATEGORY: Modules / Cooking / Runtime / Data
// PURPOSE: Runtime state for campfire interactable placeholders.
// PLACEMENT: Tracked by CCS_CampfireInteractable and CCS_CampfireService.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: BurnedOut is a placeholder for future fuel systems.
// =============================================================================

namespace CCS.Modules.Cooking
{
    public enum CCS_CampfireState
    {
        Unlit = 0,
        Lit = 1,
        Cooking = 2,
        BurnedOut = 3
    }
}
