using CCS.Modules.Playtesting;
using CCS.Modules.Regions;
using CCS.Modules.WorldSimulation;
using CCS.Survival;
using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_SettlementPopulationFoundationValidationValidator
// CATEGORY: Modules / Settlements / Editor / Validation
// PURPOSE: Validates population simulation, growth thresholds, save fields, and playtest wiring.
// PLACEMENT: Registered on CCS_SurvivalValidationPipeline at editor load.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.6.0 population foundation.
// =============================================================================

namespace CCS.Modules.Settlements.Editor
{
    public sealed class CCS_SettlementPopulationFoundationValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string ValidatorContext = "ccs.survival.validation.settlementpopulation";
        private const string MilestoneVersion = "3.6.0";

        public string ValidatorId => ValidatorContext;

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateScripts(report);
            ValidatePopulationProfile(report);
            ValidateGrowthPopulationThresholds(report);
            ValidateSimulationStateFields(report);
            ValidatePopulationUtility(report);
            ValidatePlaytest(report);

            CCS_SurvivalBootstrapVersionUtility.AddBundleVersionValidationIssue(
                report,
                ValidatorContext,
                MilestoneVersion,
                "Run CCS_SettlementPopulationFoundationBootstrapSetup.ExecuteBatch.");
            CCS_SurvivalInputValidationUtility.ValidateNoLegacyInputUsage(report, "Population Foundation Input Policy");
            CCS_SurvivalInputValidationUtility.ValidateNoFindObjectsSortModeUsage(report, "Population Foundation Obsolete API Scan");
        }

        private static void ValidateScripts(CCS_SurvivalValidationReport report)
        {
            ValidateScriptExists(report, "Assets/CCS/Modules/Settlements/Runtime/Population/CCS_SettlementPopulationProfile.cs");
            ValidateScriptExists(report, "Assets/CCS/Modules/Settlements/Runtime/Population/CCS_SettlementPopulationState.cs");
            ValidateScriptExists(report, "Assets/CCS/Modules/Settlements/Runtime/Population/CCS_SettlementPopulationSnapshot.cs");
            ValidateScriptExists(report, "Assets/CCS/Modules/WorldSimulation/Runtime/Validation/CCS_SettlementPopulationUtility.cs");
            ValidateScriptExists(report, "Assets/CCS/Modules/Settlements/Editor/Validation/CCS_SettlementPopulationFoundationBootstrapSetup.cs");
        }

        private static void ValidatePopulationProfile(CCS_SurvivalValidationReport report)
        {
            CCS_SettlementPopulationProfile profile = AssetDatabase.LoadAssetAtPath<CCS_SettlementPopulationProfile>(
                CCS_SettlementPopulationContentIds.DefaultPopulationProfilePath);
            CCS_SurvivalValidationResult validation = CCS_SettlementPopulationUtility.ValidateProfile(profile);
            report.AddIssue(
                validation.IsSuccess ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                validation.Message);

            CCS_WorldSimulationProfile worldProfile = AssetDatabase.LoadAssetAtPath<CCS_WorldSimulationProfile>(
                "Assets/CCS/Survival/Profiles/WorldSimulation/CCS_DefaultWorldSimulationProfile.asset");
            bool wired = worldProfile != null && worldProfile.SettlementPopulationProfile == profile;
            report.AddIssue(
                wired ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                wired
                    ? "World simulation profile references settlement population profile."
                    : "World simulation profile missing settlement population profile reference.");
        }

        private static void ValidateGrowthPopulationThresholds(CCS_SurvivalValidationReport report)
        {
            CCS_SettlementGrowthDefinition outpost = AssetDatabase.LoadAssetAtPath<CCS_SettlementGrowthDefinition>(
                CCS_SettlementGrowthContentIds.OutpostGrowthDefinitionPath);
            CCS_SettlementGrowthDefinition tradingPost = AssetDatabase.LoadAssetAtPath<CCS_SettlementGrowthDefinition>(
                CCS_SettlementGrowthContentIds.TradingPostGrowthDefinitionPath);
            bool valid = outpost != null
                && tradingPost != null
                && outpost.MinimumPopulation == 0
                && tradingPost.MinimumPopulation >= 50;
            report.AddIssue(
                valid ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                valid
                    ? "Growth definitions include Outpost 0+ and Trading Post 50+ population gates."
                    : "Growth definitions missing population thresholds. Run population bootstrap.");
        }

        private static void ValidateSimulationStateFields(CCS_SurvivalValidationReport report)
        {
            string source = System.IO.File.ReadAllText(
                "Assets/CCS/Modules/WorldSimulation/Runtime/Data/CCS_SettlementSimulationState.cs");
            bool hasFields = source.Contains("populationCapacity", System.StringComparison.Ordinal)
                && source.Contains("populationStability", System.StringComparison.Ordinal)
                && source.Contains("farmerCount", System.StringComparison.Ordinal);
            report.AddIssue(
                hasFields ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                hasFields
                    ? "Settlement simulation state persists population metrics for save/load."
                    : "Settlement simulation state missing population persistence fields.");
        }

        private static void ValidatePopulationUtility(CCS_SurvivalValidationReport report)
        {
            CCS_SettlementSimulationState sample = new CCS_SettlementSimulationState
            {
                settlementId = CCS_SettlementGrowthContentIds.TradingPostSettlementId,
                population = 10,
                prosperity = 40f
            };
            CCS_SettlementPopulationUtility.ApplyPopulationGrowth(
                sample,
                3f,
                AssetDatabase.LoadAssetAtPath<CCS_SettlementPopulationProfile>(
                    CCS_SettlementPopulationContentIds.DefaultPopulationProfilePath),
                CCS_RegionSpecializationType.FrontierMixed);
            bool valid = sample.population >= 10;
            report.AddIssue(
                valid ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                valid
                    ? $"Population growth sample final={sample.population}."
                    : "Population growth utility produced invalid population.");
        }

        private static void ValidatePlaytest(CCS_SurvivalValidationReport report)
        {
            string hudSource = System.IO.File.ReadAllText("Assets/CCS/Modules/Playtesting/Runtime/UI/CCS_PlaytestHud.cs");
            bool hasShortcut = hudSource.Contains("TryPlaytestPopulationFoundationShortcut", System.StringComparison.Ordinal);
            report.AddIssue(
                hasShortcut ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                hasShortcut
                    ? "Playtest HUD wires Ctrl+Shift+K population foundation shortcut."
                    : "Playtest HUD missing population foundation shortcut.");

            CCS_PlaytestProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(
                "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset");
            if (profile == null)
            {
                return;
            }

            ValidateStep(report, profile, CCS_PlaytestStepType.VerifyPopulationAfterLoad);
        }

        private static void ValidateStep(
            CCS_SurvivalValidationReport report,
            CCS_PlaytestProfile profile,
            CCS_PlaytestStepType stepType)
        {
            bool found = false;
            for (int index = 0; index < profile.StepDefinitions.Count; index++)
            {
                if (profile.StepDefinitions[index]?.StepType == stepType)
                {
                    found = true;
                    break;
                }
            }

            report.AddIssue(
                found ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                found
                    ? $"Playtest profile includes step {stepType}."
                    : $"Playtest profile missing step {stepType}. Run population bootstrap.");
        }

        private static void ValidateScriptExists(CCS_SurvivalValidationReport report, string scriptPath)
        {
            bool exists = System.IO.File.Exists(scriptPath);
            report.AddIssue(
                exists ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                exists ? $"Script present: {scriptPath}" : $"Missing script: {scriptPath}");
        }
    }
}
