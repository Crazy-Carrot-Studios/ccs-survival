using CCS.Project;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_PlayerPrefabArchitectureMenus
// CATEGORY: Modules / CharacterController / Editor
// PURPOSE: Editor menus for v0.8.0 player production prefab architecture.
// PLACEMENT: Editor-only static registration. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: Production spawn validation uses the runtime production prefab asset only.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_PlayerPrefabArchitectureMenus
    {
        private const string MenuRoot = "CCS/Character Controller/Player Architecture/";

        [MenuItem(MenuRoot + "Build Production + Test Harness Prefabs")]
        public static void BuildProductionArchitectureMenu()
        {
            CCS_CharacterControllerPlayerPrefabBuilder.EnsurePlayerPrefabs();
            CCS_PlayerPrefabArchitectureBuilder.EnsurePlayerProductionArchitecture();
            CCS_CharacterControllerPlayerPrefabBuilder.EnsurePlayerPrefabs();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            LogResult(CCS_PlayerPrefabArchitectureValidationUtility.ValidateAllPlayerPrefabArchitecture());
        }

        [MenuItem(MenuRoot + "Validate Production Prefab Architecture")]
        public static void ValidateProductionArchitectureMenu()
        {
            LogResult(CCS_PlayerPrefabArchitectureValidationUtility.ValidateProductionPlayerPrefabArchitecture());
        }

        [MenuItem(MenuRoot + "Validate Production Player Spawn Readiness")]
        public static void ValidateProductionSpawnReadinessMenu()
        {
            CCS_SurvivalValidationResult architectureResult =
                CCS_PlayerPrefabArchitectureValidationUtility.ValidateProductionPlayerPrefabArchitecture();
            if (!architectureResult.IsSuccess)
            {
                LogResult(architectureResult);
                return;
            }

            GameObject productionPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_PlayerPrefabConstants.ProductionPlayerPrefabPath);
            if (productionPrefab == null)
            {
                Debug.LogError(
                    "[Player Architecture] Missing production prefab at "
                    + CCS_PlayerPrefabConstants.ProductionPlayerPrefabPath);
                return;
            }

            Debug.Log(
                "[Player Architecture] Production prefab spawn readiness validated: "
                + CCS_PlayerPrefabConstants.ProductionPlayerPrefabPath
                + ". Assign to NetworkManager manually for production scene experiments.");
            LogResult(architectureResult);
        }

        private static void LogResult(CCS_SurvivalValidationResult result)
        {
            if (!result.IsSuccess)
            {
                Debug.LogError("[Player Architecture] Failed: " + result.Message);
                return;
            }

            if (result.IsWarning)
            {
                Debug.LogWarning("[Player Architecture] Passed with warnings: " + result.Message);
                return;
            }

            Debug.Log("[Player Architecture] Passed: " + result.Message);
        }
    }
}
