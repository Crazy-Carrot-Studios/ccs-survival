using CCS.Modules.Interaction.Editor;
using CCS.Modules.Weapons.Editor;
using CCS.Modules.CharacterController.Diagnostics;
using CCS.Project;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_RevolverHandSocketPreviewBatchEntry
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Batch-mode entry for v0.7.10 revolver hand socket preview validation.
// PLACEMENT: Editor batch utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_RevolverHandSocketPreviewBatchEntry
    {
        public static void RunFromBatchMode()
        {
            CCS_CharacterControllerMasterTestBuilder.SetupMasterTestScene();
            CCS_InteractionDetectionTestBuilder.BuildMasterTestInteractions();
            CCS_WeaponsAssetBuilder.EnsureTestDamageTargetPrefab();
            CCS_PlayerVisualKevinSwapBuilder.EnsureKevinModelOnNetworkedPlayerPrefab();
            EnsureDiagnosticsRevolverDebugDefaults();
            AssetDatabase.SaveAssets();

            string auditReportPath = CCS_RevolverSocketAndIKAuditReportBuilder.WriteReport();

            CCS_SurvivalValidationResult validationResult =
                CCS_RevolverHandSocketPreviewValidationUtility.ValidateRevolverHandSocketPreview();
            if (!validationResult.IsSuccess)
            {
                Debug.LogError("[Revolver Hand Socket Preview Batch] Validation failed: " + validationResult.Message);
                EditorApplication.Exit(1);
                return;
            }

            string reportPath = CCS_RevolverHandSocketPreviewReportBuilder.WriteReport();
            Debug.Log(
                "[Revolver Hand Socket Preview Batch] Validation passed. Audit: "
                + auditReportPath
                + " report: "
                + reportPath
                + ". "
                + validationResult.Message);
            EditorApplication.Exit(0);
        }

        private static void EnsureDiagnosticsRevolverDebugDefaults()
        {
            CCS_CharacterControllerDiagnosticsManager manager =
                Object.FindAnyObjectByType<CCS_CharacterControllerDiagnosticsManager>();
            if (manager == null)
            {
                return;
            }

            SerializedObject serializedManager = new SerializedObject(manager);
            bool changed = false;

            SerializedProperty setupPoseProperty = serializedManager.FindProperty("forceRevolverAimSetupPose");
            if (setupPoseProperty != null && setupPoseProperty.boolValue)
            {
                setupPoseProperty.boolValue = false;
                changed = true;
            }

            SerializedProperty handSocketPreviewProperty =
                serializedManager.FindProperty("forceRevolverHandSocketPreview");
            if (handSocketPreviewProperty != null && handSocketPreviewProperty.boolValue)
            {
                handSocketPreviewProperty.boolValue = false;
                changed = true;
            }

            if (!changed)
            {
                return;
            }

            serializedManager.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(manager);
            EditorSceneManager.MarkSceneDirty(manager.gameObject.scene);
            EditorSceneManager.SaveScene(manager.gameObject.scene);
        }
    }
}
