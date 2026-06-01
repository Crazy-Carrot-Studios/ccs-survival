using CCS.Survival.Editor.Development;

// =============================================================================
// SCRIPT: CCS_PlaytestValidationRegistration
// CATEGORY: Modules / Playtesting / Editor / Validation
// PURPOSE: Registers playtesting module validator with the survival validation pipeline.
// PLACEMENT: Loaded automatically at editor startup.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Milestone 1.0.2 manual playtest harness.
// =============================================================================

namespace CCS.Modules.Playtesting.Editor
{
    public static class CCS_PlaytestValidationRegistration
    {
        #region Unity Callbacks

        static CCS_PlaytestValidationRegistration()
        {
            CCS_SurvivalValidationPipeline.RegisterValidator(new CCS_PlaytestValidationValidator());
        }

        #endregion
    }
}
