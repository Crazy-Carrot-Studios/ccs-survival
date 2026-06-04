using CCS.Modules.Playtesting;
using CCS.Modules.Reputation;
using CCS.Modules.WorldSimulation;
using CCS.Survival;
using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_FrontierBusinessesFoundationValidationValidator
// CATEGORY: Modules / Settlements / Editor / Validation
// PURPOSE: Validates business simulation, settlement ownership, save fields, and playtest wiring.
// PLACEMENT: Registered on CCS_SurvivalValidationPipeline at editor load.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.7.0 frontier businesses foundation.
// =============================================================================

namespace CCS.Modules.Settlements.Editor
{
    public sealed class CCS_FrontierBusinessesFoundationValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string ValidatorContext = "ccs.survival.validation.businesses";
        private const string MilestoneVersion = "3.7.0";
        private const string RegistrationPath =
            "Assets/CCS/Survival/Runtime/Composition/CCS_SurvivalGameplayServiceRegistration.cs";

        public string ValidatorId => ValidatorContext;

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateScripts(report);
            ValidateBusinessProfile(report);
            ValidateSimulationStateFields(report);
            ValidateBusinessUtility(report);
            ValidateRegistration(report);
            ValidatePlaytest(report);

            CCS_SurvivalBootstrapVersionUtility.AddBundleVersionValidationIssue(
                report,
                ValidatorContext,
                MilestoneVersion,
                "Run CCS_FrontierBusinessesFoundationBootstrapSetup.ExecuteBatch.");
            CCS_SurvivalInputValidationUtility.ValidateNoLegacyInputUsage(report, "Businesses Foundation Input Policy");
            CCS_SurvivalInputValidationUtility.ValidateNoFindObjectsSortModeUsage(report, "Businesses Foundation Obsolete API Scan");
        }

        private static void ValidateScripts(CCS_SurvivalValidationReport report)
        {
            ValidateScriptExists(report, "Assets/CCS/Modules/Settlements/Runtime/Businesses/CCS_BusinessProfile.cs");
            ValidateScriptExists(report, "Assets/CCS/Modules/Settlements/Runtime/Services/CCS_BusinessService.cs");
            ValidateScriptExists(report, "Assets/CCS/Modules/WorldSimulation/Runtime/Validation/CCS_BusinessValidationUtility.cs");
            ValidateScriptExists(report, "Assets/CCS/Modules/Settlements/Editor/Validation/CCS_FrontierBusinessesFoundationBootstrapSetup.cs");
        }

        private static void ValidateBusinessProfile(CCS_SurvivalValidationReport report)
        {
            CCS_BusinessProfile profile = AssetDatabase.LoadAssetAtPath<CCS_BusinessProfile>(
                CCS_BusinessContentIds.DefaultBusinessProfilePath);
            CCS_SurvivalValidationResult validation = CCS_BusinessValidationUtility.ValidateProfile(profile);
            report.AddIssue(
                validation.IsSuccess ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                validation.Message);

            bool tradingPostCatalog = profile != null
                && profile.TryGetSettlementCatalog(
                    CCS_SettlementGrowthContentIds.TradingPostSettlementId,
                    out CCS_BusinessSettlementCatalogEntry tradingPostEntry)
                && tradingPostEntry != null
                && tradingPostEntry.businessTypes != null
                && tradingPostEntry.businessTypes.Length >= 5;
            report.AddIssue(
                tradingPostCatalog ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                tradingPostCatalog
                    ? "Trading Post business catalog includes core frontier businesses."
                    : "Trading Post business catalog incomplete. Run businesses bootstrap.");

            CCS_WorldSimulationProfile worldProfile = AssetDatabase.LoadAssetAtPath<CCS_WorldSimulationProfile>(
                "Assets/CCS/Survival/Profiles/WorldSimulation/CCS_DefaultWorldSimulationProfile.asset");
            bool wired = worldProfile != null && worldProfile.SettlementBusinessProfile == profile;
            report.AddIssue(
                wired ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                wired
                    ? "World simulation profile references settlement business profile."
                    : "World simulation profile missing settlement business profile reference.");
        }

        private static void ValidateSimulationStateFields(CCS_SurvivalValidationReport report)
        {
            string source = System.IO.File.ReadAllText(
                "Assets/CCS/Modules/WorldSimulation/Runtime/Data/CCS_SettlementSimulationState.cs");
            bool hasFields = source.Contains("businessStates", System.StringComparison.Ordinal);
            report.AddIssue(
                hasFields ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                hasFields
                    ? "Settlement simulation state persists business activation for save/load."
                    : "Settlement simulation state missing businessStates field.");
        }

        private static void ValidateBusinessUtility(CCS_SurvivalValidationReport report)
        {
            CCS_BusinessProfile profile = AssetDatabase.LoadAssetAtPath<CCS_BusinessProfile>(
                CCS_BusinessContentIds.DefaultBusinessProfilePath);
            CCS_SettlementSimulationState sample = new CCS_SettlementSimulationState
            {
                settlementId = CCS_SettlementGrowthContentIds.TradingPostSettlementId,
                population = 25,
                prosperity = 35f,
                isDiscovered = true,
                currentGrowthStage = (int)CCS_SettlementGrowthStage.TradingPost
            };
            CCS_BusinessValidationUtility.InitializeBusinessState(sample, profile);
            CCS_BusinessValidationUtility.BusinessEvaluationChanges changes =
                CCS_BusinessValidationUtility.EvaluateSettlementBusinesses(
                    sample,
                    profile,
                    CCS_ReputationTier.Neutral);
            bool activated = changes.Activated.Count > 0;
            report.AddIssue(
                activated ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                activated
                    ? "Business utility activates catalog entries when thresholds are met."
                    : "Business utility failed activation smoke test.");
        }

        private static void ValidateRegistration(CCS_SurvivalValidationReport report)
        {
            string source = System.IO.File.ReadAllText(RegistrationPath);
            bool wired = source.Contains("CreateBusinessService", System.StringComparison.Ordinal)
                && source.Contains("WireSettlementBusinesses", System.StringComparison.Ordinal);
            report.AddIssue(
                wired ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                wired
                    ? "Gameplay registration wires business service and settlement bridges."
                    : "Gameplay registration missing business service wiring.");
        }

        private static void ValidatePlaytest(CCS_SurvivalValidationReport report)
        {
            string hudSource = System.IO.File.ReadAllText(
                "Assets/CCS/Modules/Playtesting/Runtime/UI/CCS_PlaytestHud.cs");
            bool shortcut = hudSource.Contains("TryPlaytestBusinessesFoundationShortcut", System.StringComparison.Ordinal)
                && hudSource.Contains("KeyCode.J", System.StringComparison.Ordinal);
            report.AddIssue(
                shortcut ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                shortcut
                    ? "Playtest HUD exposes Ctrl+Shift+J businesses foundation shortcut."
                    : "Playtest HUD missing Ctrl+Shift+J businesses shortcut.");

            CCS_PlaytestProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(
                "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset");
            if (profile == null)
            {
                return;
            }

            ValidateStep(report, profile, CCS_PlaytestStepType.VerifyBusinessActivated);
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
                    : $"Playtest profile missing step {stepType}. Run businesses bootstrap.");
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
