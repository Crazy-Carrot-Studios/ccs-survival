// =============================================================================
// SCRIPT: CCS_HarvestMethodType
// CATEGORY: Modules / Resources / Runtime / Definitions
// PURPOSE: Generic classification for how a resource source is harvested.
// PLACEMENT: Serialized on gathering and world resource definitions.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Fish is defined for future systems and is not implemented in 1.2.4.
// =============================================================================

namespace CCS.Modules.Resources
{
    public enum CCS_HarvestMethodType
    {
        None = 0,
        Gather = 1,
        Chop = 2,
        Mine = 3,
        Skin = 4,
        Butcher = 5,
        Salvage = 6,
        Collect = 7,
        Dig = 8,
        Fish = 9,
        Other = 10
    }
}
