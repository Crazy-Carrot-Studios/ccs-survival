using System.IO;
using CCS.Modules.Gathering;
using CCS.Modules.Inventory;
using CCS.Survival.Composition;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_GatheringBootstrapSetup
// CATEGORY: Modules / Gathering / Editor / Validation
// PURPOSE: Creates gathering profile, resource items, and bootstrap test nodes.
// PLACEMENT: Batch entry for 0.9.9 resource gathering foundation milestone.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: SmallTree, Rock, and Bush primitives with CCS_GatheringNode components.
// =============================================================================

namespace CCS.Modules.Gathering.Editor
{
    public static class CCS_GatheringBootstrapSetup
    {
        private const string ProfilesRoot = "Assets/CCS/Survival/Profiles/Gathering";
        private const string DefaultProfilePath = ProfilesRoot + "/CCS_DefaultGatheringProfile.asset";
        private const string ItemsRoot = "Assets/CCS/Survival/Content/Items/Resources/Primitive";
        private const string StickItemPath = ItemsRoot + "/CCS_Item_Stick.asset";
        private const string StoneItemPath = ItemsRoot + "/CCS_Item_Stone.asset";
        private const string WoodItemPath = ItemsRoot + "/CCS_Item_Wood.asset";
        private const string FiberItemPath = ItemsRoot + "/CCS_Item_Fiber.asset";
        private const string InventoryProfilePath = "Assets/CCS/Survival/Profiles/Inventory/CCS_DefaultInventoryProfile.asset";
        private const string BootstrapPrefabPath = "Assets/CCS/Survival/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
        private const string BootstrapScenePath = "Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string TestAreaObjectName = "CCS_GatheringTestArea";
        private const string SmallTreeObjectName = "CCS_TestGatheringSmallTree";
        private const string RockObjectName = "CCS_TestGatheringRock";
        private const string BushObjectName = "CCS_TestGatheringBush";
        private const string LogPrefix = "[CCS_GatheringBootstrapSetup]";

        #region Public Methods

        public static void ExecuteBatch()
        {
            EnsureFolders();

            CCS_ItemDefinition stickItem = EnsureStickItem();
            CCS_ItemDefinition stoneItem = EnsureStoneItem();
            CCS_ItemDefinition woodItem = EnsureWoodItem();
            CCS_ItemDefinition fiberItem = LoadRequiredAsset<CCS_ItemDefinition>(FiberItemPath);
            EnsureInventoryProfileCatalog(stickItem, stoneItem, woodItem, fiberItem);

            CCS_GatheringProfile profile = EnsureDefaultProfile(stickItem, stoneItem, woodItem, fiberItem);
            EnsureBootstrapPrefabProfile(profile);
            EnsureBootstrapTestArea(profile);

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} Gathering bootstrap setup complete.");
            EditorApplication.Exit(0);
        }

        #endregion

        #region Private Methods

        private static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder("Assets/CCS/Survival/Profiles/Gathering"))
            {
                AssetDatabase.CreateFolder("Assets/CCS/Survival/Profiles", "Gathering");
            }

            if (!AssetDatabase.IsValidFolder(ItemsRoot))
            {
                AssetDatabase.CreateFolder("Assets/CCS/Survival/Content/Items/Resources", "Primitive");
            }
        }

        private static T LoadRequiredAsset<T>(string assetPath) where T : Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (asset == null)
            {
                Debug.LogError($"{LogPrefix} Missing required asset: {assetPath}");
                EditorApplication.Exit(1);
            }

            return asset;
        }

        private static CCS_ItemDefinition EnsureStickItem()
        {
            return EnsureResourceItem(
                StickItemPath,
                "ccs.survival.item.resource.stick",
                "Stick",
                "Primitive stick gathered from trees and bushes.");
        }

        private static CCS_ItemDefinition EnsureStoneItem()
        {
            return EnsureResourceItem(
                StoneItemPath,
                "ccs.survival.item.resource.stone",
                "Stone",
                "Primitive stone gathered from rock nodes.");
        }

        private static CCS_ItemDefinition EnsureWoodItem()
        {
            return EnsureResourceItem(
                WoodItemPath,
                "ccs.survival.item.resource.wood",
                "Wood",
                "Primitive wood gathered from small trees.");
        }

        private static CCS_ItemDefinition EnsureResourceItem(
            string assetPath,
            string itemId,
            string displayName,
            string description)
        {
            CCS_ItemDefinition itemDefinition = AssetDatabase.LoadAssetAtPath<CCS_ItemDefinition>(assetPath);
            if (itemDefinition == null)
            {
                itemDefinition = ScriptableObject.CreateInstance<CCS_ItemDefinition>();
                AssetDatabase.CreateAsset(itemDefinition, assetPath);
            }

            SerializedObject serializedItem = new SerializedObject(itemDefinition);
            serializedItem.FindProperty("itemId").stringValue = itemId;
            serializedItem.FindProperty("displayName").stringValue = displayName;
            serializedItem.FindProperty("description").stringValue = description;
            serializedItem.FindProperty("category").enumValueIndex = (int)CCS_ItemCategory.Resource;
            serializedItem.FindProperty("maxStackSize").intValue = 50;
            serializedItem.FindProperty("isStackable").boolValue = true;
            serializedItem.FindProperty("weight").floatValue = 0.1f;
            serializedItem.FindProperty("hasToolIdentity").boolValue = false;
            serializedItem.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(itemDefinition);
            return itemDefinition;
        }

        private static void EnsureInventoryProfileCatalog(
            CCS_ItemDefinition stickItem,
            CCS_ItemDefinition stoneItem,
            CCS_ItemDefinition woodItem,
            CCS_ItemDefinition fiberItem)
        {
            CCS_InventoryProfile inventoryProfile = LoadRequiredAsset<CCS_InventoryProfile>(InventoryProfilePath);
            SerializedObject serializedProfile = new SerializedObject(inventoryProfile);
            SerializedProperty catalog = serializedProfile.FindProperty("saveRestoreItemDefinitions");
            AddCatalogEntryIfMissing(catalog, stickItem);
            AddCatalogEntryIfMissing(catalog, stoneItem);
            AddCatalogEntryIfMissing(catalog, woodItem);
            AddCatalogEntryIfMissing(catalog, fiberItem);
            serializedProfile.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(inventoryProfile);
        }

        private static void AddCatalogEntryIfMissing(SerializedProperty catalog, CCS_ItemDefinition itemDefinition)
        {
            if (itemDefinition == null)
            {
                return;
            }

            for (int index = 0; index < catalog.arraySize; index++)
            {
                if (catalog.GetArrayElementAtIndex(index).objectReferenceValue == itemDefinition)
                {
                    return;
                }
            }

            int newIndex = catalog.arraySize;
            catalog.InsertArrayElementAtIndex(newIndex);
            catalog.GetArrayElementAtIndex(newIndex).objectReferenceValue = itemDefinition;
        }

        private static CCS_GatheringProfile EnsureDefaultProfile(
            CCS_ItemDefinition stickItem,
            CCS_ItemDefinition stoneItem,
            CCS_ItemDefinition woodItem,
            CCS_ItemDefinition fiberItem)
        {
            CCS_GatheringProfile profile = AssetDatabase.LoadAssetAtPath<CCS_GatheringProfile>(DefaultProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CCS_GatheringProfile>();
                AssetDatabase.CreateAsset(profile, DefaultProfilePath);
            }

            SerializedObject serializedProfile = new SerializedObject(profile);
            serializedProfile.FindProperty("profileDisplayName").stringValue = "Default Gathering";
            serializedProfile.FindProperty("profileId").stringValue = "ccs.survival.profile.gathering.default";
            serializedProfile.FindProperty("profileDescription").stringValue =
                "Default primitive gathering rules for 0.9.9 resource foundation.";
            serializedProfile.FindProperty("profileVersion").stringValue = "0.9.9";
            serializedProfile.FindProperty("nodeInteractionDistance").floatValue = 3f;
            serializedProfile.FindProperty("gatherDurationSeconds").floatValue = 0f;
            serializedProfile.FindProperty("respawnEnabled").boolValue = true;
            serializedProfile.FindProperty("respawnDelaySeconds").floatValue = 30f;

            SerializedProperty catalog = serializedProfile.FindProperty("rewardItemCatalog");
            catalog.arraySize = 4;
            catalog.GetArrayElementAtIndex(0).objectReferenceValue = stickItem;
            catalog.GetArrayElementAtIndex(1).objectReferenceValue = stoneItem;
            catalog.GetArrayElementAtIndex(2).objectReferenceValue = woodItem;
            catalog.GetArrayElementAtIndex(3).objectReferenceValue = fiberItem;

            ApplyNodeRewards(serializedProfile.FindProperty("nodeRewardSettings"));
            serializedProfile.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            profile.BuildRewardLookup();
            return profile;
        }

        private static void ApplyNodeRewards(SerializedProperty nodeRewardSettings)
        {
            nodeRewardSettings.arraySize = 3;

            ApplyRewardEntry(
                nodeRewardSettings.GetArrayElementAtIndex(0),
                CCS_GatheringNodeType.SmallTree,
                new[]
                {
                    CreateReward(CCS_GatheringResourceType.Stick, "ccs.survival.item.resource.stick", 2),
                    CreateReward(CCS_GatheringResourceType.Wood, "ccs.survival.item.resource.wood", 1)
                });

            ApplyRewardEntry(
                nodeRewardSettings.GetArrayElementAtIndex(1),
                CCS_GatheringNodeType.Rock,
                new[]
                {
                    CreateReward(CCS_GatheringResourceType.Stone, "ccs.survival.item.resource.stone", 2)
                });

            ApplyRewardEntry(
                nodeRewardSettings.GetArrayElementAtIndex(2),
                CCS_GatheringNodeType.Bush,
                new[]
                {
                    CreateReward(CCS_GatheringResourceType.PlantFiber, "ccs.survival.item.resource.fiber", 2),
                    CreateReward(CCS_GatheringResourceType.Stick, "ccs.survival.item.resource.stick", 1)
                });
        }

        private static void ApplyRewardEntry(
            SerializedProperty settingsProperty,
            CCS_GatheringNodeType nodeType,
            CCS_GatheringReward[] rewards)
        {
            settingsProperty.FindPropertyRelative("nodeType").enumValueIndex = (int)nodeType;
            SerializedProperty rewardsProperty = settingsProperty.FindPropertyRelative("rewards");
            rewardsProperty.arraySize = rewards.Length;
            for (int index = 0; index < rewards.Length; index++)
            {
                SerializedProperty rewardProperty = rewardsProperty.GetArrayElementAtIndex(index);
                rewardProperty.FindPropertyRelative("resourceType").enumValueIndex = (int)rewards[index].resourceType;
                rewardProperty.FindPropertyRelative("itemDefinitionId").stringValue = rewards[index].itemDefinitionId;
                rewardProperty.FindPropertyRelative("amount").intValue = rewards[index].amount;
            }
        }

        private static CCS_GatheringReward CreateReward(
            CCS_GatheringResourceType resourceType,
            string itemDefinitionId,
            int amount)
        {
            return new CCS_GatheringReward
            {
                resourceType = resourceType,
                itemDefinitionId = itemDefinitionId,
                amount = amount
            };
        }

        private static void EnsureBootstrapPrefabProfile(CCS_GatheringProfile profile)
        {
            GameObject prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(BootstrapPrefabPath);
            if (prefabRoot == null)
            {
                Debug.LogError($"{LogPrefix} Missing bootstrap prefab: {BootstrapPrefabPath}");
                EditorApplication.Exit(1);
                return;
            }

            CCS_SurvivalGameplayServiceHost serviceHost = prefabRoot.GetComponent<CCS_SurvivalGameplayServiceHost>();
            if (serviceHost == null)
            {
                Debug.LogError($"{LogPrefix} Bootstrap prefab is missing CCS_SurvivalGameplayServiceHost.");
                EditorApplication.Exit(1);
                return;
            }

            SerializedObject serializedHost = new SerializedObject(serviceHost);
            serializedHost.FindProperty("gatheringProfile").objectReferenceValue = profile;
            serializedHost.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(prefabRoot);
        }

        private static void EnsureBootstrapTestArea(CCS_GatheringProfile profile)
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
                testAreaObject.transform.localPosition = new Vector3(8f, 0f, 4f);
                testArea = testAreaObject.transform;
            }

            EnsureGatheringNode(
                testArea,
                SmallTreeObjectName,
                CCS_GatheringNodeType.SmallTree,
                PrimitiveType.Cylinder,
                new Vector3(-2f, 1f, 0f),
                new Vector3(0.5f, 2f, 0.5f),
                profile);
            EnsureGatheringNode(
                testArea,
                RockObjectName,
                CCS_GatheringNodeType.Rock,
                PrimitiveType.Cube,
                new Vector3(0f, 0.5f, 0f),
                new Vector3(1f, 0.8f, 1f),
                profile);
            EnsureGatheringNode(
                testArea,
                BushObjectName,
                CCS_GatheringNodeType.Bush,
                PrimitiveType.Sphere,
                new Vector3(2f, 0.4f, 0f),
                new Vector3(0.9f, 0.8f, 0.9f),
                profile);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static Transform FindSceneRoot()
        {
            GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();
            for (int index = 0; index < roots.Length; index++)
            {
                if (roots[index].name == "CCS_BuildVerificationScene")
                {
                    return roots[index].transform;
                }
            }

            return null;
        }

        private static void EnsureGatheringNode(
            Transform parent,
            string objectName,
            CCS_GatheringNodeType nodeType,
            PrimitiveType primitiveType,
            Vector3 localPosition,
            Vector3 localScale,
            CCS_GatheringProfile profile)
        {
            Transform existing = parent.Find(objectName);
            GameObject nodeObject = existing != null ? existing.gameObject : GameObject.CreatePrimitive(primitiveType);
            nodeObject.name = objectName;
            nodeObject.transform.SetParent(parent, false);
            nodeObject.transform.localPosition = localPosition;
            nodeObject.transform.localScale = localScale;

            Rigidbody rigidbody = nodeObject.GetComponent<Rigidbody>();
            if (rigidbody != null)
            {
                Object.DestroyImmediate(rigidbody);
            }

            CCS_GatheringNode gatheringNode = nodeObject.GetComponent<CCS_GatheringNode>();
            if (gatheringNode == null)
            {
                gatheringNode = nodeObject.AddComponent<CCS_GatheringNode>();
            }

            gatheringNode.ConfigureFromProfile(profile, nodeType);

            CCS_GatheringInteractable interactable = nodeObject.GetComponent<CCS_GatheringInteractable>();
            if (interactable == null)
            {
                interactable = nodeObject.AddComponent<CCS_GatheringInteractable>();
            }

            interactable.ConfigureRuntime(profile);
            EditorUtility.SetDirty(nodeObject);
        }

        #endregion
    }
}
