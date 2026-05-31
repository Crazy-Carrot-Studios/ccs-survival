using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_WorldResourceValidationRegistration
// CATEGORY: Modules / WorldResources / Editor / Validation
// PURPOSE: Registers world resource validator with the central validation pipeline.
// PLACEMENT: Editor assembly only. InitializeOnLoad registration.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Permanent validation infrastructure for 0.5.1+.
// =============================================================================

namespace CCS.Modules.WorldResources.Editor
{
    [InitializeOnLoad]
    public static class CCS_WorldResourceValidationRegistration
    {
        #region Public Methods

        static CCS_WorldResourceValidationRegistration()
        {
            CCS_SurvivalValidationPipeline.RegisterValidator(
                new CCS_WorldResourceValidationValidator());
        }

        #endregion
    }
}
