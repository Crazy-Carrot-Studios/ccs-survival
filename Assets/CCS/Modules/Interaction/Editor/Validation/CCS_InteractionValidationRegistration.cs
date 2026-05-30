using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_InteractionValidationRegistration
// CATEGORY: Modules / Interaction / Editor / Validation
// PURPOSE: Registers interaction validator with the central validation pipeline.
// PLACEMENT: Editor assembly only. InitializeOnLoad registration.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Permanent validation infrastructure for 0.3.9+.
// =============================================================================

namespace CCS.Modules.Interaction.Editor
{
    [InitializeOnLoad]
    public static class CCS_InteractionValidationRegistration
    {
        #region Public Methods

        static CCS_InteractionValidationRegistration()
        {
            CCS_SurvivalValidationPipeline.RegisterValidator(
                new CCS_InteractionValidationValidator());
        }

        #endregion
    }
}
