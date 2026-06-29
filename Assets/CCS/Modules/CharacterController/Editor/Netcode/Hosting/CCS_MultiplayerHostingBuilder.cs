using CCS.Modules.CharacterController.Editor;
using CCS.Modules.CharacterController.Netcode;
using CCS.Project;
using UnityEditor;
using UnityEditor.SceneManagement;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_MultiplayerHostingBuilder
// CATEGORY: Modules / CharacterController / Tests / Netcode / Editor
// PURPOSE: Verifies and repairs expected SCN_CCS_MultiplayerHosting hierarchy wiring.
// PLACEMENT: Editor builder utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Repairs scene objects, network prefab wiring, and hosting UI layout including Mode Select.
// =============================================================================

namespace CCS.Modules.CharacterController.Netcode.Editor
{
    public static class CCS_MultiplayerHostingBuilder
    {
        #region Public Methods

        public static bool VerifyAndRepairScene()
        {
            bool changed = VerifyAndRepairNetworkPrefabAssets();
            changed |= EnsureRequiredScenesInBuildSettings();

            Scene scene = EditorSceneManager.OpenScene(
                CCS_NetcodeConstants.MultiplayerHostingScenePath,
                OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError(
                    "[Hosting Builder] Could not open "
                    + CCS_NetcodeConstants.MultiplayerHostingScenePath);
                return false;
            }

            bool sceneChanged = false;
            sceneChanged |= EnsureDirectionalLight();
            sceneChanged |= EnsureMainCamera();
            sceneChanged |= EnsureEventSystem();
            sceneChanged |= EnsureNetworkManagerInstance();
            sceneChanged |= CCS_MultiplayerHostingSceneLayoutEditor.BuildOrRebuildLayout();
            sceneChanged |= EnsureHostingAmbientAudioInOpenScene(scene);
            sceneChanged |= CCS_MissingScriptScanUtility.RepairOpenScene(scene, out _) > 0;
            sceneChanged |= EnsureNetworkManagerInstance();

            changed |= sceneChanged;

            if (sceneChanged)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.Log("[Hosting Builder] Scene hierarchy repaired and saved.");
            }
            else if (!changed)
            {
                Debug.Log("[Hosting Builder] Scene hierarchy already matches expected layout.");
            }

            return changed;
        }

        public static bool VerifyAndRepairNetworkPrefabAssets()
        {
            return CCS_NetcodeNetworkPrefabSetupUtility.RebuildNetworkPrefabSetup();
        }

        public static bool EnsureRequiredScenesInBuildSettings()
        {
            bool changed = false;
            changed |= TryAddSceneToBuildSettings(CCS_NetcodeConstants.MultiplayerHostingScenePath);
            changed |= TryAddSceneToBuildSettings(CCS_NetcodeConstants.MasterTestScenePath);
            return changed;
        }

        private static bool TryAddSceneToBuildSettings(string scenePath)
        {
            if (string.IsNullOrEmpty(scenePath))
            {
                return false;
            }

            EditorBuildSettingsScene[] existingScenes = EditorBuildSettings.scenes;
            for (int i = 0; i < existingScenes.Length; i++)
            {
                if (existingScenes[i].path == scenePath)
                {
                    if (!existingScenes[i].enabled)
                    {
                        existingScenes[i].enabled = true;
                        EditorBuildSettings.scenes = existingScenes;
                        return true;
                    }

                    return false;
                }
            }

            EditorBuildSettingsScene[] updatedScenes = new EditorBuildSettingsScene[existingScenes.Length + 1];
            for (int i = 0; i < existingScenes.Length; i++)
            {
                updatedScenes[i] = existingScenes[i];
            }

            updatedScenes[existingScenes.Length] = new EditorBuildSettingsScene(scenePath, true);
            EditorBuildSettings.scenes = updatedScenes;
            return true;
        }

        #endregion

        #region Scene Methods

        private static bool EnsureDirectionalLight()
        {
            Light[] lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
            for (int i = 0; i < lights.Length; i++)
            {
                if (lights[i] != null && lights[i].type == LightType.Directional)
                {
                    return false;
                }
            }

            GameObject lightObject = new GameObject("Directional Light");
            Light directionalLight = lightObject.AddComponent<Light>();
            directionalLight.type = LightType.Directional;
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            return true;
        }

        private static bool EnsureMainCamera()
        {
            Camera[] cameras = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
            for (int i = 0; i < cameras.Length; i++)
            {
                if (cameras[i] != null && cameras[i].CompareTag("MainCamera"))
                {
                    return false;
                }
            }

            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
            return true;
        }

        private static bool EnsureEventSystem()
        {
            EventSystem eventSystem = Object.FindAnyObjectByType<EventSystem>();
            if (eventSystem == null)
            {
                GameObject eventSystemObject = new GameObject("EventSystem");
                eventSystemObject.AddComponent<EventSystem>();
                eventSystemObject.AddComponent<InputSystemUIInputModule>();
                return true;
            }

            bool changed = false;
            StandaloneInputModule legacyModule = eventSystem.GetComponent<StandaloneInputModule>();
            if (legacyModule != null)
            {
                Object.DestroyImmediate(legacyModule);
                changed = true;
            }

            if (eventSystem.GetComponent<InputSystemUIInputModule>() == null)
            {
                eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
                changed = true;
            }

            return changed;
        }

        private static bool EnsureNetworkManagerInstance()
        {
            GameObject existing = GameObject.Find("PF_CCS_NetworkManager");
            bool changed = false;

            if (existing != null)
            {
                changed |= CCS_MissingScriptScanUtility.RepairGameObjectHierarchy(
                    existing,
                    CCS_NetcodeConstants.MultiplayerHostingScenePath,
                    out _);

                if (existing.GetComponent<CCS_LocalMultiplayerLauncher>() == null)
                {
                    existing.AddComponent<CCS_LocalMultiplayerLauncher>();
                    changed = true;
                }

                if (CCS_NetcodeNetworkPrefabSetupUtility.SceneNetworkManagerReferencesAreValid(existing, out _))
                {
                    return changed;
                }

                Object.DestroyImmediate(existing);
                existing = null;
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_NetcodeConstants.NetworkManagerPrefabPath);
            if (prefab == null)
            {
                Debug.LogError(
                    "[Hosting Builder] Missing prefab asset: "
                    + CCS_NetcodeConstants.NetworkManagerPrefabPath);
                return changed;
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance != null)
            {
                instance.name = "PF_CCS_NetworkManager";
                changed = true;
            }

            return changed;
        }

        private static bool EnsureHostingAmbientAudioInOpenScene(Scene scene)
        {
            if (!scene.IsValid())
            {
                return false;
            }

            CCS_MasterTestRecordingAmbientAudioBuilder.EnsureAmbienceAssetsReady();
            return CCS_MasterTestRecordingAmbientAudioBuilder.EnsureAmbientAudioObjectInScene(
                scene,
                CCS_ProjectAudioConstants.HostingAmbientAudioObjectName,
                playOnStart: true);
        }

        #endregion
    }
}
