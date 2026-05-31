using CCS.Modules.Weather;
using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_EnvironmentEffectsProfile
// CATEGORY: Modules / EnvironmentEffects / Runtime / Profiles
// PURPOSE: Tuning profile for ambient temperature, wetness, and exposure simulation.
// PLACEMENT: Assets/CCS/Survival/Profiles/EnvironmentEffects/ (project shell configuration).
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Does not mutate Survival Core stats or apply damage in 0.7.2.
// =============================================================================

namespace CCS.Modules.EnvironmentEffects
{
    [CreateAssetMenu(
        fileName = "CCS_EnvironmentEffectsProfile",
        menuName = "CCS/Survival/Environment Effects/Environment Effects Profile")]
    public sealed class CCS_EnvironmentEffectsProfile : CCS_SurvivalProfileBase
    {
        #region Variables

        [Header("Time Of Day Temperature")]
        [Tooltip("Ambient temperature bonus applied during Day phase.")]
        [SerializeField] private float dayTemperatureBonus = 2f;

        [Tooltip("Ambient temperature penalty applied during Night phase.")]
        [SerializeField] private float nightTemperaturePenalty = -3f;

        [Header("Weather Temperature Modifiers")]
        [SerializeField] private float clearTemperatureModifier;
        [SerializeField] private float cloudyTemperatureModifier = -1f;
        [SerializeField] private float rainTemperatureModifier = -3f;
        [SerializeField] private float stormTemperatureModifier = -5f;
        [SerializeField] private float fogTemperatureModifier = -2f;

        [Header("Weather Wetness Modifiers")]
        [SerializeField] private float clearWetnessModifier;
        [SerializeField] private float cloudyWetnessModifier;
        [SerializeField] private float rainWetnessModifier = 0.5f;
        [SerializeField] private float stormWetnessModifier = 0.8f;
        [SerializeField] private float fogWetnessModifier = 0.2f;

        [Header("Weather Exposure Modifiers")]
        [SerializeField] private float clearExposureModifier;
        [SerializeField] private float cloudyExposureModifier = 0.1f;
        [SerializeField] private float rainExposureModifier = 0.4f;
        [SerializeField] private float stormExposureModifier = 0.8f;
        [SerializeField] private float fogExposureModifier = 0.2f;

        #endregion

        #region Properties

        public float DayTemperatureBonus => dayTemperatureBonus;

        public float NightTemperaturePenalty => nightTemperaturePenalty;

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

        public float GetExposureModifier(CCS_WeatherType weatherType)
        {
            switch (weatherType)
            {
                case CCS_WeatherType.Cloudy:
                    return cloudyExposureModifier;
                case CCS_WeatherType.Rain:
                    return rainExposureModifier;
                case CCS_WeatherType.Storm:
                    return stormExposureModifier;
                case CCS_WeatherType.Fog:
                    return fogExposureModifier;
                default:
                    return clearExposureModifier;
            }
        }

        #endregion
    }
}
