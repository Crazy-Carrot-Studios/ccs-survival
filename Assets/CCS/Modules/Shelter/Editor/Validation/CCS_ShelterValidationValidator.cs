using System.IO;
using CCS.Modules.Shelter;
using CCS.Survival;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_ShelterValidationValidator
// CATEGORY: Modules / Shelter / Editor / Validation
// PURPOSE: Validates shelter module folders, asmdefs, profile asset, and scripts.
// PLACEMENT: Registered on CCS_SurvivalValidationPipeline at editor load.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Foundation checks only. No building placement or final art requirements.
// =============================================================================

namespace CCS.Modules.Shelter.Editor
{
    public sealed class CCS_ShelterValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string ModuleRoot = "Assets/CCS/Modules/Shelter";
        private const string RuntimeRoot = ModuleRoot + "/Runtime";
        private const string EditorRoot = ModuleRoot + "/Editor";
        private const string SurvivalRoot = "Assets/CCS/Survival";
        private const string DefaultProfilePath =
            SurvivalRoot + "/Profiles/Shelter/CCS_DefaultShelterProfile.asset";
        private const string ModuleDocPath = ModuleRoot + "/Documentation/CCS_Shelter_Module.md";
        private const string GameplayServiceRegistrationPath =
            SurvivalRoot + "/Runtime/Composition/CCS_SurvivalGameplayServiceRegistration.cs";
        private const string GameplayServiceHostPath =
            SurvivalRoot + "/Runtime/Composition/CCS_SurvivalGameplayServiceHost.cs";
        private const string SaveableIdsPath =
            "Assets/CCS/Modules/SaveLoad/Runtime/Data/CCS_SaveLoadSaveableIds.cs";
        private const string EnvironmentServicePath =
            "Assets/CCS/Modules/EnvironmentEffects/Runtime/Services/CCS_EnvironmentEffectsService.cs";
        private const string EnvironmentDisplayPath =
            "Assets/CCS/Modules/EnvironmentEffects/Runtime/Validation/CCS_EnvironmentEffectsValidationUtility.cs";
        private const string BootstrapScenePath = SurvivalRoot + "/Scenes/SCN_CCS_Survival_Bootstrap.unity";

        #region Properties

        public string ValidatorId => "ccs.survival.validation.shelter";

        #endregion

        #region Public Methods

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateRequiredFolder(report, "Modules/Shelter", ModuleRoot);
            ValidateRequiredFolder(report, "Runtime/Data", RuntimeRoot + "/Data");
            ValidateRequiredFolder(report, "Runtime/Volumes", RuntimeRoot + "/Volumes");
            ValidateRequiredFolder(report, "Runtime/Services", RuntimeRoot + "/Services");
            ValidateRequiredFolder(report, "Runtime/Profiles", RuntimeRoot + "/Profiles");
            ValidateRequiredFolder(report, "Runtime/Events", RuntimeRoot + "/Events");
            ValidateRequiredFolder(report, "Runtime/Validation", RuntimeRoot + "/Validation");
            ValidateRequiredFolder(report, "Runtime/Testing", RuntimeRoot + "/Testing");
            ValidateRequiredFolder(report, "Editor/Validation", EditorRoot + "/Validation");
            ValidateRequiredFolder(report, "Documentation", ModuleRoot + "/Documentation");

            ValidateRequiredFile(report, "Runtime asmdef", RuntimeRoot + "/CCS.Modules.Shelter.Runtime.asmdef");
            ValidateRequiredFile(report, "Editor asmdef", EditorRoot + "/CCS.Modules.Shelter.Editor.asmdef");

            ValidateRequiredScript(report, "CCS_ShelterSnapshot", RuntimeRoot + "/Data/CCS_ShelterSnapshot.cs");
            ValidateRequiredScript(report, "CCS_ShelterState", RuntimeRoot + "/Data/CCS_ShelterState.cs");
            ValidateRequiredScript(report, "CCS_ShelterModifierSnapshot", RuntimeRoot + "/Data/CCS_ShelterModifierSnapshot.cs");
            ValidateRequiredScript(report, "CCS_ShelterSaveData", RuntimeRoot + "/Data/CCS_ShelterSaveData.cs");
            ValidateRequiredScript(report, "CCS_ShelterProfile", RuntimeRoot + "/Profiles/CCS_ShelterProfile.cs");
            ValidateRequiredScript(report, "CCS_ShelterVolume", RuntimeRoot + "/Volumes/CCS_ShelterVolume.cs");
            ValidateRequiredScript(report, "CCS_ShelterService", RuntimeRoot + "/Services/CCS_ShelterService.cs");
            ValidateRequiredScript(report, "CCS_ShelterRuntimeBridge", RuntimeRoot + "/Services/CCS_ShelterRuntimeBridge.cs");
            ValidateRequiredScript(report, "CCS_ShelterEvents", RuntimeRoot + "/Events/CCS_ShelterEvents.cs");
            ValidateRequiredScript(report, "CCS_ShelterEventArgs", RuntimeRoot + "/Events/CCS_ShelterEventArgs.cs");
            ValidateRequiredScript(report, "CCS_ShelterValidationUtility", RuntimeRoot + "/Validation/CCS_ShelterValidationUtility.cs");
            ValidateRequiredScript(report, "CCS_ShelterTestHarness", RuntimeRoot + "/Testing/CCS_ShelterTestHarness.cs");
            ValidateRequiredScript(
                report,
                "CCS_BuildingShelterIntegrationTestHarness",
                RuntimeRoot + "/Testing/CCS_BuildingShelterIntegrationTestHarness.cs");

            ValidateDocumentationAsset(report, "Shelter Module Doc", ModuleDocPath);
            ValidateRuntimeScriptsAvoidUnityEditor(report, RuntimeRoot);
            ValidateDefaultProfile(report);
            ValidateSaveIntegration(report);
            ValidateServiceRegistration(report);
            ValidateRestoreOrder(report);
            ValidateEnvironmentIntegration(report);
            ValidateBuildingIntegration(report);
            ValidateHudDisplay(report);
            ValidateBootstrapTestVolume(report);

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,
                ValidatorId,
                "Shelter validator completed (0.8.5 building shelter integration).");
        }

        #endregion

        #region Private Methods

        private static void ValidateDefaultProfile(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(DefaultProfilePath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Default Shelter Profile",
                    $"Missing required asset: {DefaultProfilePath}");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,
                "Default Shelter Profile",
                $"Asset present: {DefaultProfilePath}");

            CCS_ShelterProfile profile = AssetDatabase.LoadAssetAtPath<CCS_ShelterProfile>(DefaultProfilePath);
            if (profile == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Default Shelter Profile",
                    "Could not load default shelter profile asset.");
                return;
            }

            CCS_SurvivalValidationResult profileValidation = CCS_ShelterValidationUtility.ValidateProfile(profile);
            report.AddIssue(
                profileValidation.IsSuccess
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Default Shelter Profile",
                profileValidation.Message);
        }

        private static void ValidateSaveIntegration(CCS_SurvivalValidationReport report)
        {
            const string servicePath = RuntimeRoot + "/Services/CCS_ShelterService.cs";
            if (!File.Exists(servicePath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Shelter Save Integration",
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
                    "Shelter Save Integration",
                    "CCS_ShelterService implements CCS_ISaveable with versioned save payloads.");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Shelter Save Integration",
                    "CCS_ShelterService is missing CCS_ISaveable persistence implementation.");
            }
        }

        private static void ValidateServiceRegistration(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(GameplayServiceRegistrationPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Shelter Service Registration",
                    $"Missing gameplay service registration script: {GameplayServiceRegistrationPath}");
                return;
            }

            string registrationSource = File.ReadAllText(GameplayServiceRegistrationPath);
            if (registrationSource.Contains("CreateShelterService")
                && registrationSource.Contains("RegisterSaveable(shelterService)")
                && registrationSource.Contains("BindShelterService"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Shelter Service Registration",
                    "Gameplay composition registers, save-registers, and binds CCS_ShelterService.");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Shelter Service Registration",
                    "Gameplay composition is missing shelter service registration wiring.");
            }

            if (File.Exists(GameplayServiceHostPath))
            {
                string hostSource = File.ReadAllText(GameplayServiceHostPath);
                if (hostSource.Contains("shelterProfile"))
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Info,
                        "Shelter Service Host Profile",
                        "CCS_SurvivalGameplayServiceHost references shelterProfile.");
                }
                else
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Shelter Service Host Profile",
                        "CCS_SurvivalGameplayServiceHost is missing shelterProfile reference.");
                }
            }
        }

        private static void ValidateRestoreOrder(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(SaveableIdsPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Shelter Restore Order",
                    $"Missing saveable ids script: {SaveableIdsPath}");
                return;
            }

            string saveableIdsSource = File.ReadAllText(SaveableIdsPath);
            if (saveableIdsSource.Contains("GlobalShelter")
                && saveableIdsSource.Contains("GlobalEnvironment")
                && saveableIdsSource.Contains("ModuleRestoreOrder"))
            {
                int shelterIndex = saveableIdsSource.IndexOf("GlobalShelter", System.StringComparison.Ordinal);
                int environmentIndex = saveableIdsSource.IndexOf("GlobalEnvironment", System.StringComparison.Ordinal);
                if (shelterIndex >= 0 && environmentIndex > shelterIndex)
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Info,
                        "Shelter Restore Order",
                        "Saveable restore order includes shelter before environment.");
                    return;
                }
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Error,
                "Shelter Restore Order",
                "Saveable restore order is missing shelter before environment.");
        }

        private static void ValidateEnvironmentIntegration(CCS_SurvivalValidationReport report)
        {
            if (File.Exists(EnvironmentServicePath))
            {
                string serviceSource = File.ReadAllText(EnvironmentServicePath);
                if (serviceSource.Contains("BindShelterService")
                    && serviceSource.Contains("CCS_ShelterService"))
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Info,
                        "Environment Shelter Binding",
                        "CCS_EnvironmentEffectsService binds shelter service safely.");
                }
                else
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Environment Shelter Binding",
                        "CCS_EnvironmentEffectsService is missing shelter integration.");
                }
            }
        }

        private static void ValidateBuildingIntegration(CCS_SurvivalValidationReport report)
        {
            const string servicePath = RuntimeRoot + "/Services/CCS_ShelterService.cs";
            const string bridgePath =
                "Assets/CCS/Modules/Building/Runtime/Services/CCS_BuildingShelterRuntimeBridge.cs";
            const string presenterPath =
                "Assets/CCS/Modules/EnvironmentEffects/Runtime/Presentation/CCS_EnvironmentEffectsHudPresenter.cs";

            if (File.Exists(servicePath))
            {
                string serviceSource = File.ReadAllText(servicePath);
                if (serviceSource.Contains("BindBuildingService")
                    && serviceSource.Contains("SetBuildingContributions")
                    && serviceSource.Contains("RecalculateBuildingShelter")
                    && serviceSource.Contains("CCS_BuildingShelterContribution"))
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Info,
                        "Shelter Building Contributions",
                        "CCS_ShelterService accepts building shelter contributions.");
                }
                else
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Shelter Building Contributions",
                        "CCS_ShelterService is missing building contribution integration.");
                }
            }

            if (File.Exists(bridgePath))
            {
                string bridgeSource = File.ReadAllText(bridgePath);
                if (bridgeSource.Contains("TryGetBuildingService")
                    && bridgeSource.Contains("TryGetShelterContributions"))
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Info,
                        "Building Shelter Runtime Bridge",
                        "Building shelter bridge resolves building service safely.");
                }
                else
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Building Shelter Runtime Bridge",
                        "CCS_BuildingShelterRuntimeBridge is missing required methods.");
                }
            }

            if (File.Exists(GameplayServiceRegistrationPath))
            {
                string registrationSource = File.ReadAllText(GameplayServiceRegistrationPath);
                if (registrationSource.Contains("BindBuildingShelterIntegration"))
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Info,
                        "Shelter Building Composition Wiring",
                        "Gameplay composition binds building service to shelter service.");
                }
                else
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Shelter Building Composition Wiring",
                        "Gameplay composition is missing building shelter integration binding.");
                }
            }

            if (File.Exists(presenterPath))
            {
                string presenterSource = File.ReadAllText(presenterPath);
                string utilityPath =
                    "Assets/CCS/Modules/Building/Runtime/Validation/CCS_BuildingValidationUtility.cs";
                string utilitySource = File.Exists(utilityPath) ? File.ReadAllText(utilityPath) : string.Empty;

                if (presenterSource.Contains("FormatBuildingShelterHudLines")
                    && utilitySource.Contains("Building Shelter Contributions:")
                    && utilitySource.Contains("Building Shelter Active:"))
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Info,
                        "Shelter Building HUD Lines",
                        "Environment HUD presenter displays building shelter contribution lines.");
                }
                else
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Shelter Building HUD Lines",
                        "Environment HUD presenter is missing building shelter HUD lines.");
                }
            }
        }

        private static void ValidateHudDisplay(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(EnvironmentDisplayPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Shelter HUD Display",
                    $"Missing environment display utility: {EnvironmentDisplayPath}");
                return;
            }

            string displaySource = File.ReadAllText(EnvironmentDisplayPath);
            if (displaySource.Contains("Sheltered:")
                && displaySource.Contains("Shelter Wet:")
                && displaySource.Contains("Shelter Exp:")
                && displaySource.Contains("Shelter Temp:"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Shelter HUD Display",
                    "Environment display formatting includes shelter protection values.");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Shelter HUD Display",
                    "Environment display formatting is missing shelter values.");
            }
        }

        private static void ValidateBootstrapTestVolume(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(BootstrapScenePath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Warning,
                    "Bootstrap Test Shelter Volume",
                    $"Missing bootstrap scene: {BootstrapScenePath}");
                return;
            }

            string sceneText = File.ReadAllText(BootstrapScenePath);
            if (sceneText.Contains("CCS_TestShelterVolume")
                && sceneText.Contains("CCS_ShelterVolume"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Bootstrap Test Shelter Volume",
                    "Bootstrap scene includes CCS_TestShelterVolume with CCS_ShelterVolume.");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Error,
                "Bootstrap Test Shelter Volume",
                "Bootstrap scene is missing CCS_TestShelterVolume test setup.");
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
                    "Runtime shelter scripts do not reference UnityEditor.");
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
