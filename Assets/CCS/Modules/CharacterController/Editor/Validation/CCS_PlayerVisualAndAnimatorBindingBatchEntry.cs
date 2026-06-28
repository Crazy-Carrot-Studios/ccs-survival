using CCS.Project;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_PlayerVisualAndAnimatorBindingBatchEntry
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Batch-mode entry for v0.8.1 player visual recovery and Animator binding repair.
// PLACEMENT: Editor batch utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-28
// NOTES: Repairs prefab visuals/Animator wiring, then validates production and harness prefabs.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_PlayerVisualAndAnimatorBindingBatchEntry
    {
        public static void RunFromBatchMode()
        {
            Debug.Log("[Player Visual/Animator Binding Batch] Ensuring player prefab foundation...");
            CCS_CharacterControllerPlayerPrefabBuilder.EnsurePlayerPrefabs();

            Debug.Log("[Player Visual/Animator Binding Batch] Ensuring player prefab architecture...");
            CCS_PlayerPrefabArchitectureBuilder.EnsurePlayerProductionArchitecture();

            Debug.Log("[Player Visual/Animator Binding Batch] Repairing visual contamination and Animator wiring...");
            CCS_PlayerVisualAndAnimatorBindingBuilder.EnsurePlayerVisualAndAnimatorBinding();

            Debug.Log("[Player Visual/Animator Binding Batch] Re-wiring player prefab references...");
            CCS_CharacterControllerPlayerPrefabBuilder.EnsurePlayerPrefabs();
            CCS_PlayerPrefabArchitectureBuilder.EnsurePlayerProductionArchitecture();
            CCS_PlayerVisualAndAnimatorBindingBuilder.EnsurePlayerVisualAndAnimatorBinding();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            CCS_SurvivalValidationResult result =
                CCS_PlayerVisualAndAnimatorBindingValidationUtility.ValidateAllPlayerVisualAndAnimatorBinding();

            if (!result.IsSuccess)
            {
                Debug.LogError("[Player Visual/Animator Binding Batch] Validation failed: " + result.Message);
                EditorApplication.Exit(1);
                return;
            }

            Debug.Log("[Player Visual/Animator Binding Batch] Validation passed: " + result.Message);
            EditorApplication.Exit(0);
        }
    }
}
