using System.Collections.Generic;
using System.IO;
using CCS.Modules.Economy;
using CCS.Modules.Inventory;
using CCS.Modules.Mounts;
using CCS.Modules.Playtesting;
using CCS.Modules.Storage;
using CCS.Modules.Vehicles;
using CCS.Survival.Composition;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEngine;

namespace CCS.Modules.Vehicles.Editor
{
    public static class CCS_WagonFoundationBootstrapSetup
    {
        private const string LogPrefix = "[CCS_WagonFoundationBootstrapSetup]";
        private const string VehicleProfilePath = "Assets/CCS/Survival/Profiles/Vehicles/CCS_DefaultVehicleProfile.asset";
        private const string VehicleContentRoot = "Assets/CCS/Survival/Content/Vehicles";
        private const string WagonDefinitionPath = VehicleContentRoot + "/CCS_Vehicle_FrontierWagon.asset";
        private const string WagonPrefabPath = "Assets/CCS/Survival/Prefabs/Vehicles/PF_CCS_FrontierWagon.prefab";
        private const string WagonItemPath = VehicleContentRoot + "/CCS_Item_FrontierWagonDeed.asset";
        private const string HorsePrefabPath = "Assets/CCS/Survival/Prefabs/Mounts/PF_CCS_Horse.prefab";
        private const string StableVendorPath = "Assets/CCS/Survival/Content/Vendors/CCS_Vendor_FrontierStable.asset";
        private const string InventoryProfilePath = "Assets/CCS/Survival/Profiles/Inventory/CCS_DefaultInventoryProfile.asset";
        private const string BootstrapRootPrefabPath = "Assets/CCS/Survival/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
        private const string DefaultPlaytestProfilePath = "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset";
        private const string TradeDollarsPath = "Assets/CCS/Survival/Profiles/Economy/Currencies/CCS_Currency_TradeDollars.asset";

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

            EnsureHorseWagonHitchPoint();
            GameObject wagonPrefab = EnsureWagonPrefab();
            CCS_VehicleDefinition wagonDefinition = EnsureWagonDefinition(wagonPrefab);
            CCS_ItemDefinition wagonItem = EnsureFrontierWagonDeedItem();
            CCS_VehicleProfile vehicleProfile = EnsureVehicleProfile(wagonDefinition);
            EnsureStableVendorSellsWagon(tradeDollars, wagonItem);
            EnsureInventorySaveRestore(wagonItem);
            AssignVehicleProfileToBootstrapHost(vehicleProfile);
            EnsurePlaytestWagonSteps();
            BumpVersions();

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} Wagon foundation bootstrap setup complete (1.5.2).");
            EditorApplication.Exit(0);
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/CCS/Survival/Profiles/Vehicles");
            EnsureFolder(VehicleContentRoot);
            EnsureFolder("Assets/CCS/Survival/Prefabs/Vehicles");
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

        private static void EnsureHorseWagonHitchPoint()
        {
            GameObject horsePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(HorsePrefabPath);
            if (horsePrefab == null)
            {
                return;
            }

            GameObject root = PrefabUtility.LoadPrefabContents(HorsePrefabPath);
            Transform hitch = root.transform.Find("WagonHitchPoint");
            if (hitch == null)
            {
                GameObject hitchObject = new GameObject("WagonHitchPoint");
                hitchObject.transform.SetParent(root.transform, false);
                hitchObject.transform.localPosition = new Vector3(0f, 0.85f, -1.35f);
                hitchObject.transform.localRotation = Quaternion.identity;
            }

            CCS_MountWorldActor mountActor = root.GetComponent<CCS_MountWorldActor>();
            if (mountActor != null)
            {
                SerializedObject serialized = new SerializedObject(mountActor);
                serialized.FindProperty("wagonHitchPoint").objectReferenceValue =
                    root.transform.Find("WagonHitchPoint");
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }

            PrefabUtility.SaveAsPrefabAsset(root, HorsePrefabPath);
            PrefabUtility.UnloadPrefabContents(root);
        }

        private static GameObject EnsureWagonPrefab()
        {
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(WagonPrefabPath);
            if (existing != null)
            {
                return existing;
            }

            GameObject root = GameObject.CreatePrimitive(PrimitiveType.Cube);
            root.name = CCS_VehicleContentIds.WagonPrefabName;
            root.transform.localScale = new Vector3(1.8f, 0.9f, 2.8f);

            Collider primitiveCollider = root.GetComponent<Collider>();
            if (primitiveCollider != null)
            {
                Object.DestroyImmediate(primitiveCollider);
            }

            root.AddComponent<CCS_VehicleWorldActor>();
            root.AddComponent<CCS_VehicleInteractable>();
            root.AddComponent<CCS_StorageContainer>();
            root.AddComponent<CCS_WagonCargoContainer>();
            root.AddComponent<CCS_StorageContainerInteractable>();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, WagonPrefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static CCS_VehicleDefinition EnsureWagonDefinition(GameObject wagonPrefab)
        {
            CCS_VehicleDefinition definition = AssetDatabase.LoadAssetAtPath<CCS_VehicleDefinition>(WagonDefinitionPath);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<CCS_VehicleDefinition>();
                AssetDatabase.CreateAsset(definition, WagonDefinitionPath);
            }

            SerializedObject serialized = new SerializedObject(definition);
            serialized.FindProperty("vehicleId").stringValue = CCS_VehicleContentIds.FrontierWagonVehicleId;
            serialized.FindProperty("displayName").stringValue = "Frontier Wagon";
            serialized.FindProperty("description").stringValue =
                "Frontier logistics wagon for expanded mobile cargo and horse towing.";
            serialized.FindProperty("movementDragPlaceholder").floatValue = 1.25f;
            serialized.FindProperty("cargoSlotCount").intValue = 24;
            serialized.FindProperty("cargoContainerDefinitionId").stringValue =
                CCS_VehicleContentIds.WagonCargoContainerId;
            serialized.FindProperty("purchaseValue").intValue = 1800;
            serialized.FindProperty("worldPrefab").objectReferenceValue = wagonPrefab;
            serialized.FindProperty("followOffsetLocal").vector3Value = new Vector3(0f, 0f, -3.25f);
            serialized.FindProperty("followSmoothing").floatValue = 10f;
            SerializedProperty hitchIds = serialized.FindProperty("hitchCompatibleMountIds");
            hitchIds.arraySize = 1;
            hitchIds.GetArrayElementAtIndex(0).stringValue = CCS_VehicleContentIds.HitchCompatibleHorseMountId;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static CCS_ItemDefinition EnsureFrontierWagonDeedItem()
        {
            CCS_ItemDefinition item = AssetDatabase.LoadAssetAtPath<CCS_ItemDefinition>(WagonItemPath);
            if (item == null)
            {
                item = ScriptableObject.CreateInstance<CCS_ItemDefinition>();
                AssetDatabase.CreateAsset(item, WagonItemPath);
            }

            SerializedObject serialized = new SerializedObject(item);
            serialized.FindProperty("itemId").stringValue = CCS_VehicleContentIds.FrontierWagonItemId;
            serialized.FindProperty("displayName").stringValue = "Frontier Wagon Deed";
            serialized.FindProperty("description").stringValue =
                "Wagon ownership deed purchased at the frontier stable. Grants mobile cargo and horse hitching.";
            serialized.FindProperty("maxStackSize").intValue = 1;
            serialized.FindProperty("buyValue").intValue = 1800;
            serialized.FindProperty("sellValue").intValue = 350;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(item);
            return item;
        }

        private static CCS_VehicleProfile EnsureVehicleProfile(CCS_VehicleDefinition wagonDefinition)
        {
            CCS_VehicleProfile profile = AssetDatabase.LoadAssetAtPath<CCS_VehicleProfile>(VehicleProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CCS_VehicleProfile>();
                AssetDatabase.CreateAsset(profile, VehicleProfilePath);
            }

            SerializedObject serialized = new SerializedObject(profile);
            SerializedProperty definitions = serialized.FindProperty("vehicleDefinitions");
            definitions.arraySize = 1;
            definitions.GetArrayElementAtIndex(0).objectReferenceValue = wagonDefinition;
            serialized.FindProperty("defaultFrontierWagonDefinition").objectReferenceValue = wagonDefinition;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static void EnsureStableVendorSellsWagon(CCS_CurrencyDefinition currency, CCS_ItemDefinition wagonItem)
        {
            CCS_VendorDefinition vendor = AssetDatabase.LoadAssetAtPath<CCS_VendorDefinition>(StableVendorPath);
            if (vendor == null)
            {
                return;
            }

            CCS_ItemDefinition horseItem =
                AssetDatabase.LoadAssetAtPath<CCS_ItemDefinition>(
                    "Assets/CCS/Survival/Content/Mounts/CCS_Item_FrontierHorse.asset");

            SerializedObject serialized = new SerializedObject(vendor);
            serialized.FindProperty("description").stringValue =
                "Frontier stable for horse and wagon purchases (1.5.2).";
            serialized.FindProperty("currencyDefinition").objectReferenceValue = currency;

            SerializedProperty catalogItems = serialized.FindProperty("vendorInventory").FindPropertyRelative("items");
            catalogItems.arraySize = 2;
            SetVendorCatalogEntry(catalogItems, 0, horseItem, true, true, 2500);
            SetVendorCatalogEntry(catalogItems, 1, wagonItem, true, true, 1800);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(vendor);
        }

        private static void SetVendorCatalogEntry(
            SerializedProperty catalogItems,
            int index,
            CCS_ItemDefinition item,
            bool allowBuy,
            bool allowSell,
            int buyOverride)
        {
            SerializedProperty entry = catalogItems.GetArrayElementAtIndex(index);
            entry.FindPropertyRelative("itemDefinition").objectReferenceValue = item;
            entry.FindPropertyRelative("stockQuantity").intValue = -1;
            entry.FindPropertyRelative("allowBuy").boolValue = allowBuy;
            entry.FindPropertyRelative("allowSell").boolValue = allowSell;
            entry.FindPropertyRelative("buyPriceOverride").intValue = buyOverride;
            entry.FindPropertyRelative("sellPriceOverride").intValue = buyOverride / 5;
        }

        private static void EnsureInventorySaveRestore(CCS_ItemDefinition wagonItem)
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

            if (!merged.Contains(wagonItem))
            {
                merged.Add(wagonItem);
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

        private static void AssignVehicleProfileToBootstrapHost(CCS_VehicleProfile profile)
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
            serialized.FindProperty("vehicleProfile").objectReferenceValue = profile;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(host);
            PrefabUtility.SavePrefabAsset(prefabRoot);
        }

        private static void EnsurePlaytestWagonSteps()
        {
            CCS_PlaytestProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(DefaultPlaytestProfilePath);
            if (profile == null)
            {
                return;
            }

            InsertStep(profile, "ccs.survival.playtest.wagon.earn.currency", "Earn wagon currency", CCS_PlaytestStepType.EarnCurrencyForWagon, string.Empty);
            InsertStep(profile, "ccs.survival.playtest.wagon.buy", "Buy frontier wagon", CCS_PlaytestStepType.BuyWagonFromStable, CCS_VehicleContentIds.FrontierWagonItemId);
            InsertStep(profile, "ccs.survival.playtest.wagon.summon", "Summon wagon", CCS_PlaytestStepType.SummonWagon, string.Empty);
            InsertStep(profile, "ccs.survival.playtest.wagon.hitch", "Hitch wagon to horse", CCS_PlaytestStepType.HitchWagonToHorse, string.Empty);
            InsertStep(profile, "ccs.survival.playtest.wagon.ride", "Ride horse with wagon", CCS_PlaytestStepType.RideHorseWithWagon, string.Empty);
            InsertStep(profile, "ccs.survival.playtest.wagon.cargo", "Open wagon cargo", CCS_PlaytestStepType.OpenWagonCargo, string.Empty);
            InsertStep(profile, "ccs.survival.playtest.wagon.save", "Save wagon state", CCS_PlaytestStepType.SaveWagonState, string.Empty);
            InsertStep(profile, "ccs.survival.playtest.wagon.verify.load", "Verify wagon after load", CCS_PlaytestStepType.VerifyWagonPersistenceAfterLoad, string.Empty);
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
                $"Wagon playtest: {displayName}. Ctrl+Shift+W shortcuts available.";
            step.FindPropertyRelative("targetItemId").stringValue = targetItemId ?? string.Empty;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void BumpVersions()
        {
            CCS_SurvivalBootstrapVersionUtility.EnsureBundleVersionAtLeast(CCS_SurvivalBootstrapVersionUtility.CurrentMilestoneVersion);
        }
    }
}
