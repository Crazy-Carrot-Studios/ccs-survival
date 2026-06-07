using CCS.Modules.Playtesting;
using CCS.Modules.Settlements;
using CCS.Modules.WorldSimulation;
using CCS.Survival;
using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_NpcSocialFoundationValidationValidator
// CATEGORY: Modules / NPCs / Editor / Validation
// PURPOSE: Validates NPC social presence module, anchors, schedule integration, and playtest.
// PLACEMENT: Registered on CCS_SurvivalValidationPipeline at editor load.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 5.0.0 NPC social presence foundation.
// =============================================================================

namespace CCS.Modules.NPCs.Editor
{
    public sealed class CCS_NpcSocialFoundationValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string ValidatorContext = "ccs.survival.validation.npcsocial";
        private const string MilestoneVersion = "5.0.0";
        private const string RegistrationPath =
            "Assets/CCS/Survival/Runtime/Composition/CCS_SurvivalGameplayServiceRegistration.cs";

        public string ValidatorId => ValidatorContext;

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateScripts(report);
            ValidateSocialProfile(report);
            ValidateWorldProfileWiring(report);
            ValidateSimulationState(report);
            ValidateRegistration(report);
            ValidateScheduleIntegration(report);
            ValidateLabelIntegration(report);
            ValidatePlaytest(report);

            CCS_SurvivalBootstrapVersionUtility.AddBundleVersionValidationIssue(
                report,
                ValidatorContext,
                MilestoneVersion,
                "Run CCS_NpcSocialFoundationBootstrapSetup.ExecuteBatch.");
            CCS_SurvivalInputValidationUtility.ValidateNoLegacyInputUsage(
                report,
                "NPC Social Input Policy");
            CCS_SurvivalInputValidationUtility.ValidateNoFindObjectsSortModeUsage(
                report,
                "NPC Social Obsolete API Scan");
        }

        private static void ValidateScripts(CCS_SurvivalValidationReport report)
        {
            ValidateScriptExists(report, "Assets/CCS/Modules/NPCs/Runtime/Social/CCS_NpcSocialProfile.cs");
            ValidateScriptExists(report, "Assets/CCS/Modules/NPCs/Runtime/Social/CCS_NpcSocialService.cs");
            ValidateScriptExists(report, "Assets/CCS/Modules/NPCs/Runtime/Social/CCS_NpcSocialRuntimeBridge.cs");
            ValidateScriptExists(report, "Assets/CCS/Modules/NPCs/Runtime/Social/CCS_NpcSocialValidationUtility.cs");
            ValidateScriptExists(report, "Assets/CCS/Modules/Settlements/Runtime/Social/CCS_SettlementSocialAnchor.cs");
            ValidateScriptExists(report, "Assets/CCS/Survival/Runtime/Social/CCS_NpcSocialLabelBridge.cs");
            ValidateScriptExists(
                report,
                "Assets/CCS/Modules/NPCs/Editor/Validation/CCS_NpcSocialFoundationBootstrapSetup.cs");
        }

        private static void ValidateSocialProfile(CCS_SurvivalValidationReport report)
        {
            CCS_NpcSocialProfile profile = AssetDatabase.LoadAssetAtPath<CCS_NpcSocialProfile>(
                CCS_NpcSocialContentIds.DefaultSocialProfilePath);
            CCS_SurvivalValidationResult validation = CCS_NpcSocialValidationUtility.ValidateProfile(profile);
            report.AddIssue(
                validation.IsSuccess ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                validation.Message);
        }

        private static void ValidateWorldProfileWiring(CCS_SurvivalValidationReport report)
        {
            CCS_WorldSimulationProfile profile = AssetDatabase.LoadAssetAtPath<CCS_WorldSimulationProfile>(
                "Assets/CCS/Survival/Profiles/WorldSimulation/CCS_DefaultWorldSimulationProfile.asset");
            bool wired = profile != null && profile.SettlementNpcSocialProfile != null;
            report.AddIssue(
                wired ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                wired
                    ? "World simulation profile references settlement NPC social profile."
                    : "World simulation profile missing settlementNpcSocialProfile reference.");
        }

        private static void ValidateSimulationState(CCS_SurvivalValidationReport report)
        {
            string source = System.IO.File.ReadAllText(
                "Assets/CCS/Modules/WorldSimulation/Runtime/Data/CCS_SettlementSimulationState.cs");
            bool ok = source.Contains("npcSocialStates", System.StringComparison.Ordinal);
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                ok
                    ? "Settlement simulation state persists npcSocialStates."
                    : "Settlement simulation state missing npcSocialStates array.");
        }

        private static void ValidateRegistration(CCS_SurvivalValidationReport report)
        {
            string source = System.IO.File.Exists(RegistrationPath)
                ? System.IO.File.ReadAllText(RegistrationPath)
                : string.Empty;
            bool ok = source.Contains("CreateNpcSocialService", System.StringComparison.Ordinal)
                && source.Contains("WireNpcSocial", System.StringComparison.Ordinal)
                && source.Contains("SetSocialStates", System.StringComparison.Ordinal);
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                ok
                    ? "Gameplay service registration wires NPC social service and persistence."
                    : "Gameplay service registration missing NPC social wiring.");
        }

        private static void ValidateScheduleIntegration(CCS_SurvivalValidationReport report)
        {
            string scheduleSource = System.IO.File.ReadAllText(
                "Assets/CCS/Modules/NPCs/Runtime/Schedules/CCS_NpcScheduleValidationUtility.cs");
            bool leisureSocial = scheduleSource.Contains("CCS_NpcScheduleBlockType.Leisure", System.StringComparison.Ordinal)
                && scheduleSource.Contains("CCS_NpcScheduleTargetKind.SocialAnchor", System.StringComparison.Ordinal)
                && scheduleSource.Contains("TryResolveSocialAnchorTarget", System.StringComparison.Ordinal);
            report.AddIssue(
                leisureSocial ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                leisureSocial
                    ? "Leisure schedule blocks resolve nearest social anchor targets."
                    : "Schedule validation missing leisure social anchor integration.");
        }

        private static void ValidateLabelIntegration(CCS_SurvivalValidationReport report)
        {
            string source = System.IO.File.ReadAllText(
                "Assets/CCS/Modules/Settlements/Runtime/PopulationPresence/CCS_PopulationPlaceholderActor.cs");
            bool ok = source.Contains("CCS_NpcSocialLabelBridge", System.StringComparison.Ordinal)
                && source.Contains("BuildSocialDisplayLine", System.StringComparison.Ordinal);
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                ok
                    ? "Population placeholder actor shows socializing label line during leisure gatherings."
                    : "Population placeholder actor missing NPC social label integration.");
        }

        private static void ValidatePlaytest(CCS_SurvivalValidationReport report)
        {
            CCS_PlaytestProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(
                "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset");
            ValidateStep(report, profile, CCS_PlaytestStepType.DiscoverSettlementForNpcSocial);
            ValidateStep(report, profile, CCS_PlaytestStepType.VerifyWorkersGatherForNpcSocial);
            ValidateStep(report, profile, CCS_PlaytestStepType.VerifyNpcSocialGroupCount);
            ValidateStep(report, profile, CCS_PlaytestStepType.SaveNpcSocialState);
            ValidateStep(report, profile, CCS_PlaytestStepType.VerifyNpcSocialAfterLoad);

            string hudSource = System.IO.File.ReadAllText(
                "Assets/CCS/Modules/Playtesting/Runtime/UI/CCS_PlaytestHud.cs");
            bool shortcut = hudSource.Contains("TryPlaytestNpcSocialFoundationShortcut", System.StringComparison.Ordinal)
                && hudSource.Contains("WasControlAltPressed(KeyCode.P)", System.StringComparison.Ordinal);
            report.AddIssue(
                shortcut ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                shortcut
                    ? "Playtest HUD exposes Ctrl+Alt+P NPC social presence shortcut."
                    : "Playtest HUD missing Ctrl+Alt+P NPC social presence shortcut.");
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
                    : $"Playtest profile missing step {stepType}. Run NPC social bootstrap.");
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
