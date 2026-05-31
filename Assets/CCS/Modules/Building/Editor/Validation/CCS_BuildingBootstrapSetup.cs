using System.Collections.Generic;
using System.IO;
using CCS.Modules.Building;
using CCS.Modules.Inventory;
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
// PLACEMENT: Batch entry for 0.8.2 building construction costs milestone.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Placement costs, inventory integration, and development harness seeding.
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
        private const string WoodItemPath = "Assets/CCS/Survival/Profiles/WorldResources/TestItems/CCS_TestItem_Wood.asset";
        private const string StoneItemPath = "Assets/CCS/Survival/Profiles/WorldResources/TestItems/CCS_TestItem_Stone.asset";
        private const string FiberItemPath = "Assets/CCS/Survival/Profiles/WorldResources/TestItems/CCS_TestItem_Fiber.asset";
        private const string BootstrapPrefabPath = "Assets/CCS/Survival/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
        private const string BootstrapScenePath = "Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string EnvironmentHudPanelObjectName = "EnvironmentHudArea";
        private const string BuildingTestAreaName = "CCS_BuildingTestArea";
        private const string LogPrefix = "[CCS_BuildingBootstrapSetup]";

        #region Public Methods

        public static void ExecuteBatch()
        {
            EnsureFolders();

            CCS_ItemDefinition woodItem = LoadTestItem(WoodItemPath);
            CCS_ItemDefinition stoneItem = LoadTestItem(StoneItemPath);
            CCS_ItemDefinition fiberItem = LoadTestItem(FiberItemPath);

            CCS_BuildingPieceDefinition foundation = EnsureTestDefinition(
                TestFoundationPath,
                "ccs.survival.building.test.foundation",
                "Test Foundation",
                "Test foundation piece for 0.8.0 building architecture.",
                CCS_BuildingPieceType.Foundation,
                (woodItem, 4),
                (stoneItem, 2));
            CCS_BuildingPieceDefinition wall = EnsureTestDefinition(
                TestWallPath,
                "ccs.survival.building.test.wall",
                "Test Wall",
                "Test wall piece for 0.8.0 building architecture.",
                CCS_BuildingPieceType.Wall,
                (woodItem, 6));
            CCS_BuildingPieceDefinition roof = EnsureTestDefinition(
                TestRoofPath,
                "ccs.survival.building.test.roof",
                "Test Roof",
                "Test roof piece for 0.8.0 building architecture.",
                CCS_BuildingPieceType.Roof,
                (woodItem, 4),
                (fiberItem, 3));

            List<CCS_BuildingPieceDefinition> startupDefinitions = new List<CCS_BuildingPieceDefinition>
            {
                foundation,
                wall,
                roof
            };

            EnsureDefaultProfile(startupDefinitions);
            EnsureBootstrapGameplayServiceHost();
            EnsureBootstrapEnvironmentHudPanel();
            EnsureBootstrapBuildingTestArea(woodItem, stoneItem, fiberItem);

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
                "Default building rules for 0.8.2 construction costs and placement validation.";
            serializedProfile.FindProperty("profileVersion").stringValue = "0.8.2";
            serializedProfile.FindProperty("allowPlacement").boolValue = true;
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

        private static CCS_ItemDefinition LoadTestItem(string assetPath)
        {
            CCS_ItemDefinition itemDefinition = AssetDatabase.LoadAssetAtPath<CCS_ItemDefinition>(assetPath);
            if (itemDefinition == null)
            {
                Debug.LogError($"{LogPrefix} Missing test item asset: {assetPath}");
                EditorApplication.Exit(1);
            }

            return itemDefinition;
        }

        private static CCS_BuildingPieceDefinition EnsureTestDefinition(
            string assetPath,
            string pieceId,
            string displayName,
            string description,
            CCS_BuildingPieceType pieceType,
            params (CCS_ItemDefinition itemDefinition, int quantity)[] buildCosts)
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
            SetBuildCostEntries(serializedDefinition, buildCosts);
            serializedDefinition.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static void SetBuildCostEntries(
            SerializedObject serializedDefinition,
            (CCS_ItemDefinition itemDefinition, int quantity)[] buildCosts)
        {
            SerializedProperty costList = serializedDefinition.FindProperty("buildCostEntries");
            costList.ClearArray();

            if (buildCosts == null)
            {
                return;
            }

            for (int index = 0; index < buildCosts.Length; index++)
            {
                costList.InsertArrayElementAtIndex(index);
                SerializedProperty entry = costList.GetArrayElementAtIndex(index);
                entry.FindPropertyRelative("itemDefinition").objectReferenceValue = buildCosts[index].itemDefinition;
                entry.FindPropertyRelative("quantity").intValue = buildCosts[index].quantity;
            }
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
                panelRect.sizeDelta = new Vector2(190f, 260f);
            }

            Text statusText = existingPanel.Find("StatusText")?.GetComponent<Text>();
            if (statusText != null)
            {
                statusText.fontSize = 11;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void EnsureBootstrapBuildingTestArea(
            CCS_ItemDefinition woodItem,
            CCS_ItemDefinition stoneItem,
            CCS_ItemDefinition fiberItem)
        {
            Scene scene = EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Single);
            GameObject existingArea = GameObject.Find(BuildingTestAreaName);
            GameObject testArea = existingArea != null ? existingArea : new GameObject(BuildingTestAreaName);
            testArea.name = BuildingTestAreaName;
            testArea.transform.position = new Vector3(8f, 0f, 4f);

            CCS_BuildingPlacementPreview preview = testArea.GetComponent<CCS_BuildingPlacementPreview>();
            if (preview == null)
            {
                preview = testArea.AddComponent<CCS_BuildingPlacementPreview>();
            }

            CCS_BuildingPlacementTestHarness harness = testArea.GetComponent<CCS_BuildingPlacementTestHarness>();
            if (harness == null)
            {
                harness = testArea.AddComponent<CCS_BuildingPlacementTestHarness>();
            }

            SerializedObject serializedHarness = new SerializedObject(harness);
            serializedHarness.FindProperty("enableHarness").boolValue = true;
            serializedHarness.FindProperty("placementIntervalSeconds").floatValue = 4f;
            serializedHarness.FindProperty("testAreaAnchor").objectReferenceValue = testArea.transform;
            serializedHarness.FindProperty("seedWoodItem").objectReferenceValue = woodItem;
            serializedHarness.FindProperty("seedStoneItem").objectReferenceValue = stoneItem;
            serializedHarness.FindProperty("seedFiberItem").objectReferenceValue = fiberItem;
            serializedHarness.FindProperty("seedWoodQuantity").intValue = 50;
            serializedHarness.FindProperty("seedStoneQuantity").intValue = 20;
            serializedHarness.FindProperty("seedFiberQuantity").intValue = 20;
            serializedHarness.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        #endregion
    }
}
