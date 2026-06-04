using System.Collections.Generic;
using System.IO;
using CCS.Modules.Contracts;
using CCS.Modules.Playtesting;
using CCS.Modules.Regions;
using CCS.Modules.WorldSimulation;
using CCS.Survival;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_RegionSpecializationFoundationValidationValidator
// CATEGORY: Modules / Regions / Editor / Validation
// PURPOSE: Validates regional specialization layout, content, save, and playtest wiring.
// PLACEMENT: Registered on CCS_SurvivalValidationPipeline at editor load.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.1.0 regional specialization foundation.
// =============================================================================

namespace CCS.Modules.Regions.Editor
{
    public sealed class CCS_RegionSpecializationFoundationValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string ValidatorContext = "ccs.survival.validation.regional.specialization";
        private const string RegionalEconomyMilestoneVersion = "3.1.0";
        private const string DefaultRegionProfilePath = "Assets/CCS/Survival/Profiles/Regions/CCS_DefaultRegionProfile.asset";
        private const string WorldSimulationProfilePath =
            "Assets/CCS/Survival/Profiles/WorldSimulation/CCS_DefaultWorldSimulationProfile.asset";
        private const string DefaultPlaytestProfilePath =
            "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset";
        private const string SaveDataPath = "Assets/CCS/Modules/SaveSystem/Runtime/Data/CCS_SaveData.cs";

        public string ValidatorId => ValidatorContext;

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateRequiredScripts(report);
            ValidateRegionSpecializations(report);
            ValidateWorldSimulationRegionalData(report);
            ValidateContractRegionalCategories(report);
            ValidateSaveSupport(report);
            ValidatePlaytestSteps(report);

            CCS_SurvivalBootstrapVersionUtility.AddBundleVersionValidationIssue(
                report,
                ValidatorContext,
                RegionalEconomyMilestoneVersion,
                "Run CCS_RegionSpecializationFoundationBootstrapSetup.ExecuteBatch.");
            CCS_SurvivalInputValidationUtility.ValidateNoLegacyInputUsage(report, "Regional Specialization Input Policy");
            CCS_SurvivalInputValidationUtility.ValidateNoFindObjectsSortModeUsage(report, "Regional Specialization Obsolete API Scan");
        }

        private static void ValidateRequiredScripts(CCS_SurvivalValidationReport report)
        {
            ValidateRequiredScript(
                report,
                "CCS_RegionSpecializationType",
                "Assets/CCS/Modules/Regions/Runtime/Data/CCS_RegionSpecializationType.cs");
            ValidateRequiredScript(
                report,
                "CCS_RegionProductionModifier",
                "Assets/CCS/Modules/Regions/Runtime/Data/CCS_RegionProductionModifier.cs");
            ValidateRequiredScript(
                report,
                "CCS_RegionEconomyUtility",
                "Assets/CCS/Modules/Regions/Runtime/Validation/CCS_RegionEconomyUtility.cs");
            ValidateRequiredScript(
                report,
                "CCS_RegionSpecializationFoundationBootstrapSetup",
                "Assets/CCS/Modules/Regions/Editor/Validation/CCS_RegionSpecializationFoundationBootstrapSetup.cs");
        }

        private static void ValidateRegionSpecializations(CCS_SurvivalValidationReport report)
        {
            CCS_RegionProfile profile = AssetDatabase.LoadAssetAtPath<CCS_RegionProfile>(DefaultRegionProfilePath);
            if (profile == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    ValidatorContext,
                    $"Missing default region profile: {DefaultRegionProfilePath}");
                return;
            }

            CCS_SurvivalValidationResult profileResult = CCS_RegionValidationUtility.ValidateProfile(profile);
            report.AddIssue(
                profileResult.IsSuccess ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                profileResult.Message);

            ValidateRegionSpecialization(
                report,
                ContentPath("CCS_Region_PineRidgeForest.asset"),
                CCS_RegionSpecializationType.Timber);
            ValidateRegionSpecialization(
                report,
                ContentPath("CCS_Region_BrokenCreek.asset"),
                CCS_RegionSpecializationType.Agriculture);
            ValidateRegionSpecialization(
                report,
                ContentPath("CCS_Region_IronRidgeMine.asset"),
                CCS_RegionSpecializationType.Mining);
            ValidateRegionSpecialization(
                report,
                ContentPath("CCS_Region_FrontierTradingPost.asset"),
                CCS_RegionSpecializationType.FrontierMixed);
        }

        private static void ValidateRegionSpecialization(
            CCS_SurvivalValidationReport report,
            string assetPath,
            CCS_RegionSpecializationType expected)
        {
            CCS_RegionDefinition definition = AssetDatabase.LoadAssetAtPath<CCS_RegionDefinition>(assetPath);
            if (definition == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    ValidatorContext,
                    $"Missing region definition: {assetPath}");
                return;
            }

            if (definition.SpecializationType != expected)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    ValidatorContext,
                    $"Region '{definition.RegionId}' specialization expected {expected}, found {definition.SpecializationType}.");
            }

            CCS_RegionProductionModifier modifier = definition.ProductionModifier;
            if (modifier == null
                || modifier.ProductionBonus <= 0f
                || modifier.ProsperityModifier <= 0f)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    ValidatorContext,
                    $"Region '{definition.RegionId}' has invalid prosperity/production modifiers.");
            }
        }

        private static void ValidateWorldSimulationRegionalData(CCS_SurvivalValidationReport report)
        {
            CCS_WorldSimulationProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_WorldSimulationProfile>(WorldSimulationProfilePath);
            if (profile == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    ValidatorContext,
                    $"Missing world simulation profile: {WorldSimulationProfilePath}");
                return;
            }

            CCS_WorldSimulationSettlementProfileEntry[] settlementEntries = profile.SettlementEntries;
            if (settlementEntries.Length == 0
                || string.IsNullOrWhiteSpace(settlementEntries[0].regionId))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    ValidatorContext,
                    "Trading post settlement entry is missing regionId.");
            }

            CCS_WorldSimulationRegionProfileEntry[] regionEntries = profile.RegionEntries;
            for (int index = 0; index < regionEntries.Length; index++)
            {
                CCS_WorldSimulationRegionProfileEntry entry = regionEntries[index];
                if (entry == null || entry.specializationType <= 0)
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        ValidatorContext,
                        $"World simulation region entry at index {index} is missing specializationType.");
                }

                if (entry != null && (entry.productionBonus <= 0f || entry.prosperityModifier <= 0f))
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        ValidatorContext,
                        $"World simulation region '{entry.regionId}' has invalid modifiers.");
                }
            }
        }

        private static void ValidateContractRegionalCategories(CCS_SurvivalValidationReport report)
        {
            ValidateContractCategory(report, CCS_ContractContentIds.CornDeliveryContractPath, CCS_RegionSpecializationType.Agriculture);
            ValidateContractCategory(report, CCS_ContractContentIds.MilkDeliveryContractPath, CCS_RegionSpecializationType.Ranching);
            ValidateContractCategory(report, CCS_ContractContentIds.IronOreDeliveryContractPath, CCS_RegionSpecializationType.Mining);
            ValidateContractCategory(report, CCS_ContractContentIds.LumberDeliveryContractPath, CCS_RegionSpecializationType.Timber);
        }

        private static void ValidateContractCategory(
            CCS_SurvivalValidationReport report,
            string assetPath,
            CCS_RegionSpecializationType expected)
        {
            CCS_ContractDefinition definition = AssetDatabase.LoadAssetAtPath<CCS_ContractDefinition>(assetPath);
            if (definition == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    ValidatorContext,
                    $"Missing contract definition: {assetPath}");
                return;
            }

            if (definition.RegionSpecialization != expected)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    ValidatorContext,
                    $"Contract '{definition.ContractId}' expected category {expected}, found {definition.RegionSpecialization}.");
            }
        }

        private static void ValidateSaveSupport(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(SaveDataPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    ValidatorContext,
                    $"Missing save data source: {SaveDataPath}");
                return;
            }

            string saveDataSource = File.ReadAllText(SaveDataPath);
            if (!saveDataSource.Contains("specializationType")
                || !saveDataSource.Contains("dominantIndustry")
                || !saveDataSource.Contains("foodSupplyStrength"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    ValidatorContext,
                    "CCS_SaveRegionDiscoveryData is missing regional economy fields.");
            }
        }

        private static void ValidatePlaytestSteps(CCS_SurvivalValidationReport report)
        {
            CCS_PlaytestProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(DefaultPlaytestProfilePath);
            if (profile == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    ValidatorContext,
                    $"Missing playtest profile: {DefaultPlaytestProfilePath}");
                return;
            }

            IReadOnlyList<CCS_PlaytestStepDefinition> steps = profile.StepDefinitions;
            ValidatePlaytestStepExists(report, steps, CCS_PlaytestStepType.DiscoverRegionsForRegionalEconomy);
            ValidatePlaytestStepExists(report, steps, CCS_PlaytestStepType.VerifyRegionSpecialization);
            ValidatePlaytestStepExists(report, steps, CCS_PlaytestStepType.AcceptRegionalSpecialtyContract);
            ValidatePlaytestStepExists(report, steps, CCS_PlaytestStepType.CompleteRegionalSpecialtyContract);
            ValidatePlaytestStepExists(report, steps, CCS_PlaytestStepType.VerifyRegionalProsperityIncrease);
            ValidatePlaytestStepExists(report, steps, CCS_PlaytestStepType.SaveRegionalEconomyState);
            ValidatePlaytestStepExists(report, steps, CCS_PlaytestStepType.VerifyRegionalEconomyAfterLoad);
        }

        private static void ValidatePlaytestStepExists(
            CCS_SurvivalValidationReport report,
            IReadOnlyList<CCS_PlaytestStepDefinition> steps,
            CCS_PlaytestStepType stepType)
        {
            for (int index = 0; index < steps.Count; index++)
            {
                if (steps[index] != null && steps[index].StepType == stepType)
                {
                    return;
                }
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                $"Missing playtest step type {stepType}. Run regional specialization bootstrap setup.");
        }

        private static void ValidateRequiredScript(
            CCS_SurvivalValidationReport report,
            string label,
            string assetPath)
        {
            if (File.Exists(assetPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    ValidatorContext,
                    $"{label} present.");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                $"Missing required script: {assetPath}");
        }

        private static string ContentPath(string fileName)
        {
            return "Assets/CCS/Survival/Content/Regions/" + fileName;
        }
    }
}
