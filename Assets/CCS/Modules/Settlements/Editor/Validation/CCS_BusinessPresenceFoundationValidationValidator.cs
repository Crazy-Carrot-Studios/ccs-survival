using CCS.Modules.Playtesting;
using CCS.Modules.WorldSimulation;
using CCS.Survival;
using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_BusinessPresenceFoundationValidationValidator
// CATEGORY: Modules / Settlements / Editor / Validation
// PURPOSE: Validates business presence anchors, profiles, playtest, and registration wiring.
// PLACEMENT: Registered on CCS_SurvivalValidationPipeline at editor load.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.8.0 — visible business presence foundation.
// =============================================================================

namespace CCS.Modules.Settlements.Editor
{
    public sealed class CCS_BusinessPresenceFoundationValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string ValidatorContext = "ccs.survival.validation.businesspresence";
        private const string MilestoneVersion = "3.8.0";
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
                "Run CCS_BusinessPresenceFoundationBootstrapSetup.ExecuteBatch.");
            CCS_SurvivalInputValidationUtility.ValidateNoLegacyInputUsage(report, "Business Presence Input Policy");
            CCS_SurvivalInputValidationUtility.ValidateNoFindObjectsSortModeUsage(report, "Business Presence Obsolete API Scan");
        }

        private static void ValidateScripts(CCS_SurvivalValidationReport report)
        {
            ValidateScriptExists(report, "Assets/CCS/Modules/Settlements/Runtime/BusinessPresence/CCS_BusinessPresenceAnchor.cs");
            ValidateScriptExists(report, "Assets/CCS/Modules/Settlements/Runtime/Services/CCS_BusinessPresenceService.cs");
            ValidateScriptExists(report, "Assets/CCS/Modules/Settlements/Runtime/Validation/CCS_BusinessPresenceValidationUtility.cs");
            ValidateScriptExists(report, "Assets/CCS/Modules/Settlements/Editor/Validation/CCS_BusinessPresenceFoundationBootstrapSetup.cs");
        }

        private static void ValidatePresenceProfile(CCS_SurvivalValidationReport report)
        {
            CCS_BusinessPresenceProfile profile = AssetDatabase.LoadAssetAtPath<CCS_BusinessPresenceProfile>(
                CCS_BusinessPresenceContentIds.DefaultPresenceProfilePath);
            CCS_SurvivalValidationResult validation = CCS_BusinessPresenceValidationUtility.ValidateProfile(profile);
            report.AddIssue(
                validation.IsSuccess ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                validation.Message);

            CCS_BusinessProfile businessProfile = AssetDatabase.LoadAssetAtPath<CCS_BusinessProfile>(
                CCS_BusinessContentIds.DefaultBusinessProfilePath);
            bool catalogAligned = profile != null
                && businessProfile != null
                && profile.AnchorDefinitions.Length >= 8;
            report.AddIssue(
                catalogAligned ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                catalogAligned
                    ? "Business presence profile defines anchors for all milestone settlements."
                    : "Business presence profile missing settlement anchors.");
        }

        private static void ValidateWorldProfileWiring(CCS_SurvivalValidationReport report)
        {
            CCS_WorldSimulationProfile worldProfile = AssetDatabase.LoadAssetAtPath<CCS_WorldSimulationProfile>(
                "Assets/CCS/Survival/Profiles/WorldSimulation/CCS_DefaultWorldSimulationProfile.asset");
            CCS_BusinessPresenceProfile presenceProfile = AssetDatabase.LoadAssetAtPath<CCS_BusinessPresenceProfile>(
                CCS_BusinessPresenceContentIds.DefaultPresenceProfilePath);
            bool wired = worldProfile != null && worldProfile.SettlementBusinessPresenceProfile == presenceProfile;
            report.AddIssue(
                wired ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                wired
                    ? "World simulation profile references business presence profile."
                    : "World simulation profile missing business presence profile reference.");
        }

        private static void ValidateBootstrapSceneAnchors(CCS_SurvivalValidationReport report)
        {
            string sceneSource = System.IO.File.Exists(BootstrapScenePath)
                ? System.IO.File.ReadAllText(BootstrapScenePath)
                : string.Empty;
            bool hasAnchors = sceneSource.Contains("CCS_BusinessPresenceAnchor", System.StringComparison.Ordinal)
                && sceneSource.Contains(CCS_BusinessPresenceContentIds.TradingPostGeneralStoreAnchorId, System.StringComparison.Ordinal)
                && sceneSource.Contains(CCS_BusinessPresenceContentIds.BrokenCreekFarmSupplyAnchorId, System.StringComparison.Ordinal);
            report.AddIssue(
                hasAnchors ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                hasAnchors
                    ? "Bootstrap scene includes business presence anchors for milestone settlements."
                    : "Bootstrap scene missing business presence anchors. Run presence bootstrap.");
        }

        private static void ValidateUtility(CCS_SurvivalValidationReport report)
        {
            CCS_BusinessSnapshot snapshot = new CCS_BusinessSnapshot
            {
                SettlementId = CCS_SettlementGrowthContentIds.TradingPostSettlementId,
                ActiveBusinesses = new[]
                {
                    new CCS_BusinessInstance
                    {
                        BusinessType = CCS_BusinessType.GeneralStore,
                        IsActive = true,
                        MeetsActivationThresholds = true
                    }
                }
            };

            CCS_BusinessPresenceStatus status = CCS_BusinessPresenceValidationUtility.ResolvePresenceStatus(
                snapshot,
                CCS_BusinessType.GeneralStore);
            bool active = status == CCS_BusinessPresenceStatus.Active;
            report.AddIssue(
                active ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                active
                    ? "Business presence utility resolves active status from business snapshots."
                    : "Business presence utility failed status resolution smoke test.");
        }

        private static void ValidateRegistration(CCS_SurvivalValidationReport report)
        {
            string source = System.IO.File.ReadAllText(RegistrationPath);
            bool wired = source.Contains("CreateBusinessPresenceService", System.StringComparison.Ordinal)
                && source.Contains("WireBusinessPresence", System.StringComparison.Ordinal);
            report.AddIssue(
                wired ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                wired
                    ? "Gameplay registration wires business presence service and refresh hooks."
                    : "Gameplay registration missing business presence wiring.");
        }

        private static void ValidatePlaytest(CCS_SurvivalValidationReport report)
        {
            string hudSource = System.IO.File.ReadAllText(
                "Assets/CCS/Modules/Playtesting/Runtime/UI/CCS_PlaytestHud.cs");
            bool shortcut = hudSource.Contains("TryPlaytestBusinessPresenceFoundationShortcut", System.StringComparison.Ordinal)
                && hudSource.Contains("WasControlShiftPressed(KeyCode.V)", System.StringComparison.Ordinal);
            report.AddIssue(
                shortcut ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                shortcut
                    ? "Playtest HUD exposes Ctrl+Shift+V business presence shortcut."
                    : "Playtest HUD missing Ctrl+Shift+V business presence shortcut.");

            CCS_PlaytestProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(
                "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset");
            if (profile == null)
            {
                return;
            }

            ValidateStep(report, profile, CCS_PlaytestStepType.VerifyBusinessMarkerActive);
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
                    : $"Playtest profile missing step {stepType}. Run presence bootstrap.");
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
