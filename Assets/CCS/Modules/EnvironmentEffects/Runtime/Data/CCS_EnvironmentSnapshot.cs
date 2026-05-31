using CCS.Modules.TimeOfDay;
using CCS.Modules.Weather;

// =============================================================================
// SCRIPT: CCS_EnvironmentSnapshot
// CATEGORY: Modules / EnvironmentEffects / Runtime / Data
// PURPOSE: Read-only environment snapshot for HUD and future Survival Core integration.
// PLACEMENT: Produced by CCS_EnvironmentEffectsService.GetSnapshot().
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Simulation layer only. No stat mutation in 0.7.2.
// =============================================================================

namespace CCS.Modules.EnvironmentEffects
{
    public readonly struct CCS_EnvironmentSnapshot
    {
        #region Public Methods

        public CCS_EnvironmentSnapshot(
            float ambientTemperature,
            float wetness,
            float exposure,
            CCS_WeatherType weatherType,
            CCS_TimeOfDayPhase timePhase)
        {
            AmbientTemperature = ambientTemperature;
            Wetness = wetness < 0f ? 0f : wetness;
            Exposure = exposure < 0f ? 0f : exposure;
            WeatherType = weatherType;
            TimePhase = timePhase;
        }

        public static CCS_EnvironmentSnapshot Empty =>
            new CCS_EnvironmentSnapshot(0f, 0f, 0f, CCS_WeatherType.Clear, CCS_TimeOfDayPhase.Dawn);

        #endregion

        #region Properties

        public float AmbientTemperature { get; }

        public float Wetness { get; }

        public float Exposure { get; }

        public CCS_WeatherType WeatherType { get; }

        public CCS_TimeOfDayPhase TimePhase { get; }

        #endregion
    }
}
