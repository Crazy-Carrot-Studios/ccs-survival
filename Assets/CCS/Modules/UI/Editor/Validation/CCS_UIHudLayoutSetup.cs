using System.IO;
using CCS.Modules.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// =============================================================================
// SCRIPT: CCS_UIHudLayoutSetup
// CATEGORY: Modules / UI / Editor / Validation
// PURPOSE: Applies 0.4.2a HUD readability layout pass to profile and prefab assets.
// PLACEMENT: Batch entry for HUD anchor and typography milestone.
// AUTHOR: James Schilz
// CREATED: 2026-05-30
// NOTES: Rebuilds PF_CCS_HUD_Root and refreshes bootstrap scene HUD instance.
// =============================================================================

namespace CCS.Modules.UI.Editor
{
    public static class CCS_UIHudLayoutSetup
    {
        private const string DefaultProfilePath = "Assets/CCS/Survival/Profiles/UI/CCS_DefaultHudProfile.asset";
        private const string HudPrefabPath = "Assets/CCS/Modules/UI/Prefabs/PF_CCS_HUD_Root.prefab";
        private const string BootstrapScenePath = "Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string LogPrefix = "[CCS_UIHudLayoutSetup]";

        #region Public Methods

        public static void ExecuteBatch()
        {
            EnsureProfileFolders();
            CCS_HudProfile profile = ApplyDefaultProfileLayoutValues();
            RebuildHudPrefab(profile);
            RefreshBootstrapSceneHudInstance();

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} HUD layout pass complete.");
            EditorApplication.Exit(0);
        }

        #endregion

        #region Private Methods

        private static void EnsureProfileFolders()
        {
            CCS_UIHudBootstrapSetup.EnsureProfileFoldersPublic();
        }

        private static CCS_HudProfile ApplyDefaultProfileLayoutValues()
        {
            CCS_HudProfile profile = AssetDatabase.LoadAssetAtPath<CCS_HudProfile>(DefaultProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CCS_HudProfile>();
                AssetDatabase.CreateAsset(profile, DefaultProfilePath);
            }

            SerializedObject serializedProfile = new SerializedObject(profile);
            serializedProfile.FindProperty("profileDisplayName").stringValue = "Default HUD";
            serializedProfile.FindProperty("profileId").stringValue = "ccs.survival.profile.ui.default";
            serializedProfile.FindProperty("profileDescription").stringValue =
                "Default HUD presentation profile tuned for readability at 0.4.2a.";
            serializedProfile.FindProperty("profileVersion").stringValue = "0.4.2a";
            serializedProfile.FindProperty("showSurvivalBars").boolValue = true;
            serializedProfile.FindProperty("showInteractionPrompt").boolValue = true;
            serializedProfile.FindProperty("showInventorySummary").boolValue = true;
            serializedProfile.FindProperty("showEquipmentSummary").boolValue = true;
            serializedProfile.FindProperty("showNotifications").boolValue = true;

            SerializedProperty notificationProfile = serializedProfile.FindProperty("notificationProfile");
            notificationProfile.FindPropertyRelative("maxVisibleCount").intValue = 4;
            notificationProfile.FindPropertyRelative("notificationLifetimeSeconds").floatValue = 4f;
            notificationProfile.FindPropertyRelative("notificationWidth").floatValue = 400f;
            notificationProfile.FindPropertyRelative("notificationRowHeight").floatValue = 40f;
            notificationProfile.FindPropertyRelative("notificationFontSize").intValue = 16;

            SerializedProperty layoutSettings = serializedProfile.FindProperty("layoutSettings");
            layoutSettings.FindPropertyRelative("hudScale").floatValue = 1f;
            layoutSettings.FindPropertyRelative("safeMargin").floatValue = 28f;
            layoutSettings.FindPropertyRelative("survivalBarWidth").floatValue = 400f;
            layoutSettings.FindPropertyRelative("survivalBarHeight").floatValue = 34f;
            layoutSettings.FindPropertyRelative("survivalBarFontSize").intValue = 17;
            layoutSettings.FindPropertyRelative("summaryFontSize").intValue = 16;
            layoutSettings.FindPropertyRelative("interactionPromptFontSize").intValue = 22;
            layoutSettings.FindPropertyRelative("interactionPromptVerticalOffset").floatValue = 56f;
            layoutSettings.FindPropertyRelative("interactionPromptWidth").floatValue = 480f;
            layoutSettings.FindPropertyRelative("interactionPromptHeight").floatValue = 44f;
            layoutSettings.FindPropertyRelative("summaryPanelWidth").floatValue = 420f;
            layoutSettings.FindPropertyRelative("summaryPanelHeight").floatValue = 38f;
            layoutSettings.FindPropertyRelative("notificationAreaHeight").floatValue = 240f;

            serializedProfile.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssetIfDirty(profile);
            return profile;
        }

        private static void RebuildHudPrefab(CCS_HudProfile profile)
        {
            if (File.Exists(HudPrefabPath))
            {
                AssetDatabase.DeleteAsset(HudPrefabPath);
            }

            CCS_UIHudBootstrapSetup.BuildHudPrefab(profile, HudPrefabPath);
        }

        private static void RefreshBootstrapSceneHudInstance()
        {
            Scene scene = EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Single);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(HudPrefabPath);
            if (prefab == null)
            {
                Debug.LogError($"{LogPrefix} Missing HUD prefab: {HudPrefabPath}");
                EditorApplication.Exit(1);
                return;
            }

            CCS_HudRootPresenter[] existingPresenters =
                CCS.Survival.CCS_SurvivalSceneQueryUtility.FindAllObjectsByType<CCS_HudRootPresenter>();
            for (int index = 0; index < existingPresenters.Length; index++)
            {
                Object.DestroyImmediate(existingPresenters[index].gameObject);
            }

            PrefabUtility.InstantiatePrefab(prefab, scene);
            EditorSceneManager.SaveScene(scene);
        }

        #endregion
    }
}
