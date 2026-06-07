using CCS.Modules.Playtesting;
using CCS.Modules.WorldSimulation;
using CCS.Survival;
using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_NpcScheduleFoundationValidationValidator
// CATEGORY: Modules / NPCs / Editor / Validation
// PURPOSE: Validates NPC schedule module, profile wiring, movement fallback, and playtest.
// PLACEMENT: Registered on CCS_SurvivalValidationPipeline at editor load.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.6.0 NPC schedule state foundation.
// =============================================================================

namespace CCS.Modules.NPCs.Editor
{
    public sealed class CCS_NpcScheduleFoundationValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string ValidatorContext = "ccs.survival.validation.npcschedule";
        private const string MilestoneVersion = "4.6.0";
        private const string RegistrationPath =
            "Assets/CCS/Survival/Runtime/Composition/CCS_SurvivalGameplayServiceRegistration.cs";

        public string ValidatorId => ValidatorContext;

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateScripts(report);
            ValidateScheduleProfile(report);
            ValidateWorldProfileWiring(report);
            ValidateSimulationState(report);
            ValidateRegistration(report);
            ValidateMovementFallback(report);
            ValidatePlaytest(report);

            CCS_SurvivalBootstrapVersionUtility.AddBundleVersionValidationIssue(
                report,
                ValidatorContext,
                MilestoneVersion,
                "Run CCS_NpcScheduleFoundationBootstrapSetup.ExecuteBatch.");
            CCS_SurvivalInputValidationUtility.ValidateNoLegacyInputUsage(
                report,
                "NPC Schedule Input Policy");
            CCS_SurvivalInputValidationUtility.ValidateNoFindObjectsSortModeUsage(
                report,
                "NPC Schedule Obsolete API Scan");
        }

        private static void ValidateScripts(CCS_SurvivalValidationReport report)
        {
            ValidateScriptExists(report, "Assets/CCS/Modules/NPCs/Runtime/Schedules/CCS_NpcScheduleProfile.cs");
            ValidateScriptExists(report, "Assets/CCS/Modules/NPCs/Runtime/Schedules/CCS_NpcScheduleService.cs");
            ValidateScriptExists(report, "Assets/CCS/Modules/NPCs/Runtime/Schedules/CCS_NpcScheduleRuntimeBridge.cs");
            ValidateScriptExists(report, "Assets/CCS/Modules/NPCs/Runtime/Schedules/CCS_NpcScheduleValidationUtility.cs");
            ValidateScriptExists(report, "Assets/CCS/Survival/Runtime/Schedules/CCS_NpcScheduleLabelBridge.cs");
            ValidateScriptExists(
                report,
                "Assets/CCS/Modules/NPCs/Editor/Validation/CCS_NpcScheduleFoundationBootstrapSetup.cs");
        }

        private static void ValidateScheduleProfile(CCS_SurvivalValidationReport report)
        {
            CCS_NpcScheduleProfile profile = AssetDatabase.LoadAssetAtPath<CCS_NpcScheduleProfile>(
                CCS_NpcScheduleContentIds.DefaultScheduleProfilePath);
            CCS_SurvivalValidationResult validation = CCS_NpcScheduleValidationUtility.ValidateProfile(profile);
            report.AddIssue(
                validation.IsSuccess ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                validation.Message);

            bool workerDefined = profile != null
                && profile.TryGetDefinition(CCS_NpcScheduleContentIds.WorkerScheduleId, out _);
            bool serviceDefined = profile != null
                && profile.TryGetDefinition(CCS_NpcScheduleContentIds.ServiceRepresentativeScheduleId, out _);
            report.AddIssue(
                workerDefined && serviceDefined
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                workerDefined && serviceDefined
                    ? "Default worker and service representative schedules are defined."
                    : "Default schedule definitions missing. Run NPC schedule bootstrap.");
        }

        private static void ValidateWorldProfileWiring(CCS_SurvivalValidationReport report)
        {
            CCS_WorldSimulationProfile profile = AssetDatabase.LoadAssetAtPath<CCS_WorldSimulationProfile>(
                "Assets/CCS/Survival/Profiles/WorldSimulation/CCS_DefaultWorldSimulationProfile.asset");
            bool wired = profile != null && profile.SettlementNpcScheduleProfile != null;
            report.AddIssue(
                wired ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                wired
                    ? "World simulation profile references settlement NPC schedule profile."
                    : "World simulation profile missing settlementNpcScheduleProfile reference.");
        }

        private static void ValidateSimulationState(CCS_SurvivalValidationReport report)
        {
            string source = System.IO.File.ReadAllText(
                "Assets/CCS/Modules/WorldSimulation/Runtime/Data/CCS_SettlementSimulationState.cs");
            bool ok = source.Contains("npcScheduleStates", System.StringComparison.Ordinal);
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                ok
                    ? "Settlement simulation state persists npcScheduleStates."
                    : "Settlement simulation state missing npcScheduleStates array.");
        }

        private static void ValidateRegistration(CCS_SurvivalValidationReport report)
        {
            string source = System.IO.File.Exists(RegistrationPath)
                ? System.IO.File.ReadAllText(RegistrationPath)
                : string.Empty;
            bool ok = source.Contains("CreateNpcScheduleService", System.StringComparison.Ordinal)
                && source.Contains("WireNpcSchedule", System.StringComparison.Ordinal)
                && source.Contains("BindScheduleService", System.StringComparison.Ordinal);
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                ok
                    ? "Gameplay service registration wires NPC schedule service and movement binding."
                    : "Gameplay service registration missing NPC schedule wiring.");
        }

        private static void ValidateMovementFallback(CCS_SurvivalValidationReport report)
        {
            CCS_NpcMovementProfile profile = AssetDatabase.LoadAssetAtPath<CCS_NpcMovementProfile>(
                CCS_NpcMovementContentIds.DefaultMovementProfilePath);
            CCS_SurvivalValidationResult validation = CCS_NpcMovementValidationUtility.ValidateProfile(profile);
            report.AddIssue(
                validation.IsSuccess ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                validation.IsSuccess
                    ? "Movement profile fallback hours remain valid when schedule service is unavailable."
                    : "Movement profile fallback invalid for schedule-off path.");
        }

        private static void ValidatePlaytest(CCS_SurvivalValidationReport report)
        {
            CCS_PlaytestProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(
                "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset");
            ValidateStep(report, profile, CCS_PlaytestStepType.DiscoverSettlementForNpcSchedule);
            ValidateStep(report, profile, CCS_PlaytestStepType.VerifyNpcScheduleAssigned);
            ValidateStep(report, profile, CCS_PlaytestStepType.VerifyNpcScheduleWorkplaceTarget);
            ValidateStep(report, profile, CCS_PlaytestStepType.VerifyNpcScheduleHousingTarget);
            ValidateStep(report, profile, CCS_PlaytestStepType.SaveNpcScheduleState);
            ValidateStep(report, profile, CCS_PlaytestStepType.VerifyNpcScheduleAfterLoad);

            string hudSource = System.IO.File.ReadAllText(
                "Assets/CCS/Modules/Playtesting/Runtime/UI/CCS_PlaytestHud.cs");
            bool shortcut = hudSource.Contains("TryPlaytestNpcScheduleFoundationShortcut", System.StringComparison.Ordinal)
                && hudSource.Contains("WasControlAltPressed(KeyCode.S)", System.StringComparison.Ordinal);
            report.AddIssue(
                shortcut ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                shortcut
                    ? "Playtest HUD exposes Ctrl+Alt+S NPC schedule shortcut."
                    : "Playtest HUD missing Ctrl+Alt+S NPC schedule shortcut.");
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
                    : $"Playtest profile missing step {stepType}. Run NPC schedule bootstrap.");
        }

        private static void ValidateScriptExists(CCS_SurvivalValidationReport report, string assetPath)
        {
            bool exists = System.IO.File.Exists(assetPath);
            report.AddIssue(
                exists ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                exists ? $"Script present: {assetPath}" : $"Missing script: {assetPath}");
        }
    }
}
