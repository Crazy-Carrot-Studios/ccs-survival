using System.IO;
using CCS.Modules.Economy;
using CCS.Modules.Mounts;
using CCS.Modules.Playtesting;
using CCS.Modules.Storage;
using CCS.Survival;
using CCS.Survival.Composition;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEngine;

namespace CCS.Modules.Vehicles.Editor
{
    public sealed class CCS_WagonFoundationValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string ValidatorContext = "ccs.survival.validation.frontier.wagon";
        private const string VehicleProfilePath = "Assets/CCS/Survival/Profiles/Vehicles/CCS_DefaultVehicleProfile.asset";
        private const string WagonDefinitionPath = "Assets/CCS/Survival/Content/Vehicles/CCS_Vehicle_FrontierWagon.asset";
        private const string WagonPrefabPath = "Assets/CCS/Survival/Prefabs/Vehicles/PF_CCS_FrontierWagon.prefab";
        private const string WagonItemPath = "Assets/CCS/Survival/Content/Vehicles/CCS_Item_FrontierWagonDeed.asset";
        private const string HorsePrefabPath = "Assets/CCS/Survival/Prefabs/Mounts/PF_CCS_Horse.prefab";
        private const string StableVendorPath = "Assets/CCS/Survival/Content/Vendors/CCS_Vendor_FrontierStable.asset";
        private const string GeneralStoreVendorPath = "Assets/CCS/Survival/Content/Vendors/CCS_Vendor_GeneralStore.asset";

        public string ValidatorId => ValidatorContext;

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateBundleVersion(report);
            ValidateVehicleProfile(report);
            ValidateWagonDefinition(report);
            ValidateWagonPrefab(report);
            ValidateHorseHitchPoint(report);
            ValidateStableVendor(report);
            ValidateGeneralStoreDoesNotSellWagon(report);
            ValidateCompositionHost(report);
            ValidatePlaytestSteps(report);
        }

        private static void ValidateBundleVersion(CCS_SurvivalValidationReport report)
        {
            CCS_SurvivalBootstrapVersionUtility.AddBundleVersionValidationIssue(
                report,
                ValidatorContext,
                remediationHint: "Run CCS_WagonFoundationBootstrapSetup.ExecuteBatch.");
        }

        private static void ValidateVehicleProfile(CCS_SurvivalValidationReport report)
        {
            CCS_VehicleProfile profile = AssetDatabase.LoadAssetAtPath<CCS_VehicleProfile>(VehicleProfilePath);
            CCS_SurvivalValidationResult result = CCS_VehicleValidationUtility.ValidateProfile(profile);
            report.AddIssue(
                result.IsSuccess ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                result.Message);
        }

        private static void ValidateWagonDefinition(CCS_SurvivalValidationReport report)
        {
            CCS_VehicleDefinition wagon = AssetDatabase.LoadAssetAtPath<CCS_VehicleDefinition>(WagonDefinitionPath);
            bool ok = wagon != null
                && wagon.VehicleId == CCS_VehicleContentIds.FrontierWagonVehicleId
                && wagon.CargoSlotCount >= 24
                && wagon.IsHitchCompatible(CCS_VehicleContentIds.HitchCompatibleHorseMountId);
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                ok ? "Frontier wagon definition is valid." : "Frontier wagon definition is missing or invalid.");
        }

        private static void ValidateWagonPrefab(CCS_SurvivalValidationReport report)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(WagonPrefabPath);
            CCS_SurvivalValidationResult result = CCS_VehicleValidationUtility.ValidateWagonPrefab(prefab);
            report.AddIssue(
                result.IsSuccess ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                result.Message);
        }

        private static void ValidateHorseHitchPoint(CCS_SurvivalValidationReport report)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(HorsePrefabPath);
            bool ok = prefab != null
                && prefab.transform.Find("WagonHitchPoint") != null
                && prefab.GetComponent<CCS_MountWorldActor>() != null;
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                ok ? "Horse prefab includes WagonHitchPoint." : "Horse prefab missing WagonHitchPoint. Run wagon bootstrap.");
        }

        private static void ValidateStableVendor(CCS_SurvivalValidationReport report)
        {
            CCS_VendorDefinition vendor = AssetDatabase.LoadAssetAtPath<CCS_VendorDefinition>(StableVendorPath);
            bool sellsHorse = false;
            bool sellsWagon = false;
            if (vendor?.VendorInventory?.Items != null)
            {
                CCS_VendorItemEntry[] items = vendor.VendorInventory.Items;
                for (int index = 0; index < items.Length; index++)
                {
                    CCS_VendorItemEntry entry = items[index];
                    if (entry?.ItemDefinition == null || !entry.AllowBuy)
                    {
                        continue;
                    }

                    if (entry.ItemDefinition.ItemId == CCS_MountContentIds.FrontierHorseItemId)
                    {
                        sellsHorse = true;
                    }

                    if (entry.ItemDefinition.ItemId == CCS_VehicleContentIds.FrontierWagonItemId)
                    {
                        sellsWagon = true;
                    }
                }
            }

            bool ok = sellsHorse && sellsWagon;
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                ok
                    ? "Frontier stable sells horse and wagon."
                    : "Frontier stable vendor must sell horse and wagon.");
        }

        private static void ValidateGeneralStoreDoesNotSellWagon(CCS_SurvivalValidationReport report)
        {
            CCS_VendorDefinition vendor = AssetDatabase.LoadAssetAtPath<CCS_VendorDefinition>(GeneralStoreVendorPath);
            bool sellsWagon = false;
            if (vendor?.VendorInventory?.Items != null)
            {
                CCS_VendorItemEntry[] items = vendor.VendorInventory.Items;
                for (int index = 0; index < items.Length; index++)
                {
                    CCS_VendorItemEntry entry = items[index];
                    if (entry?.ItemDefinition != null
                        && entry.AllowBuy
                        && entry.ItemDefinition.ItemId == CCS_VehicleContentIds.FrontierWagonItemId)
                    {
                        sellsWagon = true;
                        break;
                    }
                }
            }

            report.AddIssue(
                sellsWagon ? CCS_SurvivalValidationIssueSeverity.Error : CCS_SurvivalValidationIssueSeverity.Info,
                ValidatorContext,
                sellsWagon
                    ? "General store must not sell frontier wagons."
                    : "General store does not sell wagons.");
        }

        private static void ValidateCompositionHost(CCS_SurvivalValidationReport report)
        {
            const string bootstrapPrefabPath = "Assets/CCS/Survival/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(bootstrapPrefabPath);
            CCS_SurvivalGameplayServiceHost host = prefab != null ? prefab.GetComponent<CCS_SurvivalGameplayServiceHost>() : null;
            SerializedObject serialized = host != null ? new SerializedObject(host) : null;
            bool ok = serialized != null
                && serialized.FindProperty("vehicleProfile").objectReferenceValue != null;
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                ok
                    ? "Bootstrap host references vehicle profile."
                    : "Bootstrap host missing vehicle profile reference.");
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
                if (steps[index]?.StepType == CCS_PlaytestStepType.BuyWagonFromStable)
                {
                    hasBuy = true;
                }

                if (steps[index]?.StepType == CCS_PlaytestStepType.VerifyWagonPersistenceAfterLoad)
                {
                    hasVerify = true;
                }
            }

            bool ok = hasBuy && hasVerify;
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                ok
                    ? "Playtest profile includes wagon foundation steps."
                    : "Playtest profile missing wagon purchase or persistence verification steps.");
        }
    }
}
