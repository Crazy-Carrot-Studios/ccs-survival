using CCS.Project;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_AIBanditVisualModelSwapBatchEntry
// CATEGORY: Modules / AI / Editor / Validation
// PURPOSE: Batch-mode entry for v0.7.7 EnemyAI bandit visual swap validation.
// PLACEMENT: Editor batch utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-29
// =============================================================================

namespace CCS.Modules.AI.Editor
{
    public static class CCS_AIBanditVisualModelSwapBatchEntry
    {
        public static void RunFromBatchMode()
        {
            string importAuditPath = CCS_ReallusionEnemyAIImportAuditReportBuilder.WriteReport();

            CCS_AIBanditModelEnemyAIPrefabBuilder.EnsureBanditModelEnemyAIPrefab();
            CCS_AIBanditVisualEnemyAISwapBuilder.EnsureEnemyAiModelOnBanditPrefab();
            CCS_AIBanditPrefabBuilder.EnsureAIBanditPrefab();
            TryDeleteLegacyPlayerVisualIfUnreferenced();

            CCS_SurvivalValidationResult validationResult =
                CCS_AIBanditVisualModelSwapValidationUtility.ValidateEnemyAiBanditVisualSwap();
            if (!validationResult.IsSuccess)
            {
                Debug.LogError("[AI Bandit Visual Swap Batch] Validation failed: " + validationResult.Message);
                EditorApplication.Exit(1);
                return;
            }

            string swapReportPath = CCS_AIBanditVisualSwapReportBuilder.WriteReport();
            Debug.Log(
                "[AI Bandit Visual Swap Batch] Validation passed. Import audit: "
                + importAuditPath
                + " swap report: "
                + swapReportPath
                + ". "
                + validationResult.Message);
            EditorApplication.Exit(0);
        }

        private static void TryDeleteLegacyPlayerVisualIfUnreferenced()
        {
            if (!System.IO.File.Exists(CCS_AIConstants.LegacyPlayerVisualPrefabPath))
            {
                return;
            }

            int referenceCount = CCS.Modules.CharacterController.Editor.CCS_PlayerVisualModelSwapValidationUtility
                .CountProjectReferencesToAsset(CCS_AIConstants.LegacyPlayerVisualPrefabPath);
            if (referenceCount > 0)
            {
                Debug.Log(
                    "[AI Bandit Visual Swap Batch] PF_CCS_Player_Visual retained ("
                    + referenceCount
                    + " references).");
                return;
            }

            if (AssetDatabase.DeleteAsset(CCS_AIConstants.LegacyPlayerVisualPrefabPath))
            {
                Debug.Log("[AI Bandit Visual Swap Batch] Deleted unreferenced PF_CCS_Player_Visual.");
            }
        }
    }
}
