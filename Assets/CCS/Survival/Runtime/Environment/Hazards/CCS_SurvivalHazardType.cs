// =============================================================================
// SCRIPT: CCS_SurvivalHazardType
// CATEGORY: Survival / Environment / Hazards
// PURPOSE: Identifies environmental hazard categories for prototype survival pressure zones.
// PLACEMENT: Used by CCS_SurvivalHazardZone and CCS_SurvivalHazardProfile.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Radiation is placeholder-safe for future expansion.
// =============================================================================

namespace CCS.Survival.Environment.Hazards
{
    public enum CCS_SurvivalHazardType
    {
        Cold = 0,
        Heat = 1,
        Toxic = 2,
        Radiation = 3,
        GenericDamage = 4
    }
}
