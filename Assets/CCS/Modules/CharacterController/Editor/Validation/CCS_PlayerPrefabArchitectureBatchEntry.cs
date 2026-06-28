using CCS.Project;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_PlayerPrefabArchitectureBatchEntry
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Batch-mode entry for v0.8.0 player production prefab architecture validation.
// PLACEMENT: Editor batch utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: Builds architecture, then validates production and test harness prefabs.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_PlayerPrefabArchitectureBatchEntry
    {
        public static void RunFromBatchMode()
        {
            Debug.Log("[Player Prefab Architecture Batch] Ensuring player prefab foundation...");
            CCS_CharacterControllerPlayerPrefabBuilder.EnsurePlayerPrefabs();

            Debug.Log("[Player Prefab Architecture Batch] Ensuring v0.8.0 player prefab architecture...");
            CCS_PlayerPrefabArchitectureBuilder.EnsurePlayerProductionArchitecture();

            Debug.Log("[Player Prefab Architecture Batch] Re-wiring player prefab references...");
            CCS_CharacterControllerPlayerPrefabBuilder.EnsurePlayerPrefabs();

            Debug.Log("[Player Prefab Architecture Batch] Repairing visual contamination and Animator wiring...");
            CCS_PlayerVisualAndAnimatorBindingBuilder.EnsurePlayerVisualAndAnimatorBinding();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            CCS_SurvivalValidationResult result =
                CCS_PlayerPrefabArchitectureValidationUtility.ValidateAllPlayerPrefabArchitecture();

            if (!result.IsSuccess)
            {
                Debug.LogError("[Player Prefab Architecture Batch] Validation failed: " + result.Message);
                EditorApplication.Exit(1);
                return;
            }

            if (result.IsWarning)
            {
                Debug.LogWarning("[Player Prefab Architecture Batch] Validation passed with warnings: " + result.Message);
            }
            else
            {
                Debug.Log("[Player Prefab Architecture Batch] Validation passed: " + result.Message);
            }

            EditorApplication.Exit(0);
        }
    }
}
