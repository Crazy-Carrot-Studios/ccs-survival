using CCS.Survival.Editor.Development;

// =============================================================================
// SCRIPT: CCS_SaveValidationRegistration
// CATEGORY: Modules / SaveSystem / Editor / Validation
// PURPOSE: Registers save system validator on the central survival validation pipeline.
// PLACEMENT: Auto-runs at editor load through InitializeOnLoad.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: No manual registration required outside this file.
// =============================================================================

namespace CCS.Modules.SaveSystem.Editor
{
    public static class CCS_SaveValidationRegistration
    {
        #region Unity Callbacks

        static CCS_SaveValidationRegistration()
        {
            CCS_SurvivalValidationPipeline.RegisterValidator(new CCS_SaveValidationValidator());
        }

        #endregion
    }
}
