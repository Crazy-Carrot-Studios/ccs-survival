using System.IO;
using CCS.Modules.Economy;
using CCS.Modules.Inventory;
using CCS.Modules.Playtesting;
using CCS.Survival.Composition;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_EconomyBootstrapSetup
// CATEGORY: Modules / Economy / Editor / Validation
// PURPOSE: Creates economy profiles, vendor catalog, item pricing, and bootstrap wiring.
// PLACEMENT: Batch entry for milestone 1.3.0 economy foundation.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: CCS_TestGeneralStore in bootstrap scene; playtest economy steps after fishing.
// =============================================================================

namespace CCS.Modules.Economy.Editor
{
    public static class CCS_EconomyBootstrapSetup
    {
        private const string EconomyProfilesRoot = "Assets/CCS/Survival/Profiles/Economy";
        private const string CurrenciesRoot = EconomyProfilesRoot + "/Currencies";
        private const string TradeDollarsCurrencyPath = CurrenciesRoot + "/CCS_Currency_TradeDollars.asset";
        private const string DefaultVendorProfilePath = EconomyProfilesRoot + "/CCS_DefaultVendorProfile.asset";
        private const string DefaultEconomyProfilePath = EconomyProfilesRoot + "/CCS_DefaultEconomyProfile.asset";
        private const string VendorsContentRoot = "Assets/CCS/Survival/Content/Vendors";
        private const string GeneralStoreVendorPath = VendorsContentRoot + "/CCS_Vendor_GeneralStore.asset";
        private const string StarterCoinItemPath = "Assets/CCS/Survival/Content/Items/Starter/CCS_Item_Coin.asset";
        private const string InventoryProfilePath = "Assets/CCS/Survival/Profiles/Inventory/CCS_DefaultInventoryProfile.asset";
        private const string PlaytestProfilePath = "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset";
        private const string BootstrapPrefabPath = "Assets/CCS/Survival/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
        private const string BootstrapScenePath = "Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string TestGeneralStoreObjectName = "CCS_TestGeneralStore";
        private const string FishingUseStepId = "ccs.survival.playtest.fishing.use";
        private const string LogPrefix = "[CCS_EconomyBootstrapSetup]";

        private const string TradeDollarsCurrencyId = "ccs.survival.currency.tradedollars";
        private const string GeneralStoreVendorId = "ccs.survival.vendor.frontier.generalstore";
        private const string RawFishItemId = "ccs.survival.item.resource.rawfish";
        private const string CordageItemId = "ccs.survival.item.frontier.cordage";

        public static void ExecuteBatch()
        {
            EnsureFolders();

            CCS_ItemDefinition coinItem = AssetDatabase.LoadAssetAtPath<CCS_ItemDefinition>(StarterCoinItemPath);
            if (coinItem == null)
            {
                Debug.LogError($"{LogPrefix} Missing starter coin item at {StarterCoinItemPath}.");
                EditorApplication.Exit(1);
                return;
            }

            ApplyItemEconomyValues();

            CCS_CurrencyDefinition tradeDollars = EnsureTradeDollarsCurrency(coinItem);
            CCS_VendorDefinition generalStore = EnsureGeneralStoreVendor(tradeDollars);
            CCS_VendorProfile vendorProfile = EnsureDefaultVendorProfile(generalStore);
            CCS_EconomyProfile economyProfile = EnsureDefaultEconomyProfile(tradeDollars, vendorProfile);

            EnsureBootstrapHost(economyProfile);
            EnsureInventoryCatalog(generalStore);
            EnsureBootstrapSceneGeneralStore(generalStore);
            EnsurePlaytestEconomySteps();

            UpdateProjectVersion();

            AssetDatabase.SaveAssets();
            Debug.Log($"{LogPrefix} Economy bootstrap setup complete.");
            EditorApplication.Exit(0);
        }

        private static void EnsureFolders()
        {
            CreateFolderChain("Assets/CCS/Survival/Profiles", "Economy/Currencies");
            CreateFolderChain("Assets/CCS/Survival/Content", "Vendors");
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

        private static void ApplyItemEconomyValues()
        {
            SetItemEconomyValues(
                "Assets/CCS/Survival/Content/Items/Fishing/CCS_Item_RawFish.asset",
                buyValue: 0,
                sellValue: 3);
            SetItemEconomyValues(
                "Assets/CCS/Survival/Content/Items/Fishing/CCS_Item_SmallFish.asset",
                buyValue: 0,
                sellValue: 2);
            SetItemEconomyValues(
                "Assets/CCS/Survival/Content/Items/Frontier/CCS_Item_Cordage.asset",
                buyValue: 4,
                sellValue: 0);
            SetItemEconomyValues(
                "Assets/CCS/Survival/Content/Items/Tools/Fishing/CCS_Item_FishingPole.asset",
                buyValue: 12,
                sellValue: 0);
            SetItemEconomyValues(
                "Assets/CCS/Survival/Content/Items/Fishing/CCS_Item_CrudeHook.asset",
                buyValue: 5,
                sellValue: 0);
            SetItemEconomyValues(
                "Assets/CCS/Survival/Content/Items/Fishing/CCS_Item_FishingLine.asset",
                buyValue: 3,
                sellValue: 0);
            SetItemEconomyValues(
                "Assets/CCS/Survival/Content/Items/Frontier/CCS_Item_Hardtack.asset",
                buyValue: 2,
                sellValue: 0);
            SetItemEconomyValues(
                "Assets/CCS/Survival/Content/Items/Frontier/CCS_Item_Tinderbox.asset",
                buyValue: 4,
                sellValue: 0);
            SetItemEconomyValues(
                "Assets/CCS/Survival/Content/Items/Frontier/CCS_Item_Arrow.asset",
                buyValue: 1,
                sellValue: 0);
        }

        private static void SetItemEconomyValues(string assetPath, int buyValue, int sellValue)
        {
            CCS_ItemDefinition item = AssetDatabase.LoadAssetAtPath<CCS_ItemDefinition>(assetPath);
            if (item == null)
            {
                Debug.LogWarning($"{LogPrefix} Skipping economy values; missing item at {assetPath}.");
                return;
            }

            SerializedObject serialized = new SerializedObject(item);
            serialized.FindProperty("hasEconomyValues").boolValue = true;
            serialized.FindProperty("buyValue").intValue = buyValue;
            serialized.FindProperty("sellValue").intValue = sellValue;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(item);
        }

        private static CCS_CurrencyDefinition EnsureTradeDollarsCurrency(CCS_ItemDefinition backingItem)
        {
            CCS_CurrencyDefinition currency =
                AssetDatabase.LoadAssetAtPath<CCS_CurrencyDefinition>(TradeDollarsCurrencyPath);
            if (currency == null)
            {
                currency = ScriptableObject.CreateInstance<CCS_CurrencyDefinition>();
                AssetDatabase.CreateAsset(currency, TradeDollarsCurrencyPath);
            }

            SerializedObject serialized = new SerializedObject(currency);
            serialized.FindProperty("currencyId").stringValue = TradeDollarsCurrencyId;
            serialized.FindProperty("displayName").stringValue = "Trade Dollars";
            serialized.FindProperty("description").stringValue =
                "Frontier general-store trade currency backed by starter coin items.";
            serialized.FindProperty("supportsStartingBalance").boolValue = true;
            serialized.FindProperty("inventoryBackingItem").objectReferenceValue = backingItem;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(currency);
            return currency;
        }

        private static CCS_VendorDefinition EnsureGeneralStoreVendor(CCS_CurrencyDefinition currency)
        {
            CCS_VendorDefinition vendor =
                AssetDatabase.LoadAssetAtPath<CCS_VendorDefinition>(GeneralStoreVendorPath);
            if (vendor == null)
            {
                vendor = ScriptableObject.CreateInstance<CCS_VendorDefinition>();
                AssetDatabase.CreateAsset(vendor, GeneralStoreVendorPath);
            }

            CCS_ItemDefinition fishingPole = LoadItem(
                "Assets/CCS/Survival/Content/Items/Tools/Fishing/CCS_Item_FishingPole.asset");
            CCS_ItemDefinition cordage = LoadItem("Assets/CCS/Survival/Content/Items/Frontier/CCS_Item_Cordage.asset");
            CCS_ItemDefinition crudeHook = LoadItem("Assets/CCS/Survival/Content/Items/Fishing/CCS_Item_CrudeHook.asset");
            CCS_ItemDefinition hardtack = LoadItem("Assets/CCS/Survival/Content/Items/Frontier/CCS_Item_Hardtack.asset");
            CCS_ItemDefinition tinderbox = LoadItem("Assets/CCS/Survival/Content/Items/Frontier/CCS_Item_Tinderbox.asset");
            CCS_ItemDefinition arrow = LoadItem("Assets/CCS/Survival/Content/Items/Frontier/CCS_Item_Arrow.asset");
            CCS_ItemDefinition rawFish = LoadItem("Assets/CCS/Survival/Content/Items/Fishing/CCS_Item_RawFish.asset");
            CCS_ItemDefinition smallFish = LoadItem("Assets/CCS/Survival/Content/Items/Fishing/CCS_Item_SmallFish.asset");

            SerializedObject serialized = new SerializedObject(vendor);
            serialized.FindProperty("vendorId").stringValue = GeneralStoreVendorId;
            serialized.FindProperty("displayName").stringValue = "Frontier General Store";
            serialized.FindProperty("description").stringValue =
                "Bootstrap general store for milestone 1.3.0 economy buy/sell validation.";
            serialized.FindProperty("currencyDefinition").objectReferenceValue = currency;

            SerializedProperty catalogItems = serialized.FindProperty("vendorInventory").FindPropertyRelative("items");
            catalogItems.arraySize = 8;
            SetVendorCatalogEntry(catalogItems, 0, fishingPole, allowBuy: true, allowSell: false);
            SetVendorCatalogEntry(catalogItems, 1, cordage, allowBuy: true, allowSell: false);
            SetVendorCatalogEntry(catalogItems, 2, crudeHook, allowBuy: true, allowSell: false);
            SetVendorCatalogEntry(catalogItems, 3, hardtack, allowBuy: true, allowSell: false);
            SetVendorCatalogEntry(catalogItems, 4, tinderbox, allowBuy: true, allowSell: false);
            SetVendorCatalogEntry(catalogItems, 5, arrow, allowBuy: true, allowSell: false);
            SetVendorCatalogEntry(catalogItems, 6, rawFish, allowBuy: false, allowSell: true);
            SetVendorCatalogEntry(catalogItems, 7, smallFish, allowBuy: false, allowSell: true);

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(vendor);
            return vendor;
        }

        private static void SetVendorCatalogEntry(
            SerializedProperty catalogItems,
            int index,
            CCS_ItemDefinition item,
            bool allowBuy,
            bool allowSell)
        {
            SerializedProperty entry = catalogItems.GetArrayElementAtIndex(index);
            entry.FindPropertyRelative("itemDefinition").objectReferenceValue = item;
            entry.FindPropertyRelative("stockQuantity").intValue = -1;
            entry.FindPropertyRelative("allowBuy").boolValue = allowBuy;
            entry.FindPropertyRelative("allowSell").boolValue = allowSell;
            entry.FindPropertyRelative("buyPriceOverride").intValue = -1;
            entry.FindPropertyRelative("sellPriceOverride").intValue = -1;
        }

        private static CCS_VendorProfile EnsureDefaultVendorProfile(CCS_VendorDefinition generalStore)
        {
            CCS_VendorProfile profile = AssetDatabase.LoadAssetAtPath<CCS_VendorProfile>(DefaultVendorProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CCS_VendorProfile>();
                AssetDatabase.CreateAsset(profile, DefaultVendorProfilePath);
            }

            SerializedObject serialized = new SerializedObject(profile);
            serialized.FindProperty("profileVersion").stringValue = "1.3.0";
            serialized.FindProperty("profileDescription").stringValue =
                "Frontier vendor catalog profile for milestone 1.3.0.";
            SerializedProperty vendors = serialized.FindProperty("vendorDefinitions");
            vendors.arraySize = 1;
            vendors.GetArrayElementAtIndex(0).objectReferenceValue = generalStore;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static CCS_EconomyProfile EnsureDefaultEconomyProfile(
            CCS_CurrencyDefinition tradeDollars,
            CCS_VendorProfile vendorProfile)
        {
            CCS_EconomyProfile profile = AssetDatabase.LoadAssetAtPath<CCS_EconomyProfile>(DefaultEconomyProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<CCS_EconomyProfile>();
                AssetDatabase.CreateAsset(profile, DefaultEconomyProfilePath);
            }

            SerializedObject serialized = new SerializedObject(profile);
            serialized.FindProperty("profileVersion").stringValue = "1.3.0";
            serialized.FindProperty("profileDescription").stringValue =
                "Frontier economy foundation profile (1.3.0).";
            SerializedProperty currencies = serialized.FindProperty("currencyDefinitions");
            currencies.arraySize = 1;
            currencies.GetArrayElementAtIndex(0).objectReferenceValue = tradeDollars;
            serialized.FindProperty("defaultCurrencyDefinition").objectReferenceValue = tradeDollars;
            serialized.FindProperty("vendorProfile").objectReferenceValue = vendorProfile;
            serialized.FindProperty("enableDebugLogging").boolValue = true;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static void EnsureBootstrapHost(CCS_EconomyProfile profile)
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
            serializedHost.FindProperty("economyProfile").objectReferenceValue = profile;
            serializedHost.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(prefabRoot);
        }

        private static void EnsureInventoryCatalog(CCS_VendorDefinition generalStore)
        {
            CCS_InventoryProfile profile = AssetDatabase.LoadAssetAtPath<CCS_InventoryProfile>(InventoryProfilePath);
            if (profile == null || generalStore?.VendorInventory?.Items == null)
            {
                Debug.LogWarning($"{LogPrefix} Could not merge vendor items into inventory save catalog.");
                return;
            }

            SerializedObject serialized = new SerializedObject(profile);
            SerializedProperty catalog = serialized.FindProperty("saveRestoreItemDefinitions");
            CCS_VendorItemEntry[] entries = generalStore.VendorInventory.Items;
            for (int index = 0; index < entries.Length; index++)
            {
                CCS_VendorItemEntry entry = entries[index];
                if (entry?.ItemDefinition != null)
                {
                    AppendUniqueItem(catalog, entry.ItemDefinition);
                }
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static void AppendUniqueItem(SerializedProperty catalog, CCS_ItemDefinition item)
        {
            for (int index = 0; index < catalog.arraySize; index++)
            {
                if (catalog.GetArrayElementAtIndex(index).objectReferenceValue == item)
                {
                    return;
                }
            }

            int newIndex = catalog.arraySize;
            catalog.InsertArrayElementAtIndex(newIndex);
            catalog.GetArrayElementAtIndex(newIndex).objectReferenceValue = item;
        }

        private static void EnsureBootstrapSceneGeneralStore(CCS_VendorDefinition vendorDefinition)
        {
            Scene scene = EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Single);
            Transform sceneRoot = FindSceneRoot();
            if (sceneRoot == null)
            {
                Debug.LogError($"{LogPrefix} Could not find CCS_BuildVerificationScene root.");
                EditorApplication.Exit(1);
                return;
            }

            Transform existing = sceneRoot.Find(TestGeneralStoreObjectName);
            GameObject storeObject;
            if (existing != null)
            {
                storeObject = existing.gameObject;
            }
            else
            {
                storeObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                storeObject.name = TestGeneralStoreObjectName;
                storeObject.transform.SetParent(sceneRoot, false);
            }

            storeObject.transform.position = new Vector3(12f, 0.5f, 8f);
            storeObject.transform.localScale = new Vector3(1.5f, 1f, 1.5f);

            Rigidbody rigidbody = storeObject.GetComponent<Rigidbody>();
            if (rigidbody != null)
            {
                Object.DestroyImmediate(rigidbody);
            }

            Collider collider = storeObject.GetComponent<Collider>();
            if (collider != null)
            {
                collider.isTrigger = true;
            }

            CCS_VendorInteractable interactable = storeObject.GetComponent<CCS_VendorInteractable>();
            if (interactable == null)
            {
                interactable = storeObject.AddComponent<CCS_VendorInteractable>();
            }

            SerializedObject serializedInteractable = new SerializedObject(interactable);
            serializedInteractable.FindProperty("vendorDefinition").objectReferenceValue = vendorDefinition;
            serializedInteractable.FindProperty("interactionDistance").floatValue = 3f;
            serializedInteractable.FindProperty("interactionDisplayNameOverride").stringValue = "General Store";
            serializedInteractable.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(storeObject);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void EnsurePlaytestEconomySteps()
        {
            CCS_PlaytestProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(PlaytestProfilePath);
            if (profile == null)
            {
                Debug.LogWarning($"{LogPrefix} Missing playtest profile; skipping economy checklist steps.");
                return;
            }

            SerializedObject serializedProfile = new SerializedObject(profile);
            serializedProfile.FindProperty("profileVersion").stringValue = "1.3.0";
            serializedProfile.FindProperty("profileDescription").stringValue =
                "Frontier starter progression and economy playtest checklist for milestone 1.3.0.";

            SerializedProperty stepList = serializedProfile.FindProperty("stepDefinitions");
            RemoveEconomySteps(stepList);

            int insertIndex = FindStepIndex(stepList, FishingUseStepId);
            if (insertIndex < 0)
            {
                insertIndex = stepList.arraySize;
            }
            else
            {
                insertIndex += 1;
            }

            InsertEconomySteps(stepList, insertIndex);
            serializedProfile.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static void RemoveEconomySteps(SerializedProperty stepList)
        {
            for (int index = stepList.arraySize - 1; index >= 0; index--)
            {
                string stepId = stepList.GetArrayElementAtIndex(index).FindPropertyRelative("stepId").stringValue;
                if (!string.IsNullOrEmpty(stepId) && stepId.StartsWith("ccs.survival.playtest.economy."))
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

        private static void InsertEconomySteps(SerializedProperty stepList, int insertIndex)
        {
            InsertStep(
                stepList,
                insertIndex++,
                "ccs.survival.playtest.economy.fish.obtain",
                "Obtain fish for trade",
                CCS_PlaytestStepType.ObtainFishForTrade,
                "Catch raw fish at CCS_TestFishingSpot or press F11 after fishing step grants inventory.",
                RawFishItemId);
            InsertStep(
                stepList,
                insertIndex++,
                "ccs.survival.playtest.economy.store.interact",
                "Interact with general store",
                CCS_PlaytestStepType.GatherResource,
                "Face CCS_TestGeneralStore and interact (F) to open vendor debug. Press F11 when the vendor panel is visible.",
                string.Empty,
                GeneralStoreVendorId);
            InsertStep(
                stepList,
                insertIndex++,
                "ccs.survival.playtest.economy.fish.sell",
                "Sell fish at vendor",
                CCS_PlaytestStepType.SellFishAtVendor,
                "With vendor active, press Shift+V to sell one raw fish (or use vendor debug Sell).",
                RawFishItemId);
            InsertStep(
                stepList,
                insertIndex++,
                "ccs.survival.playtest.economy.currency.verify.increase",
                "Verify currency increased",
                CCS_PlaytestStepType.VerifyCurrencyIncreased,
                "Confirm Trade Dollars balance increased after selling fish.");
            InsertStep(
                stepList,
                insertIndex++,
                "ccs.survival.playtest.economy.cordage.buy",
                "Buy cordage from vendor",
                CCS_PlaytestStepType.BuyItemFromVendor,
                "Press V to buy one cordage from the general store (or use vendor debug Buy).",
                CordageItemId);
            InsertStep(
                stepList,
                insertIndex++,
                "ccs.survival.playtest.economy.currency.verify.decrease",
                "Verify currency decreased",
                CCS_PlaytestStepType.VerifyCurrencyDecreased,
                "Confirm Trade Dollars were spent on cordage.");
            InsertStep(
                stepList,
                insertIndex,
                "ccs.survival.playtest.economy.inventory.verify",
                "Verify cordage in inventory",
                CCS_PlaytestStepType.VerifyVendorInventoryUpdated,
                "Confirm cordage appears in player inventory after purchase.",
                CordageItemId);
        }

        private static void InsertStep(
            SerializedProperty stepList,
            int index,
            string stepId,
            string displayName,
            CCS_PlaytestStepType stepType,
            string instructionText,
            string targetItemId = "",
            string targetObjectId = "")
        {
            stepList.InsertArrayElementAtIndex(index);
            SerializedProperty stepProperty = stepList.GetArrayElementAtIndex(index);
            stepProperty.FindPropertyRelative("stepId").stringValue = stepId;
            stepProperty.FindPropertyRelative("displayName").stringValue = displayName;
            stepProperty.FindPropertyRelative("stepType").enumValueIndex = (int)stepType;
            stepProperty.FindPropertyRelative("instructionText").stringValue = instructionText;
            stepProperty.FindPropertyRelative("targetItemId").stringValue = targetItemId ?? string.Empty;
            stepProperty.FindPropertyRelative("targetObjectId").stringValue = targetObjectId ?? string.Empty;
            stepProperty.FindPropertyRelative("requiredCount").intValue = 1;
            stepProperty.FindPropertyRelative("timeoutSeconds").floatValue = 0f;
        }

        private static void UpdateProjectVersion()
        {
            string projectSettingsPath = "ProjectSettings/ProjectSettings.asset";
            string text = File.ReadAllText(projectSettingsPath);
            text = System.Text.RegularExpressions.Regex.Replace(
                text,
                @"bundleVersion: [0-9]+\.[0-9]+\.[0-9]+",
                "bundleVersion: 1.3.0");
            File.WriteAllText(projectSettingsPath, text);
        }

        private static CCS_ItemDefinition LoadItem(string assetPath)
        {
            return AssetDatabase.LoadAssetAtPath<CCS_ItemDefinition>(assetPath);
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
