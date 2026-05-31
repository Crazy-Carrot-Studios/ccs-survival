using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_StarterLoadoutValidationRegistration
// CATEGORY: Survival / Editor / Development / Validation
// PURPOSE: Registers starter loadout validator with the central validation pipeline.
// PLACEMENT: Editor assembly only. InitializeOnLoad registration.
// AUTHOR: James Schilz
// CREATED: 2026-05-31
// NOTES: Permanent validation infrastructure for 0.9.1+.
// =============================================================================

namespace CCS.Survival.Editor.Development
{
    [InitializeOnLoad]
    public static class CCS_StarterLoadoutValidationRegistration
    {
        #region Public Methods

        static CCS_StarterLoadoutValidationRegistration()
        {
            CCS_SurvivalValidationPipeline.RegisterValidator(
                new CCS_StarterLoadoutValidationValidator());
        }

        #endregion
    }
}
