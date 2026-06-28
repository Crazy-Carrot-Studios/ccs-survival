using System.Collections.Generic;
using CCS.Project;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_PlayerHumanoidAnimationBindingBatchEntry
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Batch-mode entry for v0.8.1b humanoid clip binding repair and validation.
// PLACEMENT: Editor batch utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-28
// NOTES: Repairs hybrid clips, validates avatar/mesh binding, then runs visual binding checks.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_PlayerHumanoidAnimationBindingBatchEntry
    {
        public static void RunFromBatchMode()
        {
            Debug.Log("[Player Humanoid Animation Binding Batch] Ensuring player prefab foundation...");
            CCS_CharacterControllerPlayerPrefabBuilder.EnsurePlayerPrefabs();
            CCS_PlayerPrefabArchitectureBuilder.EnsurePlayerProductionArchitecture();
            CCS_PlayerVisualAndAnimatorBindingBuilder.EnsurePlayerVisualAndAnimatorBinding();

            Debug.Log("[Player Humanoid Animation Binding Batch] Repairing humanoid clip bindings...");
            CCS_PlayerHumanoidAnimationClipRepairBuilder.EnsurePlayerHumanoidAnimationClipRepair(
                out List<string> repairSummaries);
            for (int summaryIndex = 0; summaryIndex < repairSummaries.Count; summaryIndex++)
            {
                Debug.Log("[Player Humanoid Animation Binding Batch] " + repairSummaries[summaryIndex]);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            CCS_SurvivalValidationResult result =
                CCS_PlayerHumanoidAnimationClipValidationUtility.ValidateAllPlayerHumanoidAnimationBinding();
            if (!result.IsSuccess)
            {
                Debug.LogError("[Player Humanoid Animation Binding Batch] Validation failed: " + result.Message);
                EditorApplication.Exit(1);
                return;
            }

            Debug.Log("[Player Humanoid Animation Binding Batch] Validation passed: " + result.Message);
            EditorApplication.Exit(0);
        }
    }
}
