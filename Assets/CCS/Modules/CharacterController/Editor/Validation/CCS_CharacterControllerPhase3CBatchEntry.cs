using CCS.Project;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CharacterControllerPhase3CBatchEntry
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Batch-mode entry for v0.7.4 Phase 3C animation rebuild architecture validation.
// PLACEMENT: Editor batch utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_CharacterControllerPhase3CBatchEntry
    {
        public static void RunFromBatchMode()
        {
            CCS_SurvivalValidationResult validationResult =
                CCS_CharacterControllerPhase3CValidationUtility.ValidatePhase3CAnimationRebuildArchitecture();
            if (!validationResult.IsSuccess)
            {
                Debug.LogError("[Phase 3C Architecture Batch] Validation failed: " + validationResult.Message);
                EditorApplication.Exit(1);
                return;
            }

            string reportPath = CCS_AnimationRebuildArchitectureReportBuilder.WriteReport();
            Debug.Log(
                "[Phase 3C Architecture Batch] Validation passed. Report: "
                + reportPath
                + ". "
                + validationResult.Message);
            EditorApplication.Exit(0);
        }
    }
}
