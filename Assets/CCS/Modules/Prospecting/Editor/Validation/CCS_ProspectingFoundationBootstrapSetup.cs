using System.Collections.Generic;
using System.IO;
using CCS.Modules.Economy;
using CCS.Modules.Gathering;
using CCS.Modules.Inventory;
using CCS.Modules.Playtesting;
using CCS.Modules.Resources;
using CCS.Modules.WorldResources;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CCS.Modules.Prospecting.Editor
{
    public static class CCS_ProspectingFoundationBootstrapSetup
    {
        private const string LogPrefix = "[CCS_ProspectingFoundationBootstrapSetup]";
        private const string GatheringProfilePath = "Assets/CCS/Survival/Profiles/Gathering/CCS_DefaultGatheringProfile.asset";
        private const string FrontierWorldResourcesRoot = "Assets/CCS/Survival/Profiles/WorldResources/Frontier";
        private const string GeneralStoreVendorPath = "Assets/CCS/Survival/Content/Vendors/CCS_Vendor_GeneralStore.asset";
        private const string GunsmithVendorPath = "Assets/CCS/Survival/Content/Vendors/CCS_Vendor_FrontierGunsmith.asset";
        private const string DefaultPlaytestProfilePath = "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset";
        private const string BootstrapScenePath = "Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string IronOreItemPath = "Assets/CCS/Survival/Content/Items/Industry/CCS_Item_IronOre.asset";
        private const string RefinedIronItemPath = "Assets/CCS/Survival/Content/Items/Industry/CCS_Item_RefinedIron.asset";
        private const string PrimitivePickPath = "Assets/CCS/Survival/Content/Items/Tools/Primitive/CCS_Item_Pick.asset";
        private const string IronPickPath = "Assets/CCS/Survival/Content/Items/Industry/CCS_Item_IronPick.asset";
        private const string ClayItemPath = "Assets/CCS/Survival/Content/Items/Resources/Frontier/CCS_Item_Clay.asset";
        private const string CoalItemPath = "Assets/CCS/Survival/Content/Items/Resources/Frontier/CCS_Item_Coal.asset";
        private const string StoneItemPath = "Assets/CCS/Survival/Content/Items/Resources/Primitive/CCS_Item_Stone.asset";
        private const string FlintItemPath = "Assets/CCS/Survival/Content/Items/Resources/Frontier/CCS_Item_Flint.asset";
        private const string ScrapIronItemPath = "Assets/CCS/Survival/Content/Items/Resources/Frontier/CCS_Item_ScrapIron.asset";
        private const string NailsItemPath = "Assets/CCS/Survival/Content/Items/Resources/Frontier/CCS_Item_Nails.asset";

        public static void ExecuteBatch()
        {
            AssetDatabase.Refresh();

            CCS_ItemDefinition ironOre = LoadItem(IronOreItemPath);
            CCS_ItemDefinition coal = LoadItem(CoalItemPath);
            CCS_ItemDefinition clay = LoadItem(ClayItemPath);
            CCS_ItemDefinition stone = LoadItem(StoneItemPath);
            CCS_ItemDefinition flint = LoadItem(FlintItemPath);
            CCS_ItemDefinition scrapIron = LoadItem(ScrapIronItemPath);
            CCS_ItemDefinition nails = LoadItem(NailsItemPath);
            CCS_ItemDefinition refinedIron = LoadItem(RefinedIronItemPath);
            if (ironOre == null || coal == null || clay == null || stone == null)
            {
                Debug.LogError($"{LogPrefix} Missing prerequisite mining items.");
                EditorApplication.Exit(1);
                return;
            }

            PatchMiningItemWeights(ironOre, coal, clay, scrapIron, stone, flint, nails);
            PatchGatheringProfileMiningMetadata(ironOre, coal, clay, stone, flint, scrapIron, nails);
            PatchWorldResourceDefinitions(ironOre, coal, clay, stone, flint, scrapIron, nails);
            PatchVendorBuyCatalogs(ironOre, coal, clay, scrapIron, nails, refinedIron, stone, flint);
            EnsureProspectingTestSceneNodes();
            EnsurePlaytestMiningSteps();
            CCS_SurvivalBootstrapVersionUtility.EnsureBundleVersionAtLeast(CCS_SurvivalBootstrapVersionUtility.CurrentMilestoneVersion);

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} Prospecting and mining expansion bootstrap complete (1.7.0).");
            EditorApplication.Exit(0);
        }

        private static CCS_ItemDefinition LoadItem(string path)
        {
            return AssetDatabase.LoadAssetAtPath<CCS_ItemDefinition>(path);
        }

        private static void PatchMiningItemWeights(
            CCS_ItemDefinition ironOre,
            CCS_ItemDefinition coal,
            CCS_ItemDefinition clay,
            CCS_ItemDefinition scrapIron,
            CCS_ItemDefinition stone,
            CCS_ItemDefinition flint,
            CCS_ItemDefinition nails)
        {
            SetItemWeight(ironOre, CCS_ProspectingLogisticsUtility.DenseOreWeight);
            SetItemWeight(coal, CCS_ProspectingLogisticsUtility.DenseCoalWeight);
            SetItemWeight(clay, CCS_ProspectingLogisticsUtility.ClayWeight);
            SetItemWeight(scrapIron, CCS_ProspectingLogisticsUtility.DenseOreWeight);
            SetItemWeight(stone, 1.5f);
            SetItemWeight(flint, 0.5f);
            SetItemWeight(nails, 0.25f);
        }

        private static void SetItemWeight(CCS_ItemDefinition item, float weight)
        {
            if (item == null)
            {
                return;
            }

            SerializedObject serialized = new SerializedObject(item);
            serialized.FindProperty("weight").floatValue = weight;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(item);
        }

        private static void PatchGatheringProfileMiningMetadata(
            CCS_ItemDefinition ironOre,
            CCS_ItemDefinition coal,
            CCS_ItemDefinition clay,
            CCS_ItemDefinition stone,
            CCS_ItemDefinition flint,
            CCS_ItemDefinition scrapIron,
            CCS_ItemDefinition nails)
        {
            CCS_GatheringProfile profile = AssetDatabase.LoadAssetAtPath<CCS_GatheringProfile>(GatheringProfilePath);
            if (profile == null)
            {
                Debug.LogError($"{LogPrefix} Missing gathering profile.");
                EditorApplication.Exit(1);
                return;
            }

            SerializedObject serializedProfile = new SerializedObject(profile);
            SerializedProperty catalog = serializedProfile.FindProperty("rewardItemCatalog");
            HashSet<CCS_ItemDefinition> catalogItems = new HashSet<CCS_ItemDefinition>();
            for (int index = 0; index < catalog.arraySize; index++)
            {
                CCS_ItemDefinition existing =
                    catalog.GetArrayElementAtIndex(index).objectReferenceValue as CCS_ItemDefinition;
                if (existing != null)
                {
                    catalogItems.Add(existing);
                }
            }

            catalogItems.Add(ironOre);
            catalogItems.Add(coal);
            catalogItems.Add(clay);
            catalogItems.Add(stone);
            catalogItems.Add(flint);
            catalogItems.Add(scrapIron);
            catalogItems.Add(nails);
            catalog.arraySize = catalogItems.Count;
            int writeIndex = 0;
            foreach (CCS_ItemDefinition catalogItem in catalogItems)
            {
                catalog.GetArrayElementAtIndex(writeIndex).objectReferenceValue = catalogItem;
                writeIndex++;
            }

            SerializedProperty nodeSettings = serializedProfile.FindProperty("nodeRewardSettings");
            for (int index = 0; index < nodeSettings.arraySize; index++)
            {
                SerializedProperty entry = nodeSettings.GetArrayElementAtIndex(index);
                CCS_GatheringNodeType nodeType = (CCS_GatheringNodeType)entry.FindPropertyRelative("nodeType").intValue;
                switch (nodeType)
                {
                    case CCS_GatheringNodeType.StoneOutcrop:
                        entry.FindPropertyRelative("minimumToolTier").enumValueIndex = (int)CCS_ToolTier.Primitive;
                        SetRewards(entry, Reward(stone, 2), Reward(flint, 1));
                        break;
                    case CCS_GatheringNodeType.OreVein:
                        entry.FindPropertyRelative("minimumToolTier").enumValueIndex = (int)CCS_ToolTier.Iron;
                        SetRewards(entry, Reward(ironOre, 2));
                        break;
                    case CCS_GatheringNodeType.CoalVein:
                        entry.FindPropertyRelative("minimumToolTier").enumValueIndex = (int)CCS_ToolTier.Iron;
                        SetRewards(entry, Reward(coal, 2));
                        break;
                    case CCS_GatheringNodeType.ClayDeposit:
                        entry.FindPropertyRelative("minimumToolTier").enumValueIndex = (int)CCS_ToolTier.Primitive;
                        SetRewards(entry, Reward(clay, 2));
                        break;
                    case CCS_GatheringNodeType.SalvageMineDebris:
                        entry.FindPropertyRelative("minimumToolTier").enumValueIndex = (int)CCS_ToolTier.None;
                        SetRewards(entry, Reward(scrapIron, 2), Reward(nails, 1), Reward(coal, 1));
                        break;
                }
            }

            serializedProfile.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            profile.BuildRewardLookup();
        }

        private static void SetRewards(SerializedProperty entry, params CCS_GatheringReward[] rewards)
        {
            SerializedProperty rewardsProperty = entry.FindPropertyRelative("rewards");
            rewardsProperty.arraySize = rewards.Length;
            for (int index = 0; index < rewards.Length; index++)
            {
                SerializedProperty rewardProperty = rewardsProperty.GetArrayElementAtIndex(index);
                rewardProperty.FindPropertyRelative("resourceType").enumValueIndex =
                    (int)rewards[index].resourceType;
                rewardProperty.FindPropertyRelative("itemDefinitionId").stringValue =
                    rewards[index].itemDefinitionId;
                rewardProperty.FindPropertyRelative("amount").intValue = rewards[index].amount;
            }
        }

        private static CCS_GatheringReward Reward(CCS_ItemDefinition item, int amount)
        {
            return new CCS_GatheringReward
            {
                resourceType = CCS_GatheringResourceType.Stone,
                itemDefinitionId = item.ItemId,
                amount = amount
            };
        }

        private static void PatchWorldResourceDefinitions(
            CCS_ItemDefinition ironOre,
            CCS_ItemDefinition coal,
            CCS_ItemDefinition clay,
            CCS_ItemDefinition stone,
            CCS_ItemDefinition flint,
            CCS_ItemDefinition scrapIron,
            CCS_ItemDefinition nails)
        {
            PatchWorldDefinition(
                "CCS_FrontierResource_OreVein",
                CCS_ProspectingContentIds.OreVeinResourceId,
                "Iron Vein",
                CCS_ResourceSourceType.Mining,
                CCS_HarvestMethodType.Mine,
                CCS_RequiredToolType.Pickaxe,
                CCS_ToolTier.Iron,
                new[] { Drop(ironOre, 1, 2) });
            PatchWorldDefinition(
                "CCS_FrontierResource_CoalVein",
                CCS_ProspectingContentIds.CoalVeinResourceId,
                "Coal Vein",
                CCS_ResourceSourceType.Mining,
                CCS_HarvestMethodType.Mine,
                CCS_RequiredToolType.Pickaxe,
                CCS_ToolTier.Iron,
                new[] { Drop(coal, 1, 2) });
            PatchWorldDefinition(
                "CCS_FrontierResource_StoneOutcrop",
                CCS_ProspectingContentIds.StoneOutcropResourceId,
                "Stone Outcrop",
                CCS_ResourceSourceType.Natural,
                CCS_HarvestMethodType.Mine,
                CCS_RequiredToolType.Pickaxe,
                CCS_ToolTier.Primitive,
                new[] { Drop(stone, 2, 3), Drop(flint, 1, 1) });
            PatchWorldDefinition(
                "CCS_FrontierResource_ClayDeposit",
                CCS_ProspectingContentIds.ClayDepositResourceId,
                "Clay Deposit",
                CCS_ResourceSourceType.Natural,
                CCS_HarvestMethodType.Dig,
                CCS_RequiredToolType.Shovel,
                CCS_ToolTier.Primitive,
                new[] { Drop(clay, 1, 2) });
            EnsureWorldDefinition(
                "CCS_FrontierResource_MineDebris",
                CCS_ProspectingContentIds.MineDebrisResourceId,
                "Mine Debris",
                CCS_ResourceSourceType.Salvage,
                CCS_HarvestMethodType.Salvage,
                CCS_RequiredToolType.None,
                CCS_ToolTier.None,
                new[] { Drop(scrapIron, 1, 2), Drop(nails, 1, 2), Drop(coal, 1, 1) });
            EnsureWorldDefinition(
                "CCS_FrontierResource_AbandonedMineEntrance",
                CCS_ProspectingContentIds.AbandonedMineEntranceResourceId,
                "Abandoned Mine Entrance",
                CCS_ResourceSourceType.Mining,
                CCS_HarvestMethodType.Other,
                CCS_RequiredToolType.None,
                CCS_ToolTier.None,
                System.Array.Empty<(CCS_ItemDefinition, int, int)>());
        }

        private static void PatchWorldDefinition(
            string assetName,
            string resourceId,
            string displayName,
            CCS_ResourceSourceType sourceType,
            CCS_HarvestMethodType harvestMethod,
            CCS_RequiredToolType requiredTool,
            CCS_ToolTier minimumToolTier,
            (CCS_ItemDefinition item, int min, int max)[] drops)
        {
            string path = $"{FrontierWorldResourcesRoot}/{assetName}.asset";
            if (!File.Exists(path))
            {
                EnsureWorldDefinition(assetName, resourceId, displayName, sourceType, harvestMethod, requiredTool, minimumToolTier, drops);
                return;
            }

            CCS_ResourceDefinition definition = AssetDatabase.LoadAssetAtPath<CCS_ResourceDefinition>(path);
            SerializedObject serialized = new SerializedObject(definition);
            serialized.FindProperty("minimumToolTier").enumValueIndex = (int)minimumToolTier;
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

        private static void EnsureWorldDefinition(
            string assetName,
            string resourceId,
            string displayName,
            CCS_ResourceSourceType sourceType,
            CCS_HarvestMethodType harvestMethod,
            CCS_RequiredToolType requiredTool,
            CCS_ToolTier minimumToolTier,
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
            serialized.FindProperty("resourceSourceType").intValue = (int)sourceType;
            serialized.FindProperty("harvestMethod").intValue = (int)harvestMethod;
            serialized.FindProperty("requiredToolType").intValue = (int)requiredTool;
            serialized.FindProperty("minimumToolTier").enumValueIndex = (int)minimumToolTier;
            serialized.FindProperty("maxHarvestCount").intValue = drops.Length == 0 ? 0 : 3;
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

        private static void PatchVendorBuyCatalogs(
            CCS_ItemDefinition ironOre,
            CCS_ItemDefinition coal,
            CCS_ItemDefinition clay,
            CCS_ItemDefinition scrapIron,
            CCS_ItemDefinition nails,
            CCS_ItemDefinition refinedIron,
            CCS_ItemDefinition stone,
            CCS_ItemDefinition flint)
        {
            MergeVendorBuy(GeneralStoreVendorPath, ironOre, 6);
            MergeVendorBuy(GeneralStoreVendorPath, coal, 5);
            MergeVendorBuy(GeneralStoreVendorPath, clay, 4);
            MergeVendorBuy(GeneralStoreVendorPath, scrapIron, 7);
            MergeVendorBuy(GeneralStoreVendorPath, nails, 3);
            MergeVendorBuy(GeneralStoreVendorPath, stone, 2);
            MergeVendorBuy(GeneralStoreVendorPath, flint, 2);
            if (refinedIron != null)
            {
                MergeVendorBuy(GunsmithVendorPath, refinedIron, 28);
                MergeVendorBuy(GunsmithVendorPath, ironOre, 8);
                MergeVendorBuy(GunsmithVendorPath, coal, 6);
            }
        }

        private static void MergeVendorBuy(string vendorPath, CCS_ItemDefinition item, int buyPrice)
        {
            if (item == null)
            {
                return;
            }

            CCS_VendorDefinition vendor = AssetDatabase.LoadAssetAtPath<CCS_VendorDefinition>(vendorPath);
            if (vendor == null)
            {
                return;
            }

            SerializedObject serialized = new SerializedObject(vendor);
            SerializedProperty items = serialized.FindProperty("vendorInventory").FindPropertyRelative("items");
            for (int index = 0; index < items.arraySize; index++)
            {
                SerializedProperty entry = items.GetArrayElementAtIndex(index);
                CCS_ItemDefinition existing =
                    entry.FindPropertyRelative("itemDefinition").objectReferenceValue as CCS_ItemDefinition;
                if (existing == item)
                {
                    entry.FindPropertyRelative("allowBuy").boolValue = true;
                    entry.FindPropertyRelative("buyPriceOverride").intValue = buyPrice;
                    serialized.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(vendor);
                    return;
                }
            }

            items.InsertArrayElementAtIndex(items.arraySize);
            SerializedProperty newEntry = items.GetArrayElementAtIndex(items.arraySize - 1);
            newEntry.FindPropertyRelative("itemDefinition").objectReferenceValue = item;
            newEntry.FindPropertyRelative("allowBuy").boolValue = true;
            newEntry.FindPropertyRelative("allowSell").boolValue = false;
            newEntry.FindPropertyRelative("buyPriceOverride").intValue = buyPrice;
            newEntry.FindPropertyRelative("sellPriceOverride").intValue = 0;
            newEntry.FindPropertyRelative("stockQuantity").intValue = 99;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(vendor);
        }

        private static void EnsureProspectingTestSceneNodes()
        {
            if (!File.Exists(BootstrapScenePath))
            {
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Single);
            Transform sceneRoot = FindSceneRoot();
            if (sceneRoot == null)
            {
                return;
            }

            CCS_GatheringProfile profile = AssetDatabase.LoadAssetAtPath<CCS_GatheringProfile>(GatheringProfilePath);
            Transform testArea = sceneRoot.Find(CCS_ProspectingContentIds.ProspectingTestAreaName);
            if (testArea == null)
            {
                GameObject areaObject = new GameObject(CCS_ProspectingContentIds.ProspectingTestAreaName);
                areaObject.transform.SetParent(sceneRoot, false);
                areaObject.transform.localPosition = new Vector3(14f, 0f, 0f);
                testArea = areaObject.transform;
            }

            EnsureGatheringNode(
                testArea,
                CCS_ProspectingContentIds.TestStoneOutcropObjectName,
                CCS_GatheringNodeType.StoneOutcrop,
                CCS_ProspectingContentIds.StoneOutcropSaveNodeId,
                profile,
                PrimitiveType.Cube,
                new Vector3(-3f, 0.5f, 0f),
                new Vector3(1.4f, 1f, 1.4f));
            EnsureGatheringNode(
                testArea,
                CCS_ProspectingContentIds.TestOreVeinObjectName,
                CCS_GatheringNodeType.OreVein,
                CCS_ProspectingContentIds.OreVeinSaveNodeId,
                profile,
                PrimitiveType.Cube,
                new Vector3(-1f, 0.5f, 1.5f),
                new Vector3(1.2f, 1.2f, 1.2f),
                new Color(0.45f, 0.35f, 0.3f));
            EnsureGatheringNode(
                testArea,
                CCS_ProspectingContentIds.TestCoalVeinObjectName,
                CCS_GatheringNodeType.CoalVein,
                CCS_ProspectingContentIds.CoalVeinSaveNodeId,
                profile,
                PrimitiveType.Cube,
                new Vector3(1f, 0.5f, 1.5f),
                new Vector3(1.2f, 1.2f, 1.2f),
                new Color(0.15f, 0.15f, 0.15f));
            EnsureGatheringNode(
                testArea,
                CCS_ProspectingContentIds.TestClayDepositObjectName,
                CCS_GatheringNodeType.ClayDeposit,
                CCS_ProspectingContentIds.ClayDepositSaveNodeId,
                profile,
                PrimitiveType.Cylinder,
                new Vector3(3f, 0.3f, 0f),
                new Vector3(1.5f, 0.6f, 1.5f),
                new Color(0.55f, 0.4f, 0.3f));
            EnsureGatheringNode(
                testArea,
                CCS_ProspectingContentIds.TestMineDebrisObjectName,
                CCS_GatheringNodeType.SalvageMineDebris,
                CCS_ProspectingContentIds.MineDebrisSaveNodeId,
                profile,
                PrimitiveType.Cube,
                new Vector3(0f, 0.3f, -1.5f),
                new Vector3(2f, 0.6f, 1.5f),
                new Color(0.35f, 0.3f, 0.28f));
            EnsureProspectingSpot(testArea, new Vector3(-2f, 0.1f, 2f));
            EnsureAbandonedMineEntrance(testArea, new Vector3(2.5f, 0.5f, -2f));

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void EnsureGatheringNode(
            Transform parent,
            string objectName,
            CCS_GatheringNodeType nodeType,
            string saveNodeId,
            CCS_GatheringProfile profile,
            PrimitiveType primitiveType,
            Vector3 localPosition,
            Vector3 localScale,
            Color? color = null)
        {
            Transform existing = parent.Find(objectName);
            GameObject nodeObject = existing != null ? existing.gameObject : GameObject.CreatePrimitive(primitiveType);
            nodeObject.name = objectName;
            nodeObject.transform.SetParent(parent, false);
            nodeObject.transform.localPosition = localPosition;
            nodeObject.transform.localScale = localScale;

            if (color.HasValue)
            {
                Renderer renderer = nodeObject.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.sharedMaterial.color = color.Value;
                }
            }

            CCS_GatheringNode gatheringNode = nodeObject.GetComponent<CCS_GatheringNode>();
            if (gatheringNode == null)
            {
                gatheringNode = nodeObject.AddComponent<CCS_GatheringNode>();
            }

            gatheringNode.ConfigureFromProfile(profile, nodeType);
            gatheringNode.ConfigureSaveNodeId(saveNodeId);

            if (nodeObject.GetComponent<CCS_GatheringInteractable>() == null)
            {
                nodeObject.AddComponent<CCS_GatheringInteractable>();
            }
        }

        private static void EnsureProspectingSpot(Transform parent, Vector3 localPosition)
        {
            Transform existing = parent.Find(CCS_ProspectingContentIds.TestProspectingSpotObjectName);
            GameObject spotObject = existing != null
                ? existing.gameObject
                : GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            spotObject.name = CCS_ProspectingContentIds.TestProspectingSpotObjectName;
            spotObject.transform.SetParent(parent, false);
            spotObject.transform.localPosition = localPosition;
            spotObject.transform.localScale = new Vector3(1.5f, 0.1f, 1.5f);
            if (spotObject.GetComponent<CCS_ProspectingSpotMarker>() == null)
            {
                spotObject.AddComponent<CCS_ProspectingSpotMarker>();
            }
        }

        private static void EnsureAbandonedMineEntrance(Transform parent, Vector3 localPosition)
        {
            Transform existing = parent.Find(CCS_ProspectingContentIds.TestAbandonedMineEntranceObjectName);
            GameObject entranceObject = existing != null
                ? existing.gameObject
                : GameObject.CreatePrimitive(PrimitiveType.Cube);
            entranceObject.name = CCS_ProspectingContentIds.TestAbandonedMineEntranceObjectName;
            entranceObject.transform.SetParent(parent, false);
            entranceObject.transform.localPosition = localPosition;
            entranceObject.transform.localScale = new Vector3(2.5f, 1.5f, 0.5f);
            if (entranceObject.GetComponent<CCS_AbandonedMineEntranceMarker>() == null)
            {
                entranceObject.AddComponent<CCS_AbandonedMineEntranceMarker>();
            }
        }

        private static void EnsurePlaytestMiningSteps()
        {
            CCS_PlaytestProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(DefaultPlaytestProfilePath);
            if (profile == null)
            {
                return;
            }

            InsertStep(profile, "ccs.survival.playtest.mining.acquire.pick", "Acquire pick", CCS_PlaytestStepType.AcquirePickForMining, CCS_ProspectingContentIds.IronPickItemId);
            InsertStep(profile, "ccs.survival.playtest.mining.stone", "Mine stone outcrop", CCS_PlaytestStepType.MineStoneOutcrop, string.Empty);
            InsertStep(profile, "ccs.survival.playtest.mining.iron", "Mine iron vein", CCS_PlaytestStepType.MineIronVein, string.Empty);
            InsertStep(profile, "ccs.survival.playtest.mining.coal", "Mine coal vein", CCS_PlaytestStepType.MineCoalVein, string.Empty);
            InsertStep(profile, "ccs.survival.playtest.mining.refine", "Refine ore at forge", CCS_PlaytestStepType.RefineMinedOreAtForge, string.Empty);
            InsertStep(profile, "ccs.survival.playtest.mining.wagon", "Load mining goods into wagon", CCS_PlaytestStepType.LoadMiningGoodsIntoWagonCargo, string.Empty);
            InsertStep(profile, "ccs.survival.playtest.mining.sell", "Sell mining goods", CCS_PlaytestStepType.SellMiningGoods, string.Empty);
            InsertStep(profile, "ccs.survival.playtest.mining.currency", "Verify mining currency", CCS_PlaytestStepType.VerifyMiningCurrencyIncreased, string.Empty);
            EditorUtility.SetDirty(profile);
        }

        private static void InsertStep(
            CCS_PlaytestProfile profile,
            string stepId,
            string displayName,
            CCS_PlaytestStepType stepType,
            string targetItemId)
        {
            SerializedObject serialized = new SerializedObject(profile);
            SerializedProperty steps = serialized.FindProperty("stepDefinitions");
            for (int index = 0; index < steps.arraySize; index++)
            {
                if (steps.GetArrayElementAtIndex(index).FindPropertyRelative("stepId").stringValue == stepId)
                {
                    return;
                }
            }

            steps.InsertArrayElementAtIndex(steps.arraySize);
            SerializedProperty step = steps.GetArrayElementAtIndex(steps.arraySize - 1);
            step.FindPropertyRelative("stepId").stringValue = stepId;
            step.FindPropertyRelative("displayName").stringValue = displayName;
            step.FindPropertyRelative("stepType").enumValueIndex = (int)stepType;
            step.FindPropertyRelative("instructionText").stringValue =
                $"Mining playtest: {displayName}. Ctrl+Shift+M shortcut available.";
            step.FindPropertyRelative("targetItemId").stringValue = targetItemId ?? string.Empty;
            serialized.ApplyModifiedPropertiesWithoutUndo();
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
