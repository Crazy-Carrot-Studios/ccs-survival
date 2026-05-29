using System.IO;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SurvivalValidationUtility
// CATEGORY: Survival / Editor / Development / Validation
// PURPOSE: Reusable editor validation checks for survival folder and config expectations.
// PLACEMENT: Invoked by CCS_SurvivalValidationMenu. Future modules append checks here.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-28
// NOTES: Validates development foundation structure only in 0.3.6.
// =============================================================================

namespace CCS.Survival.Editor.Development
{
    public static class CCS_SurvivalValidationUtility
    {
        private const string SurvivalRoot = "Assets/CCS/Survival";
        private const string ExpectedBundleVersion = "0.3.6";

        #region Public Methods

        public static CCS_SurvivalValidationReport RunDevelopmentValidation()
        {
            CCS_SurvivalValidationReport report = new CCS_SurvivalValidationReport();

            ValidateRequiredFolder(report, "Runtime/Development", $"{SurvivalRoot}/Runtime/Development");
            ValidateRequiredFolder(report, "Runtime/Development/Diagnostics", $"{SurvivalRoot}/Runtime/Development/Diagnostics");
            ValidateRequiredFolder(report, "Runtime/Development/Testing", $"{SurvivalRoot}/Runtime/Development/Testing");
            ValidateRequiredFolder(report, "Runtime/Development/Settings", $"{SurvivalRoot}/Runtime/Development/Settings");
            ValidateRequiredFolder(report, "Runtime/Development/Bootstrap", $"{SurvivalRoot}/Runtime/Development/Bootstrap");
            ValidateRequiredFolder(report, "Editor/Development", $"{SurvivalRoot}/Editor/Development");
            ValidateRequiredFolder(report, "Editor/Development/Validation", $"{SurvivalRoot}/Editor/Development/Validation");
            ValidateRequiredFolder(report, "Documentation", $"{SurvivalRoot}/Documentation");

            ValidateRequiredAsset(report, "Runtime Assembly", $"{SurvivalRoot}/Runtime/CCS.Survival.Runtime.asmdef");
            ValidateRequiredAsset(report, "Editor Assembly", $"{SurvivalRoot}/Editor/CCS.Survival.Editor.asmdef");
            ValidateRequiredAsset(report, "Bootstrap Scene", $"{SurvivalRoot}/Scenes/SCN_CCS_Survival_Bootstrap.unity");
            ValidateRequiredAsset(report, "Bootstrap Prefab", $"{SurvivalRoot}/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab");

            ValidateDocumentationAsset(
                report,
                "Development Framework Support Doc",
                $"{SurvivalRoot}/Documentation/CCS_Survival_Development_Framework_Support.md");

            ValidateDocumentationAsset(
                report,
                "Module Roadmap Doc",
                $"{SurvivalRoot}/Documentation/CCS_Survival_Module_Roadmap.md");

            ValidateProjectVersion(report);

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,
                "Validation Scope",
                "0.3.6 validates development foundation folders and baseline project references only.");

            return report;
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

        private static void ValidateRequiredAsset(
            CCS_SurvivalValidationReport report,
            string context,
            string assetPath)
        {
            if (File.Exists(assetPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    context,
                    $"Asset present: {assetPath}");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Error,
                context,
                $"Missing required asset: {assetPath}");
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

        private static void ValidateProjectVersion(CCS_SurvivalValidationReport report)
        {
            string projectSettingsPath = "ProjectSettings/ProjectSettings.asset";
            if (!File.Exists(projectSettingsPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Project Version",
                    "ProjectSettings/ProjectSettings.asset was not found.");
                return;
            }

            string projectSettingsText = File.ReadAllText(projectSettingsPath);
            if (projectSettingsText.Contains($"bundleVersion: {ExpectedBundleVersion}"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Project Version",
                    $"bundleVersion matches expected milestone {ExpectedBundleVersion}.");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Warning,
                "Project Version",
                $"Expected bundleVersion {ExpectedBundleVersion}. Review ProjectSettings/ProjectSettings.asset.");
        }

        #endregion
    }
}
