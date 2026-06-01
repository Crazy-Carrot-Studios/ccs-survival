using System.IO;
using CCS.Survival;
using CCS.Survival.Composition;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_GatheringValidationValidator
// CATEGORY: Modules / Gathering / Editor / Validation
// PURPOSE: Validates gathering module layout, assets, bootstrap wiring, and rewards.
// PLACEMENT: Registered on CCS_SurvivalValidationPipeline at editor load.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Validates primitive gathering foundation for milestone 0.9.9.
// =============================================================================

namespace CCS.Modules.Gathering.Editor
{
    public sealed class CCS_GatheringValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string ModuleRoot = "Assets/CCS/Modules/Gathering";
        private const string RuntimeRoot = ModuleRoot + "/Runtime";
        private const string EditorRoot = ModuleRoot + "/Editor";
        private const string SurvivalRoot = "Assets/CCS/Survival";
        private const string DefaultProfilePath = SurvivalRoot + "/Profiles/Gathering/CCS_DefaultGatheringProfile.asset";
        private const string ModuleDocPath = ModuleRoot + "/Documentation/CCS_Gathering_Module.md";
        private const string BootstrapScenePath = SurvivalRoot + "/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string BootstrapPrefabPath = SurvivalRoot + "/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
        private const string CompositionRegistrationPath =
            SurvivalRoot + "/Runtime/Composition/CCS_SurvivalGameplayServiceRegistration.cs";

        private static readonly string[] RequiredBootstrapNodeNames =
        {
            "CCS_TestGatheringSmallTree",
            "CCS_TestGatheringRock",
            "CCS_TestGatheringBush"
        };

        #region Properties

        public string ValidatorId => "ccs.survival.validation.gathering";

        #endregion

        #region Public Methods

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateRequiredFolder(report, "Modules/Gathering", ModuleRoot);
            ValidateRequiredFolder(report, "Runtime/Data", RuntimeRoot + "/Data");
            ValidateRequiredFolder(report, "Runtime/Profiles", RuntimeRoot + "/Profiles");
            ValidateRequiredFolder(report, "Runtime/Services", RuntimeRoot + "/Services");
            ValidateRequiredFolder(report, "Runtime/Events", RuntimeRoot + "/Events");
            ValidateRequiredFolder(report, "Runtime/Nodes", RuntimeRoot + "/Nodes");
            ValidateRequiredFolder(report, "Runtime/Interaction", RuntimeRoot + "/Interaction");
            ValidateRequiredFolder(report, "Runtime/Validation", RuntimeRoot + "/Validation");
            ValidateRequiredFolder(report, "Editor/Validation", EditorRoot + "/Validation");
            ValidateRequiredFolder(report, "Documentation", ModuleRoot + "/Documentation");

            ValidateRequiredFile(report, "Runtime asmdef", RuntimeRoot + "/CCS.Modules.Gathering.Runtime.asmdef");
            ValidateRequiredFile(report, "Editor asmdef", EditorRoot + "/CCS.Modules.Gathering.Editor.asmdef");

            ValidateRequiredScript(report, "CCS_GatheringResourceType", RuntimeRoot + "/Data/CCS_GatheringResourceType.cs");
            ValidateRequiredScript(report, "CCS_GatheringNodeType", RuntimeRoot + "/Data/CCS_GatheringNodeType.cs");
            ValidateRequiredScript(report, "CCS_GatheringReward", RuntimeRoot + "/Data/CCS_GatheringReward.cs");
            ValidateRequiredScript(report, "CCS_GatheringResult", RuntimeRoot + "/Data/CCS_GatheringResult.cs");
            ValidateRequiredScript(report, "CCS_GatheringProfile", RuntimeRoot + "/Profiles/CCS_GatheringProfile.cs");
            ValidateRequiredScript(report, "CCS_GatheringNode", RuntimeRoot + "/Nodes/CCS_GatheringNode.cs");
            ValidateRequiredScript(report, "CCS_GatheringService", RuntimeRoot + "/Services/CCS_GatheringService.cs");
            ValidateRequiredScript(report, "CCS_GatheringRuntimeBridge", RuntimeRoot + "/Services/CCS_GatheringRuntimeBridge.cs");
            ValidateRequiredScript(report, "CCS_GatheringEventArgs", RuntimeRoot + "/Events/CCS_GatheringEventArgs.cs");
            ValidateRequiredScript(report, "CCS_GatheringEvents", RuntimeRoot + "/Events/CCS_GatheringEvents.cs");
            ValidateRequiredScript(report, "CCS_GatheringInteractable", RuntimeRoot + "/Interaction/CCS_GatheringInteractable.cs");
            ValidateRequiredScript(report, "CCS_GatheringValidationUtility", RuntimeRoot + "/Validation/CCS_GatheringValidationUtility.cs");

            ValidateDocumentationAsset(report, "Gathering Module Doc", ModuleDocPath);
            ValidateRequiredAsset(report, "Default Gathering Profile", DefaultProfilePath);
            ValidateGatheringProfileAsset(report);
            ValidateCompositionRegistration(report);
            ValidateBootstrapGameplayServiceHost(report);
            ValidateBootstrapGatheringNodes(report);
            ValidateRuntimeScriptsAvoidUnityEditor(report, RuntimeRoot);
        }

        #endregion

        #region Private Methods

        private static void ValidateGatheringProfileAsset(CCS_SurvivalValidationReport report)
        {
            CCS_GatheringProfile profile = AssetDatabase.LoadAssetAtPath<CCS_GatheringProfile>(DefaultProfilePath);
            if (profile == null)
            {
                return;
            }

            CCS_SurvivalValidationResult validation = CCS_GatheringValidationUtility.ValidateProfile(profile);
            report.AddIssue(
                validation.IsSuccess
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Default Gathering Profile Validation",
                validation.Message);

            if (profile.ProfileVersion != "0.9.9")
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Gathering Profile Version",
                    $"Expected profileVersion 0.9.9 but found '{profile.ProfileVersion}'.");
            }
        }

        private static void ValidateCompositionRegistration(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(CompositionRegistrationPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Gathering Service Registration",
                    $"Missing script: {CompositionRegistrationPath}");
                return;
            }

            string source = File.ReadAllText(CompositionRegistrationPath);
            report.AddIssue(
                source.Contains("CreateGatheringService")
                    && source.Contains("CCS_GatheringService")
                    && source.Contains("gatheringProfile")
                    && source.Contains("RegisterService(runtimeHost, gatheringService")
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Gathering Service Registration",
                "Gameplay composition registers and initializes CCS_GatheringService.");
        }

        private static void ValidateBootstrapGameplayServiceHost(CCS_SurvivalValidationReport report)
        {
            GameObject prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(BootstrapPrefabPath);
            if (prefabRoot == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Bootstrap Gathering Profile Wiring",
                    $"Missing prefab: {BootstrapPrefabPath}");
                return;
            }

            CCS_SurvivalGameplayServiceHost host = prefabRoot.GetComponent<CCS_SurvivalGameplayServiceHost>();
            if (host == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Bootstrap Gathering Profile Wiring",
                    "PF_CCS_Survival_BootstrapRoot is missing CCS_SurvivalGameplayServiceHost.");
                return;
            }

            SerializedObject serializedHost = new SerializedObject(host);
            Object gatheringProfile = serializedHost.FindProperty("gatheringProfile").objectReferenceValue;
            report.AddIssue(
                gatheringProfile != null
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Bootstrap Gathering Profile Wiring",
                gatheringProfile != null
                    ? "Bootstrap gameplay host references CCS_DefaultGatheringProfile."
                    : "Bootstrap gameplay host is missing gatheringProfile assignment.");
        }

        private static void ValidateBootstrapGatheringNodes(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(BootstrapScenePath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Bootstrap Scene",
                    $"Missing scene: {BootstrapScenePath}");
                return;
            }

            string sceneText = File.ReadAllText(BootstrapScenePath);
            for (int index = 0; index < RequiredBootstrapNodeNames.Length; index++)
            {
                string objectName = RequiredBootstrapNodeNames[index];
                report.AddIssue(
                    sceneText.Contains(objectName)
                        ? CCS_SurvivalValidationIssueSeverity.Info
                        : CCS_SurvivalValidationIssueSeverity.Error,
                    "Bootstrap Gathering Nodes",
                    sceneText.Contains(objectName)
                        ? $"Bootstrap scene contains {objectName}."
                        : $"Bootstrap scene is missing {objectName}.");
            }

            report.AddIssue(
                sceneText.Contains("CCS_GatheringNode")
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Bootstrap Gathering Components",
                "Bootstrap scene includes CCS_GatheringNode components.");
        }

        private static void ValidateRequiredFolder(
            CCS_SurvivalValidationReport report,
            string label,
            string folderPath)
        {
            report.AddIssue(
                AssetDatabase.IsValidFolder(folderPath) || Directory.Exists(folderPath)
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                label,
                AssetDatabase.IsValidFolder(folderPath) || Directory.Exists(folderPath)
                    ? $"Folder exists: {folderPath}"
                    : $"Missing folder: {folderPath}");
        }

        private static void ValidateRequiredFile(
            CCS_SurvivalValidationReport report,
            string label,
            string assetPath)
        {
            report.AddIssue(
                File.Exists(assetPath)
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                label,
                File.Exists(assetPath)
                    ? $"File exists: {assetPath}"
                    : $"Missing file: {assetPath}");
        }

        private static void ValidateRequiredScript(
            CCS_SurvivalValidationReport report,
            string label,
            string scriptPath)
        {
            ValidateRequiredFile(report, label, scriptPath);
        }

        private static void ValidateRequiredAsset(
            CCS_SurvivalValidationReport report,
            string label,
            string assetPath)
        {
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            report.AddIssue(
                asset != null
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                label,
                asset != null
                    ? $"Asset exists: {assetPath}"
                    : $"Missing asset: {assetPath}");
        }

        private static void ValidateDocumentationAsset(
            CCS_SurvivalValidationReport report,
            string label,
            string assetPath)
        {
            report.AddIssue(
                File.Exists(assetPath)
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Warning,
                label,
                File.Exists(assetPath)
                    ? $"Documentation present: {assetPath}"
                    : $"Documentation missing: {assetPath}");
        }

        private static void ValidateRuntimeScriptsAvoidUnityEditor(
            CCS_SurvivalValidationReport report,
            string runtimeFolderPath)
        {
            if (!Directory.Exists(runtimeFolderPath))
            {
                return;
            }

            string[] scriptPaths = Directory.GetFiles(runtimeFolderPath, "*.cs", SearchOption.AllDirectories);
            for (int index = 0; index < scriptPaths.Length; index++)
            {
                string contents = File.ReadAllText(scriptPaths[index]);
                if (contents.Contains("using UnityEditor"))
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Runtime Editor Leak",
                        $"{scriptPaths[index]} references UnityEditor.");
                }
            }
        }

        #endregion
    }
}
