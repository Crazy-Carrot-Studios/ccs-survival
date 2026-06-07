using System.IO;
using CCS.Modules.Contracts;
using CCS.Modules.Playtesting;
using CCS.Modules.Settlements;
using CCS.Survival;
using CCS.Survival.Composition;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_ContractsFoundationValidationValidator
// CATEGORY: Modules / Contracts / Editor / Validation
// PURPOSE: Validates contracts module layout, content, composition, save, and playtest wiring.
// PLACEMENT: Registered on CCS_SurvivalValidationPipeline at editor load.
// AUTHOR: James Schilz
// CREATED: 2026-06-04
// NOTES: Milestone 3.0.0 frontier contracts foundation.
// =============================================================================

namespace CCS.Modules.Contracts.Editor
{
    public sealed class CCS_ContractsFoundationValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string ValidatorContext = "ccs.survival.validation.contracts";
        private const string ContractsMilestoneVersion = "3.0.0";
        private const string ModuleRoot = "Assets/CCS/Modules/Contracts";
        private const string ModuleRootMetaPath = "Assets/CCS/Modules/Contracts.meta";
        private const string RuntimeRoot = ModuleRoot + "/Runtime";
        private const string EditorRoot = ModuleRoot + "/Editor";
        private const string ModuleDocPath = ModuleRoot + "/Documentation/CCS_Contracts_Module.md";
        private const string BootstrapPrefabPath = "Assets/CCS/Survival/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
        private const string BootstrapScenePath = "Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string CompositionRegistrationPath =
            "Assets/CCS/Survival/Runtime/Composition/CCS_SurvivalGameplayServiceRegistration.cs";
        private const string RouteResolverPath =
            "Assets/CCS/Modules/Settlements/Runtime/Services/CCS_SettlementServiceRouteResolver.cs";
        private const string SaveDataPath = "Assets/CCS/Modules/SaveSystem/Runtime/Data/CCS_SaveData.cs";
        private const string SaveServicePath = "Assets/CCS/Modules/SaveSystem/Runtime/Services/CCS_SaveService.cs";
        private const string ContentContractsMetaPath = "Assets/CCS/Survival/Content/Contracts.meta";
        private const string ProfileContractsMetaPath = "Assets/CCS/Survival/Profiles/Contracts.meta";
        private const string DefaultPlaytestProfilePath = "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset";

        public string ValidatorId => ValidatorContext;

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateRequiredFolder(report, "Modules/Contracts", ModuleRoot);
            ValidateRequiredFolder(report, "Runtime/Data", RuntimeRoot + "/Data");
            ValidateRequiredFolder(report, "Runtime/Definitions", RuntimeRoot + "/Definitions");
            ValidateRequiredFolder(report, "Runtime/Profiles", RuntimeRoot + "/Profiles");
            ValidateRequiredFolder(report, "Runtime/Services", RuntimeRoot + "/Services");
            ValidateRequiredFolder(report, "Runtime/Validation", RuntimeRoot + "/Validation");
            ValidateRequiredFolder(report, "Runtime/UI", RuntimeRoot + "/UI");
            ValidateRequiredFolder(report, "Editor/Validation", EditorRoot + "/Validation");
            ValidateRequiredFolder(report, "Documentation", ModuleRoot + "/Documentation");

            ValidateRequiredFile(report, "Runtime asmdef", RuntimeRoot + "/CCS.Modules.Contracts.Runtime.asmdef");
            ValidateRequiredFile(report, "Editor asmdef", EditorRoot + "/CCS.Modules.Contracts.Editor.asmdef");
            ValidateRequiredFile(report, "Module root meta", ModuleRootMetaPath);
            ValidateFolderMeta(report, "Content/Contracts folder", ContentContractsMetaPath);
            ValidateFolderMeta(report, "Profiles/Contracts folder", ProfileContractsMetaPath);
            ValidateRequiredScript(report, "CCS_ContractService", RuntimeRoot + "/Services/CCS_ContractService.cs");
            ValidateRequiredScript(
                report,
                "CCS_ContractRuntimeBridge",
                RuntimeRoot + "/Services/CCS_ContractRuntimeBridge.cs");
            ValidateRequiredScript(
                report,
                "CCS_ContractValidationUtility",
                RuntimeRoot + "/Validation/CCS_ContractValidationUtility.cs");
            ValidateRequiredScript(
                report,
                "CCS_ContractsFoundationBootstrapSetup",
                EditorRoot + "/Validation/CCS_ContractsFoundationBootstrapSetup.cs");
            ValidateRequiredScript(report, "CCS_ContractDebugHud", RuntimeRoot + "/UI/CCS_ContractDebugHud.cs");

            ValidateContractProfile(report);
            ValidateStarterCatalog(report);
            ValidateCompositionRegistration(report);
            ValidateBootstrapWiring(report);
            ValidateSettlementRouting(report);
            ValidateSaveSupport(report);
            ValidatePlaytestSteps(report);
            ValidateDocumentation(report);

            CCS_SurvivalBootstrapVersionUtility.AddBundleVersionValidationIssue(
                report,
                ValidatorContext,
                ContractsMilestoneVersion,
                "Run CCS_ContractsFoundationBootstrapSetup.ExecuteBatch.");
            CCS_SurvivalBootstrapVersionUtility.ValidateNoHardcodedBootstrapVersionWrites(report, ValidatorContext);
            CCS_SurvivalInputValidationUtility.ValidateNoLegacyInputUsage(report, "Contracts Input Policy");
            CCS_SurvivalInputValidationUtility.ValidateNoFindObjectsSortModeUsage(report, "Contracts Obsolete API Scan");
        }

        private static void ValidateContractProfile(CCS_SurvivalValidationReport report)
        {
            CCS_ContractProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_ContractProfile>(CCS_ContractContentIds.DefaultContractProfilePath);
            CCS_SurvivalValidationResult result = CCS_ContractValidationUtility.ValidateProfile(profile);
            report.AddIssue(
                result.IsSuccess ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                result.Message);
        }

        private static void ValidateStarterCatalog(CCS_SurvivalValidationReport report)
        {
            string[] contractPaths =
            {
                CCS_ContractContentIds.LumberDeliveryContractPath,
                CCS_ContractContentIds.CornDeliveryContractPath,
                CCS_ContractContentIds.PotatoDeliveryContractPath,
                CCS_ContractContentIds.FeedDeliveryContractPath,
                CCS_ContractContentIds.MilkDeliveryContractPath,
                CCS_ContractContentIds.IronOreDeliveryContractPath,
                CCS_ContractContentIds.RefinedIronDeliveryContractPath,
                CCS_ContractContentIds.CharcoalDeliveryContractPath,
                CCS_ContractContentIds.MixedFrontierSupplyContractPath
            };

            for (int index = 0; index < contractPaths.Length; index++)
            {
                CCS_ContractDefinition definition =
                    AssetDatabase.LoadAssetAtPath<CCS_ContractDefinition>(contractPaths[index]);
                CCS_SurvivalValidationResult result = CCS_ContractValidationUtility.ValidateDefinition(definition);
                report.AddIssue(
                    result.IsSuccess
                        ? CCS_SurvivalValidationIssueSeverity.Info
                        : CCS_SurvivalValidationIssueSeverity.Error,
                    ValidatorContext,
                    result.Message);
            }
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
            bool hasService = source.Contains("CreateContractService", System.StringComparison.Ordinal);
            bool hasProfile = source.Contains("contractsProfile", System.StringComparison.Ordinal);
            bool hasSaveBind = source.Contains("contractService", System.StringComparison.Ordinal);
            report.AddIssue(
                hasService && hasProfile && hasSaveBind
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                hasService && hasProfile && hasSaveBind
                    ? "Composition registers contract service, profile, and save wiring."
                    : "Composition missing contract service, profile, or save wiring.");
        }

        private static void ValidateBootstrapWiring(CCS_SurvivalValidationReport report)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(BootstrapPrefabPath);
            CCS_SurvivalGameplayServiceHost host = prefab != null ? prefab.GetComponent<CCS_SurvivalGameplayServiceHost>() : null;
            SerializedObject serialized = host != null ? new SerializedObject(host) : null;
            bool ok = serialized != null
                && serialized.FindProperty("contractsProfile").objectReferenceValue != null;
            report.AddIssue(
                ok ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                ok
                    ? "Bootstrap host references contracts profile."
                    : "Run CCS_ContractsFoundationBootstrapSetup.ExecuteBatch for contracts profile wiring.");
        }

        private static void ValidateSettlementRouting(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(RouteResolverPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    ValidatorContext,
                    "CCS_SettlementServiceRouteResolver.cs was not found.");
                return;
            }

            string source = File.ReadAllText(RouteResolverPath);
            bool hasRoute = source.Contains("TryActivateContractBoardRoute", System.StringComparison.Ordinal);
            bool hasBridge = source.Contains("CCS_SettlementContractBoardActivationBridge", System.StringComparison.Ordinal);

            string compositionSource = File.Exists(CompositionRegistrationPath)
                ? File.ReadAllText(CompositionRegistrationPath)
                : string.Empty;
            bool hasCompositionBridge = compositionSource.Contains(
                "WireContractBoardActivation",
                System.StringComparison.Ordinal);

            report.AddIssue(
                hasRoute && hasBridge && hasCompositionBridge
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                hasRoute && hasBridge && hasCompositionBridge
                    ? "Settlement ContractBoard routing uses composition bridge without asmdef cycle."
                    : "Settlement contract board routing or composition bridge wiring is missing.");

            SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(BootstrapScenePath);
            if (sceneAsset == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    ValidatorContext,
                    $"Missing bootstrap scene: {BootstrapScenePath}");
                return;
            }

            bool sceneHasBoard = DoesSceneContainObject(BootstrapScenePath, CCS_SettlementContentIds.TestTradingPostContractBoardObjectName);
            report.AddIssue(
                sceneHasBoard ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                sceneHasBoard
                    ? "Bootstrap scene includes CCS_TestTradingPost_ContractBoard."
                    : "Run CCS_ContractsFoundationBootstrapSetup.ExecuteBatch for contract board scene object.");
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
            bool hasContractsData = saveDataSource.Contains("CCS_SaveContractsWorldData", System.StringComparison.Ordinal);
            bool hasSnapshots = saveDataSource.Contains("CCS_ContractSnapshot[] contractInstances", System.StringComparison.Ordinal);
            bool hasDynamicStates = saveDataSource.Contains("CCS_DynamicContractState[] dynamicContractStates", System.StringComparison.Ordinal);
            report.AddIssue(
                hasContractsData && hasSnapshots && hasDynamicStates
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                hasContractsData && hasSnapshots && hasDynamicStates
                    ? "Unified save payload includes contract and dynamic contract world data."
                    : "CCS_SaveData is missing contract save payload.");

            if (!File.Exists(SaveServicePath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    ValidatorContext,
                    "CCS_SaveService.cs was not found.");
                return;
            }

            string saveServiceSource = File.ReadAllText(SaveServicePath);
            bool captures = saveServiceSource.Contains("CaptureContractsState", System.StringComparison.Ordinal)
                && saveServiceSource.Contains("CaptureDynamicContractStates", System.StringComparison.Ordinal);
            bool restores = saveServiceSource.Contains("RestoreState(FilterStaticContractSnapshots", System.StringComparison.Ordinal)
                && saveServiceSource.Contains("RestorePersistedState", System.StringComparison.Ordinal);
            report.AddIssue(
                captures && restores
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                captures && restores
                    ? "Save service captures and restores contract state."
                    : "Save service missing contract capture/restore wiring.");
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
                if (steps[index]?.StepType == CCS_PlaytestStepType.VerifyContractStateAfterLoad)
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
                    ? "Contracts playtest includes save/load verification step."
                    : "Run CCS_ContractsFoundationBootstrapSetup.ExecuteBatch for contracts playtest steps.");
        }

        private static void ValidateDocumentation(CCS_SurvivalValidationReport report)
        {
            bool exists = File.Exists(ModuleDocPath);
            report.AddIssue(
                exists ? CCS_SurvivalValidationIssueSeverity.Info : CCS_SurvivalValidationIssueSeverity.Error,
                ValidatorContext,
                exists
                    ? "Contracts module documentation present."
                    : $"Missing module doc: {ModuleDocPath}");
        }

        private static bool DoesSceneContainObject(string scenePath, string objectName)
        {
            if (string.IsNullOrWhiteSpace(scenePath) || string.IsNullOrWhiteSpace(objectName))
            {
                return false;
            }

            string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { Path.GetDirectoryName(scenePath) });
            for (int index = 0; index < sceneGuids.Length; index++)
            {
                string path = AssetDatabase.GUIDToAssetPath(sceneGuids[index]);
                if (!string.Equals(path, scenePath, System.StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string[] dependencies = AssetDatabase.GetDependencies(path, true);
                for (int dependencyIndex = 0; dependencyIndex < dependencies.Length; dependencyIndex++)
                {
                    if (dependencies[dependencyIndex].Contains(objectName, System.StringComparison.Ordinal))
                    {
                        return true;
                    }
                }
            }

            return File.ReadAllText(scenePath).Contains(objectName, System.StringComparison.Ordinal);
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
