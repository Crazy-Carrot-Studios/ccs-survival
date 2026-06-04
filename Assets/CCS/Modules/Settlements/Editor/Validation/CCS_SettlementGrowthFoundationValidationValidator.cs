using System.Collections.Generic;
using System.IO;
using CCS.Modules.Playtesting;
using CCS.Modules.WorldSimulation;
using CCS.Survival;
using CCS.Survival.Composition;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SettlementGrowthFoundationValidationValidator
// CATEGORY: Modules / Settlements / Editor / Validation
// PURPOSE: Validates settlement growth layout, content, world simulation, save, and playtest wiring.
// PLACEMENT: Registered on CCS_SurvivalValidationPipeline at editor load.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.2.0 settlement growth foundation.
// =============================================================================

namespace CCS.Modules.Settlements.Editor
{
    public sealed class CCS_SettlementGrowthFoundationValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string ValidatorContext = "ccs.survival.validation.settlement.growth";
        private const string SettlementGrowthMilestoneVersion = "3.2.0";
        private const string ModuleRoot = "Assets/CCS/Modules/Settlements";
        private const string RuntimeRoot = ModuleRoot + "/Runtime";
        private const string SettlementSimulationStatePath =
            "Assets/CCS/Modules/WorldSimulation/Runtime/Data/CCS_SettlementSimulationState.cs";
        private const string WorldSimulationProfilePath =
            "Assets/CCS/Survival/Profiles/WorldSimulation/CCS_DefaultWorldSimulationProfile.asset";
        private const string DefaultPlaytestProfilePath =
            "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset";
        private const string CompositionRegistrationPath =
            "Assets/CCS/Survival/Runtime/Composition/CCS_SurvivalGameplayServiceRegistration.cs";
        public string ValidatorId => ValidatorContext;

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateRequiredScripts(report);
            ValidateGrowthProfile(report);
            ValidateWorldSimulationGrowthWiring(report);
            ValidateSettlementSimulationGrowthFields(report);
            ValidateCompositionWiring(report);
            ValidatePlaytestSteps(report);

            CCS_SurvivalBootstrapVersionUtility.AddBundleVersionValidationIssue(
                report,
                ValidatorContext,
                SettlementGrowthMilestoneVersion,
                "Run CCS_SettlementGrowthFoundationBootstrapSetup.ExecuteBatch.");
            CCS_SurvivalInputValidationUtility.ValidateNoLegacyInputUsage(report, "Settlement Growth Input Policy");
            CCS_SurvivalInputValidationUtility.ValidateNoFindObjectsSortModeUsage(report, "Settlement Growth Obsolete API Scan");
        }

        private static void ValidateRequiredScripts(CCS_SurvivalValidationReport report)
        {
            ValidateRequiredScript(report, "CCS_SettlementGrowthStage", RuntimeRoot + "/Growth/CCS_SettlementGrowthStage.cs");
            ValidateRequiredScript(report, "CCS_SettlementGrowthDefinition", RuntimeRoot + "/Growth/CCS_SettlementGrowthDefinition.cs");
            ValidateRequiredScript(report, "CCS_SettlementGrowthProfile", RuntimeRoot + "/Profiles/CCS_SettlementGrowthProfile.cs");
            ValidateRequiredScript(report, "CCS_SettlementGrowthUtility", RuntimeRoot + "/Validation/CCS_SettlementGrowthUtility.cs");
            ValidateRequiredScript(report, "CCS_SettlementGrowthDebugHud", RuntimeRoot + "/UI/CCS_SettlementGrowthDebugHud.cs");
            ValidateRequiredScript(
                report,
                "CCS_SettlementGrowthFoundationBootstrapSetup",
                ModuleRoot + "/Editor/Validation/CCS_SettlementGrowthFoundationBootstrapSetup.cs");
        }

        private static void ValidateGrowthProfile(CCS_SurvivalValidationReport report)
        {
            CCS_SettlementGrowthProfile profile = AssetDatabase.LoadAssetAtPath<CCS_SettlementGrowthProfile>(
                CCS_SettlementGrowthContentIds.DefaultGrowthProfilePath);
            CCS_SurvivalValidationResult result = CCS_SettlementGrowthUtility.ValidateProfile(profile);
            report.AddIssue(
                result.IsSuccess ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                result.Message);

            if (profile != null
                && profile.TryGetDefinition(CCS_SettlementGrowthStage.TradingPost, out CCS_SettlementGrowthDefinition tradingPost)
                && tradingPost != null
                && tradingPost.IsActive
                && tradingPost.MinimumProsperity < 35f)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    ValidatorContext,
                    "TradingPost growth definition prosperity threshold must be at least 35.");
            }
        }

        private static void ValidateWorldSimulationGrowthWiring(CCS_SurvivalValidationReport report)
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

            if (profile.SettlementGrowthProfile == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    ValidatorContext,
                    "World simulation profile is missing settlementGrowthProfile reference.");
            }
        }

        private static void ValidateSettlementSimulationGrowthFields(CCS_SurvivalValidationReport report)
        {
            string statePath = SettlementSimulationStatePath;
            if (!File.Exists(statePath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    ValidatorContext,
                    $"Missing settlement simulation state: {statePath}");
                return;
            }

            string stateSource = File.ReadAllText(statePath);
            if (!stateSource.Contains("currentGrowthStage")
                || !stateSource.Contains("previousGrowthStage")
                || !stateSource.Contains("growthProgressPercent")
                || !stateSource.Contains("completedContractsCount"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    ValidatorContext,
                    "CCS_SettlementSimulationState is missing growth fields.");
            }
        }

        private static void ValidateCompositionWiring(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(CompositionRegistrationPath))
            {
                return;
            }

            string source = File.ReadAllText(CompositionRegistrationPath);
            if (!source.Contains("WireSettlementGrowth"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    ValidatorContext,
                    "Composition registration is missing WireSettlementGrowth.");
            }
        }

        private static void ValidatePlaytestSteps(CCS_SurvivalValidationReport report)
        {
            CCS_PlaytestProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(DefaultPlaytestProfilePath);
            if (profile == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    ValidatorContext,
                    $"Missing playtest profile: {DefaultPlaytestProfilePath}");
                return;
            }

            IReadOnlyList<CCS_PlaytestStepDefinition> steps = profile.StepDefinitions;
            ValidatePlaytestStepExists(report, steps, CCS_PlaytestStepType.DiscoverTradingPostForSettlementGrowth);
            ValidatePlaytestStepExists(report, steps, CCS_PlaytestStepType.CompleteContractForSettlementGrowth);
            ValidatePlaytestStepExists(report, steps, CCS_PlaytestStepType.VerifySettlementGrowthSupplyProsperity);
            ValidatePlaytestStepExists(report, steps, CCS_PlaytestStepType.VerifySettlementGrowthProgress);
            ValidatePlaytestStepExists(report, steps, CCS_PlaytestStepType.ReachTradingPostGrowthStage);
            ValidatePlaytestStepExists(report, steps, CCS_PlaytestStepType.VerifySettlementGrowthStageChanged);
            ValidatePlaytestStepExists(report, steps, CCS_PlaytestStepType.SaveSettlementGrowthState);
            ValidatePlaytestStepExists(report, steps, CCS_PlaytestStepType.VerifySettlementGrowthAfterLoad);
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
                $"Missing playtest step type {stepType}. Run settlement growth bootstrap setup.");
        }

        private static void ValidateRequiredScript(CCS_SurvivalValidationReport report, string label, string assetPath)
        {
            if (File.Exists(assetPath))
            {
                report.AddIssue(CCS_SurvivalValidationIssueSeverity.Info, ValidatorContext, $"{label} present.");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                $"Missing required script: {assetPath}");
        }
    }
}
