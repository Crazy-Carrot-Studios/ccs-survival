using CCS.Project;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_PlayerVisualModelSwapBatchEntry
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Batch-mode entry for v0.7.6 Kevin player visual swap validation.
// PLACEMENT: Editor batch utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_PlayerVisualModelSwapBatchEntry
    {
        public static void RunFromBatchMode()
        {
            string importAuditPath = CCS_ReallusionCharacterImportAuditReportBuilder.WriteReport();

            CCS_PlayerModelKevinPrefabBuilder.EnsurePlayerModelKevinPrefab();
            CCS_PlayerVisualKevinSwapBuilder.EnsureKevinModelOnNetworkedPlayerPrefab();
            CCS_CharacterControllerPlayerPrefabBuilder.EnsurePlayerPrefabs();

            CCS_SurvivalValidationResult validationResult =
                CCS_PlayerVisualModelSwapValidationUtility.ValidateKevinPlayerVisualSwap();
            if (!validationResult.IsSuccess)
            {
                Debug.LogError("[Player Visual Swap Batch] Validation failed: " + validationResult.Message);
                EditorApplication.Exit(1);
                return;
            }

            string swapReportPath = CCS_PlayerVisualSwapReportBuilder.WriteReport();
            Debug.Log(
                "[Player Visual Swap Batch] Validation passed. Import audit: "
                + importAuditPath
                + " swap report: "
                + swapReportPath
                + ". "
                + validationResult.Message);
            EditorApplication.Exit(0);
        }
    }
}
