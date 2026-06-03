using System.IO;
using CCS.Modules.Banking;
using CCS.Modules.Land;
using CCS.Modules.Playtesting;
using CCS.Modules.Upkeep;
using CCS.Survival;
using CCS.Survival.Composition;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_UpkeepFoundationValidationValidator
// CATEGORY: Modules / Upkeep / Editor / Validation
// PURPOSE: Validates upkeep module layout, content, composition, save, and playtest wiring.
// PLACEMENT: Registered on CCS_SurvivalValidationPipeline at editor load.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 2.5.0 tax and upkeep foundation; 2.5.1 release cleanup validation.
// =============================================================================

namespace CCS.Modules.Upkeep.Editor
{
    public sealed class CCS_UpkeepFoundationValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string ValidatorContext = "ccs.survival.validation.upkeep";
        private const string UpkeepMilestoneVersion = "2.5.1";
        private const string ModuleRoot = "Assets/CCS/Modules/Upkeep";
        private const string ModuleRootMetaPath = "Assets/CCS/Modules/Upkeep.meta";
        private const string RuntimeRoot = ModuleRoot + "/Runtime";
        private const string EditorRoot = ModuleRoot + "/Editor";
        private const string ModuleDocPath = ModuleRoot + "/Documentation/CCS_Upkeep_Module.md";
        private const string BootstrapPrefabPath = "Assets/CCS/Survival/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
        private const string CompositionRegistrationPath =
            "Assets/CCS/Survival/Runtime/Composition/CCS_SurvivalGameplayServiceRegistration.cs";
        private const string SaveDataPath = "Assets/CCS/Modules/SaveSystem/Runtime/Data/CCS_SaveData.cs";
        private const string SaveServicePath = "Assets/CCS/Modules/SaveSystem/Runtime/Services/CCS_SaveService.cs";
        private const string BankingHudPath = "Assets/CCS/Modules/Banking/Runtime/UI/CCS_BankingDebugHud.cs";
        private const string BankingServicePath = "Assets/CCS/Modules/Banking/Runtime/Services/CCS_BankingService.cs";
        private const string UpkeepServicePath = RuntimeRoot + "/Services/CCS_UpkeepService.cs";
        private const string ContentUpkeepMetaPath = "Assets/CCS/Survival/Content/Upkeep.meta";
        private const string ProfileUpkeepMetaPath = "Assets/CCS/Survival/Profiles/Upkeep.meta";
        private const string DefaultPlaytestProfilePath = "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset";

        public string ValidatorId => ValidatorContext;

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateRequiredFolder(report, "Modules/Upkeep", ModuleRoot);
            ValidateRequiredFolder(report, "Runtime/Data", RuntimeRoot + "/Data");
            ValidateRequiredFolder(report, "Runtime/Definitions", RuntimeRoot + "/Definitions");
            ValidateRequiredFolder(report, "Runtime/Profiles", RuntimeRoot + "/Profiles");
            ValidateRequiredFolder(report, "Runtime/Services", RuntimeRoot + "/Services");
            ValidateRequiredFolder(report, "Runtime/Validation", RuntimeRoot + "/Validation");
            ValidateRequiredFolder(report, "Editor/Validation", EditorRoot + "/Validation");
            ValidateRequiredFolder(report, "Documentation", ModuleRoot + "/Documentation");

            ValidateRequiredFile(report, "Runtime asmdef", RuntimeRoot + "/CCS.Modules.Upkeep.Runtime.asmdef");
            ValidateRequiredFile(report, "Editor asmdef", EditorRoot + "/CCS.Modules.Upkeep.Editor.asmdef");
            ValidateModuleFolderMeta(report);
            ValidateFolderMeta(report, "Runtime folder", RuntimeRoot + ".meta");
            ValidateFolderMeta(report, "Editor folder", EditorRoot + ".meta");
            ValidateFolderMeta(report, "Documentation folder", ModuleRoot + "/Documentation.meta");
            ValidateFolderMeta(report, "Content/Upkeep folder", ContentUpkeepMetaPath);
            ValidateFolderMeta(report, "Profiles/Upkeep folder", ProfileUpkeepMetaPath);
            ValidateRequiredScript(report, "CCS_UpkeepService", UpkeepServicePath);
            ValidateRequiredScript(
                report,
                "CCS_UpkeepRuntimeBridge",
                RuntimeRoot + "/Services/CCS_UpkeepRuntimeBridge.cs");
            ValidateRequiredScript(
                report,
                "CCS_UpkeepValidationUtility",
                RuntimeRoot + "/Validation/CCS_UpkeepValidationUtility.cs");
            ValidateRequiredScript(
                report,
                "CCS_UpkeepFoundationBootstrapSetup",
                EditorRoot + "/Validation/CCS_UpkeepFoundationBootstrapSetup.cs");

            ValidateUpkeepProfile(report);
            ValidateHomesteadTaxDefinition(report);
            ValidateCompositionRegistration(report);
            ValidateBootstrapWiring(report);
            ValidateSaveSupport(report);
            ValidateBankingIntegration(report);
            ValidateLandClaimIntegration(report);
            ValidateLandOfficeHud(report);
            ValidateReleaseSafety(report);
            ValidatePlaytestSteps(report);
            ValidateDocumentation(report);

            CCS_SurvivalBootstrapVersionUtility.AddBundleVersionValidationIssue(
                report,
                ValidatorContext,
                UpkeepMilestoneVersion,
                "Run CCS_UpkeepFoundationBootstrapSetup.ExecuteBatch.");
            CCS_SurvivalBootstrapVersionUtility.ValidateNoHardcodedBootstrapVersionWrites(report, ValidatorContext);
            CCS_SurvivalInputValidationUtility.ValidateNoLegacyInputUsage(report, "Upkeep Input Policy");
            CCS_SurvivalInputValidationUtility.ValidateNoFindObjectsSortModeUsage(report, "Upkeep Obsolete API Scan");
        }

        private static void ValidateUpkeepProfile(CCS_SurvivalValidationReport report)
        {
            CCS_UpkeepProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_UpkeepProfile>(CCS_UpkeepContentIds.DefaultUpkeepProfilePath);
            CCS_SurvivalValidationResult result = CCS_UpkeepValidationUtility.ValidateProfile(profile);
            report.AddIssue(
                result.IsSuccess ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                result.Message);
        }

        private static void ValidateHomesteadTaxDefinition(CCS_SurvivalValidationReport report)
        {
            CCS_UpkeepDefinition definition = AssetDatabase.LoadAssetAtPath<CCS_UpkeepDefinition>(
                CCS_UpkeepContentIds.FrontierHomesteadClaimTaxDefinitionPath);
            CCS_SurvivalValidationResult result = CCS_UpkeepValidationUtility.ValidateDefinition(definition);
            report.AddIssue(
                result.IsSuccess ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                result.IsSuccess
                    ? "Frontier Homestead Claim Tax definition validated."
                    : result.Message);
        }

        private static void ValidateCompositionRegistration(CCS_SurvivalValidationReport report)
        {
            bool exists = File.Exists(CompositionRegistrationPath);
            bool referencesUpkeep = exists
                && File.ReadAllText(CompositionRegistrationPath).Contains("CreateUpkeepService");
            report.AddIssue(
                referencesUpkeep
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                referencesUpkeep
                    ? "Composition registers CCS_UpkeepService."
                    : "CCS_SurvivalGameplayServiceRegistration missing CreateUpkeepService wiring.");
        }

        private static void ValidateBootstrapWiring(CCS_SurvivalValidationReport report)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(BootstrapPrefabPath);
            CCS_SurvivalGameplayServiceHost host = prefab != null ? prefab.GetComponent<CCS_SurvivalGameplayServiceHost>() : null;
            if (host == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    ValidatorContext,
                    "Bootstrap prefab missing gameplay service host for upkeep profile wiring.");
                return;
            }

            SerializedObject serialized = new SerializedObject(host);
            Object upkeepProfile = serialized.FindProperty("upkeepProfile").objectReferenceValue;
            report.AddIssue(
                upkeepProfile != null
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                upkeepProfile != null
                    ? "Bootstrap host assigns default upkeep profile."
                    : "Run CCS_UpkeepFoundationBootstrapSetup.ExecuteBatch for upkeep profile wiring.");
        }

        private static void ValidateSaveSupport(CCS_SurvivalValidationReport report)
        {
            bool saveDataOk = File.Exists(SaveDataPath)
                && File.ReadAllText(SaveDataPath).Contains("CCS_SaveUpkeepWorldData");
            bool saveServiceOk = File.Exists(SaveServicePath)
                && File.ReadAllText(SaveServicePath).Contains("CaptureUpkeep")
                && File.ReadAllText(SaveServicePath).Contains("ApplyUpkeep");
            report.AddIssue(
                saveDataOk ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                saveDataOk
                    ? "Save data includes upkeep world payload."
                    : "CCS_SaveData missing upkeep persistence fields.");
            report.AddIssue(
                saveServiceOk ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                saveServiceOk
                    ? "Save service captures and applies upkeep state."
                    : "CCS_SaveService missing upkeep capture/apply paths.");
        }

        private static void ValidateBankingIntegration(CCS_SurvivalValidationReport report)
        {
            bool hasDebit = File.Exists(BankingServicePath)
                && File.ReadAllText(BankingServicePath).Contains("TryDebitForUpkeep");
            report.AddIssue(
                hasDebit
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                hasDebit
                    ? "Banking service exposes upkeep debit without wallet credit."
                    : "CCS_BankingService missing TryDebitForUpkeep for upkeep payments.");
        }

        private static void ValidateModuleFolderMeta(CCS_SurvivalValidationReport report)
        {
            bool exists = File.Exists(ModuleRootMetaPath);
            report.AddIssue(
                exists ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                exists
                    ? "Upkeep module root folder .meta is present."
                    : $"Missing module root meta: {ModuleRootMetaPath}");
        }

        private static void ValidateFolderMeta(
            CCS_SurvivalValidationReport report,
            string label,
            string metaPath)
        {
            bool exists = File.Exists(metaPath);
            report.AddIssue(
                exists ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                exists ? $"{label} .meta is present." : $"Missing folder meta: {metaPath}");
        }

        private static void ValidateReleaseSafety(CCS_SurvivalValidationReport report)
        {
            CCS_SurvivalValidationResult result = CCS_UpkeepValidationUtility.ValidateReleaseSafetyContracts(
                UpkeepServicePath,
                SaveServicePath,
                BankingServicePath);
            report.AddIssue(
                result.IsSuccess ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                result.Message);

            bool playtestSaveLoad = false;
            CCS_PlaytestProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(DefaultPlaytestProfilePath);
            if (profile != null)
            {
                System.Collections.Generic.IReadOnlyList<CCS_PlaytestStepDefinition> steps = profile.StepDefinitions;
                bool hasRegister = false;
                bool hasVerifyLoad = false;
                for (int index = 0; index < steps.Count; index++)
                {
                    CCS_PlaytestStepType stepType = steps[index]?.StepType ?? CCS_PlaytestStepType.Spawn;
                    if (stepType == CCS_PlaytestStepType.RegisterUpkeepForLandClaim)
                    {
                        hasRegister = true;
                    }

                    if (stepType == CCS_PlaytestStepType.VerifyUpkeepAfterLoad)
                    {
                        hasVerifyLoad = true;
                    }
                }

                playtestSaveLoad = hasRegister && hasVerifyLoad;
            }

            report.AddIssue(
                playtestSaveLoad
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                playtestSaveLoad
                    ? "Upkeep playtest covers land claim registration and save/load verification."
                    : "Upkeep playtest missing register or save/load verification steps.");
        }

        private static void ValidateLandClaimIntegration(CCS_SurvivalValidationReport report)
        {
            bool compositionOk = File.Exists(CompositionRegistrationPath)
                && File.ReadAllText(CompositionRegistrationPath).Contains("WireUpkeepLandClaimIntegration");
            report.AddIssue(
                compositionOk
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                compositionOk
                    ? "Land claim placed event registers upkeep entries."
                    : "Composition missing WireUpkeepLandClaimIntegration.");
        }

        private static void ValidateLandOfficeHud(CCS_SurvivalValidationReport report)
        {
            bool hudOk = File.Exists(BankingHudPath)
                && File.ReadAllText(BankingHudPath).Contains("TryPayNearbyUpkeep")
                && File.ReadAllText(BankingHudPath).Contains("Upkeep status");
            report.AddIssue(
                hudOk ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                hudOk
                    ? "Land Office debug HUD shows upkeep status and pay action."
                    : "CCS_BankingDebugHud missing upkeep integration.");
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
                if (steps[index]?.StepType == CCS_PlaytestStepType.VerifyUpkeepAfterLoad)
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
                    ? "Upkeep playtest includes save/load verification step."
                    : "Run CCS_UpkeepFoundationBootstrapSetup.ExecuteBatch for upkeep playtest steps.");
        }

        private static void ValidateDocumentation(CCS_SurvivalValidationReport report)
        {
            bool exists = File.Exists(ModuleDocPath);
            report.AddIssue(
                exists ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                exists
                    ? "Upkeep module documentation present."
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
            string filePath)
        {
            ValidateRequiredFile(report, label, filePath);
        }
    }
}
