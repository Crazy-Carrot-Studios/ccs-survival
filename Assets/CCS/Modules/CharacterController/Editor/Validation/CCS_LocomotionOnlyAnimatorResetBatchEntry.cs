using CCS.Project;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_LocomotionOnlyAnimatorResetBatchEntry
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Batch-mode entry for v0.7.3 Phase 3B locomotion-only animator reset.
// PLACEMENT: Editor batch utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_LocomotionOnlyAnimatorResetBatchEntry
    {
        public static void RunFromBatchMode()
        {
            LocomotionOnlyAnimatorResetResult result =
                CCS_LocomotionOnlyAnimatorResetBuilder.ApplyLocomotionOnlyAnimatorReset(writeBeforeReport: true);

            CCS_SurvivalValidationResult validationResult =
                CCS_CharacterControllerPhase3BValidationUtility.ValidatePhase3BLocomotionOnlyAnimatorReset();
            if (!validationResult.IsSuccess)
            {
                Debug.LogError("[Locomotion Animator Reset Batch] Validation failed: " + validationResult.Message);
                EditorApplication.Exit(1);
                return;
            }

            Debug.Log(
                "[Locomotion Animator Reset Batch] Reset complete. ControllerChanged="
                + result.ControllerChanged
                + " PrefabChanged="
                + result.PrefabChanged
                + " PlayerRootMonoBehaviours="
                + result.PlayerRootMonoBehaviourCount
                + ". "
                + validationResult.Message);
            EditorApplication.Exit(0);
        }
    }
}
