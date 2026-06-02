using System.IO;
using CCS.Modules.Economy;
using CCS.Modules.Inventory;
using CCS.Modules.Mounts;
using CCS.Modules.Playtesting;
using CCS.Survival;
using CCS.Survival.Composition;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEngine;

namespace CCS.Modules.Firearms.Editor
{
    public sealed class CCS_FirearmFoundationValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string ValidatorContext = "ccs.survival.validation.frontier.firearm";
        private const string FirearmProfilePath = "Assets/CCS/Survival/Profiles/Firearms/CCS_DefaultFirearmProfile.asset";
        private const string GunsmithVendorPath = "Assets/CCS/Survival/Content/Vendors/CCS_Vendor_FrontierGunsmith.asset";
        private const string StableVendorPath = "Assets/CCS/Survival/Content/Vendors/CCS_Vendor_FrontierStable.asset";
        private const string GeneralStoreVendorPath = "Assets/CCS/Survival/Content/Vendors/CCS_Vendor_GeneralStore.asset";
        private const string RevolverPrefabPath = "Assets/CCS/Survival/Prefabs/Weapons/PF_CCS_FrontierRevolver.prefab";

        public string ValidatorId => ValidatorContext;

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateBundleVersion(report);
            ValidateFirearmProfile(report);
            ValidateGunsmithVendor(report);
            ValidateStableDoesNotSellFirearms(report);
            ValidateGeneralStoreDoesNotSellFirearms(report);
            ValidateRevolverPrefab(report);
            ValidateCompositionHost(report);
            ValidatePlaytestSteps(report);
        }

        private static void ValidateBundleVersion(CCS_SurvivalValidationReport report)
        {
            string projectSettingsPath = "ProjectSettings/ProjectSettings.asset";
            bool ok = File.Exists(projectSettingsPath)
                && File.ReadAllText(projectSettingsPath).Contains("bundleVersion: 1.6.0");
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                ok ? "bundleVersion is 1.6.0." : "Expected bundleVersion 1.6.0. Run CCS_FirearmFoundationBootstrapSetup.ExecuteBatch.");
        }

        private static void ValidateFirearmProfile(CCS_SurvivalValidationReport report)
        {
            CCS_FirearmProfile profile = AssetDatabase.LoadAssetAtPath<CCS_FirearmProfile>(FirearmProfilePath);
            CCS_SurvivalValidationResult result = CCS_FirearmValidationUtility.ValidateProfile(profile);
            report.AddIssue(
                result.IsSuccess ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                result.Message);
        }

        private static void ValidateGunsmithVendor(CCS_SurvivalValidationReport report)
        {
            CCS_VendorDefinition vendor = AssetDatabase.LoadAssetAtPath<CCS_VendorDefinition>(GunsmithVendorPath);
            bool sellsRevolver = VendorSellsItem(vendor, CCS_FirearmContentIds.FrontierRevolverItemId);
            bool sellsAmmo = VendorSellsItem(vendor, CCS_FirearmContentIds.RevolverCartridgeItemId);
            bool ok = vendor != null
                && vendor.VendorId == CCS_FirearmContentIds.FrontierGunsmithVendorId
                && sellsRevolver
                && sellsAmmo;
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                ok
                    ? "Frontier gunsmith vendor sells firearms and ammunition."
                    : "Frontier gunsmith vendor is missing or incomplete.");
        }

        private static void ValidateStableDoesNotSellFirearms(CCS_SurvivalValidationReport report)
        {
            bool sells = VendorSellsAnyFirearm(
                AssetDatabase.LoadAssetAtPath<CCS_VendorDefinition>(StableVendorPath));
            report.AddIssue(
                sells ? CCS_SurvivalValidationIssueSeverity.Error : CCS_SurvivalValidationIssueSeverity.Info,
                ValidatorContext,
                sells ? "Stable must not sell firearms." : "Stable does not sell firearms.");
        }

        private static void ValidateGeneralStoreDoesNotSellFirearms(CCS_SurvivalValidationReport report)
        {
            bool sells = VendorSellsAnyFirearm(
                AssetDatabase.LoadAssetAtPath<CCS_VendorDefinition>(GeneralStoreVendorPath));
            report.AddIssue(
                sells ? CCS_SurvivalValidationIssueSeverity.Error : CCS_SurvivalValidationIssueSeverity.Info,
                ValidatorContext,
                sells ? "General store must not sell firearms." : "General store does not sell firearms.");
        }

        private static void ValidateRevolverPrefab(CCS_SurvivalValidationReport report)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(RevolverPrefabPath);
            bool ok = prefab != null;
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                ok ? "PF_CCS_FrontierRevolver prefab exists." : "Missing PF_CCS_FrontierRevolver prefab.");
        }

        private static void ValidateCompositionHost(CCS_SurvivalValidationReport report)
        {
            const string bootstrapPrefabPath = "Assets/CCS/Survival/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(bootstrapPrefabPath);
            CCS_SurvivalGameplayServiceHost host = prefab != null ? prefab.GetComponent<CCS_SurvivalGameplayServiceHost>() : null;
            SerializedObject serialized = host != null ? new SerializedObject(host) : null;
            bool ok = serialized != null
                && serialized.FindProperty("firearmProfile").objectReferenceValue != null;
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                ok
                    ? "Bootstrap host references firearm profile."
                    : "Bootstrap host missing firearm profile reference.");
        }

        private static void ValidatePlaytestSteps(CCS_SurvivalValidationReport report)
        {
            const string playtestProfilePath = "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset";
            CCS_PlaytestProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(playtestProfilePath);
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
                if (steps[index]?.StepType == CCS_PlaytestStepType.BuyRevolverFromGunsmith)
                {
                    hasBuy = true;
                }

                if (steps[index]?.StepType == CCS_PlaytestStepType.VerifyFirearmPersistenceAfterLoad)
                {
                    hasVerify = true;
                }
            }

            bool ok = hasBuy && hasVerify;
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                ok
                    ? "Playtest profile includes firearm foundation steps."
                    : "Playtest profile missing firearm purchase or persistence steps.");
        }

        private static bool VendorSellsItem(CCS_VendorDefinition vendor, string itemId)
        {
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
                    && entry.ItemDefinition.ItemId == itemId)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool VendorSellsAnyFirearm(CCS_VendorDefinition vendor)
        {
            return VendorSellsItem(vendor, CCS_FirearmContentIds.FrontierRevolverItemId)
                || VendorSellsItem(vendor, CCS_FirearmContentIds.FrontierRifleItemId)
                || VendorSellsItem(vendor, CCS_FirearmContentIds.FrontierShotgunItemId);
        }
    }
}
