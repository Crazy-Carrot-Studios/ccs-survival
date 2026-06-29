using CCS.Project;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_CharacterControllerPlayerPrefabAuditBatchEntry
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Batch-mode entry for player prefab component audit (v0.7.1e Phase 2C).
// PLACEMENT: Editor batch utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: Invoked via Unity -batchmode -executeMethod ...RunFromBatchMode.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_CharacterControllerPlayerPrefabAuditBatchEntry
    {
        public static void RunFromBatchMode()
        {
            PrepareMasterTestSceneForAudit();

            PlayerPrefabAuditSummary summary =
                CCS_CharacterControllerPlayerPrefabAuditUtility.RunAuditAndWriteReport(
                    out CCS_SurvivalValidationResult validationResult);

            if (!validationResult.IsSuccess)
            {
                Debug.LogError("[Player Prefab Audit Batch] Validation failed: " + validationResult.Message);
                EditorApplication.Exit(1);
                return;
            }

            Debug.Log(
                "[Player Prefab Audit Batch] Audit completed. Report: "
                + summary.ReportAbsolutePath
                + ". "
                + validationResult.Message);
            EditorApplication.Exit(0);
        }

        private static void PrepareMasterTestSceneForAudit()
        {
            if (!System.IO.File.Exists(CCS_CharacterControllerMasterTestLayoutConstants.MasterTestScenePath))
            {
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(
                CCS_CharacterControllerMasterTestLayoutConstants.MasterTestScenePath,
                OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                return;
            }

            CCS_MasterTestRecordingAmbientAudioBuilder.EnsureMasterTestWithoutGameplayAmbience(scene);
            bool changed = CCS_CharacterControllerPhase2DMigrationUtility.ApplyPhase2DSeparation(scene);
            if (changed)
            {
                EditorSceneManager.SaveScene(scene);
            }
        }
    }
}
