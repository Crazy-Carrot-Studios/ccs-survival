// =============================================================================
// SCRIPT: CCS_SurvivalAuthorityAvatarValidationUtility
// CATEGORY: Survival / Runtime / Character / Avatar
// PURPOSE: Static validation for authority, avatar, and authority-avatar binding contracts.
// PLACEMENT: Runtime utility. Not attached to GameObjects. No spawn, scene lookup, or save IO.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: Ensures avatar AuthorityId matches authority AuthorityId when both contracts are present.
// =============================================================================

namespace CCS.Project
{
    public static class CCS_SurvivalAuthorityAvatarValidationUtility
    {
        #region Public Methods

        public static CCS_SurvivalValidationResult ValidateAuthority(CCS_ISurvivalAuthority authority)
        {
            if (authority == null)
            {
                return CCS_SurvivalValidationResult.Fail("Survival authority reference is null.");
            }

            CCS_SurvivalValidationResult authorityIdValidation =
                CCS_SurvivalIdentityUtility.ValidateAuthorityId(authority.AuthorityId);
            if (!authorityIdValidation.IsSuccess)
            {
                return authorityIdValidation;
            }

            if (string.IsNullOrWhiteSpace(authority.DisplayName))
            {
                return CCS_SurvivalValidationResult.Warn("Survival authority display name is empty (diagnostics/UI only).");
            }

            return CCS_SurvivalValidationResult.Pass();
        }

        public static CCS_SurvivalValidationResult ValidateAvatar(CCS_ISurvivalAvatar avatar)
        {
            if (avatar == null)
            {
                return CCS_SurvivalValidationResult.Fail("Survival avatar reference is null.");
            }

            CCS_SurvivalValidationResult avatarIdValidation = CCS_SurvivalIdentityUtility.ValidateAvatarId(avatar.AvatarId);
            if (!avatarIdValidation.IsSuccess)
            {
                return avatarIdValidation;
            }

            return CCS_SurvivalIdentityUtility.ValidateAuthorityId(avatar.AuthorityId);
        }

        public static CCS_SurvivalValidationResult ValidateAuthorityAvatarMatch(
            CCS_ISurvivalAuthority authority,
            CCS_ISurvivalAvatar avatar)
        {
            CCS_SurvivalValidationResult authorityValidation = ValidateAuthority(authority);
            if (!authorityValidation.IsSuccess)
            {
                return authorityValidation;
            }

            CCS_SurvivalValidationResult avatarValidation = ValidateAvatar(avatar);
            if (!avatarValidation.IsSuccess)
            {
                return avatarValidation;
            }

            if (!string.Equals(authority.AuthorityId, avatar.AuthorityId))
            {
                return CCS_SurvivalValidationResult.Fail(
                    $"Avatar AuthorityId '{avatar.AuthorityId}' does not match authority AuthorityId '{authority.AuthorityId}'.");
            }

            return CCS_SurvivalValidationResult.Pass();
        }

        public static CCS_SurvivalValidationResult ValidateBinding(CCS_SurvivalAuthorityAvatarBinding binding)
        {
            CCS_SurvivalValidationResult bindingValidation = binding.ValidateBinding();
            if (!bindingValidation.IsSuccess)
            {
                return bindingValidation;
            }

            return CCS_SurvivalValidationResult.Pass();
        }

        #endregion
    }
}
