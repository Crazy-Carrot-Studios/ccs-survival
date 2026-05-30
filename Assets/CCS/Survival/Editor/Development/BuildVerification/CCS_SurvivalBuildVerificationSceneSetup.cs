using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_SurvivalBuildVerificationSceneSetup
// CATEGORY: Survival / Editor / Development / BuildVerification
// PURPOSE: Ensures bootstrap scene has one Main Camera and build verification ground reference.
// PLACEMENT: Batch entry for 0.4.1b prototype scene build verification.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: No gameplay camera controller, Cinemachine, or UI. Saves scene and build settings.
// =============================================================================

namespace CCS.Survival.Editor.Development
{
    public static class CCS_SurvivalBuildVerificationSceneSetup
    {
        private const string ScenePath = "Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string BootstrapPrefabPath = "Assets/CCS/Survival/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
        private const string SceneRootName = "CCS_BuildVerificationScene";
        private const string GroundName = "CCS_BuildVerificationGround";
        private const string MainCameraName = "Main Camera";

        #region Public Methods

        public static void ExecuteBatch()
        {
            if (!File.Exists(ScenePath))
            {
                Debug.LogError($"[CCS_SurvivalBuildVerificationSceneSetup] Missing scene: {ScenePath}");
                EditorApplication.Exit(1);
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            ConfigureBuildSettings();
            EnsureBootstrapPrefabInstance();
            RemoveDuplicateMainCameras();
            EnsureMainCamera();
            EnsureBuildVerificationGround();

            if (!EditorSceneManager.SaveScene(scene))
            {
                Debug.LogError("[CCS_SurvivalBuildVerificationSceneSetup] Failed to save scene.");
                EditorApplication.Exit(1);
                return;
            }

            AssetDatabase.SaveAssets();
            Debug.Log("[CCS_SurvivalBuildVerificationSceneSetup] Scene setup complete.");
            EditorApplication.Exit(0);
        }

        #endregion

        #region Private Methods

        private static void ConfigureBuildSettings()
        {
            EditorBuildSettingsScene[] scenes =
            {
                new EditorBuildSettingsScene(
                    "Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity",
                    true),
                new EditorBuildSettingsScene(
                    "Assets/CCS/Framework/Core/Runtime/Scenes/SCN_CCS_Bootstrap.unity",
                    true)
            };

            EditorBuildSettings.scenes = scenes;
            Debug.Log("[CCS_SurvivalBuildVerificationSceneSetup] Build settings updated (Survival bootstrap first).");
        }

        private static void EnsureBootstrapPrefabInstance()
        {
            if (!File.Exists(BootstrapPrefabPath))
            {
                Debug.LogError($"[CCS_SurvivalBuildVerificationSceneSetup] Missing bootstrap prefab: {BootstrapPrefabPath}");
                EditorApplication.Exit(1);
                return;
            }

            CCS.Survival.CCS_SurvivalBootstrap[] survivalBootstraps =
                CCS.Survival.CCS_SurvivalSceneQueryUtility.FindActiveObjectsByType<CCS.Survival.CCS_SurvivalBootstrap>();

            for (int index = 1; index < survivalBootstraps.Length; index++)
            {
                Object.DestroyImmediate(survivalBootstraps[index].gameObject);
            }

            if (survivalBootstraps.Length >= 1)
            {
                Debug.Log("[CCS_SurvivalBuildVerificationSceneSetup] Bootstrap prefab instance already present.");
                return;
            }

            GameObject bootstrapPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BootstrapPrefabPath);
            if (bootstrapPrefab == null)
            {
                Debug.LogError($"[CCS_SurvivalBuildVerificationSceneSetup] Failed to load bootstrap prefab: {BootstrapPrefabPath}");
                EditorApplication.Exit(1);
                return;
            }

            GameObject bootstrapInstance = PrefabUtility.InstantiatePrefab(bootstrapPrefab) as GameObject;
            if (bootstrapInstance == null)
            {
                Debug.LogError("[CCS_SurvivalBuildVerificationSceneSetup] Failed to instantiate bootstrap prefab.");
                EditorApplication.Exit(1);
                return;
            }

            bootstrapInstance.name = bootstrapPrefab.name;
            Debug.Log("[CCS_SurvivalBuildVerificationSceneSetup] Bootstrap prefab instance added to scene.");
        }

        private static void RemoveDuplicateMainCameras()
        {
            Camera[] cameras = CCS.Survival.CCS_SurvivalSceneQueryUtility.FindAllObjectsByType<Camera>();
            List<Camera> mainCameras = new List<Camera>();

            for (int index = 0; index < cameras.Length; index++)
            {
                Camera camera = cameras[index];
                if (camera != null && camera.CompareTag("MainCamera"))
                {
                    mainCameras.Add(camera);
                }
            }

            for (int index = 1; index < mainCameras.Count; index++)
            {
                Object.DestroyImmediate(mainCameras[index].gameObject);
            }
        }

        private static void EnsureMainCamera()
        {
            Camera existingMainCamera = Camera.main;
            GameObject cameraObject;

            if (existingMainCamera != null)
            {
                cameraObject = existingMainCamera.gameObject;
                cameraObject.name = MainCameraName;
            }
            else
            {
                cameraObject = new GameObject(MainCameraName);
            }

            cameraObject.tag = "MainCamera";
            Transform cameraTransform = cameraObject.transform;
            cameraTransform.SetPositionAndRotation(
                new Vector3(0f, 4f, -8f),
                Quaternion.Euler(20f, 0f, 0f));

            Camera camera = cameraObject.GetComponent<Camera>();
            if (camera == null)
            {
                camera = cameraObject.AddComponent<Camera>();
            }

            camera.clearFlags = CameraClearFlags.Skybox;
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = 1000f;
            camera.fieldOfView = 60f;

            AudioListener listener = cameraObject.GetComponent<AudioListener>();
            if (listener == null)
            {
                cameraObject.AddComponent<AudioListener>();
            }

            RemoveExtraAudioListeners(cameraObject);
        }

        private static void RemoveExtraAudioListeners(GameObject keepListenerOn)
        {
            AudioListener[] listeners = CCS.Survival.CCS_SurvivalSceneQueryUtility.FindAllObjectsByType<AudioListener>();

            for (int index = 0; index < listeners.Length; index++)
            {
                AudioListener listener = listeners[index];
                if (listener == null || listener.gameObject == keepListenerOn)
                {
                    continue;
                }

                Object.DestroyImmediate(listener);
            }
        }

        private static void EnsureBuildVerificationGround()
        {
            GameObject sceneRoot = GameObject.Find(SceneRootName);
            if (sceneRoot == null)
            {
                sceneRoot = new GameObject(SceneRootName);
            }

            Transform groundTransform = sceneRoot.transform.Find(GroundName);
            GameObject groundObject;

            if (groundTransform != null)
            {
                groundObject = groundTransform.gameObject;
            }
            else
            {
                groundObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
                groundObject.name = GroundName;
                groundObject.transform.SetParent(sceneRoot.transform, false);
            }

            groundObject.transform.localPosition = Vector3.zero;
            groundObject.transform.localRotation = Quaternion.identity;
            groundObject.transform.localScale = new Vector3(2f, 1f, 2f);

            Collider groundCollider = groundObject.GetComponent<Collider>();
            if (groundCollider != null)
            {
                Object.DestroyImmediate(groundCollider);
            }
        }

        #endregion
    }
}
