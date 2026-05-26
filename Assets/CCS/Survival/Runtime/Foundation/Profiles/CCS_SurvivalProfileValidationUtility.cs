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

            if (!profile.ProfileId.StartsWith(CCS_SurvivalRuntimeConstants.ProfileIdPrefix))
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Survival profile ID must start with '{CCS_SurvivalRuntimeConstants.ProfileIdPrefix}'.");
            }

            if (!IsSaveStableProfileId(profile.ProfileId))
            {
                return CCS_SurvivalValidationResult.Fail(CCS_SurvivalRuntimeConstants.SaveStableIdGuidanceMessage);
            }

            return CCS_SurvivalValidationResult.Pass();
        }

        #endregion

        #region Private Methods

        private static bool IsSaveStableProfileId(string profileId)
        {
            for (int index = 0; index < profileId.Length; index++)
            {
                char character = profileId[index];
                bool isLowerLetter = character >= 'a' && character <= 'z';
                bool isDigit = character >= '0' && character <= '9';
                bool isSeparator = character == '.' || character == '-';

                if (!isLowerLetter && !isDigit && !isSeparator)
                {
                    return false;
                }
            }

            return true;
        }

        #endregion
    }
}
