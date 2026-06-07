using CCS.Modules.Playtesting;
using CCS.Modules.WorldSimulation;
using CCS.Survival;
using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_SettlementNewsFoundationValidationValidator
// CATEGORY: Modules / Settlements / Editor / Validation
// PURPOSE: Validates settlement news module, profile, persistence, and playtest wiring.
// PLACEMENT: Registered on CCS_SurvivalValidationPipeline at editor load.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 5.2.0 settlement news and rumors foundation.
// =============================================================================

namespace CCS.Modules.Settlements.Editor
{
    public sealed class CCS_SettlementNewsFoundationValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string ValidatorContext = "ccs.survival.validation.settlementnews";
        private const string MilestoneVersion = "5.2.0";
        private const string RegistrationPath =
            "Assets/CCS/Survival/Runtime/Composition/CCS_SurvivalGameplayServiceRegistration.cs";

        public string ValidatorId => ValidatorContext;

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateScripts(report);
            ValidateNewsProfile(report);
            ValidateWorldProfileWiring(report);
            ValidateSaveData(report);
            ValidateRegistration(report);
            ValidateEventIntegration(report);
            ValidateContractBoardIntegration(report);
            ValidateDialogueIntegration(report);
            ValidatePlaytest(report);

            CCS_SurvivalBootstrapVersionUtility.AddBundleVersionValidationIssue(
                report,
                ValidatorContext,
                MilestoneVersion,
                "Run CCS_SettlementNewsFoundationBootstrapSetup.ExecuteBatch.");
            CCS_SurvivalInputValidationUtility.ValidateNoLegacyInputUsage(
                report,
                "Settlement News Input Policy");
            CCS_SurvivalInputValidationUtility.ValidateNoFindObjectsSortModeUsage(
                report,
                "Settlement News Obsolete API Scan");
        }

        private static void ValidateScripts(CCS_SurvivalValidationReport report)
        {
            ValidateScriptExists(report, "Assets/CCS/Modules/Settlements/Runtime/News/CCS_SettlementNewsProfile.cs");
            ValidateScriptExists(report, "Assets/CCS/Modules/Settlements/Runtime/News/CCS_SettlementNewsService.cs");
            ValidateScriptExists(report, "Assets/CCS/Modules/Settlements/Runtime/News/CCS_SettlementNewsRuntimeBridge.cs");
            ValidateScriptExists(report, "Assets/CCS/Modules/Settlements/Runtime/News/CCS_SettlementNewsValidationUtility.cs");
            ValidateScriptExists(
                report,
                "Assets/CCS/Modules/Settlements/Editor/Validation/CCS_SettlementNewsFoundationBootstrapSetup.cs");
        }

        private static void ValidateNewsProfile(CCS_SurvivalValidationReport report)
        {
            CCS_SettlementNewsProfile profile = AssetDatabase.LoadAssetAtPath<CCS_SettlementNewsProfile>(
                CCS_SettlementNewsContentIds.DefaultNewsProfilePath);
            CCS_SurvivalValidationResult validation = CCS_SettlementNewsValidationUtility.ValidateProfile(profile);
            report.AddIssue(
                validation.IsSuccess ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                validation.Message);
        }

        private static void ValidateWorldProfileWiring(CCS_SurvivalValidationReport report)
        {
            CCS_WorldSimulationProfile profile = AssetDatabase.LoadAssetAtPath<CCS_WorldSimulationProfile>(
                "Assets/CCS/Survival/Profiles/WorldSimulation/CCS_DefaultWorldSimulationProfile.asset");
            bool wired = profile != null && profile.SettlementNewsProfile != null;
            report.AddIssue(
                wired ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                wired
                    ? "World simulation profile references settlement news profile."
                    : "World simulation profile missing settlementNewsProfile reference.");
        }

        private static void ValidateSaveData(CCS_SurvivalValidationReport report)
        {
            string saveSource = System.IO.File.ReadAllText(
                "Assets/CCS/Modules/SaveSystem/Runtime/Data/CCS_SaveData.cs");
            bool ok = saveSource.Contains("newsEntries", System.StringComparison.Ordinal);
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                ok
                    ? "World simulation save payload persists settlement news entries."
                    : "World simulation save payload missing newsEntries field.");

            string worldSource = System.IO.File.ReadAllText(
                "Assets/CCS/Modules/WorldSimulation/Runtime/Services/CCS_WorldSimulationService.cs");
            bool captureOk = worldSource.Contains("CaptureNewsState", System.StringComparison.Ordinal)
                && worldSource.Contains("RestoreNewsState", System.StringComparison.Ordinal);
            report.AddIssue(
                captureOk ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                captureOk
                    ? "World simulation service captures and restores settlement news state."
                    : "World simulation service missing settlement news capture/restore.");
        }

        private static void ValidateRegistration(CCS_SurvivalValidationReport report)
        {
            string source = System.IO.File.Exists(RegistrationPath)
                ? System.IO.File.ReadAllText(RegistrationPath)
                : string.Empty;
            bool ok = source.Contains("CreateSettlementNewsService", System.StringComparison.Ordinal)
                && source.Contains("WireSettlementNews", System.StringComparison.Ordinal)
                && source.Contains("NotifyEventActivated", System.StringComparison.Ordinal);
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                ok
                    ? "Gameplay service registration wires settlement news service and event integration."
                    : "Gameplay service registration missing settlement news wiring.");
        }

        private static void ValidateEventIntegration(CCS_SurvivalValidationReport report)
        {
            string eventSource = System.IO.File.ReadAllText(
                "Assets/CCS/Modules/Settlements/Runtime/Events/CCS_SettlementEventService.cs");
            bool ok = eventSource.Contains("NotifyEventActivated", System.StringComparison.Ordinal);
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                ok
                    ? "Settlement event activation notifies settlement news generation."
                    : "Settlement event service missing news activation notification.");
        }

        private static void ValidateContractBoardIntegration(CCS_SurvivalValidationReport report)
        {
            string hudSource = System.IO.File.ReadAllText(
                "Assets/CCS/Modules/Contracts/Runtime/UI/CCS_ContractDebugHud.cs");
            bool ok = hudSource.Contains("DrawRecentNewsSection", System.StringComparison.Ordinal)
                && hudSource.Contains("CCS_SettlementNewsRuntimeBridge.TryGetRecentNews", System.StringComparison.Ordinal);
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                ok
                    ? "Contract debug board displays recent settlement news entries."
                    : "Contract debug board missing recent settlement news section.");
        }

        private static void ValidateDialogueIntegration(CCS_SurvivalValidationReport report)
        {
            string dialogueSource = System.IO.File.ReadAllText(
                "Assets/CCS/Modules/NPCs/Runtime/Dialogue/CCS_NpcDialogueStubValidationUtility.cs");
            bool ok = dialogueSource.Contains(
                "CCS_SettlementNewsRuntimeBridge.ResolveRumorDialogueAppendLine",
                System.StringComparison.Ordinal);
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                ok
                    ? "Dialogue stub resolution appends one settlement rumor line."
                    : "Dialogue stub resolution missing settlement rumor append integration.");
        }

        private static void ValidatePlaytest(CCS_SurvivalValidationReport report)
        {
            CCS_PlaytestProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(
                "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset");
            ValidateStep(report, profile, CCS_PlaytestStepType.ForceEventForSettlementNews);
            ValidateStep(report, profile, CCS_PlaytestStepType.VerifySettlementNewsCreated);
            ValidateStep(report, profile, CCS_PlaytestStepType.VerifyContractBoardSettlementNews);
            ValidateStep(report, profile, CCS_PlaytestStepType.VerifySettlementNewsPropagated);
            ValidateStep(report, profile, CCS_PlaytestStepType.SaveSettlementNewsState);
            ValidateStep(report, profile, CCS_PlaytestStepType.VerifySettlementNewsAfterLoad);

            string hudSource = System.IO.File.ReadAllText(
                "Assets/CCS/Modules/Playtesting/Runtime/UI/CCS_PlaytestHud.cs");
            bool shortcut = hudSource.Contains("TryPlaytestSettlementNewsFoundationShortcut", System.StringComparison.Ordinal)
                && hudSource.Contains("WasControlAltPressed(KeyCode.N)", System.StringComparison.Ordinal);
            report.AddIssue(
                shortcut ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                shortcut
                    ? "Playtest HUD exposes Ctrl+Alt+N settlement news shortcut."
                    : "Playtest HUD missing Ctrl+Alt+N settlement news shortcut.");
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
                    : $"Playtest profile missing step {stepType}. Run settlement news bootstrap.");
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
