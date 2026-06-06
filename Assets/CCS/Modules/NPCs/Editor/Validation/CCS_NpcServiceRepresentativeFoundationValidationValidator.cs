using CCS.Modules.Playtesting;
using CCS.Modules.Settlements;
using CCS.Modules.WorldSimulation;
using CCS.Survival;
using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_NpcServiceRepresentativeFoundationValidationValidator
// CATEGORY: Modules / NPCs / Editor / Validation
// PURPOSE: Validates NPC service representative module, profile, wiring, and playtest.
// PLACEMENT: Registered on CCS_SurvivalValidationPipeline at editor load.
// AUTHOR: James Schilz
// CREATED: 2026-06-05
// NOTES: Milestone 4.3.0 NPC service representatives foundation.
// =============================================================================

namespace CCS.Modules.NPCs.Editor
{
    public sealed class CCS_NpcServiceRepresentativeFoundationValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string ValidatorContext = "ccs.survival.validation.npcservicerepresentative";
        private const string MilestoneVersion = "4.3.0";
        private const string RegistrationPath =
            "Assets/CCS/Survival/Runtime/Composition/CCS_SurvivalGameplayServiceRegistration.cs";

        public string ValidatorId => ValidatorContext;

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateScripts(report);
            ValidateRepresentativeProfile(report);
            ValidateWorldProfileWiring(report);
            ValidatePlaceholderIntegration(report);
            ValidateUtility(report);
            ValidateRegistration(report);
            ValidateServicePointFallback(report);
            ValidatePlaytest(report);

            CCS_SurvivalBootstrapVersionUtility.AddBundleVersionValidationIssue(
                report,
                ValidatorContext,
                MilestoneVersion,
                "Run CCS_NpcServiceRepresentativeFoundationBootstrapSetup.ExecuteBatch.");
            CCS_SurvivalInputValidationUtility.ValidateNoLegacyInputUsage(
                report,
                "NPC Service Representative Input Policy");
            CCS_SurvivalInputValidationUtility.ValidateNoFindObjectsSortModeUsage(
                report,
                "NPC Service Representative Obsolete API Scan");
        }

        private static void ValidateScripts(CCS_SurvivalValidationReport report)
        {
            ValidateScriptExists(report, "Assets/CCS/Modules/NPCs/Runtime/Data/CCS_NpcServiceRepresentativeProfile.cs");
            ValidateScriptExists(report, "Assets/CCS/Modules/NPCs/Runtime/Services/CCS_NpcServiceRepresentativeService.cs");
            ValidateScriptExists(report, "Assets/CCS/Modules/NPCs/Runtime/Services/CCS_NpcServiceRepresentativeRuntimeBridge.cs");
            ValidateScriptExists(report, "Assets/CCS/Modules/NPCs/Runtime/Components/CCS_NpcServiceRepresentativeInteractable.cs");
            ValidateScriptExists(report, "Assets/CCS/Modules/NPCs/Runtime/Validation/CCS_NpcServiceRepresentativeValidationUtility.cs");
            ValidateScriptExists(report, "Assets/CCS/Modules/NPCs/Runtime/Validation/CCS_NpcServiceRepresentativeUtility.cs");
            ValidateScriptExists(report, "Assets/CCS/Modules/Settlements/Runtime/Services/CCS_SettlementServicePointRuntimeBridge.cs");
            ValidateScriptExists(
                report,
                "Assets/CCS/Modules/NPCs/Editor/Validation/CCS_NpcServiceRepresentativeFoundationBootstrapSetup.cs");
            ValidateScriptExists(report, "Assets/CCS/Modules/NPCs/Documentation/CCS_Npc_Module.md");
        }

        private static void ValidateRepresentativeProfile(CCS_SurvivalValidationReport report)
        {
            CCS_NpcServiceRepresentativeProfile profile = AssetDatabase.LoadAssetAtPath<CCS_NpcServiceRepresentativeProfile>(
                CCS_NpcServiceRepresentativeContentIds.DefaultRepresentativeProfilePath);
            CCS_SurvivalValidationResult validation =
                CCS_NpcServiceRepresentativeValidationUtility.ValidateProfile(profile);
            report.AddIssue(
                validation.IsSuccess ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                validation.Message);
        }

        private static void ValidateWorldProfileWiring(CCS_SurvivalValidationReport report)
        {
            CCS_WorldSimulationProfile worldProfile = AssetDatabase.LoadAssetAtPath<CCS_WorldSimulationProfile>(
                "Assets/CCS/Survival/Profiles/WorldSimulation/CCS_DefaultWorldSimulationProfile.asset");
            CCS_NpcServiceRepresentativeProfile representativeProfile =
                AssetDatabase.LoadAssetAtPath<CCS_NpcServiceRepresentativeProfile>(
                    CCS_NpcServiceRepresentativeContentIds.DefaultRepresentativeProfilePath);
            bool wired = worldProfile != null
                && worldProfile.SettlementNpcServiceRepresentativeProfile == representativeProfile;
            report.AddIssue(
                wired ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                wired
                    ? "World simulation profile references NPC service representative profile."
                    : "World simulation profile missing NPC service representative profile reference.");
        }

        private static void ValidatePlaceholderIntegration(CCS_SurvivalValidationReport report)
        {
            string source = System.IO.File.ReadAllText(
                "Assets/CCS/Modules/Settlements/Runtime/PopulationPresence/CCS_PopulationPlaceholderActor.cs");
            bool integrated = source.Contains("IsServiceRepresentative", System.StringComparison.Ordinal)
                && source.Contains("ApplyServiceRepresentativePresentation", System.StringComparison.Ordinal)
                && source.Contains("RepresentativeTitle", System.StringComparison.Ordinal);
            report.AddIssue(
                integrated ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                integrated
                    ? "Population placeholder actor integrates service representative label mode."
                    : "Population placeholder actor missing service representative integration.");
        }

        private static void ValidateUtility(CCS_SurvivalValidationReport report)
        {
            CCS_SettlementServiceRouteType merchantRoute =
                CCS_NpcServiceRepresentativeUtility.ResolveRouteType(CCS_NpcRoleType.Merchant);
            bool merchantOk = merchantRoute == CCS_SettlementServiceRouteType.Vendor;
            report.AddIssue(
                merchantOk ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                merchantOk
                    ? "Representative utility maps Merchant role to Vendor route."
                    : "Representative utility Merchant route mapping failed.");

            CCS_SettlementServiceRouteType bankerRoute =
                CCS_NpcServiceRepresentativeUtility.ResolveRouteType(CCS_NpcRoleType.Banker);
            bool bankerOk = bankerRoute == CCS_SettlementServiceRouteType.Bank;
            report.AddIssue(
                bankerOk ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                bankerOk
                    ? "Representative utility maps Banker role to Bank route."
                    : "Representative utility Banker route mapping failed.");

            CCS_NpcServiceRepresentativeState[] states =
            {
                new CCS_NpcServiceRepresentativeState { representativeId = "ccs.survival.npc.representative.test.a" },
                new CCS_NpcServiceRepresentativeState { representativeId = "ccs.survival.npc.representative.test.b" }
            };
            CCS_SurvivalValidationResult persistValidation =
                CCS_NpcServiceRepresentativeValidationUtility.ValidatePersistedStates(states);
            report.AddIssue(
                persistValidation.IsSuccess
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                persistValidation.Message);

            string stateSource = System.IO.File.ReadAllText(
                "Assets/CCS/Modules/WorldSimulation/Runtime/Data/CCS_SettlementSimulationState.cs");
            bool saveField = stateSource.Contains("npcServiceRepresentativeStates", System.StringComparison.Ordinal);
            report.AddIssue(
                saveField ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                saveField
                    ? "Settlement simulation state persists npcServiceRepresentativeStates."
                    : "Settlement simulation state missing npcServiceRepresentativeStates save field.");
        }

        private static void ValidateRegistration(CCS_SurvivalValidationReport report)
        {
            string source = System.IO.File.ReadAllText(RegistrationPath);
            bool wired = source.Contains("CreateNpcServiceRepresentativeService", System.StringComparison.Ordinal)
                && source.Contains("WireNpcServiceRepresentatives", System.StringComparison.Ordinal);
            report.AddIssue(
                wired ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                wired
                    ? "Gameplay registration wires NPC service representative service."
                    : "Gameplay registration missing NPC service representative wiring.");
        }

        private static void ValidateServicePointFallback(CCS_SurvivalValidationReport report)
        {
            string interactableSource = System.IO.File.ReadAllText(
                "Assets/CCS/Modules/NPCs/Runtime/Components/CCS_NpcServiceRepresentativeInteractable.cs");
            bool routesThroughResolver = interactableSource.Contains(
                "CCS_SettlementServiceRouteResolver.TryActivate",
                System.StringComparison.Ordinal);
            report.AddIssue(
                routesThroughResolver ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                routesThroughResolver
                    ? "Representative interactable routes through settlement service resolver."
                    : "Representative interactable missing settlement service resolver routing.");

            string servicePointSource = System.IO.File.ReadAllText(
                "Assets/CCS/Modules/Settlements/Runtime/Components/CCS_SettlementServicePoint.cs");
            bool servicePointRegistered = servicePointSource.Contains("CCS_SettlementServicePointRuntimeBridge.Register",
                System.StringComparison.Ordinal);
            report.AddIssue(
                servicePointRegistered ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                servicePointRegistered
                    ? "Settlement service points remain registered for fallback interaction."
                    : "Settlement service point fallback registration missing.");
        }

        private static void ValidatePlaytest(CCS_SurvivalValidationReport report)
        {
            string hudSource = System.IO.File.ReadAllText(
                "Assets/CCS/Modules/Playtesting/Runtime/UI/CCS_PlaytestHud.cs");
            bool shortcut = hudSource.Contains("TryPlaytestNpcServiceRepresentativeFoundationShortcut",
                    System.StringComparison.Ordinal)
                && hudSource.Contains("WasControlAltPressed(KeyCode.R)", System.StringComparison.Ordinal);
            report.AddIssue(
                shortcut ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                shortcut
                    ? "Playtest HUD exposes Ctrl+Alt+R NPC service representative shortcut."
                    : "Playtest HUD missing Ctrl+Alt+R NPC service representative shortcut.");

            string hotkeySource = System.IO.File.ReadAllText(
                "Assets/CCS/Modules/CharacterController/Runtime/Input/CCS_DevHotkeyUtility.cs");
            bool registered = hotkeySource.Contains("KeyCode.R, requiresControl: true, requiresAlt: true",
                System.StringComparison.Ordinal);
            report.AddIssue(
                registered ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                registered
                    ? "Dev hotkey registry includes Ctrl+Alt+R for NPC service representatives."
                    : "Dev hotkey registry missing Ctrl+Alt+R binding.");

            CCS_PlaytestProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(
                "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset");
            if (profile == null)
            {
                return;
            }

            ValidateStep(report, profile, CCS_PlaytestStepType.VerifyGeneralStoreRepresentativeAssigned);
            ValidateStep(report, profile, CCS_PlaytestStepType.VerifyNpcServiceRepresentativeAfterLoad);
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
                    : $"Playtest profile missing step {stepType}. Run representative bootstrap.");
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
