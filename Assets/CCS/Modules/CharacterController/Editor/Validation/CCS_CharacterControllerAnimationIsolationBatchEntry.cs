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
// NOTES: v0.6.4 — validates revolver upper-body isolation and no Invector runtime references.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_CharacterControllerAnimationIsolationBatchEntry
    {
        public static void RunFromBatchMode()
        {
            CCS_CharacterControllerAnimationIsolationBuilder.EnsurePlayerAnimationIsolation();

            CCS_SurvivalValidationResult[] validations =
            {
                CCS_CharacterControllerAnimationValidationUtility.ValidatePlayerAnimatorControllerAnimationIsolation(),
                CCS_CharacterControllerAnimationValidationUtility.ValidateAimLocomotionAnimatorParameters(),
                CCS_CharacterControllerAnimationValidationUtility.ValidateAimStrafeAnimationIsolation(),
                CCS_CharacterControllerAnimationValidationUtility.ValidateRevolverUpperBodyAnimationIsolation(),
                CCS_CharacterControllerAnimationValidationUtility.ValidateRevolverWildWestHardReplaceAimRuntime(),
                CCS_CharacterControllerAnimationValidationUtility.ValidateNoInvectorRuntimeReferences(),
                CCS_CharacterControllerAnimationValidationUtility.ValidateFullDrawClipPreservedAfterBuilder()
            };

            for (int i = 0; i < validations.Length; i++)
            {
                CCS_SurvivalValidationResult result = validations[i];
                if (result.IsSuccess)
                {
                    continue;
                }

                Debug.LogError("[Animation Isolation Batch] Validation failed: " + result.Message);
                EditorApplication.Exit(1);
                return;
            }

            Debug.Log("[Animation Isolation Batch] Validation passed: player animation isolation, aim strafe, revolver upper-body wiring, and FullDraw clip preservation validated.");
            EditorApplication.Exit(0);
        }
    }
}
