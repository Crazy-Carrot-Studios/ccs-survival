using System.IO;
using CCS.Modules.Building;
using CCS.Survival;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_BuildingValidationValidator
// CATEGORY: Modules / Building / Editor / Validation
// PURPOSE: Validates building module folders, asmdefs, profile, definitions, and wiring.
// PLACEMENT: Registered on CCS_SurvivalValidationPipeline at editor load.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Architecture only. No placement, snapping, or build mode requirements.
// =============================================================================

namespace CCS.Modules.Building.Editor
{
    public sealed class CCS_BuildingValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string ModuleRoot = "Assets/CCS/Modules/Building";
        private const string RuntimeRoot = ModuleRoot + "/Runtime";
        private const string EditorRoot = ModuleRoot + "/Editor";
        private const string SurvivalRoot = "Assets/CCS/Survival";
        private const string DefaultProfilePath =
            SurvivalRoot + "/Profiles/Building/CCS_DefaultBuildingProfile.asset";
        private const string TestFoundationPath =
            SurvivalRoot + "/Content/Building/Definitions/CCS_TestFoundation.asset";
        private const string TestWallPath =
            SurvivalRoot + "/Content/Building/Definitions/CCS_TestWall.asset";
        private const string TestRoofPath =
            SurvivalRoot + "/Content/Building/Definitions/CCS_TestRoof.asset";
        private const string ModuleDocPath = ModuleRoot + "/Documentation/CCS_Building_Module.md";
        private const string GameplayServiceRegistrationPath =
            SurvivalRoot + "/Runtime/Composition/CCS_SurvivalGameplayServiceRegistration.cs";
        private const string GameplayServiceHostPath =
            SurvivalRoot + "/Runtime/Composition/CCS_SurvivalGameplayServiceHost.cs";
        private const string SaveableIdsPath =
            "Assets/CCS/Modules/SaveLoad/Runtime/Data/CCS_SaveLoadSaveableIds.cs";
        private const string EnvironmentHudPresenterPath =
            "Assets/CCS/Modules/EnvironmentEffects/Runtime/Presentation/CCS_EnvironmentEffectsHudPresenter.cs";
        private const string BuildingValidationUtilityPath =
            RuntimeRoot + "/Validation/CCS_BuildingValidationUtility.cs";

        #region Properties

        public string ValidatorId => "ccs.survival.validation.building";

        #endregion

        #region Public Methods

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateRequiredFolder(report, "Modules/Building", ModuleRoot);
            ValidateRequiredFolder(report, "Runtime/Definitions", RuntimeRoot + "/Definitions");
            ValidateRequiredFolder(report, "Runtime/Data", RuntimeRoot + "/Data");
            ValidateRequiredFolder(report, "Runtime/Services", RuntimeRoot + "/Services");
            ValidateRequiredFolder(report, "Runtime/Profiles", RuntimeRoot + "/Profiles");
            ValidateRequiredFolder(report, "Runtime/Events", RuntimeRoot + "/Events");
            ValidateRequiredFolder(report, "Runtime/Validation", RuntimeRoot + "/Validation");
            ValidateRequiredFolder(report, "Editor/Validation", EditorRoot + "/Validation");
            ValidateRequiredFolder(report, "Documentation", ModuleRoot + "/Documentation");

            ValidateRequiredFile(report, "Runtime asmdef", RuntimeRoot + "/CCS.Modules.Building.Runtime.asmdef");
            ValidateRequiredFile(report, "Editor asmdef", EditorRoot + "/CCS.Modules.Building.Editor.asmdef");

            ValidateRequiredScript(report, "CCS_BuildingPieceType", RuntimeRoot + "/Definitions/CCS_BuildingPieceType.cs");
            ValidateRequiredScript(report, "CCS_BuildingPieceDefinition", RuntimeRoot + "/Definitions/CCS_BuildingPieceDefinition.cs");
            ValidateRequiredScript(report, "CCS_BuildingPieceSnapshot", RuntimeRoot + "/Data/CCS_BuildingPieceSnapshot.cs");
            ValidateRequiredScript(report, "CCS_BuildingState", RuntimeRoot + "/Data/CCS_BuildingState.cs");
            ValidateRequiredScript(report, "CCS_BuildingSaveData", RuntimeRoot + "/Data/CCS_BuildingSaveData.cs");
            ValidateRequiredScript(report, "CCS_BuildingProfile", RuntimeRoot + "/Profiles/CCS_BuildingProfile.cs");
            ValidateRequiredScript(report, "CCS_BuildingService", RuntimeRoot + "/Services/CCS_BuildingService.cs");
            ValidateRequiredScript(report, "CCS_BuildingRuntimeBridge", RuntimeRoot + "/Services/CCS_BuildingRuntimeBridge.cs");
            ValidateRequiredScript(report, "CCS_BuildingEvents", RuntimeRoot + "/Events/CCS_BuildingEvents.cs");
            ValidateRequiredScript(report, "CCS_BuildingEventArgs", RuntimeRoot + "/Events/CCS_BuildingEventArgs.cs");
            ValidateRequiredScript(report, "CCS_BuildingValidationUtility", BuildingValidationUtilityPath);

            ValidateDocumentationAsset(report, "Building Module Doc", ModuleDocPath);
            ValidateRuntimeScriptsAvoidUnityEditor(report, RuntimeRoot);
            ValidateDefaultProfile(report);
            ValidateTestDefinitions(report);
            ValidateSaveIntegration(report);
            ValidateServiceRegistration(report);
            ValidateRestoreOrder(report);
            ValidateHudDisplay(report);

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,
                ValidatorId,
                "Building validator completed (0.8.0 foundation).");
        }

        #endregion

        #region Private Methods

        private static void ValidateDefaultProfile(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(DefaultProfilePath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Default Building Profile",
                    $"Missing required asset: {DefaultProfilePath}");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,
                "Default Building Profile",
                $"Asset present: {DefaultProfilePath}");

            CCS_BuildingProfile profile = AssetDatabase.LoadAssetAtPath<CCS_BuildingProfile>(DefaultProfilePath);
            if (profile == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Default Building Profile",
                    "Could not load default building profile asset.");
                return;
            }

            CCS_SurvivalValidationResult profileValidation = CCS_BuildingValidationUtility.ValidateProfile(profile);
            report.AddIssue(
                profileValidation.IsSuccess
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Default Building Profile",
                profileValidation.Message);
        }

        private static void ValidateTestDefinitions(CCS_SurvivalValidationReport report)
        {
            ValidateTestDefinitionAsset(report, "CCS_TestFoundation", TestFoundationPath);
            ValidateTestDefinitionAsset(report, "CCS_TestWall", TestWallPath);
            ValidateTestDefinitionAsset(report, "CCS_TestRoof", TestRoofPath);
        }

        private static void ValidateTestDefinitionAsset(
            CCS_SurvivalValidationReport report,
            string context,
            string assetPath)
        {
            if (!File.Exists(assetPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    context,
                    $"Missing required definition asset: {assetPath}");
                return;
            }

            CCS_BuildingPieceDefinition definition =
                AssetDatabase.LoadAssetAtPath<CCS_BuildingPieceDefinition>(assetPath);

            if (definition == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    context,
                    $"Could not load definition asset: {assetPath}");
                return;
            }

            CCS_SurvivalValidationResult validation = CCS_BuildingValidationUtility.ValidateDefinition(definition);
            report.AddIssue(
                validation.IsSuccess
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                context,
                validation.Message);
        }

        private static void ValidateSaveIntegration(CCS_SurvivalValidationReport report)
        {
            const string servicePath = RuntimeRoot + "/Services/CCS_BuildingService.cs";
            if (!File.Exists(servicePath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Building Save Integration",
                    $"Missing service script: {servicePath}");
                return;
            }

            string serviceSource = File.ReadAllText(servicePath);
            if (serviceSource.Contains("CCS_ISaveable")
                && serviceSource.Contains("CaptureState")
                && serviceSource.Contains("RestoreState")
                && serviceSource.Contains("saveDataVersion"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Building Save Integration",
                    "CCS_BuildingService implements CCS_ISaveable with versioned save payloads.");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Building Save Integration",
                    "CCS_BuildingService is missing CCS_ISaveable persistence implementation.");
            }
        }

        private static void ValidateServiceRegistration(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(GameplayServiceRegistrationPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Building Service Registration",
                    $"Missing gameplay service registration script: {GameplayServiceRegistrationPath}");
                return;
            }

            string registrationSource = File.ReadAllText(GameplayServiceRegistrationPath);
            if (registrationSource.Contains("CreateBuildingService")
                && registrationSource.Contains("RegisterSaveable(buildingService)"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Building Service Registration",
                    "Gameplay composition registers and save-registers CCS_BuildingService.");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Building Service Registration",
                    "Gameplay composition is missing building service registration wiring.");
            }

            if (File.Exists(GameplayServiceHostPath))
            {
                string hostSource = File.ReadAllText(GameplayServiceHostPath);
                if (hostSource.Contains("buildingProfile"))
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Info,
                        "Building Service Host Profile",
                        "CCS_SurvivalGameplayServiceHost references buildingProfile.");
                }
                else
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Building Service Host Profile",
                        "CCS_SurvivalGameplayServiceHost is missing buildingProfile reference.");
                }
            }
        }

        private static void ValidateRestoreOrder(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(SaveableIdsPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Building Restore Order",
                    $"Missing saveable ids script: {SaveableIdsPath}");
                return;
            }

            string saveableIdsSource = File.ReadAllText(SaveableIdsPath);
            if (saveableIdsSource.Contains("GlobalEnvironment")
                && saveableIdsSource.Contains("GlobalBuilding")
                && saveableIdsSource.Contains("ModuleRestoreOrder"))
            {
                int environmentIndex = saveableIdsSource.IndexOf("GlobalEnvironment", System.StringComparison.Ordinal);
                int buildingIndex = saveableIdsSource.IndexOf("GlobalBuilding", System.StringComparison.Ordinal);
                if (environmentIndex >= 0 && buildingIndex > environmentIndex)
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Info,
                        "Building Restore Order",
                        "Saveable restore order includes building after environment.");
                    return;
                }
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Error,
                "Building Restore Order",
                "Saveable restore order is missing building after environment.");
        }

        private static void ValidateHudDisplay(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(EnvironmentHudPresenterPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Building HUD Display",
                    $"Missing environment HUD presenter: {EnvironmentHudPresenterPath}");
                return;
            }

            string presenterSource = File.ReadAllText(EnvironmentHudPresenterPath);
            if (presenterSource.Contains("FormatBuildingDefinitionCountLine")
                && presenterSource.Contains("CCS_BuildingRuntimeBridge"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Building HUD Display",
                    "Environment HUD presenter displays registered building definition count.");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Error,
                "Building HUD Display",
                "Environment HUD presenter is missing building definition count display.");
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

        private static void ValidateRuntimeScriptsAvoidUnityEditor(
            CCS_SurvivalValidationReport report,
            string runtimeRoot)
        {
            bool foundEditorReference = false;
            string[] scriptGuids = AssetDatabase.FindAssets("t:Script", new[] { runtimeRoot });
            for (int index = 0; index < scriptGuids.Length; index++)
            {
                string path = AssetDatabase.GUIDToAssetPath(scriptGuids[index]);
                if (ScriptContainsUnityEditorReference(File.ReadAllText(path)))
                {
                    foundEditorReference = true;
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Runtime Script Editor Reference",
                        $"Runtime script references UnityEditor: {path}");
                }
            }

            if (!foundEditorReference)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Runtime Script Editor Reference",
                    "Runtime building scripts do not reference UnityEditor.");
            }
        }

        private static bool ScriptContainsUnityEditorReference(string contents)
        {
            string[] lines = contents.Split('\n');
            for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
            {
                string trimmedLine = lines[lineIndex].Trim();
                if (trimmedLine.StartsWith("//"))
                {
                    continue;
                }

                int commentIndex = trimmedLine.IndexOf("//", System.StringComparison.Ordinal);
                if (commentIndex >= 0)
                {
                    trimmedLine = trimmedLine.Substring(0, commentIndex).Trim();
                }

                if (trimmedLine.StartsWith("using UnityEditor", System.StringComparison.Ordinal) ||
                    trimmedLine.Contains("UnityEditor.", System.StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}
