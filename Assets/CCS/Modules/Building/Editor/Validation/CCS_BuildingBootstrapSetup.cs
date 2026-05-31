using System.Collections.Generic;
using System.IO;
using CCS.Modules.Building;
using CCS.Modules.UI;
using CCS.Survival.Composition;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// =============================================================================
// SCRIPT: CCS_BuildingBootstrapSetup
// CATEGORY: Modules / Building / Editor / Validation
// PURPOSE: Creates default profile, test definitions, and bootstrap gameplay wiring.
// PLACEMENT: Batch entry for 0.8.0 building foundation milestone.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Architecture only. No placement, snapping, holograms, or build mode.
// =============================================================================

namespace CCS.Modules.Building.Editor
{
    public static class CCS_BuildingBootstrapSetup
    {
        private const string ProfilesRoot = "Assets/CCS/Survival/Profiles/Building";
        private const string DefaultProfilePath = ProfilesRoot + "/CCS_DefaultBuildingProfile.asset";
        private const string DefinitionsRoot = "Assets/CCS/Survival/Content/Building/Definitions";
        private const string TestFoundationPath = DefinitionsRoot + "/CCS_TestFoundation.asset";
        private const string TestWallPath = DefinitionsRoot + "/CCS_TestWall.asset";
        private const string TestRoofPath = DefinitionsRoot + "/CCS_TestRoof.asset";
        private const string BootstrapPrefabPath = "Assets/CCS/Survival/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
        private const string BootstrapScenePath = "Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string EnvironmentHudPanelObjectName = "EnvironmentHudArea";
        private const string LogPrefix = "[CCS_BuildingBootstrapSetup]";

        #region Public Methods

        public static void ExecuteBatch()
        {
            EnsureFolders();
            CCS_BuildingPieceDefinition foundation = EnsureTestDefinition(
                TestFoundationPath,
                "ccs.survival.building.test.foundation",
                "Test Foundation",
                "Test foundation piece for 0.8.0 building architecture.",
                CCS_BuildingPieceType.Foundation);
            CCS_BuildingPieceDefinition wall = EnsureTestDefinition(
                TestWallPath,
                "ccs.survival.building.test.wall",
                "Test Wall",
                "Test wall piece for 0.8.0 building architecture.",
                CCS_BuildingPieceType.Wall);
            CCS_BuildingPieceDefinition roof = EnsureTestDefinition(
                TestRoofPath,
                "ccs.survival.building.test.roof",
                "Test Roof",
                "Test roof piece for 0.8.0 building architecture.",
                CCS_BuildingPieceType.Roof);

            List<CCS_BuildingPieceDefinition> startupDefinitions = new List<CCS_BuildingPieceDefinition>
            {
                foundation,
                wall,
                roof
            };

            EnsureDefaultProfile(startupDefinitions);
            EnsureBootstrapGameplayServiceHost();
            EnsureBootstrapEnvironmentHudPanel();

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} Building bootstrap setup complete.");
            EditorApplication.Exit(0);
        }

        #endregion

        #region Private Methods

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/CCS/Survival/Profiles");
            EnsureFolder(ProfilesRoot);
            EnsureFolder("Assets/CCS/Survival/Content");
            EnsureFolder("Assets/CCS/Survival/Content/Building");
            EnsureFolder(DefinitionsRoot);
        }

        private static void EnsureFolder(string folderPath)
        {
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                string parent = Path.GetDirectoryName(folderPath)?.Replace('\\', '/') ?? "Assets";
                string folderName = Path.GetFileName(folderPath);
                AssetDatabase.CreateFolder(parent, folderName);
            }
        }

        private static CCS_BuildingProfile EnsureDefaultProfile(
            IReadOnlyList<CCS_BuildingPieceDefinition> startupDefinitions)
        {
            CCS_BuildingProfile profile = AssetDatabase.LoadAssetAtPath<CCS_BuildingProfile>(DefaultProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CCS_BuildingProfile>();
                AssetDatabase.CreateAsset(profile, DefaultProfilePath);
            }

            SerializedObject serializedProfile = new SerializedObject(profile);
            serializedProfile.FindProperty("profileDisplayName").stringValue = "Default Building";
            serializedProfile.FindProperty("profileId").stringValue = "ccs.survival.profile.building.default";
            serializedProfile.FindProperty("profileDescription").stringValue =
                "Default building rules for 0.8.0 architecture foundation.";
            serializedProfile.FindProperty("profileVersion").stringValue = "0.8.0";
            serializedProfile.FindProperty("allowPlacement").boolValue = false;
            serializedProfile.FindProperty("allowDemolition").boolValue = false;
            serializedProfile.FindProperty("allowUpgrades").boolValue = false;

            SerializedProperty startupList = serializedProfile.FindProperty("startupDefinitions");
            startupList.ClearArray();
            for (int index = 0; index < startupDefinitions.Count; index++)
            {
                startupList.InsertArrayElementAtIndex(index);
                startupList.GetArrayElementAtIndex(index).objectReferenceValue = startupDefinitions[index];
            }

            serializedProfile.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static CCS_BuildingPieceDefinition EnsureTestDefinition(
            string assetPath,
            string pieceId,
            string displayName,
            string description,
            CCS_BuildingPieceType pieceType)
        {
            CCS_BuildingPieceDefinition definition =
                AssetDatabase.LoadAssetAtPath<CCS_BuildingPieceDefinition>(assetPath);

            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<CCS_BuildingPieceDefinition>();
                AssetDatabase.CreateAsset(definition, assetPath);
            }

            SerializedObject serializedDefinition = new SerializedObject(definition);
            serializedDefinition.FindProperty("pieceId").stringValue = pieceId;
            serializedDefinition.FindProperty("displayName").stringValue = displayName;
            serializedDefinition.FindProperty("description").stringValue = description;
            serializedDefinition.FindProperty("buildingPieceType").enumValueIndex = (int)pieceType;
            serializedDefinition.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
            return definition;
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
            serializedHost.FindProperty("buildingProfile").objectReferenceValue =
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
            if (existingPanel == null)
            {
                Debug.LogError($"{LogPrefix} Bootstrap scene is missing EnvironmentHudArea panel.");
                EditorApplication.Exit(1);
                return;
            }

            RectTransform panelRect = existingPanel.GetComponent<RectTransform>();
            if (panelRect != null)
            {
                panelRect.sizeDelta = new Vector2(190f, 225f);
            }

            Text statusText = existingPanel.Find("StatusText")?.GetComponent<Text>();
            if (statusText != null)
            {
                statusText.fontSize = 11;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        #endregion
    }
}
