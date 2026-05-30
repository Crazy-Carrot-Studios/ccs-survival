using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_UIValidationUtility
// CATEGORY: Modules / UI / Runtime / Validation
// PURPOSE: Runtime-safe validation for HUD profiles and module structure checks.
// PLACEMENT: Used by editor validators and bootstrap checks.
// AUTHOR: James Schilz
// CREATED: 2026-05-30
// NOTES: Returns CCS_SurvivalValidationResult. No UnityEditor references.
// =============================================================================

namespace CCS.Modules.UI
{
    public static class CCS_UIValidationUtility
    {
        #region Public Methods

        public static CCS_SurvivalValidationResult ValidateProfile(CCS_HudProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail("HUD profile is null.");
            }

            CCS_SurvivalValidationResult baseValidation = CCS_SurvivalProfileValidationUtility.ValidateProfile(profile);
            if (!baseValidation.IsSuccess)
            {
                return baseValidation;
            }

            CCS_NotificationProfile notificationProfile = profile.NotificationProfile;
            if (notificationProfile == null)
            {
                return CCS_SurvivalValidationResult.Warn("HUD profile notification settings are null.");
            }

            if (notificationProfile.MaxVisibleCount <= 0)
            {
                return CCS_SurvivalValidationResult.Fail("Notification max visible count must be greater than zero.");
            }

            if (notificationProfile.NotificationLifetimeSeconds <= 0f)
            {
                return CCS_SurvivalValidationResult.Fail("Notification lifetime must be greater than zero.");
            }

            if (notificationProfile.NotificationWidth <= 0f)
            {
                return CCS_SurvivalValidationResult.Fail("Notification width must be greater than zero.");
            }

            if (notificationProfile.NotificationRowHeight <= 0f)
            {
                return CCS_SurvivalValidationResult.Fail("Notification row height must be greater than zero.");
            }

            CCS_HudLayoutSettings layoutSettings = profile.LayoutSettings;
            if (layoutSettings == null)
            {
                return CCS_SurvivalValidationResult.Warn("HUD profile layout settings are null.");
            }

            if (layoutSettings.SurvivalBarWidth <= 0f || layoutSettings.SurvivalBarHeight <= 0f)
            {
                return CCS_SurvivalValidationResult.Fail("Survival bar layout dimensions must be greater than zero.");
            }

            return CCS_SurvivalValidationResult.Pass("HUD profile validated.");
        }

        #endregion
    }
}
