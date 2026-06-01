using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_SleepValidationRegistration
// CATEGORY: Modules / Sleep / Editor / Validation
// PURPOSE: Registers sleep validator on the central survival validation pipeline.
// PLACEMENT: Auto-runs at editor load through InitializeOnLoad.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: No manual registration required outside this file.
// =============================================================================

namespace CCS.Modules.Sleep.Editor
{
    [InitializeOnLoad]
    public static class CCS_SleepValidationRegistration
    {
        #region Unity Callbacks

        static CCS_SleepValidationRegistration()
        {
            CCS_SurvivalValidationPipeline.RegisterValidator(new CCS_SleepValidationValidator());
        }

        #endregion
    }
}
