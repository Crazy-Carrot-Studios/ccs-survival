using CCS.Modules.Playtesting;
using CCS.Modules.WorldSimulation;
using CCS.Survival;
using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_SettlementVisualGrowthFoundationValidationValidator
// CATEGORY: Modules / Settlements / Editor / Validation
// PURPOSE: Validates settlement visual growth anchors, profiles, playtest, and wiring.
// PLACEMENT: Registered on CCS_SurvivalValidationPipeline at editor load.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.9.0 settlement visual growth foundation.
// =============================================================================

namespace CCS.Modules.Settlements.Editor
{
    public sealed class CCS_SettlementVisualGrowthFoundationValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string ValidatorContext = "ccs.survival.validation.settlementvisualgrowth";
        private const string MilestoneVersion = "3.9.0";
        private const string BootstrapScenePath = "Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string RegistrationPath =
            "Assets/CCS/Survival/Runtime/Composition/CCS_SurvivalGameplayServiceRegistration.cs";

        public string ValidatorId => ValidatorContext;

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateScripts(report);
            ValidateVisualGrowthProfile(report);
            ValidateWorldProfileWiring(report);
            ValidateBootstrapSceneAnchors(report);
            ValidateUtility(report);
            ValidateRegistration(report);
            ValidatePlaytest(report);

            CCS_SurvivalBootstrapVersionUtility.AddBundleVersionValidationIssue(
                report,
                ValidatorContext,
                MilestoneVersion,
                "Run CCS_SettlementVisualGrowthFoundationBootstrapSetup.ExecuteBatch.");
            CCS_SurvivalInputValidationUtility.ValidateNoLegacyInputUsage(report, "Settlement Visual Growth Input Policy");
            CCS_SurvivalInputValidationUtility.ValidateNoFindObjectsSortModeUsage(
                report,
                "Settlement Visual Growth Obsolete API Scan");
        }

        private static void ValidateScripts(CCS_SurvivalValidationReport report)
        {
            ValidateScriptExists(
                report,
                "Assets/CCS/Modules/Settlements/Runtime/VisualGrowth/CCS_SettlementVisualGrowthAnchor.cs");
            ValidateScriptExists(
                report,
                "Assets/CCS/Modules/Settlements/Runtime/Services/CCS_SettlementVisualGrowthService.cs");
            ValidateScriptExists(
                report,
                "Assets/CCS/Modules/Settlements/Runtime/Validation/CCS_SettlementVisualGrowthValidationUtility.cs");
            ValidateScriptExists(
                report,
                "Assets/CCS/Modules/Settlements/Editor/Validation/CCS_SettlementVisualGrowthFoundationBootstrapSetup.cs");
        }

        private static void ValidateVisualGrowthProfile(CCS_SurvivalValidationReport report)
        {
            CCS_SettlementVisualGrowthProfile profile = AssetDatabase.LoadAssetAtPath<CCS_SettlementVisualGrowthProfile>(
                CCS_SettlementVisualGrowthContentIds.DefaultVisualGrowthProfilePath);
            CCS_SurvivalValidationResult validation =
                CCS_SettlementVisualGrowthValidationUtility.ValidateProfile(profile);
            report.AddIssue(
                validation.IsSuccess ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                validation.Message);

            bool hasOutpostAnchors = false;
            bool hasTradingPostAnchors = false;
            if (profile != null)
            {
                CCS_SettlementVisualGrowthDefinition[] definitions = profile.AnchorDefinitions;
                for (int index = 0; index < definitions.Length; index++)
                {
                    CCS_SettlementVisualGrowthDefinition definition = definitions[index];
                    if (definition == null)
                    {
                        continue;
                    }

                    if (definition.requiredGrowthStage == CCS_SettlementGrowthStage.Outpost)
                    {
                        hasOutpostAnchors = true;
                    }

                    if (definition.requiredGrowthStage == CCS_SettlementGrowthStage.TradingPost)
                    {
                        hasTradingPostAnchors = true;
                    }
                }
            }

            report.AddIssue(
                hasOutpostAnchors ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                hasOutpostAnchors
                    ? "Visual growth profile defines Outpost stage anchors."
                    : "Visual growth profile missing Outpost anchors.");

            report.AddIssue(
                hasTradingPostAnchors ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                hasTradingPostAnchors
                    ? "Visual growth profile defines TradingPost stage anchors."
                    : "Visual growth profile missing TradingPost anchors.");
        }

        private static void ValidateWorldProfileWiring(CCS_SurvivalValidationReport report)
        {
            CCS_WorldSimulationProfile worldProfile = AssetDatabase.LoadAssetAtPath<CCS_WorldSimulationProfile>(
                "Assets/CCS/Survival/Profiles/WorldSimulation/CCS_DefaultWorldSimulationProfile.asset");
            CCS_SettlementVisualGrowthProfile visualProfile = AssetDatabase.LoadAssetAtPath<CCS_SettlementVisualGrowthProfile>(
                CCS_SettlementVisualGrowthContentIds.DefaultVisualGrowthProfilePath);
            bool wired = worldProfile != null && worldProfile.SettlementVisualGrowthProfile == visualProfile;
            report.AddIssue(
                wired ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                wired
                    ? "World simulation profile references settlement visual growth profile."
                    : "World simulation profile missing settlement visual growth profile reference.");
        }

        private static void ValidateBootstrapSceneAnchors(CCS_SurvivalValidationReport report)
        {
            string sceneSource = System.IO.File.Exists(BootstrapScenePath)
                ? System.IO.File.ReadAllText(BootstrapScenePath)
                : string.Empty;
            bool hasAnchors = sceneSource.Contains("CCS_SettlementVisualGrowthAnchor", System.StringComparison.Ordinal)
                && sceneSource.Contains(CCS_SettlementVisualGrowthContentIds.TradingPostOutpostCampAnchorId, System.StringComparison.Ordinal)
                && sceneSource.Contains(CCS_SettlementVisualGrowthContentIds.TradingPostTradingSignAnchorId, System.StringComparison.Ordinal);
            report.AddIssue(
                hasAnchors ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                hasAnchors
                    ? "Bootstrap scene includes settlement visual growth anchors."
                    : "Bootstrap scene missing visual growth anchors. Run visual growth bootstrap.");
        }

        private static void ValidateUtility(CCS_SurvivalValidationReport report)
        {
            CCS_SettlementGrowthSnapshot snapshot = new CCS_SettlementGrowthSnapshot
            {
                SettlementId = CCS_SettlementGrowthContentIds.TradingPostSettlementId,
                CurrentGrowthStage = CCS_SettlementGrowthStage.Outpost
            };

            CCS_SettlementVisualGrowthStatus status = CCS_SettlementVisualGrowthValidationUtility.ResolveVisualStatus(
                snapshot,
                CCS_SettlementGrowthStage.Outpost,
                true);
            bool active = status == CCS_SettlementVisualGrowthStatus.Active;
            report.AddIssue(
                active ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                active
                    ? "Visual growth utility resolves Outpost markers as Active."
                    : "Visual growth utility failed Outpost status smoke test.");
        }

        private static void ValidateRegistration(CCS_SurvivalValidationReport report)
        {
            string source = System.IO.File.ReadAllText(RegistrationPath);
            bool wired = source.Contains("CreateSettlementVisualGrowthService", System.StringComparison.Ordinal)
                && source.Contains("WireSettlementVisualGrowth", System.StringComparison.Ordinal);
            report.AddIssue(
                wired ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                wired
                    ? "Gameplay registration wires settlement visual growth service and refresh hooks."
                    : "Gameplay registration missing settlement visual growth wiring.");
        }

        private static void ValidatePlaytest(CCS_SurvivalValidationReport report)
        {
            string hudSource = System.IO.File.ReadAllText(
                "Assets/CCS/Modules/Playtesting/Runtime/UI/CCS_PlaytestHud.cs");
            bool shortcut = hudSource.Contains("TryPlaytestSettlementVisualGrowthFoundationShortcut", System.StringComparison.Ordinal)
                && hudSource.Contains("WasControlShiftPressed(KeyCode.Z)", System.StringComparison.Ordinal);
            report.AddIssue(
                shortcut ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                shortcut
                    ? "Playtest HUD exposes Ctrl+Shift+Z settlement visual growth shortcut."
                    : "Playtest HUD missing Ctrl+Shift+Z settlement visual growth shortcut.");

            CCS_PlaytestProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(
                "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset");
            if (profile == null)
            {
                return;
            }

            ValidateStep(report, profile, CCS_PlaytestStepType.VerifyTradingPostVisualMarkersActive);
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
                    : $"Playtest profile missing step {stepType}. Run visual growth bootstrap.");
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
