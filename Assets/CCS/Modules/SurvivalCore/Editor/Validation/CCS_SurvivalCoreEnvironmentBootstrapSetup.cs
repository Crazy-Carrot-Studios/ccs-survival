using CCS.Modules.SurvivalCore;
using CCS.Modules.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// =============================================================================
// SCRIPT: CCS_SurvivalCoreEnvironmentBootstrapSetup
// CATEGORY: Modules / SurvivalCore / Editor / Validation
// PURPOSE: Updates default survival core profile and bootstrap influence HUD panel.
// PLACEMENT: Batch entry for 0.7.3 environment integration milestone.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: No Health damage, hypothermia, or clothing insulation in 0.7.3.
// =============================================================================

namespace CCS.Modules.SurvivalCore.Editor
{
    public static class CCS_SurvivalCoreEnvironmentBootstrapSetup
    {
        private const string DefaultProfilePath =
            "Assets/CCS/Survival/Profiles/SurvivalCore/CCS_DefaultSurvivalCoreProfile.asset";
        private const string BootstrapScenePath = "Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string InfluenceHudPanelObjectName = "EnvironmentInfluenceHudArea";
        private const string LogPrefix = "[CCS_SurvivalCoreEnvironmentBootstrapSetup]";

        #region Public Methods

        public static void ExecuteBatch()
        {
            EnsureDefaultProfile();
            EnsureBootstrapInfluenceHudPanel();

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} Survival core environment bootstrap setup complete.");
            EditorApplication.Exit(0);
        }

        #endregion

        #region Private Methods

        private static void EnsureDefaultProfile()
        {
            CCS_SurvivalCoreProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_SurvivalCoreProfile>(DefaultProfilePath);
            if (profile == null)
            {
                Debug.LogError($"{LogPrefix} Missing default survival core profile: {DefaultProfilePath}");
                EditorApplication.Exit(1);
                return;
            }

            SerializedObject serializedProfile = new SerializedObject(profile);
            serializedProfile.FindProperty("profileDescription").stringValue =
                "Default survival core stat tuning with environment integration for 0.7.3.";
            serializedProfile.FindProperty("profileVersion").stringValue = "0.7.3";
            serializedProfile.FindProperty("temperatureRecoveryRate").floatValue = 0.15f;
            serializedProfile.FindProperty("temperatureDecayRate").floatValue = 0.12f;
            serializedProfile.FindProperty("exposureFatigueMultiplier").floatValue = 0.008f;
            serializedProfile.FindProperty("wetnessThirstMultiplier").floatValue = 0.01f;
            serializedProfile.FindProperty("minimumTemperatureClamp").floatValue = 0f;
            serializedProfile.FindProperty("maximumTemperatureClamp").floatValue = 100f;
            serializedProfile.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static void EnsureBootstrapInfluenceHudPanel()
        {
            Scene scene = EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Single);
            CCS_HudRootPresenter hudRoot = Object.FindFirstObjectByType<CCS_HudRootPresenter>();
            if (hudRoot == null)
            {
                Debug.LogError($"{LogPrefix} Bootstrap scene is missing PF_CCS_HUD_Root instance.");
                EditorApplication.Exit(1);
                return;
            }

            Transform existingPanel = hudRoot.transform.Find(InfluenceHudPanelObjectName);
            GameObject panelObject = existingPanel != null
                ? existingPanel.gameObject
                : new GameObject(InfluenceHudPanelObjectName);

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
            panelRect.anchoredPosition = new Vector2(-28f, -330f);
            panelRect.sizeDelta = new Vector2(180f, 90f);

            CCS_SurvivalEnvironmentInfluenceHudPresenter hudPresenter =
                panelObject.GetComponent<CCS_SurvivalEnvironmentInfluenceHudPresenter>();
            if (hudPresenter == null)
            {
                hudPresenter = panelObject.AddComponent<CCS_SurvivalEnvironmentInfluenceHudPresenter>();
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
                statusText.text = "Temp Δ: 0\nFatigue Δ: 0\nThirst Δ: 0";
            }

            SerializedObject serializedPresenter = new SerializedObject(hudPresenter);
            serializedPresenter.FindProperty("statusText").objectReferenceValue = statusText;
            serializedPresenter.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        #endregion
    }
}
