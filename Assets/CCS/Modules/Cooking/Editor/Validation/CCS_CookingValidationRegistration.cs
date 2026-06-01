using CCS.Survival.Editor.Development;

// =============================================================================
// SCRIPT: CCS_CookingValidationRegistration
// CATEGORY: Modules / Cooking / Editor / Validation
// PURPOSE: Registers cooking validator on the central survival validation pipeline.
// PLACEMENT: Auto-runs at editor load through InitializeOnLoad.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: No manual registration required outside this file.
// =============================================================================

namespace CCS.Modules.Cooking.Editor
{
    public static class CCS_CookingValidationRegistration
    {
        #region Unity Callbacks

        static CCS_CookingValidationRegistration()
        {
            CCS_SurvivalValidationPipeline.RegisterValidator(new CCS_CookingValidationValidator());
        }

        #endregion
    }
}
