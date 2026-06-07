using CCS.Modules.Playtesting;
using CCS.Modules.WorldSimulation;
using CCS.Survival;
using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_NpcAffiliationFoundationValidationValidator
// CATEGORY: Modules / NPCs / Editor / Validation
// PURPOSE: Validates NPC affiliation module, persistence, labels, and playtest.
// PLACEMENT: Registered on CCS_SurvivalValidationPipeline at editor load.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.8.0 NPC settlement affiliation foundation.
// =============================================================================

namespace CCS.Modules.NPCs.Editor
{
    public sealed class CCS_NpcAffiliationFoundationValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string ValidatorContext = "ccs.survival.validation.npcaffiliation";
        private const string MilestoneVersion = "4.8.0";
        private const string RegistrationPath =
            "Assets/CCS/Survival/Runtime/Composition/CCS_SurvivalGameplayServiceRegistration.cs";

        public string ValidatorId => ValidatorContext;

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateScripts(report);
            ValidateAffiliationProfile(report);
            ValidateWorldProfileWiring(report);
            ValidateSimulationState(report);
            ValidateRegistration(report);
            ValidateLabelIntegration(report);
            ValidatePlaytest(report);

            CCS_SurvivalBootstrapVersionUtility.AddBundleVersionValidationIssue(
                report,
                ValidatorContext,
                MilestoneVersion,
                "Run CCS_NpcAffiliationFoundationBootstrapSetup.ExecuteBatch.");
            CCS_SurvivalInputValidationUtility.ValidateNoLegacyInputUsage(
                report,
                "NPC Affiliation Input Policy");
            CCS_SurvivalInputValidationUtility.ValidateNoFindObjectsSortModeUsage(
                report,
                "NPC Affiliation Obsolete API Scan");
        }

        private static void ValidateScripts(CCS_SurvivalValidationReport report)
        {
            ValidateScriptExists(report, "Assets/CCS/Modules/NPCs/Runtime/Affiliations/CCS_NpcAffiliationProfile.cs");
            ValidateScriptExists(report, "Assets/CCS/Modules/NPCs/Runtime/Affiliations/CCS_NpcAffiliationService.cs");
            ValidateScriptExists(report, "Assets/CCS/Modules/NPCs/Runtime/Affiliations/CCS_NpcAffiliationRuntimeBridge.cs");
            ValidateScriptExists(report, "Assets/CCS/Modules/NPCs/Runtime/Affiliations/CCS_NpcAffiliationValidationUtility.cs");
            ValidateScriptExists(report, "Assets/CCS/Survival/Runtime/Affiliations/CCS_NpcAffiliationLabelBridge.cs");
            ValidateScriptExists(
                report,
                "Assets/CCS/Modules/NPCs/Editor/Validation/CCS_NpcAffiliationFoundationBootstrapSetup.cs");
        }

        private static void ValidateAffiliationProfile(CCS_SurvivalValidationReport report)
        {
            CCS_NpcAffiliationProfile profile = AssetDatabase.LoadAssetAtPath<CCS_NpcAffiliationProfile>(
                CCS_NpcAffiliationContentIds.DefaultAffiliationProfilePath);
            CCS_SurvivalValidationResult validation = CCS_NpcAffiliationValidationUtility.ValidateProfile(profile);
            report.AddIssue(
                validation.IsSuccess ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                validation.Message);
        }

        private static void ValidateWorldProfileWiring(CCS_SurvivalValidationReport report)
        {
            CCS_WorldSimulationProfile profile = AssetDatabase.LoadAssetAtPath<CCS_WorldSimulationProfile>(
                "Assets/CCS/Survival/Profiles/WorldSimulation/CCS_DefaultWorldSimulationProfile.asset");
            bool wired = profile != null && profile.SettlementNpcAffiliationProfile != null;
            report.AddIssue(
                wired ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                wired
                    ? "World simulation profile references settlement NPC affiliation profile."
                    : "World simulation profile missing settlementNpcAffiliationProfile reference.");
        }

        private static void ValidateSimulationState(CCS_SurvivalValidationReport report)
        {
            string source = System.IO.File.ReadAllText(
                "Assets/CCS/Modules/WorldSimulation/Runtime/Data/CCS_SettlementSimulationState.cs");
            bool ok = source.Contains("npcAffiliationStates", System.StringComparison.Ordinal);
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                ok
                    ? "Settlement simulation state persists npcAffiliationStates."
                    : "Settlement simulation state missing npcAffiliationStates array.");
        }

        private static void ValidateRegistration(CCS_SurvivalValidationReport report)
        {
            string source = System.IO.File.Exists(RegistrationPath)
                ? System.IO.File.ReadAllText(RegistrationPath)
                : string.Empty;
            bool ok = source.Contains("CreateNpcAffiliationService", System.StringComparison.Ordinal)
                && source.Contains("WireNpcAffiliation", System.StringComparison.Ordinal)
                && source.Contains("SetAffiliationStates", System.StringComparison.Ordinal);
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                ok
                    ? "Gameplay service registration wires NPC affiliation service and persistence."
                    : "Gameplay service registration missing NPC affiliation wiring.");
        }

        private static void ValidateLabelIntegration(CCS_SurvivalValidationReport report)
        {
            string source = System.IO.File.ReadAllText(
                "Assets/CCS/Modules/Settlements/Runtime/PopulationPresence/CCS_PopulationPlaceholderActor.cs");
            bool ok = source.Contains("CCS_NpcAffiliationLabelBridge", System.StringComparison.Ordinal)
                && source.Contains("BuildSettlementDisplayLine", System.StringComparison.Ordinal)
                && source.Contains("BuildAffiliationDebugLine", System.StringComparison.Ordinal)
                && source.Contains("BuildAffiliationDetailDebugLine", System.StringComparison.Ordinal);
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                ok
                    ? "Population placeholder actor shows settlement affiliation labels and debug HUD lines."
                    : "Population placeholder actor missing NPC affiliation presentation integration.");
        }

        private static void ValidatePlaytest(CCS_SurvivalValidationReport report)
        {
            CCS_PlaytestProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(
                "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset");
            ValidateStep(report, profile, CCS_PlaytestStepType.DiscoverSettlementForNpcAffiliation);
            ValidateStep(report, profile, CCS_PlaytestStepType.VerifyNpcSettlementAffiliation);
            ValidateStep(report, profile, CCS_PlaytestStepType.VerifyNpcRepresentativeAffiliation);
            ValidateStep(report, profile, CCS_PlaytestStepType.SaveNpcAffiliationState);
            ValidateStep(report, profile, CCS_PlaytestStepType.VerifyNpcAffiliationAfterLoad);

            string hudSource = System.IO.File.ReadAllText(
                "Assets/CCS/Modules/Playtesting/Runtime/UI/CCS_PlaytestHud.cs");
            bool shortcut = hudSource.Contains("TryPlaytestNpcAffiliationFoundationShortcut", System.StringComparison.Ordinal)
                && hudSource.Contains("WasControlAltPressed(KeyCode.F)", System.StringComparison.Ordinal);
            report.AddIssue(
                shortcut ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                shortcut
                    ? "Playtest HUD exposes Ctrl+Alt+F NPC affiliation shortcut."
                    : "Playtest HUD missing Ctrl+Alt+F NPC affiliation shortcut.");
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
                    : $"Playtest profile missing step {stepType}. Run NPC affiliation bootstrap.");
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
