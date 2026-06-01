// =============================================================================
// SCRIPT: CCS_ToolArchetype
// CATEGORY: Modules / Inventory / Runtime / Definitions
// PURPOSE: Stable tool archetype identity for harvesting and crafting progression.
// PLACEMENT: Referenced by CCS_ItemDefinition tool metadata.
// AUTHOR: James Schilz
// CREATED: 2026-05-31
// NOTES: Bone variants share archetype with primitive tools; tier differentiates effectiveness.
// =============================================================================

namespace CCS.Modules.Inventory
{
    public enum CCS_ToolArchetype
    {
        None = 0,
        Knife = 1,
        Hatchet = 2,
        Pick = 3,
        Shovel = 4
    }
}
