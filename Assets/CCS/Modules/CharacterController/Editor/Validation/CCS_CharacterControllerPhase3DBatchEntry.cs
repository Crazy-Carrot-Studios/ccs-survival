using CCS.Project;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CharacterControllerPhase3DBatchEntry
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Batch-mode entry for v0.7.5 Phase 3D player prefab hierarchy architecture validation.
// PLACEMENT: Editor batch utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_CharacterControllerPhase3DBatchEntry
    {
        public static void RunFromBatchMode()
        {
            CCS_SurvivalValidationResult validationResult =
                CCS_CharacterControllerPhase3DValidationUtility.ValidatePhase3DPlayerPrefabHierarchyArchitecture();
            if (!validationResult.IsSuccess)
            {
                Debug.LogError("[Phase 3D Hierarchy Architecture Batch] Validation failed: " + validationResult.Message);
                EditorApplication.Exit(1);
                return;
            }

            string reportPath = CCS_PlayerPrefabHierarchyArchitectureReportBuilder.WriteAllReports();
            Debug.Log(
                "[Phase 3D Hierarchy Architecture Batch] Validation passed. Report: "
                + reportPath
                + ". "
                + validationResult.Message);
            EditorApplication.Exit(0);
        }
    }
}
