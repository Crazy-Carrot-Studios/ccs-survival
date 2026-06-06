using CCS.Modules.Playtesting;
using CCS.Modules.Settlements;
using CCS.Modules.WorldSimulation;
using CCS.Survival;
using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_NpcIdentityFoundationValidationValidator
// CATEGORY: Modules / NPCs / Editor / Validation
// PURPOSE: Validates NPC identity module, profile, wiring, and playtest harness.
// PLACEMENT: Registered on CCS_SurvivalValidationPipeline at editor load.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 4.1.0 NPC identity and role foundation.
// =============================================================================

namespace CCS.Modules.NPCs.Editor
{
    public sealed class CCS_NpcIdentityFoundationValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string ValidatorContext = "ccs.survival.validation.npcidentity";
        private const string MilestoneVersion = "4.1.0";
        private const string RegistrationPath =
            "Assets/CCS/Survival/Runtime/Composition/CCS_SurvivalGameplayServiceRegistration.cs";

        public string ValidatorId => ValidatorContext;

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateScripts(report);
            ValidateIdentityProfile(report);
            ValidateWorldProfileWiring(report);
            ValidatePlaceholderIntegration(report);
            ValidateUtility(report);
            ValidateRegistration(report);
            ValidatePlaytest(report);

            CCS_SurvivalBootstrapVersionUtility.AddBundleVersionValidationIssue(
                report,
                ValidatorContext,
                MilestoneVersion,
                "Run CCS_NpcIdentityFoundationBootstrapSetup.ExecuteBatch.");
            CCS_SurvivalInputValidationUtility.ValidateNoLegacyInputUsage(report, "NPC Identity Input Policy");
            CCS_SurvivalInputValidationUtility.ValidateNoFindObjectsSortModeUsage(
                report,
                "NPC Identity Obsolete API Scan");
        }

        private static void ValidateScripts(CCS_SurvivalValidationReport report)
        {
            ValidateScriptExists(report, "Assets/CCS/Modules/NPCs/Runtime/Data/CCS_NpcIdentityProfile.cs");
            ValidateScriptExists(report, "Assets/CCS/Modules/NPCs/Runtime/Services/CCS_NpcIdentityService.cs");
            ValidateScriptExists(report, "Assets/CCS/Modules/NPCs/Runtime/Services/CCS_NpcRuntimeBridge.cs");
            ValidateScriptExists(report, "Assets/CCS/Modules/NPCs/Runtime/Validation/CCS_NpcIdentityValidationUtility.cs");
            ValidateScriptExists(
                report,
                "Assets/CCS/Modules/Settlements/Runtime/PopulationPresence/CCS_PopulationPlaceholderActor.cs");
            ValidateScriptExists(
                report,
                "Assets/CCS/Modules/NPCs/Editor/Validation/CCS_NpcIdentityFoundationBootstrapSetup.cs");
            ValidateScriptExists(report, "Assets/CCS/Modules/NPCs/Documentation/CCS_Npc_Module.md");
        }

        private static void ValidateIdentityProfile(CCS_SurvivalValidationReport report)
        {
            CCS_NpcIdentityProfile profile = AssetDatabase.LoadAssetAtPath<CCS_NpcIdentityProfile>(
                CCS_NpcIdentityContentIds.DefaultIdentityProfilePath);
            CCS_SurvivalValidationResult validation = CCS_NpcIdentityValidationUtility.ValidateProfile(profile);
            report.AddIssue(
                validation.IsSuccess ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                validation.Message);
        }

        private static void ValidateWorldProfileWiring(CCS_SurvivalValidationReport report)
        {
            CCS_WorldSimulationProfile worldProfile = AssetDatabase.LoadAssetAtPath<CCS_WorldSimulationProfile>(
                "Assets/CCS/Survival/Profiles/WorldSimulation/CCS_DefaultWorldSimulationProfile.asset");
            CCS_NpcIdentityProfile identityProfile = AssetDatabase.LoadAssetAtPath<CCS_NpcIdentityProfile>(
                CCS_NpcIdentityContentIds.DefaultIdentityProfilePath);
            bool wired = worldProfile != null && worldProfile.SettlementNpcIdentityProfile == identityProfile;
            report.AddIssue(
                wired ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                wired
                    ? "World simulation profile references NPC identity profile."
                    : "World simulation profile missing NPC identity profile reference.");
        }

        private static void ValidatePlaceholderIntegration(CCS_SurvivalValidationReport report)
        {
            string source = System.IO.File.ReadAllText(
                "Assets/CCS/Modules/Settlements/Runtime/PopulationPresence/CCS_PopulationPlaceholderActor.cs");
            bool integrated = source.Contains("npcIdentityId", System.StringComparison.Ordinal)
                && source.Contains("ApplyIdentityData", System.StringComparison.Ordinal)
                && source.Contains("CCS_PopulationPlaceholderIdentityBridge", System.StringComparison.Ordinal);
            report.AddIssue(
                integrated ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                integrated
                    ? "Population placeholder actor integrates NPC identity fields and bridge."
                    : "Population placeholder actor missing NPC identity integration.");
        }

        private static void ValidateUtility(CCS_SurvivalValidationReport report)
        {
            CCS_NpcIdentityProfile identityProfile = AssetDatabase.LoadAssetAtPath<CCS_NpcIdentityProfile>(
                CCS_NpcIdentityContentIds.DefaultIdentityProfilePath);
            CCS_NpcRoleType role = CCS_NpcIdentityValidationUtility.ResolveRole(
                identityProfile,
                CCS_SettlementGrowthContentIds.TradingPostSettlementId,
                CCS_SettlementPopulationCategory.Merchants,
                "ccs.survival.business.generalstore");
            bool fallbackOk = role == CCS_NpcRoleType.Merchant;
            report.AddIssue(
                fallbackOk ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                fallbackOk
                    ? "NPC identity utility resolves merchant workforce fallback role."
                    : "NPC identity utility workforce fallback failed.");

            CCS_NpcIdentityState[] states =
            {
                new CCS_NpcIdentityState { npcIdentityId = "ccs.survival.npc.test.a" },
                new CCS_NpcIdentityState { npcIdentityId = "ccs.survival.npc.test.b" }
            };
            CCS_SurvivalValidationResult persistValidation =
                CCS_NpcIdentityValidationUtility.ValidatePersistedStates(states);
            report.AddIssue(
                persistValidation.IsSuccess
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                persistValidation.Message);

            string stateSource = System.IO.File.ReadAllText(
                "Assets/CCS/Modules/WorldSimulation/Runtime/Data/CCS_SettlementSimulationState.cs");
            bool saveField = stateSource.Contains("npcIdentityStates", System.StringComparison.Ordinal);
            report.AddIssue(
                saveField ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                saveField
                    ? "Settlement simulation state persists npcIdentityStates."
                    : "Settlement simulation state missing npcIdentityStates save field.");
        }

        private static void ValidateRegistration(CCS_SurvivalValidationReport report)
        {
            string source = System.IO.File.ReadAllText(RegistrationPath);
            bool wired = source.Contains("CreateNpcIdentityService", System.StringComparison.Ordinal)
                && source.Contains("WireNpcIdentity", System.StringComparison.Ordinal);
            report.AddIssue(
                wired ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                wired
                    ? "Gameplay registration wires NPC identity service."
                    : "Gameplay registration missing NPC identity wiring.");
        }

        private static void ValidatePlaytest(CCS_SurvivalValidationReport report)
        {
            string hudSource = System.IO.File.ReadAllText(
                "Assets/CCS/Modules/Playtesting/Runtime/UI/CCS_PlaytestHud.cs");
            bool shortcut = hudSource.Contains("TryPlaytestNpcIdentityFoundationShortcut", System.StringComparison.Ordinal)
                && hudSource.Contains("WasControlShiftPressed(KeyCode.E)", System.StringComparison.Ordinal);
            report.AddIssue(
                shortcut ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                shortcut
                    ? "Playtest HUD exposes Ctrl+Shift+E NPC identity shortcut (Ctrl+Shift+I reserved)."
                    : "Playtest HUD missing Ctrl+Shift+E NPC identity shortcut.");

            CCS_PlaytestProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(
                "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset");
            if (profile == null)
            {
                return;
            }

            ValidateStep(report, profile, CCS_PlaytestStepType.VerifyPlaceholderActorHasIdentity);
            ValidateStep(report, profile, CCS_PlaytestStepType.VerifyNpcIdentityAfterLoad);
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
                    : $"Playtest profile missing step {stepType}. Run NPC identity bootstrap.");
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
