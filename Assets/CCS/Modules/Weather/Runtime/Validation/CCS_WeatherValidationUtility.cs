using System;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_WeatherValidationUtility
// CATEGORY: Modules / Weather / Runtime / Validation
// PURPOSE: Profile and weather type validation helpers for runtime and editor checks.
// PLACEMENT: Used by weather service initialization and editor validation pipeline.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Foundation validation only. No visual or audio requirements.
// =============================================================================

namespace CCS.Modules.Weather
{
    public static class CCS_WeatherValidationUtility
    {
        #region Public Methods

        public static CCS_SurvivalValidationResult ValidateProfile(CCS_WeatherProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail("Weather profile is null.");
            }

            CCS_SurvivalValidationResult baseValidation = CCS_SurvivalProfileValidationUtility.ValidateProfile(profile);
            if (!baseValidation.IsSuccess)
            {
                return baseValidation;
            }

            if (profile.MinimumWeatherDurationSeconds < 0f)
            {
                return CCS_SurvivalValidationResult.Fail("Minimum weather duration cannot be negative.");
            }

            if (profile.MaximumWeatherDurationSeconds < profile.MinimumWeatherDurationSeconds)
            {
                return CCS_SurvivalValidationResult.Fail(
                    "Maximum weather duration must be greater than or equal to minimum duration.");
            }

            if (profile.TransitionDurationSeconds < 0f)
            {
                return CCS_SurvivalValidationResult.Fail("Transition duration cannot be negative.");
            }

            if (!IsDefinedWeatherType(profile.StartingWeather))
            {
                return CCS_SurvivalValidationResult.Fail("Starting weather is not a supported weather type.");
            }

            return CCS_SurvivalValidationResult.Pass("Weather profile validated.");
        }

        public static bool IsDefinedWeatherType(CCS_WeatherType weatherType)
        {
            return Enum.IsDefined(typeof(CCS_WeatherType), weatherType);
        }

        public static bool ValidateRequiredWeatherTypes(out string missingTypesMessage)
        {
            missingTypesMessage = string.Empty;
            string[] requiredNames =
            {
                nameof(CCS_WeatherType.Clear),
                nameof(CCS_WeatherType.Cloudy),
                nameof(CCS_WeatherType.Rain),
                nameof(CCS_WeatherType.Storm),
                nameof(CCS_WeatherType.Fog)
            };

            for (int index = 0; index < requiredNames.Length; index++)
            {
                if (!Enum.IsDefined(typeof(CCS_WeatherType), requiredNames[index]))
                {
                    missingTypesMessage = $"Missing required weather enum entry: {requiredNames[index]}";
                    return false;
                }
            }

            return true;
        }

        public static string FormatWeatherDisplay(CCS_WeatherSnapshot snapshot)
        {
            if (snapshot.IsTransitioning)
            {
                int progressPercent = (int)(snapshot.TransitionProgress * 100f);
                return $"Weather: {snapshot.TransitionTargetWeather} ({progressPercent}%)";
            }

            return $"Weather: {snapshot.CurrentWeather}";
        }

        #endregion
    }
}
