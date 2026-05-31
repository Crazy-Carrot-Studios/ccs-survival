using System.IO;
using CCS.Modules.TimeOfDay;
using CCS.Modules.UI;
using CCS.Survival.Composition;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// =============================================================================
// SCRIPT: CCS_TimeOfDayBootstrapSetup
// CATEGORY: Modules / TimeOfDay / Editor / Validation
// PURPOSE: Creates default profile, bootstrap wiring, and HUD time display.
// PLACEMENT: Batch entry for 0.7.0 time-of-day foundation milestone.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: No lighting, weather, or final clock art in 0.7.0.
// =============================================================================

namespace CCS.Modules.TimeOfDay.Editor
{
    public static class CCS_TimeOfDayBootstrapSetup
    {
        private const string ProfilesRoot = "Assets/CCS/Survival/Profiles/TimeOfDay";
        private const string DefaultProfilePath = ProfilesRoot + "/CCS_DefaultTimeOfDayProfile.asset";
        private const string BootstrapPrefabPath = "Assets/CCS/Survival/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
        private const string BootstrapScenePath = "Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string TimeHudPanelObjectName = "TimeOfDayHudArea";
        private const string LogPrefix = "[CCS_TimeOfDayBootstrapSetup]";

        #region Public Methods

        public static void ExecuteBatch()
        {
            EnsureFolders();
            EnsureDefaultProfile();
            EnsureBootstrapGameplayServiceHost();
            EnsureBootstrapTimeHudPanel();

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} Time-of-day bootstrap setup complete.");
            EditorApplication.Exit(0);
        }

        #endregion

        #region Private Methods

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/CCS/Survival/Profiles");
            EnsureFolder(ProfilesRoot);
        }

        private static CCS_TimeOfDayProfile EnsureDefaultProfile()
        {
            CCS_TimeOfDayProfile profile = AssetDatabase.LoadAssetAtPath<CCS_TimeOfDayProfile>(DefaultProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CCS_TimeOfDayProfile>();
                AssetDatabase.CreateAsset(profile, DefaultProfilePath);
            }

            SerializedObject serializedProfile = new SerializedObject(profile);
            serializedProfile.FindProperty("profileDisplayName").stringValue = "Default Time Of Day";
            serializedProfile.FindProperty("profileId").stringValue = "ccs.survival.profile.timeofday.default";
            serializedProfile.FindProperty("profileDescription").stringValue =
                "Default global game clock rules for 0.7.0 foundation.";
            serializedProfile.FindProperty("profileVersion").stringValue = "0.7.0";
            serializedProfile.FindProperty("startDay").intValue = 1;
            serializedProfile.FindProperty("startHour").intValue = 7;
            serializedProfile.FindProperty("startMinute").intValue = 0;
            serializedProfile.FindProperty("realSecondsPerGameDay").floatValue = 1800f;
            serializedProfile.FindProperty("pauseTimeOnStart").boolValue = false;
            serializedProfile.FindProperty("dawnStartHour").intValue = 5;
            serializedProfile.FindProperty("dayStartHour").intValue = 7;
            serializedProfile.FindProperty("duskStartHour").intValue = 18;
            serializedProfile.FindProperty("nightStartHour").intValue = 20;
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
            serializedHost.FindProperty("timeOfDayProfile").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<Object>(DefaultProfilePath);
            serializedHost.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(prefabContents, prefabPath);
            PrefabUtility.UnloadPrefabContents(prefabContents);
        }

        private static void EnsureBootstrapTimeHudPanel()
        {
            Scene scene = EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Single);
            CCS_HudRootPresenter hudRoot = Object.FindFirstObjectByType<CCS_HudRootPresenter>();
            if (hudRoot == null)
            {
                Debug.LogError($"{LogPrefix} Bootstrap scene is missing PF_CCS_HUD_Root instance.");
                EditorApplication.Exit(1);
                return;
            }

            Transform existingPanel = hudRoot.transform.Find(TimeHudPanelObjectName);
            GameObject panelObject = existingPanel != null
                ? existingPanel.gameObject
                : new GameObject(TimeHudPanelObjectName);

            if (existingPanel == null)
            {
                panelObject.transform.SetParent(hudRoot.transform, false);
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
            panelRect.anchorMin = new Vector2(1f, 1f);
            panelRect.anchorMax = new Vector2(1f, 1f);
            panelRect.pivot = new Vector2(1f, 1f);
            panelRect.anchoredPosition = new Vector2(-28f, -28f);
            panelRect.sizeDelta = new Vector2(180f, 110f);

            CCS_TimeOfDayHudPresenter hudPresenter = panelObject.GetComponent<CCS_TimeOfDayHudPresenter>();
            if (hudPresenter == null)
            {
                hudPresenter = panelObject.AddComponent<CCS_TimeOfDayHudPresenter>();
            }

            Text statusText = panelObject.transform.Find("StatusText")?.GetComponent<Text>();
            if (statusText == null)
            {
                GameObject statusObject = new GameObject("StatusText");
                statusObject.transform.SetParent(panelObject.transform, false);
                RectTransform statusRect = statusObject.AddComponent<RectTransform>();
                statusRect.anchorMin = Vector2.zero;
                statusRect.anchorMax = Vector2.one;
                statusRect.offsetMin = new Vector2(8f, 8f);
                statusRect.offsetMax = new Vector2(-8f, -8f);
                statusText = statusObject.AddComponent<Text>();
                statusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                statusText.fontSize = 14;
                statusText.alignment = TextAnchor.UpperLeft;
                statusText.color = new Color(0.95f, 0.95f, 0.95f, 1f);
                statusText.horizontalOverflow = HorizontalWrapMode.Wrap;
                statusText.verticalOverflow = VerticalWrapMode.Truncate;
                statusText.text = "Time";
            }

            SerializedObject serializedPresenter = new SerializedObject(hudPresenter);
            serializedPresenter.FindProperty("statusText").objectReferenceValue = statusText;
            serializedPresenter.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
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
