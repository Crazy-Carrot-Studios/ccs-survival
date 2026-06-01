// =============================================================================
// SCRIPT: CCS_CraftingStationType
// CATEGORY: Modules / Crafting / Runtime / Definitions
// PURPOSE: Station categories required by recipes and runtime station context.
// PLACEMENT: Referenced by recipe definitions and CCS_CraftingStationContext.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: FirePit is the campfire station type (Campfire crafting). Hand = inventory crafting.
// =============================================================================

namespace CCS.Modules.Crafting
{
    public enum CCS_CraftingStationType
    {
        Hand = 0,
        FirePit = 1,
        Workbench = 2,
        Forge = 3,
        Apothecary = 4
    }
}
