using System.IO;
using CCS.Modules.UI;
using CCS.Modules.Weather;
using CCS.Survival.Composition;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// =============================================================================
// SCRIPT: CCS_WeatherBootstrapSetup
// CATEGORY: Modules / Weather / Editor / Validation
// PURPOSE: Creates default profile, bootstrap wiring, and HUD weather display.
// PLACEMENT: Batch entry for 0.7.1 weather foundation milestone.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: No VFX, lighting, audio, or final weather art in 0.7.1.
// =============================================================================

namespace CCS.Modules.Weather.Editor
{
    public static class CCS_WeatherBootstrapSetup
    {
        private const string ProfilesRoot = "Assets/CCS/Survival/Profiles/Weather";
        private const string DefaultProfilePath = ProfilesRoot + "/CCS_DefaultWeatherProfile.asset";
        private const string BootstrapPrefabPath = "Assets/CCS/Survival/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
        private const string BootstrapScenePath = "Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string WeatherHudPanelObjectName = "WeatherHudArea";
        private const string LogPrefix = "[CCS_WeatherBootstrapSetup]";

        #region Public Methods

        public static void ExecuteBatch()
        {
            EnsureFolders();
            EnsureDefaultProfile();
            EnsureBootstrapGameplayServiceHost();
            EnsureBootstrapWeatherHudPanel();

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} Weather bootstrap setup complete.");
            EditorApplication.Exit(0);
        }

        #endregion

        #region Private Methods

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/CCS/Survival/Profiles");
            EnsureFolder(ProfilesRoot);
        }

        private static CCS_WeatherProfile EnsureDefaultProfile()
        {
            CCS_WeatherProfile profile = AssetDatabase.LoadAssetAtPath<CCS_WeatherProfile>(DefaultProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CCS_WeatherProfile>();
                AssetDatabase.CreateAsset(profile, DefaultProfilePath);
            }

            SerializedObject serializedProfile = new SerializedObject(profile);
            serializedProfile.FindProperty("profileDisplayName").stringValue = "Default Weather";
            serializedProfile.FindProperty("profileId").stringValue = "ccs.survival.profile.weather.default";
            serializedProfile.FindProperty("profileDescription").stringValue =
                "Default global weather rules for 0.7.1 foundation.";
            serializedProfile.FindProperty("profileVersion").stringValue = "0.7.1";
            serializedProfile.FindProperty("startingWeather").enumValueIndex = (int)CCS_WeatherType.Clear;
            serializedProfile.FindProperty("weatherChangeEnabled").boolValue = true;
            serializedProfile.FindProperty("minimumWeatherDurationSeconds").floatValue = 300f;
            serializedProfile.FindProperty("maximumWeatherDurationSeconds").floatValue = 900f;
            serializedProfile.FindProperty("transitionDurationSeconds").floatValue = 20f;
            serializedProfile.FindProperty("clearTemperatureModifier").floatValue = 0f;
            serializedProfile.FindProperty("cloudyTemperatureModifier").floatValue = -1f;
            serializedProfile.FindProperty("rainTemperatureModifier").floatValue = -3f;
            serializedProfile.FindProperty("stormTemperatureModifier").floatValue = -5f;
            serializedProfile.FindProperty("fogTemperatureModifier").floatValue = -2f;
            serializedProfile.FindProperty("clearWetnessModifier").floatValue = 0f;
            serializedProfile.FindProperty("cloudyWetnessModifier").floatValue = 0f;
            serializedProfile.FindProperty("rainWetnessModifier").floatValue = 0.5f;
            serializedProfile.FindProperty("stormWetnessModifier").floatValue = 0.8f;
            serializedProfile.FindProperty("fogWetnessModifier").floatValue = 0.2f;
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
            serializedHost.FindProperty("weatherProfile").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<Object>(DefaultProfilePath);
            serializedHost.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(prefabContents, prefabPath);
            PrefabUtility.UnloadPrefabContents(prefabContents);
        }

        private static void EnsureBootstrapWeatherHudPanel()
        {
            Scene scene = EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Single);
            CCS_HudRootPresenter hudRoot = Object.FindFirstObjectByType<CCS_HudRootPresenter>();
            if (hudRoot == null)
            {
                Debug.LogError($"{LogPrefix} Bootstrap scene is missing PF_CCS_HUD_Root instance.");
                EditorApplication.Exit(1);
                return;
            }

            Transform existingPanel = hudRoot.transform.Find(WeatherHudPanelObjectName);
            GameObject panelObject = existingPanel != null
                ? existingPanel.gameObject
                : new GameObject(WeatherHudPanelObjectName);

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
            panelRect.anchoredPosition = new Vector2(-28f, -150f);
            panelRect.sizeDelta = new Vector2(180f, 70f);

            CCS_WeatherHudPresenter hudPresenter = panelObject.GetComponent<CCS_WeatherHudPresenter>();
            if (hudPresenter == null)
            {
                hudPresenter = panelObject.AddComponent<CCS_WeatherHudPresenter>();
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
                statusText.text = "Weather: Clear";
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
