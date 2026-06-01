using System.IO;
using CCS.Modules.Inventory;
using CCS.Modules.Wildlife;
using CCS.Modules.WorldResources;
using CCS.Survival.Composition;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_WildlifeBootstrapSetup
// CATEGORY: Modules / Wildlife / Editor / Validation
// PURPOSE: Creates default profile, test definitions, items, and bootstrap scene carcass placeholders.
// PLACEMENT: Batch entry for 0.9.3 wildlife resource foundation milestone.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Carcass placeholders only. No AI, combat, or spawning systems.
// =============================================================================

namespace CCS.Modules.Wildlife.Editor
{
    public static class CCS_WildlifeBootstrapSetup
    {
        private const string ProfilesRoot = "Assets/CCS/Survival/Profiles/Wildlife";
        private const string DefaultProfilePath = ProfilesRoot + "/CCS_DefaultWildlifeProfile.asset";
        private const string WildlifeItemsRoot = "Assets/CCS/Survival/Content/Items/Resources/Wildlife";
        private const string PrimitiveItemsRoot = "Assets/CCS/Survival/Content/Items/Resources/Primitive";
        private const string WildlifeDefinitionsRoot = "Assets/CCS/Survival/Content/Wildlife/Definitions";
        private const string BootstrapScenePath = "Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string BootstrapPrefabPath = "Assets/CCS/Survival/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
        private const string TestAreaObjectName = "CCS_WildlifeTestArea";
        private const string RabbitCarcassObjectName = "CCS_TestRabbitCarcass";
        private const string DeerCarcassObjectName = "CCS_TestDeerCarcass";
        private const string LogPrefix = "[CCS_WildlifeBootstrapSetup]";

        #region Public Methods

        public static void ExecuteBatch()
        {
            EnsureFolders();
            CCS_WildlifeProfile profile = EnsureDefaultProfile();
            CCS_ItemDefinition boneItem = EnsureExistingItem(
                PrimitiveItemsRoot + "/CCS_Item_Bone.asset",
                "ccs.survival.item.resource.bone",
                "Bone");
            CCS_ItemDefinition hideItem = EnsureExistingItem(
                PrimitiveItemsRoot + "/CCS_Item_Hide.asset",
                "ccs.survival.item.resource.hide",
                "Hide");
            CCS_ItemDefinition sinewItem = EnsureExistingItem(
                PrimitiveItemsRoot + "/CCS_Item_Sinew.asset",
                "ccs.survival.item.resource.sinew",
                "Sinew");
            CCS_ItemDefinition rawMeatItem = EnsureWildlifeItem(
                "CCS_Item_RawMeat",
                "ccs.survival.item.resource.rawmeat",
                "Raw Meat");

            CCS_WildlifeDefinition rabbitDefinition = EnsureTestWildlifeDefinition(
                "CCS_TestRabbit",
                "ccs.survival.wildlife.test.rabbit",
                "Test Rabbit Carcass",
                CCS_WildlifeType.SmallGame,
                CCS_RequiredToolType.Knife,
                1,
                new[]
                {
                    CreateDrop(rawMeatItem, 1, 1),
                    CreateDrop(hideItem, 1, 1),
                    CreateDrop(boneItem, 1, 1)
                });

            CCS_WildlifeDefinition deerDefinition = EnsureTestWildlifeDefinition(
                "CCS_TestDeerCarcass",
                "ccs.survival.wildlife.test.deercarcass",
                "Test Deer Carcass",
                CCS_WildlifeType.Deer,
                CCS_RequiredToolType.Knife,
                1,
                new[]
                {
                    CreateDrop(rawMeatItem, 3, 3),
                    CreateDrop(hideItem, 2, 2),
                    CreateDrop(boneItem, 2, 2),
                    CreateDrop(sinewItem, 1, 1)
                });

            EnsureBootstrapPrefabProfile(profile);
            EnsureBootstrapTestArea(profile, rabbitDefinition, deerDefinition);

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} Wildlife bootstrap setup complete.");
            EditorApplication.Exit(0);
        }

        #endregion

        #region Private Methods

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/CCS/Survival/Profiles");
            EnsureFolder(ProfilesRoot);
            EnsureFolder("Assets/CCS/Survival/Content/Wildlife");
            EnsureFolder(WildlifeDefinitionsRoot);
            EnsureFolder(WildlifeItemsRoot);
        }

        private static CCS_WildlifeProfile EnsureDefaultProfile()
        {
            CCS_WildlifeProfile profile = AssetDatabase.LoadAssetAtPath<CCS_WildlifeProfile>(DefaultProfilePath);
            if (profile != null)
            {
                SerializedObject serializedProfile = new SerializedObject(profile);
                serializedProfile.FindProperty("profileVersion").stringValue = "0.9.3";
                serializedProfile.ApplyModifiedPropertiesWithoutUndo();
                return profile;
            }

            profile = ScriptableObject.CreateInstance<CCS_WildlifeProfile>();
            SerializedObject newProfile = new SerializedObject(profile);
            newProfile.FindProperty("profileDisplayName").stringValue = "Default Wildlife";
            newProfile.FindProperty("profileId").stringValue = "ccs.survival.profile.wildlife.default";
            newProfile.FindProperty("profileDescription").stringValue =
                "Default wildlife carcass harvesting rules for 0.9.3 foundation.";
            newProfile.FindProperty("profileVersion").stringValue = "0.9.3";
            newProfile.FindProperty("enableCarcassHarvesting").boolValue = true;
            newProfile.FindProperty("defaultHarvestCount").intValue = 1;
            newProfile.FindProperty("enableRespawnPlaceholder").boolValue = false;
            newProfile.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.CreateAsset(profile, DefaultProfilePath);
            return profile;
        }

        private static CCS_ItemDefinition EnsureExistingItem(string assetPath, string itemId, string displayName)
        {
            CCS_ItemDefinition itemDefinition = AssetDatabase.LoadAssetAtPath<CCS_ItemDefinition>(assetPath);
            if (itemDefinition != null)
            {
                return itemDefinition;
            }

            itemDefinition = ScriptableObject.CreateInstance<CCS_ItemDefinition>();
            SerializedObject serializedItem = new SerializedObject(itemDefinition);
            serializedItem.FindProperty("itemId").stringValue = itemId;
            serializedItem.FindProperty("displayName").stringValue = displayName;
            serializedItem.FindProperty("description").stringValue = "Wildlife resource placeholder item.";
            serializedItem.FindProperty("maxStackSize").intValue = 50;
            serializedItem.FindProperty("isStackable").boolValue = true;
            serializedItem.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(itemDefinition, assetPath);
            return itemDefinition;
        }

        private static CCS_ItemDefinition EnsureWildlifeItem(string assetName, string itemId, string displayName)
        {
            string assetPath = $"{WildlifeItemsRoot}/{assetName}.asset";
            CCS_ItemDefinition itemDefinition = AssetDatabase.LoadAssetAtPath<CCS_ItemDefinition>(assetPath);
            if (itemDefinition != null)
            {
                return itemDefinition;
            }

            itemDefinition = ScriptableObject.CreateInstance<CCS_ItemDefinition>();
            SerializedObject serializedItem = new SerializedObject(itemDefinition);
            serializedItem.FindProperty("itemId").stringValue = itemId;
            serializedItem.FindProperty("displayName").stringValue = displayName;
            serializedItem.FindProperty("description").stringValue =
                "Raw meat wildlife resource placeholder. No cooking or nutrition logic yet.";
            serializedItem.FindProperty("maxStackSize").intValue = 50;
            serializedItem.FindProperty("isStackable").boolValue = true;
            serializedItem.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(itemDefinition, assetPath);
            return itemDefinition;
        }

        private static (CCS_ItemDefinition item, int min, int max) CreateDrop(
            CCS_ItemDefinition item,
            int minQuantity,
            int maxQuantity)
        {
            return (item, minQuantity, maxQuantity);
        }

        private static CCS_WildlifeDefinition EnsureTestWildlifeDefinition(
            string assetName,
            string wildlifeId,
            string displayName,
            CCS_WildlifeType wildlifeType,
            CCS_RequiredToolType requiredToolType,
            int maxHarvestCount,
            (CCS_ItemDefinition item, int min, int max)[] drops)
        {
            string assetPath = $"{WildlifeDefinitionsRoot}/{assetName}.asset";
            CCS_WildlifeDefinition wildlifeDefinition =
                AssetDatabase.LoadAssetAtPath<CCS_WildlifeDefinition>(assetPath);

            if (wildlifeDefinition == null)
            {
                wildlifeDefinition = ScriptableObject.CreateInstance<CCS_WildlifeDefinition>();
                AssetDatabase.CreateAsset(wildlifeDefinition, assetPath);
            }

            SerializedObject serializedDefinition = new SerializedObject(wildlifeDefinition);
            serializedDefinition.FindProperty("wildlifeId").stringValue = wildlifeId;
            serializedDefinition.FindProperty("displayName").stringValue = displayName;
            serializedDefinition.FindProperty("wildlifeType").enumValueIndex = (int)wildlifeType;
            serializedDefinition.FindProperty("harvestToolRequirement").enumValueIndex = (int)requiredToolType;
            serializedDefinition.FindProperty("maxHarvestCount").intValue = maxHarvestCount;
            serializedDefinition.FindProperty("respawnTimeSeconds").floatValue = 0f;
            serializedDefinition.FindProperty("isAggressive").boolValue = false;

            SerializedProperty harvestDrops = serializedDefinition.FindProperty("harvestDrops");
            harvestDrops.arraySize = drops.Length;
            for (int index = 0; index < drops.Length; index++)
            {
                SerializedProperty dropEntry = harvestDrops.GetArrayElementAtIndex(index);
                dropEntry.FindPropertyRelative("itemDefinition").objectReferenceValue = drops[index].item;
                dropEntry.FindPropertyRelative("minQuantity").intValue = drops[index].min;
                dropEntry.FindPropertyRelative("maxQuantity").intValue = drops[index].max;
            }

            serializedDefinition.ApplyModifiedPropertiesWithoutUndo();
            return wildlifeDefinition;
        }

        private static void EnsureBootstrapPrefabProfile(CCS_WildlifeProfile profile)
        {
            GameObject prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(BootstrapPrefabPath);
            if (prefabRoot == null)
            {
                Debug.LogError($"{LogPrefix} Missing bootstrap prefab: {BootstrapPrefabPath}");
                EditorApplication.Exit(1);
                return;
            }

            CCS_SurvivalGameplayServiceHost serviceHost =
                prefabRoot.GetComponent<CCS_SurvivalGameplayServiceHost>();

            if (serviceHost == null)
            {
                Debug.LogError($"{LogPrefix} Bootstrap prefab is missing CCS_SurvivalGameplayServiceHost.");
                EditorApplication.Exit(1);
                return;
            }

            SerializedObject serializedHost = new SerializedObject(serviceHost);
            serializedHost.FindProperty("wildlifeProfile").objectReferenceValue = profile;
            serializedHost.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(prefabRoot);
        }

        private static void EnsureBootstrapTestArea(
            CCS_WildlifeProfile profile,
            CCS_WildlifeDefinition rabbitDefinition,
            CCS_WildlifeDefinition deerDefinition)
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
                testAreaObject.transform.localPosition = new Vector3(4f, 0f, 4f);
                testArea = testAreaObject.transform;
            }

            EnsureHarvestableWildlife(
                testArea,
                RabbitCarcassObjectName,
                PrimitiveType.Sphere,
                new Vector3(-1f, 0.35f, 0f),
                new Vector3(0.5f, 0.5f, 0.5f),
                rabbitDefinition,
                profile);

            EnsureHarvestableWildlife(
                testArea,
                DeerCarcassObjectName,
                PrimitiveType.Capsule,
                new Vector3(1.5f, 0.75f, 0f),
                new Vector3(0.8f, 1.5f, 0.8f),
                deerDefinition,
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

        private static void EnsureHarvestableWildlife(
            Transform parent,
            string objectName,
            PrimitiveType primitiveType,
            Vector3 localPosition,
            Vector3 localScale,
            CCS_WildlifeDefinition wildlifeDefinition,
            CCS_WildlifeProfile profile)
        {
            Transform existing = parent.Find(objectName);
            GameObject wildlifeObject;

            if (existing != null)
            {
                wildlifeObject = existing.gameObject;
            }
            else
            {
                wildlifeObject = GameObject.CreatePrimitive(primitiveType);
                wildlifeObject.name = objectName;
                wildlifeObject.transform.SetParent(parent, false);
            }

            wildlifeObject.transform.localPosition = localPosition;
            wildlifeObject.transform.localScale = localScale;

            Collider collider = wildlifeObject.GetComponent<Collider>();
            if (collider != null)
            {
                collider.isTrigger = false;
                collider.enabled = true;
            }

            CCS_HarvestableWildlife harvestable = wildlifeObject.GetComponent<CCS_HarvestableWildlife>();
            if (harvestable == null)
            {
                harvestable = wildlifeObject.AddComponent<CCS_HarvestableWildlife>();
            }

            SerializedObject serializedHarvestable = new SerializedObject(harvestable);
            serializedHarvestable.FindProperty("wildlifeDefinition").objectReferenceValue = wildlifeDefinition;
            serializedHarvestable.FindProperty("wildlifeProfile").objectReferenceValue = profile;
            serializedHarvestable.FindProperty("assumeRequiredToolEquipped").boolValue = false;
            serializedHarvestable.FindProperty("interactionDistance").floatValue = 3f;
            serializedHarvestable.ApplyModifiedPropertiesWithoutUndo();
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
