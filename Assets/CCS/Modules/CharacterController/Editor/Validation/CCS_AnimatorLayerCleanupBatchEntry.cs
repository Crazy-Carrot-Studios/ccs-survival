using CCS.Project;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_AnimatorLayerCleanupBatchEntry
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Batch-mode entry for v0.7.2 player animator layer cleanup and validation.
// PLACEMENT: Editor batch utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: Ensures Base Layer locomotion-only, RevolverUpperBody aim/strafe, Interaction pickup layer.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_AnimatorLayerCleanupBatchEntry
    {
        public static void RunFromBatchMode()
        {
            CCS_CharacterControllerAnimationIsolationBuilder.EnsurePlayerAnimationIsolation();
            CCS_RevolverAimSimplificationBuilder.EnsureAnimatorLayerCleanupPass();

            CCS_SurvivalValidationResult[] validations =
            {
                CCS_CharacterControllerAnimationValidationUtility.ValidatePlayerAnimatorControllerAnimationIsolation(),
                CCS_CharacterControllerAnimationValidationUtility.ValidateAimLocomotionAnimatorParameters(),
                CCS_CharacterControllerAnimationValidationUtility.ValidateAimStrafeAnimationIsolation(),
                CCS_CharacterControllerAnimationValidationUtility.ValidateRevolverUpperBodyAnimationIsolation(),
                CCS_CharacterControllerAnimationValidationUtility.ValidateInteractionLayerAnimationIsolation(),
                CCS_CharacterControllerAnimationValidationUtility.ValidateRevolverWildWestHardReplaceAimRuntime(),
                CCS_CharacterControllerAnimationValidationUtility.ValidateNoInvectorRuntimeReferences(),
                CCS_CharacterControllerAnimationValidationUtility.ValidateRuntimeAnimatorControllerAgreement(),
            };

            for (int i = 0; i < validations.Length; i++)
            {
                CCS_SurvivalValidationResult result = validations[i];
                if (result.IsSuccess)
                {
                    continue;
                }

                Debug.LogError("[Animator Layer Cleanup Batch] Validation failed: " + result.Message);
                EditorApplication.Exit(1);
                return;
            }

            Debug.Log(
                "[Animator Layer Cleanup Batch] Validation passed: Base Layer locomotion-only, "
                + "RevolverUpperBody aim/strafe, and Interaction pickup layer validated.");
            EditorApplication.Exit(0);
        }
    }
}
