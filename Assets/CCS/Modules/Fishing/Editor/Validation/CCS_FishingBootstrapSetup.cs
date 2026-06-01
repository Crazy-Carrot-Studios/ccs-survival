using System.IO;
using CCS.Modules.Equipment;
using CCS.Modules.Fishing;
using CCS.Modules.Inventory;
using CCS.Modules.Resources;
using CCS.Survival.Composition;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_FishingBootstrapSetup
// CATEGORY: Modules / Fishing / Editor / Validation
// PURPOSE: Creates fishing profile, frontier fishing items, test spot, and host wiring.
// PLACEMENT: Batch entry for milestone 1.2.5 fishing foundation.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: PF_CCS_TestFishingSpot + CCS_TestFishingSpot in bootstrap scene.
// =============================================================================

namespace CCS.Modules.Fishing.Editor
{
    public static class CCS_FishingBootstrapSetup
    {
        private const string ProfilesRoot = "Assets/CCS/Survival/Profiles/Fishing";
        private const string DefaultProfilePath = ProfilesRoot + "/CCS_DefaultFishingProfile.asset";
        private const string SpotDefinitionsRoot = "Assets/CCS/Survival/Content/Fishing/Spots";
        private const string TestSpotDefinitionPath = SpotDefinitionsRoot + "/CCS_FishingSpotDefinition_TestPond.asset";
        private const string ItemsRoot = "Assets/CCS/Survival/Content/Items/Fishing";
        private const string ToolsRoot = "Assets/CCS/Survival/Content/Items/Tools/Fishing";
        private const string PrefabsRoot = "Assets/CCS/Survival/Prefabs/Fishing";
        private const string TestSpotPrefabPath = PrefabsRoot + "/PF_CCS_TestFishingSpot.prefab";
        private const string InventoryProfilePath = "Assets/CCS/Survival/Profiles/Inventory/CCS_DefaultInventoryProfile.asset";
        private const string EquipmentVisualProfilePath =
            "Assets/CCS/Survival/Profiles/Equipment/CCS_DefaultEquipmentVisualProfile.asset";
        private const string BootstrapPrefabPath = "Assets/CCS/Survival/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
        private const string BootstrapScenePath = "Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string TestAreaObjectName = "CCS_FishingTestArea";
        private const string TestSpotObjectName = "CCS_TestFishingSpot";
        private const string LogPrefix = "[CCS_FishingBootstrapSetup]";

        public static void ExecuteBatch()
        {
            EnsureFolders();

            CCS_ItemDefinition crudeHook = EnsureMaterialItem(
                $"{ItemsRoot}/CCS_Item_CrudeHook.asset",
                "ccs.survival.item.tool.crudehook",
                "Crude Hook",
                "Primitive hook for frontier fishing rigs.");
            CCS_ItemDefinition fishingLine = EnsureMaterialItem(
                $"{ItemsRoot}/CCS_Item_FishingLine.asset",
                "ccs.survival.item.tool.fishingline",
                "Fishing Line",
                "Twisted plant fiber line for primitive fishing.");
            CCS_ItemDefinition bait = EnsureConsumableItem(
                $"{ItemsRoot}/CCS_Item_Bait.asset",
                "ccs.survival.item.consumable.bait",
                "Bait");
            CCS_ItemDefinition rawFish = EnsureResourceFishItem(
                $"{ItemsRoot}/CCS_Item_RawFish.asset",
                "ccs.survival.item.resource.rawfish",
                "Raw Fish");
            CCS_ItemDefinition smallFish = EnsureResourceFishItem(
                $"{ItemsRoot}/CCS_Item_SmallFish.asset",
                "ccs.survival.item.resource.smallfish",
                "Small Fish");
            CCS_ItemDefinition junk = EnsureResourceFishItem(
                $"{ItemsRoot}/CCS_Item_Junk.asset",
                "ccs.survival.item.resource.junk",
                "Junk");
            CCS_ItemDefinition fishingPole = EnsureFishingPoleItem(
                $"{ToolsRoot}/CCS_Item_FishingPole.asset",
                "ccs.survival.item.tool.fishingpole",
                "Fishing Pole");

            EnsureInventoryCatalog(
                crudeHook,
                fishingLine,
                bait,
                rawFish,
                smallFish,
                junk,
                fishingPole);
            EnsureEquipmentVisual(fishingPole);

            CCS_FishingSpotDefinition spotDefinition = EnsureTestSpotDefinition();
            CCS_FishingProfile profile = EnsureDefaultProfile(
                fishingPole,
                rawFish,
                smallFish,
                junk,
                bait,
                crudeHook,
                fishingLine);
            EnsureBootstrapHost(profile);
            EnsureTestSpotPrefab(spotDefinition);
            EnsureBootstrapSceneTestSpot(spotDefinition);

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} Fishing bootstrap setup complete.");
            EditorApplication.Exit(0);
        }

        private static void EnsureFolders()
        {
            CreateFolderChain("Assets/CCS/Survival/Profiles", "Fishing");
            CreateFolderChain("Assets/CCS/Survival/Content", "Fishing/Spots");
            CreateFolderChain("Assets/CCS/Survival/Content/Items", "Fishing");
            CreateFolderChain("Assets/CCS/Survival/Content/Items/Tools", "Fishing");
            CreateFolderChain("Assets/CCS/Survival/Prefabs", "Fishing");
        }

        private static void CreateFolderChain(string parent, string chain)
        {
            string[] parts = chain.Split('/');
            string current = parent;
            for (int index = 0; index < parts.Length; index++)
            {
                string next = current + "/" + parts[index];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[index]);
                }

                current = next;
            }
        }

        private static CCS_ItemDefinition EnsureFishingPoleItem(string assetPath, string itemId, string displayName)
        {
            CCS_ItemDefinition item = EnsureGenericItem(
                assetPath,
                itemId,
                displayName,
                "Primitive frontier fishing pole. Use on fishable water sources.",
                CCS_ItemCategory.Tool,
                1,
                1.2f,
                CCS_ItemGameplayKind.Tool);
            SerializedObject serialized = new SerializedObject(item);
            serialized.FindProperty("hasToolIdentity").boolValue = true;
            serialized.FindProperty("toolType").intValue = (int)CCS_ItemToolType.FishingPole;
            serialized.FindProperty("toolArchetype").intValue = (int)CCS_ToolArchetype.FishingPole;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(item);
            return item;
        }

        private static CCS_ItemDefinition EnsureMaterialItem(
            string assetPath,
            string itemId,
            string displayName,
            string description)
        {
            return EnsureGenericItem(
                assetPath,
                itemId,
                displayName,
                description,
                CCS_ItemCategory.Material,
                20,
                0.1f,
                CCS_ItemGameplayKind.Generic);
        }

        private static CCS_ItemDefinition EnsureConsumableItem(string assetPath, string itemId, string displayName)
        {
            return EnsureGenericItem(
                assetPath,
                itemId,
                displayName,
                "Primitive bait for frontier fishing.",
                CCS_ItemCategory.Consumable,
                20,
                0.05f,
                CCS_ItemGameplayKind.Generic);
        }

        private static CCS_ItemDefinition EnsureResourceFishItem(string assetPath, string itemId, string displayName)
        {
            return EnsureGenericItem(
                assetPath,
                itemId,
                displayName,
                "Frontier fishing catch placeholder.",
                CCS_ItemCategory.Material,
                20,
                0.3f,
                CCS_ItemGameplayKind.Generic);
        }

        private static CCS_ItemDefinition EnsureGenericItem(
            string assetPath,
            string itemId,
            string displayName,
            string description,
            CCS_ItemCategory category,
            int maxStack,
            float weight,
            CCS_ItemGameplayKind gameplayKind)
        {
            CCS_ItemDefinition item = AssetDatabase.LoadAssetAtPath<CCS_ItemDefinition>(assetPath);
            if (item == null)
            {
                item = ScriptableObject.CreateInstance<CCS_ItemDefinition>();
                AssetDatabase.CreateAsset(item, assetPath);
            }

            SerializedObject serialized = new SerializedObject(item);
            serialized.FindProperty("itemId").stringValue = itemId;
            serialized.FindProperty("displayName").stringValue = displayName;
            serialized.FindProperty("description").stringValue = description;
            serialized.FindProperty("category").intValue = (int)category;
            serialized.FindProperty("maxStackSize").intValue = maxStack;
            serialized.FindProperty("weight").floatValue = weight;
            serialized.FindProperty("gameplayKind").intValue = (int)gameplayKind;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(item);
            return item;
        }

        private static void EnsureInventoryCatalog(params CCS_ItemDefinition[] items)
        {
            CCS_InventoryProfile profile = AssetDatabase.LoadAssetAtPath<CCS_InventoryProfile>(InventoryProfilePath);
            if (profile == null)
            {
                Debug.LogError($"{LogPrefix} Missing inventory profile.");
                EditorApplication.Exit(1);
                return;
            }

            SerializedObject serialized = new SerializedObject(profile);
            SerializedProperty catalog = serialized.FindProperty("saveRestoreItemDefinitions");
            AppendUniqueItems(catalog, items);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static void AppendUniqueItems(SerializedProperty catalog, CCS_ItemDefinition[] items)
        {
            if (catalog == null)
            {
                Debug.LogError($"{LogPrefix} Inventory profile is missing saveRestoreItemDefinitions.");
                EditorApplication.Exit(1);
                return;
            }

            for (int itemIndex = 0; itemIndex < items.Length; itemIndex++)
            {
                CCS_ItemDefinition item = items[itemIndex];
                if (item == null)
                {
                    continue;
                }

                bool exists = false;
                for (int index = 0; index < catalog.arraySize; index++)
                {
                    if (catalog.GetArrayElementAtIndex(index).objectReferenceValue == item)
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                {
                    int newIndex = catalog.arraySize;
                    catalog.InsertArrayElementAtIndex(newIndex);
                    catalog.GetArrayElementAtIndex(newIndex).objectReferenceValue = item;
                }
            }
        }

        private static void EnsureEquipmentVisual(CCS_ItemDefinition fishingPole)
        {
            CCS_EquipmentVisualProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_EquipmentVisualProfile>(EquipmentVisualProfilePath);
            if (profile == null || fishingPole == null)
            {
                return;
            }

            string definitionPath = "Assets/CCS/Survival/Content/Equipment/Visuals/CCS_EquipmentVisual_FishingPole.asset";
            CCS_EquipmentVisualDefinition definition =
                AssetDatabase.LoadAssetAtPath<CCS_EquipmentVisualDefinition>(definitionPath);
            GameObject genericPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/CCS/Survival/Prefabs/Equipment/Visuals/PF_CCS_Visual_GenericTool.prefab");

            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<CCS_EquipmentVisualDefinition>();
                AssetDatabase.CreateAsset(definition, definitionPath);
            }

            SerializedObject serializedDefinition = new SerializedObject(definition);
            serializedDefinition.FindProperty("itemId").stringValue = fishingPole.ItemId;
            serializedDefinition.FindProperty("visualPrefab").objectReferenceValue = genericPrefab;
            serializedDefinition.FindProperty("attachmentSocket").intValue =
                (int)CCS_EquipmentAttachmentSocketType.RightHand;
            serializedDefinition.FindProperty("localPositionOffset").vector3Value = new Vector3(0f, 0f, 0.1f);
            serializedDefinition.FindProperty("localEulerOffset").vector3Value = new Vector3(15f, 0f, 90f);
            serializedDefinition.FindProperty("localScale").vector3Value = new Vector3(0.08f, 0.6f, 0.08f);
            serializedDefinition.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);

            SerializedObject serializedProfile = new SerializedObject(profile);
            SerializedProperty definitions = serializedProfile.FindProperty("visualDefinitions");
            bool exists = false;
            for (int index = 0; index < definitions.arraySize; index++)
            {
                if (definitions.GetArrayElementAtIndex(index).objectReferenceValue == definition)
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                int newIndex = definitions.arraySize;
                definitions.InsertArrayElementAtIndex(newIndex);
                definitions.GetArrayElementAtIndex(newIndex).objectReferenceValue = definition;
            }

            serializedProfile.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static CCS_FishingSpotDefinition EnsureTestSpotDefinition()
        {
            CCS_FishingSpotDefinition definition =
                AssetDatabase.LoadAssetAtPath<CCS_FishingSpotDefinition>(TestSpotDefinitionPath);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<CCS_FishingSpotDefinition>();
                AssetDatabase.CreateAsset(definition, TestSpotDefinitionPath);
            }

            SerializedObject serialized = new SerializedObject(definition);
            serialized.FindProperty("spotId").stringValue = "ccs.survival.fishing.spot.testpond";
            serialized.FindProperty("displayName").stringValue = "Test Pond";
            serialized.FindProperty("waterBodyType").intValue = (int)CCS_FishingWaterBodyType.Pond;
            serialized.FindProperty("resourceSourceType").intValue = (int)CCS_ResourceSourceType.Water;
            serialized.FindProperty("harvestMethod").intValue = (int)CCS_HarvestMethodType.Fish;
            serialized.FindProperty("requiredToolType").intValue = (int)CCS_ItemToolType.FishingPole;
            SerializedProperty bait = serialized.FindProperty("baitRequirement");
            bait.FindPropertyRelative("requireBait").boolValue = false;
            serialized.FindProperty("interactionDistance").floatValue = 4f;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static CCS_FishingProfile EnsureDefaultProfile(params CCS_ItemDefinition[] catalog)
        {
            CCS_FishingProfile profile = AssetDatabase.LoadAssetAtPath<CCS_FishingProfile>(DefaultProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CCS_FishingProfile>();
                AssetDatabase.CreateAsset(profile, DefaultProfilePath);
            }

            SerializedObject serialized = new SerializedObject(profile);
            serialized.FindProperty("profileVersion").stringValue = "1.2.5";
            serialized.FindProperty("profileDescription").stringValue =
                "Frontier fishing foundation profile (1.2.5).";
            serialized.FindProperty("defaultInteractionDistance").floatValue = 4f;
            SerializedProperty itemCatalog = serialized.FindProperty("itemCatalog");
            itemCatalog.arraySize = catalog.Length;
            for (int index = 0; index < catalog.Length; index++)
            {
                itemCatalog.GetArrayElementAtIndex(index).objectReferenceValue = catalog[index];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static void EnsureBootstrapHost(CCS_FishingProfile profile)
        {
            GameObject prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(BootstrapPrefabPath);
            if (prefabRoot == null)
            {
                Debug.LogError($"{LogPrefix} Missing bootstrap prefab.");
                EditorApplication.Exit(1);
                return;
            }

            CCS_SurvivalGameplayServiceHost host = prefabRoot.GetComponent<CCS_SurvivalGameplayServiceHost>();
            if (host == null)
            {
                Debug.LogError($"{LogPrefix} Missing gameplay service host.");
                EditorApplication.Exit(1);
                return;
            }

            SerializedObject serializedHost = new SerializedObject(host);
            serializedHost.FindProperty("fishingProfile").objectReferenceValue = profile;
            serializedHost.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(prefabRoot);
        }

        private static void EnsureTestSpotPrefab(CCS_FishingSpotDefinition spotDefinition)
        {
            GameObject spotObject = AssetDatabase.LoadAssetAtPath<GameObject>(TestSpotPrefabPath);
            if (spotObject == null)
            {
                spotObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                spotObject.name = "PF_CCS_TestFishingSpot";
                spotObject.transform.localScale = new Vector3(2.5f, 0.2f, 2.5f);
                Rigidbody rigidbody = spotObject.GetComponent<Rigidbody>();
                if (rigidbody != null)
                {
                    Object.DestroyImmediate(rigidbody);
                }

                CCS_FishingSpot fishingSpot = spotObject.AddComponent<CCS_FishingSpot>();
                fishingSpot.ConfigureRuntime(spotDefinition);
                PrefabUtility.SaveAsPrefabAsset(spotObject, TestSpotPrefabPath);
                Object.DestroyImmediate(spotObject);
                return;
            }

            CCS_FishingSpot existingSpot = spotObject.GetComponent<CCS_FishingSpot>();
            if (existingSpot != null)
            {
                existingSpot.ConfigureRuntime(spotDefinition);
                EditorUtility.SetDirty(spotObject);
            }
        }

        private static void EnsureBootstrapSceneTestSpot(CCS_FishingSpotDefinition spotDefinition)
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
                GameObject areaObject = new GameObject(TestAreaObjectName);
                areaObject.transform.SetParent(sceneRoot, false);
                areaObject.transform.localPosition = new Vector3(10f, 0f, 8f);
                testArea = areaObject.transform;
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(TestSpotPrefabPath);
            Transform existing = testArea.Find(TestSpotObjectName);
            GameObject spotInstance;
            if (existing != null)
            {
                spotInstance = existing.gameObject;
            }
            else if (prefab != null)
            {
                spotInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, testArea);
                spotInstance.name = TestSpotObjectName;
            }
            else
            {
                spotInstance = GameObject.CreatePrimitive(PrimitiveType.Cube);
                spotInstance.name = TestSpotObjectName;
                spotInstance.transform.SetParent(testArea, false);
            }

            spotInstance.transform.localPosition = Vector3.zero;
            CCS_FishingSpot fishingSpot = spotInstance.GetComponent<CCS_FishingSpot>();
            if (fishingSpot == null)
            {
                fishingSpot = spotInstance.AddComponent<CCS_FishingSpot>();
            }

            fishingSpot.ConfigureRuntime(spotDefinition);
            EditorUtility.SetDirty(spotInstance);
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
    }
}
