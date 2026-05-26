// =============================================================================
// SCRIPT: CCS_SurvivalProfileValidationUtility
// CATEGORY: Survival / Runtime / Foundation / Profiles
// PURPOSE: Static validation for survival setup profile ScriptableObject assets.
// PLACEMENT: Runtime utility. Not attached to GameObjects. No gameplay configuration rules.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: profileId must be save-stable. No Unity asset path or scene reference identity.
// =============================================================================

namespace CCS.Survival
{
    public static class CCS_SurvivalProfileValidationUtility
    {
        #region Public Methods

        public static CCS_SurvivalValidationResult ValidateProfile(CCS_SurvivalProfileBase profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail("Survival profile reference is null.");
            }

            if (string.IsNullOrWhiteSpace(profile.ProfileId))
            {
                return CCS_SurvivalValidationResult.Fail(CCS_SurvivalRuntimeConstants.InvalidProfileIdMessage);
            }

            if (string.IsNullOrWhiteSpace(profile.ProfileDisplayName))
            {
                return CCS_SurvivalValidationResult.Fail("Survival profile display name is null or empty.");
            }

            if (string.IsNullOrWhiteSpace(profile.ProfileVersion))
            {
                return CCS_SurvivalValidationResult.Fail("Survival profile version is null or empty.");
            }

            CCS_SurvivalValidationResult profileIdValidation = CCS_SurvivalIdentityUtility.ValidateProfileId(profile.ProfileId);
            if (!profileIdValidation.IsSuccess)
            {
                return profileIdValidation;
            }

            return CCS_SurvivalValidationResult.Pass(
                $"Survival profile validated: {profile.ProfileId}.");
        }

        #endregion
    }
}
