using System.IO;
using CCS.Modules.CharacterController.Tests;
using CCS.Modules.Weapons;
using CCS.Project;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_MasterTestRecordingAmbientAudioBuilder
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Ensures Master Test recording ambience and testing manager scene wiring.
// PLACEMENT: Editor builder invoked from Master Test scene setup.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Master Test recording ambience assets only. Gameplay music lives on hosting scene.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_MasterTestRecordingAmbientAudioBuilder
    {
        #region Public Methods

        public static bool EnsureMasterTestRecordingAmbience(Scene masterTestScene)
        {
            return EnsureMasterTestWithoutGameplayAmbience(masterTestScene);
        }

        public static bool EnsureMasterTestWithoutGameplayAmbience(Scene masterTestScene)
        {
            masterTestScene = EnsureMasterTestSceneLoadedAndActive(masterTestScene);
            if (!masterTestScene.IsValid())
            {
                return false;
            }

            bool changed = false;
            changed |= RemoveAmbientAudioFromScene(masterTestScene);
            changed |= EnsureTestingManagerObject(masterTestScene, includeAmbienceReference: false);
            if (changed)
            {
                EditorSceneManager.MarkSceneDirty(masterTestScene);
            }

            return changed;
        }

        public static void EnsureAmbienceAssetsReady()
        {
            EnsureAmbienceFolderExists();
            EnsureAmbienceClipImportSettings();
        }

        public static bool EnsureAmbientAudioObjectInScene(
            Scene targetScene,
            string objectName,
            bool playOnStart)
        {
            return EnsureAmbientAudioObject(
                targetScene,
                objectName,
                playOnStart,
                CCS_ProjectAudioConstants.HostingAmbientPlaylistDefaultVolume);
        }

        public static bool EnsureAmbientAudioObjectInScene(
            Scene targetScene,
            string objectName,
            bool playOnStart,
            float targetVolume)
        {
            return EnsureAmbientAudioObject(targetScene, objectName, playOnStart, targetVolume);
        }

        private static bool RemoveAmbientAudioFromScene(Scene scene)
        {
            GameObject ambientObject = FindRootObject(scene, CCS_ProjectAudioConstants.MasterTestAmbientAudioObjectName);
            if (ambientObject == null)
            {
                return false;
            }

            Object.DestroyImmediate(ambientObject);
            return true;
        }

        #endregion

        #region Private Methods

        private static Scene EnsureMasterTestSceneLoadedAndActive(Scene masterTestScene)
        {
            if (!masterTestScene.IsValid() || !masterTestScene.isLoaded)
            {
                masterTestScene = EditorSceneManager.OpenScene(
                    CCS_CharacterControllerMasterTestLayoutConstants.MasterTestScenePath,
                    OpenSceneMode.Single);
            }

            if (!masterTestScene.IsValid())
            {
                Debug.LogError(
                    "[Master Test Ambient Audio] Could not open "
                    + CCS_CharacterControllerMasterTestLayoutConstants.MasterTestScenePath);
                return default;
            }

            if (SceneManager.GetActiveScene() != masterTestScene)
            {
                EditorSceneManager.SetActiveScene(masterTestScene);
            }

            return masterTestScene;
        }

        private static void EnsureAmbienceFolderExists()
        {
            if (!AssetDatabase.IsValidFolder("Assets/CCS/Project/Audio"))
            {
                AssetDatabase.CreateFolder("Assets/CCS/Project", "Audio");
            }

            if (!AssetDatabase.IsValidFolder(CCS_ProjectAudioConstants.AmbienceRootPath))
            {
                AssetDatabase.CreateFolder("Assets/CCS/Project/Audio", "Ambience");
            }
        }

        private static void EnsureAmbienceClipImportSettings()
        {
            ApplyAmbienceImportSettings(CCS_ProjectAudioConstants.WesternGame2ClipPath);
            ApplyAmbienceImportSettings(CCS_ProjectAudioConstants.WesternTheme7ClipPath);
        }

        private static void ApplyAmbienceImportSettings(string assetPath)
        {
            if (!File.Exists(assetPath))
            {
                Debug.LogWarning(
                    "[Master Test Ambient Audio] Missing ambience clip at "
                    + assetPath
                    + ". Copy recording clips into "
                    + CCS_ProjectAudioConstants.AmbienceRootPath
                    + " before Play Mode.");
                return;
            }

            AudioImporter importer = AssetImporter.GetAtPath(assetPath) as AudioImporter;
            if (importer == null)
            {
                return;
            }

            AudioImporterSampleSettings settings = importer.defaultSampleSettings;
            bool changed = false;
            if (settings.loadType != AudioClipLoadType.Streaming)
            {
                settings.loadType = AudioClipLoadType.Streaming;
                changed = true;
            }

            if (settings.compressionFormat != AudioCompressionFormat.Vorbis)
            {
                settings.compressionFormat = AudioCompressionFormat.Vorbis;
                changed = true;
            }

            if (settings.quality < 0.55f || settings.quality > 0.70f)
            {
                settings.quality = 0.65f;
                changed = true;
            }

            if (settings.preloadAudioData)
            {
                settings.preloadAudioData = false;
                changed = true;
            }

            if (importer.forceToMono)
            {
                importer.forceToMono = false;
                changed = true;
            }

            if (!importer.loadInBackground)
            {
                importer.loadInBackground = true;
                changed = true;
            }

            if (changed)
            {
                importer.defaultSampleSettings = settings;
                importer.SaveAndReimport();
            }
        }

        private static bool EnsureAmbientAudioObject(Scene targetScene, string objectName, bool playOnStart, float targetVolume)
        {
            bool changed = false;
            GameObject ambientObject = FindRootObject(targetScene, objectName);
            if (ambientObject == null)
            {
                ambientObject = new GameObject(objectName);
                if (ambientObject.scene != targetScene)
                {
                    SceneManager.MoveGameObjectToScene(ambientObject, targetScene);
                }

                changed = true;
            }

            AudioSource audioSource = ambientObject.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = ambientObject.AddComponent<AudioSource>();
                changed = true;
            }

            if (audioSource.playOnAwake)
            {
                audioSource.playOnAwake = false;
                changed = true;
            }

            if (audioSource.loop)
            {
                audioSource.loop = false;
                changed = true;
            }

            if (audioSource.mute)
            {
                audioSource.mute = false;
                changed = true;
            }

            if (Mathf.Approximately(audioSource.volume, 0f))
            {
                audioSource.volume = targetVolume;
                changed = true;
            }

            if (audioSource.spatialBlend > 0.001f)
            {
                audioSource.spatialBlend = 0f;
                changed = true;
            }

            if (audioSource.priority != 128)
            {
                audioSource.priority = 128;
                changed = true;
            }

            if (audioSource.dopplerLevel > 0.001f)
            {
                audioSource.dopplerLevel = 0f;
                changed = true;
            }

            if (audioSource.reverbZoneMix > 0.001f)
            {
                audioSource.reverbZoneMix = 0f;
                changed = true;
            }

            CCS_AmbientAudioPlaylist playlist = ambientObject.GetComponent<CCS_AmbientAudioPlaylist>();
            if (playlist == null)
            {
                playlist = ambientObject.AddComponent<CCS_AmbientAudioPlaylist>();
                changed = true;
            }

            AudioClip gameClip = AssetDatabase.LoadAssetAtPath<AudioClip>(
                CCS_ProjectAudioConstants.WesternGame2ClipPath);
            AudioClip themeClip = AssetDatabase.LoadAssetAtPath<AudioClip>(
                CCS_ProjectAudioConstants.WesternTheme7ClipPath);
            AudioClip[] expectedPlaylist = { gameClip, themeClip };

            SerializedObject serializedPlaylist = new SerializedObject(playlist);
            SerializedProperty playlistProperty = serializedPlaylist.FindProperty("playlist");
            SerializedProperty volumeProperty = serializedPlaylist.FindProperty("volume");
            SerializedProperty playOnStartProperty = serializedPlaylist.FindProperty("playOnStart");
            SerializedProperty repeatProperty = serializedPlaylist.FindProperty("repeatPlaylist");
            SerializedProperty playlistEnabledProperty = serializedPlaylist.FindProperty("playlistEnabled");
            SerializedProperty debugProperty = serializedPlaylist.FindProperty("debugPlaylist");

            if (playlistProperty != null && !SerializedArrayMatches(playlistProperty, expectedPlaylist))
            {
                playlistProperty.arraySize = expectedPlaylist.Length;
                for (int i = 0; i < expectedPlaylist.Length; i++)
                {
                    playlistProperty.GetArrayElementAtIndex(i).objectReferenceValue = expectedPlaylist[i];
                }

                changed = true;
            }

            if (volumeProperty != null && !Mathf.Approximately(volumeProperty.floatValue, targetVolume))
            {
                volumeProperty.floatValue = targetVolume;
                changed = true;
            }

            if (playOnStartProperty != null && playOnStartProperty.boolValue != playOnStart)
            {
                playOnStartProperty.boolValue = playOnStart;
                changed = true;
            }

            if (repeatProperty != null && !repeatProperty.boolValue)
            {
                repeatProperty.boolValue = true;
                changed = true;
            }

            if (playlistEnabledProperty != null && !playlistEnabledProperty.boolValue)
            {
                playlistEnabledProperty.boolValue = true;
                changed = true;
            }

            if (debugProperty != null && debugProperty.boolValue)
            {
                debugProperty.boolValue = false;
                changed = true;
            }

            serializedPlaylist.ApplyModifiedPropertiesWithoutUndo();
            return changed;
        }

        private static bool EnsureTestingManagerObject(Scene masterTestScene, bool includeAmbienceReference)
        {
            bool changed = false;
            GameObject testingManagerObject = FindRootObject(
                masterTestScene,
                CCS_ProjectAudioConstants.MasterTestTestingManagerObjectName);
            if (testingManagerObject == null)
            {
                testingManagerObject = new GameObject(CCS_ProjectAudioConstants.MasterTestTestingManagerObjectName);
                if (testingManagerObject.scene != masterTestScene)
                {
                    SceneManager.MoveGameObjectToScene(testingManagerObject, masterTestScene);
                }

                changed = true;
            }

            CCS_MasterTestSceneTestingManager testingManager =
                testingManagerObject.GetComponent<CCS_MasterTestSceneTestingManager>();
            if (testingManager == null)
            {
                testingManager = testingManagerObject.AddComponent<CCS_MasterTestSceneTestingManager>();
                changed = true;
            }

            CCS_AmbientAudioPlaylist ambientPlaylist = includeAmbienceReference
                ? FindRootObject(
                    masterTestScene,
                    CCS_ProjectAudioConstants.MasterTestAmbientAudioObjectName)
                    ?.GetComponent<CCS_AmbientAudioPlaylist>()
                : null;

            SerializedObject serializedManager = new SerializedObject(testingManager);
            SerializedProperty ambienceEnabledProperty = serializedManager.FindProperty("enableRecordingAmbience");
            SerializedProperty playlistProperty = serializedManager.FindProperty("ambientAudioPlaylist");
            SerializedProperty applyOnStartProperty = serializedManager.FindProperty("applyOnStart");
            SerializedProperty applyInEditorProperty = serializedManager.FindProperty("applyInEditorWhenChanged");
            SerializedProperty debugProperty = serializedManager.FindProperty("debugTestingManager");
            SerializedProperty armIkProperty = serializedManager.FindProperty("enableArmToReticleIK");
            SerializedProperty convergenceProperty = serializedManager.FindProperty("enableVisualAimConvergence");
            SerializedProperty reticleModeProperty = serializedManager.FindProperty("reticleMode");
            SerializedProperty reticleClampProperty = serializedManager.FindProperty("enableReticleClamp");
            SerializedProperty maxDriftProperty = serializedManager.FindProperty("maxReticleDriftPixels");
            SerializedProperty pitchBlendProperty = serializedManager.FindProperty("enableThirdPersonAimPitchBlend");
            SerializedProperty aimDebugRaysProperty = serializedManager.FindProperty("enableAimDebugRays");

            if (ambienceEnabledProperty != null && ambienceEnabledProperty.boolValue != includeAmbienceReference)
            {
                ambienceEnabledProperty.boolValue = includeAmbienceReference;
                changed = true;
            }

            if (playlistProperty != null && includeAmbienceReference)
            {
                if (ambientPlaylist != null && playlistProperty.objectReferenceValue != ambientPlaylist)
                {
                    playlistProperty.objectReferenceValue = ambientPlaylist;
                    changed = true;
                }
            }
            else if (playlistProperty != null && playlistProperty.objectReferenceValue != null)
            {
                playlistProperty.objectReferenceValue = null;
                changed = true;
            }

            if (applyOnStartProperty != null && !applyOnStartProperty.boolValue)
            {
                applyOnStartProperty.boolValue = true;
                changed = true;
            }

            if (applyInEditorProperty != null && !applyInEditorProperty.boolValue)
            {
                applyInEditorProperty.boolValue = true;
                changed = true;
            }

            if (debugProperty != null && debugProperty.boolValue)
            {
                debugProperty.boolValue = false;
                changed = true;
            }

            if (armIkProperty != null && armIkProperty.boolValue)
            {
                armIkProperty.boolValue = false;
                changed = true;
            }

            if (convergenceProperty != null && convergenceProperty.boolValue)
            {
                convergenceProperty.boolValue = false;
                changed = true;
            }

            if (reticleModeProperty != null
                && reticleModeProperty.enumValueIndex != (int)CCS_AimReticleMode.HybridCameraCenterWithMuzzleDrift)
            {
                reticleModeProperty.enumValueIndex = (int)CCS_AimReticleMode.HybridCameraCenterWithMuzzleDrift;
                changed = true;
            }

            if (reticleClampProperty != null && !reticleClampProperty.boolValue)
            {
                reticleClampProperty.boolValue = true;
                changed = true;
            }

            if (maxDriftProperty != null
                && !Mathf.Approximately(
                    maxDriftProperty.floatValue,
                    CCS_WeaponsConstants.MasterTestMaxReticleDriftPixelsDefault))
            {
                maxDriftProperty.floatValue = CCS_WeaponsConstants.MasterTestMaxReticleDriftPixelsDefault;
                changed = true;
            }

            if (pitchBlendProperty != null && pitchBlendProperty.boolValue)
            {
                pitchBlendProperty.boolValue = false;
                changed = true;
            }

            if (aimDebugRaysProperty != null && aimDebugRaysProperty.boolValue)
            {
                aimDebugRaysProperty.boolValue = false;
                changed = true;
            }

            serializedManager.ApplyModifiedPropertiesWithoutUndo();
            return changed;
        }

        private static bool SerializedArrayMatches(SerializedProperty arrayProperty, Object[] expectedValues)
        {
            if (arrayProperty == null || !arrayProperty.isArray)
            {
                return false;
            }

            if (arrayProperty.arraySize != expectedValues.Length)
            {
                return false;
            }

            for (int i = 0; i < expectedValues.Length; i++)
            {
                if (arrayProperty.GetArrayElementAtIndex(i).objectReferenceValue != expectedValues[i])
                {
                    return false;
                }
            }

            return true;
        }

        private static GameObject FindRootObject(Scene scene, string objectName)
        {
            if (!scene.IsValid())
            {
                return null;
            }

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

        #endregion
    }
}
