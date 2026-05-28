// =============================================================================
// SCRIPT: CCS_SurvivalVitalsModifierType
// CATEGORY: Survival / Environment / VitalsZones
// PURPOSE: Identifies direct survival vitals modifier categories for prototype overlap testbed zones.
// PLACEMENT: Used by CCS_SurvivalVitalsModifierZone and CCS_SurvivalVitalsModifierProfile.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Broad weather/hazard pressure remains on CCS_SurvivalHazardZone. No inventory or items yet.
// =============================================================================

namespace CCS.Survival.Environment.VitalsZones
{
    public enum CCS_SurvivalVitalsModifierType
    {
        HungerDrain = 0,
        HungerRestore = 1,
        ThirstDrain = 2,
        ThirstRestore = 3,
        StaminaDrain = 4,
        StaminaRestore = 5,
        ExposureIncrease = 6,
        ExposureRecovery = 7,
        TemperatureIncrease = 8,
        TemperatureDecrease = 9,
        HealthDrain = 10,
        HealthRestore = 11
    }
}
