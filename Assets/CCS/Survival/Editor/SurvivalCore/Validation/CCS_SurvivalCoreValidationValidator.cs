using System.IO;

// =============================================================================
// SCRIPT: CCS_SurvivalCoreValidationValidator
// CATEGORY: Survival / Editor / SurvivalCore / Validation
// PURPOSE: Editor validator for survival core folders, scripts, and profile rules.
// PLACEMENT: Registered with CCS_SurvivalValidationPipeline via CCS_SurvivalCoreValidationRegistration.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-28
// NOTES: Does not hard-code menu logic; appends to central validation report.
// =============================================================================

namespace CCS.Survival.Editor.SurvivalCore
{
    public sealed class CCS_SurvivalCoreValidationValidator : Development.CCS_ISurvivalValidationValidator
    {
        private const string SurvivalRoot = "Assets/CCS/Survival";
        private const string SurvivalCoreRoot = "Assets/CCS/Survival/Runtime/SurvivalCore";

        #region Properties

        public string ValidatorId => "ccs.survival.validation.survivalcore";

        #endregion

        #region Public Methods

        public void Validate(Development.CCS_SurvivalValidationReport report)
        {
            ValidateRequiredFolder(report, "Runtime/SurvivalCore", SurvivalCoreRoot);
            ValidateRequiredFolder(report, "Runtime/SurvivalCore/Stats", $"{SurvivalCoreRoot}/Stats");
            ValidateRequiredFolder(report, "Runtime/SurvivalCore/Profiles", $"{SurvivalCoreRoot}/Profiles");
            ValidateRequiredFolder(report, "Runtime/SurvivalCore/Runtime", $"{SurvivalCoreRoot}/Runtime");
            ValidateRequiredFolder(report, "Runtime/SurvivalCore/Events", $"{SurvivalCoreRoot}/Events");
            ValidateRequiredFolder(report, "Runtime/SurvivalCore/Validation", $"{SurvivalCoreRoot}/Validation");
            ValidateRequiredFolder(report, "Editor/SurvivalCore", $"{SurvivalRoot}/Editor/SurvivalCore");
            ValidateRequiredFolder(report, "Editor/SurvivalCore/Validation", $"{SurvivalRoot}/Editor/SurvivalCore/Validation");
            ValidateRequiredFolder(report, "Editor/SurvivalCore/Tools", $"{SurvivalRoot}/Editor/SurvivalCore/Tools");

            ValidateRequiredScript(report, "CCS_SurvivalStatType", $"{SurvivalCoreRoot}/Stats/CCS_SurvivalStatType.cs");
            ValidateRequiredScript(report, "CCS_SurvivalCoreService", $"{SurvivalCoreRoot}/Runtime/CCS_SurvivalCoreService.cs");
            ValidateRequiredScript(report, "CCS_SurvivalCoreProfile", $"{SurvivalCoreRoot}/Profiles/CCS_SurvivalCoreProfile.cs");
            ValidateRequiredScript(report, "CCS_SurvivalCoreValidationUtility", $"{SurvivalCoreRoot}/Validation/CCS_SurvivalCoreValidationUtility.cs");

            ValidateDocumentationAsset(
                report,
                "Survival Core Module Doc",
                $"{SurvivalRoot}/Documentation/CCS_Survival_Core_Module.md");

            string defaultProfilePath =
                $"{SurvivalRoot}/Profiles/SurvivalCore/CCS_DefaultSurvivalCoreProfile.asset";

            if (File.Exists(defaultProfilePath))
            {
                report.AddIssue(
                    Development.CCS_SurvivalValidationIssueSeverity.Info,
                    "Default Survival Core Profile",
                    $"Asset present: {defaultProfilePath}");

                CCS.Survival.SurvivalCore.CCS_SurvivalCoreProfile profile =
                    UnityEditor.AssetDatabase.LoadAssetAtPath<CCS.Survival.SurvivalCore.CCS_SurvivalCoreProfile>(
                        defaultProfilePath);

                if (profile != null)
                {
                    CCS.Survival.CCS_SurvivalValidationResult profileValidation =
                        CCS.Survival.SurvivalCore.CCS_SurvivalCoreValidationUtility.ValidateProfile(profile);

                    Development.CCS_SurvivalValidationIssueSeverity severity =
                        profileValidation.IsSuccess
                            ? Development.CCS_SurvivalValidationIssueSeverity.Info
                            : Development.CCS_SurvivalValidationIssueSeverity.Error;

                    report.AddIssue(severity, "Default Survival Core Profile", profileValidation.Message);
                }
            }
            else
            {
                report.AddIssue(
                    Development.CCS_SurvivalValidationIssueSeverity.Warning,
                    "Default Survival Core Profile",
                    $"Missing recommended asset: {defaultProfilePath}. Use menu to create.");
            }

            report.AddIssue(
                Development.CCS_SurvivalValidationIssueSeverity.Info,
                ValidatorId,
                "Survival core validator completed.");
        }

        #endregion

        #region Private Methods

        private static void ValidateRequiredFolder(
            Development.CCS_SurvivalValidationReport report,
            string context,
            string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                report.AddIssue(
                    Development.CCS_SurvivalValidationIssueSeverity.Info,
                    context,
                    $"Folder present: {folderPath}");
                return;
            }

            report.AddIssue(
                Development.CCS_SurvivalValidationIssueSeverity.Error,
                context,
                $"Missing required folder: {folderPath}");
        }

        private static void ValidateRequiredScript(
            Development.CCS_SurvivalValidationReport report,
            string context,
            string scriptPath)
        {
            if (File.Exists(scriptPath))
            {
                report.AddIssue(
                    Development.CCS_SurvivalValidationIssueSeverity.Info,
                    context,
                    $"Script present: {scriptPath}");
                return;
            }

            report.AddIssue(
                Development.CCS_SurvivalValidationIssueSeverity.Error,
                context,
                $"Missing required script: {scriptPath}");
        }

        private static void ValidateDocumentationAsset(
            Development.CCS_SurvivalValidationReport report,
            string context,
            string assetPath)
        {
            if (File.Exists(assetPath))
            {
                report.AddIssue(
                    Development.CCS_SurvivalValidationIssueSeverity.Info,
                    context,
                    $"Documentation present: {assetPath}");
                return;
            }

            report.AddIssue(
                Development.CCS_SurvivalValidationIssueSeverity.Warning,
                context,
                $"Documentation missing: {assetPath}");
        }

        #endregion
    }
}
