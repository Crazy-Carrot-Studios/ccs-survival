using System.Collections.Generic;
using CCS.Modules.Gathering;
using CCS.Modules.Inventory;
using CCS.Modules.Resources;
using CCS.Modules.WorldResources;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_FrontierResourceFrameworkBootstrapSetup
// CATEGORY: Modules / Resources / Editor / Bootstrap
// PURPOSE: Creates frontier resource items, gathering metadata, and definition assets.
// PLACEMENT: Batch entry for milestone 1.2.4 frontier resource framework audit.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Does not spawn terrain clutter. Prepares practical source definitions only.
// =============================================================================

namespace CCS.Modules.Resources.Editor
{
    public static class CCS_FrontierResourceFrameworkBootstrapSetup
    {
        private const string FrontierItemsRoot = "Assets/CCS/Survival/Content/Items/Resources/Frontier";
        private const string FrontierWorldResourcesRoot = "Assets/CCS/Survival/Profiles/WorldResources/Frontier";
        private const string GatheringProfilePath = "Assets/CCS/Survival/Profiles/Gathering/CCS_DefaultGatheringProfile.asset";
        private const string BootstrapScenePath = "Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string FrontierTestAreaName = "CCS_FrontierResourceTestArea";
        private const string LogPrefix = "[CCS_FrontierResourceFrameworkBootstrapSetup]";

        public static void ExecuteBatch()
        {
            EnsureFolders();

            CCS_ItemDefinition wood = LoadOrCreateItem(
                "Assets/CCS/Survival/Content/Items/Resources/Primitive/CCS_Item_Wood.asset",
                "CCS_Item_Wood",
                "ccs.survival.item.resource.wood",
                "Wood");
            CCS_ItemDefinition stick = LoadOrCreateItem(
                "Assets/CCS/Survival/Content/Items/Resources/Primitive/CCS_Item_Stick.asset",
                "CCS_Item_Stick",
                "ccs.survival.item.resource.stick",
                "Stick");
            CCS_ItemDefinition stone = LoadOrCreateItem(
                "Assets/CCS/Survival/Content/Items/Resources/Primitive/CCS_Item_Stone.asset",
                "CCS_Item_Stone",
                "ccs.survival.item.resource.stone",
                "Stone");
            CCS_ItemDefinition fiber = LoadOrCreateItem(
                "Assets/CCS/Survival/Content/Items/Resources/Primitive/CCS_Item_Fiber.asset",
                "CCS_Item_PlantFiber",
                "ccs.survival.item.resource.fiber",
                "Plant Fiber");
            CCS_ItemDefinition bark = EnsureItem("CCS_Item_Bark", "ccs.survival.item.resource.bark", "Bark");
            CCS_ItemDefinition sapling = EnsureItem("CCS_Item_Sapling", "ccs.survival.item.resource.sapling", "Sapling");
            CCS_ItemDefinition flint = EnsureItem("CCS_Item_Flint", "ccs.survival.item.resource.flint", "Flint");
            CCS_ItemDefinition clay = EnsureItem("CCS_Item_Clay", "ccs.survival.item.resource.clay", "Clay");
            CCS_ItemDefinition coal = EnsureItem("CCS_Item_Coal", "ccs.survival.item.resource.coal", "Coal");
            CCS_ItemDefinition ore = EnsureItem("CCS_Item_Ore", "ccs.survival.item.resource.ore", "Ore");
            CCS_ItemDefinition scrapIron = EnsureItem("CCS_Item_ScrapIron", "ccs.survival.item.resource.scrapiron", "Scrap Iron");
            CCS_ItemDefinition nails = EnsureItem("CCS_Item_Nails", "ccs.survival.item.resource.nails", "Nails");
            CCS_ItemDefinition canvas = EnsureItem("CCS_Item_Canvas", "ccs.survival.item.resource.canvas", "Canvas");
            CCS_ItemDefinition water = EnsureItem("CCS_Item_Water", "ccs.survival.item.resource.water", "Water");

            EnsureGatheringProfileMetadata(
                wood,
                stick,
                stone,
                fiber,
                bark,
                sapling,
                flint,
                clay,
                coal,
                ore,
                scrapIron,
                nails,
                canvas,
                water);

            EnsureFrontierWorldResourceDefinitions(
                wood,
                bark,
                stone,
                fiber,
                flint,
                clay,
                coal,
                ore,
                scrapIron,
                nails,
                canvas,
                water);

            EnsureFrontierGatheringTestNodes();
            PatchLegacyWorldTestDefinitions();

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} Frontier resource framework bootstrap complete.");
            EditorApplication.Exit(0);
        }

        private static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder("Assets/CCS/Survival/Content/Items/Resources/Frontier"))
            {
                AssetDatabase.CreateFolder("Assets/CCS/Survival/Content/Items/Resources", "Frontier");
            }

            if (!AssetDatabase.IsValidFolder(FrontierWorldResourcesRoot))
            {
                AssetDatabase.CreateFolder("Assets/CCS/Survival/Profiles/WorldResources", "Frontier");
            }
        }

        private static CCS_ItemDefinition LoadOrCreateItem(
            string existingPath,
            string frontierAssetName,
            string itemId,
            string displayName)
        {
            CCS_ItemDefinition existing = AssetDatabase.LoadAssetAtPath<CCS_ItemDefinition>(existingPath);
            if (existing != null)
            {
                return existing;
            }

            return EnsureItem(frontierAssetName, itemId, displayName);
        }

        private static CCS_ItemDefinition EnsureItem(string assetName, string itemId, string displayName)
        {
            string path = $"{FrontierItemsRoot}/{assetName}.asset";
            CCS_ItemDefinition item = AssetDatabase.LoadAssetAtPath<CCS_ItemDefinition>(path);
            if (item != null)
            {
                return item;
            }

            item = ScriptableObject.CreateInstance<CCS_ItemDefinition>();
            SerializedObject serialized = new SerializedObject(item);
            serialized.FindProperty("itemId").stringValue = itemId;
            serialized.FindProperty("displayName").stringValue = displayName;
            serialized.FindProperty("description").stringValue =
                "Frontier resource material for practical source harvesting.";
            serialized.FindProperty("maxStackSize").intValue = 99;
            serialized.FindProperty("isStackable").boolValue = true;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(item, path);
            return item;
        }

        private static void EnsureGatheringProfileMetadata(
            params CCS_ItemDefinition[] catalogItems)
        {
            CCS_GatheringProfile profile = AssetDatabase.LoadAssetAtPath<CCS_GatheringProfile>(GatheringProfilePath);
            if (profile == null)
            {
                Debug.LogError($"{LogPrefix} Missing gathering profile: {GatheringProfilePath}");
                EditorApplication.Exit(1);
                return;
            }

            SerializedObject serializedProfile = new SerializedObject(profile);
            serializedProfile.FindProperty("profileVersion").stringValue = "1.2.4";
            serializedProfile.FindProperty("profileDescription").stringValue =
                "Gathering profile with frontier resource source and harvest metadata (1.2.4).";

            SerializedProperty catalog = serializedProfile.FindProperty("rewardItemCatalog");
            HashSet<CCS_ItemDefinition> uniqueCatalog = new HashSet<CCS_ItemDefinition>();
            for (int index = 0; index < catalog.arraySize; index++)
            {
                CCS_ItemDefinition existing = catalog.GetArrayElementAtIndex(index).objectReferenceValue as CCS_ItemDefinition;
                if (existing != null)
                {
                    uniqueCatalog.Add(existing);
                }
            }

            for (int index = 0; index < catalogItems.Length; index++)
            {
                if (catalogItems[index] != null)
                {
                    uniqueCatalog.Add(catalogItems[index]);
                }
            }

            catalog.arraySize = uniqueCatalog.Count;
            int writeIndex = 0;
            foreach (CCS_ItemDefinition catalogItem in uniqueCatalog)
            {
                catalog.GetArrayElementAtIndex(writeIndex).objectReferenceValue = catalogItem;
                writeIndex++;
            }

            SerializedProperty nodeSettings = serializedProfile.FindProperty("nodeRewardSettings");
            nodeSettings.arraySize = 3;
            PatchLegacyGatheringEntries(nodeSettings);
            nodeSettings.arraySize = 15;
            int frontierIndex = 3;

            ApplyGatheringNodeSettings(
                nodeSettings.GetArrayElementAtIndex(frontierIndex++),
                CCS_GatheringNodeType.Tree,
                CCS_ResourceSourceType.Natural,
                CCS_HarvestMethodType.Chop,
                CCS_ItemToolType.Axe,
                new[]
                {
                    Reward(CCS_GatheringResourceType.Wood, "ccs.survival.item.resource.wood", 2),
                    Reward(CCS_GatheringResourceType.Wood, "ccs.survival.item.resource.bark", 1),
                    Reward(CCS_GatheringResourceType.Stick, "ccs.survival.item.resource.sapling", 1)
                });

            ApplyGatheringNodeSettings(
                nodeSettings.GetArrayElementAtIndex(frontierIndex++),
                CCS_GatheringNodeType.DeadfallLog,
                CCS_ResourceSourceType.Natural,
                CCS_HarvestMethodType.Chop,
                CCS_ItemToolType.Axe,
                new[] { Reward(CCS_GatheringResourceType.Wood, "ccs.survival.item.resource.wood", 2) });

            ApplyGatheringNodeSettings(
                nodeSettings.GetArrayElementAtIndex(frontierIndex++),
                CCS_GatheringNodeType.FiberPlant,
                CCS_ResourceSourceType.Natural,
                CCS_HarvestMethodType.Gather,
                CCS_ItemToolType.None,
                new[] { Reward(CCS_GatheringResourceType.PlantFiber, "ccs.survival.item.resource.fiber", 2) });

            ApplyGatheringNodeSettings(
                nodeSettings.GetArrayElementAtIndex(frontierIndex++),
                CCS_GatheringNodeType.StoneOutcrop,
                CCS_ResourceSourceType.Natural,
                CCS_HarvestMethodType.Mine,
                CCS_ItemToolType.Pickaxe,
                new[]
                {
                    Reward(CCS_GatheringResourceType.Stone, "ccs.survival.item.resource.stone", 2),
                    Reward(CCS_GatheringResourceType.Stone, "ccs.survival.item.resource.flint", 1)
                });

            ApplyGatheringNodeSettings(
                nodeSettings.GetArrayElementAtIndex(frontierIndex++),
                CCS_GatheringNodeType.ClayDeposit,
                CCS_ResourceSourceType.Natural,
                CCS_HarvestMethodType.Dig,
                CCS_ItemToolType.Shovel,
                new[] { Reward(CCS_GatheringResourceType.Stone, "ccs.survival.item.resource.clay", 2) });

            ApplyGatheringNodeSettings(
                nodeSettings.GetArrayElementAtIndex(frontierIndex++),
                CCS_GatheringNodeType.WaterSource,
                CCS_ResourceSourceType.Water,
                CCS_HarvestMethodType.Collect,
                CCS_ItemToolType.None,
                new[] { Reward(CCS_GatheringResourceType.None, "ccs.survival.item.resource.water", 1) });

            ApplyGatheringNodeSettings(
                nodeSettings.GetArrayElementAtIndex(frontierIndex++),
                CCS_GatheringNodeType.OreVein,
                CCS_ResourceSourceType.Mining,
                CCS_HarvestMethodType.Mine,
                CCS_ItemToolType.Pickaxe,
                new[] { Reward(CCS_GatheringResourceType.Stone, "ccs.survival.item.resource.ore", 2) });

            ApplyGatheringNodeSettings(
                nodeSettings.GetArrayElementAtIndex(frontierIndex++),
                CCS_GatheringNodeType.CoalVein,
                CCS_ResourceSourceType.Mining,
                CCS_HarvestMethodType.Mine,
                CCS_ItemToolType.Pickaxe,
                new[] { Reward(CCS_GatheringResourceType.Stone, "ccs.survival.item.resource.coal", 2) });

            ApplyGatheringNodeSettings(
                nodeSettings.GetArrayElementAtIndex(frontierIndex++),
                CCS_GatheringNodeType.SalvageAbandonedWagon,
                CCS_ResourceSourceType.Salvage,
                CCS_HarvestMethodType.Salvage,
                CCS_ItemToolType.None,
                new[]
                {
                    Reward(CCS_GatheringResourceType.None, "ccs.survival.item.resource.scrapiron", 1),
                    Reward(CCS_GatheringResourceType.None, "ccs.survival.item.resource.nails", 2),
                    Reward(CCS_GatheringResourceType.Wood, "ccs.survival.item.resource.wood", 1),
                    Reward(CCS_GatheringResourceType.None, "ccs.survival.item.resource.canvas", 1)
                });

            ApplyGatheringNodeSettings(
                nodeSettings.GetArrayElementAtIndex(frontierIndex++),
                CCS_GatheringNodeType.SalvageCampRemains,
                CCS_ResourceSourceType.Salvage,
                CCS_HarvestMethodType.Salvage,
                CCS_ItemToolType.None,
                new[] { Reward(CCS_GatheringResourceType.Stick, "ccs.survival.item.resource.stick", 1) });

            ApplyGatheringNodeSettings(
                nodeSettings.GetArrayElementAtIndex(frontierIndex++),
                CCS_GatheringNodeType.SalvageHomesteadRuins,
                CCS_ResourceSourceType.Salvage,
                CCS_HarvestMethodType.Salvage,
                CCS_ItemToolType.None,
                new[]
                {
                    Reward(CCS_GatheringResourceType.Wood, "ccs.survival.item.resource.wood", 2),
                    Reward(CCS_GatheringResourceType.None, "ccs.survival.item.resource.nails", 1)
                });

            ApplyGatheringNodeSettings(
                nodeSettings.GetArrayElementAtIndex(frontierIndex),
                CCS_GatheringNodeType.SalvageMineDebris,
                CCS_ResourceSourceType.Salvage,
                CCS_HarvestMethodType.Salvage,
                CCS_ItemToolType.None,
                new[]
                {
                    Reward(CCS_GatheringResourceType.Stone, "ccs.survival.item.resource.scrapiron", 2),
                    Reward(CCS_GatheringResourceType.Stone, "ccs.survival.item.resource.coal", 1)
                });

            serializedProfile.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            profile.BuildRewardLookup();
        }

        private static void PatchLegacyGatheringEntries(SerializedProperty nodeSettings)
        {
            for (int index = 0; index < nodeSettings.arraySize; index++)
            {
                SerializedProperty entry = nodeSettings.GetArrayElementAtIndex(index);
                CCS_GatheringNodeType nodeType = (CCS_GatheringNodeType)entry.FindPropertyRelative("nodeType").intValue;
                switch (nodeType)
                {
                    case CCS_GatheringNodeType.SmallTree:
                        entry.FindPropertyRelative("resourceSourceType").intValue = (int)CCS_ResourceSourceType.Natural;
                        entry.FindPropertyRelative("harvestMethod").intValue = (int)CCS_HarvestMethodType.Chop;
                        entry.FindPropertyRelative("requiredToolType").intValue = (int)CCS_ItemToolType.Axe;
                        break;
                    case CCS_GatheringNodeType.Rock:
                        entry.FindPropertyRelative("resourceSourceType").intValue = (int)CCS_ResourceSourceType.Natural;
                        entry.FindPropertyRelative("harvestMethod").intValue = (int)CCS_HarvestMethodType.Mine;
                        entry.FindPropertyRelative("requiredToolType").intValue = (int)CCS_ItemToolType.Pickaxe;
                        break;
                    case CCS_GatheringNodeType.Bush:
                        entry.FindPropertyRelative("resourceSourceType").intValue = (int)CCS_ResourceSourceType.Natural;
                        entry.FindPropertyRelative("harvestMethod").intValue = (int)CCS_HarvestMethodType.Gather;
                        entry.FindPropertyRelative("requiredToolType").intValue = (int)CCS_ItemToolType.None;
                        break;
                }
            }
        }

        private static void ApplyGatheringNodeSettings(
            SerializedProperty settingsProperty,
            CCS_GatheringNodeType nodeType,
            CCS_ResourceSourceType sourceType,
            CCS_HarvestMethodType harvestMethod,
            CCS_ItemToolType requiredTool,
            CCS_GatheringReward[] rewards)
        {
            settingsProperty.FindPropertyRelative("nodeType").intValue = (int)nodeType;
            settingsProperty.FindPropertyRelative("resourceSourceType").intValue = (int)sourceType;
            settingsProperty.FindPropertyRelative("harvestMethod").intValue = (int)harvestMethod;
            settingsProperty.FindPropertyRelative("requiredToolType").intValue = (int)requiredTool;

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

        private static CCS_GatheringReward Reward(
            CCS_GatheringResourceType resourceType,
            string itemId,
            int amount)
        {
            return new CCS_GatheringReward
            {
                resourceType = resourceType,
                itemDefinitionId = itemId,
                amount = amount
            };
        }

        private static void EnsureFrontierWorldResourceDefinitions(
            CCS_ItemDefinition wood,
            CCS_ItemDefinition bark,
            CCS_ItemDefinition stone,
            CCS_ItemDefinition fiber,
            CCS_ItemDefinition flint,
            CCS_ItemDefinition clay,
            CCS_ItemDefinition coal,
            CCS_ItemDefinition ore,
            CCS_ItemDefinition scrapIron,
            CCS_ItemDefinition nails,
            CCS_ItemDefinition canvas,
            CCS_ItemDefinition water)
        {
            EnsureWorldDefinition(
                "CCS_FrontierResource_Tree",
                "ccs.survival.resource.frontier.tree",
                "Frontier Tree",
                CCS_ResourceNodeType.Tree,
                CCS_ResourceSourceType.Natural,
                CCS_HarvestMethodType.Chop,
                CCS_RequiredToolType.Axe,
                new[] { Drop(wood, 1, 2), Drop(bark, 1, 1) });

            EnsureWorldDefinition(
                "CCS_FrontierResource_StoneOutcrop",
                "ccs.survival.resource.frontier.stoneoutcrop",
                "Stone Outcrop",
                CCS_ResourceNodeType.Rock,
                CCS_ResourceSourceType.Natural,
                CCS_HarvestMethodType.Mine,
                CCS_RequiredToolType.Pickaxe,
                new[] { Drop(stone, 2, 3), Drop(flint, 1, 1) });

            EnsureWorldDefinition(
                "CCS_FrontierResource_OreVein",
                "ccs.survival.resource.frontier.orevein",
                "Ore Vein",
                CCS_ResourceNodeType.Rock,
                CCS_ResourceSourceType.Mining,
                CCS_HarvestMethodType.Mine,
                CCS_RequiredToolType.Pickaxe,
                new[] { Drop(ore, 1, 2) });

            EnsureWorldDefinition(
                "CCS_FrontierResource_CoalVein",
                "ccs.survival.resource.frontier.coalvein",
                "Coal Vein",
                CCS_ResourceNodeType.Rock,
                CCS_ResourceSourceType.Mining,
                CCS_HarvestMethodType.Mine,
                CCS_RequiredToolType.Pickaxe,
                new[] { Drop(coal, 1, 2) });

            EnsureWorldDefinition(
                "CCS_FrontierResource_FiberPlant",
                "ccs.survival.resource.frontier.fiberplant",
                "Fiber Plant",
                CCS_ResourceNodeType.Plant,
                CCS_ResourceSourceType.Natural,
                CCS_HarvestMethodType.Gather,
                CCS_RequiredToolType.None,
                new[] { Drop(fiber, 1, 2) });

            EnsureWorldDefinition(
                "CCS_FrontierResource_ClayDeposit",
                "ccs.survival.resource.frontier.claydeposit",
                "Clay Deposit",
                CCS_ResourceNodeType.Gatherable,
                CCS_ResourceSourceType.Natural,
                CCS_HarvestMethodType.Dig,
                CCS_RequiredToolType.Shovel,
                new[] { Drop(clay, 1, 2) });

            EnsureWorldDefinition(
                "CCS_FrontierResource_SalvageWagon",
                "ccs.survival.resource.frontier.salvage.wagon",
                "Abandoned Wagon",
                CCS_ResourceNodeType.Custom,
                CCS_ResourceSourceType.Salvage,
                CCS_HarvestMethodType.Salvage,
                CCS_RequiredToolType.None,
                new[] { Drop(scrapIron, 1, 2), Drop(nails, 1, 3), Drop(wood, 1, 1), Drop(canvas, 1, 1) });

            EnsureWorldDefinition(
                "CCS_FrontierResource_WaterSource",
                "ccs.survival.resource.frontier.watersource",
                "Water Source",
                CCS_ResourceNodeType.Gatherable,
                CCS_ResourceSourceType.Water,
                CCS_HarvestMethodType.Collect,
                CCS_RequiredToolType.None,
                new[] { Drop(water, 1, 2) });
        }

        private static void EnsureWorldDefinition(
            string assetName,
            string resourceId,
            string displayName,
            CCS_ResourceNodeType nodeType,
            CCS_ResourceSourceType sourceType,
            CCS_HarvestMethodType harvestMethod,
            CCS_RequiredToolType requiredTool,
            (CCS_ItemDefinition item, int min, int max)[] drops)
        {
            string path = $"{FrontierWorldResourcesRoot}/{assetName}.asset";
            CCS_ResourceDefinition definition = AssetDatabase.LoadAssetAtPath<CCS_ResourceDefinition>(path);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<CCS_ResourceDefinition>();
                AssetDatabase.CreateAsset(definition, path);
            }

            SerializedObject serialized = new SerializedObject(definition);
            serialized.FindProperty("resourceId").stringValue = resourceId;
            serialized.FindProperty("displayName").stringValue = displayName;
            serialized.FindProperty("nodeType").intValue = (int)nodeType;
            serialized.FindProperty("resourceSourceType").intValue = (int)sourceType;
            serialized.FindProperty("harvestMethod").intValue = (int)harvestMethod;
            serialized.FindProperty("maxHarvestCount").intValue = 2;
            serialized.FindProperty("respawnTimeSeconds").floatValue = 45f;
            serialized.FindProperty("requiredToolType").intValue = (int)requiredTool;

            SerializedProperty dropList = serialized.FindProperty("dropDefinitions");
            dropList.arraySize = drops.Length;
            for (int index = 0; index < drops.Length; index++)
            {
                SerializedProperty dropEntry = dropList.GetArrayElementAtIndex(index);
                dropEntry.FindPropertyRelative("itemDefinition").objectReferenceValue = drops[index].item;
                dropEntry.FindPropertyRelative("minQuantity").intValue = drops[index].min;
                dropEntry.FindPropertyRelative("maxQuantity").intValue = drops[index].max;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
        }

        private static (CCS_ItemDefinition item, int min, int max) Drop(CCS_ItemDefinition item, int min, int max)
        {
            return (item, min, max);
        }

        private static void PatchLegacyWorldTestDefinitions()
        {
            PatchWorldDefinitionMetadata(
                "Assets/CCS/Survival/Profiles/WorldResources/TestResources/CCS_TestResource_Tree.asset",
                CCS_ResourceSourceType.Natural,
                CCS_HarvestMethodType.Chop,
                CCS_RequiredToolType.Axe);
            PatchWorldDefinitionMetadata(
                "Assets/CCS/Survival/Profiles/WorldResources/TestResources/CCS_TestResource_Rock.asset",
                CCS_ResourceSourceType.Natural,
                CCS_HarvestMethodType.Mine,
                CCS_RequiredToolType.Pickaxe);
            PatchWorldDefinitionMetadata(
                "Assets/CCS/Survival/Profiles/WorldResources/TestResources/CCS_TestResource_Plant.asset",
                CCS_ResourceSourceType.Natural,
                CCS_HarvestMethodType.Gather,
                CCS_RequiredToolType.Knife);
        }

        private static void PatchWorldDefinitionMetadata(
            string assetPath,
            CCS_ResourceSourceType sourceType,
            CCS_HarvestMethodType harvestMethod,
            CCS_RequiredToolType requiredToolType)
        {
            CCS_ResourceDefinition definition = AssetDatabase.LoadAssetAtPath<CCS_ResourceDefinition>(assetPath);
            if (definition == null)
            {
                return;
            }

            SerializedObject serialized = new SerializedObject(definition);
            serialized.FindProperty("resourceSourceType").intValue = (int)sourceType;
            serialized.FindProperty("harvestMethod").intValue = (int)harvestMethod;
            serialized.FindProperty("requiredToolType").intValue = (int)requiredToolType;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
        }

        private static void EnsureFrontierGatheringTestNodes()
        {
            Scene scene = EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Single);
            Transform sceneRoot = FindSceneRoot();
            if (sceneRoot == null)
            {
                return;
            }

            CCS_GatheringProfile profile = AssetDatabase.LoadAssetAtPath<CCS_GatheringProfile>(GatheringProfilePath);
            Transform testArea = sceneRoot.Find(FrontierTestAreaName);
            if (testArea == null)
            {
                GameObject areaObject = new GameObject(FrontierTestAreaName);
                areaObject.transform.SetParent(sceneRoot, false);
                areaObject.transform.localPosition = new Vector3(8f, 0f, 0f);
                testArea = areaObject.transform;
            }

            EnsureGatheringNode(testArea, "CCS_TestFrontierTree", CCS_GatheringNodeType.Tree, profile, PrimitiveType.Cylinder, new Vector3(-2f, 1f, 0f), new Vector3(0.7f, 2f, 0.7f));
            EnsureGatheringNode(testArea, "CCS_TestFrontierStoneOutcrop", CCS_GatheringNodeType.StoneOutcrop, profile, PrimitiveType.Cube, new Vector3(0f, 0.5f, 0f), new Vector3(1.4f, 1f, 1.4f));
            EnsureGatheringNode(testArea, "CCS_TestFrontierSalvageWagon", CCS_GatheringNodeType.SalvageAbandonedWagon, profile, PrimitiveType.Cube, new Vector3(2f, 0.4f, 0f), new Vector3(1.8f, 0.8f, 1.2f));

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void EnsureGatheringNode(
            Transform parent,
            string objectName,
            CCS_GatheringNodeType nodeType,
            CCS_GatheringProfile profile,
            PrimitiveType primitiveType,
            Vector3 localPosition,
            Vector3 localScale)
        {
            Transform existing = parent.Find(objectName);
            GameObject nodeObject = existing != null ? existing.gameObject : GameObject.CreatePrimitive(primitiveType);
            nodeObject.name = objectName;
            nodeObject.transform.SetParent(parent, false);
            nodeObject.transform.localPosition = localPosition;
            nodeObject.transform.localScale = localScale;

            CCS_GatheringNode gatheringNode = nodeObject.GetComponent<CCS_GatheringNode>();
            if (gatheringNode == null)
            {
                gatheringNode = nodeObject.AddComponent<CCS_GatheringNode>();
            }

            gatheringNode.ConfigureFromProfile(profile, nodeType);

            if (nodeObject.GetComponent<CCS_GatheringInteractable>() == null)
            {
                nodeObject.AddComponent<CCS_GatheringInteractable>();
            }
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
