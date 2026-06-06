using CCS.Modules.Playtesting;
using CCS.Modules.WorldSimulation;
using CCS.Survival;
using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_SettlementHousingFoundationValidationValidator
// CATEGORY: Modules / Settlements / Editor / Validation
// PURPOSE: Validates settlement housing profiles, anchors, wiring, and playtest harness.
// PLACEMENT: Registered on CCS_SurvivalValidationPipeline at editor load.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.4.0 settlement housing foundation.
// =============================================================================

namespace CCS.Modules.Settlements.Editor
{
    public sealed class CCS_SettlementHousingFoundationValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string ValidatorContext = "ccs.survival.validation.settlementhousing";
        private const string MilestoneVersion = "4.4.0";
        private const string BootstrapScenePath = "Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string RegistrationPath =
            "Assets/CCS/Survival/Runtime/Composition/CCS_SurvivalGameplayServiceRegistration.cs";

        public string ValidatorId => ValidatorContext;

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateScripts(report);
            ValidateHousingProfile(report);
            ValidateWorldProfileWiring(report);
            ValidateBootstrapSceneAnchors(report);
            ValidateUtility(report);
            ValidateRegistration(report);
            ValidatePlaytest(report);

            CCS_SurvivalBootstrapVersionUtility.AddBundleVersionValidationIssue(
                report,
                ValidatorContext,
                MilestoneVersion,
                "Run CCS_SettlementHousingFoundationBootstrapSetup.ExecuteBatch.");
            CCS_SurvivalInputValidationUtility.ValidateNoLegacyInputUsage(report, "Settlement Housing Input Policy");
            CCS_SurvivalInputValidationUtility.ValidateNoFindObjectsSortModeUsage(
                report,
                "Settlement Housing Obsolete API Scan");
        }

        private static void ValidateScripts(CCS_SurvivalValidationReport report)
        {
            ValidateScriptExists(report, "Assets/CCS/Modules/Settlements/Runtime/Housing/CCS_SettlementHousingProfile.cs");
            ValidateScriptExists(report, "Assets/CCS/Modules/Settlements/Runtime/Housing/CCS_SettlementHousingAnchor.cs");
            ValidateScriptExists(report, "Assets/CCS/Modules/Settlements/Runtime/Services/CCS_SettlementHousingService.cs");
            ValidateScriptExists(report, "Assets/CCS/Modules/Settlements/Runtime/Validation/CCS_SettlementHousingValidationUtility.cs");
            ValidateScriptExists(
                report,
                "Assets/CCS/Modules/Settlements/Editor/Validation/CCS_SettlementHousingFoundationBootstrapSetup.cs");
        }

        private static void ValidateHousingProfile(CCS_SurvivalValidationReport report)
        {
            CCS_SettlementHousingProfile profile = AssetDatabase.LoadAssetAtPath<CCS_SettlementHousingProfile>(
                CCS_SettlementHousingContentIds.DefaultHousingProfilePath);
            CCS_SurvivalValidationResult validation =
                CCS_SettlementHousingValidationUtility.ValidateProfile(profile);
            report.AddIssue(
                validation.IsSuccess ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                validation.Message);

            bool hasFourDefinitions = profile != null && profile.HousingDefinitions.Length >= 4;
            report.AddIssue(
                hasFourDefinitions ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                hasFourDefinitions
                    ? "Settlement housing profile defines housing for all bootstrap settlements."
                    : "Settlement housing profile missing bootstrap settlement housing entries.");
        }

        private static void ValidateWorldProfileWiring(CCS_SurvivalValidationReport report)
        {
            CCS_WorldSimulationProfile worldProfile = AssetDatabase.LoadAssetAtPath<CCS_WorldSimulationProfile>(
                "Assets/CCS/Survival/Profiles/WorldSimulation/CCS_DefaultWorldSimulationProfile.asset");
            CCS_SettlementHousingProfile housingProfile = AssetDatabase.LoadAssetAtPath<CCS_SettlementHousingProfile>(
                CCS_SettlementHousingContentIds.DefaultHousingProfilePath);
            bool wired = worldProfile != null && worldProfile.SettlementHousingProfile == housingProfile;
            report.AddIssue(
                wired ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                wired
                    ? "World simulation profile references settlement housing profile."
                    : "World simulation profile missing settlement housing profile reference.");
        }

        private static void ValidateBootstrapSceneAnchors(CCS_SurvivalValidationReport report)
        {
            string sceneSource = System.IO.File.Exists(BootstrapScenePath)
                ? System.IO.File.ReadAllText(BootstrapScenePath)
                : string.Empty;
            bool hasAnchors = sceneSource.Contains("CCS_SettlementHousingAnchor", System.StringComparison.Ordinal)
                && sceneSource.Contains(CCS_SettlementHousingContentIds.TradingPostBoardingHouseAnchorId, System.StringComparison.Ordinal)
                && sceneSource.Contains(CCS_SettlementHousingContentIds.PineRidgeWorkerCabinAnchorId, System.StringComparison.Ordinal);
            report.AddIssue(
                hasAnchors ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                hasAnchors
                    ? "Bootstrap scene includes settlement housing anchors."
                    : "Bootstrap scene missing settlement housing anchors. Run housing bootstrap.");
        }

        private static void ValidateUtility(CCS_SurvivalValidationReport report)
        {
            CCS_SettlementHousingState[] states =
            {
                new CCS_SettlementHousingState
                {
                    housingId = CCS_SettlementHousingContentIds.TradingPostBoardingHouseId,
                    displayName = "Boarding House",
                    capacityContribution = 20,
                    isActive = true
                }
            };

            int housingCapacity = CCS_SettlementHousingValidationUtility.ResolveActiveHousingCapacity(states);
            bool ok = housingCapacity == 20;
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                ok
                    ? "Settlement housing utility resolves active housing capacity."
                    : "Settlement housing utility failed capacity smoke test.");
        }

        private static void ValidateRegistration(CCS_SurvivalValidationReport report)
        {
            string source = System.IO.File.ReadAllText(RegistrationPath);
            bool wired = source.Contains("CreateSettlementHousingService", System.StringComparison.Ordinal)
                && source.Contains("WireSettlementHousing", System.StringComparison.Ordinal);
            report.AddIssue(
                wired ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                wired
                    ? "Gameplay registration wires settlement housing service and refresh hooks."
                    : "Gameplay registration missing settlement housing wiring.");
        }

        private static void ValidatePlaytest(CCS_SurvivalValidationReport report)
        {
            string hudSource = System.IO.File.ReadAllText(
                "Assets/CCS/Modules/Playtesting/Runtime/UI/CCS_PlaytestHud.cs");
            bool shortcut = hudSource.Contains("TryPlaytestSettlementHousingFoundationShortcut", System.StringComparison.Ordinal)
                && hudSource.Contains("WasControlAltPressed(KeyCode.H)", System.StringComparison.Ordinal);
            report.AddIssue(
                shortcut ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                shortcut
                    ? "Playtest HUD exposes Ctrl+Alt+H settlement housing shortcut."
                    : "Playtest HUD missing Ctrl+Alt+H settlement housing shortcut.");

            CCS_PlaytestProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(
                "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset");
            if (profile == null)
            {
                return;
            }

            ValidateStep(report, profile, CCS_PlaytestStepType.VerifySettlementHousingAfterLoad);
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
                    : $"Playtest profile missing step {stepType}. Run settlement housing bootstrap.");
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
