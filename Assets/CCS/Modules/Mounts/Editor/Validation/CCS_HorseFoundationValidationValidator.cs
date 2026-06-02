using System.IO;
using CCS.Modules.CharacterController;
using CCS.Modules.Economy;
using CCS.Modules.Storage;
using CCS.Modules.Playtesting;
using CCS.Survival;
using CCS.Survival.Composition;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEngine;

namespace CCS.Modules.Mounts.Editor
{
    public sealed class CCS_HorseFoundationValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string ValidatorContext = "ccs.survival.validation.frontier.horse";
        private const string MountProfilePath = "Assets/CCS/Survival/Profiles/Mounts/CCS_DefaultMountProfile.asset";
        private const string HorseDefinitionPath = "Assets/CCS/Survival/Content/Mounts/CCS_Mount_Horse.asset";
        private const string HorsePrefabPath = "Assets/CCS/Survival/Prefabs/Mounts/PF_CCS_Horse.prefab";
        private const string HorseItemPath = "Assets/CCS/Survival/Content/Mounts/CCS_Item_FrontierHorse.asset";
        private const string StableVendorPath = "Assets/CCS/Survival/Content/Vendors/CCS_Vendor_FrontierStable.asset";
        private const string GeneralStoreVendorPath = "Assets/CCS/Survival/Content/Vendors/CCS_Vendor_GeneralStore.asset";
        private const string CharacterProfilePath = "Assets/CCS/Survival/Profiles/CharacterController/CCS_DefaultCharacterControllerProfile.asset";

        public string ValidatorId => ValidatorContext;

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateBundleVersion(report);
            ValidateMountProfile(report);
            ValidateHorseDefinition(report);
            ValidateStableVendor(report);
            ValidateGeneralStoreDoesNotSellHorses(report);
            ValidateHorsePrefab(report);
            ValidateCameraHorsePlaceholder(report);
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
                ok ? "bundleVersion is 1.6.0." : "Expected bundleVersion 1.6.0. Run CCS_WagonFoundationBootstrapSetup.ExecuteBatch.");
        }

        private static void ValidateMountProfile(CCS_SurvivalValidationReport report)
        {
            CCS_MountProfile profile = AssetDatabase.LoadAssetAtPath<CCS_MountProfile>(MountProfilePath);
            CCS_SurvivalValidationResult result = CCS_MountValidationUtility.ValidateProfile(profile);
            report.AddIssue(
                result.IsSuccess ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                result.Message);
        }

        private static void ValidateHorseDefinition(CCS_SurvivalValidationReport report)
        {
            CCS_MountDefinition horse = AssetDatabase.LoadAssetAtPath<CCS_MountDefinition>(HorseDefinitionPath);
            CCS_SurvivalValidationResult result = CCS_MountValidationUtility.ValidateDefinition(horse);
            report.AddIssue(
                result.IsSuccess ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                result.Message);
        }

        private static void ValidateStableVendor(CCS_SurvivalValidationReport report)
        {
            CCS_VendorDefinition vendor = AssetDatabase.LoadAssetAtPath<CCS_VendorDefinition>(StableVendorPath);
            bool ok = vendor != null
                && vendor.VendorId == CCS_MountContentIds.FrontierStableVendorId
                && vendor.VendorInventory != null
                && vendor.VendorInventory.Items != null
                && vendor.VendorInventory.Items.Length > 0;
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                ok
                    ? "Frontier stable vendor exists with horse purchase catalog."
                    : "Missing or invalid frontier stable vendor.");
        }

        private static void ValidateGeneralStoreDoesNotSellHorses(CCS_SurvivalValidationReport report)
        {
            CCS_VendorDefinition vendor = AssetDatabase.LoadAssetAtPath<CCS_VendorDefinition>(GeneralStoreVendorPath);
            bool sellsHorse = false;
            if (vendor?.VendorInventory?.Items != null)
            {
                CCS_VendorItemEntry[] items = vendor.VendorInventory.Items;
                for (int index = 0; index < items.Length; index++)
                {
                    CCS_VendorItemEntry entry = items[index];
                    if (entry?.ItemDefinition != null
                        && entry.AllowBuy
                        && entry.ItemDefinition.ItemId == CCS_MountContentIds.FrontierHorseItemId)
                    {
                        sellsHorse = true;
                        break;
                    }
                }
            }

            report.AddIssue(
                sellsHorse ? CCS_SurvivalValidationIssueSeverity.Error : CCS_SurvivalValidationIssueSeverity.Info,
                ValidatorContext,
                sellsHorse
                    ? "General store must not sell frontier horses."
                    : "General store does not sell horses.");
        }

        private static void ValidateHorsePrefab(CCS_SurvivalValidationReport report)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(HorsePrefabPath);
            bool ok = prefab != null
                && prefab.GetComponent<CCS_MountWorldActor>() != null
                && prefab.GetComponent<CCS_MountInteractable>() != null
                && prefab.GetComponent<CCS_HorseSaddlebagContainer>() != null
                && prefab.GetComponent<CCS_StorageContainer>() != null;
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                ok ? "PF_CCS_Horse prefab includes mount and saddlebag components." : "PF_CCS_Horse prefab is missing required components.");
        }

        private static void ValidateCameraHorsePlaceholder(CCS_SurvivalValidationReport report)
        {
            CCS_CharacterControllerProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_CharacterControllerProfile>(CharacterProfilePath);
            bool hasHorseDistance = profile?.Camera != null && profile.Camera.HorseCameraDistance > 0f;
            bool enumDefinesHorse = System.Enum.IsDefined(typeof(CCS_CharacterCameraMode), CCS_CharacterCameraMode.Horse);
            bool ok = hasHorseDistance && enumDefinesHorse;
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                ok
                    ? "Character camera profile includes Horse mode placeholder tuning."
                    : "Horse camera mode placeholder is missing from character controller profile.");
        }

        private static void ValidateCompositionHost(CCS_SurvivalValidationReport report)
        {
            const string bootstrapPrefabPath = "Assets/CCS/Survival/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(bootstrapPrefabPath);
            CCS_SurvivalGameplayServiceHost host = prefab != null ? prefab.GetComponent<CCS_SurvivalGameplayServiceHost>() : null;
            SerializedObject serialized = host != null ? new SerializedObject(host) : null;
            bool ok = serialized != null
                && serialized.FindProperty("mountProfile").objectReferenceValue != null;
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                ok
                    ? "Bootstrap host references mount profile."
                    : "Bootstrap host missing mount profile reference.");
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
                if (steps[index]?.StepType == CCS_PlaytestStepType.BuyHorseFromStable)
                {
                    hasBuy = true;
                }

                if (steps[index]?.StepType == CCS_PlaytestStepType.VerifyHorsePersistenceAfterLoad)
                {
                    hasVerify = true;
                }
            }

            bool ok = hasBuy && hasVerify;
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                ok
                    ? "Playtest profile includes horse foundation steps."
                    : "Playtest profile missing horse purchase or persistence verification steps.");
        }
    }
}
