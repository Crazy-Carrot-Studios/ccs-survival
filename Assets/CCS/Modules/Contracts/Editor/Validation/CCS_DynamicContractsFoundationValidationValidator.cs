using System.IO;
using CCS.Modules.Contracts;
using CCS.Modules.Playtesting;
using CCS.Survival;
using CCS.Survival.Composition;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_DynamicContractsFoundationValidationValidator
// CATEGORY: Modules / Contracts / Editor / Validation
// PURPOSE: Validates dynamic contract module layout, wiring, save/load, and playtest harness.
// PLACEMENT: Registered on CCS_SurvivalValidationPipeline at editor load.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 5.3.0 dynamic contract generation foundation.
// =============================================================================

namespace CCS.Modules.Contracts.Editor
{
    public sealed class CCS_DynamicContractsFoundationValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string ValidatorContext = "ccs.survival.validation.dynamiccontracts";
        private const string MilestoneVersion = "5.3.0";
        private const string DynamicRoot = "Assets/CCS/Modules/Contracts/Runtime/Dynamic";
        private const string CompositionRegistrationPath =
            "Assets/CCS/Survival/Runtime/Composition/CCS_SurvivalGameplayServiceRegistration.cs";
        private const string SaveDataPath = "Assets/CCS/Modules/SaveSystem/Runtime/Data/CCS_SaveData.cs";
        private const string SaveServicePath = "Assets/CCS/Modules/SaveSystem/Runtime/Services/CCS_SaveService.cs";
        private const string ContractHudPath = "Assets/CCS/Modules/Contracts/Runtime/UI/CCS_ContractDebugHud.cs";
        private const string PlaytestHudPath = "Assets/CCS/Modules/Playtesting/Runtime/UI/CCS_PlaytestHud.cs";
        private const string PlaytestProfilePath =
            "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset";

        public string ValidatorId => ValidatorContext;

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateRequiredFile(report, "CCS_DynamicContractService", DynamicRoot + "/CCS_DynamicContractService.cs");
            ValidateRequiredFile(report, "CCS_DynamicContractProfile", DynamicRoot + "/CCS_DynamicContractProfile.cs");
            ValidateRequiredFile(report, "CCS_DynamicContractValidationUtility", DynamicRoot + "/CCS_DynamicContractValidationUtility.cs");
            ValidateRequiredFile(report, "CCS_DynamicContractRuntimeBridge", DynamicRoot + "/CCS_DynamicContractRuntimeBridge.cs");
            ValidateRequiredFile(
                report,
                "CCS_DynamicContractsFoundationBootstrapSetup",
                "Assets/CCS/Modules/Contracts/Editor/Validation/CCS_DynamicContractsFoundationBootstrapSetup.cs");
            ValidateRequiredFile(
                report,
                "CCS_ContractsValidationRegistration",
                "Assets/CCS/Modules/Contracts/Editor/Validation/CCS_ContractsValidationRegistration.cs");

            CCS_DynamicContractProfile profile = AssetDatabase.LoadAssetAtPath<CCS_DynamicContractProfile>(
                CCS_DynamicContractContentIds.DefaultDynamicContractProfilePath);
            CCS_SurvivalValidationResult validation = CCS_DynamicContractValidationUtility.ValidateProfile(profile);
            report.AddIssue(
                validation.IsSuccess ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                validation.Message);

            CCS_ContractProfile contractProfile =
                AssetDatabase.LoadAssetAtPath<CCS_ContractProfile>(CCS_ContractContentIds.DefaultContractProfilePath);
            bool linked = contractProfile != null && contractProfile.DynamicContractProfile != null;
            report.AddIssue(
                linked ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                linked
                    ? "Default contract profile links dynamic contract profile."
                    : "Default contract profile missing dynamic contract profile link.");

            ValidateComposition(report);
            ValidateSaveSupport(report);
            ValidateBoardIntegration(report);
            ValidatePlaytest(report);

            CCS_SurvivalBootstrapVersionUtility.AddBundleVersionValidationIssue(
                report,
                ValidatorContext,
                MilestoneVersion,
                "Run CCS_DynamicContractsFoundationBootstrapSetup.ExecuteBatch.");
            CCS_SurvivalInputValidationUtility.ValidateNoLegacyInputUsage(report, "Dynamic Contracts Input Policy");
        }

        private static void ValidateComposition(CCS_SurvivalValidationReport report)
        {
            string source = File.Exists(CompositionRegistrationPath)
                ? File.ReadAllText(CompositionRegistrationPath)
                : string.Empty;
            bool ok = source.Contains("CreateDynamicContractService", System.StringComparison.Ordinal)
                && source.Contains("WireDynamicContracts", System.StringComparison.Ordinal)
                && source.Contains("SettlementSupplyChanged", System.StringComparison.Ordinal);
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                ok
                    ? "Gameplay composition wires dynamic contract service, supply evaluation, and event hooks."
                    : "Gameplay composition missing dynamic contract wiring.");
        }

        private static void ValidateSaveSupport(CCS_SurvivalValidationReport report)
        {
            string saveData = File.Exists(SaveDataPath) ? File.ReadAllText(SaveDataPath) : string.Empty;
            string saveService = File.Exists(SaveServicePath) ? File.ReadAllText(SaveServicePath) : string.Empty;
            bool dataOk = saveData.Contains("dynamicContractStates", System.StringComparison.Ordinal)
                && saveData.Contains("dynamicRuleCooldowns", System.StringComparison.Ordinal);
            bool serviceOk = saveService.Contains("CaptureDynamicContractStates", System.StringComparison.Ordinal)
                && saveService.Contains("RestorePersistedState", System.StringComparison.Ordinal);
            report.AddIssue(
                dataOk ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                dataOk ? "Save payload includes dynamic contract states and cooldowns." : "Save payload missing dynamic contract fields.");
            report.AddIssue(
                serviceOk ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                serviceOk ? "Save service captures and restores dynamic contract state." : "Save service missing dynamic contract capture/restore.");
        }

        private static void ValidateBoardIntegration(CCS_SurvivalValidationReport report)
        {
            string hudSource = File.Exists(ContractHudPath) ? File.ReadAllText(ContractHudPath) : string.Empty;
            bool ok = hudSource.Contains("Generated dynamic contracts", System.StringComparison.Ordinal)
                && hudSource.Contains("CCS_DynamicContractValidationUtility.IsGeneratedContractId", System.StringComparison.Ordinal);
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                ok
                    ? "Contract debug board exposes generated dynamic contract section."
                    : "Contract debug board missing generated dynamic contract section.");
        }

        private static void ValidatePlaytest(CCS_SurvivalValidationReport report)
        {
            string hudSource = File.Exists(PlaytestHudPath) ? File.ReadAllText(PlaytestHudPath) : string.Empty;
            bool shortcutOk = hudSource.Contains("TryPlaytestDynamicContractsFoundationShortcut", System.StringComparison.Ordinal)
                && hudSource.Contains("WasControlAltPressed(KeyCode.C)", System.StringComparison.Ordinal);
            report.AddIssue(
                shortcutOk ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                shortcutOk
                    ? "Playtest HUD exposes Ctrl+Alt+C dynamic contracts shortcut."
                    : "Playtest HUD missing Ctrl+Alt+C dynamic contracts shortcut.");

            CCS_PlaytestProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(PlaytestProfilePath);
            ValidateStep(report, profile, CCS_PlaytestStepType.DiscoverSettlementsForDynamicContracts);
            ValidateStep(report, profile, CCS_PlaytestStepType.GenerateNeedBasedDynamicContract);
            ValidateStep(report, profile, CCS_PlaytestStepType.VerifyGeneratedDynamicContractOnBoard);
            ValidateStep(report, profile, CCS_PlaytestStepType.SaveDynamicContractState);
            ValidateStep(report, profile, CCS_PlaytestStepType.VerifyDynamicContractStateAfterLoad);
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
                    : $"Playtest profile missing step {stepType}.");
        }

        private static void ValidateRequiredFile(CCS_SurvivalValidationReport report, string label, string path)
        {
            bool exists = File.Exists(path);
            report.AddIssue(
                exists ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                exists ? $"{label} present." : $"Missing required file: {path}");
        }
    }
}
