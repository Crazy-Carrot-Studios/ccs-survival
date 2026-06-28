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
        private const string LegacyHostingAmbientAudioObjectName = "CCS_AmbientAudio";

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
            bool changed = RemoveDuplicateAmbientAudioObjects(hostingScene);
            changed |= EnsureCanonicalHostingAmbientAudio(hostingScene);
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

            bool changed = false;
            changed |= DestroyRootObjectIfPresent(
                masterTestScene,
                CCS_ProjectAudioConstants.MasterTestAmbientAudioObjectName);
            changed |= DestroyRootObjectIfPresent(
                masterTestScene,
                CCS_ProjectAudioConstants.HostingAmbientAudioObjectName);
            changed |= DestroyRootObjectIfPresent(masterTestScene, LegacyHostingAmbientAudioObjectName);
            if (changed)
            {
                EditorSceneManager.MarkSceneDirty(masterTestScene);
            }

            return changed;
        }

        private static bool EnsureCanonicalHostingAmbientAudio(Scene hostingScene)
        {
            bool changed = false;
            GameObject ambientObject = FindRootObject(hostingScene, CCS_ProjectAudioConstants.HostingAmbientAudioObjectName);
            GameObject legacyObject = FindRootObject(hostingScene, LegacyHostingAmbientAudioObjectName);
            if (ambientObject == null && legacyObject != null)
            {
                legacyObject.name = CCS_ProjectAudioConstants.HostingAmbientAudioObjectName;
                ambientObject = legacyObject;
                changed = true;
            }

            changed |= CCS_MasterTestRecordingAmbientAudioBuilder.EnsureAmbientAudioObjectInScene(
                hostingScene,
                CCS_ProjectAudioConstants.HostingAmbientAudioObjectName,
                playOnStart: true,
                targetVolume: CCS_ProjectAudioConstants.HostingAmbientPlaylistDefaultVolume);
            return changed;
        }

        private static bool RemoveDuplicateAmbientAudioObjects(Scene scene)
        {
            bool changed = false;
            GameObject[] roots = scene.GetRootGameObjects();
            GameObject canonical = null;
            for (int i = 0; i < roots.Length; i++)
            {
                GameObject root = roots[i];
                if (root == null || !IsAmbientAudioObjectName(root.name))
                {
                    continue;
                }

                if (canonical == null)
                {
                    canonical = root;
                    continue;
                }

                Object.DestroyImmediate(root);
                changed = true;
            }

            return changed;
        }

        private static bool IsAmbientAudioObjectName(string objectName)
        {
            return objectName == CCS_ProjectAudioConstants.HostingAmbientAudioObjectName
                || objectName == LegacyHostingAmbientAudioObjectName;
        }

        private static bool DestroyRootObjectIfPresent(Scene scene, string objectName)
        {
            GameObject ambientObject = FindRootObject(scene, objectName);
            if (ambientObject == null)
            {
                return false;
            }

            Object.DestroyImmediate(ambientObject);
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
