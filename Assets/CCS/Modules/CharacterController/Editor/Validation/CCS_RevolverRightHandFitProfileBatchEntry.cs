using CCS.Modules.Interaction.Editor;
using CCS.Modules.Weapons.Editor;
using CCS.Project;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_RevolverRightHandFitProfileBatchEntry
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Batch-mode entry for v0.7.10b right-hand revolver fit profile validation.
// PLACEMENT: Editor batch utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-30
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_RevolverRightHandFitProfileBatchEntry
    {
        public static void RunFromBatchMode()
        {
            CCS_CharacterControllerMasterTestBuilder.SetupMasterTestScene();
            CCS_InteractionDetectionTestBuilder.BuildMasterTestInteractions();
            CCS_WeaponsAssetBuilder.EnsureTestDamageTargetPrefab();
            CCS_PlayerVisualKevinSwapBuilder.EnsureKevinModelOnNetworkedPlayerPrefab();
            AssetDatabase.SaveAssets();

            string auditReportPath = CCS_RevolverRightHandFitAuditReportBuilder.WriteReport();

            CCS_SurvivalValidationResult validationResult =
                CCS_RevolverRightHandFitProfileValidationUtility.ValidateRevolverRightHandFitProfile();
            if (!validationResult.IsSuccess)
            {
                Debug.LogError("[Revolver Right Hand Fit Profile Batch] Validation failed: " + validationResult.Message);
                EditorApplication.Exit(1);
                return;
            }

            string reportPath = CCS_RevolverRightHandFitProfileReportBuilder.WriteReport();
            Debug.Log(
                "[Revolver Right Hand Fit Profile Batch] Validation passed. Audit: "
                + auditReportPath
                + " report: "
                + reportPath
                + ". "
                + validationResult.Message);
            EditorApplication.Exit(0);
        }
    }
}
