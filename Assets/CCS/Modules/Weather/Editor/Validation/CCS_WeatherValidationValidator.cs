using System.IO;
using CCS.Modules.Weather;
using CCS.Survival;
using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_WeatherValidationValidator
// CATEGORY: Modules / Weather / Editor / Validation
// PURPOSE: Validates weather module folders, asmdefs, profile asset, and scripts.
// PLACEMENT: Registered on CCS_SurvivalValidationPipeline at editor load.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Foundation checks only. No VFX, lighting, or audio requirements.
// =============================================================================

namespace CCS.Modules.Weather.Editor
{
    public sealed class CCS_WeatherValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string ModuleRoot = "Assets/CCS/Modules/Weather";
        private const string RuntimeRoot = ModuleRoot + "/Runtime";
        private const string EditorRoot = ModuleRoot + "/Editor";
        private const string SurvivalRoot = "Assets/CCS/Survival";
        private const string DefaultProfilePath =
            SurvivalRoot + "/Profiles/Weather/CCS_DefaultWeatherProfile.asset";
        private const string ModuleDocPath = ModuleRoot + "/Documentation/CCS_Weather_Module.md";
        private const string GameplayServiceRegistrationPath =
            SurvivalRoot + "/Runtime/Composition/CCS_SurvivalGameplayServiceRegistration.cs";
        private const string SaveableIdsPath =
            "Assets/CCS/Modules/SaveLoad/Runtime/Data/CCS_SaveLoadSaveableIds.cs";

        #region Properties

        public string ValidatorId => "ccs.survival.validation.weather";

        #endregion

        #region Public Methods

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateRequiredFolder(report, "Modules/Weather", ModuleRoot);
            ValidateRequiredFolder(report, "Runtime/Definitions", RuntimeRoot + "/Definitions");
            ValidateRequiredFolder(report, "Runtime/Data", RuntimeRoot + "/Data");
            ValidateRequiredFolder(report, "Runtime/Services", RuntimeRoot + "/Services");
            ValidateRequiredFolder(report, "Runtime/Profiles", RuntimeRoot + "/Profiles");
            ValidateRequiredFolder(report, "Runtime/Events", RuntimeRoot + "/Events");
            ValidateRequiredFolder(report, "Runtime/Validation", RuntimeRoot + "/Validation");
            ValidateRequiredFolder(report, "Runtime/Presentation", RuntimeRoot + "/Presentation");
            ValidateRequiredFolder(report, "Editor/Validation", EditorRoot + "/Validation");
            ValidateRequiredFolder(report, "Documentation", ModuleRoot + "/Documentation");

            ValidateRequiredFile(report, "Runtime asmdef", RuntimeRoot + "/CCS.Modules.Weather.Runtime.asmdef");
            ValidateRequiredFile(report, "Editor asmdef", EditorRoot + "/CCS.Modules.Weather.Editor.asmdef");

            ValidateRequiredScript(report, "CCS_WeatherType", RuntimeRoot + "/Definitions/CCS_WeatherType.cs");
            ValidateRequiredScript(report, "CCS_WeatherTransitionMode", RuntimeRoot + "/Definitions/CCS_WeatherTransitionMode.cs");
            ValidateRequiredScript(report, "CCS_WeatherState", RuntimeRoot + "/Data/CCS_WeatherState.cs");
            ValidateRequiredScript(report, "CCS_WeatherSnapshot", RuntimeRoot + "/Data/CCS_WeatherSnapshot.cs");
            ValidateRequiredScript(report, "CCS_WeatherSaveData", RuntimeRoot + "/Data/CCS_WeatherSaveData.cs");
            ValidateRequiredScript(report, "CCS_WeatherProfile", RuntimeRoot + "/Profiles/CCS_WeatherProfile.cs");
            ValidateRequiredScript(report, "CCS_WeatherEvents", RuntimeRoot + "/Events/CCS_WeatherEvents.cs");
            ValidateRequiredScript(report, "CCS_WeatherEventArgs", RuntimeRoot + "/Events/CCS_WeatherEventArgs.cs");
            ValidateRequiredScript(report, "CCS_WeatherValidationUtility", RuntimeRoot + "/Validation/CCS_WeatherValidationUtility.cs");
            ValidateRequiredScript(report, "CCS_WeatherService", RuntimeRoot + "/Services/CCS_WeatherService.cs");
            ValidateRequiredScript(report, "CCS_WeatherRuntimeBridge", RuntimeRoot + "/Services/CCS_WeatherRuntimeBridge.cs");
            ValidateRequiredScript(report, "CCS_WeatherHudPresenter", RuntimeRoot + "/Presentation/CCS_WeatherHudPresenter.cs");

            ValidateDocumentationAsset(report, "Weather Module Doc", ModuleDocPath);
            ValidateWeatherEnumEntries(report);
            ValidateRuntimeScriptsAvoidUnityEditor(report, RuntimeRoot);
            ValidateDefaultProfile(report);
            ValidateSaveIntegration(report);
            ValidateServiceRegistration(report);
            ValidateRestoreOrder(report);
            ValidateBootstrapHudPresenter(report);

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,
                ValidatorId,
                "Weather validator completed (0.7.1 foundation; VFX/lighting/audio deferred).");
        }

        #endregion

        #region Private Methods

        private static void ValidateWeatherEnumEntries(CCS_SurvivalValidationReport report)
        {
            if (CCS_WeatherValidationUtility.ValidateRequiredWeatherTypes(out string missingTypesMessage))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Weather Enum Entries",
                    "Required weather enum entries validated: Clear, Cloudy, Rain, Storm, Fog.");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Error,
                "Weather Enum Entries",
                missingTypesMessage);
        }

        private static void ValidateDefaultProfile(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(DefaultProfilePath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Default Weather Profile",
                    $"Missing required asset: {DefaultProfilePath}");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,
                "Default Weather Profile",
                $"Asset present: {DefaultProfilePath}");

            CCS_WeatherProfile profile = AssetDatabase.LoadAssetAtPath<CCS_WeatherProfile>(DefaultProfilePath);
            if (profile == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Default Weather Profile",
                    "Could not load default weather profile asset.");
                return;
            }

            CCS_SurvivalValidationResult profileValidation = CCS_WeatherValidationUtility.ValidateProfile(profile);
            report.AddIssue(
                profileValidation.IsSuccess
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Default Weather Profile",
                profileValidation.Message);
        }

        private static void ValidateSaveIntegration(CCS_SurvivalValidationReport report)
        {
            const string servicePath = RuntimeRoot + "/Services/CCS_WeatherService.cs";
            if (!File.Exists(servicePath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Weather Save Integration",
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
                    "Weather Save Integration",
                    "CCS_WeatherService implements CCS_ISaveable with versioned save payloads.");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Error,
                "Weather Save Integration",
                "CCS_WeatherService is missing CCS_ISaveable persistence implementation.");
        }

        private static void ValidateServiceRegistration(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(GameplayServiceRegistrationPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Weather Service Registration",
                    $"Missing gameplay service registration script: {GameplayServiceRegistrationPath}");
                return;
            }

            string registrationSource = File.ReadAllText(GameplayServiceRegistrationPath);
            if (registrationSource.Contains("CreateWeatherService")
                && registrationSource.Contains("RegisterSaveable(weatherService)")
                && registrationSource.Contains("BindTimeOfDayService"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Weather Service Registration",
                    "Gameplay composition registers, binds time-of-day, and save-registers CCS_WeatherService.");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Error,
                "Weather Service Registration",
                "Gameplay composition is missing weather service registration wiring.");
        }

        private static void ValidateRestoreOrder(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(SaveableIdsPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Weather Restore Order",
                    $"Missing saveable ids script: {SaveableIdsPath}");
                return;
            }

            string saveableIdsSource = File.ReadAllText(SaveableIdsPath);
            if (saveableIdsSource.Contains("GlobalWeather")
                && saveableIdsSource.Contains("GlobalTimeOfDay")
                && saveableIdsSource.Contains("ModuleRestoreOrder"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Weather Restore Order",
                    "Saveable restore order includes weather after time-of-day.");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Error,
                "Weather Restore Order",
                "Saveable restore order is missing weather after time-of-day.");
        }

        private static void ValidateBootstrapHudPresenter(CCS_SurvivalValidationReport report)
        {
            const string bootstrapScenePath = SurvivalRoot + "/Scenes/SCN_CCS_Survival_Bootstrap.unity";
            if (!File.Exists(bootstrapScenePath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Warning,
                    "Bootstrap Weather HUD",
                    $"Missing bootstrap scene: {bootstrapScenePath}");
                return;
            }

            string sceneText = File.ReadAllText(bootstrapScenePath);
            if (sceneText.Contains("CCS_WeatherHudPresenter"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Bootstrap Weather HUD",
                    "Bootstrap scene includes CCS_WeatherHudPresenter.");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Error,
                "Bootstrap Weather HUD",
                "Bootstrap scene is missing CCS_WeatherHudPresenter.");
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
                    "Runtime weather scripts do not reference UnityEditor.");
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
