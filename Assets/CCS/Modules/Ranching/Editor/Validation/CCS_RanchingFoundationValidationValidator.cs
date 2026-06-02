using System.IO;
using CCS.Modules.Economy;
using CCS.Modules.Inventory;
using CCS.Modules.Playtesting;
using CCS.Modules.Ranching;
using CCS.Survival;
using CCS.Survival.Composition;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_RanchingFoundationValidationValidator
// CATEGORY: Modules / Ranching / Editor / Validation
// PURPOSE: Validates ranching module layout, content, composition, and save wiring.
// PLACEMENT: Registered on CCS_SurvivalValidationPipeline at editor load.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// NOTES: Milestone 2.1.0 ranching foundation.
// =============================================================================

namespace CCS.Modules.Ranching.Editor
{
    public sealed class CCS_RanchingFoundationValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string ValidatorContext = "ccs.survival.validation.ranching";
        private const string ModuleRoot = "Assets/CCS/Modules/Ranching";
        private const string RuntimeRoot = ModuleRoot + "/Runtime";
        private const string EditorRoot = ModuleRoot + "/Editor";
        private const string ModuleDocPath = ModuleRoot + "/Documentation/CCS_Ranching_Module.md";
        private const string LivestockProfilePath = "Assets/CCS/Survival/Profiles/Ranching/CCS_DefaultLivestockProfile.asset";
        private const string BootstrapPrefabPath = "Assets/CCS/Survival/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
        private const string CompositionRegistrationPath =
            "Assets/CCS/Survival/Runtime/Composition/CCS_SurvivalGameplayServiceRegistration.cs";
        private const string SaveDataPath = "Assets/CCS/Modules/SaveSystem/Runtime/Data/CCS_SaveData.cs";
        private const string GeneralStoreVendorPath = "Assets/CCS/Survival/Content/Vendors/CCS_Vendor_GeneralStore.asset";
        private const string StableVendorPath = "Assets/CCS/Survival/Content/Vendors/CCS_Vendor_FrontierStable.asset";
        private const string DefaultPlaytestProfilePath = "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset";

        public string ValidatorId => ValidatorContext;

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateRequiredFolder(report, "Modules/Ranching", ModuleRoot);
            ValidateRequiredFolder(report, "Runtime/Data", RuntimeRoot + "/Data");
            ValidateRequiredFolder(report, "Runtime/Definitions", RuntimeRoot + "/Definitions");
            ValidateRequiredFolder(report, "Runtime/Profiles", RuntimeRoot + "/Profiles");
            ValidateRequiredFolder(report, "Runtime/Services", RuntimeRoot + "/Services");
            ValidateRequiredFolder(report, "Runtime/Validation", RuntimeRoot + "/Validation");
            ValidateRequiredFolder(report, "Editor/Validation", EditorRoot + "/Validation");
            ValidateRequiredFolder(report, "Documentation", ModuleRoot + "/Documentation");

            ValidateRequiredFile(report, "Runtime asmdef", RuntimeRoot + "/CCS.Modules.Ranching.Runtime.asmdef");
            ValidateRequiredFile(report, "Editor asmdef", EditorRoot + "/CCS.Modules.Ranching.Editor.asmdef");
            ValidateRequiredScript(report, "CCS_RanchService", RuntimeRoot + "/Services/CCS_RanchService.cs");
            ValidateRequiredScript(report, "CCS_RanchRuntimeBridge", RuntimeRoot + "/Services/CCS_RanchRuntimeBridge.cs");
            ValidateRequiredScript(report, "CCS_RanchValidationUtility", RuntimeRoot + "/Validation/CCS_RanchValidationUtility.cs");
            ValidateRequiredScript(report, "CCS_RanchingFoundationBootstrapSetup", EditorRoot + "/Validation/CCS_RanchingFoundationBootstrapSetup.cs");

            ValidateLivestockProfile(report);
            ValidateProductItems(report);
            ValidateVendorCatalog(report);
            ValidateCompositionRegistration(report);
            ValidateBootstrapWiring(report);
            ValidateSaveSupport(report);
            ValidatePlaytestSteps(report);
            ValidateDocumentation(report);

            CCS_SurvivalBootstrapVersionUtility.AddBundleVersionValidationIssue(
                report,
                ValidatorContext,
                CCS_SurvivalBootstrapVersionUtility.CurrentMilestoneVersion,
                "Run CCS_RanchingFoundationBootstrapSetup.ExecuteBatch.");
            CCS_SurvivalBootstrapVersionUtility.ValidateNoHardcodedBootstrapVersionWrites(report, ValidatorContext);
            CCS_SurvivalInputValidationUtility.ValidateNoLegacyInputUsage(report, "Ranching Input Policy");
            CCS_SurvivalInputValidationUtility.ValidateNoFindObjectsSortModeUsage(report, "Ranching Obsolete API Scan");
        }

        private static void ValidateLivestockProfile(CCS_SurvivalValidationReport report)
        {
            CCS_LivestockProfile profile = AssetDatabase.LoadAssetAtPath<CCS_LivestockProfile>(LivestockProfilePath);
            CCS_SurvivalValidationResult result = CCS_RanchValidationUtility.ValidateProfile(profile);
            report.AddIssue(
                result.IsSuccess ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                result.Message);
        }

        private static void ValidateProductItems(CCS_SurvivalValidationReport report)
        {
            ValidateItemAtPath(report, "Assets/CCS/Survival/Content/Ranching/Items/CCS_Item_RanchEgg.asset", CCS_RanchingContentIds.EggItemId);
            ValidateItemAtPath(report, "Assets/CCS/Survival/Content/Ranching/Items/CCS_Item_RanchMilk.asset", CCS_RanchingContentIds.MilkItemId);
            ValidateItemAtPath(report, "Assets/CCS/Survival/Content/Ranching/Items/CCS_Item_RanchFeed.asset", CCS_RanchingContentIds.FeedItemId);
        }

        private static void ValidateItemAtPath(CCS_SurvivalValidationReport report, string path, string expectedItemId)
        {
            CCS_ItemDefinition item = AssetDatabase.LoadAssetAtPath<CCS_ItemDefinition>(path);
            CCS_SurvivalValidationResult result = CCS_RanchValidationUtility.ValidateProductItem(item, expectedItemId);
            report.AddIssue(
                result.IsSuccess ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                result.Message);
        }

        private static void ValidateVendorCatalog(CCS_SurvivalValidationReport report)
        {
            bool generalStoreSellsChicken = VendorSellsItem(GeneralStoreVendorPath, CCS_RanchingContentIds.ChickenItemId);
            bool generalStoreBuysEgg = VendorBuysItem(GeneralStoreVendorPath, CCS_RanchingContentIds.EggItemId);
            bool generalStoreBuysMilk = VendorBuysItem(GeneralStoreVendorPath, CCS_RanchingContentIds.MilkItemId);
            bool stableSellsLivestock = VendorSellsItem(StableVendorPath, CCS_RanchingContentIds.ChickenItemId)
                || VendorSellsItem(StableVendorPath, CCS_RanchingContentIds.GoatItemId);

            report.AddIssue(
                generalStoreSellsChicken || stableSellsLivestock
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                generalStoreSellsChicken || stableSellsLivestock
                    ? "Vendor catalog includes livestock purchase entries."
                    : "Missing livestock purchase entries on general store or stable vendor.");

            report.AddIssue(
                generalStoreBuysEgg && generalStoreBuysMilk
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                generalStoreBuysEgg && generalStoreBuysMilk
                    ? "General store buys ranch egg and milk products."
                    : "General store missing ranch product buyback entries.");
        }

        private static bool VendorSellsItem(string vendorPath, string itemId)
        {
            CCS_VendorDefinition vendor = AssetDatabase.LoadAssetAtPath<CCS_VendorDefinition>(vendorPath);
            if (vendor?.VendorInventory?.Items == null)
            {
                return false;
            }

            CCS_VendorItemEntry[] items = vendor.VendorInventory.Items;
            for (int index = 0; index < items.Length; index++)
            {
                CCS_VendorItemEntry entry = items[index];
                if (entry?.ItemDefinition != null
                    && entry.AllowBuy
                    && string.Equals(entry.ItemDefinition.ItemId, itemId, System.StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool VendorBuysItem(string vendorPath, string itemId)
        {
            CCS_VendorDefinition vendor = AssetDatabase.LoadAssetAtPath<CCS_VendorDefinition>(vendorPath);
            if (vendor?.VendorInventory?.Items == null)
            {
                return false;
            }

            CCS_VendorItemEntry[] items = vendor.VendorInventory.Items;
            for (int index = 0; index < items.Length; index++)
            {
                CCS_VendorItemEntry entry = items[index];
                if (entry?.ItemDefinition != null
                    && entry.AllowSell
                    && string.Equals(entry.ItemDefinition.ItemId, itemId, System.StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static void ValidateCompositionRegistration(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(CompositionRegistrationPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    ValidatorContext,
                    "Missing CCS_SurvivalGameplayServiceRegistration.cs.");
                return;
            }

            string source = File.ReadAllText(CompositionRegistrationPath);
            bool hasRanchService = source.Contains("CCS_RanchService", System.StringComparison.Ordinal);
            bool hasRanchHandler = source.Contains("BindFrontierRanchPlacementHandler", System.StringComparison.Ordinal);
            report.AddIssue(
                hasRanchService && hasRanchHandler
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                hasRanchService && hasRanchHandler
                    ? "Composition registers ranch service and active item placement handler."
                    : "Composition missing ranch service or placement handler wiring.");
        }

        private static void ValidateBootstrapWiring(CCS_SurvivalValidationReport report)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(BootstrapPrefabPath);
            CCS_SurvivalGameplayServiceHost host = prefab != null ? prefab.GetComponent<CCS_SurvivalGameplayServiceHost>() : null;
            SerializedObject serialized = host != null ? new SerializedObject(host) : null;
            bool ok = serialized != null
                && serialized.FindProperty("livestockProfile").objectReferenceValue != null;
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                ok
                    ? "Bootstrap host references livestock profile."
                    : "Bootstrap host missing livestock profile reference.");
        }

        private static void ValidateSaveSupport(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(SaveDataPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    ValidatorContext,
                    "CCS_SaveData.cs was not found.");
                return;
            }

            string source = File.ReadAllText(SaveDataPath);
            bool hasRanchingData = source.Contains("CCS_SaveRanchingWorldData", System.StringComparison.Ordinal);
            report.AddIssue(
                hasRanchingData
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                hasRanchingData
                    ? "CCS_SaveData includes ranching world snapshots."
                    : "CCS_SaveData is missing CCS_SaveRanchingWorldData.");
        }

        private static void ValidatePlaytestSteps(CCS_SurvivalValidationReport report)
        {
            CCS_PlaytestProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(DefaultPlaytestProfilePath);
            if (profile == null)
            {
                report.AddIssue(CCS_SurvivalValidationIssueSeverity.Error, ValidatorContext, "Missing playtest profile.");
                return;
            }

            bool hasBuy = false;
            bool hasVerify = false;
            System.Collections.Generic.IReadOnlyList<CCS_PlaytestStepDefinition> steps = profile.StepDefinitions;
            for (int index = 0; index < steps.Count; index++)
            {
                if (steps[index]?.StepType == CCS_PlaytestStepType.BuyChickenFromVendor)
                {
                    hasBuy = true;
                }

                if (steps[index]?.StepType == CCS_PlaytestStepType.VerifyRanchStateAfterLoad)
                {
                    hasVerify = true;
                }
            }

            bool ok = hasBuy && hasVerify;
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                ok
                    ? "Playtest profile includes ranching foundation steps."
                    : "Playtest profile missing ranch purchase or persistence verification steps.");
        }

        private static void ValidateDocumentation(CCS_SurvivalValidationReport report)
        {
            bool ok = File.Exists(ModuleDocPath);
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                ok ? "Ranching module documentation exists." : "Missing CCS_Ranching_Module.md.");
        }

        private static void ValidateRequiredFolder(CCS_SurvivalValidationReport report, string label, string path)
        {
            bool ok = AssetDatabase.IsValidFolder(path);
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                ok ? $"Folder exists: {label}." : $"Missing folder: {path}.");
        }

        private static void ValidateRequiredFile(CCS_SurvivalValidationReport report, string label, string path)
        {
            bool ok = File.Exists(path);
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                ok ? $"File exists: {label}." : $"Missing file: {path}.");
        }

        private static void ValidateRequiredScript(CCS_SurvivalValidationReport report, string label, string path)
        {
            ValidateRequiredFile(report, label, path);
        }
    }
}
