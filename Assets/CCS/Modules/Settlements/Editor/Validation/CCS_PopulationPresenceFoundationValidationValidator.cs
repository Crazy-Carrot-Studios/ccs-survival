using CCS.Modules.Playtesting;
using CCS.Modules.WorldSimulation;
using CCS.Survival;
using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_PopulationPresenceFoundationValidationValidator
// CATEGORY: Modules / Settlements / Editor / Validation
// PURPOSE: Validates population presence anchors, profiles, playtest, and wiring.
// PLACEMENT: Registered on CCS_SurvivalValidationPipeline at editor load.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 4.0.0 NPC population placeholder foundation.
// =============================================================================

namespace CCS.Modules.Settlements.Editor
{
    public sealed class CCS_PopulationPresenceFoundationValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string ValidatorContext = "ccs.survival.validation.populationpresence";
        private const string MilestoneVersion = "4.0.0";
        private const string BootstrapScenePath = "Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string RegistrationPath =
            "Assets/CCS/Survival/Runtime/Composition/CCS_SurvivalGameplayServiceRegistration.cs";

        public string ValidatorId => ValidatorContext;

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateScripts(report);
            ValidatePresenceProfile(report);
            ValidateWorldProfileWiring(report);
            ValidateBootstrapSceneAnchors(report);
            ValidateUtility(report);
            ValidateRegistration(report);
            ValidatePlaytest(report);

            CCS_SurvivalBootstrapVersionUtility.AddBundleVersionValidationIssue(
                report,
                ValidatorContext,
                MilestoneVersion,
                "Run CCS_PopulationPresenceFoundationBootstrapSetup.ExecuteBatch.");
            CCS_SurvivalInputValidationUtility.ValidateNoLegacyInputUsage(report, "Population Presence Input Policy");
            CCS_SurvivalInputValidationUtility.ValidateNoFindObjectsSortModeUsage(
                report,
                "Population Presence Obsolete API Scan");
        }

        private static void ValidateScripts(CCS_SurvivalValidationReport report)
        {
            ValidateScriptExists(
                report,
                "Assets/CCS/Modules/Settlements/Runtime/PopulationPresence/CCS_PopulationPresenceAnchor.cs");
            ValidateScriptExists(
                report,
                "Assets/CCS/Modules/Settlements/Runtime/Services/CCS_PopulationPresenceService.cs");
            ValidateScriptExists(
                report,
                "Assets/CCS/Modules/Settlements/Runtime/Validation/CCS_PopulationPresenceValidationUtility.cs");
            ValidateScriptExists(
                report,
                "Assets/CCS/Modules/Settlements/Editor/Validation/CCS_PopulationPresenceFoundationBootstrapSetup.cs");
        }

        private static void ValidatePresenceProfile(CCS_SurvivalValidationReport report)
        {
            CCS_PopulationPresenceProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PopulationPresenceProfile>(
                CCS_PopulationPresenceContentIds.DefaultPresenceProfilePath);
            CCS_SurvivalValidationResult validation =
                CCS_PopulationPresenceValidationUtility.ValidateProfile(profile);
            report.AddIssue(
                validation.IsSuccess ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                validation.Message);

            bool hasEightAnchors = profile != null && profile.AnchorDefinitions.Length >= 8;
            report.AddIssue(
                hasEightAnchors ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                hasEightAnchors
                    ? "Population presence profile defines anchors for all milestone settlements."
                    : "Population presence profile missing settlement workforce anchors.");
        }

        private static void ValidateWorldProfileWiring(CCS_SurvivalValidationReport report)
        {
            CCS_WorldSimulationProfile worldProfile = AssetDatabase.LoadAssetAtPath<CCS_WorldSimulationProfile>(
                "Assets/CCS/Survival/Profiles/WorldSimulation/CCS_DefaultWorldSimulationProfile.asset");
            CCS_PopulationPresenceProfile presenceProfile = AssetDatabase.LoadAssetAtPath<CCS_PopulationPresenceProfile>(
                CCS_PopulationPresenceContentIds.DefaultPresenceProfilePath);
            bool wired = worldProfile != null
                && worldProfile.SettlementPopulationPresenceProfile == presenceProfile;
            report.AddIssue(
                wired ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                wired
                    ? "World simulation profile references population presence profile."
                    : "World simulation profile missing population presence profile reference.");
        }

        private static void ValidateBootstrapSceneAnchors(CCS_SurvivalValidationReport report)
        {
            string sceneSource = System.IO.File.Exists(BootstrapScenePath)
                ? System.IO.File.ReadAllText(BootstrapScenePath)
                : string.Empty;
            bool hasAnchors = sceneSource.Contains("CCS_PopulationPresenceAnchor", System.StringComparison.Ordinal)
                && sceneSource.Contains(CCS_PopulationPresenceContentIds.TradingPostMerchantsAnchorId, System.StringComparison.Ordinal)
                && sceneSource.Contains(CCS_PopulationPresenceContentIds.BrokenCreekFarmersAnchorId, System.StringComparison.Ordinal);
            report.AddIssue(
                hasAnchors ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                hasAnchors
                    ? "Bootstrap scene includes population presence anchors."
                    : "Bootstrap scene missing population presence anchors. Run population presence bootstrap.");
        }

        private static void ValidateUtility(CCS_SurvivalValidationReport report)
        {
            CCS_SettlementPopulationSnapshot snapshot = new CCS_SettlementPopulationSnapshot
            {
                SettlementId = CCS_SettlementGrowthContentIds.TradingPostSettlementId,
                MerchantCount = 3
            };

            int visible = CCS_PopulationPresenceValidationUtility.ResolveVisibleActorCount(
                snapshot,
                CCS_SettlementPopulationCategory.Merchants,
                1,
                4,
                true,
                true);
            bool ok = visible == 3;
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                ok
                    ? "Population presence utility caps visible actors from workforce counts."
                    : "Population presence utility failed visible actor count smoke test.");
        }

        private static void ValidateRegistration(CCS_SurvivalValidationReport report)
        {
            string source = System.IO.File.ReadAllText(RegistrationPath);
            bool wired = source.Contains("CreatePopulationPresenceService", System.StringComparison.Ordinal)
                && source.Contains("WirePopulationPresence", System.StringComparison.Ordinal);
            report.AddIssue(
                wired ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                wired
                    ? "Gameplay registration wires population presence service and refresh hooks."
                    : "Gameplay registration missing population presence wiring.");
        }

        private static void ValidatePlaytest(CCS_SurvivalValidationReport report)
        {
            string hudSource = System.IO.File.ReadAllText(
                "Assets/CCS/Modules/Playtesting/Runtime/UI/CCS_PlaytestHud.cs");
            bool shortcut = hudSource.Contains("TryPlaytestPopulationPresenceFoundationShortcut", System.StringComparison.Ordinal)
                && hudSource.Contains("WasControlShiftPressed(KeyCode.X)", System.StringComparison.Ordinal);
            report.AddIssue(
                shortcut ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                shortcut
                    ? "Playtest HUD exposes Ctrl+Shift+X population presence shortcut."
                    : "Playtest HUD missing Ctrl+Shift+X population presence shortcut.");

            CCS_PlaytestProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(
                "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset");
            if (profile == null)
            {
                return;
            }

            ValidateStep(report, profile, CCS_PlaytestStepType.VerifyPopulationPlaceholderActorsVisible);
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
                    : $"Playtest profile missing step {stepType}. Run population presence bootstrap.");
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
