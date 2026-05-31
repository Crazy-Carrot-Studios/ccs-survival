using System.IO;
using CCS.Modules.Interaction;
using CCS.Modules.WorldResources;
using CCS.Survival;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_WorldResourceValidationValidator
// CATEGORY: Modules / WorldResources / Editor / Validation
// PURPOSE: Validates world resource module folders, asmdefs, profile asset, and scripts.
// PLACEMENT: Registered on CCS_SurvivalValidationPipeline at editor load.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Validates bootstrap test resource placeholders in SCN_CCS_Survival_Bootstrap.
// =============================================================================

namespace CCS.Modules.WorldResources.Editor
{
    public sealed class CCS_WorldResourceValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string ModuleRoot = "Assets/CCS/Modules/WorldResources";
        private const string RuntimeRoot = ModuleRoot + "/Runtime";
        private const string EditorRoot = ModuleRoot + "/Editor";
        private const string SurvivalRoot = "Assets/CCS/Survival";
        private const string DefaultProfilePath =
            SurvivalRoot + "/Profiles/WorldResources/CCS_DefaultWorldResourceProfile.asset";
        private const string ModuleDocPath = ModuleRoot + "/Documentation/CCS_World_Resources_Module.md";
        private const string BootstrapScenePath = SurvivalRoot + "/Scenes/SCN_CCS_Survival_Bootstrap.unity";

        private static readonly string[] RequiredTestObjectNames =
        {
            "CCS_TestTree",
            "CCS_TestRock",
            "CCS_TestPlant"
        };

        #region Properties

        public string ValidatorId => "ccs.survival.validation.worldresources";

        #endregion

        #region Public Methods

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateRequiredFolder(report, "Modules/WorldResources", ModuleRoot);
            ValidateRequiredFolder(report, "Runtime/Definitions", RuntimeRoot + "/Definitions");
            ValidateRequiredFolder(report, "Runtime/Data", RuntimeRoot + "/Data");
            ValidateRequiredFolder(report, "Runtime/Harvesting", RuntimeRoot + "/Harvesting");
            ValidateRequiredFolder(report, "Runtime/Respawn", RuntimeRoot + "/Respawn");
            ValidateRequiredFolder(report, "Runtime/Events", RuntimeRoot + "/Events");
            ValidateRequiredFolder(report, "Runtime/Profiles", RuntimeRoot + "/Profiles");
            ValidateRequiredFolder(report, "Runtime/Validation", RuntimeRoot + "/Validation");
            ValidateRequiredFolder(report, "Runtime/Services", RuntimeRoot + "/Services");
            ValidateRequiredFolder(report, "Runtime/Testing", RuntimeRoot + "/Testing");
            ValidateRequiredFolder(report, "Editor/Validation", EditorRoot + "/Validation");
            ValidateRequiredFolder(report, "Documentation", ModuleRoot + "/Documentation");

            ValidateRequiredFile(report, "Runtime asmdef", RuntimeRoot + "/CCS.Modules.WorldResources.Runtime.asmdef");
            ValidateRequiredFile(report, "Editor asmdef", EditorRoot + "/CCS.Modules.WorldResources.Editor.asmdef");

            ValidateRequiredScript(report, "CCS_ResourceNodeType", RuntimeRoot + "/Definitions/CCS_ResourceNodeType.cs");
            ValidateRequiredScript(report, "CCS_RequiredToolType", RuntimeRoot + "/Definitions/CCS_RequiredToolType.cs");
            ValidateRequiredScript(report, "CCS_ResourceDefinition", RuntimeRoot + "/Definitions/CCS_ResourceDefinition.cs");
            ValidateRequiredScript(report, "CCS_ResourceDropDefinition", RuntimeRoot + "/Definitions/CCS_ResourceDropDefinition.cs");
            ValidateRequiredScript(report, "CCS_HarvestRequest", RuntimeRoot + "/Data/CCS_HarvestRequest.cs");
            ValidateRequiredScript(report, "CCS_HarvestResult", RuntimeRoot + "/Data/CCS_HarvestResult.cs");
            ValidateRequiredScript(report, "CCS_ResourceNodeState", RuntimeRoot + "/Data/CCS_ResourceNodeState.cs");
            ValidateRequiredScript(report, "CCS_ResourceSnapshot", RuntimeRoot + "/Data/CCS_ResourceSnapshot.cs");
            ValidateRequiredScript(report, "CCS_ResourceHarvestService", RuntimeRoot + "/Harvesting/CCS_ResourceHarvestService.cs");
            ValidateRequiredScript(report, "CCS_HarvestableResource", RuntimeRoot + "/Harvesting/CCS_HarvestableResource.cs");
            ValidateRequiredScript(report, "CCS_ResourceRespawnState", RuntimeRoot + "/Respawn/CCS_ResourceRespawnState.cs");
            ValidateRequiredScript(report, "CCS_ResourceRespawnService", RuntimeRoot + "/Respawn/CCS_ResourceRespawnService.cs");
            ValidateRequiredScript(report, "CCS_ResourceEventArgs", RuntimeRoot + "/Events/CCS_ResourceEventArgs.cs");
            ValidateRequiredScript(report, "CCS_ResourceEvents", RuntimeRoot + "/Events/CCS_ResourceEvents.cs");
            ValidateRequiredScript(report, "CCS_WorldResourceProfile", RuntimeRoot + "/Profiles/CCS_WorldResourceProfile.cs");
            ValidateRequiredScript(report, "CCS_WorldResourceValidationUtility", RuntimeRoot + "/Validation/CCS_WorldResourceValidationUtility.cs");
            ValidateRequiredScript(report, "CCS_WorldResourceRuntimeBridge", RuntimeRoot + "/Services/CCS_WorldResourceRuntimeBridge.cs");
            ValidateRequiredScript(report, "CCS_ResourceHarvestingTestHarness", RuntimeRoot + "/Testing/CCS_ResourceHarvestingTestHarness.cs");

            ValidateDocumentationAsset(report, "World Resources Module Doc", ModuleDocPath);

            report.AddIssue(
                typeof(CCS_IInteractableResultProvider).IsAssignableFrom(typeof(CCS_HarvestableResource))
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Harvestable Interaction Contract",
                "CCS_HarvestableResource implements CCS_IInteractableResultProvider for harvest integration.");

            ValidateTestContentAssets(report);
            ValidateBootstrapGameplayServiceHost(report);

            CCS_SurvivalValidationResult nodeTypeValidation =
                CCS_WorldResourceValidationUtility.ValidateRequiredNodeTypes();

            report.AddIssue(
                nodeTypeValidation.IsSuccess
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Resource Node Types",
                nodeTypeValidation.Message);

            CCS_SurvivalValidationResult toolTypeValidation =
                CCS_WorldResourceValidationUtility.ValidateRequiredToolTypes();

            report.AddIssue(
                toolTypeValidation.IsSuccess
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Required Tool Types",
                toolTypeValidation.Message);

            ValidateResourceDefinitionRules(report);
            ValidateBootstrapTestObjects(report);

            if (File.Exists(DefaultProfilePath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Default World Resource Profile",
                    $"Asset present: {DefaultProfilePath}");

                CCS_WorldResourceProfile profile =
                    AssetDatabase.LoadAssetAtPath<CCS_WorldResourceProfile>(DefaultProfilePath);

                if (profile != null)
                {
                    CCS_SurvivalValidationResult profileValidation =
                        CCS_WorldResourceValidationUtility.ValidateProfile(profile);

                    CCS_SurvivalValidationIssueSeverity severity =
                        profileValidation.IsSuccess
                            ? CCS_SurvivalValidationIssueSeverity.Info
                            : profileValidation.IsWarning
                                ? CCS_SurvivalValidationIssueSeverity.Warning
                                : CCS_SurvivalValidationIssueSeverity.Error;

                    report.AddIssue(severity, "Default World Resource Profile", profileValidation.Message);
                }
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Default World Resource Profile",
                    $"Missing required asset: {DefaultProfilePath}");
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,
                ValidatorId,
                "World resource validator completed (runtime architecture foundation; no UI/final art/save coupling).");
        }

        #endregion

        #region Private Methods

        private static void ValidateResourceDefinitionRules(CCS_SurvivalValidationReport report)
        {
            CCS_SurvivalValidationResult nullDefinitionValidation =
                CCS_WorldResourceValidationUtility.ValidateResourceDefinition(null);

            if (nullDefinitionValidation.IsSuccess)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Resource Definition Validation",
                    "ValidateResourceDefinition(null) should fail.");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Resource Definition Validation",
                    "Resource validation rejects null resource definitions.");
            }
        }

        private static void ValidateBootstrapTestObjects(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(BootstrapScenePath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Bootstrap Test Resources",
                    $"Missing bootstrap scene: {BootstrapScenePath}");
                return;
            }

            string sceneText = File.ReadAllText(BootstrapScenePath);
            for (int i = 0; i < RequiredTestObjectNames.Length; i++)
            {
                string objectName = RequiredTestObjectNames[i];
                if (sceneText.Contains("m_Name: " + objectName))
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Info,
                        "Bootstrap Test Resources",
                        $"Scene contains test object: {objectName}");
                }
                else
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Bootstrap Test Resources",
                        $"Missing test object in bootstrap scene: {objectName}");
                }
            }

            if (sceneText.Contains("CCS_HarvestableResource"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Bootstrap Test Resources",
                    "Bootstrap scene references CCS_HarvestableResource components.");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Warning,
                    "Bootstrap Test Resources",
                    "Bootstrap scene does not reference CCS_HarvestableResource components.");
            }
            if (sceneText.Contains("CCS_ResourceHarvestingTestHarness"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Bootstrap Test Resources",
                    "Bootstrap scene references development harvest test harness.");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Warning,
                    "Bootstrap Test Resources",
                    "Bootstrap scene does not reference CCS_ResourceHarvestingTestHarness.");
            }
        }

        private static void ValidateTestContentAssets(CCS_SurvivalValidationReport report)
        {
            string[] testResourcePaths =
            {
                SurvivalRoot + "/Profiles/WorldResources/TestResources/CCS_TestResource_Tree.asset",
                SurvivalRoot + "/Profiles/WorldResources/TestResources/CCS_TestResource_Rock.asset",
                SurvivalRoot + "/Profiles/WorldResources/TestResources/CCS_TestResource_Plant.asset"
            };

            for (int i = 0; i < testResourcePaths.Length; i++)
            {
                string assetPath = testResourcePaths[i];
                if (!File.Exists(assetPath))
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Test Resource Definitions",
                        $"Missing test resource definition: {assetPath}");
                    continue;
                }

                CCS_ResourceDefinition resourceDefinition =
                    AssetDatabase.LoadAssetAtPath<CCS_ResourceDefinition>(assetPath);

                if (resourceDefinition == null)
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Test Resource Definitions",
                        $"Could not load test resource definition: {assetPath}");
                    continue;
                }

                CCS_SurvivalValidationResult validation =
                    CCS_WorldResourceValidationUtility.ValidateResourceDefinition(resourceDefinition);

                report.AddIssue(
                    validation.IsSuccess
                        ? CCS_SurvivalValidationIssueSeverity.Info
                        : CCS_SurvivalValidationIssueSeverity.Error,
                    "Test Resource Definitions",
                    $"{assetPath}: {validation.Message}");
            }
        }

        private static void ValidateBootstrapGameplayServiceHost(CCS_SurvivalValidationReport report)
        {
            const string bootstrapPrefabPath = SurvivalRoot + "/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
            if (!File.Exists(bootstrapPrefabPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Warning,
                    "Gameplay Service Host",
                    $"Missing bootstrap prefab: {bootstrapPrefabPath}");
                return;
            }

            string prefabText = File.ReadAllText(bootstrapPrefabPath);
            if (prefabText.Contains("CCS_SurvivalGameplayServiceHost"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Gameplay Service Host",
                    "Bootstrap prefab includes CCS_SurvivalGameplayServiceHost.");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Gameplay Service Host",
                    "Bootstrap prefab is missing CCS_SurvivalGameplayServiceHost.");
            }

            if (prefabText.Contains("worldResourceProfile"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Gameplay Service Host",
                    "Bootstrap gameplay service host references worldResourceProfile.");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Warning,
                    "Gameplay Service Host",
                    "Bootstrap gameplay service host is missing worldResourceProfile assignment.");
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
                $"Missing required folder: {folderPath}");
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
                $"Missing required file: {filePath}");
        }

        private static void ValidateRequiredScript(
            CCS_SurvivalValidationReport report,
            string context,
            string scriptPath)
        {
            ValidateRequiredFile(report, context, scriptPath);
        }

        private static void ValidateDocumentationAsset(
            CCS_SurvivalValidationReport report,
            string context,
            string assetPath)
        {
            if (File.Exists(assetPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    context,
                    $"Documentation present: {assetPath}");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Warning,
                context,
                $"Documentation missing: {assetPath}");
        }

        #endregion
    }
}
