// =============================================================================
// SCRIPT: CCS_WeatherState
// CATEGORY: Modules / Weather / Runtime / Data
// PURPOSE: Mutable runtime weather state owned by CCS_WeatherService.
// PLACEMENT: Internal to weather service tick and save/restore flows.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Not exposed directly; use CCS_WeatherSnapshot for read-only access.
// =============================================================================

namespace CCS.Modules.Weather
{
    public sealed class CCS_WeatherState
    {
        #region Variables

        public CCS_WeatherType CurrentWeather = CCS_WeatherType.Clear;

        public CCS_WeatherType PreviousWeather = CCS_WeatherType.Clear;

        public CCS_WeatherType TransitionTargetWeather = CCS_WeatherType.Clear;

        public float TransitionProgress;

        public float RemainingDurationSeconds;

        public bool IsTransitioning;

        public bool IsPaused;

        #endregion
    }
}
