// =============================================================================
// SCRIPT: CCS_WeatherSnapshot
// CATEGORY: Modules / Weather / Runtime / Data
// PURPOSE: Read-only weather snapshot for HUD, debug, and future environment systems.
// PLACEMENT: Produced by CCS_WeatherService.GetSnapshot().
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Immutable-friendly value type. No gameplay mutation.
// =============================================================================

namespace CCS.Modules.Weather
{
    public readonly struct CCS_WeatherSnapshot
    {
        #region Public Methods

        public CCS_WeatherSnapshot(
            CCS_WeatherType currentWeather,
            CCS_WeatherType previousWeather,
            CCS_WeatherType transitionTargetWeather,
            float transitionProgress,
            float remainingDurationSeconds,
            float temperatureModifier,
            float wetnessModifier,
            bool isTransitioning,
            bool isPaused)
        {
            CurrentWeather = currentWeather;
            PreviousWeather = previousWeather;
            TransitionTargetWeather = transitionTargetWeather;
            TransitionProgress = transitionProgress < 0f ? 0f : transitionProgress > 1f ? 1f : transitionProgress;
            RemainingDurationSeconds = remainingDurationSeconds < 0f ? 0f : remainingDurationSeconds;
            TemperatureModifier = temperatureModifier;
            WetnessModifier = wetnessModifier;
            IsTransitioning = isTransitioning;
            IsPaused = isPaused;
        }

        public static CCS_WeatherSnapshot Empty =>
            new CCS_WeatherSnapshot(
                CCS_WeatherType.Clear,
                CCS_WeatherType.Clear,
                CCS_WeatherType.Clear,
                0f,
                0f,
                0f,
                0f,
                false,
                false);

        #endregion

        #region Properties

        public CCS_WeatherType CurrentWeather { get; }

        public CCS_WeatherType PreviousWeather { get; }

        public CCS_WeatherType TransitionTargetWeather { get; }

        public float TransitionProgress { get; }

        public float RemainingDurationSeconds { get; }

        public float TemperatureModifier { get; }

        public float WetnessModifier { get; }

        public bool IsTransitioning { get; }

        public bool IsPaused { get; }

        #endregion
    }
}
