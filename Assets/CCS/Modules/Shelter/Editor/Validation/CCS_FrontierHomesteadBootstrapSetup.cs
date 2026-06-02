using System.IO;
using CCS.Modules.Economy;
using CCS.Modules.Inventory;
using CCS.Modules.Playtesting;
using CCS.Modules.Shelter;
using CCS.Modules.Storage;
using CCS.Survival.Composition;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEngine;

namespace CCS.Modules.Shelter.Editor
{
    public static class CCS_FrontierHomesteadBootstrapSetup
    {
        private const string LogPrefix = "[CCS_FrontierHomesteadBootstrapSetup]";
        private const string CampProfilePath = "Assets/CCS/Survival/Profiles/Camp/CCS_DefaultCampDefinition.asset";
        private const string FrontierStorageCampProfilePath =
            "Assets/CCS/Survival/Profiles/Storage/CCS_FrontierStorageCampProfile.asset";
        private const string CampTierProfilePath = "Assets/CCS/Survival/Profiles/Camp/CCS_DefaultCampTierProfile.asset";
        private const string FrontierStorageRoot = "Assets/CCS/Survival/Content/Storage/Frontier";
        private const string FrontierStructuresRoot = "Assets/CCS/Survival/Content/Structures/Frontier";
        private const string FrontierItemsRoot = "Assets/CCS/Survival/Content/Items/Frontier";
        private const string GeneralStoreVendorPath = "Assets/CCS/Survival/Content/Vendors/CCS_Vendor_GeneralStore.asset";
        private const string BootstrapRootPrefabPath = "Assets/CCS/Survival/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
        private const string DefaultPlaytestProfilePath = "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset";
        private const string ShelterCampPersistenceStepId = "ccs.survival.playtest.shelter.verify.save";

        private const string SupplyCrateKitItemId = "ccs.survival.item.frontier.supplycratekit";
        private const string TrapperChestKitItemId = "ccs.survival.item.frontier.trapperchestkit";
        private const string WorkbenchKitItemId = "ccs.survival.item.frontier.workbenchkit";
        private const string CanvasBundleItemId = "ccs.survival.item.frontier.canvasbundle";
        private const string LumberBundleItemId = "ccs.survival.item.frontier.lumberbundle";

        public static void ExecuteBatch()
        {
            AssetDatabase.Refresh();
            EnsureFolders();

            CCS_ItemDefinition supplyCrateKit = EnsureKitItem(
                "CCS_Item_SupplyCrateKit",
                SupplyCrateKitItemId,
                "Supply Crate Kit");
            CCS_ItemDefinition trapperChestKit = EnsureKitItem(
                "CCS_Item_TrapperChestKit",
                TrapperChestKitItemId,
                "Trapper Chest Kit");
            CCS_ItemDefinition workbenchKit = EnsureKitItem(
                "CCS_Item_WorkbenchKit",
                WorkbenchKitItemId,
                "Frontier Workbench Kit");
            CCS_ItemDefinition canvasBundle = EnsurePlaceholderBundle(
                "CCS_Item_CanvasBundle",
                CanvasBundleItemId,
                "Canvas Bundle");
            CCS_ItemDefinition lumberBundle = EnsurePlaceholderBundle(
                "CCS_Item_LumberBundle",
                LumberBundleItemId,
                "Lumber Bundle");

            GameObject supplyCratePrefab = EnsureStoragePrefab("PF_CCS_SupplyCrate", PrimitiveType.Cube, new Vector3(1.1f, 0.8f, 1.1f));
            GameObject trapperChestPrefab = EnsureStoragePrefab("PF_CCS_TrapperChest", PrimitiveType.Cube, new Vector3(1.2f, 0.7f, 0.9f));

            CCS_StorageContainerDefinition supplyCrate = EnsureStorageDefinition(
                "CCS_StorageDefinition_SupplyCrate",
                "ccs.survival.storage.frontier.supplycrate",
                "Supply Crate",
                10,
                supplyCratePrefab,
                supplyCrateKit);
            CCS_StorageContainerDefinition trapperChest = EnsureStorageDefinition(
                "CCS_StorageDefinition_TrapperChest",
                "ccs.survival.storage.frontier.trapperchest",
                "Trapper Chest",
                12,
                trapperChestPrefab,
                trapperChestKit);

            CCS_WorkbenchDefinition workbench = EnsureWorkbenchDefinition(workbenchKit);

            CCS_CampTierProfile tierProfile = EnsureCampTierProfile();
            CCS_FrontierStorageCampProfile storageCampProfile = EnsureFrontierStorageCampProfile(supplyCrate, trapperChest);
            CCS_CampDefinition campDefinition = EnsureCampDefinition(tierProfile, workbench);
            AssignCampDefinitionToBootstrapHost(campDefinition);
            AssignFrontierStorageCampProfileToBootstrapHost(storageCampProfile);

            EnsureGeneralStoreHomesteadCatalog(supplyCrateKit, workbenchKit, canvasBundle, lumberBundle);
            EnsurePlaytestHomesteadSteps();
            BumpProfileVersions();

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} Frontier homestead bootstrap setup complete (1.4.1).");
            EditorApplication.Exit(0);
        }

        private static void UpdateProjectVersion()
        {
            CCS_SurvivalBootstrapVersionUtility.EnsureBundleVersionAtLeast(
                CCS_SurvivalBootstrapVersionUtility.CurrentMilestoneVersion);
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/CCS/Survival/Content/Storage/Frontier");
            EnsureFolder(FrontierStorageRoot + "/Prefabs");
            EnsureFolder(FrontierStructuresRoot);
            EnsureFolder(FrontierItemsRoot);
            EnsureFolder("Assets/CCS/Survival/Profiles/Camp");
            EnsureFolder("Assets/CCS/Survival/Profiles/Storage");
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

        private static CCS_ItemDefinition EnsureKitItem(string assetName, string itemId, string displayName)
        {
            string path = $"{FrontierItemsRoot}/{assetName}.asset";
            CCS_ItemDefinition item = AssetDatabase.LoadAssetAtPath<CCS_ItemDefinition>(path);
            if (item == null)
            {
                item = ScriptableObject.CreateInstance<CCS_ItemDefinition>();
                AssetDatabase.CreateAsset(item, path);
            }

            SerializedObject serialized = new SerializedObject(item);
            serialized.FindProperty("itemId").stringValue = itemId;
            serialized.FindProperty("displayName").stringValue = displayName;
            serialized.FindProperty("description").stringValue =
                "Frontier homestead placement kit. Use to preview and place structure on level ground.";
            serialized.FindProperty("category").enumValueIndex = (int)CCS_ItemCategory.Generic;
            serialized.FindProperty("gameplayKind").enumValueIndex = (int)CCS_ItemGameplayKind.Placeable;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(item);
            return item;
        }

        private static CCS_ItemDefinition EnsurePlaceholderBundle(string assetName, string itemId, string displayName)
        {
            string path = $"{FrontierItemsRoot}/{assetName}.asset";
            CCS_ItemDefinition item = AssetDatabase.LoadAssetAtPath<CCS_ItemDefinition>(path);
            if (item == null)
            {
                item = ScriptableObject.CreateInstance<CCS_ItemDefinition>();
                AssetDatabase.CreateAsset(item, path);
            }

            SerializedObject serialized = new SerializedObject(item);
            serialized.FindProperty("itemId").stringValue = itemId;
            serialized.FindProperty("displayName").stringValue = displayName;
            serialized.FindProperty("description").stringValue = "Placeholder bundle for future homestead economy (1.4.1).";
            serialized.FindProperty("category").enumValueIndex = (int)CCS_ItemCategory.Material;
            serialized.FindProperty("gameplayKind").enumValueIndex = (int)CCS_ItemGameplayKind.Generic;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(item);
            return item;
        }

        private static GameObject EnsureStoragePrefab(string prefabName, PrimitiveType primitive, Vector3 scale)
        {
            string path = $"{FrontierStorageRoot}/Prefabs/{prefabName}.prefab";
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null)
            {
                return existing;
            }

            GameObject root = GameObject.CreatePrimitive(primitive);
            root.name = prefabName;
            root.transform.localScale = scale;
            Collider collider = root.GetComponent<Collider>();
            if (collider != null)
            {
                collider.isTrigger = false;
            }

            root.AddComponent<CCS_StorageContainer>();
            root.AddComponent<CCS_StorageContainerInteractable>();
            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        private static CCS_StorageContainerDefinition EnsureStorageDefinition(
            string assetName,
            string containerId,
            string displayName,
            int slotCount,
            GameObject prefab,
            CCS_ItemDefinition kitItem)
        {
            string path = $"{FrontierStorageRoot}/{assetName}.asset";
            CCS_StorageContainerDefinition definition =
                AssetDatabase.LoadAssetAtPath<CCS_StorageContainerDefinition>(path);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<CCS_StorageContainerDefinition>();
                AssetDatabase.CreateAsset(definition, path);
            }

            SerializedObject serialized = new SerializedObject(definition);
            serialized.FindProperty("containerId").stringValue = containerId;
            serialized.FindProperty("displayName").stringValue = displayName;
            serialized.FindProperty("slotCount").intValue = slotCount;
            serialized.FindProperty("prefabReference").objectReferenceValue = prefab;
            serialized.FindProperty("placeableKitItem").objectReferenceValue = kitItem;
            serialized.FindProperty("contributesToCampTier").boolValue = true;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static CCS_WorkbenchDefinition EnsureWorkbenchDefinition(CCS_ItemDefinition kitItem)
        {
            string path = $"{FrontierStructuresRoot}/CCS_WorkbenchDefinition_FrontierWorkbench.asset";
            CCS_WorkbenchDefinition definition = AssetDatabase.LoadAssetAtPath<CCS_WorkbenchDefinition>(path);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<CCS_WorkbenchDefinition>();
                AssetDatabase.CreateAsset(definition, path);
            }

            SerializedObject serialized = new SerializedObject(definition);
            serialized.FindProperty("workbenchDefinitionId").stringValue = "ccs.survival.workbench.frontier";
            serialized.FindProperty("displayName").stringValue = "Frontier Workbench";
            serialized.FindProperty("placeableKitItem").objectReferenceValue = kitItem;
            serialized.FindProperty("contributesToCampTier").boolValue = true;
            serialized.FindProperty("campStructureKind").enumValueIndex = (int)CCS_CampStructureKind.WorkArea;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static CCS_CampTierProfile EnsureCampTierProfile()
        {
            CCS_CampTierProfile profile = AssetDatabase.LoadAssetAtPath<CCS_CampTierProfile>(CampTierProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CCS_CampTierProfile>();
                AssetDatabase.CreateAsset(profile, CampTierProfilePath);
            }

            SerializedObject serialized = new SerializedObject(profile);
            serialized.FindProperty("profileVersion").stringValue = "1.4.1";
            serialized.FindProperty("profileDescription").stringValue =
                "Frontier camp tier ladder: TemporaryCamp, FrontierCamp, FrontierHomestead (1.4.1).";

            SerializedProperty tiers = serialized.FindProperty("tierDefinitions");
            tiers.arraySize = 3;
            ConfigureTier(tiers.GetArrayElementAtIndex(0), CCS_CampTier.TemporaryCamp, "Temporary Camp", CCS_CampTier.None,
                new[] { CCS_CampStructureKind.Shelter, CCS_CampStructureKind.Campfire, CCS_CampStructureKind.Bedroll });
            ConfigureTier(tiers.GetArrayElementAtIndex(1), CCS_CampTier.FrontierCamp, "Frontier Camp", CCS_CampTier.TemporaryCamp,
                new[] { CCS_CampStructureKind.Storage });
            ConfigureTier(tiers.GetArrayElementAtIndex(2), CCS_CampTier.FrontierHomestead, "Frontier Homestead", CCS_CampTier.FrontierCamp,
                new[] { CCS_CampStructureKind.WorkArea });

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static void ConfigureTier(
            SerializedProperty tierProperty,
            CCS_CampTier tier,
            string displayName,
            CCS_CampTier prerequisite,
            CCS_CampStructureKind[] requirements)
        {
            tierProperty.FindPropertyRelative("campTier").enumValueIndex = (int)tier;
            tierProperty.FindPropertyRelative("displayName").stringValue = displayName;
            tierProperty.FindPropertyRelative("prerequisiteTier").enumValueIndex = (int)prerequisite;
            SerializedProperty requirementList = tierProperty.FindPropertyRelative("requirements");
            requirementList.arraySize = requirements.Length;
            for (int index = 0; index < requirements.Length; index++)
            {
                SerializedProperty requirement = requirementList.GetArrayElementAtIndex(index);
                requirement.FindPropertyRelative("structureKind").enumValueIndex = (int)requirements[index];
            }
        }

        private static CCS_FrontierStorageCampProfile EnsureFrontierStorageCampProfile(
            CCS_StorageContainerDefinition supplyCrate,
            CCS_StorageContainerDefinition trapperChest)
        {
            CCS_FrontierStorageCampProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_FrontierStorageCampProfile>(FrontierStorageCampProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CCS_FrontierStorageCampProfile>();
                AssetDatabase.CreateAsset(profile, FrontierStorageCampProfilePath);
            }

            SerializedObject serialized = new SerializedObject(profile);
            serialized.FindProperty("profileVersion").stringValue = "1.4.1";
            serialized.FindProperty("profileDescription").stringValue =
                "Frontier placeable storage definitions for camp tier progression (1.4.1).";
            SerializedProperty storageList = serialized.FindProperty("frontierStorageDefinitions");
            storageList.arraySize = 2;
            storageList.GetArrayElementAtIndex(0).objectReferenceValue = supplyCrate;
            storageList.GetArrayElementAtIndex(1).objectReferenceValue = trapperChest;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static CCS_CampDefinition EnsureCampDefinition(
            CCS_CampTierProfile tierProfile,
            CCS_WorkbenchDefinition workbench)
        {
            CCS_CampDefinition definition = AssetDatabase.LoadAssetAtPath<CCS_CampDefinition>(CampProfilePath);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<CCS_CampDefinition>();
                AssetDatabase.CreateAsset(definition, CampProfilePath);
            }

            SerializedObject serialized = new SerializedObject(definition);
            serialized.FindProperty("profileVersion").stringValue = "1.4.1";
            serialized.FindProperty("profileDescription").stringValue =
                "Frontier homestead camp tracking with tier profile, storage, and workbench (1.4.1).";
            serialized.FindProperty("campTierProfile").objectReferenceValue = tierProfile;

            SerializedProperty workbenchList = serialized.FindProperty("workbenchDefinitions");
            workbenchList.arraySize = 1;
            workbenchList.GetArrayElementAtIndex(0).objectReferenceValue = workbench;

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static void AssignCampDefinitionToBootstrapHost(CCS_CampDefinition campDefinition)
        {
            AssignHostProfileReference("campDefinition", campDefinition);
        }

        private static void AssignFrontierStorageCampProfileToBootstrapHost(CCS_FrontierStorageCampProfile profile)
        {
            AssignHostProfileReference("frontierStorageCampProfile", profile);
        }

        private static void AssignHostProfileReference(string propertyName, Object profile)
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
            serialized.FindProperty(propertyName).objectReferenceValue = profile;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(host);
            EditorUtility.SetDirty(prefabRoot);
        }

        private static void EnsureGeneralStoreHomesteadCatalog(
            CCS_ItemDefinition supplyCrateKit,
            CCS_ItemDefinition workbenchKit,
            CCS_ItemDefinition canvasBundle,
            CCS_ItemDefinition lumberBundle)
        {
            CCS_VendorDefinition vendor = AssetDatabase.LoadAssetAtPath<CCS_VendorDefinition>(GeneralStoreVendorPath);
            if (vendor == null)
            {
                return;
            }

            SerializedObject serialized = new SerializedObject(vendor);
            serialized.FindProperty("description").stringValue =
                "Frontier general store (1.4.1). Sells homestead kits and material bundles.";
            SerializedProperty items = serialized.FindProperty("vendorInventory").FindPropertyRelative("items");
            MergeVendorRow(items, supplyCrateKit, true, false, 12);
            MergeVendorRow(items, workbenchKit, true, false, 18);
            MergeVendorRow(items, canvasBundle, true, false, 10);
            MergeVendorRow(items, lumberBundle, true, false, 10);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(vendor);
        }

        private static void MergeVendorRow(
            SerializedProperty items,
            CCS_ItemDefinition item,
            bool allowBuy,
            bool allowSell,
            int buyPrice)
        {
            if (item == null)
            {
                return;
            }

            for (int index = 0; index < items.arraySize; index++)
            {
                SerializedProperty entry = items.GetArrayElementAtIndex(index);
                if (entry.FindPropertyRelative("itemDefinition").objectReferenceValue != item)
                {
                    continue;
                }

                entry.FindPropertyRelative("allowBuy").boolValue = allowBuy;
                entry.FindPropertyRelative("allowSell").boolValue = allowSell;
                entry.FindPropertyRelative("buyPriceOverride").intValue = allowBuy ? buyPrice : -1;
                return;
            }

            int newIndex = items.arraySize;
            items.InsertArrayElementAtIndex(newIndex);
            SerializedProperty newEntry = items.GetArrayElementAtIndex(newIndex);
            newEntry.FindPropertyRelative("itemDefinition").objectReferenceValue = item;
            newEntry.FindPropertyRelative("stockQuantity").intValue = -1;
            newEntry.FindPropertyRelative("allowBuy").boolValue = allowBuy;
            newEntry.FindPropertyRelative("allowSell").boolValue = allowSell;
            newEntry.FindPropertyRelative("buyPriceOverride").intValue = allowBuy ? buyPrice : -1;
            newEntry.FindPropertyRelative("sellPriceOverride").intValue = allowSell ? 4 : -1;
        }

        private static void EnsurePlaytestHomesteadSteps()
        {
            CCS_PlaytestProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(DefaultPlaytestProfilePath);
            if (profile == null)
            {
                return;
            }

            SerializedObject serializedProfile = new SerializedObject(profile);
            SerializedProperty stepList = serializedProfile.FindProperty("stepDefinitions");
            RemoveHomesteadSteps(stepList);

            int insertIndex = FindStepIndex(stepList, ShelterCampPersistenceStepId);
            if (insertIndex < 0)
            {
                insertIndex = stepList.arraySize;
            }
            else
            {
                insertIndex += 1;
            }

            InsertStep(stepList, ref insertIndex, "ccs.survival.playtest.homestead.buy.crate", "Buy Supply Crate kit", CCS_PlaytestStepType.BuySupplyCrateKitForHomestead, "Press Ctrl+Shift+K to grant Supply Crate kit.", SupplyCrateKitItemId);
            InsertStep(stepList, ref insertIndex, "ccs.survival.playtest.homestead.place.crate", "Place Supply Crate", CCS_PlaytestStepType.PlaceSupplyCrateForFrontierCamp, "Equip kit and use twice to place storage in camp radius.", SupplyCrateKitItemId);
            InsertStep(stepList, ref insertIndex, "ccs.survival.playtest.homestead.verify.frontiercamp", "Verify FrontierCamp", CCS_PlaytestStepType.VerifyFrontierCampTier, "Camp tier should be FrontierCamp with storage.");
            InsertStep(stepList, ref insertIndex, "ccs.survival.playtest.homestead.buy.workbench", "Buy Workbench kit", CCS_PlaytestStepType.BuyWorkbenchKitForHomestead, "Press Ctrl+Shift+W to grant Workbench kit.", WorkbenchKitItemId);
            InsertStep(stepList, ref insertIndex, "ccs.survival.playtest.homestead.place.workbench", "Place Workbench", CCS_PlaytestStepType.PlaceWorkbenchForHomestead, "Place workbench within camp radius.", WorkbenchKitItemId);
            InsertStep(stepList, ref insertIndex, "ccs.survival.playtest.homestead.verify.homestead", "Verify FrontierHomestead", CCS_PlaytestStepType.VerifyFrontierHomesteadTier, "Camp tier should be FrontierHomestead.");
            InsertStep(stepList, ref insertIndex, "ccs.survival.playtest.homestead.save", "Save homestead camp", CCS_PlaytestStepType.SaveHomesteadCampState, "Save game with homestead tier.");
            InsertStep(stepList, ref insertIndex, "ccs.survival.playtest.homestead.verify.load", "Verify homestead save/load", CCS_PlaytestStepType.VerifyHomesteadCampPersistenceAfterLoad, "Load save; homestead tier and structures should restore.");

            serializedProfile.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static void BumpProfileVersions()
        {
            UpdateProjectVersion();
            BumpVersion("Assets/CCS/Survival/Profiles/Economy/CCS_DefaultEconomyProfile.asset", "1.4.1");
            BumpVersion("Assets/CCS/Survival/Profiles/Economy/CCS_DefaultVendorProfile.asset", "1.4.1");

            CCS_PlaytestProfile playtest = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(DefaultPlaytestProfilePath);
            if (playtest != null)
            {
                SerializedObject serialized = new SerializedObject(playtest);
                serialized.FindProperty("profileVersion").stringValue = "1.4.1";
                serialized.FindProperty("profileDescription").stringValue =
                    "Frontier progression with shelter camp and homestead tier loop (1.4.1).";
                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(playtest);
            }
        }

        private static void BumpVersion(string path, string version)
        {
            ScriptableObject asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
            if (asset == null)
            {
                return;
            }

            SerializedObject serialized = new SerializedObject(asset);
            serialized.FindProperty("profileVersion").stringValue = version;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(asset);
        }

        private static void RemoveHomesteadSteps(SerializedProperty stepList)
        {
            for (int index = stepList.arraySize - 1; index >= 0; index--)
            {
                SerializedProperty step = stepList.GetArrayElementAtIndex(index);
                CCS_PlaytestStepType stepType = (CCS_PlaytestStepType)step.FindPropertyRelative("stepType").enumValueIndex;
                if (stepType >= CCS_PlaytestStepType.BuySupplyCrateKitForHomestead
                    && stepType <= CCS_PlaytestStepType.VerifyHomesteadCampPersistenceAfterLoad)
                {
                    stepList.DeleteArrayElementAtIndex(index);
                }
            }
        }

        private static int FindStepIndex(SerializedProperty stepList, string stepId)
        {
            for (int index = 0; index < stepList.arraySize; index++)
            {
                if (stepList.GetArrayElementAtIndex(index).FindPropertyRelative("stepId").stringValue == stepId)
                {
                    return index;
                }
            }

            return -1;
        }

        private static void InsertStep(
            SerializedProperty stepList,
            ref int insertIndex,
            string stepId,
            string displayName,
            CCS_PlaytestStepType stepType,
            string instructionText,
            string targetItemId = "")
        {
            stepList.InsertArrayElementAtIndex(insertIndex);
            SerializedProperty step = stepList.GetArrayElementAtIndex(insertIndex);
            step.FindPropertyRelative("stepId").stringValue = stepId;
            step.FindPropertyRelative("displayName").stringValue = displayName;
            step.FindPropertyRelative("stepType").enumValueIndex = (int)stepType;
            step.FindPropertyRelative("instructionText").stringValue = instructionText;
            step.FindPropertyRelative("targetItemId").stringValue = targetItemId ?? string.Empty;
            step.FindPropertyRelative("targetObjectId").stringValue = string.Empty;
            step.FindPropertyRelative("requiredCount").intValue = 1;
            insertIndex++;
        }
    }
}
