// =============================================================================
// SCRIPT: CCS_WildlifeType
// CATEGORY: Modules / Wildlife / Runtime / Definitions
// PURPOSE: High-level wildlife classification for resource foundation placeholders.
// PLACEMENT: Referenced by CCS_WildlifeDefinition assets.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Predator is a placeholder only. No AI or combat behavior in 0.9.3.
// =============================================================================

namespace CCS.Modules.Wildlife
{
    public enum CCS_WildlifeType
    {
        SmallGame = 0,
        Deer = 1,
        Predator = 2,
        Bird = 3,
        Fish = 4,
        Custom = 5
    }
}
