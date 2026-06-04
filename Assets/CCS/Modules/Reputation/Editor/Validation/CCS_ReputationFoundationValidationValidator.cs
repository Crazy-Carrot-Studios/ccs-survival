using System.IO;
using CCS.Modules.Playtesting;
using CCS.Modules.Reputation;
using CCS.Survival;
using CCS.Survival.Composition;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_ReputationFoundationValidationValidator
// CATEGORY: Modules / Reputation / Editor / Validation
// PURPOSE: Validates reputation module layout, content, composition, save, and playtest wiring.
// PLACEMENT: Registered on CCS_SurvivalValidationPipeline at editor load.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 2.7.0 reputation and service trust foundation.
// =============================================================================

namespace CCS.Modules.Reputation.Editor
{
    public sealed class CCS_ReputationFoundationValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string ValidatorContext = "ccs.survival.validation.reputation";
        private const string ReputationMilestoneVersion = "2.7.0";
        private const string ModuleRoot = "Assets/CCS/Modules/Reputation";
        private const string ModuleRootMetaPath = "Assets/CCS/Modules/Reputation.meta";
        private const string RuntimeRoot = ModuleRoot + "/Runtime";
        private const string EditorRoot = ModuleRoot + "/Editor";
        private const string ModuleDocPath = ModuleRoot + "/Documentation/CCS_Reputation_Module.md";
        private const string BootstrapPrefabPath = "Assets/CCS/Survival/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
        private const string CompositionRegistrationPath =
            "Assets/CCS/Survival/Runtime/Composition/CCS_SurvivalGameplayServiceRegistration.cs";
        private const string SettlementServicePath =
            "Assets/CCS/Modules/Settlements/Runtime/Services/CCS_SettlementService.cs";
        private const string SaveDataPath = "Assets/CCS/Modules/SaveSystem/Runtime/Data/CCS_SaveData.cs";
        private const string SaveServicePath = "Assets/CCS/Modules/SaveSystem/Runtime/Services/CCS_SaveService.cs";
        private const string ReputationServicePath = RuntimeRoot + "/Services/CCS_ReputationService.cs";
        private const string ReputationHudPath = RuntimeRoot + "/UI/CCS_ReputationDebugHud.cs";
        private const string ContentReputationMetaPath = "Assets/CCS/Survival/Content/Reputation.meta";
        private const string ProfileReputationMetaPath = "Assets/CCS/Survival/Profiles/Reputation.meta";
        private const string DefaultPlaytestProfilePath = "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset";

        public string ValidatorId => ValidatorContext;

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateRequiredFolder(report, "Modules/Reputation", ModuleRoot);
            ValidateRequiredFolder(report, "Runtime/Data", RuntimeRoot + "/Data");
            ValidateRequiredFolder(report, "Runtime/Definitions", RuntimeRoot + "/Definitions");
            ValidateRequiredFolder(report, "Runtime/Profiles", RuntimeRoot + "/Profiles");
            ValidateRequiredFolder(report, "Runtime/Services", RuntimeRoot + "/Services");
            ValidateRequiredFolder(report, "Runtime/Validation", RuntimeRoot + "/Validation");
            ValidateRequiredFolder(report, "Runtime/UI", RuntimeRoot + "/UI");
            ValidateRequiredFolder(report, "Editor/Validation", EditorRoot + "/Validation");
            ValidateRequiredFolder(report, "Documentation", ModuleRoot + "/Documentation");

            ValidateRequiredFile(report, "Runtime asmdef", RuntimeRoot + "/CCS.Modules.Reputation.Runtime.asmdef");
            ValidateRequiredFile(report, "Editor asmdef", EditorRoot + "/CCS.Modules.Reputation.Editor.asmdef");
            ValidateRequiredFile(report, "Module root meta", ModuleRootMetaPath);
            ValidateFolderMeta(report, "Content/Reputation folder", ContentReputationMetaPath);
            ValidateFolderMeta(report, "Profiles/Reputation folder", ProfileReputationMetaPath);
            ValidateRequiredScript(report, "CCS_ReputationService", ReputationServicePath);
            ValidateRequiredScript(
                report,
                "CCS_ReputationRuntimeBridge",
                RuntimeRoot + "/Services/CCS_ReputationRuntimeBridge.cs");
            ValidateRequiredScript(
                report,
                "CCS_ReputationValidationUtility",
                RuntimeRoot + "/Validation/CCS_ReputationValidationUtility.cs");
            ValidateRequiredScript(
                report,
                "CCS_ReputationFoundationBootstrapSetup",
                EditorRoot + "/Validation/CCS_ReputationFoundationBootstrapSetup.cs");
            ValidateRequiredScript(report, "CCS_ReputationDebugHud", ReputationHudPath);

            ValidateReputationProfile(report);
            ValidateTradingPostDefinition(report);
            ValidateCompositionRegistration(report);
            ValidateBootstrapWiring(report);
            ValidateSettlementIntegration(report);
            ValidateSaveSupport(report);
            ValidateIntegrationHooks(report);
            ValidatePlaytestSteps(report);
            ValidateDocumentation(report);

            CCS_SurvivalBootstrapVersionUtility.AddBundleVersionValidationIssue(
                report,
                ValidatorContext,
                ReputationMilestoneVersion,
                "Run CCS_ReputationFoundationBootstrapSetup.ExecuteBatch.");
            CCS_SurvivalBootstrapVersionUtility.ValidateNoHardcodedBootstrapVersionWrites(report, ValidatorContext);
            CCS_SurvivalInputValidationUtility.ValidateNoLegacyInputUsage(report, "Reputation Input Policy");
            CCS_SurvivalInputValidationUtility.ValidateNoFindObjectsSortModeUsage(report, "Reputation Obsolete API Scan");
        }

        private static void ValidateReputationProfile(CCS_SurvivalValidationReport report)
        {
            CCS_ReputationProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_ReputationProfile>(CCS_ReputationContentIds.DefaultReputationProfilePath);
            CCS_SurvivalValidationResult result = CCS_ReputationValidationUtility.ValidateProfile(profile);
            report.AddIssue(
                result.IsSuccess ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                result.Message);
        }

        private static void ValidateTradingPostDefinition(CCS_SurvivalValidationReport report)
        {
            CCS_ReputationDefinition definition = AssetDatabase.LoadAssetAtPath<CCS_ReputationDefinition>(
                CCS_ReputationContentIds.FrontierTradingPostReputationDefinitionPath);
            CCS_SurvivalValidationResult result = CCS_ReputationValidationUtility.ValidateDefinition(definition);
            report.AddIssue(
                result.IsSuccess ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                result.Message);
        }

        private static void ValidateCompositionRegistration(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(CompositionRegistrationPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    ValidatorContext,
                    "Missing CCS_SurvivalGameplayServiceRegistration.cs.");
                return;
            }

            string source = File.ReadAllText(CompositionRegistrationPath);
            bool hasService = source.Contains("CreateReputationService", System.StringComparison.Ordinal);
            bool hasProfile = source.Contains("reputationProfile", System.StringComparison.Ordinal);
            bool hasHooks = source.Contains("WireReputationEventHooks", System.StringComparison.Ordinal);
            report.AddIssue(
                hasService && hasProfile && hasHooks
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                hasService && hasProfile && hasHooks
                    ? "Composition registers reputation service, profile, and event hooks."
                    : "Composition missing reputation service, profile, or event hook wiring.");
        }

        private static void ValidateBootstrapWiring(CCS_SurvivalValidationReport report)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(BootstrapPrefabPath);
            CCS_SurvivalGameplayServiceHost host = prefab != null ? prefab.GetComponent<CCS_SurvivalGameplayServiceHost>() : null;
            SerializedObject serialized = host != null ? new SerializedObject(host) : null;
            bool ok = serialized != null
                && serialized.FindProperty("reputationProfile").objectReferenceValue != null;
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                ok
                    ? "Bootstrap host references reputation profile."
                    : "Run CCS_ReputationFoundationBootstrapSetup.ExecuteBatch for reputation profile wiring.");
        }

        private static void ValidateSettlementIntegration(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(SettlementServicePath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    ValidatorContext,
                    "CCS_SettlementService.cs was not found.");
                return;
            }

            string source = File.ReadAllText(SettlementServicePath);
            bool hasGet = source.Contains("TryGetSettlementReputation", System.StringComparison.Ordinal);
            bool hasEvent = source.Contains("SettlementReputationChanged", System.StringComparison.Ordinal);
            report.AddIssue(
                hasGet && hasEvent
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                hasGet && hasEvent
                    ? "Settlement service exposes reputation query and change events."
                    : "Settlement service missing reputation integration APIs.");
        }

        private static void ValidateSaveSupport(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(SaveDataPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    ValidatorContext,
                    "CCS_SaveData.cs was not found.");
                return;
            }

            string saveDataSource = File.ReadAllText(SaveDataPath);
            bool hasReputationData = saveDataSource.Contains("CCS_SaveReputationWorldData", System.StringComparison.Ordinal);
            bool hasSnapshots = saveDataSource.Contains("CCS_ReputationSnapshot[] standings", System.StringComparison.Ordinal);
            report.AddIssue(
                hasReputationData && hasSnapshots
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                hasReputationData && hasSnapshots
                    ? "Unified save payload includes reputation world data."
                    : "CCS_SaveData is missing reputation save payload.");

            if (!File.Exists(SaveServicePath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    ValidatorContext,
                    "CCS_SaveService.cs was not found.");
                return;
            }

            string saveServiceSource = File.ReadAllText(SaveServicePath);
            bool captures = saveServiceSource.Contains("CaptureReputationState", System.StringComparison.Ordinal);
            bool restores = saveServiceSource.Contains("RestoreState(bankingData?.accounts)", System.StringComparison.Ordinal)
                || saveServiceSource.Contains("CaptureReputation", System.StringComparison.Ordinal);
            bool restoresReputation = saveServiceSource.Contains("reputationService.RestoreState", System.StringComparison.Ordinal)
                || saveServiceSource.Contains("CaptureReputation", System.StringComparison.Ordinal);
            report.AddIssue(
                captures && restoresReputation
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                captures && restoresReputation
                    ? "Save service captures and restores reputation state."
                    : "Save service missing reputation capture/restore wiring.");
        }

        private static void ValidateIntegrationHooks(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(CompositionRegistrationPath))
            {
                return;
            }

            string source = File.ReadAllText(CompositionRegistrationPath);
            bool vendorHook = source.Contains("TryApplyGoodsSold", System.StringComparison.Ordinal);
            bool loanHook = source.Contains("TryApplyLoanRepaid", System.StringComparison.Ordinal);
            bool upkeepHook = source.Contains("TryApplyUpkeepPaid", System.StringComparison.Ordinal);
            report.AddIssue(
                vendorHook && loanHook && upkeepHook
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                vendorHook && loanHook && upkeepHook
                    ? "Reputation event hooks wired for vendor, loan, and upkeep flows."
                    : "Composition missing one or more reputation event hooks.");
        }

        private static void ValidatePlaytestSteps(CCS_SurvivalValidationReport report)
        {
            CCS_PlaytestProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(DefaultPlaytestProfilePath);
            if (profile == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    ValidatorContext,
                    $"Missing playtest profile at {DefaultPlaytestProfilePath}.");
                return;
            }

            bool hasVerifyLoad = false;
            System.Collections.Generic.IReadOnlyList<CCS_PlaytestStepDefinition> steps = profile.StepDefinitions;
            for (int index = 0; index < steps.Count; index++)
            {
                if (steps[index]?.StepType == CCS_PlaytestStepType.VerifyReputationAfterLoad)
                {
                    hasVerifyLoad = true;
                    break;
                }
            }

            report.AddIssue(
                hasVerifyLoad
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                hasVerifyLoad
                    ? "Reputation playtest includes save/load verification step."
                    : "Run CCS_ReputationFoundationBootstrapSetup.ExecuteBatch for reputation playtest steps.");
        }

        private static void ValidateDocumentation(CCS_SurvivalValidationReport report)
        {
            bool exists = File.Exists(ModuleDocPath);
            report.AddIssue(
                exists ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                exists
                    ? "Reputation module documentation present."
                    : $"Missing module doc: {ModuleDocPath}");
        }

        private static void ValidateRequiredFolder(
            CCS_SurvivalValidationReport report,
            string label,
            string folderPath)
        {
            bool exists = AssetDatabase.IsValidFolder(folderPath);
            report.AddIssue(
                exists ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                exists ? $"{label} folder present." : $"Missing folder: {folderPath}");
        }

        private static void ValidateRequiredFile(
            CCS_SurvivalValidationReport report,
            string label,
            string filePath)
        {
            bool exists = File.Exists(filePath);
            report.AddIssue(
                exists ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                exists ? $"{label} present." : $"Missing file: {filePath}");
        }

        private static void ValidateRequiredScript(
            CCS_SurvivalValidationReport report,
            string label,
            string scriptPath)
        {
            ValidateRequiredFile(report, label, scriptPath);
        }

        private static void ValidateFolderMeta(CCS_SurvivalValidationReport report, string label, string metaPath)
        {
            bool exists = File.Exists(metaPath);
            report.AddIssue(
                exists ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                exists ? $"{label} meta present." : $"Missing meta: {metaPath}");
        }
    }
}
