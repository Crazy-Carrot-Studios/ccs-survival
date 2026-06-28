using CCS.Project;
using CCS.Modules.CharacterController.Editor;
using CCS.Project.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_MultiplayerHostingSceneBatchEntry
// CATEGORY: Modules / CharacterController / Tests / Netcode / Editor
// PURPOSE: Batch-mode entry for hosting scene repair, ambient audio, and validation.
// PLACEMENT: Editor batch utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: v0.7.1 hosting scene polish validation entry point.
// =============================================================================

namespace CCS.Modules.CharacterController.Tests.Netcode.Editor
{
    public static class CCS_MultiplayerHostingSceneBatchEntry
    {
        public static void RunFromBatchMode()
        {
            CCS_MultiplayerHostingBuilder.VerifyAndRepairScene();
            CCS_HostingAmbientAudioBuilder.EnsureHostingSceneAmbientAudio();

            Scene masterTestScene = EditorSceneManager.OpenScene(
                CCS_NetcodeTestConstants.MasterTestScenePath,
                OpenSceneMode.Single);
            if (masterTestScene.IsValid())
            {
                CCS_MasterTestRecordingAmbientAudioBuilder.EnsureMasterTestWithoutGameplayAmbience(masterTestScene);
                EditorSceneManager.SaveScene(masterTestScene);
            }

            CCS_SurvivalValidationResult result = CCS_MultiplayerHostingValidator.ValidateHostingScene();
            if (!result.IsSuccess)
            {
                Debug.LogError("[Hosting Scene Batch] Validation failed: " + result.Message);
                EditorApplication.Exit(1);
                return;
            }

            Debug.Log("[Hosting Scene Batch] Hosting scene setup and validation passed.");
            EditorApplication.Exit(0);
        }
    }
}
