using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_UIValidationRegistration
// CATEGORY: Modules / UI / Editor / Validation
// PURPOSE: Registers UI validator with the central validation pipeline.
// PLACEMENT: Editor assembly only. InitializeOnLoad registration.
// AUTHOR: James Schilz
// CREATED: 2026-05-30
// NOTES: Permanent validation infrastructure for 0.4.2+.
// =============================================================================

namespace CCS.Modules.UI.Editor
{
    [InitializeOnLoad]
    public static class CCS_UIValidationRegistration
    {
        #region Public Methods

        static CCS_UIValidationRegistration()
        {
            CCS_SurvivalValidationPipeline.RegisterValidator(new CCS_UIValidationValidator());
        }

        #endregion
    }
}
