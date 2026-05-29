using UnityEditor;

// =============================================================================
// SCRIPT: CCS_SurvivalCoreValidationRegistration
// CATEGORY: Survival / Editor / SurvivalCore / Validation
// PURPOSE: Registers survival core validator with the central validation pipeline at editor load.
// PLACEMENT: Editor assembly only. No runtime behavior.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-28
// NOTES: Uses InitializeOnLoad so menus do not own registration logic.
// =============================================================================

namespace CCS.Survival.Editor.SurvivalCore
{
    [InitializeOnLoad]
    public static class CCS_SurvivalCoreValidationRegistration
    {
        #region Public Methods

        static CCS_SurvivalCoreValidationRegistration()
        {
            Development.CCS_SurvivalValidationPipeline.RegisterValidator(
                new CCS_SurvivalCoreValidationValidator());
        }

        #endregion
    }
}
