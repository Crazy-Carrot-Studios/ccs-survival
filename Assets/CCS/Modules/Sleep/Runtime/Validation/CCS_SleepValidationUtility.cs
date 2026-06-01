using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_SleepValidationUtility
// CATEGORY: Modules / Sleep / Runtime / Validation
// PURPOSE: Runtime-safe validation for sleep profiles and sleep hour rules.
// PLACEMENT: Used by editor validators and CCS_SleepService preflight checks.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Returns CCS_SurvivalValidationResult. Runtime-only; no editor APIs.
// =============================================================================

namespace CCS.Modules.Sleep
{
    public static class CCS_SleepValidationUtility
    {
        #region Public Methods

        public static CCS_SurvivalValidationResult ValidateProfile(CCS_SleepProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail("Sleep profile is null.");
            }

            CCS_SurvivalValidationResult baseValidation = CCS_SurvivalProfileValidationUtility.ValidateProfile(profile);
            if (!baseValidation.IsSuccess)
            {
                return baseValidation;
            }

            if (profile.DefaultSleepHours <= 0f)
            {
                return CCS_SurvivalValidationResult.Fail("Default sleep hours must be greater than zero.");
            }

            if (profile.MinimumSleepHours <= 0f || profile.MaximumSleepHours <= 0f)
            {
                return CCS_SurvivalValidationResult.Fail("Minimum and maximum sleep hours must be greater than zero.");
            }

            if (profile.MinimumSleepHours > profile.MaximumSleepHours)
            {
                return CCS_SurvivalValidationResult.Fail("Minimum sleep hours must be <= maximum sleep hours.");
            }

            if (profile.DefaultSleepHours < profile.MinimumSleepHours
                || profile.DefaultSleepHours > profile.MaximumSleepHours)
            {
                return CCS_SurvivalValidationResult.Fail(
                    "Default sleep hours must be within minimum and maximum bounds.");
            }

            if (profile.FatigueRestorePerHour < 0f)
            {
                return CCS_SurvivalValidationResult.Fail("Fatigue restore per hour must be non-negative.");
            }

            if (profile.UnshelteredFatigueRestoreMultiplier < 0f)
            {
                return CCS_SurvivalValidationResult.Fail("Unsheltered fatigue restore multiplier must be non-negative.");
            }

            if (profile.HungerDrainDuringSleepMultiplier < 0f
                || profile.ThirstDrainDuringSleepMultiplier < 0f)
            {
                return CCS_SurvivalValidationResult.Fail("Sleep stat drain multipliers must be non-negative.");
            }

            if (profile.RequireBedroll && profile.BedrollItemDefinition == null)
            {
                return CCS_SurvivalValidationResult.Fail("Bedroll item definition is required when requireBedroll is enabled.");
            }

            return CCS_SurvivalValidationResult.Pass("Sleep profile validated.");
        }

        public static float ClampSleepHours(CCS_SleepProfile profile, float requestedHours)
        {
            if (profile == null)
            {
                return 0f;
            }

            float resolvedHours = requestedHours > 0f ? requestedHours : profile.DefaultSleepHours;
            if (resolvedHours < profile.MinimumSleepHours)
            {
                resolvedHours = profile.MinimumSleepHours;
            }

            if (resolvedHours > profile.MaximumSleepHours)
            {
                resolvedHours = profile.MaximumSleepHours;
            }

            return resolvedHours;
        }

        #endregion
    }
}
