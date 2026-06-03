using System.IO;
using CCS.Modules.Economy;
using CCS.Modules.Inventory;
using CCS.Modules.Playtesting;
using CCS.Modules.Farming;
using CCS.Survival;
using CCS.Survival.Composition;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_FarmingFoundationValidationValidator
// CATEGORY: Modules / Farming / Editor / Validation
// PURPOSE: Validates farming module layout, content, composition, and save wiring.
// PLACEMENT: Registered on CCS_SurvivalValidationPipeline at editor load.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// NOTES: Milestone 2.2.0 farming foundation.
// =============================================================================

namespace CCS.Modules.Farming.Editor
{
    public sealed class CCS_FarmingFoundationValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string ValidatorContext = "ccs.survival.validation.farming";
        private const string FarmingMilestoneVersion = "2.2.0";
        private const string ModuleRoot = "Assets/CCS/Modules/Farming";
        private const string RuntimeRoot = ModuleRoot + "/Runtime";
        private const string EditorRoot = ModuleRoot + "/Editor";
        private const string ModuleDocPath = ModuleRoot + "/Documentation/CCS_Farming_Module.md";
        private const string CropProfilePath = "Assets/CCS/Survival/Profiles/Farming/CCS_DefaultCropProfile.asset";
        private const string BootstrapPrefabPath = "Assets/CCS/Survival/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
        private const string CompositionRegistrationPath =
            "Assets/CCS/Survival/Runtime/Composition/CCS_SurvivalGameplayServiceRegistration.cs";
        private const string SaveDataPath = "Assets/CCS/Modules/SaveSystem/Runtime/Data/CCS_SaveData.cs";
        private const string GeneralStoreVendorPath = "Assets/CCS/Survival/Content/Vendors/CCS_Vendor_GeneralStore.asset";
        private const string DefaultPlaytestProfilePath = "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset";

        public string ValidatorId => ValidatorContext;

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateRequiredFolder(report, "Modules/Farming", ModuleRoot);
            ValidateRequiredFolder(report, "Runtime/Data", RuntimeRoot + "/Data");
            ValidateRequiredFolder(report, "Runtime/Definitions", RuntimeRoot + "/Definitions");
            ValidateRequiredFolder(report, "Runtime/Profiles", RuntimeRoot + "/Profiles");
            ValidateRequiredFolder(report, "Runtime/Services", RuntimeRoot + "/Services");
            ValidateRequiredFolder(report, "Runtime/Validation", RuntimeRoot + "/Validation");
            ValidateRequiredFolder(report, "Runtime/Components", RuntimeRoot + "/Components");
            ValidateRequiredFolder(report, "Editor/Validation", EditorRoot + "/Validation");
            ValidateRequiredFolder(report, "Documentation", ModuleRoot + "/Documentation");

            ValidateRequiredFile(report, "Runtime asmdef", RuntimeRoot + "/CCS.Modules.Farming.Runtime.asmdef");
            ValidateRequiredFile(report, "Editor asmdef", EditorRoot + "/CCS.Modules.Farming.Editor.asmdef");
            ValidateRequiredScript(report, "CCS_FarmService", RuntimeRoot + "/Services/CCS_FarmService.cs");
            ValidateRequiredScript(report, "CCS_FarmRuntimeBridge", RuntimeRoot + "/Services/CCS_FarmRuntimeBridge.cs");
            ValidateRequiredScript(report, "CCS_FarmValidationUtility", RuntimeRoot + "/Validation/CCS_FarmValidationUtility.cs");
            ValidateRequiredScript(
                report,
                "CCS_FarmingFoundationBootstrapSetup",
                EditorRoot + "/Validation/CCS_FarmingFoundationBootstrapSetup.cs");

            ValidateCropProfile(report);
            ValidateHarvestItems(report);
            ValidateVendorCatalog(report);
            ValidateCompositionRegistration(report);
            ValidateBootstrapWiring(report);
            ValidateSaveSupport(report);
            ValidatePlaytestSteps(report);
            ValidateDocumentation(report);

            CCS_SurvivalBootstrapVersionUtility.AddBundleVersionValidationIssue(
                report,
                ValidatorContext,
                FarmingMilestoneVersion,
                "Run CCS_FarmingFoundationBootstrapSetup.ExecuteBatch.");
            CCS_SurvivalBootstrapVersionUtility.ValidateNoHardcodedBootstrapVersionWrites(report, ValidatorContext);
            CCS_SurvivalInputValidationUtility.ValidateNoLegacyInputUsage(report, "Farming Input Policy");
            CCS_SurvivalInputValidationUtility.ValidateNoFindObjectsSortModeUsage(report, "Farming Obsolete API Scan");
        }

        private static void ValidateCropProfile(CCS_SurvivalValidationReport report)
        {
            CCS_CropProfile profile = AssetDatabase.LoadAssetAtPath<CCS_CropProfile>(CropProfilePath);
            CCS_SurvivalValidationResult result = CCS_FarmValidationUtility.ValidateProfile(profile);
            report.AddIssue(
                result.IsSuccess ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                result.Message);
        }

        private static void ValidateHarvestItems(CCS_SurvivalValidationReport report)
        {
            ValidateItemAtPath(
                report,
                "Assets/CCS/Survival/Content/Farming/Items/CCS_Item_Corn.asset",
                CCS_FarmingContentIds.CornHarvestItemId);
            ValidateItemAtPath(
                report,
                "Assets/CCS/Survival/Content/Farming/Items/CCS_Item_Beans.asset",
                CCS_FarmingContentIds.BeanHarvestItemId);
            ValidateItemAtPath(
                report,
                "Assets/CCS/Survival/Content/Farming/Items/CCS_Item_Potatoes.asset",
                CCS_FarmingContentIds.PotatoHarvestItemId);
            ValidateItemAtPath(
                report,
                "Assets/CCS/Survival/Content/Farming/Items/CCS_Item_Wheat.asset",
                CCS_FarmingContentIds.WheatHarvestItemId);
        }

        private static void ValidateItemAtPath(CCS_SurvivalValidationReport report, string path, string expectedItemId)
        {
            CCS_ItemDefinition item = AssetDatabase.LoadAssetAtPath<CCS_ItemDefinition>(path);
            CCS_SurvivalValidationResult result = CCS_FarmValidationUtility.ValidateItem(item, expectedItemId);
            report.AddIssue(
                result.IsSuccess ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                result.Message);
        }

        private static void ValidateVendorCatalog(CCS_SurvivalValidationReport report)
        {
            bool sellsPlotKit = VendorSellsItem(GeneralStoreVendorPath, CCS_FarmingContentIds.FarmPlotKitItemId);
            bool sellsCornSeed = VendorSellsItem(GeneralStoreVendorPath, CCS_FarmingContentIds.CornSeedItemId);
            bool buysCorn = VendorBuysItem(GeneralStoreVendorPath, CCS_FarmingContentIds.CornHarvestItemId);
            bool buysWheat = VendorBuysItem(GeneralStoreVendorPath, CCS_FarmingContentIds.WheatHarvestItemId);

            report.AddIssue(
                sellsPlotKit && sellsCornSeed
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                sellsPlotKit && sellsCornSeed
                    ? "General store sells farm plot kit and crop seeds."
                    : "General store missing farm plot kit or seed purchase entries.");

            report.AddIssue(
                buysCorn && buysWheat
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                buysCorn && buysWheat
                    ? "General store buys frontier crop harvest products."
                    : "General store missing crop harvest buyback entries.");
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
            bool hasFarmService = source.Contains("CreateFarmService", System.StringComparison.Ordinal);
            bool hasFarmHandler = source.Contains("BindFrontierFarmPlotPlacementHandler", System.StringComparison.Ordinal);
            report.AddIssue(
                hasFarmService && hasFarmHandler
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                hasFarmService && hasFarmHandler
                    ? "Composition registers farm service and active item placement handler."
                    : "Composition missing farm service or placement handler wiring.");
        }

        private static void ValidateBootstrapWiring(CCS_SurvivalValidationReport report)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(BootstrapPrefabPath);
            CCS_SurvivalGameplayServiceHost host = prefab != null ? prefab.GetComponent<CCS_SurvivalGameplayServiceHost>() : null;
            SerializedObject serialized = host != null ? new SerializedObject(host) : null;
            bool ok = serialized != null
                && serialized.FindProperty("farmingProfile").objectReferenceValue != null;
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                ok
                    ? "Bootstrap host references farming profile."
                    : "Bootstrap host missing farming profile reference.");
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
            bool hasFarmingData = source.Contains("CCS_SaveFarmingWorldData", System.StringComparison.Ordinal);
            report.AddIssue(
                hasFarmingData
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                hasFarmingData
                    ? "CCS_SaveData includes farming world snapshots."
                    : "CCS_SaveData is missing CCS_SaveFarmingWorldData.");
        }

        private static void ValidatePlaytestSteps(CCS_SurvivalValidationReport report)
        {
            CCS_PlaytestProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(DefaultPlaytestProfilePath);
            if (profile == null)
            {
                report.AddIssue(CCS_SurvivalValidationIssueSeverity.Error, ValidatorContext, "Missing playtest profile.");
                return;
            }

            bool hasBuyPlot = false;
            bool hasVerifyLoad = false;
            System.Collections.Generic.IReadOnlyList<CCS_PlaytestStepDefinition> steps = profile.StepDefinitions;
            for (int index = 0; index < steps.Count; index++)
            {
                if (steps[index]?.StepType == CCS_PlaytestStepType.BuyFarmPlotKit)
                {
                    hasBuyPlot = true;
                }

                if (steps[index]?.StepType == CCS_PlaytestStepType.VerifyFarmStateAfterLoad)
                {
                    hasVerifyLoad = true;
                }
            }

            bool ok = hasBuyPlot && hasVerifyLoad;
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                ok
                    ? "Playtest profile includes farming foundation steps."
                    : "Playtest profile missing farm plot purchase or persistence verification steps.");
        }

        private static void ValidateDocumentation(CCS_SurvivalValidationReport report)
        {
            bool ok = File.Exists(ModuleDocPath);
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                ok ? "Farming module documentation exists." : "Missing CCS_Farming_Module.md.");
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
