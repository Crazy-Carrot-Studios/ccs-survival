using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_SurvivalCoreValidationRegistration
// CATEGORY: Modules / SurvivalCore / Editor / Validation
// PURPOSE: Registers survival core validator with the central validation pipeline at editor load.
// PLACEMENT: Editor assembly only. No runtime behavior.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-28
// NOTES: Uses InitializeOnLoad so menus do not own registration logic.
// =============================================================================

namespace CCS.Modules.SurvivalCore.Editor
{
    [InitializeOnLoad]
    public static class CCS_SurvivalCoreValidationRegistration
    {
        #region Public Methods

        static CCS_SurvivalCoreValidationRegistration()
        {
            CCS_SurvivalValidationPipeline.RegisterValidator(
                new CCS_SurvivalCoreValidationValidator());
        }

        #endregion
    }
}
