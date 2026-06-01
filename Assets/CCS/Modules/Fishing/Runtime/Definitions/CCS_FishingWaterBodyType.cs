// =============================================================================
// SCRIPT: CCS_FishingWaterBodyType
// CATEGORY: Modules / Fishing / Runtime / Definitions
// PURPOSE: Labels primitive frontier fishing spots (river, pond, lake, stream).
// PLACEMENT: Serialized on CCS_FishingSpotDefinition for display and validation.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Does not drive terrain or shader requirements in 1.2.5 foundation.
// =============================================================================

namespace CCS.Modules.Fishing
{
    public enum CCS_FishingWaterBodyType
    {
        RiverEdge = 0,
        Pond = 1,
        Lake = 2,
        Stream = 3
    }
}
