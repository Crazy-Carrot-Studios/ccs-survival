using System.IO;
using CCS.Modules.Combat;
using CCS.Modules.Economy;
using CCS.Modules.Hotbar;
using CCS.Modules.Inventory;
using CCS.Modules.Playtesting;
using CCS.Modules.Wildlife;
using CCS.Survival;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_FrontierHuntingValidationValidator
// CATEGORY: Modules / Wildlife / Editor / Validation
// PURPOSE: Validates frontier hunting harvest profile, bow weapon tuning, turkey content, and playtest steps.
// PLACEMENT: Registered on CCS_SurvivalValidationPipeline at editor load.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Milestone 1.3.2 frontier hunting foundation validation.
// =============================================================================

namespace CCS.Modules.Wildlife.Editor
{
    public sealed class CCS_FrontierHuntingValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string SurvivalRoot = "Assets/CCS/Survival";
        private const string HarvestContentRoot = SurvivalRoot + "/Content/Wildlife/Harvest";
        private const string DefaultHarvestProfilePath =
            SurvivalRoot + "/Profiles/Wildlife/CCS_DefaultWildlifeHarvestProfile.asset";
        private const string DefaultWildlifeProfilePath =
            SurvivalRoot + "/Profiles/Wildlife/CCS_DefaultWildlifeProfile.asset";
        private const string DefaultCombatProfilePath = SurvivalRoot + "/Profiles/Combat/CCS_DefaultCombatProfile.asset";
        private const string DefaultActiveItemProfilePath =
            SurvivalRoot + "/Profiles/Hotbar/CCS_DefaultActiveItemProfile.asset";
        private const string DefaultPlaytestProfilePath =
            SurvivalRoot + "/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset";
        private const string GeneralStoreVendorPath = SurvivalRoot + "/Content/Vendors/CCS_Vendor_GeneralStore.asset";
        private const string BowItemPath = SurvivalRoot + "/Content/Items/Frontier/CCS_Item_Bow.asset";
        private const string BoneItemPath = SurvivalRoot + "/Content/Items/Resources/Primitive/CCS_Item_Bone.asset";
        private const string BootstrapScenePath = SurvivalRoot + "/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string RabbitDefinitionPath = SurvivalRoot + "/Content/Wildlife/Definitions/CCS_TestRabbit.asset";
        private const string DeerDefinitionPath = SurvivalRoot + "/Content/Wildlife/Definitions/CCS_TestDeerCarcass.asset";
        private const string TurkeyDefinitionPath = SurvivalRoot + "/Content/Wildlife/Definitions/CCS_TestTurkeyCarcass.asset";
        private const string RabbitHarvestPath = HarvestContentRoot + "/CCS_HarvestDefinition_Rabbit.asset";
        private const string TurkeyHarvestPath = HarvestContentRoot + "/CCS_HarvestDefinition_Turkey.asset";
        private const string DeerHarvestPath = HarvestContentRoot + "/CCS_HarvestDefinition_Deer.asset";
        private const string ProjectSettingsPath = "ProjectSettings/ProjectSettings.asset";
        private const string BowItemId = "ccs.survival.item.frontier.bow";

        private static readonly string[] RequiredHarvestDefinitionPaths =
        {
            RabbitHarvestPath,
            TurkeyHarvestPath,
            DeerHarvestPath
        };

        private static readonly string[] RequiredBootstrapObjectNames =
        {
            "CCS_TestTurkeyCarcass",
            "CCS_TestTurkey"
        };

        private static readonly string[] RequiredHuntingStepIds =
        {
            "ccs.survival.playtest.hunting.bow.obtain",
            "ccs.survival.playtest.hunting.bow.equip",
            "ccs.survival.playtest.hunting.rabbit.kill",
            "ccs.survival.playtest.hunting.knife.equip",
            "ccs.survival.playtest.hunting.carcass.harvest",
            "ccs.survival.playtest.hunting.hide.verify",
            "ccs.survival.playtest.hunting.hide.sell",
            "ccs.survival.playtest.hunting.currency.verify"
        };

        #region Properties

        public string ValidatorId => "ccs.survival.validation.frontier.hunting";

        #endregion

        #region Public Methods

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateProjectVersion(report);
            ValidateRequiredFolder(report, "Wildlife Harvest Content", HarvestContentRoot);
            ValidateHarvestDefinitionAssets(report);
            ValidateHarvestProfile(report);
            ValidateWildlifeProfileHarvestLink(report);
            ValidateBowItem(report);
            ValidateCombatTurkeySettings(report);
            ValidateBootstrapTurkeyContent(report);
            ValidateVendorBoneSell(report);
            ValidateActiveItemHarvestRouting(report);
            ValidatePlaytestHuntingSteps(report);

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,
                ValidatorId,
                "Frontier hunting validator completed (milestone 1.3.2).");
        }

        #endregion

        #region Private Methods

        private static void ValidateProjectVersion(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(ProjectSettingsPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Project Version",
                    "Missing ProjectSettings.asset.");
                return;
            }

            string projectSettingsText = File.ReadAllText(ProjectSettingsPath);
            report.AddIssue(
                projectSettingsText.Contains("bundleVersion: 1.3.2")
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Project Version",
                projectSettingsText.Contains("bundleVersion: 1.3.2")
                    ? "bundleVersion is 1.3.2."
                    : "Expected bundleVersion 1.3.2. Run CCS_FrontierHuntingBootstrapSetup.ExecuteBatch.");
        }

        private static void ValidateRequiredFolder(
            CCS_SurvivalValidationReport report,
            string context,
            string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    context,
                    $"Folder present: {folderPath}");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Error,
                context,
                $"Missing required folder: {folderPath}");
        }

        private static void ValidateHarvestDefinitionAssets(CCS_SurvivalValidationReport report)
        {
            ValidateRequiredAsset(report, "Test Rabbit Definition", RabbitDefinitionPath);
            ValidateRequiredAsset(report, "Test Deer Carcass Definition", DeerDefinitionPath);
            ValidateRequiredAsset(report, "Test Turkey Carcass Definition", TurkeyDefinitionPath);

            for (int index = 0; index < RequiredHarvestDefinitionPaths.Length; index++)
            {
                string assetPath = RequiredHarvestDefinitionPaths[index];
                if (!File.Exists(assetPath))
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Wildlife Harvest Definitions",
                        $"Missing harvest definition asset: {assetPath}");
                    continue;
                }

                CCS_WildlifeHarvestDefinition definition =
                    AssetDatabase.LoadAssetAtPath<CCS_WildlifeHarvestDefinition>(assetPath);
                if (definition == null)
                {
                    continue;
                }

                CCS_SurvivalValidationResult validation =
                    CCS_WildlifeHarvestValidationUtility.ValidateHarvestDefinition(definition);
                report.AddIssue(
                    validation.IsSuccess
                        ? CCS_SurvivalValidationIssueSeverity.Info
                        : CCS_SurvivalValidationIssueSeverity.Error,
                    "Wildlife Harvest Definitions",
                    validation.Message);
            }

            ValidateWildlifeHarvestLink(
                report,
                "Test Rabbit Harvest Link",
                RabbitDefinitionPath,
                RabbitHarvestPath);
            ValidateWildlifeHarvestLink(
                report,
                "Test Turkey Harvest Link",
                TurkeyDefinitionPath,
                TurkeyHarvestPath);
            ValidateWildlifeHarvestLink(
                report,
                "Test Deer Harvest Link",
                DeerDefinitionPath,
                DeerHarvestPath);
        }

        private static void ValidateWildlifeHarvestLink(
            CCS_SurvivalValidationReport report,
            string context,
            string wildlifeDefinitionPath,
            string harvestDefinitionPath)
        {
            CCS_WildlifeDefinition wildlifeDefinition =
                AssetDatabase.LoadAssetAtPath<CCS_WildlifeDefinition>(wildlifeDefinitionPath);
            CCS_WildlifeHarvestDefinition harvestDefinition =
                AssetDatabase.LoadAssetAtPath<CCS_WildlifeHarvestDefinition>(harvestDefinitionPath);

            if (wildlifeDefinition == null || harvestDefinition == null)
            {
                return;
            }

            bool linked = wildlifeDefinition.HarvestDefinition == harvestDefinition
                && harvestDefinition.WildlifeDefinition == wildlifeDefinition;
            report.AddIssue(
                linked
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                context,
                linked
                    ? $"Wildlife and harvest definitions linked at {wildlifeDefinitionPath}."
                    : $"Wildlife definition at {wildlifeDefinitionPath} is not linked to {harvestDefinitionPath}.");
        }

        private static void ValidateHarvestProfile(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(DefaultHarvestProfilePath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Default Wildlife Harvest Profile",
                    $"Missing asset: {DefaultHarvestProfilePath}");
                return;
            }

            CCS_WildlifeHarvestProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_WildlifeHarvestProfile>(DefaultHarvestProfilePath);
            if (profile == null)
            {
                return;
            }

            CCS_SurvivalValidationResult validation =
                CCS_WildlifeHarvestValidationUtility.ValidateHarvestProfile(profile);
            report.AddIssue(
                validation.IsSuccess
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Default Wildlife Harvest Profile",
                validation.Message);

            if (profile.ProfileVersion != "1.3.2")
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Default Wildlife Harvest Profile Version",
                    $"Expected profileVersion 1.3.2 but found '{profile.ProfileVersion}'.");
            }
        }

        private static void ValidateWildlifeProfileHarvestLink(CCS_SurvivalValidationReport report)
        {
            CCS_WildlifeProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_WildlifeProfile>(DefaultWildlifeProfilePath);
            if (profile == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Default Wildlife Profile",
                    $"Missing asset: {DefaultWildlifeProfilePath}");
                return;
            }

            CCS_WildlifeHarvestProfile harvestProfile =
                AssetDatabase.LoadAssetAtPath<CCS_WildlifeHarvestProfile>(DefaultHarvestProfilePath);
            report.AddIssue(
                profile.HarvestProfile == harvestProfile
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Default Wildlife Profile Harvest Link",
                profile.HarvestProfile == harvestProfile
                    ? "Default wildlife profile assigns the default harvest profile."
                    : "Default wildlife profile is missing harvest profile assignment.");
        }

        private static void ValidateBowItem(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(BowItemPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Frontier Bow Item",
                    $"Missing asset: {BowItemPath}");
                return;
            }

            CCS_ItemDefinition bow = AssetDatabase.LoadAssetAtPath<CCS_ItemDefinition>(BowItemPath);
            if (bow == null)
            {
                return;
            }

            bool validIdentity = bow.HasWeaponIdentity
                && bow.WeaponArchetype == CCS_WeaponArchetype.Bow
                && bow.RangeType == CCS_RangeType.LongRanged
                && bow.GameplayKind == CCS_ItemGameplayKind.Weapon;
            bool validStats = Mathf.Approximately(bow.MeleeDamage, 14f)
                && Mathf.Approximately(bow.MeleeRange, 28f);

            report.AddIssue(
                validIdentity && validStats
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Frontier Bow Item",
                validIdentity && validStats
                    ? "Bow has frontier hunting weapon identity and tuning (14 damage, 28 range)."
                    : "Bow is missing frontier hunting weapon identity or expected melee tuning.");

            if (bow.ItemId != BowItemId)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Frontier Bow Item ID",
                    $"Expected itemId {BowItemId} but found '{bow.ItemId}'.");
            }
        }

        private static void ValidateCombatTurkeySettings(CCS_SurvivalValidationReport report)
        {
            CCS_CombatProfile profile = AssetDatabase.LoadAssetAtPath<CCS_CombatProfile>(DefaultCombatProfilePath);
            if (profile == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Combat Turkey Settings",
                    $"Missing combat profile: {DefaultCombatProfilePath}");
                return;
            }

            CCS_WildlifeDefinition turkeyDefinition =
                AssetDatabase.LoadAssetAtPath<CCS_WildlifeDefinition>(TurkeyDefinitionPath);
            CCS_CombatWildlifeSpeciesSettings turkeySettings = profile.GetSpeciesSettings(CCS_WildlifeAiSpecies.Turkey);

            bool validSettings = turkeySettings.maxHealth > 0f
                && turkeySettings.carcassObjectName == "CCS_TestTurkeyCarcass";
            bool validCarcassDefinition = profile.GetCarcassDefinition(CCS_WildlifeAiSpecies.Turkey) == turkeyDefinition;

            report.AddIssue(
                validSettings && validCarcassDefinition
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Combat Turkey Settings",
                validSettings && validCarcassDefinition
                    ? "Combat profile includes turkey species settings and carcass definition."
                    : "Combat profile is missing turkey species settings or carcass definition link.");
        }

        private static void ValidateBootstrapTurkeyContent(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(BootstrapScenePath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Bootstrap Turkey Content",
                    $"Missing bootstrap scene: {BootstrapScenePath}");
                return;
            }

            string sceneText = File.ReadAllText(BootstrapScenePath);
            for (int index = 0; index < RequiredBootstrapObjectNames.Length; index++)
            {
                string objectName = RequiredBootstrapObjectNames[index];
                report.AddIssue(
                    sceneText.Contains(objectName)
                        ? CCS_SurvivalValidationIssueSeverity.Info
                        : CCS_SurvivalValidationIssueSeverity.Error,
                    "Bootstrap Turkey Content",
                    sceneText.Contains(objectName)
                        ? $"Bootstrap scene contains {objectName}."
                        : $"Bootstrap scene is missing {objectName}.");
            }
        }

        private static void ValidateVendorBoneSell(CCS_SurvivalValidationReport report)
        {
            CCS_ItemDefinition boneItem = AssetDatabase.LoadAssetAtPath<CCS_ItemDefinition>(BoneItemPath);
            CCS_VendorDefinition vendor = AssetDatabase.LoadAssetAtPath<CCS_VendorDefinition>(GeneralStoreVendorPath);
            if (boneItem == null || vendor == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Vendor Bone Sell",
                    "Missing bone item or general store vendor asset.");
                return;
            }

            bool sellableInCatalog = false;
            CCS_VendorItemEntry[] entries = vendor.VendorInventory?.Items;
            if (entries != null)
            {
                for (int index = 0; index < entries.Length; index++)
                {
                    CCS_VendorItemEntry entry = entries[index];
                    if (entry?.ItemDefinition == boneItem && entry.AllowSell)
                    {
                        sellableInCatalog = true;
                        break;
                    }
                }
            }

            report.AddIssue(
                boneItem.HasEconomyValues && boneItem.SellValue > 0 && sellableInCatalog
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Vendor Bone Sell",
                boneItem.HasEconomyValues && boneItem.SellValue > 0 && sellableInCatalog
                    ? "General store vendor accepts bone sell transactions."
                    : "Bone item or vendor catalog is missing sell configuration.");
        }

        private static void ValidateActiveItemHarvestRouting(CCS_SurvivalValidationReport report)
        {
            CCS_ActiveItemProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_ActiveItemProfile>(DefaultActiveItemProfilePath);
            if (profile == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Active Item Harvest Routing",
                    $"Missing active item profile: {DefaultActiveItemProfilePath}");
                return;
            }

            report.AddIssue(
                profile.EnableWildlifeHarvestRouting
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Active Item Harvest Routing",
                profile.EnableWildlifeHarvestRouting
                    ? "Active item profile enables wildlife harvest routing."
                    : "enableWildlifeHarvestRouting must be enabled on the default active item profile.");
        }

        private static void ValidatePlaytestHuntingSteps(CCS_SurvivalValidationReport report)
        {
            CCS_PlaytestProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(DefaultPlaytestProfilePath);
            if (profile == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Warning,
                    "Playtest Hunting Steps",
                    $"Missing playtest profile: {DefaultPlaytestProfilePath}");
                return;
            }

            if (profile.ProfileVersion != "1.3.2")
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Warning,
                    "Playtest Profile Version",
                    $"Expected profileVersion 1.3.2 but found '{profile.ProfileVersion}'.");
            }

            for (int index = 0; index < RequiredHuntingStepIds.Length; index++)
            {
                string stepId = RequiredHuntingStepIds[index];
                bool found = false;
                for (int stepIndex = 0; stepIndex < profile.StepDefinitions.Count; stepIndex++)
                {
                    if (profile.StepDefinitions[stepIndex]?.StepId == stepId)
                    {
                        found = true;
                        break;
                    }
                }

                report.AddIssue(
                    found
                        ? CCS_SurvivalValidationIssueSeverity.Info
                        : CCS_SurvivalValidationIssueSeverity.Error,
                    "Playtest Hunting Steps",
                    found
                        ? $"Playtest profile contains step {stepId}."
                        : $"Playtest profile is missing step {stepId}.");
            }

            ValidatePlaytestStepType(
                report,
                profile,
                "ccs.survival.playtest.hunting.bow.obtain",
                CCS_PlaytestStepType.ObtainBowForHunt);
            ValidatePlaytestStepType(
                report,
                profile,
                "ccs.survival.playtest.hunting.bow.equip",
                CCS_PlaytestStepType.EquipBowForHunt);
            ValidatePlaytestStepType(
                report,
                profile,
                "ccs.survival.playtest.hunting.hide.sell",
                CCS_PlaytestStepType.SellHuntingResourceAtVendor);
        }

        private static void ValidatePlaytestStepType(
            CCS_SurvivalValidationReport report,
            CCS_PlaytestProfile profile,
            string stepId,
            CCS_PlaytestStepType expectedType)
        {
            for (int index = 0; index < profile.StepDefinitions.Count; index++)
            {
                CCS_PlaytestStepDefinition step = profile.StepDefinitions[index];
                if (step?.StepId != stepId)
                {
                    continue;
                }

                report.AddIssue(
                    step.StepType == expectedType
                        ? CCS_SurvivalValidationIssueSeverity.Info
                        : CCS_SurvivalValidationIssueSeverity.Error,
                    "Playtest Hunting Step Types",
                    step.StepType == expectedType
                        ? $"Step {stepId} uses {expectedType}."
                        : $"Step {stepId} expected {expectedType} but found {step.StepType}.");
                return;
            }
        }

        private static void ValidateRequiredAsset(
            CCS_SurvivalValidationReport report,
            string context,
            string assetPath)
        {
            if (File.Exists(assetPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    context,
                    $"Asset present: {assetPath}");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Error,
                context,
                $"Missing required asset: {assetPath}");
        }

        #endregion
    }
}
