// =============================================================================
// SCRIPT: CCS_CampTier
// CATEGORY: Modules / Shelter / Runtime / Data
// PURPOSE: Frontier camp progression tier used by camp tracking.
// PLACEMENT: Referenced by shelter definitions and CCS_CampService.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// =============================================================================

namespace CCS.Modules.Shelter
{
    public enum CCS_CampTier
    {
        None = 0,
        TemporaryCamp = 1,
        FrontierCamp = 2,
        FrontierHomestead = 3
    }
}
