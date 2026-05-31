using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_TimeOfDayValidationUtility
// CATEGORY: Modules / TimeOfDay / Runtime / Validation
// PURPOSE: Runtime-safe validation for time-of-day profiles and phase boundaries.
// PLACEMENT: Used by editor validators and bootstrap setup.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Returns CCS_SurvivalValidationResult. No UnityEditor references.
// =============================================================================

namespace CCS.Modules.TimeOfDay
{
    public static class CCS_TimeOfDayValidationUtility
    {
        #region Public Methods

        public static CCS_SurvivalValidationResult ValidateProfile(CCS_TimeOfDayProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail("Time of day profile is null.");
            }

            CCS_SurvivalValidationResult baseValidation = CCS_SurvivalProfileValidationUtility.ValidateProfile(profile);
            if (!baseValidation.IsSuccess)
            {
                return baseValidation;
            }

            if (profile.StartDay < 1)
            {
                return CCS_SurvivalValidationResult.Fail("Start day must be at least 1.");
            }

            if (!IsValidHour(profile.StartHour))
            {
                return CCS_SurvivalValidationResult.Fail("Start hour must be between 0 and 23.");
            }

            if (!IsValidMinute(profile.StartMinute))
            {
                return CCS_SurvivalValidationResult.Fail("Start minute must be between 0 and 59.");
            }

            if (profile.RealSecondsPerGameDay <= 0f)
            {
                return CCS_SurvivalValidationResult.Fail("Real seconds per game day must be greater than zero.");
            }

            return ValidatePhaseOrder(profile);
        }

        public static CCS_SurvivalValidationResult ValidatePhaseOrder(CCS_TimeOfDayProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail("Time of day profile is null.");
            }

            if (!IsValidHour(profile.DawnStartHour)
                || !IsValidHour(profile.DayStartHour)
                || !IsValidHour(profile.DuskStartHour)
                || !IsValidHour(profile.NightStartHour))
            {
                return CCS_SurvivalValidationResult.Fail("Phase boundary hours must be between 0 and 23.");
            }

            if (profile.DawnStartHour >= profile.DayStartHour)
            {
                return CCS_SurvivalValidationResult.Fail("Dawn start hour must be before day start hour.");
            }

            if (profile.DayStartHour >= profile.DuskStartHour)
            {
                return CCS_SurvivalValidationResult.Fail("Day start hour must be before dusk start hour.");
            }

            if (profile.DuskStartHour >= profile.NightStartHour)
            {
                return CCS_SurvivalValidationResult.Fail("Dusk start hour must be before night start hour.");
            }

            return CCS_SurvivalValidationResult.Pass("Time of day profile and phase order validated.");
        }

        public static CCS_TimeOfDayPhase ResolvePhase(int hour, CCS_TimeOfDayProfile profile)
        {
            if (profile == null)
            {
                return CCS_TimeOfDayPhase.Day;
            }

            int normalizedHour = NormalizeHour(hour);

            if (normalizedHour >= profile.DuskStartHour && normalizedHour < profile.NightStartHour)
            {
                return CCS_TimeOfDayPhase.Dusk;
            }

            if (normalizedHour >= profile.NightStartHour || normalizedHour < profile.DawnStartHour)
            {
                return CCS_TimeOfDayPhase.Night;
            }

            if (normalizedHour >= profile.DawnStartHour && normalizedHour < profile.DayStartHour)
            {
                return CCS_TimeOfDayPhase.Dawn;
            }

            return CCS_TimeOfDayPhase.Day;
        }

        public static int NormalizeHour(int hour)
        {
            if (hour < 0)
            {
                return 0;
            }

            return hour > 23 ? 23 : hour;
        }

        public static int NormalizeMinute(int minute)
        {
            if (minute < 0)
            {
                return 0;
            }

            return minute > 59 ? 59 : minute;
        }

        public static float ConvertHourMinuteToMinutesIntoDay(int hour, int minute)
        {
            return NormalizeHour(hour) * 60f + NormalizeMinute(minute);
        }

        #endregion

        #region Private Methods

        private static bool IsValidHour(int hour)
        {
            return hour >= 0 && hour <= 23;
        }

        private static bool IsValidMinute(int minute)
        {
            return minute >= 0 && minute <= 59;
        }

        #endregion
    }
}
