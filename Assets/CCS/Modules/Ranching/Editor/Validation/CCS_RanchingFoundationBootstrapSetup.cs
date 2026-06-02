using System.Collections.Generic;
using System.IO;
using CCS.Modules.Economy;
using CCS.Modules.Inventory;
using CCS.Modules.Playtesting;
using CCS.Modules.Ranching;
using CCS.Modules.Shelter;
using CCS.Survival.Composition;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEngine;

namespace CCS.Modules.Ranching.Editor
{
    public static class CCS_RanchingFoundationBootstrapSetup
    {
        private const string LogPrefix = "[CCS_RanchingFoundationBootstrapSetup]";
        private const string LivestockProfilePath = "Assets/CCS/Survival/Profiles/Ranching/CCS_DefaultLivestockProfile.asset";
        private const string RanchContentRoot = "Assets/CCS/Survival/Content/Ranching";
        private const string LivestockContentRoot = RanchContentRoot + "/Livestock";
        private const string StructuresContentRoot = RanchContentRoot + "/Structures";
        private const string ItemsContentRoot = RanchContentRoot + "/Items";
        private const string PrefabsRoot = "Assets/CCS/Survival/Prefabs/Ranching";
        private const string GeneralStoreVendorPath = "Assets/CCS/Survival/Content/Vendors/CCS_Vendor_GeneralStore.asset";
        private const string StableVendorPath = "Assets/CCS/Survival/Content/Vendors/CCS_Vendor_FrontierStable.asset";
        private const string InventoryProfilePath = "Assets/CCS/Survival/Profiles/Inventory/CCS_DefaultInventoryProfile.asset";
        private const string BootstrapRootPrefabPath = "Assets/CCS/Survival/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
        private const string DefaultPlaytestProfilePath = "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset";
        private const string TradeDollarsPath = "Assets/CCS/Survival/Profiles/Economy/Currencies/CCS_Currency_TradeDollars.asset";
        private const string HorseItemPath = "Assets/CCS/Survival/Content/Mounts/CCS_Item_FrontierHorse.asset";
        private const string WagonItemPath = "Assets/CCS/Survival/Content/Vehicles/CCS_Item_FrontierWagonDeed.asset";

        public static void ExecuteBatch()
        {
            AssetDatabase.Refresh();
            EnsureFolders();

            CCS_CurrencyDefinition tradeDollars = AssetDatabase.LoadAssetAtPath<CCS_CurrencyDefinition>(TradeDollarsPath);
            if (tradeDollars == null)
            {
                Debug.LogError($"{LogPrefix} Missing trade dollars currency.");
                EditorApplication.Exit(1);
                return;
            }

            GameObject chickenPrefab = EnsureLivestockPrefab("PF_CCS_Chicken", 0.4f);
            GameObject goatPrefab = EnsureLivestockPrefab("PF_CCS_Goat", 0.7f);
            GameObject cowPrefab = EnsureLivestockPrefab("PF_CCS_Cow", 1.2f);
            GameObject pigPrefab = EnsureLivestockPrefab("PF_CCS_Pig", 0.8f);

            CCS_ItemDefinition chickenItem = EnsureLivestockPurchaseItem(
                "CCS_Item_LivestockChicken",
                CCS_RanchingContentIds.ChickenItemId,
                "Chicken",
                "Frontier chicken purchase deed. Assign to a chicken coop to begin egg production.",
                175);
            CCS_ItemDefinition goatItem = EnsureLivestockPurchaseItem(
                "CCS_Item_LivestockGoat",
                CCS_RanchingContentIds.GoatItemId,
                "Goat",
                "Frontier goat purchase deed. Assign to an animal pen for milk production.",
                425);
            CCS_ItemDefinition cowItem = EnsureLivestockPurchaseItem(
                "CCS_Item_LivestockCow",
                CCS_RanchingContentIds.CowItemId,
                "Cow",
                "Frontier cow purchase deed. Assign to an animal pen for milk production.",
                850);
            CCS_ItemDefinition pigItem = EnsureLivestockPurchaseItem(
                "CCS_Item_LivestockPig",
                CCS_RanchingContentIds.PigItemId,
                "Pig",
                "Frontier pig purchase deed. Future meat production placeholder.",
                375);

            CCS_ItemDefinition eggItem = EnsureRanchProductItem(
                "CCS_Item_RanchEgg",
                CCS_RanchingContentIds.EggItemId,
                "Egg",
                "Fresh ranch egg collected from chickens.",
                18,
                0);
            CCS_ItemDefinition milkItem = EnsureRanchProductItem(
                "CCS_Item_RanchMilk",
                CCS_RanchingContentIds.MilkItemId,
                "Milk",
                "Fresh ranch milk collected from goats and cows.",
                28,
                0);
            CCS_ItemDefinition feedItem = EnsureRanchProductItem(
                "CCS_Item_RanchFeed",
                CCS_RanchingContentIds.FeedItemId,
                "Livestock Feed",
                "Frontier livestock feed placeholder for ranch production support.",
                0,
                20);
            CCS_ItemDefinition rawPorkItem = EnsureRanchProductItem(
                "CCS_Item_RanchRawPork",
                CCS_RanchingContentIds.RawPorkItemId,
                "Raw Pork",
                "Future pig meat product placeholder.",
                0,
                0);
            CCS_ItemDefinition rawBeefItem = EnsureRanchProductItem(
                "CCS_Item_RanchRawBeef",
                CCS_RanchingContentIds.RawBeefItemId,
                "Raw Beef",
                "Future cow meat product placeholder.",
                0,
                0);

            CCS_ItemDefinition chickenCoopKit = EnsureRanchKitItem(
                "CCS_Item_ChickenCoopKit",
                CCS_RanchingContentIds.ChickenCoopKitItemId,
                "Chicken Coop Kit",
                225);
            CCS_ItemDefinition animalPenKit = EnsureRanchKitItem(
                "CCS_Item_AnimalPenKit",
                CCS_RanchingContentIds.AnimalPenKitItemId,
                "Animal Pen Kit",
                350);
            CCS_ItemDefinition feedTroughKit = EnsureRanchKitItem(
                "CCS_Item_FeedTroughKit",
                CCS_RanchingContentIds.FeedTroughKitItemId,
                "Feed Trough Kit",
                90);
            CCS_ItemDefinition waterTroughKit = EnsureRanchKitItem(
                "CCS_Item_WaterTroughKit",
                CCS_RanchingContentIds.WaterTroughKitItemId,
                "Water Trough Kit",
                90);

            CCS_LivestockDefinition chickenDefinition = EnsureChickenDefinition(
                chickenPrefab,
                chickenItem,
                eggItem);
            CCS_LivestockDefinition goatDefinition = EnsureGoatDefinition(
                goatPrefab,
                goatItem,
                milkItem);
            CCS_LivestockDefinition cowDefinition = EnsureCowDefinition(
                cowPrefab,
                cowItem,
                milkItem);
            CCS_LivestockDefinition pigDefinition = EnsurePigDefinition(
                pigPrefab,
                pigItem,
                rawPorkItem);

            CCS_RanchStructureDefinition chickenCoop = EnsureChickenCoopStructure(chickenCoopKit);
            CCS_RanchStructureDefinition animalPen = EnsureAnimalPenStructure(animalPenKit);
            CCS_RanchStructureDefinition feedTrough = EnsureFeedTroughStructure(feedTroughKit);
            CCS_RanchStructureDefinition waterTrough = EnsureWaterTroughStructure(waterTroughKit);

            CCS_LivestockProfile livestockProfile = EnsureLivestockProfile(
                chickenDefinition,
                goatDefinition,
                cowDefinition,
                pigDefinition,
                chickenCoop,
                animalPen,
                feedTrough,
                waterTrough);

            EnsureGeneralStoreRanchCatalog(
                chickenItem,
                goatItem,
                cowItem,
                pigItem,
                feedItem,
                chickenCoopKit,
                animalPenKit,
                feedTroughKit,
                waterTroughKit,
                eggItem,
                milkItem);
            EnsureStableRanchCatalog(
                tradeDollars,
                chickenItem,
                goatItem,
                cowItem,
                pigItem,
                feedItem,
                chickenCoopKit,
                animalPenKit,
                feedTroughKit,
                waterTroughKit);

            EnsureInventorySaveRestore(
                chickenItem,
                goatItem,
                cowItem,
                pigItem,
                feedItem,
                eggItem,
                milkItem,
                rawPorkItem,
                rawBeefItem,
                chickenCoopKit,
                animalPenKit,
                feedTroughKit,
                waterTroughKit);

            AssignLivestockProfileToBootstrapHost(livestockProfile);
            EnsurePlaytestRanchSteps();
            BumpVersions();

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} Ranching foundation bootstrap setup complete (2.1.0).");
            EditorApplication.Exit(0);
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/CCS/Survival/Profiles/Ranching");
            EnsureFolder(LivestockContentRoot);
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

        private static GameObject EnsureLivestockPrefab(string prefabName, float uniformScale)
        {
            string prefabPath = $"{PrefabsRoot}/{prefabName}.prefab";
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existing != null)
            {
                return existing;
            }

            GameObject root = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            root.name = prefabName;
            root.transform.localScale = Vector3.one * uniformScale;

            CapsuleCollider primitiveCollider = root.GetComponent<CapsuleCollider>();
            if (primitiveCollider != null)
            {
                Object.DestroyImmediate(primitiveCollider);
            }

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static CCS_ItemDefinition EnsureLivestockPurchaseItem(
            string assetName,
            string itemId,
            string displayName,
            string description,
            int buyValue)
        {
            string path = $"{ItemsContentRoot}/{assetName}.asset";
            CCS_ItemDefinition item = LoadOrCreateItem(path);
            SerializedObject serialized = new SerializedObject(item);
            serialized.FindProperty("itemId").stringValue = itemId;
            serialized.FindProperty("displayName").stringValue = displayName;
            serialized.FindProperty("description").stringValue = description;
            serialized.FindProperty("category").enumValueIndex = (int)CCS_ItemCategory.Generic;
            serialized.FindProperty("gameplayKind").enumValueIndex = (int)CCS_ItemGameplayKind.Generic;
            serialized.FindProperty("maxStackSize").intValue = 1;
            serialized.FindProperty("buyValue").intValue = buyValue;
            serialized.FindProperty("sellValue").intValue = buyValue / 5;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(item);
            return item;
        }

        private static CCS_ItemDefinition EnsureRanchProductItem(
            string assetName,
            string itemId,
            string displayName,
            string description,
            int sellValue,
            int buyValue)
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
            serialized.FindProperty("buyValue").intValue = buyValue;
            serialized.FindProperty("sellValue").intValue = sellValue;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(item);
            return item;
        }

        private static CCS_ItemDefinition EnsureRanchKitItem(
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
                "Frontier ranch placement kit. Use to preview and place ranch structures on level ground.";
            serialized.FindProperty("category").enumValueIndex = (int)CCS_ItemCategory.Generic;
            serialized.FindProperty("gameplayKind").enumValueIndex = (int)CCS_ItemGameplayKind.Placeable;
            serialized.FindProperty("maxStackSize").intValue = 5;
            serialized.FindProperty("buyValue").intValue = buyValue;
            serialized.FindProperty("sellValue").intValue = buyValue / 5;
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

        private static CCS_LivestockDefinition EnsureChickenDefinition(
            GameObject worldPrefab,
            CCS_ItemDefinition purchaseItem,
            CCS_ItemDefinition productionItem)
        {
            return EnsureLivestockDefinition(
                "CCS_Livestock_Chicken",
                CCS_RanchingContentIds.ChickenLivestockId,
                "Chicken",
                CCS_LivestockType.Chicken,
                worldPrefab,
                purchaseItem,
                productionItem,
                30f,
                true,
                true,
                CCS_RanchStructureKind.ChickenCoop);
        }

        private static CCS_LivestockDefinition EnsureGoatDefinition(
            GameObject worldPrefab,
            CCS_ItemDefinition purchaseItem,
            CCS_ItemDefinition productionItem)
        {
            return EnsureLivestockDefinition(
                "CCS_Livestock_Goat",
                CCS_RanchingContentIds.GoatLivestockId,
                "Goat",
                CCS_LivestockType.Goat,
                worldPrefab,
                purchaseItem,
                productionItem,
                60f,
                true,
                true,
                CCS_RanchStructureKind.AnimalPen);
        }

        private static CCS_LivestockDefinition EnsureCowDefinition(
            GameObject worldPrefab,
            CCS_ItemDefinition purchaseItem,
            CCS_ItemDefinition productionItem)
        {
            return EnsureLivestockDefinition(
                "CCS_Livestock_Cow",
                CCS_RanchingContentIds.CowLivestockId,
                "Cow",
                CCS_LivestockType.Cow,
                worldPrefab,
                purchaseItem,
                productionItem,
                90f,
                true,
                true,
                CCS_RanchStructureKind.AnimalPen);
        }

        private static CCS_LivestockDefinition EnsurePigDefinition(
            GameObject worldPrefab,
            CCS_ItemDefinition purchaseItem,
            CCS_ItemDefinition productionItem)
        {
            return EnsureLivestockDefinition(
                "CCS_Livestock_Pig",
                CCS_RanchingContentIds.PigLivestockId,
                "Pig",
                CCS_LivestockType.Pig,
                worldPrefab,
                purchaseItem,
                productionItem,
                99999f,
                false,
                false,
                CCS_RanchStructureKind.AnimalPen);
        }

        private static CCS_LivestockDefinition EnsureLivestockDefinition(
            string assetName,
            string livestockId,
            string displayName,
            CCS_LivestockType livestockType,
            GameObject worldPrefab,
            CCS_ItemDefinition purchaseItem,
            CCS_ItemDefinition productionItem,
            float productionIntervalSeconds,
            bool requiresFeed,
            bool requiresWater,
            CCS_RanchStructureKind requiredStructureKind)
        {
            string path = $"{LivestockContentRoot}/{assetName}.asset";
            CCS_LivestockDefinition definition = AssetDatabase.LoadAssetAtPath<CCS_LivestockDefinition>(path);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<CCS_LivestockDefinition>();
                AssetDatabase.CreateAsset(definition, path);
            }

            SerializedObject serialized = new SerializedObject(definition);
            serialized.FindProperty("livestockId").stringValue = livestockId;
            serialized.FindProperty("displayName").stringValue = displayName;
            serialized.FindProperty("livestockType").enumValueIndex = (int)livestockType;
            serialized.FindProperty("purchaseItem").objectReferenceValue = purchaseItem;
            serialized.FindProperty("worldPrefab").objectReferenceValue = worldPrefab;
            serialized.FindProperty("productionIntervalSeconds").floatValue = productionIntervalSeconds;
            serialized.FindProperty("requiresFeed").boolValue = requiresFeed;
            serialized.FindProperty("requiresWater").boolValue = requiresWater;
            serialized.FindProperty("productionItemId").stringValue = productionItem != null ? productionItem.ItemId : string.Empty;
            serialized.FindProperty("productionItem").objectReferenceValue = productionItem;
            serialized.FindProperty("productionQuantity").intValue = 1;
            serialized.FindProperty("requiredStructureKind").enumValueIndex = (int)requiredStructureKind;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static CCS_RanchStructureDefinition EnsureChickenCoopStructure(CCS_ItemDefinition kitItem)
        {
            return EnsureRanchStructureDefinition(
                "CCS_RanchStructure_ChickenCoop",
                CCS_RanchingContentIds.ChickenCoopStructureId,
                "Chicken Coop",
                CCS_RanchStructureKind.ChickenCoop,
                kitItem,
                true,
                new Vector3(1.6f, 1f, 1.6f));
        }

        private static CCS_RanchStructureDefinition EnsureAnimalPenStructure(CCS_ItemDefinition kitItem)
        {
            return EnsureRanchStructureDefinition(
                "CCS_RanchStructure_AnimalPen",
                CCS_RanchingContentIds.AnimalPenStructureId,
                "Animal Pen",
                CCS_RanchStructureKind.AnimalPen,
                kitItem,
                true,
                new Vector3(2f, 1f, 2f));
        }

        private static CCS_RanchStructureDefinition EnsureFeedTroughStructure(CCS_ItemDefinition kitItem)
        {
            return EnsureRanchStructureDefinition(
                "CCS_RanchStructure_FeedTrough",
                CCS_RanchingContentIds.FeedTroughStructureId,
                "Feed Trough",
                CCS_RanchStructureKind.FeedTrough,
                kitItem,
                false,
                new Vector3(1.2f, 0.5f, 0.6f));
        }

        private static CCS_RanchStructureDefinition EnsureWaterTroughStructure(CCS_ItemDefinition kitItem)
        {
            return EnsureRanchStructureDefinition(
                "CCS_RanchStructure_WaterTrough",
                CCS_RanchingContentIds.WaterTroughStructureId,
                "Water Trough",
                CCS_RanchStructureKind.WaterTrough,
                kitItem,
                false,
                new Vector3(1.2f, 0.5f, 0.6f));
        }

        private static CCS_RanchStructureDefinition EnsureRanchStructureDefinition(
            string assetName,
            string structureDefinitionId,
            string displayName,
            CCS_RanchStructureKind structureKind,
            CCS_ItemDefinition kitItem,
            bool contributesToCampTier,
            Vector3 placedLocalScale)
        {
            string path = $"{StructuresContentRoot}/{assetName}.asset";
            CCS_RanchStructureDefinition definition = AssetDatabase.LoadAssetAtPath<CCS_RanchStructureDefinition>(path);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<CCS_RanchStructureDefinition>();
                AssetDatabase.CreateAsset(definition, path);
            }

            SerializedObject serialized = new SerializedObject(definition);
            serialized.FindProperty("structureDefinitionId").stringValue = structureDefinitionId;
            serialized.FindProperty("displayName").stringValue = displayName;
            serialized.FindProperty("structureKind").enumValueIndex = (int)structureKind;
            serialized.FindProperty("placeableKitItem").objectReferenceValue = kitItem;
            serialized.FindProperty("contributesToCampTier").boolValue = contributesToCampTier;
            serialized.FindProperty("campStructureKind").enumValueIndex = (int)CCS_CampStructureKind.Livestock;
            serialized.FindProperty("placementPrimitive").enumValueIndex = (int)PrimitiveType.Cube;
            serialized.FindProperty("placedLocalScale").vector3Value = placedLocalScale;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static CCS_LivestockProfile EnsureLivestockProfile(
            CCS_LivestockDefinition chickenDefinition,
            CCS_LivestockDefinition goatDefinition,
            CCS_LivestockDefinition cowDefinition,
            CCS_LivestockDefinition pigDefinition,
            CCS_RanchStructureDefinition chickenCoop,
            CCS_RanchStructureDefinition animalPen,
            CCS_RanchStructureDefinition feedTrough,
            CCS_RanchStructureDefinition waterTrough)
        {
            CCS_LivestockProfile profile = AssetDatabase.LoadAssetAtPath<CCS_LivestockProfile>(LivestockProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CCS_LivestockProfile>();
                AssetDatabase.CreateAsset(profile, LivestockProfilePath);
            }

            SerializedObject serialized = new SerializedObject(profile);
            serialized.FindProperty("profileId").stringValue = CCS_RanchingContentIds.DefaultProfileId;
            serialized.FindProperty("profileDisplayName").stringValue = "Default Livestock Profile";
            serialized.FindProperty("profileDescription").stringValue =
                "Frontier ranching livestock and structure catalog (2.1.0).";

            SerializedProperty livestockDefinitions = serialized.FindProperty("livestockDefinitions");
            livestockDefinitions.arraySize = 4;
            livestockDefinitions.GetArrayElementAtIndex(0).objectReferenceValue = chickenDefinition;
            livestockDefinitions.GetArrayElementAtIndex(1).objectReferenceValue = goatDefinition;
            livestockDefinitions.GetArrayElementAtIndex(2).objectReferenceValue = cowDefinition;
            livestockDefinitions.GetArrayElementAtIndex(3).objectReferenceValue = pigDefinition;

            SerializedProperty structureDefinitions = serialized.FindProperty("ranchStructureDefinitions");
            structureDefinitions.arraySize = 4;
            structureDefinitions.GetArrayElementAtIndex(0).objectReferenceValue = chickenCoop;
            structureDefinitions.GetArrayElementAtIndex(1).objectReferenceValue = animalPen;
            structureDefinitions.GetArrayElementAtIndex(2).objectReferenceValue = feedTrough;
            structureDefinitions.GetArrayElementAtIndex(3).objectReferenceValue = waterTrough;

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static void EnsureGeneralStoreRanchCatalog(
            CCS_ItemDefinition chickenItem,
            CCS_ItemDefinition goatItem,
            CCS_ItemDefinition cowItem,
            CCS_ItemDefinition pigItem,
            CCS_ItemDefinition feedItem,
            CCS_ItemDefinition chickenCoopKit,
            CCS_ItemDefinition animalPenKit,
            CCS_ItemDefinition feedTroughKit,
            CCS_ItemDefinition waterTroughKit,
            CCS_ItemDefinition eggItem,
            CCS_ItemDefinition milkItem)
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
                "Frontier general store with ranch livestock, kits, and product buyback (2.1.0).";

            SerializedProperty catalogItems = serialized.FindProperty("vendorInventory").FindPropertyRelative("items");
            MergeVendorCatalogEntry(catalogItems, chickenItem, true, false, 175, 35);
            MergeVendorCatalogEntry(catalogItems, goatItem, true, false, 425, 85);
            MergeVendorCatalogEntry(catalogItems, cowItem, true, false, 850, 170);
            MergeVendorCatalogEntry(catalogItems, pigItem, true, false, 375, 75);
            MergeVendorCatalogEntry(catalogItems, feedItem, true, false, 20, 4);
            MergeVendorCatalogEntry(catalogItems, chickenCoopKit, true, false, 225, 45);
            MergeVendorCatalogEntry(catalogItems, animalPenKit, true, false, 350, 70);
            MergeVendorCatalogEntry(catalogItems, feedTroughKit, true, false, 90, 18);
            MergeVendorCatalogEntry(catalogItems, waterTroughKit, true, false, 90, 18);
            MergeVendorCatalogEntry(catalogItems, eggItem, false, true, 0, 18);
            MergeVendorCatalogEntry(catalogItems, milkItem, false, true, 0, 28);

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(vendor);
        }

        private static void EnsureStableRanchCatalog(
            CCS_CurrencyDefinition currency,
            CCS_ItemDefinition chickenItem,
            CCS_ItemDefinition goatItem,
            CCS_ItemDefinition cowItem,
            CCS_ItemDefinition pigItem,
            CCS_ItemDefinition feedItem,
            CCS_ItemDefinition chickenCoopKit,
            CCS_ItemDefinition animalPenKit,
            CCS_ItemDefinition feedTroughKit,
            CCS_ItemDefinition waterTroughKit)
        {
            CCS_VendorDefinition vendor = AssetDatabase.LoadAssetAtPath<CCS_VendorDefinition>(StableVendorPath);
            if (vendor == null)
            {
                Debug.LogError($"{LogPrefix} Missing frontier stable vendor.");
                EditorApplication.Exit(1);
                return;
            }

            CCS_ItemDefinition horseItem = AssetDatabase.LoadAssetAtPath<CCS_ItemDefinition>(HorseItemPath);
            CCS_ItemDefinition wagonItem = AssetDatabase.LoadAssetAtPath<CCS_ItemDefinition>(WagonItemPath);

            SerializedObject serialized = new SerializedObject(vendor);
            serialized.FindProperty("description").stringValue =
                "Frontier stable for horse, wagon, and ranch purchases (2.1.0).";
            serialized.FindProperty("currencyDefinition").objectReferenceValue = currency;

            SerializedProperty catalogItems = serialized.FindProperty("vendorInventory").FindPropertyRelative("items");
            MergeVendorCatalogEntry(catalogItems, horseItem, true, true, 2500, 500);
            MergeVendorCatalogEntry(catalogItems, wagonItem, true, true, 1800, 360);
            MergeVendorCatalogEntry(catalogItems, chickenItem, true, false, 175, 35);
            MergeVendorCatalogEntry(catalogItems, goatItem, true, false, 425, 85);
            MergeVendorCatalogEntry(catalogItems, cowItem, true, false, 850, 170);
            MergeVendorCatalogEntry(catalogItems, pigItem, true, false, 375, 75);
            MergeVendorCatalogEntry(catalogItems, feedItem, true, false, 20, 4);
            MergeVendorCatalogEntry(catalogItems, chickenCoopKit, true, false, 225, 45);
            MergeVendorCatalogEntry(catalogItems, animalPenKit, true, false, 350, 70);
            MergeVendorCatalogEntry(catalogItems, feedTroughKit, true, false, 90, 18);
            MergeVendorCatalogEntry(catalogItems, waterTroughKit, true, false, 90, 18);

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

        private static void EnsureInventorySaveRestore(params CCS_ItemDefinition[] ranchItems)
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

            for (int index = 0; index < ranchItems.Length; index++)
            {
                CCS_ItemDefinition item = ranchItems[index];
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

        private static void AssignLivestockProfileToBootstrapHost(CCS_LivestockProfile profile)
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
            serialized.FindProperty("livestockProfile").objectReferenceValue = profile;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(host);
            PrefabUtility.SavePrefabAsset(prefabRoot);
        }

        private static void EnsurePlaytestRanchSteps()
        {
            CCS_PlaytestProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(DefaultPlaytestProfilePath);
            if (profile == null)
            {
                return;
            }

            InsertStep(
                profile,
                "ccs.survival.playtest.ranch.buy.chicken",
                "Buy chicken from vendor",
                CCS_PlaytestStepType.BuyChickenFromVendor,
                CCS_RanchingContentIds.ChickenItemId);
            InsertStep(
                profile,
                "ccs.survival.playtest.ranch.place.coop",
                "Place chicken coop",
                CCS_PlaytestStepType.PlaceChickenCoop,
                string.Empty);
            InsertStep(
                profile,
                "ccs.survival.playtest.ranch.assign.chicken",
                "Assign chicken to coop",
                CCS_PlaytestStepType.AssignChickenToCoop,
                string.Empty);
            InsertStep(
                profile,
                "ccs.survival.playtest.ranch.force.production",
                "Force ranch production",
                CCS_PlaytestStepType.ForceRanchProduction,
                string.Empty);
            InsertStep(
                profile,
                "ccs.survival.playtest.ranch.collect",
                "Collect ranch product",
                CCS_PlaytestStepType.CollectRanchProduct,
                string.Empty);
            InsertStep(
                profile,
                "ccs.survival.playtest.ranch.sell.egg",
                "Sell ranch egg",
                CCS_PlaytestStepType.SellRanchEgg,
                CCS_RanchingContentIds.EggItemId);
            InsertStep(
                profile,
                "ccs.survival.playtest.ranch.verify.food",
                "Verify ranch food supply increased",
                CCS_PlaytestStepType.VerifyRanchFoodSupplyIncreased,
                string.Empty);
            InsertStep(
                profile,
                "ccs.survival.playtest.ranch.save",
                "Save ranch state",
                CCS_PlaytestStepType.SaveRanchState,
                string.Empty);
            InsertStep(
                profile,
                "ccs.survival.playtest.ranch.verify.load",
                "Verify ranch state after load",
                CCS_PlaytestStepType.VerifyRanchStateAfterLoad,
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
                $"Ranch playtest: {displayName}. Ctrl+Shift+R shortcuts available.";
            step.FindPropertyRelative("targetItemId").stringValue = targetItemId ?? string.Empty;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void BumpVersions()
        {
            CCS_SurvivalBootstrapVersionUtility.EnsureBundleVersionAtLeast(
                CCS_SurvivalBootstrapVersionUtility.CurrentMilestoneVersion);
        }
    }
}
