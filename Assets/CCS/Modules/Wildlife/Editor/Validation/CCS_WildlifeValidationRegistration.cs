using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_WildlifeValidationRegistration
// CATEGORY: Modules / Wildlife / Editor / Validation
// PURPOSE: Registers wildlife validator with the central validation pipeline.
// PLACEMENT: Editor assembly only. InitializeOnLoad registration.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Permanent validation infrastructure for 0.9.3+.
// =============================================================================

namespace CCS.Modules.Wildlife.Editor
{
    [InitializeOnLoad]
    public static class CCS_WildlifeValidationRegistration
    {
        #region Public Methods

        static CCS_WildlifeValidationRegistration()
        {
            CCS_SurvivalValidationPipeline.RegisterValidator(new CCS_WildlifeValidationValidator());
        }

        #endregion
    }
}
