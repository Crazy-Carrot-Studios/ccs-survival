using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_FrontierHuntingValidationRegistration
// CATEGORY: Modules / Wildlife / Editor / Validation
// PURPOSE: Registers frontier hunting validator with the central validation pipeline.
// PLACEMENT: Editor assembly only. InitializeOnLoad registration.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Milestone 1.3.2 frontier hunting foundation validation registration.
// =============================================================================

namespace CCS.Modules.Wildlife.Editor
{
    [InitializeOnLoad]
    public static class CCS_FrontierHuntingValidationRegistration
    {
        #region Public Methods

        static CCS_FrontierHuntingValidationRegistration()
        {
            CCS_SurvivalValidationPipeline.RegisterValidator(new CCS_FrontierHuntingValidationValidator());
        }

        #endregion
    }
}
