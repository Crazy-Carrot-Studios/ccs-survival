using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_TimeOfDayValidationRegistration
// CATEGORY: Modules / TimeOfDay / Editor / Validation
// PURPOSE: Registers time-of-day validator with the central validation pipeline.
// PLACEMENT: Editor assembly only. InitializeOnLoad registration.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Permanent validation infrastructure for 0.7.0+.
// =============================================================================

namespace CCS.Modules.TimeOfDay.Editor
{
    [InitializeOnLoad]
    public static class CCS_TimeOfDayValidationRegistration
    {
        #region Public Methods

        static CCS_TimeOfDayValidationRegistration()
        {
            CCS_SurvivalValidationPipeline.RegisterValidator(new CCS_TimeOfDayValidationValidator());
        }

        #endregion
    }
}
