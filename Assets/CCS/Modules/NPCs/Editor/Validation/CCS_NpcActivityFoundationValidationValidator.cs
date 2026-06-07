using CCS.Modules.Playtesting;
using CCS.Modules.WorldSimulation;
using CCS.Survival;
using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_NpcActivityFoundationValidationValidator
// CATEGORY: Modules / NPCs / Editor / Validation
// PURPOSE: Validates NPC activity module, mappings, fallback safety, and playtest.
// PLACEMENT: Registered on CCS_SurvivalValidationPipeline at editor load.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.7.0 NPC activity state foundation.
// =============================================================================

namespace CCS.Modules.NPCs.Editor
{
    public sealed class CCS_NpcActivityFoundationValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string ValidatorContext = "ccs.survival.validation.npcactivity";
        private const string MilestoneVersion = "4.7.0";
        private const string RegistrationPath =
            "Assets/CCS/Survival/Runtime/Composition/CCS_SurvivalGameplayServiceRegistration.cs";

        public string ValidatorId => ValidatorContext;

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateScripts(report);
            ValidateActivityProfile(report);
            ValidateWorldProfileWiring(report);
            ValidateSimulationState(report);
            ValidateRegistration(report);
            ValidateFallbackSafety(report);
            ValidatePlaceholderIntegration(report);
            ValidatePlaytest(report);

            CCS_SurvivalBootstrapVersionUtility.AddBundleVersionValidationIssue(
                report,
                ValidatorContext,
                MilestoneVersion,
                "Run CCS_NpcActivityFoundationBootstrapSetup.ExecuteBatch.");
            CCS_SurvivalInputValidationUtility.ValidateNoLegacyInputUsage(
                report,
                "NPC Activity Input Policy");
            CCS_SurvivalInputValidationUtility.ValidateNoFindObjectsSortModeUsage(
                report,
                "NPC Activity Obsolete API Scan");
        }

        private static void ValidateScripts(CCS_SurvivalValidationReport report)
        {
            ValidateScriptExists(report, "Assets/CCS/Modules/NPCs/Runtime/Activities/CCS_NpcActivityProfile.cs");
            ValidateScriptExists(report, "Assets/CCS/Modules/NPCs/Runtime/Activities/CCS_NpcActivityService.cs");
            ValidateScriptExists(report, "Assets/CCS/Modules/NPCs/Runtime/Activities/CCS_NpcActivityRuntimeBridge.cs");
            ValidateScriptExists(report, "Assets/CCS/Modules/NPCs/Runtime/Activities/CCS_NpcActivityValidationUtility.cs");
            ValidateScriptExists(report, "Assets/CCS/Survival/Runtime/Activities/CCS_NpcActivityLabelBridge.cs");
            ValidateScriptExists(report, "Assets/CCS/Survival/Runtime/Activities/CCS_INpcPresentationHost.cs");
            ValidateScriptExists(
                report,
                "Assets/CCS/Modules/NPCs/Editor/Validation/CCS_NpcActivityFoundationBootstrapSetup.cs");
        }

        private static void ValidateActivityProfile(CCS_SurvivalValidationReport report)
        {
            CCS_NpcActivityProfile profile = AssetDatabase.LoadAssetAtPath<CCS_NpcActivityProfile>(
                CCS_NpcActivityContentIds.DefaultActivityProfilePath);
            CCS_SurvivalValidationResult validation = CCS_NpcActivityValidationUtility.ValidateProfile(profile);
            report.AddIssue(
                validation.IsSuccess ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                validation.Message);
        }

        private static void ValidateWorldProfileWiring(CCS_SurvivalValidationReport report)
        {
            CCS_WorldSimulationProfile profile = AssetDatabase.LoadAssetAtPath<CCS_WorldSimulationProfile>(
                "Assets/CCS/Survival/Profiles/WorldSimulation/CCS_DefaultWorldSimulationProfile.asset");
            bool wired = profile != null && profile.SettlementNpcActivityProfile != null;
            report.AddIssue(
                wired ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                wired
                    ? "World simulation profile references settlement NPC activity profile."
                    : "World simulation profile missing settlementNpcActivityProfile reference.");
        }

        private static void ValidateSimulationState(CCS_SurvivalValidationReport report)
        {
            string source = System.IO.File.ReadAllText(
                "Assets/CCS/Modules/WorldSimulation/Runtime/Data/CCS_SettlementSimulationState.cs");
            bool ok = source.Contains("npcActivityStates", System.StringComparison.Ordinal);
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                ok
                    ? "Settlement simulation state persists npcActivityStates."
                    : "Settlement simulation state missing npcActivityStates array.");
        }

        private static void ValidateRegistration(CCS_SurvivalValidationReport report)
        {
            string source = System.IO.File.Exists(RegistrationPath)
                ? System.IO.File.ReadAllText(RegistrationPath)
                : string.Empty;
            bool ok = source.Contains("CreateNpcActivityService", System.StringComparison.Ordinal)
                && source.Contains("WireNpcActivity", System.StringComparison.Ordinal)
                && source.Contains("BindActivityHostUpdatedCallback", System.StringComparison.Ordinal);
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                ok
                    ? "Gameplay service registration wires NPC activity service and movement callback."
                    : "Gameplay service registration missing NPC activity wiring.");
        }

        private static void ValidateFallbackSafety(CCS_SurvivalValidationReport report)
        {
            CCS_NpcActivityProfile profile = AssetDatabase.LoadAssetAtPath<CCS_NpcActivityProfile>(
                CCS_NpcActivityContentIds.DefaultActivityProfilePath);
            CCS_NpcActivityType scheduleMissing = CCS_NpcActivityValidationUtility.ResolveActivity(
                profile,
                CCS_NpcScheduleBlockType.Unknown,
                CCS_NpcMovementStatus.Unknown,
                false,
                false);
            CCS_NpcActivityType movementMissing = CCS_NpcActivityValidationUtility.ResolveActivity(
                profile,
                CCS_NpcScheduleBlockType.Work,
                CCS_NpcMovementStatus.Unknown,
                true,
                false);
            CCS_NpcActivityType traveling = CCS_NpcActivityValidationUtility.ResolveActivity(
                profile,
                CCS_NpcScheduleBlockType.Work,
                CCS_NpcMovementStatus.TravelingToWork,
                true,
                true);
            bool ok = scheduleMissing != CCS_NpcActivityType.None
                && movementMissing != CCS_NpcActivityType.None
                && traveling == CCS_NpcActivityType.Traveling;
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                ok
                    ? "Activity resolution handles missing schedule/movement services and traveling override safely."
                    : "Activity fallback or traveling override resolution is unsafe.");
        }

        private static void ValidatePlaceholderIntegration(CCS_SurvivalValidationReport report)
        {
            string source = System.IO.File.ReadAllText(
                "Assets/CCS/Modules/Settlements/Runtime/PopulationPresence/CCS_PopulationPlaceholderActor.cs");
            bool ok = source.Contains("CCS_INpcPresentationHost", System.StringComparison.Ordinal)
                && source.Contains("CCS_NpcActivityLabelBridge", System.StringComparison.Ordinal)
                && source.Contains("CCS_NpcActivity_Indicator", System.StringComparison.Ordinal);
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                ok
                    ? "Population placeholder actor shows activity labels and primitive indicator."
                    : "Population placeholder actor missing NPC activity presentation integration.");
        }

        private static void ValidatePlaytest(CCS_SurvivalValidationReport report)
        {
            CCS_PlaytestProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(
                "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset");
            ValidateStep(report, profile, CCS_PlaytestStepType.DiscoverSettlementForNpcActivity);
            ValidateStep(report, profile, CCS_PlaytestStepType.VerifyNpcActivityWorkingOrServing);
            ValidateStep(report, profile, CCS_PlaytestStepType.VerifyNpcActivityTraveling);
            ValidateStep(report, profile, CCS_PlaytestStepType.SaveNpcActivityState);
            ValidateStep(report, profile, CCS_PlaytestStepType.VerifyNpcActivityAfterLoad);

            string hudSource = System.IO.File.ReadAllText(
                "Assets/CCS/Modules/Playtesting/Runtime/UI/CCS_PlaytestHud.cs");
            bool shortcut = hudSource.Contains("TryPlaytestNpcActivityFoundationShortcut", System.StringComparison.Ordinal)
                && hudSource.Contains("WasControlAltPressed(KeyCode.A)", System.StringComparison.Ordinal);
            report.AddIssue(
                shortcut ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                shortcut
                    ? "Playtest HUD exposes Ctrl+Alt+A NPC activity shortcut."
                    : "Playtest HUD missing Ctrl+Alt+A NPC activity shortcut.");
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
                    : $"Playtest profile missing step {stepType}. Run NPC activity bootstrap.");
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
