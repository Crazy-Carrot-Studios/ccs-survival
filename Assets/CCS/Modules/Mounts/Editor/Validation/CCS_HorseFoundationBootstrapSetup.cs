using System.Collections.Generic;
using System.IO;
using CCS.Modules.Economy;
using CCS.Modules.Inventory;
using CCS.Modules.Mounts;
using CCS.Modules.Playtesting;
using CCS.Modules.Storage;
using CCS.Survival.Composition;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEngine;

namespace CCS.Modules.Mounts.Editor
{
    public static class CCS_HorseFoundationBootstrapSetup
    {
        private const string LogPrefix = "[CCS_HorseFoundationBootstrapSetup]";
        private const string MountProfilePath = "Assets/CCS/Survival/Profiles/Mounts/CCS_DefaultMountProfile.asset";
        private const string MountContentRoot = "Assets/CCS/Survival/Content/Mounts";
        private const string HorseDefinitionPath = MountContentRoot + "/CCS_Mount_Horse.asset";
        private const string HorsePrefabPath = "Assets/CCS/Survival/Prefabs/Mounts/PF_CCS_Horse.prefab";
        private const string HorseItemPath = MountContentRoot + "/CCS_Item_FrontierHorse.asset";
        private const string StableVendorPath = "Assets/CCS/Survival/Content/Vendors/CCS_Vendor_FrontierStable.asset";
        private const string EconomyProfilePath = "Assets/CCS/Survival/Profiles/Economy/CCS_DefaultEconomyProfile.asset";
        private const string VendorProfilePath = "Assets/CCS/Survival/Profiles/Economy/CCS_DefaultVendorProfile.asset";
        private const string InventoryProfilePath = "Assets/CCS/Survival/Profiles/Inventory/CCS_DefaultInventoryProfile.asset";
        private const string BootstrapRootPrefabPath = "Assets/CCS/Survival/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
        private const string DefaultPlaytestProfilePath = "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset";
        private const string TradeDollarsPath = "Assets/CCS/Survival/Profiles/Economy/Currencies/CCS_Currency_TradeDollars.asset";
        private const string TestStableObjectName = "CCS_TestFrontierStable";

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

            GameObject horsePrefab = EnsureHorsePrefab();
            CCS_MountDefinition horseDefinition = EnsureHorseDefinition(horsePrefab);
            CCS_ItemDefinition frontierHorseItem = EnsureFrontierHorseItem();
            CCS_MountProfile mountProfile = EnsureMountProfile(horseDefinition);
            CCS_VendorDefinition stableVendor = EnsureStableVendor(tradeDollars, frontierHorseItem);
            EnsureVendorProfileIncludesStable(stableVendor);
            EnsureInventorySaveRestore(frontierHorseItem);
            AssignMountProfileToBootstrapHost(mountProfile);
            EnsureBootstrapSceneStable(stableVendor);
            EnsurePlaytestHorseSteps();
            BumpVersions();

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} Horse foundation bootstrap setup complete (1.5.1).");
            EditorApplication.Exit(0);
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/CCS/Survival/Profiles/Mounts");
            EnsureFolder(MountContentRoot);
            EnsureFolder("Assets/CCS/Survival/Prefabs/Mounts");
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

        private static GameObject EnsureHorsePrefab()
        {
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(HorsePrefabPath);
            if (existing != null)
            {
                return existing;
            }

            GameObject root = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            root.name = CCS_MountContentIds.HorsePrefabName;
            root.transform.localScale = new Vector3(1.2f, 1.1f, 2.4f);

            CapsuleCollider primitiveCollider = root.GetComponent<CapsuleCollider>();
            if (primitiveCollider != null)
            {
                Object.DestroyImmediate(primitiveCollider);
            }

            UnityEngine.CharacterController controller = root.AddComponent<UnityEngine.CharacterController>();
            controller.height = 2.2f;
            controller.radius = 0.55f;
            controller.center = new Vector3(0f, 1.1f, 0f);

            root.AddComponent<CCS_MountWorldActor>();
            root.AddComponent<CCS_MountInteractable>();
            root.AddComponent<CCS_StorageContainer>();
            root.AddComponent<CCS_HorseSaddlebagContainer>();
            root.AddComponent<CCS_StorageContainerInteractable>();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, HorsePrefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static CCS_MountDefinition EnsureHorseDefinition(GameObject horsePrefab)
        {
            CCS_MountDefinition definition = AssetDatabase.LoadAssetAtPath<CCS_MountDefinition>(HorseDefinitionPath);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<CCS_MountDefinition>();
                AssetDatabase.CreateAsset(definition, HorseDefinitionPath);
            }

            SerializedObject serialized = new SerializedObject(definition);
            serialized.FindProperty("mountId").stringValue = CCS_MountContentIds.HorseMountId;
            serialized.FindProperty("displayName").stringValue = "Frontier Horse";
            serialized.FindProperty("description").stringValue =
                "First mount milestone for frontier travel, saddlebag storage, and camp presence.";
            serialized.FindProperty("movementSpeed").floatValue = 4.75f;
            serialized.FindProperty("sprintSpeed").floatValue = 7.5f;
            serialized.FindProperty("staminaPlaceholder").floatValue = 100f;
            serialized.FindProperty("carryCapacityBonus").intValue = 0;
            serialized.FindProperty("purchaseValue").intValue = 2500;
            serialized.FindProperty("worldPrefab").objectReferenceValue = horsePrefab;
            serialized.FindProperty("saddlebagContainerDefinitionId").stringValue =
                CCS_MountContentIds.HorseSaddlebagContainerId;
            serialized.FindProperty("saddlebagSlotCount").intValue = 12;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static CCS_ItemDefinition EnsureFrontierHorseItem()
        {
            CCS_ItemDefinition item = AssetDatabase.LoadAssetAtPath<CCS_ItemDefinition>(HorseItemPath);
            if (item == null)
            {
                item = ScriptableObject.CreateInstance<CCS_ItemDefinition>();
                AssetDatabase.CreateAsset(item, HorseItemPath);
            }

            SerializedObject serialized = new SerializedObject(item);
            serialized.FindProperty("itemId").stringValue = CCS_MountContentIds.FrontierHorseItemId;
            serialized.FindProperty("displayName").stringValue = "Frontier Horse";
            serialized.FindProperty("description").stringValue =
                "Horse ownership deed purchased at the frontier stable. Grants a ridden mount and saddlebags.";
            serialized.FindProperty("maxStackSize").intValue = 1;
            serialized.FindProperty("buyValue").intValue = 2500;
            serialized.FindProperty("sellValue").intValue = 500;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(item);
            return item;
        }

        private static CCS_MountProfile EnsureMountProfile(CCS_MountDefinition horseDefinition)
        {
            CCS_MountProfile profile = AssetDatabase.LoadAssetAtPath<CCS_MountProfile>(MountProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CCS_MountProfile>();
                AssetDatabase.CreateAsset(profile, MountProfilePath);
            }

            SerializedObject serialized = new SerializedObject(profile);
            SerializedProperty definitions = serialized.FindProperty("mountDefinitions");
            definitions.arraySize = 1;
            definitions.GetArrayElementAtIndex(0).objectReferenceValue = horseDefinition;
            serialized.FindProperty("defaultHorseDefinition").objectReferenceValue = horseDefinition;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static CCS_VendorDefinition EnsureStableVendor(
            CCS_CurrencyDefinition currency,
            CCS_ItemDefinition horseItem)
        {
            CCS_VendorDefinition vendor = AssetDatabase.LoadAssetAtPath<CCS_VendorDefinition>(StableVendorPath);
            if (vendor == null)
            {
                vendor = ScriptableObject.CreateInstance<CCS_VendorDefinition>();
                AssetDatabase.CreateAsset(vendor, StableVendorPath);
            }

            SerializedObject serialized = new SerializedObject(vendor);
            serialized.FindProperty("vendorId").stringValue = CCS_MountContentIds.FrontierStableVendorId;
            serialized.FindProperty("displayName").stringValue = "Frontier Stable";
            serialized.FindProperty("description").stringValue =
                "Frontier stable for horse purchases and future horse supplies (1.5.1).";
            serialized.FindProperty("currencyDefinition").objectReferenceValue = currency;

            SerializedProperty catalogItems = serialized.FindProperty("vendorInventory").FindPropertyRelative("items");
            catalogItems.arraySize = 1;
            SetVendorCatalogEntry(catalogItems, 0, horseItem, true, true, 2500);

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(vendor);
            return vendor;
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
            entry.FindPropertyRelative("sellPriceOverride").intValue = 500;
        }

        private static void EnsureVendorProfileIncludesStable(CCS_VendorDefinition stableVendor)
        {
            CCS_VendorProfile vendorProfile = AssetDatabase.LoadAssetAtPath<CCS_VendorProfile>(VendorProfilePath);
            if (vendorProfile == null)
            {
                return;
            }

            List<CCS_VendorDefinition> merged = new List<CCS_VendorDefinition>();
            CCS_VendorDefinition[] existing = vendorProfile.VendorDefinitions;
            for (int index = 0; index < existing.Length; index++)
            {
                if (existing[index] != null && !merged.Contains(existing[index]))
                {
                    merged.Add(existing[index]);
                }
            }

            if (!merged.Contains(stableVendor))
            {
                merged.Add(stableVendor);
            }

            SerializedObject serialized = new SerializedObject(vendorProfile);
            SerializedProperty vendors = serialized.FindProperty("vendorDefinitions");
            vendors.arraySize = merged.Count;
            for (int index = 0; index < merged.Count; index++)
            {
                vendors.GetArrayElementAtIndex(index).objectReferenceValue = merged[index];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(vendorProfile);
        }

        private static void EnsureInventorySaveRestore(CCS_ItemDefinition horseItem)
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

            if (!merged.Contains(horseItem))
            {
                merged.Add(horseItem);
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

        private static void AssignMountProfileToBootstrapHost(CCS_MountProfile profile)
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
            serialized.FindProperty("mountProfile").objectReferenceValue = profile;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(host);
            PrefabUtility.SavePrefabAsset(prefabRoot);
        }

        private static void EnsureBootstrapSceneStable(CCS_VendorDefinition stableVendor)
        {
            const string bootstrapScenePath = "Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity";
            if (!File.Exists(bootstrapScenePath))
            {
                return;
            }

            UnityEngine.SceneManagement.Scene scene =
                UnityEditor.SceneManagement.EditorSceneManager.OpenScene(bootstrapScenePath);
            GameObject sceneRoot = GameObject.Find("CCS_SurvivalBootstrapSceneRoot");
            if (sceneRoot == null)
            {
                return;
            }

            Transform existing = sceneRoot.transform.Find(TestStableObjectName);
            GameObject stableObject = existing != null ? existing.gameObject : new GameObject(TestStableObjectName);
            if (existing == null)
            {
                stableObject.transform.SetParent(sceneRoot.transform, false);
                stableObject.transform.localPosition = new Vector3(8f, 0f, 6f);
            }

            CCS_VendorInteractable interactable = stableObject.GetComponent<CCS_VendorInteractable>();
            if (interactable == null)
            {
                interactable = stableObject.AddComponent<CCS_VendorInteractable>();
            }

            SerializedObject serialized = new SerializedObject(interactable);
            serialized.FindProperty("vendorDefinition").objectReferenceValue = stableVendor;
            serialized.FindProperty("interactionDisplayNameOverride").stringValue = "Frontier Stable";
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(stableObject);
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
        }

        private static void EnsurePlaytestHorseSteps()
        {
            CCS_PlaytestProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(DefaultPlaytestProfilePath);
            if (profile == null)
            {
                return;
            }

            InsertStep(profile, "ccs.survival.playtest.horse.earn.currency", "Earn horse currency", CCS_PlaytestStepType.EarnCurrencyForHorse, string.Empty);
            InsertStep(profile, "ccs.survival.playtest.horse.buy", "Buy frontier horse", CCS_PlaytestStepType.BuyHorseFromStable, CCS_MountContentIds.FrontierHorseItemId);
            InsertStep(profile, "ccs.survival.playtest.horse.summon", "Summon horse", CCS_PlaytestStepType.SummonHorse, string.Empty);
            InsertStep(profile, "ccs.survival.playtest.horse.mount", "Mount horse", CCS_PlaytestStepType.MountHorse, string.Empty);
            InsertStep(profile, "ccs.survival.playtest.horse.ride", "Ride horse", CCS_PlaytestStepType.RideHorse, string.Empty);
            InsertStep(profile, "ccs.survival.playtest.horse.saddlebag", "Open saddlebag", CCS_PlaytestStepType.OpenHorseSaddlebag, string.Empty);
            InsertStep(profile, "ccs.survival.playtest.horse.save", "Save horse state", CCS_PlaytestStepType.SaveHorseState, string.Empty);
            InsertStep(profile, "ccs.survival.playtest.horse.verify.load", "Verify horse after load", CCS_PlaytestStepType.VerifyHorsePersistenceAfterLoad, string.Empty);
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
                $"Horse playtest: {displayName}. Ctrl+Shift+H shortcuts available.";
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
