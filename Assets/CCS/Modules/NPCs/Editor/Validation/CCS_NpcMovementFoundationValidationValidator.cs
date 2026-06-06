using CCS.Modules.Playtesting;
using CCS.Modules.WorldSimulation;
using CCS.Survival;
using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_NpcMovementFoundationValidationValidator
// CATEGORY: Modules / NPCs / Editor / Validation
// PURPOSE: Validates NPC movement module, profile wiring, and playtest integration.
// PLACEMENT: Registered on CCS_SurvivalValidationPipeline at editor load.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.5.0 NPC movement foundation.
// =============================================================================

namespace CCS.Modules.NPCs.Editor
{
    public sealed class CCS_NpcMovementFoundationValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string ValidatorContext = "ccs.survival.validation.npcmovement";
        private const string MilestoneVersion = "4.5.0";
        private const string RegistrationPath =
            "Assets/CCS/Survival/Runtime/Composition/CCS_SurvivalGameplayServiceRegistration.cs";

        public string ValidatorId => ValidatorContext;

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateScripts(report);
            ValidateMovementProfile(report);
            ValidateWorldProfileWiring(report);
            ValidatePlaceholderIntegration(report);
            ValidateSimulationState(report);
            ValidateRegistration(report);
            ValidatePlaytest(report);

            CCS_SurvivalBootstrapVersionUtility.AddBundleVersionValidationIssue(
                report,
                ValidatorContext,
                MilestoneVersion,
                "Run CCS_NpcMovementFoundationBootstrapSetup.ExecuteBatch.");
            CCS_SurvivalInputValidationUtility.ValidateNoLegacyInputUsage(
                report,
                "NPC Movement Input Policy");
            CCS_SurvivalInputValidationUtility.ValidateNoFindObjectsSortModeUsage(
                report,
                "NPC Movement Obsolete API Scan");
        }

        private static void ValidateScripts(CCS_SurvivalValidationReport report)
        {
            ValidateScriptExists(report, "Assets/CCS/Modules/NPCs/Runtime/Movement/CCS_NpcMovementProfile.cs");
            ValidateScriptExists(report, "Assets/CCS/Modules/NPCs/Runtime/Movement/CCS_NpcMovementService.cs");
            ValidateScriptExists(report, "Assets/CCS/Modules/NPCs/Runtime/Movement/CCS_NpcMovementRuntimeBridge.cs");
            ValidateScriptExists(report, "Assets/CCS/Modules/NPCs/Runtime/Movement/CCS_NpcMovementValidationUtility.cs");
            ValidateScriptExists(report, "Assets/CCS/Survival/Runtime/Movement/CCS_INpcMovementHost.cs");
            ValidateScriptExists(
                report,
                "Assets/CCS/Modules/NPCs/Editor/Validation/CCS_NpcMovementFoundationBootstrapSetup.cs");
        }

        private static void ValidateMovementProfile(CCS_SurvivalValidationReport report)
        {
            CCS_NpcMovementProfile profile = AssetDatabase.LoadAssetAtPath<CCS_NpcMovementProfile>(
                CCS_NpcMovementContentIds.DefaultMovementProfilePath);
            CCS_SurvivalValidationResult validation = CCS_NpcMovementValidationUtility.ValidateProfile(profile);
            report.AddIssue(
                validation.IsSuccess ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                validation.Message);
        }

        private static void ValidateWorldProfileWiring(CCS_SurvivalValidationReport report)
        {
            CCS_WorldSimulationProfile profile = AssetDatabase.LoadAssetAtPath<CCS_WorldSimulationProfile>(
                "Assets/CCS/Survival/Profiles/WorldSimulation/CCS_DefaultWorldSimulationProfile.asset");
            bool wired = profile != null && profile.SettlementNpcMovementProfile != null;
            report.AddIssue(
                wired ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                wired
                    ? "World simulation profile references settlement NPC movement profile."
                    : "World simulation profile missing settlementNpcMovementProfile reference.");
        }

        private static void ValidatePlaceholderIntegration(CCS_SurvivalValidationReport report)
        {
            string source = System.IO.File.ReadAllText(
                "Assets/CCS/Modules/Settlements/Runtime/PopulationPresence/CCS_PopulationPlaceholderActor.cs");
            bool ok = source.Contains("CCS_INpcMovementHost", System.StringComparison.Ordinal)
                && source.Contains("WorkforceAnchorId", System.StringComparison.Ordinal)
                && source.Contains("MovementTransform", System.StringComparison.Ordinal);
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                ok
                    ? "Population placeholder actor implements NPC movement host contract."
                    : "Population placeholder actor missing CCS_INpcMovementHost integration.");
        }

        private static void ValidateSimulationState(CCS_SurvivalValidationReport report)
        {
            string source = System.IO.File.ReadAllText(
                "Assets/CCS/Modules/WorldSimulation/Runtime/Data/CCS_SettlementSimulationState.cs");
            bool ok = source.Contains("npcMovementStates", System.StringComparison.Ordinal);
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                ok
                    ? "Settlement simulation state persists npcMovementStates."
                    : "Settlement simulation state missing npcMovementStates array.");
        }

        private static void ValidateRegistration(CCS_SurvivalValidationReport report)
        {
            string source = System.IO.File.Exists(RegistrationPath)
                ? System.IO.File.ReadAllText(RegistrationPath)
                : string.Empty;
            bool ok = source.Contains("CreateNpcMovementService", System.StringComparison.Ordinal)
                && source.Contains("WireNpcMovement", System.StringComparison.Ordinal)
                && source.Contains("RegisterNpcMovementUpdatable", System.StringComparison.Ordinal);
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                ok
                    ? "Gameplay service registration wires NPC movement service and updatable."
                    : "Gameplay service registration missing NPC movement wiring.");
        }

        private static void ValidatePlaytest(CCS_SurvivalValidationReport report)
        {
            CCS_PlaytestProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(
                "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset");
            ValidateStep(report, profile, CCS_PlaytestStepType.DiscoverSettlementForNpcMovement);
            ValidateStep(report, profile, CCS_PlaytestStepType.VerifyWorkerMovementActive);
            ValidateStep(report, profile, CCS_PlaytestStepType.VerifyRepresentativeMovementActive);
            ValidateStep(report, profile, CCS_PlaytestStepType.SaveNpcMovementState);
            ValidateStep(report, profile, CCS_PlaytestStepType.VerifyNpcMovementAfterLoad);

            string hudSource = System.IO.File.ReadAllText(
                "Assets/CCS/Modules/Playtesting/Runtime/UI/CCS_PlaytestHud.cs");
            bool shortcut = hudSource.Contains("TryPlaytestNpcMovementFoundationShortcut", System.StringComparison.Ordinal)
                && hudSource.Contains("WasControlAltPressed(KeyCode.M)", System.StringComparison.Ordinal);
            report.AddIssue(
                shortcut ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                shortcut
                    ? "Playtest HUD exposes Ctrl+Alt+M NPC movement shortcut."
                    : "Playtest HUD missing Ctrl+Alt+M NPC movement shortcut.");
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
                    : $"Playtest profile missing step {stepType}. Run NPC movement bootstrap.");
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
