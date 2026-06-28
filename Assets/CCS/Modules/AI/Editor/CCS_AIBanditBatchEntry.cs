using CCS.Modules.CharacterController.Editor;
using CCS.Project;
using CCS.Project.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_AIBanditBatchEntry
// CATEGORY: Modules / AI / Editor
// PURPOSE: Batch-mode entry point for AI prefab setup, netcode registration, and validation.
// PLACEMENT: Editor batch utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: Invoked via Unity -batchmode -executeMethod ...RunFromBatchMode.
// =============================================================================

namespace CCS.Modules.AI.Editor
{
    public static class CCS_AIBanditBatchEntry
    {
        public static void RunFromBatchMode()
        {
            CCS_AIBanditPrefabBuilder.EnsureAIBanditPrefab();
            CCS_RevolverAimSimplificationBuilder.EnsureRevolverAimSimplificationPass();
            TryRebuildNetcodePrefabSetupViaReflection();
            CCS_AINavigationMasterTestBuilder.EnsureMasterTestNavigation();
            CCS_AIBanditMasterTestBuilder.EnsureMasterTestBanditSpawner();
            CCS_HostingAmbientAudioBuilder.EnsureHostingSceneAmbientAudio();

            Scene masterTestScene = EditorSceneManager.OpenScene(
                "Assets/CCS/Scenes/CharacterController/SCN_CCS_CharacterController_MasterTest.unity",
                OpenSceneMode.Single);
            if (masterTestScene.IsValid())
            {
                CCS_MasterTestRecordingAmbientAudioBuilder.EnsureMasterTestWithoutGameplayAmbience(masterTestScene);
                EditorSceneManager.SaveScene(masterTestScene);
            }

            CCS_SurvivalValidationResult result = CCS_AIBanditValidationUtility.ValidateMilestoneB13Foundation();
            if (!result.IsSuccess)
            {
                Debug.LogError("[AI Batch] Validation failed: " + result.Message);
                EditorApplication.Exit(1);
                return;
            }

            Debug.Log("[AI Batch] Validation passed: AI prefab, netcode registration, and scene spawner are configured.");
            EditorApplication.Exit(0);
        }

        private static void TryRebuildNetcodePrefabSetupViaReflection()
        {
            System.Type utilityType = System.Type.GetType(
                "CCS.Modules.CharacterController.Tests.Netcode.Editor.CCS_NetcodeNetworkPrefabSetupUtility, CCS.Modules.CharacterController.Tests.Netcode.Editor");
            if (utilityType == null)
            {
                return;
            }

            System.Reflection.MethodInfo method = utilityType.GetMethod(
                "RebuildNetworkPrefabSetup",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (method == null)
            {
                return;
            }

            method.Invoke(null, null);
        }
    }
}
