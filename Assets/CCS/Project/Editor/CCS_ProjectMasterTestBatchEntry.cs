using CCS.Modules.CharacterController.Editor;
using CCS.Project;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_ProjectMasterTestBatchEntry
// CATEGORY: Project / Editor
// PURPOSE: Batch-mode Master Test setup, validation, and full project audit.
// PLACEMENT: Editor batch utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Invoked via Unity -batchmode -executeMethod ...RunFromBatchMode.
// =============================================================================

namespace CCS.Project.Editor
{
    public static class CCS_ProjectMasterTestBatchEntry
    {
        public static void RunFromBatchMode()
        {
            CCS_CharacterControllerMasterTestBuilder.SetupMasterTestScene();

            CCS_CharacterCameraRigInputBuilder.ApplyValidationBaselineObstacleAvoidance(false);
            CCS_SurvivalValidationResult baselineResult =
                CCS_CharacterControllerMasterTestValidator.ValidateMasterTestCameraBaseline();
            if (!baselineResult.IsSuccess)
            {
                Debug.LogError("[Validation Baseline] Failed: " + baselineResult.Message);
                EditorApplication.Exit(1);
                return;
            }

            Debug.Log("[Validation Baseline] Passed: " + baselineResult.Message);

            CCS_CharacterCameraRigInputBuilder.RefreshCameraRigObstacleSettingsFromProfiles();
            CCS_SurvivalValidationResult masterTestResult =
                CCS_CharacterControllerMasterTestValidator.ValidateMasterTestScene();
            if (!masterTestResult.IsSuccess)
            {
                Debug.LogError("[Validation] Failed: " + masterTestResult.Message);
                EditorApplication.Exit(1);
                return;
            }

            Debug.Log("[Validation] Passed: " + masterTestResult.Message);

            CCS_SurvivalValidationResult auditResult =
                CCS_ProjectAuditValidator.RunProjectAudit(includeModuleValidators: true);
            if (!auditResult.IsSuccess)
            {
                Debug.LogError("[Project Audit] Failed: " + auditResult.Message);
                EditorApplication.Exit(1);
                return;
            }

            Debug.Log("[Project Audit] Passed: " + auditResult.Message);

            CCS_SurvivalValidationResult animationInventoryResult =
                CCS_AnimationInventoryReporter.GenerateWildWestInventory();
            if (!animationInventoryResult.IsSuccess)
            {
                Debug.LogError("[Animation Inventory] Failed: " + animationInventoryResult.Message);
                EditorApplication.Exit(1);
                return;
            }

            Debug.Log("[Animation Inventory] " + animationInventoryResult.Message);

            CCS_SurvivalValidationResult animationReportValidation =
                CCS_AnimationInventoryReporter.ValidateReportsExist();
            if (!animationReportValidation.IsSuccess)
            {
                Debug.LogError("[Animation Inventory] Failed: " + animationReportValidation.Message);
                EditorApplication.Exit(1);
                return;
            }

            Debug.Log("[Master Test Batch] Setup, validation, and project audit completed successfully.");
            EditorApplication.Exit(0);
        }
    }
}
