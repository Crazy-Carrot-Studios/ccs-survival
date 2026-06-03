using System.IO;
using CCS.Modules.Banking;
using CCS.Modules.Playtesting;
using CCS.Modules.Settlements;
using CCS.Survival;
using CCS.Survival.Composition;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_BankingFoundationValidationValidator
// CATEGORY: Modules / Banking / Editor / Validation
// PURPOSE: Validates banking module layout, content, composition, save, and playtest wiring.
// PLACEMENT: Registered on CCS_SurvivalValidationPipeline at editor load.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 2.4.0 banking and land office foundation.
// =============================================================================

namespace CCS.Modules.Banking.Editor
{
    public sealed class CCS_BankingFoundationValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string ValidatorContext = "ccs.survival.validation.banking";
        private const string BankingMilestoneVersion = "2.4.0";
        private const string ModuleRoot = "Assets/CCS/Modules/Banking";
        private const string RuntimeRoot = ModuleRoot + "/Runtime";
        private const string EditorRoot = ModuleRoot + "/Editor";
        private const string ModuleDocPath = ModuleRoot + "/Documentation/CCS_Banking_Module.md";
        private const string BootstrapPrefabPath = "Assets/CCS/Survival/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
        private const string CompositionRegistrationPath =
            "Assets/CCS/Survival/Runtime/Composition/CCS_SurvivalGameplayServiceRegistration.cs";
        private const string SaveDataPath = "Assets/CCS/Modules/SaveSystem/Runtime/Data/CCS_SaveData.cs";
        private const string BootstrapScenePath = "Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string DefaultPlaytestProfilePath = "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset";

        public string ValidatorId => ValidatorContext;

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateRequiredFolder(report, "Modules/Banking", ModuleRoot);
            ValidateRequiredFolder(report, "Runtime/Data", RuntimeRoot + "/Data");
            ValidateRequiredFolder(report, "Runtime/Definitions", RuntimeRoot + "/Definitions");
            ValidateRequiredFolder(report, "Runtime/Profiles", RuntimeRoot + "/Profiles");
            ValidateRequiredFolder(report, "Runtime/Services", RuntimeRoot + "/Services");
            ValidateRequiredFolder(report, "Runtime/Validation", RuntimeRoot + "/Validation");
            ValidateRequiredFolder(report, "Runtime/UI", RuntimeRoot + "/UI");
            ValidateRequiredFolder(report, "Editor/Validation", EditorRoot + "/Validation");
            ValidateRequiredFolder(report, "Documentation", ModuleRoot + "/Documentation");

            ValidateRequiredFile(report, "Runtime asmdef", RuntimeRoot + "/CCS.Modules.Banking.Runtime.asmdef");
            ValidateRequiredFile(report, "Editor asmdef", EditorRoot + "/CCS.Modules.Banking.Editor.asmdef");
            ValidateRequiredScript(report, "CCS_BankingService", RuntimeRoot + "/Services/CCS_BankingService.cs");
            ValidateRequiredScript(
                report,
                "CCS_BankingRuntimeBridge",
                RuntimeRoot + "/Services/CCS_BankingRuntimeBridge.cs");
            ValidateRequiredScript(
                report,
                "CCS_BankingValidationUtility",
                RuntimeRoot + "/Validation/CCS_BankingValidationUtility.cs");
            ValidateRequiredScript(
                report,
                "CCS_BankingDebugHud",
                RuntimeRoot + "/UI/CCS_BankingDebugHud.cs");
            ValidateRequiredScript(
                report,
                "CCS_BankingFoundationBootstrapSetup",
                EditorRoot + "/Validation/CCS_BankingFoundationBootstrapSetup.cs");

            ValidateBankProfile(report);
            ValidateAccountDefinition(report);
            ValidateCompositionRegistration(report);
            ValidateBootstrapWiring(report);
            ValidateSaveSupport(report);
            ValidateSettlementServicePoints(report);
            ValidateServiceBindings(report);
            ValidatePlaytestSteps(report);
            ValidateDocumentation(report);

            CCS_SurvivalBootstrapVersionUtility.AddBundleVersionValidationIssue(
                report,
                ValidatorContext,
                BankingMilestoneVersion,
                "Run CCS_BankingFoundationBootstrapSetup.ExecuteBatch.");
            CCS_SurvivalBootstrapVersionUtility.ValidateNoHardcodedBootstrapVersionWrites(report, ValidatorContext);
            CCS_SurvivalInputValidationUtility.ValidateNoLegacyInputUsage(report, "Banking Input Policy");
            CCS_SurvivalInputValidationUtility.ValidateNoFindObjectsSortModeUsage(report, "Banking Obsolete API Scan");
        }

        private static void ValidateBankProfile(CCS_SurvivalValidationReport report)
        {
            CCS_BankAccountProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_BankAccountProfile>(CCS_BankingContentIds.DefaultBankProfilePath);
            CCS_SurvivalValidationResult result = CCS_BankingValidationUtility.ValidateProfile(profile);
            report.AddIssue(
                result.IsSuccess ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                result.Message);
        }

        private static void ValidateAccountDefinition(CCS_SurvivalValidationReport report)
        {
            CCS_BankAccountDefinition definition = AssetDatabase.LoadAssetAtPath<CCS_BankAccountDefinition>(
                CCS_BankingContentIds.FrontierSavingsAccountDefinitionPath);
            CCS_SurvivalValidationResult result = CCS_BankingValidationUtility.ValidateAccountDefinition(definition);
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
            bool hasBankingService = source.Contains("CreateBankingService", System.StringComparison.Ordinal);
            bool hasBankProfile = source.Contains("bankAccountProfile", System.StringComparison.Ordinal);
            report.AddIssue(
                hasBankingService && hasBankProfile
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                hasBankingService && hasBankProfile
                    ? "Composition registers banking service and bank account profile."
                    : "Composition missing banking service or profile wiring.");
        }

        private static void ValidateBootstrapWiring(CCS_SurvivalValidationReport report)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(BootstrapPrefabPath);
            CCS_SurvivalGameplayServiceHost host = prefab != null ? prefab.GetComponent<CCS_SurvivalGameplayServiceHost>() : null;
            SerializedObject serialized = host != null ? new SerializedObject(host) : null;
            bool ok = serialized != null
                && serialized.FindProperty("bankAccountProfile").objectReferenceValue != null;
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                ok
                    ? "Bootstrap host references bank account profile."
                    : "Bootstrap host missing bank account profile reference.");
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

            string source = File.ReadAllText(SaveDataPath);
            bool hasBankingData = source.Contains("CCS_SaveBankingWorldData", System.StringComparison.Ordinal);
            report.AddIssue(
                hasBankingData
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                hasBankingData
                    ? "Unified save payload includes banking world data."
                    : "CCS_SaveData is missing CCS_SaveBankingWorldData.");
        }

        private static void ValidateSettlementServicePoints(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(BootstrapScenePath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    ValidatorContext,
                    $"Missing bootstrap scene: {BootstrapScenePath}");
                return;
            }

            string sceneText = File.ReadAllText(BootstrapScenePath);
            bool hasBank = sceneText.Contains(CCS_SettlementContentIds.BankServicePointId, System.StringComparison.Ordinal);
            bool hasLandOffice = sceneText.Contains(
                CCS_SettlementContentIds.LandOfficeServicePointId,
                System.StringComparison.Ordinal);
            report.AddIssue(
                hasBank && hasLandOffice
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                hasBank && hasLandOffice
                    ? "Bootstrap trading post includes bank and land office service points."
                    : "Run CCS_FrontierSettlementBootstrapSetup.ExecuteBatch for bank/land office cubes.");
        }

        private static void ValidateServiceBindings(CCS_SurvivalValidationReport report)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(BootstrapPrefabPath);
            CCS_SurvivalGameplayServiceHost host = prefab != null ? prefab.GetComponent<CCS_SurvivalGameplayServiceHost>() : null;
            if (host == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    ValidatorContext,
                    "Bootstrap host missing for service binding validation.");
                return;
            }

            SerializedObject serialized = new SerializedObject(host);
            Object economyProfile = serialized.FindProperty("economyProfile").objectReferenceValue;
            Object landProfile = serialized.FindProperty("landClaimProfile").objectReferenceValue;
            bool economyOk = economyProfile != null;
            bool landOk = landProfile != null;
            report.AddIssue(
                economyOk ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                economyOk
                    ? "Economy profile assigned for currency service binding."
                    : "Bootstrap host missing economy profile for banking currency binding.");
            report.AddIssue(
                landOk ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                landOk
                    ? "Land claim profile assigned for land office display binding."
                    : "Bootstrap host missing land claim profile for land office binding.");
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
                if (steps[index]?.StepType == CCS_PlaytestStepType.VerifyBankBalanceAfterLoad)
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
                    ? "Banking playtest includes save/load verification step."
                    : "Run CCS_BankingFoundationBootstrapSetup.ExecuteBatch for banking playtest steps.");
        }

        private static void ValidateDocumentation(CCS_SurvivalValidationReport report)
        {
            bool exists = File.Exists(ModuleDocPath);
            report.AddIssue(
                exists ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                exists
                    ? "Banking module documentation present."
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
    }
}
