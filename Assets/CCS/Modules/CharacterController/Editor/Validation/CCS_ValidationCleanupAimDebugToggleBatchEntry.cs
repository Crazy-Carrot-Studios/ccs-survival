using CCS.Modules.Interaction.Editor;
using CCS.Modules.Weapons.Editor;
using CCS.Modules.CharacterController.Diagnostics;
using CCS.Project;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_ValidationCleanupAimDebugToggleBatchEntry
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Batch-mode entry for v0.7.9 validation cleanup and aim debug toggle validation.
// PLACEMENT: Editor batch utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_ValidationCleanupAimDebugToggleBatchEntry
    {
        public static void RunFromBatchMode()
        {
            CCS_CharacterControllerMasterTestBuilder.SetupMasterTestScene();
            CCS_InteractionDetectionTestBuilder.BuildMasterTestInteractions();
            CCS_WeaponsAssetBuilder.EnsureTestDamageTargetPrefab();
            EnsureDiagnosticsRevolverAimSetupPoseDefault();
            AssetDatabase.SaveAssets();

            CCS_SurvivalValidationResult validationResult =
                CCS_ValidationCleanupAimDebugToggleValidationUtility.ValidateValidationCleanupAimDebugToggle();
            if (!validationResult.IsSuccess)
            {
                Debug.LogError("[Validation Cleanup Aim Debug Batch] Validation failed: " + validationResult.Message);
                EditorApplication.Exit(1);
                return;
            }

            string reportPath = CCS_ValidationCleanupAimDebugToggleReportBuilder.WriteReport();
            Debug.Log(
                "[Validation Cleanup Aim Debug Batch] Validation passed. Report: "
                + reportPath
                + ". "
                + validationResult.Message);
            EditorApplication.Exit(0);
        }

        private static void EnsureDiagnosticsRevolverAimSetupPoseDefault()
        {
            CCS_CharacterControllerDiagnosticsManager manager =
                Object.FindAnyObjectByType<CCS_CharacterControllerDiagnosticsManager>();
            if (manager == null)
            {
                return;
            }

            SerializedObject serializedManager = new SerializedObject(manager);
            SerializedProperty setupPoseProperty = serializedManager.FindProperty("forceRevolverAimSetupPose");
            if (setupPoseProperty != null && setupPoseProperty.boolValue)
            {
                setupPoseProperty.boolValue = false;
                serializedManager.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(manager);
                EditorSceneManager.MarkSceneDirty(manager.gameObject.scene);
                EditorSceneManager.SaveScene(manager.gameObject.scene);
            }
        }
    }
}
