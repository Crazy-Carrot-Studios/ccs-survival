using System.Collections.Generic;
using System.IO;
using CCS.Modules.Economy;
using CCS.Modules.Inventory;
using CCS.Survival;
using CCS.Survival.Composition;
using CCS.Survival.Editor.Development;
using CCS.Survival.Player.Loadout;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_EconomyValidationValidator
// CATEGORY: Modules / Economy / Editor / Validation
// PURPOSE: Validates economy module layout, assets, bootstrap wiring, and item pricing.
// PLACEMENT: Registered on CCS_SurvivalValidationPipeline at editor load.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Milestone 1.3.1 vendor trading polish and hatchet acquisition validation.
// =============================================================================

namespace CCS.Modules.Economy.Editor
{
    public sealed class CCS_EconomyValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string ModuleRoot = "Assets/CCS/Modules/Economy";
        private const string RuntimeRoot = ModuleRoot + "/Runtime";
        private const string EditorRoot = ModuleRoot + "/Editor";
        private const string SurvivalRoot = "Assets/CCS/Survival";
        private const string EconomyProfilesRoot = SurvivalRoot + "/Profiles/Economy";
        private const string DefaultEconomyProfilePath = EconomyProfilesRoot + "/CCS_DefaultEconomyProfile.asset";
        private const string TradeDollarsCurrencyPath =
            EconomyProfilesRoot + "/Currencies/CCS_Currency_TradeDollars.asset";
        private const string GeneralStoreVendorPath =
            SurvivalRoot + "/Content/Vendors/CCS_Vendor_GeneralStore.asset";
        private const string StarterCoinItemPath = SurvivalRoot + "/Content/Items/Starter/CCS_Item_Coin.asset";
        private const string StarterLoadoutProfilePath =
            SurvivalRoot + "/Profiles/StarterLoadout/CCS_DefaultStarterLoadoutProfile.asset";
        private const string BootstrapScenePath = SurvivalRoot + "/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string BootstrapPrefabPath = SurvivalRoot + "/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
        private const string CompositionRegistrationPath =
            SurvivalRoot + "/Runtime/Composition/CCS_SurvivalGameplayServiceRegistration.cs";

        private const string TradeDollarsCurrencyId = "ccs.survival.currency.tradedollars";
        private const string TradeDollarsBackingItemId = "ccs.survival.item.starter.dollars";
        private const string GeneralStoreVendorId = "ccs.survival.vendor.frontier.generalstore";
        private const string TestGeneralStoreObjectName = "CCS_TestGeneralStore";
        private const string BoneHatchetItemPath =
            SurvivalRoot + "/Content/Items/Tools/Bone/CCS_Item_BoneHatchet.asset";
        private const string BoneHatchetItemId = "ccs.survival.item.tool.hatchet.bone";

        public string ValidatorId => "ccs.survival.validation.economy";

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateRequiredFolder(report, "Modules/Economy", ModuleRoot);
            ValidateRequiredFolder(report, "Runtime/Services", RuntimeRoot + "/Services");
            ValidateRequiredFolder(report, "Runtime/Currency", RuntimeRoot + "/Currency");
            ValidateRequiredFolder(report, "Runtime/Vendors", RuntimeRoot + "/Vendors");
            ValidateRequiredFolder(report, "Runtime/Profiles", RuntimeRoot + "/Profiles");
            ValidateRequiredFolder(report, "Runtime/Validation", RuntimeRoot + "/Validation");
            ValidateRequiredFolder(report, "Editor/Validation", EditorRoot + "/Validation");

            ValidateRequiredFile(report, "Runtime asmdef", RuntimeRoot + "/CCS.Modules.Economy.Runtime.asmdef");
            ValidateRequiredFile(report, "Editor asmdef", EditorRoot + "/CCS.Modules.Economy.Editor.asmdef");

            ValidateRequiredScript(report, "CCS_EconomyProfile", RuntimeRoot + "/Profiles/CCS_EconomyProfile.cs");
            ValidateRequiredScript(report, "CCS_CurrencyService", RuntimeRoot + "/Services/CCS_CurrencyService.cs");
            ValidateRequiredScript(report, "CCS_VendorService", RuntimeRoot + "/Services/CCS_VendorService.cs");
            ValidateRequiredScript(report, "CCS_EconomyValidationUtility", RuntimeRoot + "/Validation/CCS_EconomyValidationUtility.cs");

            ValidateRequiredAsset(report, "Default Economy Profile", DefaultEconomyProfilePath);
            ValidateEconomyProfile(report);
            ValidateTradeDollarsCurrency(report);
            ValidateGeneralStoreVendor(report);
            ValidateCompositionRegistration(report);
            ValidateBootstrapHost(report);
            ValidateBootstrapSceneGeneralStore(report);
            ValidateEconomyItemValues(report);
            ValidateStarterLoadoutCurrency(report);
            ValidateStarterLoadoutKnifeOnly(report);
            ValidateGeneralStoreProgressionCatalog(report);
            ValidateRuntimeScriptsAvoidUnityEditor(report, RuntimeRoot);
        }

        private static void ValidateEconomyProfile(CCS_SurvivalValidationReport report)
        {
            CCS_EconomyProfile profile = AssetDatabase.LoadAssetAtPath<CCS_EconomyProfile>(DefaultEconomyProfilePath);
            if (profile == null)
            {
                return;
            }

            CCS_SurvivalValidationResult validation = CCS_EconomyValidationUtility.ValidateProfile(profile);
            report.AddIssue(
                validation.IsSuccess
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Default Economy Profile Validation",
                validation.Message);

            if (profile.ProfileVersion != "1.5.0")
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Economy Profile Version",
                    $"Expected profileVersion 1.5.0 but found '{profile.ProfileVersion}'.");
            }
        }

        private static void ValidateTradeDollarsCurrency(CCS_SurvivalValidationReport report)
        {
            CCS_CurrencyDefinition currency =
                AssetDatabase.LoadAssetAtPath<CCS_CurrencyDefinition>(TradeDollarsCurrencyPath);
            if (currency == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Trade Dollars Currency",
                    $"Missing currency at {TradeDollarsCurrencyPath}. Run CCS_EconomyBootstrapSetup.ExecuteBatch.");
                return;
            }

            if (currency.CurrencyId != TradeDollarsCurrencyId)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Trade Dollars Currency Id",
                    $"Expected {TradeDollarsCurrencyId} but found '{currency.CurrencyId}'.");
            }

            CCS_ItemDefinition backingItem = currency.InventoryBackingItem;
            if (backingItem == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Trade Dollars Backing Item",
                    "Trade Dollars currency is missing inventory backing item.");
                return;
            }

            if (backingItem.ItemId != TradeDollarsBackingItemId)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Trade Dollars Backing Item Id",
                    $"Expected backing item {TradeDollarsBackingItemId} but found '{backingItem.ItemId}'.");
            }
        }

        private static void ValidateGeneralStoreVendor(CCS_SurvivalValidationReport report)
        {
            CCS_VendorDefinition vendor =
                AssetDatabase.LoadAssetAtPath<CCS_VendorDefinition>(GeneralStoreVendorPath);
            if (vendor == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "General Store Vendor",
                    $"Missing vendor at {GeneralStoreVendorPath}. Run CCS_EconomyBootstrapSetup.ExecuteBatch.");
                return;
            }

            if (vendor.VendorId != GeneralStoreVendorId)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "General Store Vendor Id",
                    $"Expected {GeneralStoreVendorId} but found '{vendor.VendorId}'.");
            }

            CCS_SurvivalValidationResult validation = CCS_EconomyValidationUtility.ValidateVendorDefinition(vendor);
            report.AddIssue(
                validation.IsSuccess
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "General Store Vendor Validation",
                validation.Message);

            ValidateRequiredAsset(report, "Bone Hatchet Item", BoneHatchetItemPath);
            CCS_ItemDefinition hatchetItem = AssetDatabase.LoadAssetAtPath<CCS_ItemDefinition>(BoneHatchetItemPath);
            if (hatchetItem != null)
            {
                if (hatchetItem.ItemId != BoneHatchetItemId)
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Bone Hatchet Item Id",
                        $"Expected {BoneHatchetItemId} but found '{hatchetItem.ItemId}'.");
                }

                if (!hatchetItem.HasEconomyValues || hatchetItem.BuyValue <= 0)
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Bone Hatchet Buy Value",
                        "Bone hatchet must define a positive buy value (acquired via trade, not starter loadout).");
                }
            }
        }

        private static void ValidateCompositionRegistration(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(CompositionRegistrationPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Composition Registration File",
                    $"Missing {CompositionRegistrationPath}.");
                return;
            }

            string source = File.ReadAllText(CompositionRegistrationPath);
            bool hasEconomyServices = source.Contains("CCS_CurrencyService")
                && source.Contains("CCS_VendorService");
            report.AddIssue(
                hasEconomyServices
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Economy Service Registration",
                hasEconomyServices
                    ? "CCS_SurvivalGameplayServiceRegistration registers currency and vendor services."
                    : "Composition registration is missing economy service wiring.");
        }

        private static void ValidateBootstrapHost(CCS_SurvivalValidationReport report)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(BootstrapPrefabPath);
            if (prefab == null)
            {
                return;
            }

            CCS_SurvivalGameplayServiceHost host = prefab.GetComponent<CCS_SurvivalGameplayServiceHost>();
            if (host == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Bootstrap Gameplay Host",
                    "Bootstrap prefab is missing CCS_SurvivalGameplayServiceHost.");
                return;
            }

            SerializedObject serializedHost = new SerializedObject(host);
            Object economyProfile = serializedHost.FindProperty("economyProfile").objectReferenceValue;
            report.AddIssue(
                economyProfile != null
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Bootstrap Economy Profile",
                economyProfile != null
                    ? "Bootstrap gameplay host has economyProfile assigned."
                    : "Bootstrap gameplay host is missing economyProfile assignment.");
        }

        private static void ValidateBootstrapSceneGeneralStore(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(BootstrapScenePath))
            {
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Additive);
            try
            {
                CCS_VendorInteractable[] vendors =
                    Object.FindObjectsByType<CCS_VendorInteractable>();
                bool foundTestStore = false;
                for (int index = 0; index < vendors.Length; index++)
                {
                    CCS_VendorInteractable vendor = vendors[index];
                    if (vendor == null)
                    {
                        continue;
                    }

                    if (vendor.name == TestGeneralStoreObjectName)
                    {
                        foundTestStore = true;
                    }

                    if (vendor.VendorDefinition == null)
                    {
                        report.AddIssue(
                            CCS_SurvivalValidationIssueSeverity.Error,
                            $"{vendor.name} Vendor Definition",
                            $"{vendor.name} is missing vendor definition.");
                    }
                }

                report.AddIssue(
                    foundTestStore
                        ? CCS_SurvivalValidationIssueSeverity.Info
                        : CCS_SurvivalValidationIssueSeverity.Error,
                    "Bootstrap Test General Store",
                    foundTestStore
                        ? "Bootstrap scene contains CCS_TestGeneralStore."
                        : "Bootstrap scene is missing CCS_TestGeneralStore. Run CCS_EconomyBootstrapSetup.ExecuteBatch.");
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static void ValidateEconomyItemValues(CCS_SurvivalValidationReport report)
        {
            HashSet<CCS_ItemDefinition> scannedItems = new HashSet<CCS_ItemDefinition>();
            CCS_EconomyProfile economyProfile =
                AssetDatabase.LoadAssetAtPath<CCS_EconomyProfile>(DefaultEconomyProfilePath);
            if (economyProfile?.VendorProfile != null)
            {
                CCS_VendorDefinition[] vendors = economyProfile.VendorProfile.VendorDefinitions;
                for (int vendorIndex = 0; vendorIndex < vendors.Length; vendorIndex++)
                {
                    CCS_VendorDefinition vendor = vendors[vendorIndex];
                    if (vendor?.VendorInventory?.Items == null)
                    {
                        continue;
                    }

                    CCS_VendorItemEntry[] entries = vendor.VendorInventory.Items;
                    for (int entryIndex = 0; entryIndex < entries.Length; entryIndex++)
                    {
                        CCS_VendorItemEntry entry = entries[entryIndex];
                        if (entry?.ItemDefinition != null)
                        {
                            scannedItems.Add(entry.ItemDefinition);
                        }
                    }
                }
            }

            string[] economyItemPaths =
            {
                SurvivalRoot + "/Content/Items/Fishing/CCS_Item_RawFish.asset",
                SurvivalRoot + "/Content/Items/Fishing/CCS_Item_SmallFish.asset",
                SurvivalRoot + "/Content/Items/Fishing/CCS_Item_CrudeHook.asset",
                SurvivalRoot + "/Content/Items/Fishing/CCS_Item_FishingLine.asset",
                SurvivalRoot + "/Content/Items/Tools/Fishing/CCS_Item_FishingPole.asset",
                SurvivalRoot + "/Content/Items/Frontier/CCS_Item_Cordage.asset",
                SurvivalRoot + "/Content/Items/Frontier/CCS_Item_Hardtack.asset",
                SurvivalRoot + "/Content/Items/Frontier/CCS_Item_Tinderbox.asset",
                SurvivalRoot + "/Content/Items/Frontier/CCS_Item_Arrow.asset",
                BoneHatchetItemPath,
                SurvivalRoot + "/Content/Items/Frontier/CCS_Item_SimpleTrap.asset",
                SurvivalRoot + "/Content/Items/Resources/Primitive/CCS_Item_Hide.asset",
                SurvivalRoot + "/Content/Items/Resources/Wildlife/CCS_Item_RawMeat.asset",
                SurvivalRoot + "/Content/Items/Resources/Frontier/CCS_Item_ScrapIron.asset",
                SurvivalRoot + "/Content/Items/Frontier/CCS_Item_Feather.asset",
                SurvivalRoot + "/Content/Items/Frontier/CCS_Item_AnimalFat.asset",
                SurvivalRoot + "/Content/Items/Fishing/CCS_Item_Junk.asset"
            };

            for (int index = 0; index < economyItemPaths.Length; index++)
            {
                CCS_ItemDefinition item = AssetDatabase.LoadAssetAtPath<CCS_ItemDefinition>(economyItemPaths[index]);
                if (item != null)
                {
                    scannedItems.Add(item);
                }
            }

            foreach (CCS_ItemDefinition item in scannedItems)
            {
                if (item == null || !item.HasEconomyValues)
                {
                    continue;
                }

                if (item.BuyValue < 0 || item.SellValue < 0)
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Item Economy Values",
                        $"Item '{item.ItemId}' has negative buy/sell values (buy={item.BuyValue}, sell={item.SellValue}).");
                }
            }
        }

        private static void ValidateStarterLoadoutKnifeOnly(CCS_SurvivalValidationReport report)
        {
            CCS_StarterLoadoutProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_StarterLoadoutProfile>(StarterLoadoutProfilePath);
            if (profile == null)
            {
                return;
            }

            CCS_StarterLoadoutEntry[] entries = profile.StartingItems;
            if (entries == null)
            {
                return;
            }

            for (int index = 0; index < entries.Length; index++)
            {
                CCS_StarterLoadoutEntry entry = entries[index];
                if (entry?.ItemDefinition == null)
                {
                    continue;
                }

                if (entry.ItemDefinition.ItemId == BoneHatchetItemId)
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Starter Loadout Knife Only",
                        "Bone hatchet must not be in the default starter loadout (trade progression only).");
                    return;
                }
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,
                "Starter Loadout Knife Only",
                "Starter loadout does not include bone hatchet (trade acquisition path).");
        }

        private static void ValidateGeneralStoreProgressionCatalog(CCS_SurvivalValidationReport report)
        {
            CCS_VendorDefinition vendor =
                AssetDatabase.LoadAssetAtPath<CCS_VendorDefinition>(GeneralStoreVendorPath);
            if (vendor?.VendorInventory?.Items == null)
            {
                return;
            }

            bool hasProgressionTool = false;
            bool hatchetBuyable = false;
            bool rawFishSellOnly = false;
            CCS_VendorItemEntry[] entries = vendor.VendorInventory.Items;
            for (int index = 0; index < entries.Length; index++)
            {
                CCS_VendorItemEntry entry = entries[index];
                if (entry?.ItemDefinition == null)
                {
                    continue;
                }

                if (entry.ItemDefinition.ItemId == BoneHatchetItemId)
                {
                    hatchetBuyable = entry.AllowBuy && !entry.AllowSell;
                }

                if (entry.ItemDefinition.ItemId == "ccs.survival.item.resource.rawfish")
                {
                    rawFishSellOnly = entry.AllowSell && !entry.AllowBuy;
                }

                if (entry.AllowBuy
                    && entry.ItemDefinition.GameplayKind == CCS_ItemGameplayKind.Tool)
                {
                    hasProgressionTool = true;
                }
            }

            report.AddIssue(
                hasProgressionTool
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "General Store Progression Tool",
                hasProgressionTool
                    ? "General Store sells at least one progression tool."
                    : "General Store must sell at least one progression tool (e.g. hatchet).");

            report.AddIssue(
                hatchetBuyable
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "General Store Hatchet Buy Rule",
                hatchetBuyable
                    ? "Bone hatchet is buy-only at the General Store."
                    : "Bone hatchet must be purchasable (buy enabled, sell disabled) at the General Store.");

            report.AddIssue(
                rawFishSellOnly
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "General Store Raw Fish Sell Rule",
                rawFishSellOnly
                    ? "Raw fish is sell-only at the General Store."
                    : "Raw fish must be sell-only (purchase disabled) at the General Store.");
        }

        private static void ValidateStarterLoadoutCurrency(CCS_SurvivalValidationReport report)
        {
            CCS_StarterLoadoutProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_StarterLoadoutProfile>(StarterLoadoutProfilePath);
            if (profile == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Starter Loadout Profile",
                    $"Missing starter loadout profile at {StarterLoadoutProfilePath}.");
                return;
            }

            CCS_ItemDefinition currencyItem = profile.CurrencyItemDefinition;
            if (currencyItem == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Starter Loadout Currency",
                    "Starter loadout profile is missing currencyItemDefinition.");
                return;
            }

            if (currencyItem.ItemId != TradeDollarsBackingItemId)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Starter Loadout Currency Id",
                    $"Expected currency item {TradeDollarsBackingItemId} but found '{currencyItem.ItemId}'.");
            }

            CCS_CurrencyDefinition tradeDollars =
                AssetDatabase.LoadAssetAtPath<CCS_CurrencyDefinition>(TradeDollarsCurrencyPath);
            if (tradeDollars != null && tradeDollars.InventoryBackingItem != currencyItem)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Starter Currency Backing Mismatch",
                    "Starter loadout currency item must match Trade Dollars currency backing item.");
            }

            if (profile.StartingCurrencyAmount <= 0)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Starter Loadout Currency Amount",
                    "Starter loadout profile must define a positive startingCurrencyAmount.");
            }
        }

        private static void ValidateRequiredFolder(
            CCS_SurvivalValidationReport report,
            string label,
            string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    $"Missing Folder: {label}",
                    path);
            }
        }

        private static void ValidateRequiredFile(
            CCS_SurvivalValidationReport report,
            string label,
            string path)
        {
            if (!File.Exists(path))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    $"Missing File: {label}",
                    path);
            }
        }

        private static void ValidateRequiredScript(
            CCS_SurvivalValidationReport report,
            string label,
            string path)
        {
            if (!File.Exists(path))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    $"Missing Script: {label}",
                    path);
            }
        }

        private static void ValidateRequiredAsset(
            CCS_SurvivalValidationReport report,
            string label,
            string path)
        {
            if (AssetDatabase.LoadAssetAtPath<Object>(path) == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    $"Missing Asset: {label}",
                    $"{path}. Run CCS_EconomyBootstrapSetup.ExecuteBatch.");
            }
        }

        private static void ValidateRuntimeScriptsAvoidUnityEditor(
            CCS_SurvivalValidationReport report,
            string runtimeRoot)
        {
            if (!Directory.Exists(runtimeRoot))
            {
                return;
            }

            string[] files = Directory.GetFiles(runtimeRoot, "*.cs", SearchOption.AllDirectories);
            for (int index = 0; index < files.Length; index++)
            {
                string file = files[index].Replace('\\', '/');
                string contents = File.ReadAllText(file);
                if (contents.Contains("using UnityEditor"))
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Economy Runtime Editor Reference",
                        $"{file} must not reference UnityEditor.");
                }
            }
        }
    }
}
