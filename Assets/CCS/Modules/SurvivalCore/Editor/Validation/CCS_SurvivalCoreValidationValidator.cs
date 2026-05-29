using System.IO;
using CCS.Modules.SurvivalCore;
using CCS.Survival;
using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_SurvivalCoreValidationValidator
// CATEGORY: Modules / SurvivalCore / Editor / Validation
// PURPOSE: Editor validator for survival core folders, scripts, and profile rules.
// PLACEMENT: Registered with CCS_SurvivalValidationPipeline via CCS_SurvivalCoreValidationRegistration.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-28
// NOTES: Does not hard-code menu logic; appends to central validation report.
// =============================================================================

namespace CCS.Modules.SurvivalCore.Editor
{
    public sealed class CCS_SurvivalCoreValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string SurvivalRoot = "Assets/CCS/Survival";
        private const string SurvivalCoreRoot = "Assets/CCS/Modules/SurvivalCore";
        private const string SurvivalCoreRuntimeRoot = SurvivalCoreRoot + "/Runtime";
        private const string SurvivalCoreEditorRoot = SurvivalCoreRoot + "/Editor";
        private const string SurvivalCoreDocPath =
            SurvivalCoreRoot + "/Documentation/CCS_Survival_Core_Module.md";

        #region Properties

        public string ValidatorId => "ccs.survival.validation.survivalcore";

        #endregion

        #region Public Methods

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateRequiredFolder(report, "Modules/SurvivalCore/Runtime", SurvivalCoreRuntimeRoot);
            ValidateRequiredFolder(report, "Modules/SurvivalCore/Runtime/Stats", $"{SurvivalCoreRuntimeRoot}/Stats");
            ValidateRequiredFolder(report, "Modules/SurvivalCore/Runtime/Profiles", $"{SurvivalCoreRuntimeRoot}/Profiles");
            ValidateRequiredFolder(report, "Modules/SurvivalCore/Runtime/Runtime", $"{SurvivalCoreRuntimeRoot}/Runtime");
            ValidateRequiredFolder(report, "Modules/SurvivalCore/Runtime/Events", $"{SurvivalCoreRuntimeRoot}/Events");
            ValidateRequiredFolder(report, "Modules/SurvivalCore/Runtime/Validation", $"{SurvivalCoreRuntimeRoot}/Validation");
            ValidateRequiredFolder(report, "Modules/SurvivalCore/Editor", SurvivalCoreEditorRoot);
            ValidateRequiredFolder(report, "Modules/SurvivalCore/Editor/Validation", $"{SurvivalCoreEditorRoot}/Validation");
            ValidateRequiredFolder(report, "Modules/SurvivalCore/Editor/Tools", $"{SurvivalCoreEditorRoot}/Tools");
            ValidateRequiredFolder(report, "Modules/SurvivalCore/Documentation", $"{SurvivalCoreRoot}/Documentation");

            ValidateRequiredScript(report, "CCS_SurvivalStatType", $"{SurvivalCoreRuntimeRoot}/Stats/CCS_SurvivalStatType.cs");
            ValidateRequiredScript(report, "CCS_SurvivalCoreService", $"{SurvivalCoreRuntimeRoot}/Runtime/CCS_SurvivalCoreService.cs");
            ValidateRequiredScript(report, "CCS_SurvivalCoreProfile", $"{SurvivalCoreRuntimeRoot}/Profiles/CCS_SurvivalCoreProfile.cs");
            ValidateRequiredScript(report, "CCS_SurvivalCoreValidationUtility", $"{SurvivalCoreRuntimeRoot}/Validation/CCS_SurvivalCoreValidationUtility.cs");

            ValidateDocumentationAsset(report, "Survival Core Module Doc", SurvivalCoreDocPath);

            string defaultProfilePath =
                $"{SurvivalRoot}/Profiles/SurvivalCore/CCS_DefaultSurvivalCoreProfile.asset";

            if (File.Exists(defaultProfilePath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Default Survival Core Profile",
                    $"Asset present: {defaultProfilePath}");

                CCS_SurvivalCoreProfile profile =
                    AssetDatabase.LoadAssetAtPath<CCS_SurvivalCoreProfile>(defaultProfilePath);

                if (profile != null)
                {
                    CCS_SurvivalValidationResult profileValidation =
                        CCS_SurvivalCoreValidationUtility.ValidateProfile(profile);

                    CCS_SurvivalValidationIssueSeverity severity =
                        profileValidation.IsSuccess
                            ? CCS_SurvivalValidationIssueSeverity.Info
                            : CCS_SurvivalValidationIssueSeverity.Error;

                    report.AddIssue(severity, "Default Survival Core Profile", profileValidation.Message);
                }
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Warning,
                    "Default Survival Core Profile",
                    $"Missing recommended asset: {defaultProfilePath}. Use menu to create.");
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,
                ValidatorId,
                "Survival core validator completed.");
        }

        #endregion

        #region Private Methods

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

        private static void ValidateRequiredScript(
            CCS_SurvivalValidationReport report,
            string context,
            string scriptPath)
        {
            if (File.Exists(scriptPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    context,
                    $"Script present: {scriptPath}");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Error,
                context,
                $"Missing required script: {scriptPath}");
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
