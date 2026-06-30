using CCS.Project;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SingleRevolverAimLayerBatchEntry
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Batch-mode entry for v0.7.8 single revolver aim upper-body layer validation.
// PLACEMENT: Editor batch utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-29
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_SingleRevolverAimLayerBatchEntry
    {
        public static void RunFromBatchMode()
        {
            string assetAuditPath = CCS_RevolverAimLayerAssetAuditReportBuilder.WriteReport();

            if (!CCS_SingleRevolverAimLayerBuilder.EnsureSingleRevolverAimLayer())
            {
                Debug.LogError("[Single Revolver Aim Layer Batch] Builder failed.");
                EditorApplication.Exit(1);
                return;
            }

            CCS_SingleRevolverAimLayerBuilder.EnsureSingleRevolverAimAnimatorOnNetworkedPlayerPrefab();
            CCS_PlayerVisualKevinSwapBuilder.EnsureKevinModelOnNetworkedPlayerPrefab();

            CCS_SurvivalValidationResult validationResult =
                CCS_SingleRevolverAimLayerValidationUtility.ValidateSingleRevolverAimLayer();
            if (!validationResult.IsSuccess)
            {
                Debug.LogError("[Single Revolver Aim Layer Batch] Validation failed: " + validationResult.Message);
                EditorApplication.Exit(1);
                return;
            }

            string reportPath = CCS_SingleRevolverAimLayerReportBuilder.WriteReport();
            Debug.Log(
                "[Single Revolver Aim Layer Batch] Validation passed. Asset audit: "
                + assetAuditPath
                + " report: "
                + reportPath
                + ". "
                + validationResult.Message);
            EditorApplication.Exit(0);
        }
    }
}
