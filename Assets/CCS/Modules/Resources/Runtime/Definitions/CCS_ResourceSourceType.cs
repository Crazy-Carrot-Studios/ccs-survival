// =============================================================================
// SCRIPT: CCS_ResourceSourceType
// CATEGORY: Modules / Resources / Runtime / Definitions
// PURPOSE: Generic classification for where harvestable materials originate.
// PLACEMENT: Serialized on gathering and world resource definitions.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Game-agnostic. Western content uses these categories via data, not code branches.
// =============================================================================

namespace CCS.Modules.Resources
{
    public enum CCS_ResourceSourceType
    {
        None = 0,
        Natural = 1,
        Wildlife = 2,
        Salvage = 3,
        Mining = 4,
        Water = 5,
        Agriculture = 6,
        Crafted = 7,
        Other = 8
    }
}
