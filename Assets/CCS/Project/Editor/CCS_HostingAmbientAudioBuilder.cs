using CCS.Modules.CharacterController.Editor;
using CCS.Project;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_HostingAmbientAudioBuilder
// CATEGORY: Project / Editor / Audio
// PURPOSE: Ensures ambient playlist on multiplayer hosting / mode-select scene.
// PLACEMENT: Editor builder invoked from hosting scene setup.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Plays continuously on mode select and hosting; removed from Master Test gameplay.
// =============================================================================

namespace CCS.Project.Editor
{
    public static class CCS_HostingAmbientAudioBuilder
    {
        public static bool EnsureHostingSceneAmbientAudio()
        {
            Scene hostingScene = EditorSceneManager.OpenScene(
                CCS_ProjectAudioConstants.MultiplayerHostingScenePath,
                OpenSceneMode.Single);
            if (!hostingScene.IsValid())
            {
                return false;
            }

            CCS_MasterTestRecordingAmbientAudioBuilder.EnsureAmbienceAssetsReady();
            bool changed = CCS_MasterTestRecordingAmbientAudioBuilder.EnsureAmbientAudioObjectInScene(
                hostingScene,
                CCS_ProjectAudioConstants.HostingAmbientAudioObjectName,
                playOnStart: true);
            if (changed)
            {
                EditorSceneManager.MarkSceneDirty(hostingScene);
                EditorSceneManager.SaveScene(hostingScene);
            }

            return changed;
        }

        public static bool RemoveAmbientAudioFromMasterTest(Scene masterTestScene)
        {
            if (!masterTestScene.IsValid())
            {
                return false;
            }

            GameObject ambientObject = FindRootObject(
                masterTestScene,
                CCS_ProjectAudioConstants.MasterTestAmbientAudioObjectName);
            if (ambientObject == null)
            {
                return false;
            }

            Object.DestroyImmediate(ambientObject);
            EditorSceneManager.MarkSceneDirty(masterTestScene);
            return true;
        }

        private static GameObject FindRootObject(Scene scene, string objectName)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                if (roots[i] != null && roots[i].name == objectName)
                {
                    return roots[i];
                }
            }

            return null;
        }
    }
}
