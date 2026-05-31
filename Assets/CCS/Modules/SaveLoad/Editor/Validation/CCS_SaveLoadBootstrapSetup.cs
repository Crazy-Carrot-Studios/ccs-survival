using System.IO;
using CCS.Modules.SaveLoad;
using CCS.Modules.UI;
using CCS.Survival.Composition;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// =============================================================================
// SCRIPT: CCS_SaveLoadBootstrapSetup
// CATEGORY: Modules / SaveLoad / Editor / Validation
// PURPOSE: Creates default profile, bootstrap wiring, and development test saveable.
// PLACEMENT: Batch entry for 0.6.0 save/load foundation and 0.6.1 debug controls.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Framework setup only. No gameplay module persistence yet.
// =============================================================================

namespace CCS.Modules.SaveLoad.Editor
{
    public static class CCS_SaveLoadBootstrapSetup
    {
        private const string ProfilesRoot = "Assets/CCS/Survival/Profiles/SaveLoad";
        private const string DefaultProfilePath = ProfilesRoot + "/CCS_DefaultSaveLoadProfile.asset";
        private const string BootstrapPrefabPath = "Assets/CCS/Survival/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
        private const string BootstrapScenePath = "Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string TestSaveableObjectName = "CCS_TestSaveableComponent";
        private const string DebugControllerObjectName = "CCS_SaveLoadDebugController";
        private const string DebugPanelObjectName = "SaveLoadDebugArea";
        private const string LogPrefix = "[CCS_SaveLoadBootstrapSetup]";

        #region Public Methods

        public static void ExecuteBatch()
        {
            EnsureFolders();
            EnsureDefaultProfile();
            EnsureBootstrapGameplayServiceHost();
            EnsureBootstrapTestSaveable();
            EnsureBootstrapDebugControls();

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} Save/load bootstrap setup complete.");
            EditorApplication.Exit(0);
        }

        #endregion

        #region Private Methods

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/CCS/Survival/Profiles");
            EnsureFolder(ProfilesRoot);
        }

        private static CCS_SaveLoadProfile EnsureDefaultProfile()
        {
            CCS_SaveLoadProfile profile = AssetDatabase.LoadAssetAtPath<CCS_SaveLoadProfile>(DefaultProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CCS_SaveLoadProfile>();
                AssetDatabase.CreateAsset(profile, DefaultProfilePath);
            }

            SerializedObject serializedProfile = new SerializedObject(profile);
            serializedProfile.FindProperty("profileDisplayName").stringValue = "Default Save Load";
            serializedProfile.FindProperty("profileId").stringValue = "ccs.survival.profile.saveload.default";
            serializedProfile.FindProperty("profileDescription").stringValue =
                "Default save/load rules for 0.6.1 debug controls.";
            serializedProfile.FindProperty("profileVersion").stringValue = "0.6.1";
            serializedProfile.FindProperty("autoSaveEnabled").boolValue = false;
            serializedProfile.FindProperty("autoSaveIntervalSeconds").floatValue = 300f;
            serializedProfile.FindProperty("maxSaveSlots").intValue = 10;
            serializedProfile.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static void EnsureBootstrapGameplayServiceHost()
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
            serializedHost.FindProperty("saveLoadProfile").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<Object>(DefaultProfilePath);
            serializedHost.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(prefabContents, prefabPath);
            PrefabUtility.UnloadPrefabContents(prefabContents);
        }

        private static void EnsureBootstrapTestSaveable()
        {
            Scene scene = EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Single);
            Transform sceneRoot = FindSceneRoot();
            if (sceneRoot == null)
            {
                Debug.LogError($"{LogPrefix} Could not find CCS_BuildVerificationScene root.");
                EditorApplication.Exit(1);
                return;
            }

            Transform existing = sceneRoot.Find(TestSaveableObjectName);
            GameObject saveableObject = existing != null
                ? existing.gameObject
                : new GameObject(TestSaveableObjectName);

            if (existing == null)
            {
                saveableObject.transform.SetParent(sceneRoot, false);
            }

            CCS_TestSaveableComponent testSaveable = saveableObject.GetComponent<CCS_TestSaveableComponent>();
            if (testSaveable == null)
            {
                testSaveable = saveableObject.AddComponent<CCS_TestSaveableComponent>();
            }

            SerializedObject serializedSaveable = new SerializedObject(testSaveable);
            serializedSaveable.FindProperty("enableTestSaveable").boolValue = true;
            serializedSaveable.FindProperty("testString").stringValue = "bootstrap-test";
            serializedSaveable.FindProperty("testInteger").intValue = 42;
            serializedSaveable.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void EnsureBootstrapDebugControls()
        {
            Scene scene = EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Single);
            Transform sceneRoot = FindSceneRoot();
            if (sceneRoot == null)
            {
                Debug.LogError($"{LogPrefix} Could not find CCS_BuildVerificationScene root for debug controls.");
                EditorApplication.Exit(1);
                return;
            }

            CCS_SaveLoadDebugController debugController = EnsureDebugController(sceneRoot);
            CCS_HudRootPresenter hudRoot = Object.FindFirstObjectByType<CCS_HudRootPresenter>();
            if (hudRoot == null)
            {
                Debug.LogError($"{LogPrefix} Bootstrap scene is missing PF_CCS_HUD_Root instance.");
                EditorApplication.Exit(1);
                return;
            }

            EnsureDebugPanel(hudRoot.transform, debugController);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static CCS_SaveLoadDebugController EnsureDebugController(Transform sceneRoot)
        {
            Transform existing = sceneRoot.Find(DebugControllerObjectName);
            GameObject controllerObject = existing != null
                ? existing.gameObject
                : new GameObject(DebugControllerObjectName);

            if (existing == null)
            {
                controllerObject.transform.SetParent(sceneRoot, false);
            }

            CCS_SaveLoadDebugController debugController =
                controllerObject.GetComponent<CCS_SaveLoadDebugController>();

            if (debugController == null)
            {
                debugController = controllerObject.AddComponent<CCS_SaveLoadDebugController>();
            }

            SerializedObject serializedController = new SerializedObject(debugController);
            serializedController.FindProperty("enableDebugControls").boolValue = true;
            serializedController.FindProperty("selectedSlotId").stringValue = "slot_01";
            serializedController.ApplyModifiedPropertiesWithoutUndo();

            return debugController;
        }

        private static void EnsureDebugPanel(Transform hudRoot, CCS_SaveLoadDebugController debugController)
        {
            Transform existingPanel = hudRoot.Find(DebugPanelObjectName);
            GameObject panelObject = existingPanel != null
                ? existingPanel.gameObject
                : new GameObject(DebugPanelObjectName);

            if (existingPanel == null)
            {
                panelObject.transform.SetParent(hudRoot, false);
            }

            RectTransform panelRect = panelObject.GetComponent<RectTransform>();
            if (panelRect == null)
            {
                panelRect = panelObject.AddComponent<RectTransform>();
            }

            Image panelBackground = panelObject.GetComponent<Image>();
            if (panelBackground == null)
            {
                panelBackground = panelObject.AddComponent<Image>();
            }

            panelBackground.color = new Color(0f, 0f, 0f, 0.35f);
            panelRect.anchorMin = new Vector2(0f, 1f);
            panelRect.anchorMax = new Vector2(0f, 1f);
            panelRect.pivot = new Vector2(0f, 1f);
            panelRect.anchoredPosition = new Vector2(28f, -280f);
            panelRect.sizeDelta = new Vector2(380f, 210f);

            CCS_SaveLoadDebugPanelPresenter panelPresenter =
                panelObject.GetComponent<CCS_SaveLoadDebugPanelPresenter>();

            if (panelPresenter == null)
            {
                panelPresenter = panelObject.AddComponent<CCS_SaveLoadDebugPanelPresenter>();
            }

            Text statusText = panelObject.transform.Find("StatusText")?.GetComponent<Text>();
            if (statusText == null)
            {
                GameObject statusObject = new GameObject("StatusText");
                statusObject.transform.SetParent(panelObject.transform, false);
                RectTransform statusRect = statusObject.AddComponent<RectTransform>();
                statusRect.anchorMin = new Vector2(0f, 1f);
                statusRect.anchorMax = new Vector2(1f, 1f);
                statusRect.pivot = new Vector2(0.5f, 1f);
                statusRect.anchoredPosition = new Vector2(0f, -8f);
                statusRect.sizeDelta = new Vector2(-16f, 96f);
                statusText = statusObject.AddComponent<Text>();
                statusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                statusText.fontSize = 14;
                statusText.alignment = TextAnchor.UpperLeft;
                statusText.color = new Color(0.95f, 0.95f, 0.95f, 1f);
                statusText.horizontalOverflow = HorizontalWrapMode.Wrap;
                statusText.verticalOverflow = VerticalWrapMode.Truncate;
                statusText.text = "Save Debug";
            }

            Transform buttonRow = panelObject.transform.Find("ButtonRow");
            if (buttonRow == null)
            {
                GameObject buttonRowObject = new GameObject("ButtonRow");
                buttonRowObject.transform.SetParent(panelObject.transform, false);
                RectTransform buttonRowRect = buttonRowObject.AddComponent<RectTransform>();
                buttonRowRect.anchorMin = new Vector2(0f, 0f);
                buttonRowRect.anchorMax = new Vector2(1f, 0f);
                buttonRowRect.pivot = new Vector2(0.5f, 0f);
                buttonRowRect.anchoredPosition = new Vector2(0f, 8f);
                buttonRowRect.sizeDelta = new Vector2(-16f, 88f);
                HorizontalLayoutGroup layoutGroup = buttonRowObject.AddComponent<HorizontalLayoutGroup>();
                layoutGroup.spacing = 6f;
                layoutGroup.childAlignment = TextAnchor.MiddleLeft;
                layoutGroup.childControlWidth = true;
                layoutGroup.childControlHeight = true;
                layoutGroup.childForceExpandWidth = true;
                layoutGroup.childForceExpandHeight = true;
                buttonRow = buttonRowObject.transform;
            }

            Button saveButton = EnsureDebugButton(buttonRow, "SaveButton", "Save");
            Button loadButton = EnsureDebugButton(buttonRow, "LoadButton", "Load");
            Button deleteButton = EnsureDebugButton(buttonRow, "DeleteButton", "Del");
            Button refreshButton = EnsureDebugButton(buttonRow, "RefreshButton", "Ref");
            Button previousSlotButton = EnsureDebugButton(buttonRow, "PrevSlotButton", "<");
            Button nextSlotButton = EnsureDebugButton(buttonRow, "NextSlotButton", ">");

            SerializedObject serializedPanel = new SerializedObject(panelPresenter);
            serializedPanel.FindProperty("debugController").objectReferenceValue = debugController;
            serializedPanel.FindProperty("statusText").objectReferenceValue = statusText;
            serializedPanel.FindProperty("saveButton").objectReferenceValue = saveButton;
            serializedPanel.FindProperty("loadButton").objectReferenceValue = loadButton;
            serializedPanel.FindProperty("deleteButton").objectReferenceValue = deleteButton;
            serializedPanel.FindProperty("refreshButton").objectReferenceValue = refreshButton;
            serializedPanel.FindProperty("previousSlotButton").objectReferenceValue = previousSlotButton;
            serializedPanel.FindProperty("nextSlotButton").objectReferenceValue = nextSlotButton;
            serializedPanel.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Button EnsureDebugButton(Transform parent, string objectName, string label)
        {
            Transform existing = parent.Find(objectName);
            GameObject buttonObject = existing != null ? existing.gameObject : new GameObject(objectName);
            if (existing == null)
            {
                buttonObject.transform.SetParent(parent, false);
            }

            Image buttonImage = buttonObject.GetComponent<Image>();
            if (buttonImage == null)
            {
                buttonImage = buttonObject.AddComponent<Image>();
            }

            buttonImage.color = new Color(0.15f, 0.15f, 0.15f, 0.85f);

            Button button = buttonObject.GetComponent<Button>();
            if (button == null)
            {
                button = buttonObject.AddComponent<Button>();
            }

            Transform labelTransform = buttonObject.transform.Find("Label");
            Text labelText;
            if (labelTransform == null)
            {
                GameObject labelObject = new GameObject("Label");
                labelObject.transform.SetParent(buttonObject.transform, false);
                RectTransform labelRect = labelObject.AddComponent<RectTransform>();
                labelRect.anchorMin = Vector2.zero;
                labelRect.anchorMax = Vector2.one;
                labelRect.offsetMin = Vector2.zero;
                labelRect.offsetMax = Vector2.zero;
                labelText = labelObject.AddComponent<Text>();
                labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                labelText.fontSize = 13;
                labelText.alignment = TextAnchor.MiddleCenter;
                labelText.color = Color.white;
            }
            else
            {
                labelText = labelTransform.GetComponent<Text>();
            }

            if (labelText != null)
            {
                labelText.text = label;
            }

            return button;
        }

        private static Transform FindSceneRoot()
        {
            GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                if (roots[i].name == "CCS_BuildVerificationScene")
                {
                    return roots[i].transform;
                }
            }

            return null;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string parent = Path.GetDirectoryName(path)?.Replace("\\", "/");
            string folderName = Path.GetFileName(path);
            if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(folderName))
            {
                return;
            }

            if (!AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, folderName);
        }

        #endregion
    }
}
