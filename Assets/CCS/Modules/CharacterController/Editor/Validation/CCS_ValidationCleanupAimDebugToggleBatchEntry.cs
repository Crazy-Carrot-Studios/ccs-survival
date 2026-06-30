using CCS.Modules.Interaction.Editor;
using CCS.Modules.Weapons.Editor;
using CCS.Project;
using UnityEditor;
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
    }
}
