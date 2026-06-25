using CCS.Modules.CharacterController.Editor.AnimationFitStudio;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_RevolverFullDrawNudgeBatchEntry
// CATEGORY: Modules / CharacterController / Editor
// PURPOSE: Batch-mode entry for default FullDraw Humanoid muscle nudge.
// PLACEMENT: Editor batch utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Invoked via Unity -batchmode -executeMethod ...RunFromBatchMode.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_RevolverFullDrawNudgeBatchEntry
    {
        public static void RunFromBatchMode()
        {
            if (!CCS_RevolverFullDrawHumanoidPoseNudgeUtility.ApplyDefaultRightArmAimingNudge(
                    out CCS_RevolverFullDrawHumanoidNudgeResult result))
            {
                Debug.LogError("[Revolver FullDraw Nudge Batch] Failed: " + result.ErrorMessage);
                EditorApplication.Exit(1);
                return;
            }

            Debug.Log("[Revolver FullDraw Nudge Batch] Applied default right-arm aiming nudge successfully.");
            EditorApplication.Exit(0);
        }
    }
}
