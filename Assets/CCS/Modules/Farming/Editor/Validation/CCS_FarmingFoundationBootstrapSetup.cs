using System.Collections.Generic;
using System.IO;
using CCS.Modules.Economy;
using CCS.Modules.Inventory;
using CCS.Modules.Playtesting;
using CCS.Modules.Farming;
using CCS.Survival.Composition;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_FarmingFoundationBootstrapSetup
// CATEGORY: Modules / Farming / Editor / Validation
// PURPOSE: Batch-creates farming content, vendor catalog, playtest steps, and wiring.
// PLACEMENT: Invoked by build verification / milestone bootstrap pipeline.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// NOTES: Milestone 2.2.0 — crops, farm plots, general store catalog, bootstrap host.
// =============================================================================

namespace CCS.Modules.Farming.Editor
{
    public static class CCS_FarmingFoundationBootstrapSetup
    {
        private const string LogPrefix = "[CCS_FarmingFoundationBootstrapSetup]";
        private const string FarmingMilestoneVersion = "2.2.0";
        private const float PlaytestGrowthDurationSeconds = 45f;
        private const string CropProfilePath = "Assets/CCS/Survival/Profiles/Farming/CCS_DefaultCropProfile.asset";
        private const string FarmingContentRoot = "Assets/CCS/Survival/Content/Farming";
        private const string CropsContentRoot = FarmingContentRoot + "/Crops";
        private const string StructuresContentRoot = FarmingContentRoot + "/Structures";
        private const string ItemsContentRoot = FarmingContentRoot + "/Items";
        private const string PrefabsRoot = "Assets/CCS/Survival/Prefabs/Farming";
        private const string GeneralStoreVendorPath = "Assets/CCS/Survival/Content/Vendors/CCS_Vendor_GeneralStore.asset";
        private const string InventoryProfilePath = "Assets/CCS/Survival/Profiles/Inventory/CCS_DefaultInventoryProfile.asset";
        private const string BootstrapRootPrefabPath = "Assets/CCS/Survival/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
        private const string DefaultPlaytestProfilePath = "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset";

        public static void ExecuteBatch()
        {
            AssetDatabase.Refresh();
            EnsureFolders();

            GameObject cornPrefab = EnsureCropPrefab("PF_CCS_Crop_Corn", 0.6f);
            GameObject beanPrefab = EnsureCropPrefab("PF_CCS_Crop_Beans", 0.5f);
            GameObject potatoPrefab = EnsureCropPrefab("PF_CCS_Crop_Potatoes", 0.55f);
            GameObject wheatPrefab = EnsureCropPrefab("PF_CCS_Crop_Wheat", 0.7f);

            CCS_ItemDefinition farmPlotKit = EnsureFarmPlotKitItem();
            CCS_ItemDefinition cornSeed = EnsureSeedItem(
                "CCS_Item_CornSeed",
                CCS_FarmingContentIds.CornSeedItemId,
                "Corn Seed",
                15);
            CCS_ItemDefinition beanSeed = EnsureSeedItem(
                "CCS_Item_BeanSeed",
                CCS_FarmingContentIds.BeanSeedItemId,
                "Bean Seed",
                12);
            CCS_ItemDefinition potatoSeed = EnsureSeedItem(
                "CCS_Item_PotatoSeed",
                CCS_FarmingContentIds.PotatoSeedItemId,
                "Potato Seed",
                14);
            CCS_ItemDefinition wheatSeed = EnsureSeedItem(
                "CCS_Item_WheatSeed",
                CCS_FarmingContentIds.WheatSeedItemId,
                "Wheat Seed",
                18);

            CCS_ItemDefinition cornHarvest = EnsureHarvestItem(
                "CCS_Item_Corn",
                CCS_FarmingContentIds.CornHarvestItemId,
                "Corn",
                "Frontier corn harvest. Food and future livestock feed.",
                18);
            CCS_ItemDefinition beanHarvest = EnsureHarvestItem(
                "CCS_Item_Beans",
                CCS_FarmingContentIds.BeanHarvestItemId,
                "Beans",
                "Frontier bean harvest.",
                12);
            CCS_ItemDefinition potatoHarvest = EnsureHarvestItem(
                "CCS_Item_Potatoes",
                CCS_FarmingContentIds.PotatoHarvestItemId,
                "Potatoes",
                "Frontier potato harvest.",
                10);
            CCS_ItemDefinition wheatHarvest = EnsureHarvestItem(
                "CCS_Item_Wheat",
                CCS_FarmingContentIds.WheatHarvestItemId,
                "Wheat",
                "Frontier wheat harvest. Food and future livestock feed.",
                22);

            CCS_CropDefinition cornCrop = EnsureCornCropDefinition(
                cornPrefab,
                cornSeed,
                cornHarvest);
            CCS_CropDefinition beanCrop = EnsureBeanCropDefinition(
                beanPrefab,
                beanSeed,
                beanHarvest);
            CCS_CropDefinition potatoCrop = EnsurePotatoCropDefinition(
                potatoPrefab,
                potatoSeed,
                potatoHarvest);
            CCS_CropDefinition wheatCrop = EnsureWheatCropDefinition(
                wheatPrefab,
                wheatSeed,
                wheatHarvest);

            CCS_FarmPlotDefinition farmPlot = EnsureFarmPlotStructure(farmPlotKit);

            CCS_CropProfile cropProfile = EnsureCropProfile(
                cornCrop,
                beanCrop,
                potatoCrop,
                wheatCrop,
                farmPlot);

            EnsureGeneralStoreFarmingCatalog(
                farmPlotKit,
                cornSeed,
                beanSeed,
                potatoSeed,
                wheatSeed,
                cornHarvest,
                beanHarvest,
                potatoHarvest,
                wheatHarvest);

            EnsureInventorySaveRestore(
                farmPlotKit,
                cornSeed,
                beanSeed,
                potatoSeed,
                wheatSeed,
                cornHarvest,
                beanHarvest,
                potatoHarvest,
                wheatHarvest);

            AssignFarmingProfileToBootstrapHost(cropProfile);
            EnsurePlaytestFarmSteps();
            BumpVersions();

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} Farming foundation bootstrap setup complete ({FarmingMilestoneVersion}).");
            EditorApplication.Exit(0);
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/CCS/Survival/Profiles/Farming");
            EnsureFolder(CropsContentRoot);
            EnsureFolder(StructuresContentRoot);
            EnsureFolder(ItemsContentRoot);
            EnsureFolder(PrefabsRoot);
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string parent = Path.GetDirectoryName(folderPath)?.Replace('\\', '/');
            string folderName = Path.GetFileName(folderPath);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, folderName);
        }

        private static GameObject EnsureCropPrefab(string prefabName, float uniformScale)
        {
            string prefabPath = $"{PrefabsRoot}/{prefabName}.prefab";
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existing != null)
            {
                return existing;
            }

            GameObject root = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            root.name = prefabName;
            root.transform.localScale = Vector3.one * uniformScale;

            Collider primitiveCollider = root.GetComponent<Collider>();
            if (primitiveCollider != null)
            {
                Object.DestroyImmediate(primitiveCollider);
            }

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static CCS_ItemDefinition EnsureFarmPlotKitItem()
        {
            string path = $"{ItemsContentRoot}/CCS_Item_FarmPlotKit.asset";
            CCS_ItemDefinition item = LoadOrCreateItem(path);
            SerializedObject serialized = new SerializedObject(item);
            serialized.FindProperty("itemId").stringValue = CCS_FarmingContentIds.FarmPlotKitItemId;
            serialized.FindProperty("displayName").stringValue = "Farm Plot Kit";
            serialized.FindProperty("description").stringValue =
                "Frontier farm plot placement kit. Use to preview and place tillable plots on level ground.";
            serialized.FindProperty("category").enumValueIndex = (int)CCS_ItemCategory.Generic;
            serialized.FindProperty("gameplayKind").enumValueIndex = (int)CCS_ItemGameplayKind.Placeable;
            serialized.FindProperty("maxStackSize").intValue = 5;
            serialized.FindProperty("buyValue").intValue = 95;
            serialized.FindProperty("sellValue").intValue = 19;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(item);
            return item;
        }

        private static CCS_ItemDefinition EnsureSeedItem(
            string assetName,
            string itemId,
            string displayName,
            int buyValue)
        {
            string path = $"{ItemsContentRoot}/{assetName}.asset";
            CCS_ItemDefinition item = LoadOrCreateItem(path);
            SerializedObject serialized = new SerializedObject(item);
            serialized.FindProperty("itemId").stringValue = itemId;
            serialized.FindProperty("displayName").stringValue = displayName;
            serialized.FindProperty("description").stringValue =
                "Frontier crop seed. Select on hotbar and use near an empty farm plot to plant.";
            serialized.FindProperty("category").enumValueIndex = (int)CCS_ItemCategory.Generic;
            serialized.FindProperty("gameplayKind").enumValueIndex = (int)CCS_ItemGameplayKind.Generic;
            serialized.FindProperty("maxStackSize").intValue = 20;
            serialized.FindProperty("isStackable").boolValue = true;
            serialized.FindProperty("buyValue").intValue = buyValue;
            serialized.FindProperty("sellValue").intValue = buyValue / 5;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(item);
            return item;
        }

        private static CCS_ItemDefinition EnsureHarvestItem(
            string assetName,
            string itemId,
            string displayName,
            string description,
            int sellValue)
        {
            string path = $"{ItemsContentRoot}/{assetName}.asset";
            CCS_ItemDefinition item = LoadOrCreateItem(path);
            SerializedObject serialized = new SerializedObject(item);
            serialized.FindProperty("itemId").stringValue = itemId;
            serialized.FindProperty("displayName").stringValue = displayName;
            serialized.FindProperty("description").stringValue = description;
            serialized.FindProperty("category").enumValueIndex = (int)CCS_ItemCategory.Resource;
            serialized.FindProperty("gameplayKind").enumValueIndex = (int)CCS_ItemGameplayKind.Generic;
            serialized.FindProperty("maxStackSize").intValue = 20;
            serialized.FindProperty("isStackable").boolValue = true;
            serialized.FindProperty("buyValue").intValue = 0;
            serialized.FindProperty("sellValue").intValue = sellValue;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(item);
            return item;
        }

        private static CCS_ItemDefinition LoadOrCreateItem(string path)
        {
            CCS_ItemDefinition item = AssetDatabase.LoadAssetAtPath<CCS_ItemDefinition>(path);
            if (item == null)
            {
                item = ScriptableObject.CreateInstance<CCS_ItemDefinition>();
                AssetDatabase.CreateAsset(item, path);
            }

            return item;
        }

        private static CCS_CropDefinition EnsureCornCropDefinition(
            GameObject worldPrefab,
            CCS_ItemDefinition seedItem,
            CCS_ItemDefinition harvestItem)
        {
            return EnsureCropDefinition(
                "CCS_Crop_Corn",
                CCS_FarmingContentIds.CornCropId,
                "Corn",
                worldPrefab,
                seedItem,
                harvestItem,
                true,
                true,
                new Color(0.9f, 0.8f, 0.2f));
        }

        private static CCS_CropDefinition EnsureBeanCropDefinition(
            GameObject worldPrefab,
            CCS_ItemDefinition seedItem,
            CCS_ItemDefinition harvestItem)
        {
            return EnsureCropDefinition(
                "CCS_Crop_Beans",
                CCS_FarmingContentIds.BeanCropId,
                "Beans",
                worldPrefab,
                seedItem,
                harvestItem,
                true,
                false,
                new Color(0.35f, 0.55f, 0.25f));
        }

        private static CCS_CropDefinition EnsurePotatoCropDefinition(
            GameObject worldPrefab,
            CCS_ItemDefinition seedItem,
            CCS_ItemDefinition harvestItem)
        {
            return EnsureCropDefinition(
                "CCS_Crop_Potatoes",
                CCS_FarmingContentIds.PotatoCropId,
                "Potatoes",
                worldPrefab,
                seedItem,
                harvestItem,
                true,
                false,
                new Color(0.6f, 0.45f, 0.3f));
        }

        private static CCS_CropDefinition EnsureWheatCropDefinition(
            GameObject worldPrefab,
            CCS_ItemDefinition seedItem,
            CCS_ItemDefinition harvestItem)
        {
            return EnsureCropDefinition(
                "CCS_Crop_Wheat",
                CCS_FarmingContentIds.WheatCropId,
                "Wheat",
                worldPrefab,
                seedItem,
                harvestItem,
                true,
                true,
                new Color(0.85f, 0.75f, 0.35f));
        }

        private static CCS_CropDefinition EnsureCropDefinition(
            string assetName,
            string cropId,
            string displayName,
            GameObject worldPrefab,
            CCS_ItemDefinition seedItem,
            CCS_ItemDefinition harvestItem,
            bool isFoodCrop,
            bool isFutureLivestockFeed,
            Color matureCropColor)
        {
            string path = $"{CropsContentRoot}/{assetName}.asset";
            CCS_CropDefinition definition = AssetDatabase.LoadAssetAtPath<CCS_CropDefinition>(path);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<CCS_CropDefinition>();
                AssetDatabase.CreateAsset(definition, path);
            }

            SerializedObject serialized = new SerializedObject(definition);
            serialized.FindProperty("cropId").stringValue = cropId;
            serialized.FindProperty("displayName").stringValue = displayName;
            serialized.FindProperty("seedItem").objectReferenceValue = seedItem;
            serialized.FindProperty("harvestItem").objectReferenceValue = harvestItem;
            serialized.FindProperty("growthDurationSeconds").floatValue = PlaytestGrowthDurationSeconds;
            serialized.FindProperty("seedReturnQuantity").intValue = 1;
            serialized.FindProperty("isFoodCrop").boolValue = isFoodCrop;
            serialized.FindProperty("isFutureLivestockFeed").boolValue = isFutureLivestockFeed;
            serialized.FindProperty("cropVisualPrefab").objectReferenceValue = worldPrefab;
            serialized.FindProperty("fallbackCropPrimitive").enumValueIndex = (int)PrimitiveType.Cylinder;
            serialized.FindProperty("matureCropColor").colorValue = matureCropColor;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static CCS_FarmPlotDefinition EnsureFarmPlotStructure(CCS_ItemDefinition kitItem)
        {
            string path = $"{StructuresContentRoot}/CCS_FarmStructure_FarmPlot.asset";
            CCS_FarmPlotDefinition definition = AssetDatabase.LoadAssetAtPath<CCS_FarmPlotDefinition>(path);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<CCS_FarmPlotDefinition>();
                AssetDatabase.CreateAsset(definition, path);
            }

            SerializedObject serialized = new SerializedObject(definition);
            serialized.FindProperty("plotDefinitionId").stringValue = CCS_FarmingContentIds.FarmPlotStructureId;
            serialized.FindProperty("displayName").stringValue = "Farm Plot";
            serialized.FindProperty("placeableKitItem").objectReferenceValue = kitItem;
            serialized.FindProperty("contributesToCampTier").boolValue = true;
            serialized.FindProperty("registersAgriculturePresence").boolValue = true;
            serialized.FindProperty("placementPrimitive").enumValueIndex = (int)PrimitiveType.Cube;
            serialized.FindProperty("placedLocalScale").vector3Value = new Vector3(1.8f, 0.15f, 1.8f);
            serialized.FindProperty("plotColor").colorValue = new Color(0.45f, 0.3f, 0.15f);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static CCS_CropProfile EnsureCropProfile(
            CCS_CropDefinition cornCrop,
            CCS_CropDefinition beanCrop,
            CCS_CropDefinition potatoCrop,
            CCS_CropDefinition wheatCrop,
            CCS_FarmPlotDefinition farmPlot)
        {
            CCS_CropProfile profile = AssetDatabase.LoadAssetAtPath<CCS_CropProfile>(CropProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CCS_CropProfile>();
                AssetDatabase.CreateAsset(profile, CropProfilePath);
            }

            SerializedObject serialized = new SerializedObject(profile);
            serialized.FindProperty("profileId").stringValue = CCS_FarmingContentIds.DefaultProfileId;
            serialized.FindProperty("profileDisplayName").stringValue = "Default Crop Profile";
            serialized.FindProperty("profileDescription").stringValue =
                $"Frontier farming crop and farm plot catalog ({FarmingMilestoneVersion}).";

            SerializedProperty cropDefinitions = serialized.FindProperty("cropDefinitions");
            cropDefinitions.arraySize = 4;
            cropDefinitions.GetArrayElementAtIndex(0).objectReferenceValue = cornCrop;
            cropDefinitions.GetArrayElementAtIndex(1).objectReferenceValue = beanCrop;
            cropDefinitions.GetArrayElementAtIndex(2).objectReferenceValue = potatoCrop;
            cropDefinitions.GetArrayElementAtIndex(3).objectReferenceValue = wheatCrop;

            SerializedProperty plotDefinitions = serialized.FindProperty("farmPlotDefinitions");
            plotDefinitions.arraySize = 1;
            plotDefinitions.GetArrayElementAtIndex(0).objectReferenceValue = farmPlot;

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static void EnsureGeneralStoreFarmingCatalog(
            CCS_ItemDefinition farmPlotKit,
            CCS_ItemDefinition cornSeed,
            CCS_ItemDefinition beanSeed,
            CCS_ItemDefinition potatoSeed,
            CCS_ItemDefinition wheatSeed,
            CCS_ItemDefinition cornHarvest,
            CCS_ItemDefinition beanHarvest,
            CCS_ItemDefinition potatoHarvest,
            CCS_ItemDefinition wheatHarvest)
        {
            CCS_VendorDefinition vendor = AssetDatabase.LoadAssetAtPath<CCS_VendorDefinition>(GeneralStoreVendorPath);
            if (vendor == null)
            {
                Debug.LogError($"{LogPrefix} Missing general store vendor.");
                EditorApplication.Exit(1);
                return;
            }

            SerializedObject serialized = new SerializedObject(vendor);
            serialized.FindProperty("description").stringValue =
                $"Frontier general store with farming kits, seeds, and crop buyback ({FarmingMilestoneVersion}).";

            SerializedProperty catalogItems = serialized.FindProperty("vendorInventory").FindPropertyRelative("items");
            MergeVendorCatalogEntry(catalogItems, farmPlotKit, true, false, 95, 19);
            MergeVendorCatalogEntry(catalogItems, cornSeed, true, false, 15, 3);
            MergeVendorCatalogEntry(catalogItems, beanSeed, true, false, 12, 2);
            MergeVendorCatalogEntry(catalogItems, potatoSeed, true, false, 14, 3);
            MergeVendorCatalogEntry(catalogItems, wheatSeed, true, false, 18, 4);
            MergeVendorCatalogEntry(catalogItems, cornHarvest, false, true, 0, 18);
            MergeVendorCatalogEntry(catalogItems, beanHarvest, false, true, 0, 12);
            MergeVendorCatalogEntry(catalogItems, potatoHarvest, false, true, 0, 10);
            MergeVendorCatalogEntry(catalogItems, wheatHarvest, false, true, 0, 22);

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(vendor);
        }

        private static void MergeVendorCatalogEntry(
            SerializedProperty catalogItems,
            CCS_ItemDefinition item,
            bool allowBuy,
            bool allowSell,
            int buyOverride,
            int sellOverride)
        {
            if (item == null)
            {
                return;
            }

            for (int index = 0; index < catalogItems.arraySize; index++)
            {
                SerializedProperty entry = catalogItems.GetArrayElementAtIndex(index);
                if (entry.FindPropertyRelative("itemDefinition").objectReferenceValue != item)
                {
                    continue;
                }

                entry.FindPropertyRelative("stockQuantity").intValue = -1;
                entry.FindPropertyRelative("allowBuy").boolValue = allowBuy;
                entry.FindPropertyRelative("allowSell").boolValue = allowSell;
                if (allowBuy)
                {
                    entry.FindPropertyRelative("buyPriceOverride").intValue = buyOverride;
                }

                if (allowSell)
                {
                    entry.FindPropertyRelative("sellPriceOverride").intValue = sellOverride;
                }

                return;
            }

            int newIndex = catalogItems.arraySize;
            catalogItems.InsertArrayElementAtIndex(newIndex);
            SerializedProperty newEntry = catalogItems.GetArrayElementAtIndex(newIndex);
            newEntry.FindPropertyRelative("itemDefinition").objectReferenceValue = item;
            newEntry.FindPropertyRelative("stockQuantity").intValue = -1;
            newEntry.FindPropertyRelative("allowBuy").boolValue = allowBuy;
            newEntry.FindPropertyRelative("allowSell").boolValue = allowSell;
            newEntry.FindPropertyRelative("buyPriceOverride").intValue = allowBuy ? buyOverride : -1;
            newEntry.FindPropertyRelative("sellPriceOverride").intValue = allowSell ? sellOverride : -1;
        }

        private static void EnsureInventorySaveRestore(params CCS_ItemDefinition[] farmingItems)
        {
            CCS_InventoryProfile inventoryProfile =
                AssetDatabase.LoadAssetAtPath<CCS_InventoryProfile>(InventoryProfilePath);
            if (inventoryProfile == null)
            {
                return;
            }

            List<CCS_ItemDefinition> merged = new List<CCS_ItemDefinition>();
            CCS_ItemDefinition[] existing = inventoryProfile.SaveRestoreItemDefinitions;
            for (int index = 0; index < existing.Length; index++)
            {
                if (existing[index] != null && !merged.Contains(existing[index]))
                {
                    merged.Add(existing[index]);
                }
            }

            for (int index = 0; index < farmingItems.Length; index++)
            {
                CCS_ItemDefinition item = farmingItems[index];
                if (item != null && !merged.Contains(item))
                {
                    merged.Add(item);
                }
            }

            SerializedObject serialized = new SerializedObject(inventoryProfile);
            SerializedProperty restoreList = serialized.FindProperty("saveRestoreItemDefinitions");
            restoreList.arraySize = merged.Count;
            for (int index = 0; index < merged.Count; index++)
            {
                restoreList.GetArrayElementAtIndex(index).objectReferenceValue = merged[index];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(inventoryProfile);
        }

        private static void AssignFarmingProfileToBootstrapHost(CCS_CropProfile profile)
        {
            GameObject prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(BootstrapRootPrefabPath);
            if (prefabRoot == null)
            {
                return;
            }

            CCS_SurvivalGameplayServiceHost host = prefabRoot.GetComponent<CCS_SurvivalGameplayServiceHost>();
            if (host == null)
            {
                return;
            }

            SerializedObject serialized = new SerializedObject(host);
            serialized.FindProperty("farmingProfile").objectReferenceValue = profile;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(host);
            PrefabUtility.SavePrefabAsset(prefabRoot);
        }

        private static void EnsurePlaytestFarmSteps()
        {
            CCS_PlaytestProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(DefaultPlaytestProfilePath);
            if (profile == null)
            {
                return;
            }

            InsertStep(
                profile,
                "ccs.survival.playtest.farming.buy.plotkit",
                "Buy farm plot kit from vendor",
                CCS_PlaytestStepType.BuyFarmPlotKit,
                CCS_FarmingContentIds.FarmPlotKitItemId);
            InsertStep(
                profile,
                "ccs.survival.playtest.farming.place.plot",
                "Place farm plot",
                CCS_PlaytestStepType.PlaceFarmPlot,
                CCS_FarmingContentIds.FarmPlotKitItemId);
            InsertStep(
                profile,
                "ccs.survival.playtest.farming.buy.cornseed",
                "Buy corn seed from vendor",
                CCS_PlaytestStepType.BuyCornSeed,
                CCS_FarmingContentIds.CornSeedItemId);
            InsertStep(
                profile,
                "ccs.survival.playtest.farming.plant.corn",
                "Plant corn seed in farm plot",
                CCS_PlaytestStepType.PlantCornSeed,
                CCS_FarmingContentIds.CornSeedItemId);
            InsertStep(
                profile,
                "ccs.survival.playtest.farming.force.growth",
                "Force crop growth",
                CCS_PlaytestStepType.ForceCropGrowth,
                string.Empty);
            InsertStep(
                profile,
                "ccs.survival.playtest.farming.harvest",
                "Harvest mature crop",
                CCS_PlaytestStepType.HarvestCrop,
                string.Empty);
            InsertStep(
                profile,
                "ccs.survival.playtest.farming.sell.crop",
                "Sell harvested crop",
                CCS_PlaytestStepType.SellCrop,
                CCS_FarmingContentIds.CornHarvestItemId);
            InsertStep(
                profile,
                "ccs.survival.playtest.farming.verify.food",
                "Verify farm food supply increased",
                CCS_PlaytestStepType.VerifyFarmFoodSupplyIncreased,
                string.Empty);
            InsertStep(
                profile,
                "ccs.survival.playtest.farming.save",
                "Save farm state",
                CCS_PlaytestStepType.SaveFarmState,
                string.Empty);
            InsertStep(
                profile,
                "ccs.survival.playtest.farming.verify.load",
                "Verify farm state after load",
                CCS_PlaytestStepType.VerifyFarmStateAfterLoad,
                string.Empty);
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
                $"Farming playtest: {displayName}. Ctrl+Shift+P shortcuts available.";
            step.FindPropertyRelative("targetItemId").stringValue = targetItemId ?? string.Empty;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void BumpVersions()
        {
            CCS_SurvivalBootstrapVersionUtility.EnsureBundleVersionAtLeast(FarmingMilestoneVersion);
        }
    }
}
