using CCS.Project;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CharacterControllerValidationRegistration
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Registers editor menu entry for character controller validation.
// PLACEMENT: Editor-only static registration. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Menu path under CCS/Modules/Character Controller/.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_CharacterControllerValidationRegistration
    {
        #region Public Methods

        [MenuItem("CCS/Modules/Character Controller/Validate")]
        public static void ValidateCharacterControllerMenu()
        {
            CCS_SurvivalValidationResult result = CCS_CharacterControllerValidationValidator.ValidateAll();
            if (result.IsSuccess)
            {
                string prefix = result.IsWarning ? "Warning" : "Passed";
                Debug.Log($"[Character Controller Validation] {prefix}: {result.Message}");
            }
            else
            {
                Debug.LogError($"[Character Controller Validation] Failed: {result.Message}");
            }
        }

        #endregion
    }
}
