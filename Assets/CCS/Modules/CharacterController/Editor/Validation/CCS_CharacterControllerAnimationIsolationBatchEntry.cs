using CCS.Project;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CharacterControllerAnimationIsolationBatchEntry
// CATEGORY: Modules / CharacterController / Editor
// PURPOSE: Batch-mode entry for player animation clip isolation and validation.
// PLACEMENT: Editor batch utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Invoked via Unity -batchmode -executeMethod ...RunFromBatchMode.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_CharacterControllerAnimationIsolationBatchEntry
    {
        public static void RunFromBatchMode()
        {
            CCS_CharacterControllerAnimationIsolationBuilder.EnsurePlayerAnimationIsolation();

            CCS_SurvivalValidationResult result =
                CCS_CharacterControllerAnimationValidationUtility.ValidatePlayerAnimatorControllerAnimationIsolation();
            if (result.IsSuccess)
            {
                Debug.Log("[Animation Isolation Batch] Validation passed: " + result.Message);
                EditorApplication.Exit(0);
                return;
            }

            Debug.LogError("[Animation Isolation Batch] Validation failed: " + result.Message);
            EditorApplication.Exit(1);
        }
    }
}
