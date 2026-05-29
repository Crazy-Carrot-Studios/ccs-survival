// =============================================================================
// SCRIPT: CCS_SurvivalValidationUtility
// CATEGORY: Survival / Editor / Development / Validation
// PURPOSE: Facade for survival editor validation through the central pipeline.
// PLACEMENT: Called by tools and tests. Menus should prefer CCS_SurvivalValidationPipeline directly.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-28
// NOTES: Do not add checks here. Register CCS_ISurvivalValidationValidator implementations instead.
// =============================================================================

namespace CCS.Survival.Editor.Development
{
    public static class CCS_SurvivalValidationUtility
    {
        #region Public Methods

        public static CCS_SurvivalValidationReport RunDevelopmentValidation()
        {
            return CCS_SurvivalValidationPipeline.RunAll();
        }

        public static void RegisterValidator(CCS_ISurvivalValidationValidator validator)
        {
            CCS_SurvivalValidationPipeline.RegisterValidator(validator);
        }

        public static void UnregisterValidator(string validatorId)
        {
            CCS_SurvivalValidationPipeline.UnregisterValidator(validatorId);
        }

        #endregion
    }
}
