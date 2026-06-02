using System.Collections.Generic;
using System.IO;
using CCS.Modules.Playtesting;
using CCS.Modules.Regions;
using CCS.Modules.Settlements;
using CCS.Modules.WorldSimulation;
using CCS.Survival;
using CCS.Survival.Composition;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_WorldSimulationValidationValidator
// CATEGORY: Modules / WorldSimulation / Editor / Validation
// PURPOSE: Validates world simulation module layout, content, composition, and save wiring.
// PLACEMENT: Registered on CCS_SurvivalValidationPipeline at editor load.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// NOTES: Milestone 2.0.0 frontier world simulation foundation.
// =============================================================================

namespace CCS.Modules.WorldSimulation.Editor
{
    public sealed class CCS_WorldSimulationValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string ModuleRoot = "Assets/CCS/Modules/WorldSimulation";
        private const string RuntimeRoot = ModuleRoot + "/Runtime";
        private const string EditorRoot = ModuleRoot + "/Editor";
        private const string ModuleDocPath = ModuleRoot + "/Documentation/CCS_WorldSimulation_Module.md";
        private const string DefaultProfilePath = "Assets/CCS/Survival/Profiles/WorldSimulation/CCS_DefaultWorldSimulationProfile.asset";
        private const string DefaultSettlementProfilePath = "Assets/CCS/Survival/Profiles/Settlements/CCS_DefaultSettlementProfile.asset";
        private const string DefaultRegionProfilePath = "Assets/CCS/Survival/Profiles/Regions/CCS_DefaultRegionProfile.asset";
        private const string BootstrapPrefabPath = "Assets/CCS/Survival/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
        private const string CompositionRegistrationPath =
            "Assets/CCS/Survival/Runtime/Composition/CCS_SurvivalGameplayServiceRegistration.cs";
        private const string DefaultPlaytestProfilePath = "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset";

        public string ValidatorId => "ccs.survival.validation.worldsimulation";

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateRequiredFolder(report, "Modules/WorldSimulation", ModuleRoot);
            ValidateRequiredFolder(report, "Runtime/Data", RuntimeRoot + "/Data");
            ValidateRequiredFolder(report, "Runtime/Services", RuntimeRoot + "/Services");
            ValidateRequiredFolder(report, "Runtime/Profiles", RuntimeRoot + "/Profiles");
            ValidateRequiredFolder(report, "Runtime/Validation", RuntimeRoot + "/Validation");
            ValidateRequiredFolder(report, "Editor/Validation", EditorRoot + "/Validation");
            ValidateRequiredFolder(report, "Documentation", ModuleRoot + "/Documentation");

            ValidateRequiredFile(report, "Runtime asmdef", RuntimeRoot + "/CCS.Modules.WorldSimulation.Runtime.asmdef");
            ValidateRequiredFile(report, "Editor asmdef", EditorRoot + "/CCS.Modules.WorldSimulation.Editor.asmdef");
            ValidateRequiredScript(report, "CCS_WorldSimulationService", RuntimeRoot + "/Services/CCS_WorldSimulationService.cs");
            ValidateRequiredScript(report, "CCS_WorldSimulationRuntimeBridge", RuntimeRoot + "/Services/CCS_WorldSimulationRuntimeBridge.cs");

            ValidateWorldSimulationContent(report);
            ValidateCompositionRegistration(report);
            ValidateBootstrapWiring(report);
            ValidateSaveSupport(report);
            ValidatePlaytestWorldSimulationSteps(report);
            ValidateDocumentation(report);

            CCS_SurvivalBootstrapVersionUtility.AddBundleVersionValidationIssue(
                report,
                "World Simulation Version",
                CCS_SurvivalBootstrapVersionUtility.CurrentMilestoneVersion,
                "Run CCS_WorldSimulationBootstrapSetup.ExecuteBatch.");
            CCS_SurvivalBootstrapVersionUtility.ValidateNoHardcodedBootstrapVersionWrites(report, "World Simulation Version");
            CCS_SurvivalInputValidationUtility.ValidateNoLegacyInputUsage(report, "World Simulation Input Policy");
            CCS_SurvivalInputValidationUtility.ValidateNoFindObjectsSortModeUsage(report, "World Simulation Obsolete API Scan");
        }

        private static void ValidateWorldSimulationContent(CCS_SurvivalValidationReport report)
        {
            CCS_WorldSimulationProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_WorldSimulationProfile>(DefaultProfilePath);
            if (profile == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "World Simulation Profile",
                    $"Missing default profile: {DefaultProfilePath}");
                return;
            }

            CCS_SurvivalValidationResult validation = CCS_WorldSimulationValidationUtility.ValidateProfile(profile);
            report.AddIssue(
                validation.IsSuccess
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "World Simulation Profile",
                validation.Message);

            HashSet<string> settlementIds = new HashSet<string>();
            CCS_WorldSimulationSettlementProfileEntry[] settlementEntries = profile.SettlementEntries;
            for (int index = 0; index < settlementEntries.Length; index++)
            {
                CCS_WorldSimulationSettlementProfileEntry entry = settlementEntries[index];
                if (entry == null || string.IsNullOrWhiteSpace(entry.settlementId))
                {
                    continue;
                }

                if (!settlementIds.Add(entry.settlementId))
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "World Simulation Profile",
                        $"Duplicate settlement simulation id '{entry.settlementId}'.");
                }
            }

            ValidateSettlementReferences(report, profile);
            ValidateRegionReferences(report, profile);
        }

        private static void ValidateSettlementReferences(CCS_SurvivalValidationReport report, CCS_WorldSimulationProfile profile)
        {
            CCS_SettlementProfile settlementProfile =
                AssetDatabase.LoadAssetAtPath<CCS_SettlementProfile>(DefaultSettlementProfilePath);
            if (settlementProfile == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Warning,
                    "World Simulation Settlement References",
                    $"Missing settlement profile: {DefaultSettlementProfilePath}");
                return;
            }

            List<string> knownSettlementIds = new List<string>();
            CCS_SettlementDefinition[] definitions = settlementProfile.SettlementDefinitions;
            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_SettlementDefinition definition = definitions[index];
                if (definition != null && !string.IsNullOrWhiteSpace(definition.SettlementId))
                {
                    knownSettlementIds.Add(definition.SettlementId);
                }
            }

            CCS_SurvivalValidationResult validation = CCS_WorldSimulationValidationUtility.ValidateSettlementReferences(
                profile,
                knownSettlementIds.ToArray());
            report.AddIssue(
                validation.IsSuccess
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "World Simulation Settlement References",
                validation.Message);
        }

        private static void ValidateRegionReferences(CCS_SurvivalValidationReport report, CCS_WorldSimulationProfile profile)
        {
            CCS_RegionProfile regionProfile = AssetDatabase.LoadAssetAtPath<CCS_RegionProfile>(DefaultRegionProfilePath);
            if (regionProfile == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Warning,
                    "World Simulation Region References",
                    $"Missing region profile: {DefaultRegionProfilePath}");
                return;
            }

            List<string> knownRegionIds = new List<string>();
            CCS_RegionDefinition[] definitions = regionProfile.RegionDefinitions;
            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_RegionDefinition definition = definitions[index];
                if (definition != null && !string.IsNullOrWhiteSpace(definition.RegionId))
                {
                    knownRegionIds.Add(definition.RegionId);
                }
            }

            CCS_SurvivalValidationResult validation = CCS_WorldSimulationValidationUtility.ValidateRegionReferences(
                profile,
                knownRegionIds.ToArray());
            report.AddIssue(
                validation.IsSuccess
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "World Simulation Region References",
                validation.Message);
        }

        private static void ValidateCompositionRegistration(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(CompositionRegistrationPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "World Simulation Composition",
                    $"Missing composition registration: {CompositionRegistrationPath}");
                return;
            }

            string source = File.ReadAllText(CompositionRegistrationPath);
            if (source.Contains("CreateWorldSimulationService")
                && source.Contains("CCS_WorldSimulationService")
                && source.Contains("HandleVendorTransactionCompleted"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "World Simulation Composition",
                    "Gameplay composition registers world simulation and vendor integration.");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "World Simulation Composition",
                    "CCS_SurvivalGameplayServiceRegistration is missing world simulation wiring.");
            }
        }

        private static void ValidateBootstrapWiring(CCS_SurvivalValidationReport report)
        {
            GameObject prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(BootstrapPrefabPath);
            if (prefabRoot == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "World Simulation Bootstrap",
                    $"Missing bootstrap prefab: {BootstrapPrefabPath}");
                return;
            }

            CCS_SurvivalGameplayServiceHost host = prefabRoot.GetComponent<CCS_SurvivalGameplayServiceHost>();
            if (host == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "World Simulation Bootstrap",
                    "Bootstrap prefab missing CCS_SurvivalGameplayServiceHost.");
                return;
            }

            SerializedObject serializedHost = new SerializedObject(host);
            Object profileReference = serializedHost.FindProperty("worldSimulationProfile").objectReferenceValue;
            if (profileReference == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "World Simulation Bootstrap",
                    "Bootstrap host is missing worldSimulationProfile assignment.");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,
                "World Simulation Bootstrap",
                "Bootstrap host references default world simulation profile.");
        }

        private static void ValidateSaveSupport(CCS_SurvivalValidationReport report)
        {
            const string saveDataPath = "Assets/CCS/Modules/SaveSystem/Runtime/Data/CCS_SaveData.cs";
            const string saveServicePath = "Assets/CCS/Modules/SaveSystem/Runtime/Services/CCS_SaveService.cs";
            if (!File.Exists(saveDataPath) || !File.Exists(saveServicePath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "World Simulation Save",
                    "Save system files missing for world simulation validation.");
                return;
            }

            string saveDataSource = File.ReadAllText(saveDataPath);
            string saveServiceSource = File.ReadAllText(saveServicePath);
            if (saveDataSource.Contains("CCS_SaveWorldSimulationData")
                && saveServiceSource.Contains("CaptureWorldSimulation")
                && saveServiceSource.Contains("ApplyWorldSimulation"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "World Simulation Save",
                    "Unified save payload includes world simulation state.");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "World Simulation Save",
                    "Save system is missing world simulation capture/apply integration.");
            }
        }

        private static void ValidatePlaytestWorldSimulationSteps(CCS_SurvivalValidationReport report)
        {
            CCS_PlaytestProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(DefaultPlaytestProfilePath);
            if (profile == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Warning,
                    "World Simulation Playtest",
                    $"Missing playtest profile: {DefaultPlaytestProfilePath}");
                return;
            }

            bool hasDiscover = false;
            bool hasSave = false;
            bool hasLoadVerify = false;
            IReadOnlyList<CCS_PlaytestStepDefinition> steps = profile.StepDefinitions;
            for (int index = 0; index < steps.Count; index++)
            {
                CCS_PlaytestStepDefinition step = steps[index];
                if (step == null)
                {
                    continue;
                }

                if (step.StepType == CCS_PlaytestStepType.DiscoverSettlementForWorldSimulation)
                {
                    hasDiscover = true;
                }

                if (step.StepType == CCS_PlaytestStepType.SaveWorldSimulationState)
                {
                    hasSave = true;
                }

                if (step.StepType == CCS_PlaytestStepType.VerifyWorldSimulationRestoredAfterLoad)
                {
                    hasLoadVerify = true;
                }
            }

            if (hasDiscover && hasSave && hasLoadVerify)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "World Simulation Playtest",
                    "Bootstrap playtest profile includes world simulation loop steps.");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "World Simulation Playtest",
                    "Playtest profile is missing world simulation steps. Run world simulation bootstrap.");
            }
        }

        private static void ValidateDocumentation(CCS_SurvivalValidationReport report)
        {
            if (File.Exists(ModuleDocPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "World Simulation Documentation",
                    "Module documentation present.");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "World Simulation Documentation",
                    $"Missing module documentation: {ModuleDocPath}");
            }
        }

        private static void ValidateRequiredFolder(CCS_SurvivalValidationReport report, string label, string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                report.AddIssue(CCS_SurvivalValidationIssueSeverity.Info, "World Simulation Layout", $"{label} folder present.");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "World Simulation Layout",
                    $"Missing required folder: {path}");
            }
        }

        private static void ValidateRequiredFile(CCS_SurvivalValidationReport report, string label, string path)
        {
            if (File.Exists(path))
            {
                report.AddIssue(CCS_SurvivalValidationIssueSeverity.Info, "World Simulation Layout", $"{label} present.");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "World Simulation Layout",
                    $"Missing required file: {path}");
            }
        }

        private static void ValidateRequiredScript(CCS_SurvivalValidationReport report, string label, string path)
        {
            ValidateRequiredFile(report, label, path);
        }
    }
}
