using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_InteractionValidationUtility
// CATEGORY: Modules / Interaction / Runtime / Validation
// PURPOSE: Runtime-safe validation for interaction profiles and tuning values.
// PLACEMENT: Used by editor validators and future bootstrap checks.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Returns CCS_SurvivalValidationResult. No UnityEditor references.
// =============================================================================

namespace CCS.Modules.Interaction
{
    public static class CCS_InteractionValidationUtility
    {
        #region Public Methods

        public static CCS_SurvivalValidationResult ValidateProfile(CCS_InteractionProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail("Interaction profile is null.");
            }

            CCS_SurvivalValidationResult baseValidation = CCS_SurvivalProfileValidationUtility.ValidateProfile(profile);
            if (!baseValidation.IsSuccess)
            {
                return baseValidation;
            }

            if (profile.InteractionDistance <= 0f)
            {
                return CCS_SurvivalValidationResult.Fail("Interaction distance must be greater than zero.");
            }

            if (profile.InteractionLayers.value == 0)
            {
                return CCS_SurvivalValidationResult.Warn("Interaction layer mask is empty; scanner will never hit colliders.");
            }

            return CCS_SurvivalValidationResult.Pass("Interaction profile validated.");
        }

        #endregion
    }
}
