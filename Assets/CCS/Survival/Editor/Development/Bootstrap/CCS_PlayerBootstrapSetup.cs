using System.IO;
using CCS.Modules.CharacterController;
using CCS.Survival.Composition;
using CCS.Survival.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_PlayerBootstrapSetup
// CATEGORY: Survival / Editor / Development / Bootstrap
// PURPOSE: Creates player prefab, wires bootstrap scene, and assigns controller profile.
// PLACEMENT: Batch entry for 0.9.0 character controller gameplay integration.
// AUTHOR: James Schilz
// CREATED: 2026-05-31
// NOTES: Disables scene Main Camera and noisy harness defaults for player testing.
// =============================================================================

namespace CCS.Survival.Editor.Development
{
    public static class CCS_PlayerBootstrapSetup
    {
        private const string InputActionsPath = "Assets/CCS/Survival/Input/CCS_Survival_InputActions.inputactions";
        private const string PlayerPrefabPath = "Assets/CCS/Survival/Prefabs/Player/PF_CCS_Player.prefab";
        private const string CharacterControllerProfilePath =
            "Assets/CCS/Survival/Profiles/CharacterController/CCS_DefaultCharacterControllerProfile.asset";
        private const string BootstrapPrefabPath = "Assets/CCS/Survival/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
        private const string BootstrapScenePath = "Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string LogPrefix = "[CCS_PlayerBootstrapSetup]";

        #region Public Methods

        public static void ExecuteBatch()
        {
            EnsureFolders();

            InputActionAsset inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsPath);
            if (inputActions == null)
            {
                Debug.LogError($"{LogPrefix} Missing input actions asset: {InputActionsPath}");
                EditorApplication.Exit(1);
                return;
            }

            UpdateCharacterControllerProfile();
            EnsurePlayerPrefab(inputActions);
            EnsureBootstrapGameplayHostProfile();
            EnsureBootstrapScenePlayerInstance();
            DisableNoisyHarnessDefaultsInScene();

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} Player bootstrap setup complete.");
            EditorApplication.Exit(0);
        }

        #endregion

        #region Private Methods

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/CCS/Survival/Input");
            EnsureFolder("Assets/CCS/Survival/Prefabs/Player");
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string parent = Path.GetDirectoryName(folderPath)?.Replace('\\', '/') ?? "Assets";
            string folderName = Path.GetFileName(folderPath);
            AssetDatabase.CreateFolder(parent, folderName);
        }

        private static void UpdateCharacterControllerProfile()
        {
            CCS_CharacterControllerProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_CharacterControllerProfile>(CharacterControllerProfilePath);
            if (profile == null)
            {
                Debug.LogError($"{LogPrefix} Missing profile: {CharacterControllerProfilePath}");
                EditorApplication.Exit(1);
                return;
            }

            SerializedObject serializedProfile = new SerializedObject(profile);
            serializedProfile.FindProperty("profileVersion").stringValue = "0.9.0";
            serializedProfile.FindProperty("profileDescription").stringValue =
                "Default character controller tuning for 0.9.0 playable player integration.";
            serializedProfile.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static void EnsurePlayerPrefab(InputActionAsset inputActions)
        {
            CCS_CharacterControllerProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_CharacterControllerProfile>(CharacterControllerProfilePath);

            GameObject playerRoot = new GameObject("PF_CCS_Player");

            UnityEngine.CharacterController controller = playerRoot.AddComponent<UnityEngine.CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.35f;
            controller.center = new Vector3(0f, 0.9f, 0f);
            controller.stepOffset = 0.35f;
            controller.slopeLimit = 45f;

            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.name = "VisualCapsule";
            Object.DestroyImmediate(visual.GetComponent<CapsuleCollider>());
            visual.transform.SetParent(playerRoot.transform, false);
            visual.transform.localPosition = new Vector3(0f, 0.9f, 0f);

            GameObject cameraPivot = new GameObject("CameraPivot");
            cameraPivot.transform.SetParent(playerRoot.transform, false);
            cameraPivot.transform.localPosition = new Vector3(0f, 1.6f, 0f);

            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetParent(cameraPivot.transform, false);
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = 1000f;
            camera.fieldOfView = 60f;
            cameraObject.AddComponent<AudioListener>();

            CCS_CharacterInputActionProvider inputProvider =
                playerRoot.AddComponent<CCS_CharacterInputActionProvider>();
            SerializedObject serializedInput = new SerializedObject(inputProvider);
            serializedInput.FindProperty("inputActions").objectReferenceValue = inputActions;
            serializedInput.ApplyModifiedPropertiesWithoutUndo();

            CCS_PlayerGameplayController gameplayController =
                playerRoot.AddComponent<CCS_PlayerGameplayController>();
            SerializedObject serializedGameplay = new SerializedObject(gameplayController);
            serializedGameplay.FindProperty("characterControllerProfile").objectReferenceValue = profile;
            serializedGameplay.FindProperty("cameraPivot").objectReferenceValue = cameraPivot.transform;
            serializedGameplay.FindProperty("playerCamera").objectReferenceValue = camera;
            serializedGameplay.FindProperty("lockCursorOnStart").boolValue = true;
            serializedGameplay.ApplyModifiedPropertiesWithoutUndo();

            CCS_InteractionPlayerDriver interactionDriver = playerRoot.AddComponent<CCS_InteractionPlayerDriver>();
            SerializedObject serializedInteraction = new SerializedObject(interactionDriver);
            serializedInteraction.FindProperty("interactionCamera").objectReferenceValue = camera;
            serializedInteraction.ApplyModifiedPropertiesWithoutUndo();

            if (File.Exists(PlayerPrefabPath))
            {
                AssetDatabase.DeleteAsset(PlayerPrefabPath);
            }

            PrefabUtility.SaveAsPrefabAsset(playerRoot, PlayerPrefabPath);
            Object.DestroyImmediate(playerRoot);
        }

        private static void EnsureBootstrapGameplayHostProfile()
        {
            GameObject prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(BootstrapPrefabPath);
            if (prefabRoot == null)
            {
                Debug.LogError($"{LogPrefix} Missing bootstrap prefab: {BootstrapPrefabPath}");
                EditorApplication.Exit(1);
                return;
            }

            string prefabPath = AssetDatabase.GetAssetPath(prefabRoot);
            GameObject prefabContents = PrefabUtility.LoadPrefabContents(prefabPath);
            CCS_SurvivalGameplayServiceHost host = prefabContents.GetComponent<CCS_SurvivalGameplayServiceHost>();
            if (host == null)
            {
                host = prefabContents.AddComponent<CCS_SurvivalGameplayServiceHost>();
            }

            SerializedObject serializedHost = new SerializedObject(host);
            serializedHost.FindProperty("characterControllerProfile").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<Object>(CharacterControllerProfilePath);
            serializedHost.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(prefabContents, prefabPath);
            PrefabUtility.UnloadPrefabContents(prefabContents);
        }

        private static void EnsureBootstrapScenePlayerInstance()
        {
            Scene scene = EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Single);

            GameObject existingPlayer = GameObject.Find("PF_CCS_Player");
            if (existingPlayer != null)
            {
                Object.DestroyImmediate(existingPlayer);
            }

            GameObject sceneMainCamera = GameObject.Find("Main Camera");
            if (sceneMainCamera != null && sceneMainCamera.transform.parent == null)
            {
                sceneMainCamera.SetActive(false);
            }

            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            if (playerPrefab == null)
            {
                Debug.LogError($"{LogPrefix} Missing player prefab: {PlayerPrefabPath}");
                EditorApplication.Exit(1);
                return;
            }

            GameObject playerInstance = PrefabUtility.InstantiatePrefab(playerPrefab, scene) as GameObject;
            if (playerInstance == null)
            {
                Debug.LogError($"{LogPrefix} Failed to instantiate player prefab in bootstrap scene.");
                EditorApplication.Exit(1);
                return;
            }

            playerInstance.name = "PF_CCS_Player";
            playerInstance.transform.SetPositionAndRotation(
                new Vector3(-1.5f, 0.1f, 0f),
                Quaternion.Euler(0f, 90f, 0f));

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void DisableNoisyHarnessDefaultsInScene()
        {
            Scene scene = EditorSceneManager.GetActiveScene();
            foreach (MonoBehaviour behaviour in Object.FindObjectsByType<MonoBehaviour>())
            {
                if (behaviour == null)
                {
                    continue;
                }

                string typeName = behaviour.GetType().Name;
                if (!typeName.EndsWith("TestHarness"))
                {
                    continue;
                }

                SerializedObject serializedHarness = new SerializedObject(behaviour);
                SerializedProperty enableProperty = serializedHarness.FindProperty("enableHarness");
                if (enableProperty != null && enableProperty.propertyType == SerializedPropertyType.Boolean)
                {
                    enableProperty.boolValue = false;
                    serializedHarness.ApplyModifiedPropertiesWithoutUndo();
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        #endregion
    }
}
