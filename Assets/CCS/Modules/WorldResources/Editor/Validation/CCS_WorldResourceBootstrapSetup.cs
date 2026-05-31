using System.IO;
using CCS.Modules.Inventory;
using CCS.Modules.WorldResources;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_WorldResourceBootstrapSetup
// CATEGORY: Modules / WorldResources / Editor / Validation
// PURPOSE: Creates default profile, test definitions, and bootstrap scene test nodes.
// PLACEMENT: Batch entry for 0.5.1 world resource foundation milestone.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Primitive placeholders only. No final art or terrain systems.
// =============================================================================

namespace CCS.Modules.WorldResources.Editor
{
    public static class CCS_WorldResourceBootstrapSetup
    {
        private const string ProfilesRoot = "Assets/CCS/Survival/Profiles/WorldResources";
        private const string DefaultProfilePath = ProfilesRoot + "/CCS_DefaultWorldResourceProfile.asset";
        private const string TestItemsRoot = ProfilesRoot + "/TestItems";
        private const string TestResourcesRoot = ProfilesRoot + "/TestResources";
        private const string BootstrapScenePath = "Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string LogPrefix = "[CCS_WorldResourceBootstrapSetup]";

        private const string TestTreeObjectName = "CCS_TestTree";
        private const string TestRockObjectName = "CCS_TestRock";
        private const string TestPlantObjectName = "CCS_TestPlant";
        private const string TestAreaObjectName = "CCS_WorldResourceTestArea";

        #region Public Methods

        public static void ExecuteBatch()
        {
            EnsureFolders();
            CCS_WorldResourceProfile profile = EnsureDefaultProfile();
            CCS_ItemDefinition woodItem = EnsureTestItem("CCS_TestItem_Wood", "ccs.survival.item.test.wood", "Test Wood");
            CCS_ItemDefinition stoneItem = EnsureTestItem("CCS_TestItem_Stone", "ccs.survival.item.test.stone", "Test Stone");
            CCS_ItemDefinition fiberItem = EnsureTestItem("CCS_TestItem_Fiber", "ccs.survival.item.test.fiber", "Test Fiber");

            CCS_ResourceDefinition treeDefinition = EnsureTestResourceDefinition(
                "CCS_TestResource_Tree",
                "ccs.survival.resource.test.tree",
                "Test Tree",
                CCS_ResourceNodeType.Tree,
                CCS_RequiredToolType.Axe,
                3,
                30f,
                woodItem,
                1,
                2);

            CCS_ResourceDefinition rockDefinition = EnsureTestResourceDefinition(
                "CCS_TestResource_Rock",
                "ccs.survival.resource.test.rock",
                "Test Rock",
                CCS_ResourceNodeType.Rock,
                CCS_RequiredToolType.Pickaxe,
                2,
                45f,
                stoneItem,
                1,
                1);

            CCS_ResourceDefinition plantDefinition = EnsureTestResourceDefinition(
                "CCS_TestResource_Plant",
                "ccs.survival.resource.test.plant",
                "Test Plant",
                CCS_ResourceNodeType.Plant,
                CCS_RequiredToolType.Knife,
                1,
                20f,
                fiberItem,
                1,
                2);

            EnsureBootstrapTestArea(profile, treeDefinition, rockDefinition, plantDefinition);

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} World resource bootstrap setup complete.");
            EditorApplication.Exit(0);
        }

        #endregion

        #region Private Methods

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/CCS/Survival/Profiles");
            EnsureFolder(ProfilesRoot);
            EnsureFolder(TestItemsRoot);
            EnsureFolder(TestResourcesRoot);
        }

        private static CCS_WorldResourceProfile EnsureDefaultProfile()
        {
            CCS_WorldResourceProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_WorldResourceProfile>(DefaultProfilePath);

            if (profile != null)
            {
                return profile;
            }

            profile = ScriptableObject.CreateInstance<CCS_WorldResourceProfile>();
            SerializedObject serializedProfile = new SerializedObject(profile);
            serializedProfile.FindProperty("profileDisplayName").stringValue = "Default World Resources";
            serializedProfile.FindProperty("profileId").stringValue = "ccs.survival.profile.worldresources.default";
            serializedProfile.FindProperty("profileDescription").stringValue =
                "Default world resource rules for 0.5.1 foundation.";
            serializedProfile.FindProperty("profileVersion").stringValue = "0.5.1";
            serializedProfile.FindProperty("enableRespawn").boolValue = true;
            serializedProfile.FindProperty("globalRespawnMultiplier").floatValue = 1f;
            serializedProfile.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.CreateAsset(profile, DefaultProfilePath);
            return profile;
        }

        private static CCS_ItemDefinition EnsureTestItem(string assetName, string itemId, string displayName)
        {
            string assetPath = $"{TestItemsRoot}/{assetName}.asset";
            CCS_ItemDefinition itemDefinition = AssetDatabase.LoadAssetAtPath<CCS_ItemDefinition>(assetPath);
            if (itemDefinition != null)
            {
                return itemDefinition;
            }

            itemDefinition = ScriptableObject.CreateInstance<CCS_ItemDefinition>();
            SerializedObject serializedItem = new SerializedObject(itemDefinition);
            serializedItem.FindProperty("itemId").stringValue = itemId;
            serializedItem.FindProperty("displayName").stringValue = displayName;
            serializedItem.FindProperty("description").stringValue = "Bootstrap verification test item.";
            serializedItem.FindProperty("maxStackSize").intValue = 99;
            serializedItem.FindProperty("isStackable").boolValue = true;
            serializedItem.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.CreateAsset(itemDefinition, assetPath);
            return itemDefinition;
        }

        private static CCS_ResourceDefinition EnsureTestResourceDefinition(
            string assetName,
            string resourceId,
            string displayName,
            CCS_ResourceNodeType nodeType,
            CCS_RequiredToolType requiredToolType,
            int maxHarvestCount,
            float respawnTimeSeconds,
            CCS_ItemDefinition dropItem,
            int minQuantity,
            int maxQuantity)
        {
            string assetPath = $"{TestResourcesRoot}/{assetName}.asset";
            CCS_ResourceDefinition resourceDefinition =
                AssetDatabase.LoadAssetAtPath<CCS_ResourceDefinition>(assetPath);

            if (resourceDefinition != null)
            {
                return resourceDefinition;
            }

            resourceDefinition = ScriptableObject.CreateInstance<CCS_ResourceDefinition>();
            SerializedObject serializedResource = new SerializedObject(resourceDefinition);
            serializedResource.FindProperty("resourceId").stringValue = resourceId;
            serializedResource.FindProperty("displayName").stringValue = displayName;
            serializedResource.FindProperty("nodeType").enumValueIndex = (int)nodeType;
            serializedResource.FindProperty("maxHarvestCount").intValue = maxHarvestCount;
            serializedResource.FindProperty("respawnTimeSeconds").floatValue = respawnTimeSeconds;
            serializedResource.FindProperty("requiredToolType").enumValueIndex = (int)requiredToolType;

            SerializedProperty dropDefinitions = serializedResource.FindProperty("dropDefinitions");
            dropDefinitions.arraySize = 1;
            SerializedProperty dropEntry = dropDefinitions.GetArrayElementAtIndex(0);
            dropEntry.FindPropertyRelative("itemDefinition").objectReferenceValue = dropItem;
            dropEntry.FindPropertyRelative("minQuantity").intValue = minQuantity;
            dropEntry.FindPropertyRelative("maxQuantity").intValue = maxQuantity;
            serializedResource.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.CreateAsset(resourceDefinition, assetPath);
            return resourceDefinition;
        }

        private static void EnsureBootstrapTestArea(
            CCS_WorldResourceProfile profile,
            CCS_ResourceDefinition treeDefinition,
            CCS_ResourceDefinition rockDefinition,
            CCS_ResourceDefinition plantDefinition)
        {
            Scene scene = EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Single);
            Transform sceneRoot = FindSceneRoot();
            if (sceneRoot == null)
            {
                Debug.LogError($"{LogPrefix} Could not find CCS_BuildVerificationScene root.");
                EditorApplication.Exit(1);
                return;
            }

            Transform testArea = sceneRoot.Find(TestAreaObjectName);
            if (testArea == null)
            {
                GameObject testAreaObject = new GameObject(TestAreaObjectName);
                testAreaObject.transform.SetParent(sceneRoot, false);
                testAreaObject.transform.localPosition = new Vector3(0f, 0f, 4f);
                testArea = testAreaObject.transform;
            }

            EnsureHarvestableNode(testArea, TestTreeObjectName, PrimitiveType.Cylinder, new Vector3(-2f, 1f, 0f), new Vector3(0.6f, 2f, 0.6f), treeDefinition, profile);
            EnsureHarvestableNode(testArea, TestRockObjectName, PrimitiveType.Cube, new Vector3(0f, 0.5f, 0f), new Vector3(1.2f, 1f, 1.2f), rockDefinition, profile);
            EnsureHarvestableNode(testArea, TestPlantObjectName, PrimitiveType.Sphere, new Vector3(2f, 0.5f, 0f), new Vector3(0.8f, 0.8f, 0.8f), plantDefinition, profile);
            EnsureDevelopmentHarness(sceneRoot);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
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

        private static void EnsureHarvestableNode(
            Transform parent,
            string objectName,
            PrimitiveType primitiveType,
            Vector3 localPosition,
            Vector3 localScale,
            CCS_ResourceDefinition resourceDefinition,
            CCS_WorldResourceProfile profile)
        {
            Transform existing = parent.Find(objectName);
            GameObject nodeObject;

            if (existing != null)
            {
                nodeObject = existing.gameObject;
            }
            else
            {
                nodeObject = GameObject.CreatePrimitive(primitiveType);
                nodeObject.name = objectName;
                nodeObject.transform.SetParent(parent, false);
            }

            nodeObject.transform.localPosition = localPosition;
            nodeObject.transform.localScale = localScale;

            CCS_HarvestableResource harvestable = nodeObject.GetComponent<CCS_HarvestableResource>();
            if (harvestable == null)
            {
                harvestable = nodeObject.AddComponent<CCS_HarvestableResource>();
            }

            SerializedObject serializedHarvestable = new SerializedObject(harvestable);
            serializedHarvestable.FindProperty("resourceDefinition").objectReferenceValue = resourceDefinition;
            serializedHarvestable.FindProperty("worldResourceProfile").objectReferenceValue = profile;
            serializedHarvestable.FindProperty("assumeRequiredToolEquipped").boolValue = true;
            serializedHarvestable.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void EnsureDevelopmentHarness(Transform sceneRoot)
        {
            const string harnessObjectName = "CCS_ResourceHarvestingTestHarness";
            Transform existingHarness = sceneRoot.Find(harnessObjectName);
            GameObject harnessObject = existingHarness != null
                ? existingHarness.gameObject
                : new GameObject(harnessObjectName);

            if (existingHarness == null)
            {
                harnessObject.transform.SetParent(sceneRoot, false);
            }

            CCS_ResourceHarvestingTestHarness harness =
                harnessObject.GetComponent<CCS_ResourceHarvestingTestHarness>();

            if (harness == null)
            {
                harness = harnessObject.AddComponent<CCS_ResourceHarvestingTestHarness>();
            }

            SerializedObject serializedHarness = new SerializedObject(harness);
            serializedHarness.FindProperty("enableHarness").boolValue = true;
            serializedHarness.FindProperty("interactIntervalSeconds").floatValue = 3f;
            serializedHarness.ApplyModifiedPropertiesWithoutUndo();

            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                mainCamera.transform.localPosition = new Vector3(0f, 4f, -8f);
                mainCamera.transform.localRotation = Quaternion.Euler(20f, 0f, 0f);
            }
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
