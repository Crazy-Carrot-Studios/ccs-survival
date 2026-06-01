using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_CombatValidationRegistration
// CATEGORY: Modules / Combat / Editor / Validation
// PURPOSE: Registers combat validator on the central survival validation pipeline.
// PLACEMENT: Auto-runs at editor load through InitializeOnLoad.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: No manual registration required outside this file.
// =============================================================================

namespace CCS.Modules.Combat.Editor
{
    [InitializeOnLoad]
    public static class CCS_CombatValidationRegistration
    {
        #region Unity Callbacks

        static CCS_CombatValidationRegistration()
        {
            CCS_SurvivalValidationPipeline.RegisterValidator(new CCS_CombatValidationValidator());
        }

        #endregion
    }
}
