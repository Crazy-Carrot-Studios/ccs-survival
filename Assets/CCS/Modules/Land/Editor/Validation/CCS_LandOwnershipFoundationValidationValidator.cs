using System.IO;
using CCS.Modules.Economy;
using CCS.Modules.Inventory;
using CCS.Modules.Land;
using CCS.Modules.Playtesting;
using CCS.Survival;
using CCS.Survival.Composition;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_LandOwnershipFoundationValidationValidator
// CATEGORY: Modules / Land / Editor / Validation
// PURPOSE: Validates land module layout, content, composition, and save wiring.
// PLACEMENT: Registered on CCS_SurvivalValidationPipeline at editor load.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// NOTES: Milestone 2.3.0 land ownership foundation.
// =============================================================================

namespace CCS.Modules.Land.Editor
{
    public sealed class CCS_LandOwnershipFoundationValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string ValidatorContext = "ccs.survival.validation.land";
        private const string LandMilestoneVersion = "2.3.0";
        private const string ModuleRoot = "Assets/CCS/Modules/Land";
        private const string RuntimeRoot = ModuleRoot + "/Runtime";
        private const string EditorRoot = ModuleRoot + "/Editor";
        private const string ModuleDocPath = ModuleRoot + "/Documentation/CCS_Land_Module.md";
        private const string LandProfilePath = "Assets/CCS/Survival/Profiles/Land/CCS_DefaultLandClaimProfile.asset";
        private const string BootstrapPrefabPath = "Assets/CCS/Survival/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
        private const string CompositionRegistrationPath =
            "Assets/CCS/Survival/Runtime/Composition/CCS_SurvivalGameplayServiceRegistration.cs";
        private const string SaveDataPath = "Assets/CCS/Modules/SaveSystem/Runtime/Data/CCS_SaveData.cs";
        private const string GeneralStoreVendorPath = "Assets/CCS/Survival/Content/Vendors/CCS_Vendor_GeneralStore.asset";
        private const string DefaultPlaytestProfilePath = "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset";

        public string ValidatorId => ValidatorContext;

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateRequiredFolder(report, "Modules/Land", ModuleRoot);
            ValidateRequiredFolder(report, "Runtime/Data", RuntimeRoot + "/Data");
            ValidateRequiredFolder(report, "Runtime/Definitions", RuntimeRoot + "/Definitions");
            ValidateRequiredFolder(report, "Runtime/Profiles", RuntimeRoot + "/Profiles");
            ValidateRequiredFolder(report, "Runtime/Services", RuntimeRoot + "/Services");
            ValidateRequiredFolder(report, "Runtime/Validation", RuntimeRoot + "/Validation");
            ValidateRequiredFolder(report, "Editor/Validation", EditorRoot + "/Validation");
            ValidateRequiredFolder(report, "Documentation", ModuleRoot + "/Documentation");

            ValidateRequiredFile(report, "Runtime asmdef", RuntimeRoot + "/CCS.Modules.Land.Runtime.asmdef");
            ValidateRequiredFile(report, "Editor asmdef", EditorRoot + "/CCS.Modules.Land.Editor.asmdef");
            ValidateRequiredScript(report, "CCS_LandClaimService", RuntimeRoot + "/Services/CCS_LandClaimService.cs");
            ValidateRequiredScript(
                report,
                "CCS_LandClaimRuntimeBridge",
                RuntimeRoot + "/Services/CCS_LandClaimRuntimeBridge.cs");
            ValidateRequiredScript(
                report,
                "CCS_LandClaimValidationUtility",
                RuntimeRoot + "/Validation/CCS_LandClaimValidationUtility.cs");
            ValidateRequiredScript(
                report,
                "CCS_LandOwnershipFoundationBootstrapSetup",
                EditorRoot + "/Validation/CCS_LandOwnershipFoundationBootstrapSetup.cs");

            ValidateLandProfile(report);
            ValidateDeedItem(report);
            ValidateVendorCatalog(report);
            ValidateCompositionRegistration(report);
            ValidateBootstrapWiring(report);
            ValidateSaveSupport(report);
            ValidatePlaytestSteps(report);
            ValidateDocumentation(report);

            CCS_SurvivalBootstrapVersionUtility.AddBundleVersionValidationIssue(
                report,
                ValidatorContext,
                LandMilestoneVersion,
                "Run CCS_LandOwnershipFoundationBootstrapSetup.ExecuteBatch.");
            CCS_SurvivalBootstrapVersionUtility.ValidateNoHardcodedBootstrapVersionWrites(report, ValidatorContext);
            CCS_SurvivalInputValidationUtility.ValidateNoLegacyInputUsage(report, "Land Input Policy");
            CCS_SurvivalInputValidationUtility.ValidateNoFindObjectsSortModeUsage(report, "Land Obsolete API Scan");
        }

        private static void ValidateLandProfile(CCS_SurvivalValidationReport report)
        {
            CCS_LandClaimProfile profile = AssetDatabase.LoadAssetAtPath<CCS_LandClaimProfile>(LandProfilePath);
            CCS_SurvivalValidationResult result = CCS_LandClaimValidationUtility.ValidateProfile(profile);
            report.AddIssue(
                result.IsSuccess ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                result.Message);
        }

        private static void ValidateDeedItem(CCS_SurvivalValidationReport report)
        {
            ValidateItemAtPath(
                report,
                "Assets/CCS/Survival/Content/Land/Items/CCS_Item_HomesteadClaimDeed.asset",
                CCS_LandContentIds.HomesteadClaimDeedItemId);
        }

        private static void ValidateItemAtPath(CCS_SurvivalValidationReport report, string path, string expectedItemId)
        {
            CCS_ItemDefinition item = AssetDatabase.LoadAssetAtPath<CCS_ItemDefinition>(path);
            CCS_SurvivalValidationResult result = CCS_LandClaimValidationUtility.ValidateItem(item, expectedItemId);
            report.AddIssue(
                result.IsSuccess ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                result.Message);
        }

        private static void ValidateVendorCatalog(CCS_SurvivalValidationReport report)
        {
            bool sellsDeed = VendorSellsItem(GeneralStoreVendorPath, CCS_LandContentIds.HomesteadClaimDeedItemId);
            report.AddIssue(
                sellsDeed ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                sellsDeed
                    ? "General store sells homestead claim deed."
                    : "General store missing homestead claim deed purchase entry.");
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
            bool hasLandService = source.Contains("CreateLandClaimService", System.StringComparison.Ordinal);
            bool hasLandHandler = source.Contains("BindFrontierLandClaimPlacementHandler", System.StringComparison.Ordinal);
            report.AddIssue(
                hasLandService && hasLandHandler
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                hasLandService && hasLandHandler
                    ? "Composition registers land claim service and deed placement handler."
                    : "Composition missing land claim service or placement handler wiring.");
        }

        private static void ValidateBootstrapWiring(CCS_SurvivalValidationReport report)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(BootstrapPrefabPath);
            CCS_SurvivalGameplayServiceHost host = prefab != null ? prefab.GetComponent<CCS_SurvivalGameplayServiceHost>() : null;
            SerializedObject serialized = host != null ? new SerializedObject(host) : null;
            bool ok = serialized != null
                && serialized.FindProperty("landClaimProfile").objectReferenceValue != null;
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                ok
                    ? "Bootstrap host references land claim profile."
                    : "Bootstrap host missing land claim profile reference.");
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
            bool hasLandData = source.Contains("CCS_SaveLandWorldData", System.StringComparison.Ordinal);
            report.AddIssue(
                hasLandData
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                hasLandData
                    ? "CCS_SaveData includes land world snapshots."
                    : "CCS_SaveData is missing CCS_SaveLandWorldData.");
        }

        private static void ValidatePlaytestSteps(CCS_SurvivalValidationReport report)
        {
            CCS_PlaytestProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(DefaultPlaytestProfilePath);
            if (profile == null)
            {
                report.AddIssue(CCS_SurvivalValidationIssueSeverity.Error, ValidatorContext, "Missing playtest profile.");
                return;
            }

            bool hasBuyDeed = false;
            bool hasVerifyLoad = false;
            System.Collections.Generic.IReadOnlyList<CCS_PlaytestStepDefinition> steps = profile.StepDefinitions;
            for (int index = 0; index < steps.Count; index++)
            {
                if (steps[index]?.StepType == CCS_PlaytestStepType.BuyHomesteadClaimDeed)
                {
                    hasBuyDeed = true;
                }

                if (steps[index]?.StepType == CCS_PlaytestStepType.VerifyLandClaimAfterLoad)
                {
                    hasVerifyLoad = true;
                }
            }

            bool ok = hasBuyDeed && hasVerifyLoad;
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                ok
                    ? "Playtest profile includes land ownership foundation steps."
                    : "Playtest profile missing homestead deed purchase or persistence verification steps.");
        }

        private static void ValidateDocumentation(CCS_SurvivalValidationReport report)
        {
            bool ok = File.Exists(ModuleDocPath);
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                ok ? "Land module documentation exists." : "Missing CCS_Land_Module.md.");
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
