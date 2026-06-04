// =============================================================================
// SCRIPT: CCS_RegionSpecializationType
// CATEGORY: Modules / Regions / Runtime / Data
// PURPOSE: Economic identity archetype for frontier world regions.
// PLACEMENT: Used by region definitions, contracts, and world simulation.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.1.0 regional specialization foundation.
// =============================================================================

namespace CCS.Modules.Regions
{
    public enum CCS_RegionSpecializationType
    {
        Unknown = 0,
        Agriculture = 1,
        Ranching = 2,
        Mining = 3,
        Timber = 4,
        FrontierMixed = 5
    }
}
