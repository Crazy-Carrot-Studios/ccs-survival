using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_CharacterControllerValidationRegistration
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Registers character controller validator with the central validation pipeline.
// PLACEMENT: Editor assembly only. InitializeOnLoad registration.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Permanent validation infrastructure for 0.3.8+.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    [InitializeOnLoad]
    public static class CCS_CharacterControllerValidationRegistration
    {
        #region Public Methods

        static CCS_CharacterControllerValidationRegistration()
        {
            CCS_SurvivalValidationPipeline.RegisterValidator(
                new CCS_CharacterControllerValidationValidator());
        }

        #endregion
    }
}
