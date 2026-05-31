using System.IO;
using CCS.Modules.EnvironmentEffects;
using CCS.Modules.UI;
using CCS.Survival.Composition;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// =============================================================================
// SCRIPT: CCS_EnvironmentEffectsBootstrapSetup
// CATEGORY: Modules / EnvironmentEffects / Editor / Validation
// PURPOSE: Creates default profile, bootstrap wiring, and HUD environment display.
// PLACEMENT: Batch entry for 0.7.2 environment effects foundation milestone.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Simulation layer only. No Survival Core stat mutation in 0.7.2.
// =============================================================================

namespace CCS.Modules.EnvironmentEffects.Editor
{
    public static class CCS_EnvironmentEffectsBootstrapSetup
    {
        private const string ProfilesRoot = "Assets/CCS/Survival/Profiles/EnvironmentEffects";
        private const string DefaultProfilePath = ProfilesRoot + "/CCS_DefaultEnvironmentEffectsProfile.asset";
        private const string BootstrapPrefabPath = "Assets/CCS/Survival/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
        private const string BootstrapScenePath = "Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string EnvironmentHudPanelObjectName = "EnvironmentHudArea";
        private const string LogPrefix = "[CCS_EnvironmentEffectsBootstrapSetup]";

        #region Public Methods

        public static void ExecuteBatch()
        {
            EnsureFolders();
            EnsureDefaultProfile();
            EnsureBootstrapGameplayServiceHost();
            EnsureBootstrapEnvironmentHudPanel();

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} Environment effects bootstrap setup complete.");
            EditorApplication.Exit(0);
        }

        #endregion

        #region Private Methods

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/CCS/Survival/Profiles");
            EnsureFolder(ProfilesRoot);
        }

        private static CCS_EnvironmentEffectsProfile EnsureDefaultProfile()
        {
            CCS_EnvironmentEffectsProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_EnvironmentEffectsProfile>(DefaultProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CCS_EnvironmentEffectsProfile>();
                AssetDatabase.CreateAsset(profile, DefaultProfilePath);
            }

            SerializedObject serializedProfile = new SerializedObject(profile);
            serializedProfile.FindProperty("profileDisplayName").stringValue = "Default Environment Effects";
            serializedProfile.FindProperty("profileId").stringValue = "ccs.survival.profile.environment.default";
            serializedProfile.FindProperty("profileDescription").stringValue =
                "Default environment simulation rules for 0.7.2 foundation.";
            serializedProfile.FindProperty("profileVersion").stringValue = "0.7.2";
            serializedProfile.FindProperty("dayTemperatureBonus").floatValue = 2f;
            serializedProfile.FindProperty("nightTemperaturePenalty").floatValue = -3f;
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
            serializedProfile.FindProperty("clearExposureModifier").floatValue = 0f;
            serializedProfile.FindProperty("cloudyExposureModifier").floatValue = 0.1f;
            serializedProfile.FindProperty("rainExposureModifier").floatValue = 0.4f;
            serializedProfile.FindProperty("stormExposureModifier").floatValue = 0.8f;
            serializedProfile.FindProperty("fogExposureModifier").floatValue = 0.2f;
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
            serializedHost.FindProperty("environmentEffectsProfile").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<Object>(DefaultProfilePath);
            serializedHost.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(prefabContents, prefabPath);
            PrefabUtility.UnloadPrefabContents(prefabContents);
        }

        private static void EnsureBootstrapEnvironmentHudPanel()
        {
            Scene scene = EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Single);
            CCS_HudRootPresenter hudRoot = Object.FindFirstObjectByType<CCS_HudRootPresenter>();
            if (hudRoot == null)
            {
                Debug.LogError($"{LogPrefix} Bootstrap scene is missing PF_CCS_HUD_Root instance.");
                EditorApplication.Exit(1);
                return;
            }

            Transform existingPanel = hudRoot.transform.Find(EnvironmentHudPanelObjectName);
            GameObject panelObject = existingPanel != null
                ? existingPanel.gameObject
                : new GameObject(EnvironmentHudPanelObjectName);

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
            panelRect.anchoredPosition = new Vector2(-28f, -232f);
            panelRect.sizeDelta = new Vector2(180f, 90f);

            CCS_EnvironmentEffectsHudPresenter hudPresenter =
                panelObject.GetComponent<CCS_EnvironmentEffectsHudPresenter>();
            if (hudPresenter == null)
            {
                hudPresenter = panelObject.AddComponent<CCS_EnvironmentEffectsHudPresenter>();
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
                statusText.text = "Env Temp: 0\nWetness: 0\nExposure: 0";
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
