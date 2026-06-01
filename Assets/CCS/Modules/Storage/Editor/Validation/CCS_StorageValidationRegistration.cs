using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_StorageValidationRegistration
// CATEGORY: Modules / Storage / Editor / Validation
// PURPOSE: Registers storage validator with the central validation pipeline.
// PLACEMENT: Editor assembly only. InitializeOnLoad registration.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Milestone 1.1.2 storage container foundation.
// =============================================================================

namespace CCS.Modules.Storage.Editor
{
    [InitializeOnLoad]
    public static class CCS_StorageValidationRegistration
    {
        #region Public Methods

        static CCS_StorageValidationRegistration()
        {
            CCS_SurvivalValidationPipeline.RegisterValidator(new CCS_StorageValidationValidator());
        }

        #endregion
    }
}
