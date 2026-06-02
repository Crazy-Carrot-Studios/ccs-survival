// =============================================================================
// SCRIPT: CCS_RegionType
// CATEGORY: Modules / Regions / Runtime / Data
// PURPOSE: Generic frontier world region archetypes.
// PLACEMENT: Referenced by CCS_RegionDefinition and discovery snapshots.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// NOTES: Milestone 1.9.0 frontier region foundation.
// =============================================================================

namespace CCS.Modules.Regions
{
    public enum CCS_RegionType
    {
        Forest = 0,
        Creek = 1,
        Mine = 2,
        TradingPost = 3,
        Other = 99
    }
}
