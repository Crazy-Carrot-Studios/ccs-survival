using System.IO;
using CCS.Modules.TimeOfDay;
using CCS.Survival;
using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_TimeOfDayValidationValidator
// CATEGORY: Modules / TimeOfDay / Editor / Validation
// PURPOSE: Validates time-of-day module folders, asmdefs, profile asset, and scripts.
// PLACEMENT: Registered on CCS_SurvivalValidationPipeline at editor load.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-31
// NOTES: Foundation checks only. No lighting, weather, or AI schedule requirements.
// =============================================================================

namespace CCS.Modules.TimeOfDay.Editor
{
    public sealed class CCS_TimeOfDayValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string ModuleRoot = "Assets/CCS/Modules/TimeOfDay";
        private const string RuntimeRoot = ModuleRoot + "/Runtime";
        private const string EditorRoot = ModuleRoot + "/Editor";
        private const string SurvivalRoot = "Assets/CCS/Survival";
        private const string DefaultProfilePath =
            SurvivalRoot + "/Profiles/TimeOfDay/CCS_DefaultTimeOfDayProfile.asset";
        private const string ModuleDocPath = ModuleRoot + "/Documentation/CCS_Time_Of_Day_Module.md";
        private const string GameplayServiceRegistrationPath =
            SurvivalRoot + "/Runtime/Composition/CCS_SurvivalGameplayServiceRegistration.cs";

        #region Properties

        public string ValidatorId => "ccs.survival.validation.timeofday";

        #endregion

        #region Public Methods

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateRequiredFolder(report, "Modules/TimeOfDay", ModuleRoot);
            ValidateRequiredFolder(report, "Runtime/Data", RuntimeRoot + "/Data");
            ValidateRequiredFolder(report, "Runtime/Services", RuntimeRoot + "/Services");
            ValidateRequiredFolder(report, "Runtime/Profiles", RuntimeRoot + "/Profiles");
            ValidateRequiredFolder(report, "Runtime/Events", RuntimeRoot + "/Events");
            ValidateRequiredFolder(report, "Runtime/Validation", RuntimeRoot + "/Validation");
            ValidateRequiredFolder(report, "Runtime/Presentation", RuntimeRoot + "/Presentation");
            ValidateRequiredFolder(report, "Editor/Validation", EditorRoot + "/Validation");
            ValidateRequiredFolder(report, "Documentation", ModuleRoot + "/Documentation");

            ValidateRequiredFile(report, "Runtime asmdef", RuntimeRoot + "/CCS.Modules.TimeOfDay.Runtime.asmdef");
            ValidateRequiredFile(report, "Editor asmdef", EditorRoot + "/CCS.Modules.TimeOfDay.Editor.asmdef");

            ValidateRequiredScript(report, "CCS_TimeOfDayPhase", RuntimeRoot + "/Data/CCS_TimeOfDayPhase.cs");
            ValidateRequiredScript(report, "CCS_GameTimeSnapshot", RuntimeRoot + "/Data/CCS_GameTimeSnapshot.cs");
            ValidateRequiredScript(report, "CCS_GameClockState", RuntimeRoot + "/Data/CCS_GameClockState.cs");
            ValidateRequiredScript(report, "CCS_TimeOfDaySaveData", RuntimeRoot + "/Data/CCS_TimeOfDaySaveData.cs");
            ValidateRequiredScript(report, "CCS_TimeOfDayProfile", RuntimeRoot + "/Profiles/CCS_TimeOfDayProfile.cs");
            ValidateRequiredScript(report, "CCS_TimeOfDayEvents", RuntimeRoot + "/Events/CCS_TimeOfDayEvents.cs");
            ValidateRequiredScript(report, "CCS_TimeOfDayEventArgs", RuntimeRoot + "/Events/CCS_TimeOfDayEventArgs.cs");
            ValidateRequiredScript(report, "CCS_TimeOfDayValidationUtility", RuntimeRoot + "/Validation/CCS_TimeOfDayValidationUtility.cs");
            ValidateRequiredScript(report, "CCS_TimeOfDayService", RuntimeRoot + "/Services/CCS_TimeOfDayService.cs");
            ValidateRequiredScript(report, "CCS_TimeOfDayRuntimeBridge", RuntimeRoot + "/Services/CCS_TimeOfDayRuntimeBridge.cs");
            ValidateRequiredScript(report, "CCS_TimeOfDayHudPresenter", RuntimeRoot + "/Presentation/CCS_TimeOfDayHudPresenter.cs");

            ValidateDocumentationAsset(report, "Time Of Day Module Doc", ModuleDocPath);
            ValidateRuntimeScriptsAvoidUnityEditor(report, RuntimeRoot);
            ValidateDefaultProfile(report);
            ValidateSaveIntegration(report);
            ValidateServiceRegistration(report);
            ValidateBootstrapHudPresenter(report);

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,
                ValidatorId,
                "Time of day validator completed (0.7.0 foundation; weather/lighting deferred).");
        }

        #endregion

        #region Private Methods

        private static void ValidateDefaultProfile(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(DefaultProfilePath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Default Time Of Day Profile",
                    $"Missing required asset: {DefaultProfilePath}");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,
                "Default Time Of Day Profile",
                $"Asset present: {DefaultProfilePath}");

            CCS_TimeOfDayProfile profile = AssetDatabase.LoadAssetAtPath<CCS_TimeOfDayProfile>(DefaultProfilePath);
            if (profile == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Default Time Of Day Profile",
                    "Could not load default time-of-day profile asset.");
                return;
            }

            CCS_SurvivalValidationResult profileValidation = CCS_TimeOfDayValidationUtility.ValidateProfile(profile);
            report.AddIssue(
                profileValidation.IsSuccess
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Default Time Of Day Profile",
                profileValidation.Message);
        }

        private static void ValidateSaveIntegration(CCS_SurvivalValidationReport report)
        {
            const string servicePath = RuntimeRoot + "/Services/CCS_TimeOfDayService.cs";
            if (!File.Exists(servicePath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Time Of Day Save Integration",
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
                    "Time Of Day Save Integration",
                    "CCS_TimeOfDayService implements CCS_ISaveable with versioned save payloads.");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Time Of Day Save Integration",
                    "CCS_TimeOfDayService is missing CCS_ISaveable persistence implementation.");
            }
        }

        private static void ValidateServiceRegistration(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(GameplayServiceRegistrationPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Time Of Day Service Registration",
                    $"Missing gameplay service registration script: {GameplayServiceRegistrationPath}");
                return;
            }

            string registrationSource = File.ReadAllText(GameplayServiceRegistrationPath);
            if (registrationSource.Contains("CreateTimeOfDayService")
                && registrationSource.Contains("RegisterSaveable(timeOfDayService)"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Time Of Day Service Registration",
                    "Gameplay composition registers and save-registers CCS_TimeOfDayService.");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Error,
                "Time Of Day Service Registration",
                "Gameplay composition is missing time-of-day service registration wiring.");
        }

        private static void ValidateBootstrapHudPresenter(CCS_SurvivalValidationReport report)
        {
            const string bootstrapScenePath = SurvivalRoot + "/Scenes/SCN_CCS_Survival_Bootstrap.unity";
            if (!File.Exists(bootstrapScenePath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Warning,
                    "Bootstrap Time HUD",
                    $"Missing bootstrap scene: {bootstrapScenePath}");
                return;
            }

            string sceneText = File.ReadAllText(bootstrapScenePath);
            if (sceneText.Contains("CCS_TimeOfDayHudPresenter"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Bootstrap Time HUD",
                    "Bootstrap scene includes CCS_TimeOfDayHudPresenter.");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Error,
                "Bootstrap Time HUD",
                "Bootstrap scene is missing CCS_TimeOfDayHudPresenter.");
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
                    "Runtime time-of-day scripts do not reference UnityEditor.");
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
