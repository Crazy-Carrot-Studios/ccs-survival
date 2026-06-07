using CCS.Modules.Playtesting;
using CCS.Modules.WorldSimulation;
using CCS.Survival;
using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_SettlementEventsFoundationValidationValidator
// CATEGORY: Modules / Settlements / Editor / Validation
// PURPOSE: Validates settlement events module, profile, persistence, and playtest wiring.
// PLACEMENT: Registered on CCS_SurvivalValidationPipeline at editor load.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 5.1.0 dynamic settlement events foundation.
// =============================================================================

namespace CCS.Modules.Settlements.Editor
{
    public sealed class CCS_SettlementEventsFoundationValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string ValidatorContext = "ccs.survival.validation.settlementevents";
        private const string MilestoneVersion = "5.1.0";
        private const string RegistrationPath =
            "Assets/CCS/Survival/Runtime/Composition/CCS_SurvivalGameplayServiceRegistration.cs";

        public string ValidatorId => ValidatorContext;

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateScripts(report);
            ValidateEventProfile(report);
            ValidateWorldProfileWiring(report);
            ValidateSimulationState(report);
            ValidateRegistration(report);
            ValidateNpcIntegration(report);
            ValidatePlaytest(report);

            CCS_SurvivalBootstrapVersionUtility.AddBundleVersionValidationIssue(
                report,
                ValidatorContext,
                MilestoneVersion,
                "Run CCS_SettlementEventsFoundationBootstrapSetup.ExecuteBatch.");
            CCS_SurvivalInputValidationUtility.ValidateNoLegacyInputUsage(
                report,
                "Settlement Events Input Policy");
            CCS_SurvivalInputValidationUtility.ValidateNoFindObjectsSortModeUsage(
                report,
                "Settlement Events Obsolete API Scan");
        }

        private static void ValidateScripts(CCS_SurvivalValidationReport report)
        {
            ValidateScriptExists(report, "Assets/CCS/Modules/Settlements/Runtime/Events/CCS_SettlementEventProfile.cs");
            ValidateScriptExists(report, "Assets/CCS/Modules/Settlements/Runtime/Events/CCS_SettlementEventService.cs");
            ValidateScriptExists(report, "Assets/CCS/Modules/Settlements/Runtime/Events/CCS_SettlementEventRuntimeBridge.cs");
            ValidateScriptExists(report, "Assets/CCS/Modules/Settlements/Runtime/Events/CCS_SettlementEventValidationUtility.cs");
            ValidateScriptExists(report, "Assets/CCS/Modules/Settlements/Runtime/Events/CCS_SettlementEventMarker.cs");
            ValidateScriptExists(report, "Assets/CCS/Modules/Settlements/Runtime/Events/CCS_SettlementEventLabel.cs");
            ValidateScriptExists(
                report,
                "Assets/CCS/Modules/Settlements/Editor/Validation/CCS_SettlementEventsFoundationBootstrapSetup.cs");
        }

        private static void ValidateEventProfile(CCS_SurvivalValidationReport report)
        {
            CCS_SettlementEventProfile profile = AssetDatabase.LoadAssetAtPath<CCS_SettlementEventProfile>(
                CCS_SettlementEventContentIds.DefaultEventProfilePath);
            CCS_SurvivalValidationResult validation = CCS_SettlementEventValidationUtility.ValidateProfile(profile);
            report.AddIssue(
                validation.IsSuccess ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                validation.Message);
        }

        private static void ValidateWorldProfileWiring(CCS_SurvivalValidationReport report)
        {
            CCS_WorldSimulationProfile profile = AssetDatabase.LoadAssetAtPath<CCS_WorldSimulationProfile>(
                "Assets/CCS/Survival/Profiles/WorldSimulation/CCS_DefaultWorldSimulationProfile.asset");
            bool wired = profile != null && profile.SettlementEventProfile != null;
            report.AddIssue(
                wired ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                wired
                    ? "World simulation profile references settlement event profile."
                    : "World simulation profile missing settlementEventProfile reference.");
        }

        private static void ValidateSimulationState(CCS_SurvivalValidationReport report)
        {
            string source = System.IO.File.ReadAllText(
                "Assets/CCS/Modules/WorldSimulation/Runtime/Data/CCS_SettlementSimulationState.cs");
            bool ok = source.Contains("activeSettlementEvent", System.StringComparison.Ordinal);
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                ok
                    ? "Settlement simulation state persists activeSettlementEvent."
                    : "Settlement simulation state missing activeSettlementEvent field.");
        }

        private static void ValidateRegistration(CCS_SurvivalValidationReport report)
        {
            string source = System.IO.File.Exists(RegistrationPath)
                ? System.IO.File.ReadAllText(RegistrationPath)
                : string.Empty;
            bool ok = source.Contains("CreateSettlementEventService", System.StringComparison.Ordinal)
                && source.Contains("WireSettlementEvents", System.StringComparison.Ordinal)
                && source.Contains("SetActiveSettlementEvent", System.StringComparison.Ordinal);
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                ok
                    ? "Gameplay service registration wires settlement event service and persistence."
                    : "Gameplay service registration missing settlement event wiring.");
        }

        private static void ValidateNpcIntegration(CCS_SurvivalValidationReport report)
        {
            string dialogueSource = System.IO.File.ReadAllText(
                "Assets/CCS/Modules/NPCs/Runtime/Dialogue/CCS_NpcDialogueStubValidationUtility.cs");
            bool dialogueOk = dialogueSource.Contains(
                "CCS_SettlementEventRuntimeBridge.ResolveDialogueAppendLine",
                System.StringComparison.Ordinal);
            report.AddIssue(
                dialogueOk ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                dialogueOk
                    ? "Dialogue stub resolution appends active settlement event lines."
                    : "Dialogue stub resolution missing settlement event append integration.");

            string socialSource = System.IO.File.ReadAllText(
                "Assets/CCS/Modules/NPCs/Runtime/Social/CCS_NpcSocialValidationUtility.cs");
            bool socialOk = socialSource.Contains("CCS_SettlementEventRuntimeBridge.TryGetActiveEvent", System.StringComparison.Ordinal);
            report.AddIssue(
                socialOk ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                socialOk
                    ? "Social gathering resolution prefers active settlement event anchors."
                    : "Social gathering resolution missing settlement event anchor preference.");
        }

        private static void ValidatePlaytest(CCS_SurvivalValidationReport report)
        {
            CCS_PlaytestProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(
                "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset");
            ValidateStep(report, profile, CCS_PlaytestStepType.DiscoverSettlementForSettlementEvents);
            ValidateStep(report, profile, CCS_PlaytestStepType.ForceMarketDayForSettlementEvents);
            ValidateStep(report, profile, CCS_PlaytestStepType.VerifySettlementEventMarker);
            ValidateStep(report, profile, CCS_PlaytestStepType.SaveSettlementEventState);
            ValidateStep(report, profile, CCS_PlaytestStepType.VerifySettlementEventAfterLoad);

            string hudSource = System.IO.File.ReadAllText(
                "Assets/CCS/Modules/Playtesting/Runtime/UI/CCS_PlaytestHud.cs");
            bool shortcut = hudSource.Contains("TryPlaytestSettlementEventsFoundationShortcut", System.StringComparison.Ordinal)
                && hudSource.Contains("WasControlAltPressed(KeyCode.E)", System.StringComparison.Ordinal);
            report.AddIssue(
                shortcut ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                shortcut
                    ? "Playtest HUD exposes Ctrl+Alt+E settlement events shortcut."
                    : "Playtest HUD missing Ctrl+Alt+E settlement events shortcut.");
        }

        private static void ValidateStep(
            CCS_SurvivalValidationReport report,
            CCS_PlaytestProfile profile,
            CCS_PlaytestStepType stepType)
        {
            bool present = false;
            if (profile?.StepDefinitions != null)
            {
                for (int index = 0; index < profile.StepDefinitions.Count; index++)
                {
                    if (profile.StepDefinitions[index]?.StepType == stepType)
                    {
                        present = true;
                        break;
                    }
                }
            }

            report.AddIssue(
                present ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                present
                    ? $"Playtest profile includes step {stepType}."
                    : $"Playtest profile missing step {stepType}. Run settlement events bootstrap.");
        }

        private static void ValidateScriptExists(CCS_SurvivalValidationReport report, string assetPath)
        {
            bool exists = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath) != null;
            report.AddIssue(
                exists ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                exists ? $"Script present: {assetPath}." : $"Missing script: {assetPath}.");
        }
    }
}
