using CCS.Modules.Playtesting;
using CCS.Modules.WorldSimulation;
using CCS.Survival;
using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_NpcDialogueFoundationValidationValidator
// CATEGORY: Modules / NPCs / Editor / Validation
// PURPOSE: Validates NPC dialogue stub module, profile, integration, and playtest.
// PLACEMENT: Registered on CCS_SurvivalValidationPipeline at editor load.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.9.0 NPC dialogue stub foundation.
// =============================================================================

namespace CCS.Modules.NPCs.Editor
{
    public sealed class CCS_NpcDialogueFoundationValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string ValidatorContext = "ccs.survival.validation.npcdialogue";
        private const string MilestoneVersion = "4.9.0";
        private const string RegistrationPath =
            "Assets/CCS/Survival/Runtime/Composition/CCS_SurvivalGameplayServiceRegistration.cs";

        public string ValidatorId => ValidatorContext;

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateScripts(report);
            ValidateDialogueProfile(report);
            ValidateWorldProfileWiring(report);
            ValidateRegistration(report);
            ValidateRepresentativeIntegration(report);
            ValidatePlaytest(report);

            CCS_SurvivalBootstrapVersionUtility.AddBundleVersionValidationIssue(
                report,
                ValidatorContext,
                MilestoneVersion,
                "Run CCS_NpcDialogueFoundationBootstrapSetup.ExecuteBatch.");
            CCS_SurvivalInputValidationUtility.ValidateNoLegacyInputUsage(
                report,
                "NPC Dialogue Input Policy");
            CCS_SurvivalInputValidationUtility.ValidateNoFindObjectsSortModeUsage(
                report,
                "NPC Dialogue Obsolete API Scan");
        }

        private static void ValidateScripts(CCS_SurvivalValidationReport report)
        {
            ValidateScriptExists(report, "Assets/CCS/Modules/NPCs/Runtime/Dialogue/CCS_NpcDialogueStubProfile.cs");
            ValidateScriptExists(report, "Assets/CCS/Modules/NPCs/Runtime/Dialogue/CCS_NpcDialogueStubService.cs");
            ValidateScriptExists(report, "Assets/CCS/Modules/NPCs/Runtime/Dialogue/CCS_NpcDialogueStubRuntimeBridge.cs");
            ValidateScriptExists(report, "Assets/CCS/Modules/NPCs/Runtime/Dialogue/CCS_NpcDialogueStubValidationUtility.cs");
            ValidateScriptExists(report, "Assets/CCS/Modules/NPCs/Runtime/UI/CCS_NpcDialogueStubDebugHud.cs");
            ValidateScriptExists(report, "Assets/CCS/Modules/NPCs/Runtime/Components/CCS_NpcDialogueStubInteractable.cs");
            ValidateScriptExists(
                report,
                "Assets/CCS/Modules/NPCs/Editor/Validation/CCS_NpcDialogueFoundationBootstrapSetup.cs");
        }

        private static void ValidateDialogueProfile(CCS_SurvivalValidationReport report)
        {
            CCS_NpcDialogueStubProfile profile = AssetDatabase.LoadAssetAtPath<CCS_NpcDialogueStubProfile>(
                CCS_NpcDialogueStubContentIds.DefaultDialogueStubProfilePath);
            CCS_SurvivalValidationResult validation = CCS_NpcDialogueStubValidationUtility.ValidateProfile(profile);
            report.AddIssue(
                validation.IsSuccess ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                validation.Message);
        }

        private static void ValidateWorldProfileWiring(CCS_SurvivalValidationReport report)
        {
            CCS_WorldSimulationProfile profile = AssetDatabase.LoadAssetAtPath<CCS_WorldSimulationProfile>(
                "Assets/CCS/Survival/Profiles/WorldSimulation/CCS_DefaultWorldSimulationProfile.asset");
            bool wired = profile != null && profile.SettlementNpcDialogueStubProfile != null;
            report.AddIssue(
                wired ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                wired
                    ? "World simulation profile references settlement NPC dialogue stub profile."
                    : "World simulation profile missing settlementNpcDialogueStubProfile reference.");
        }

        private static void ValidateRegistration(CCS_SurvivalValidationReport report)
        {
            string source = System.IO.File.Exists(RegistrationPath)
                ? System.IO.File.ReadAllText(RegistrationPath)
                : string.Empty;
            bool ok = source.Contains("CreateNpcDialogueStubService", System.StringComparison.Ordinal)
                && source.Contains("WireNpcDialogueStub", System.StringComparison.Ordinal);
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                ok
                    ? "Gameplay service registration wires NPC dialogue stub service."
                    : "Gameplay service registration missing NPC dialogue stub wiring.");
        }

        private static void ValidateRepresentativeIntegration(CCS_SurvivalValidationReport report)
        {
            string source = System.IO.File.ReadAllText(
                "Assets/CCS/Modules/NPCs/Runtime/Components/CCS_NpcServiceRepresentativeInteractable.cs");
            bool ok = source.Contains("CCS_NpcDialogueStubValidationUtility.BuildRequestFromHost", System.StringComparison.Ordinal)
                && source.Contains("CCS_NpcDialogueStubRuntimeBridge.ResolveDialogue", System.StringComparison.Ordinal)
                && source.Contains("CCS_SettlementServiceRouteResolver.TryActivate", System.StringComparison.Ordinal);
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                ok
                    ? "Representative interactable resolves dialogue stubs before existing service routing."
                    : "Representative interactable missing dialogue stub integration.");
        }

        private static void ValidatePlaytest(CCS_SurvivalValidationReport report)
        {
            CCS_PlaytestProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(
                "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset");
            ValidateStep(report, profile, CCS_PlaytestStepType.DiscoverSettlementForNpcDialogue);
            ValidateStep(report, profile, CCS_PlaytestStepType.VerifyNpcDialogueGreeting);
            ValidateStep(report, profile, CCS_PlaytestStepType.VerifyNpcDialogueServiceHint);
            ValidateStep(report, profile, CCS_PlaytestStepType.SaveNpcDialogueState);
            ValidateStep(report, profile, CCS_PlaytestStepType.VerifyNpcDialogueAfterLoad);

            string hudSource = System.IO.File.ReadAllText(
                "Assets/CCS/Modules/Playtesting/Runtime/UI/CCS_PlaytestHud.cs");
            bool shortcut = hudSource.Contains("TryPlaytestNpcDialogueFoundationShortcut", System.StringComparison.Ordinal)
                && hudSource.Contains("WasControlAltPressed(KeyCode.D)", System.StringComparison.Ordinal);
            report.AddIssue(
                shortcut ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                shortcut
                    ? "Playtest HUD exposes Ctrl+Alt+D NPC dialogue shortcut."
                    : "Playtest HUD missing Ctrl+Alt+D NPC dialogue shortcut.");
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
                    : $"Playtest profile missing step {stepType}. Run NPC dialogue bootstrap.");
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
