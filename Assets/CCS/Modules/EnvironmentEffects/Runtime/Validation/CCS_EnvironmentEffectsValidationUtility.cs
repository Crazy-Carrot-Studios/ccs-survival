using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_EnvironmentEffectsValidationUtility
// CATEGORY: Modules / EnvironmentEffects / Runtime / Validation
// PURPOSE: Profile validation helpers for runtime and editor checks.
// PLACEMENT: Used by environment service initialization and editor validation pipeline.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Foundation validation only. No Survival Core mutation requirements.
// =============================================================================

namespace CCS.Modules.EnvironmentEffects
{
    public static class CCS_EnvironmentEffectsValidationUtility
    {
        #region Public Methods

        public static CCS_SurvivalValidationResult ValidateProfile(CCS_EnvironmentEffectsProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail("Environment effects profile is null.");
            }

            CCS_SurvivalValidationResult baseValidation = CCS_SurvivalProfileValidationUtility.ValidateProfile(profile);
            if (!baseValidation.IsSuccess)
            {
                return baseValidation;
            }

            return CCS_SurvivalValidationResult.Pass("Environment effects profile validated.");
        }

        public static string FormatEnvironmentDisplay(CCS_EnvironmentSnapshot snapshot)
        {
            return
                $"Env Temp: {snapshot.AmbientTemperature:0.#}\n" +
                $"Wetness: {snapshot.Wetness:0.##}\n" +
                $"Exposure: {snapshot.Exposure:0.##}";
        }

        #endregion
    }
}
