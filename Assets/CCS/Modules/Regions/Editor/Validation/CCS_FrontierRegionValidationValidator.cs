using System.Collections.Generic;
using System.IO;
using CCS.Modules.Playtesting;
using CCS.Modules.Settlements;
using CCS.Survival;
using CCS.Survival.Composition;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_FrontierRegionValidationValidator
// CATEGORY: Modules / Regions / Editor / Validation
// PURPOSE: Validates region module layout, content, composition, and bootstrap wiring.
// PLACEMENT: Registered on CCS_SurvivalValidationPipeline at editor load.
// AUTHOR: James Schilz
// CREATED: 2026-06-02
// NOTES: Milestone 1.9.0 frontier region foundation.
// =============================================================================

namespace CCS.Modules.Regions.Editor
{
    public sealed class CCS_FrontierRegionValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string ModuleRoot = "Assets/CCS/Modules/Regions";
        private const string RuntimeRoot = ModuleRoot + "/Runtime";
        private const string EditorRoot = ModuleRoot + "/Editor";
        private const string ModuleDocPath = ModuleRoot + "/Documentation/CCS_Regions_Module.md";
        private const string DefaultRegionProfilePath = "Assets/CCS/Survival/Profiles/Regions/CCS_DefaultRegionProfile.asset";
        private const string BootstrapPrefabPath = "Assets/CCS/Survival/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
        private const string BootstrapScenePath = "Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string CompositionRegistrationPath =
            "Assets/CCS/Survival/Runtime/Composition/CCS_SurvivalGameplayServiceRegistration.cs";
        private const string DefaultSettlementProfilePath = "Assets/CCS/Survival/Profiles/Settlements/CCS_DefaultSettlementProfile.asset";

        public string ValidatorId => "ccs.survival.validation.regions";

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateRequiredFolder(report, "Modules/Regions", ModuleRoot);
            ValidateRequiredFolder(report, "Runtime/Services", RuntimeRoot + "/Services");
            ValidateRequiredFolder(report, "Runtime/Components", RuntimeRoot + "/Components");
            ValidateRequiredFolder(report, "Runtime/Definitions", RuntimeRoot + "/Definitions");
            ValidateRequiredFolder(report, "Runtime/Profiles", RuntimeRoot + "/Profiles");
            ValidateRequiredFolder(report, "Runtime/Validation", RuntimeRoot + "/Validation");
            ValidateRequiredFolder(report, "Editor/Validation", EditorRoot + "/Validation");
            ValidateRequiredFolder(report, "Documentation", ModuleRoot + "/Documentation");

            ValidateRequiredFile(report, "Runtime asmdef", RuntimeRoot + "/CCS.Modules.Regions.Runtime.asmdef");
            ValidateRequiredFile(report, "Editor asmdef", EditorRoot + "/CCS.Modules.Regions.Editor.asmdef");
            ValidateRequiredScript(report, "CCS_RegionService", RuntimeRoot + "/Services/CCS_RegionService.cs");
            ValidateRequiredScript(report, "CCS_RegionVolume", RuntimeRoot + "/Components/CCS_RegionVolume.cs");
            ValidateRequiredScript(report, "CCS_RegionRuntimeBridge", RuntimeRoot + "/Services/CCS_RegionRuntimeBridge.cs");

            ValidateRegionContent(report);
            ValidateCompositionRegistration(report);
            ValidateBootstrapWiring(report);
            ValidateSaveSupport(report);
            ValidatePlaytestRegionSteps(report);
            ValidateDocumentation(report);

            CCS_SurvivalBootstrapVersionUtility.AddBundleVersionValidationIssue(
                report,
                "Regions Version",
                CCS_SurvivalBootstrapVersionUtility.CurrentMilestoneVersion,
                "Run CCS_FrontierRegionBootstrapSetup.ExecuteBatch.");
            CCS_SurvivalBootstrapVersionUtility.ValidateNoHardcodedBootstrapVersionWrites(report, "Regions Version");
            CCS_SurvivalInputValidationUtility.ValidateNoLegacyInputUsage(report, "Regions Input Policy");
            CCS_SurvivalInputValidationUtility.ValidateNoFindObjectsSortModeUsage(report, "Regions Obsolete API Scan");
        }

        private static void ValidateRegionContent(CCS_SurvivalValidationReport report)
        {
            CCS_RegionProfile profile = AssetDatabase.LoadAssetAtPath<CCS_RegionProfile>(DefaultRegionProfilePath);
            if (profile == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Regions Profile",
                    $"Missing default region profile: {DefaultRegionProfilePath}");
                return;
            }

            CCS_SurvivalValidationResult validation = CCS_RegionValidationUtility.ValidateProfile(profile);
            report.AddIssue(
                validation.IsSuccess
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Regions Profile",
                validation.Message);

            HashSet<string> regionIds = new HashSet<string>();
            CCS_RegionDefinition[] definitions = profile.RegionDefinitions;
            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_RegionDefinition definition = definitions[index];
                if (definition == null || string.IsNullOrWhiteSpace(definition.RegionId))
                {
                    continue;
                }

                if (!regionIds.Add(definition.RegionId))
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Regions Profile",
                        $"Duplicate region id '{definition.RegionId}'.");
                }
            }

            if (regionIds.Contains(CCS_RegionContentIds.PineRidgeForestRegionId)
                && regionIds.Contains(CCS_RegionContentIds.BrokenCreekRegionId)
                && regionIds.Contains(CCS_RegionContentIds.IronRidgeMineRegionId)
                && regionIds.Contains(CCS_RegionContentIds.FrontierTradingPostRegionId))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Regions Content",
                    "Bootstrap frontier region definitions are present.");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Regions Content",
                    "Bootstrap region definitions are incomplete. Run region bootstrap.");
            }

            ValidateSettlementAssignments(report, profile);
        }

        private static void ValidateSettlementAssignments(CCS_SurvivalValidationReport report, CCS_RegionProfile profile)
        {
            CCS_SettlementProfile settlementProfile =
                AssetDatabase.LoadAssetAtPath<CCS_SettlementProfile>(DefaultSettlementProfilePath);
            if (settlementProfile == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Warning,
                    "Regions Settlement Ownership",
                    $"Missing settlement profile for cross-validation: {DefaultSettlementProfilePath}");
                return;
            }

            List<string> settlementIds = new List<string>();
            CCS_SettlementDefinition[] definitions = settlementProfile.SettlementDefinitions;
            for (int index = 0; index < definitions.Length; index++)
            {
                CCS_SettlementDefinition definition = definitions[index];
                if (definition != null && !string.IsNullOrWhiteSpace(definition.SettlementId))
                {
                    settlementIds.Add(definition.SettlementId);
                }
            }

            CCS_SurvivalValidationResult validation = CCS_RegionValidationUtility.ValidateSettlementAssignments(
                profile,
                settlementIds.ToArray());
            report.AddIssue(
                validation.IsSuccess
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Regions Settlement Ownership",
                validation.Message);
        }

        private static void ValidateCompositionRegistration(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(CompositionRegistrationPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Regions Composition",
                    $"Missing file: {CompositionRegistrationPath}");
                return;
            }

            string source = File.ReadAllText(CompositionRegistrationPath);
            if (source.Contains("CreateRegionService") && source.Contains("CCS_RegionService"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Regions Composition",
                    "CCS_RegionService is registered in gameplay composition.");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Regions Composition",
                    "CCS_RegionService registration is missing.");
            }
        }

        private static void ValidateBootstrapWiring(CCS_SurvivalValidationReport report)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(BootstrapPrefabPath);
            if (prefab == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Regions Bootstrap",
                    $"Missing prefab: {BootstrapPrefabPath}");
                return;
            }

            CCS_SurvivalGameplayServiceHost host = prefab.GetComponent<CCS_SurvivalGameplayServiceHost>();
            if (host == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Regions Bootstrap",
                    "Bootstrap prefab is missing CCS_SurvivalGameplayServiceHost.");
                return;
            }

            SerializedObject serializedHost = new SerializedObject(host);
            if (serializedHost.FindProperty("regionProfile").objectReferenceValue == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Regions Bootstrap",
                    "Gameplay service host regionProfile is not assigned.");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Regions Bootstrap",
                    "Gameplay service host references region profile.");
            }

            string sceneText = File.Exists(BootstrapScenePath) ? File.ReadAllText(BootstrapScenePath) : string.Empty;
            ValidateSceneRegionVolumes(report, sceneText);
        }

        private static void ValidateSceneRegionVolumes(CCS_SurvivalValidationReport report, string sceneText)
        {
            string[] requiredVolumeNames =
            {
                CCS_RegionContentIds.PineRidgeForestVolumeObjectName,
                CCS_RegionContentIds.BrokenCreekVolumeObjectName,
                CCS_RegionContentIds.IronRidgeMineVolumeObjectName,
                CCS_RegionContentIds.FrontierTradingPostRegionVolumeObjectName
            };

            bool allVolumesPresent = true;
            for (int index = 0; index < requiredVolumeNames.Length; index++)
            {
                if (!sceneText.Contains(requiredVolumeNames[index]))
                {
                    allVolumesPresent = false;
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Warning,
                        "Regions Bootstrap Volumes",
                        $"Bootstrap scene may be missing volume '{requiredVolumeNames[index]}'. Run region bootstrap.");
                }
            }

            if (allVolumesPresent && sceneText.Contains("CCS.Modules.Regions.CCS_RegionVolume"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Regions Bootstrap Volumes",
                    "Bootstrap scene contains frontier region trigger volumes.");
            }
        }

        private static void ValidateSaveSupport(CCS_SurvivalValidationReport report)
        {
            const string saveDataPath = "Assets/CCS/Modules/SaveSystem/Runtime/Data/CCS_SaveData.cs";
            if (!File.Exists(saveDataPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Regions Save",
                    $"Missing save data file: {saveDataPath}");
                return;
            }

            string saveDataSource = File.ReadAllText(saveDataPath);
            if (saveDataSource.Contains("CCS_SaveRegionsWorldData"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Regions Save",
                    "Unified save payload includes region discovery data.");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Regions Save",
                    "CCS_SaveData is missing CCS_SaveRegionsWorldData.");
            }
        }

        private static void ValidatePlaytestRegionSteps(CCS_SurvivalValidationReport report)
        {
            const string playtestProfilePath = "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset";
            CCS_PlaytestProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(playtestProfilePath);
            if (profile == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Warning,
                    "Regions Playtest",
                    $"Missing playtest profile: {playtestProfilePath}");
                return;
            }

            bool hasSave = false;
            bool hasLoadVerify = false;
            bool hasVerifyAll = false;
            IReadOnlyList<CCS_PlaytestStepDefinition> steps = profile.StepDefinitions;
            for (int index = 0; index < steps.Count; index++)
            {
                CCS_PlaytestStepDefinition step = steps[index];
                if (step == null)
                {
                    continue;
                }

                if (step.StepType == CCS_PlaytestStepType.SaveRegionDiscovery)
                {
                    hasSave = true;
                }

                if (step.StepType == CCS_PlaytestStepType.VerifyRegionDiscoveryAfterLoad)
                {
                    hasLoadVerify = true;
                }

                if (step.StepType == CCS_PlaytestStepType.VerifyAllRegionsDiscovered)
                {
                    hasVerifyAll = true;
                }
            }

            if (hasSave && hasLoadVerify && hasVerifyAll)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Regions Playtest",
                    "Region playtest includes discover-all, save, and load verification steps.");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Regions Playtest",
                    "Region playtest steps are incomplete. Run region bootstrap.");
            }
        }

        private static void ValidateDocumentation(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(ModuleDocPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Regions Documentation",
                    $"Missing module documentation: {ModuleDocPath}");
                return;
            }

            string documentation = File.ReadAllText(ModuleDocPath);
            if (documentation.Contains("Frontier Region Loop") && documentation.Contains("Pine Ridge Forest"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Regions Documentation",
                    "Region module documentation validated.");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Warning,
                    "Regions Documentation",
                    "Region module documentation missing frontier loop details.");
            }
        }

        private static void ValidateRequiredFolder(
            CCS_SurvivalValidationReport report,
            string context,
            string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    context,
                    $"Folder present: {folderPath}");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Error,
                context,
                $"Missing folder: {folderPath}");
        }

        private static void ValidateRequiredFile(
            CCS_SurvivalValidationReport report,
            string context,
            string filePath)
        {
            if (File.Exists(filePath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    context,
                    $"File present: {filePath}");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Error,
                context,
                $"Missing file: {filePath}");
        }

        private static void ValidateRequiredScript(
            CCS_SurvivalValidationReport report,
            string context,
            string scriptPath)
        {
            ValidateRequiredFile(report, context, scriptPath);
        }
    }
}
