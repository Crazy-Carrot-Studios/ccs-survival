using CCS.Modules.Contracts;
using CCS.Modules.Playtesting;
using CCS.Survival;
using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_TradeRoutesRiskFoundationValidationValidator
// CATEGORY: Modules / Settlements / Editor / Validation
// PURPOSE: Validates route risk metadata, freight reward scaling, and playtest wiring.
// PLACEMENT: Registered on CCS_SurvivalValidationPipeline at editor load.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.5.0 route risk and freight bonus foundation.
// =============================================================================

namespace CCS.Modules.Settlements.Editor
{
    public sealed class CCS_TradeRoutesRiskFoundationValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string ValidatorContext = "ccs.survival.validation.traderoutes.risk";
        private const string MilestoneVersion = "3.5.0";

        public string ValidatorId => ValidatorContext;

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateScripts(report);
            ValidateTradeRouteProfile(report);
            ValidateRewardUtility(report);
            ValidateContractCompletionResult(report);
            ValidatePlaytestSteps(report);

            CCS_SurvivalBootstrapVersionUtility.AddBundleVersionValidationIssue(
                report,
                ValidatorContext,
                MilestoneVersion,
                "Run CCS_TradeRoutesRiskFoundationBootstrapSetup.ExecuteBatch.");
            CCS_SurvivalInputValidationUtility.ValidateNoLegacyInputUsage(report, "Route Risk Freight Input Policy");
            CCS_SurvivalInputValidationUtility.ValidateNoFindObjectsSortModeUsage(report, "Route Risk Freight Obsolete API Scan");
        }

        private static void ValidateScripts(CCS_SurvivalValidationReport report)
        {
            ValidateScriptExists(report, "Assets/CCS/Modules/Settlements/Runtime/TradeRoutes/CCS_TradeRouteRiskLevel.cs");
            ValidateScriptExists(report, "Assets/CCS/Modules/Settlements/Runtime/Validation/CCS_TradeRouteRewardModifierUtility.cs");
            ValidateScriptExists(report, "Assets/CCS/Modules/Settlements/Editor/Validation/CCS_TradeRoutesRiskFoundationBootstrapSetup.cs");
        }

        private static void ValidateTradeRouteProfile(CCS_SurvivalValidationReport report)
        {
            CCS_TradeRouteProfile profile = AssetDatabase.LoadAssetAtPath<CCS_TradeRouteProfile>(
                CCS_MultiSettlementContentIds.TradeRoutesProfilePath);
            CCS_SurvivalValidationResult validation = CCS_TradeRouteUtility.ValidateProfile(profile);
            report.AddIssue(
                validation.IsSuccess ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                validation.Message);
        }

        private static void ValidateRewardUtility(CCS_SurvivalValidationReport report)
        {
            CCS_TradeRouteDefinition sample = AssetDatabase.LoadAssetAtPath<CCS_TradeRouteDefinition>(
                CCS_MultiSettlementContentIds.TradeRoutesContentRoot + "/CCS_TradeRoute_IronRidge_TradingPost.asset");
            CCS_TradeRouteFreightRewardBreakdown breakdown =
                CCS_TradeRouteRewardModifierUtility.CalculateFreightTradeDollars(24, sample);
            bool valid = breakdown.FinalTradeDollars >= breakdown.BaseTradeDollars
                && breakdown.FinalTradeDollars >= 0;
            report.AddIssue(
                valid ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                valid
                    ? $"Freight reward scaling sample final={breakdown.FinalTradeDollars} (base {breakdown.BaseTradeDollars})."
                    : "Freight reward scaling produced invalid final reward.");
        }

        private static void ValidateContractCompletionResult(CCS_SurvivalValidationReport report)
        {
            string source = System.IO.File.ReadAllText(
                "Assets/CCS/Modules/Contracts/Runtime/Data/CCS_ContractCompletionResult.cs");
            bool hasFields = source.Contains("BaseTradeDollarsReward", System.StringComparison.Ordinal)
                && source.Contains("RouteRewardMultiplier", System.StringComparison.Ordinal)
                && source.Contains("RiskRewardMultiplier", System.StringComparison.Ordinal)
                && source.Contains("LinkedTradeRouteId", System.StringComparison.Ordinal);
            report.AddIssue(
                hasFields ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                hasFields
                    ? "Contract completion result exposes freight route reward breakdown fields."
                    : "Contract completion result is missing freight reward breakdown fields.");

            string hudSource = System.IO.File.ReadAllText("Assets/CCS/Modules/Contracts/Runtime/UI/CCS_ContractDebugHud.cs");
            bool hudShowsRoute = hudSource.Contains("DrawFreightRouteRewardInfo", System.StringComparison.Ordinal);
            report.AddIssue(
                hudShowsRoute ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                hudShowsRoute
                    ? "Contract debug HUD shows route risk and reward preview."
                    : "Contract debug HUD is missing route risk reward preview.");
        }

        private static void ValidatePlaytestSteps(CCS_SurvivalValidationReport report)
        {
            CCS_PlaytestProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(
                "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset");
            if (profile == null)
            {
                return;
            }

            ValidateStep(report, profile, CCS_PlaytestStepType.VerifyModerateRiskFreightHigherReward);
            ValidateStep(report, profile, CCS_PlaytestStepType.VerifyRouteRiskFreightStateAfterLoad);

            string hudSource = System.IO.File.ReadAllText("Assets/CCS/Modules/Playtesting/Runtime/UI/CCS_PlaytestHud.cs");
            bool hasShortcut = hudSource.Contains("TryPlaytestRouteRiskFreightShortcut", System.StringComparison.Ordinal);
            report.AddIssue(
                hasShortcut ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                hasShortcut
                    ? "Playtest HUD wires Ctrl+Shift+Q route risk freight shortcut."
                    : "Playtest HUD is missing route risk freight shortcut.");
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
                    : $"Playtest profile missing step {stepType}. Run risk bootstrap.");
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
