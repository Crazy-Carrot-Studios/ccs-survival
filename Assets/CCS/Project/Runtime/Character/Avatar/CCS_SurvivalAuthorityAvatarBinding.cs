// =============================================================================
// SCRIPT: CCS_SurvivalAuthorityAvatarBinding
// CATEGORY: Survival / Runtime / Character / Avatar
// PURPOSE: Readonly relationship between a survival authority ID and avatar ID for future binding/spawn planning.
// PLACEMENT: Value type passed between future authority and avatar systems. No spawn or save IO.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: BindingId is optional stable key for future save/network correlation of authority-avatar pairs.
// =============================================================================

namespace CCS.Project
{
    public readonly struct CCS_SurvivalAuthorityAvatarBinding
    {
        #region Public Methods

        public CCS_SurvivalAuthorityAvatarBinding(string authorityId, string avatarId, string bindingId)
        {
            AuthorityId = authorityId ?? string.Empty;
            AvatarId = avatarId ?? string.Empty;
            BindingId = bindingId ?? string.Empty;
        }

        public CCS_SurvivalValidationResult ValidateBinding()
        {
            CCS_SurvivalValidationResult authorityValidation = CCS_SurvivalIdentityUtility.ValidateAuthorityId(AuthorityId);
            if (!authorityValidation.IsSuccess)
            {
                return authorityValidation;
            }

            CCS_SurvivalValidationResult avatarValidation = CCS_SurvivalIdentityUtility.ValidateAvatarId(AvatarId);
            if (!avatarValidation.IsSuccess)
            {
                return avatarValidation;
            }

            if (!string.IsNullOrWhiteSpace(BindingId))
            {
                CCS_SurvivalValidationResult bindingIdValidation = CCS_SurvivalIdentityUtility.ValidateBindingId(BindingId);
                if (!bindingIdValidation.IsSuccess)
                {
                    return bindingIdValidation;
                }
            }

            return CCS_SurvivalValidationResult.Pass();
        }

        #endregion

        #region Properties

        public string AuthorityId { get; }

        public string AvatarId { get; }

        public string BindingId { get; }

        #endregion
    }
}
