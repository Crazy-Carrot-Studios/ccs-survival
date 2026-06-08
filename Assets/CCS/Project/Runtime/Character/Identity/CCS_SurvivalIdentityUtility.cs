// =============================================================================
// SCRIPT: CCS_SurvivalIdentityUtility
// CATEGORY: Survival / Runtime / Character / Identity
// PURPOSE: Static validation for save-stable authority, avatar, profile, and binding identity strings.
// PLACEMENT: Runtime utility. Not attached to GameObjects. No save IO or scene lookup.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: Rejects Unity instance IDs, asset paths, scene paths, and GameObject names as authoritative identity.
// =============================================================================

namespace CCS.Project
{
    public static class CCS_SurvivalIdentityUtility
    {
        #region Public Methods

        public static CCS_SurvivalValidationResult ValidateAuthorityId(string authorityId)
        {
            return ValidateStableRuntimeId(
                authorityId,
                CCS_SurvivalRuntimeConstants.AuthorityIdPrefix,
                "Survival authority ID");
        }

        public static CCS_SurvivalValidationResult ValidateAvatarId(string avatarId)
        {
            return ValidateStableRuntimeId(
                avatarId,
                CCS_SurvivalRuntimeConstants.AvatarIdPrefix,
                "Survival avatar ID");
        }

        public static CCS_SurvivalValidationResult ValidateBindingId(string bindingId)
        {
            return ValidateStableRuntimeId(
                bindingId,
                CCS_SurvivalRuntimeConstants.BindingIdPrefix,
                "Survival authority-avatar binding ID");
        }

        public static CCS_SurvivalValidationResult ValidateProfileId(string profileId)
        {
            if (string.IsNullOrWhiteSpace(profileId))
            {
                return CCS_SurvivalValidationResult.Fail(CCS_SurvivalRuntimeConstants.InvalidProfileIdMessage);
            }

            if (!profileId.StartsWith(CCS_SurvivalRuntimeConstants.ProfileIdPrefix))
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Survival profile ID must start with '{CCS_SurvivalRuntimeConstants.ProfileIdPrefix}'.");
            }

            if (!IsLowercaseDotSeparatedFormat(profileId))
            {
                return CCS_SurvivalValidationResult.Fail(CCS_SurvivalRuntimeConstants.StableRuntimeIdentityGuidanceMessage);
            }

            if (UsesForbiddenUnityIdentitySource(profileId))
            {
                return CCS_SurvivalValidationResult.Fail(CCS_SurvivalRuntimeConstants.ForbiddenUnityIdentitySourceMessage);
            }

            return CCS_SurvivalValidationResult.Pass();
        }

        public static CCS_SurvivalValidationResult ValidateStableRuntimeId(
            string runtimeId,
            string requiredPrefix,
            string identityLabel)
        {
            if (string.IsNullOrWhiteSpace(runtimeId))
            {
                return CCS_SurvivalValidationResult.Fail($"{identityLabel} is null or empty.");
            }

            if (runtimeId != runtimeId.Trim())
            {
                return CCS_SurvivalValidationResult.Fail($"{identityLabel} must not contain leading or trailing whitespace.");
            }

            if (!runtimeId.StartsWith(requiredPrefix))
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"{identityLabel} must start with '{requiredPrefix}'.");
            }

            if (!IsLowercaseDotSeparatedFormat(runtimeId))
            {
                return CCS_SurvivalValidationResult.Fail(CCS_SurvivalRuntimeConstants.StableRuntimeIdentityGuidanceMessage);
            }

            if (UsesForbiddenUnityIdentitySource(runtimeId))
            {
                return CCS_SurvivalValidationResult.Fail(CCS_SurvivalRuntimeConstants.ForbiddenUnityIdentitySourceMessage);
            }

            return CCS_SurvivalValidationResult.Pass();
        }

        #endregion

        #region Private Methods

        private static bool IsLowercaseDotSeparatedFormat(string runtimeId)
        {
            for (int index = 0; index < runtimeId.Length; index++)
            {
                char character = runtimeId[index];
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

        private static bool UsesForbiddenUnityIdentitySource(string runtimeId)
        {
            if (runtimeId.Contains("Assets/") || runtimeId.Contains("Assets\\"))
            {
                return true;
            }

            if (runtimeId.Contains('/') || runtimeId.Contains('\\'))
            {
                return true;
            }

            if (runtimeId.Contains(" ") || runtimeId.Contains(":"))
            {
                return true;
            }

            return false;
        }

        #endregion
    }
}
