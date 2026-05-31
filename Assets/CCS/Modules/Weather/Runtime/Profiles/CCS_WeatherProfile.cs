using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_WeatherProfile
// CATEGORY: Modules / Weather / Runtime / Profiles
// PURPOSE: Tuning profile for weather types, durations, transitions, and modifiers.
// PLACEMENT: Assets/CCS/Survival/Profiles/Weather/ (project shell configuration).
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: No VFX, lighting, audio, or Survival Core stat mutation in 0.7.1.
// =============================================================================

namespace CCS.Modules.Weather
{
    [CreateAssetMenu(
        fileName = "CCS_WeatherProfile",
        menuName = "CCS/Survival/Weather/Weather Profile")]
    public sealed class CCS_WeatherProfile : CCS_SurvivalProfileBase
    {
        #region Variables

        [Header("Starting Weather")]
        [Tooltip("Weather applied when the service initializes.")]
        [SerializeField] private CCS_WeatherType startingWeather = CCS_WeatherType.Clear;

        [Tooltip("When enabled, weather automatically transitions after duration expires.")]
        [SerializeField] private bool weatherChangeEnabled = true;

        [Header("Weather Duration (Seconds)")]
        [Tooltip("Minimum real-time seconds before automatic weather may change.")]
        [SerializeField] private float minimumWeatherDurationSeconds = 300f;

        [Tooltip("Maximum real-time seconds before automatic weather may change.")]
        [SerializeField] private float maximumWeatherDurationSeconds = 900f;

        [Tooltip("Real-time seconds used for timed weather transitions.")]
        [SerializeField] private float transitionDurationSeconds = 20f;

        [Header("Temperature Modifiers")]
        [SerializeField] private float clearTemperatureModifier;
        [SerializeField] private float cloudyTemperatureModifier = -1f;
        [SerializeField] private float rainTemperatureModifier = -3f;
        [SerializeField] private float stormTemperatureModifier = -5f;
        [SerializeField] private float fogTemperatureModifier = -2f;

        [Header("Wetness Modifiers")]
        [SerializeField] private float clearWetnessModifier;
        [SerializeField] private float cloudyWetnessModifier;
        [SerializeField] private float rainWetnessModifier = 0.5f;
        [SerializeField] private float stormWetnessModifier = 0.8f;
        [SerializeField] private float fogWetnessModifier = 0.2f;

        #endregion

        #region Properties

        public CCS_WeatherType StartingWeather => startingWeather;

        public bool WeatherChangeEnabled => weatherChangeEnabled;

        public float MinimumWeatherDurationSeconds => minimumWeatherDurationSeconds;

        public float MaximumWeatherDurationSeconds => maximumWeatherDurationSeconds;

        public float TransitionDurationSeconds => transitionDurationSeconds;

        #endregion

        #region Public Methods

        public float GetTemperatureModifier(CCS_WeatherType weatherType)
        {
            switch (weatherType)
            {
                case CCS_WeatherType.Cloudy:
                    return cloudyTemperatureModifier;
                case CCS_WeatherType.Rain:
                    return rainTemperatureModifier;
                case CCS_WeatherType.Storm:
                    return stormTemperatureModifier;
                case CCS_WeatherType.Fog:
                    return fogTemperatureModifier;
                default:
                    return clearTemperatureModifier;
            }
        }

        public float GetWetnessModifier(CCS_WeatherType weatherType)
        {
            switch (weatherType)
            {
                case CCS_WeatherType.Cloudy:
                    return cloudyWetnessModifier;
                case CCS_WeatherType.Rain:
                    return rainWetnessModifier;
                case CCS_WeatherType.Storm:
                    return stormWetnessModifier;
                case CCS_WeatherType.Fog:
                    return fogWetnessModifier;
                default:
                    return clearWetnessModifier;
            }
        }

        #endregion
    }
}
