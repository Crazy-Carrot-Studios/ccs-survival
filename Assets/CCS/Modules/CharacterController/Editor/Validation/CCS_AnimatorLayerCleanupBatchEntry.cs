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
// NOTES: v0.7.3 — clip reconnect, motion validation, and runtime layer-weight verification.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_AnimatorLayerCleanupBatchEntry
    {
        public static void RunFromBatchMode()
        {
            CCS_CharacterControllerAnimationIsolationBuilder.EnsurePlayerAnimationIsolation();
            CCS_RevolverAimSimplificationBuilder.EnsureAnimatorLayerCleanupPass();
            if (!CCS_AnimatorClipReconnectBuilder.EnsurePlayerAnimatorClipReconnect(out System.Collections.Generic.List<string> reconnectErrors))
            {
                for (int errorIndex = 0; errorIndex < reconnectErrors.Count; errorIndex++)
                {
                    Debug.LogError(
                        "[Animator Layer Cleanup Batch] Clip reconnect failed: "
                        + reconnectErrors[errorIndex]);
                }

                EditorApplication.Exit(1);
                return;
            }

            CCS_SurvivalValidationResult[] validations =
            {
                CCS_CharacterControllerAnimationValidationUtility.ValidatePlayerAnimatorControllerAnimationIsolation(),
                CCS_CharacterControllerAnimationValidationUtility.ValidateAimLocomotionAnimatorParameters(),
                CCS_CharacterControllerAnimationValidationUtility.ValidateAimStrafeAnimationIsolation(),
                CCS_CharacterControllerAnimationValidationUtility.ValidateRevolverUpperBodyAnimationIsolation(),
                CCS_CharacterControllerAnimationValidationUtility.ValidateInteractionLayerAnimationIsolation(),
                CCS_CharacterControllerAnimationValidationUtility.ValidateAnimatorMotionPlaybackReadiness(),
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
                "[Animator Layer Cleanup Batch] Validation passed: layer structure, clip reconnect, "
                + "motion assignments, and runtime layer-weight contracts validated.");
            EditorApplication.Exit(0);
        }
    }
}
