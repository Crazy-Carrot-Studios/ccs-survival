using System.IO;

// =============================================================================
// SCRIPT: CCS_SurvivalFoundationValidationValidator
// CATEGORY: Survival / Editor / Development / Validation
// PURPOSE: Foundation milestone validator for folder structure and project version checks.
// PLACEMENT: Registered by CCS_SurvivalValidationPipeline at first run.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-28
// NOTES: Future modules add separate validators; do not extend this class indefinitely.
// =============================================================================

namespace CCS.Survival.Editor.Development
{
    public sealed class CCS_SurvivalFoundationValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string SurvivalRoot = "Assets/CCS/Survival";
        private const string ExpectedBundleVersion = "0.4.0";

        #region Properties

        public string ValidatorId => "ccs.survival.validation.foundation";

        #endregion

        #region Public Methods

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateRequiredFolder(report, "Runtime/Development", $"{SurvivalRoot}/Runtime/Development");
            ValidateRequiredFolder(report, "Runtime/Development/Diagnostics", $"{SurvivalRoot}/Runtime/Development/Diagnostics");
            ValidateRequiredFolder(report, "Runtime/Development/Testing", $"{SurvivalRoot}/Runtime/Development/Testing");
            ValidateRequiredFolder(report, "Runtime/Development/Testing/Traversal", $"{SurvivalRoot}/Runtime/Development/Testing/Traversal");
            ValidateRequiredFolder(report, "Runtime/Development/Testing/Simulation", $"{SurvivalRoot}/Runtime/Development/Testing/Simulation");
            ValidateRequiredFolder(report, "Runtime/Development/Testing/Inventory", $"{SurvivalRoot}/Runtime/Development/Testing/Inventory");
            ValidateRequiredFolder(report, "Runtime/Development/Testing/SaveLoad", $"{SurvivalRoot}/Runtime/Development/Testing/SaveLoad");
            ValidateRequiredFolder(report, "Runtime/Development/Settings", $"{SurvivalRoot}/Runtime/Development/Settings");
            ValidateRequiredFolder(report, "Runtime/Development/Bootstrap", $"{SurvivalRoot}/Runtime/Development/Bootstrap");
            ValidateRequiredFolder(report, "Editor/Development", $"{SurvivalRoot}/Editor/Development");
            ValidateRequiredFolder(report, "Editor/Development/Validation", $"{SurvivalRoot}/Editor/Development/Validation");
            ValidateRequiredFolder(report, "Documentation", $"{SurvivalRoot}/Documentation");

            ValidateRequiredAsset(report, "Runtime Assembly", $"{SurvivalRoot}/Runtime/CCS.Survival.Runtime.asmdef");
            ValidateRequiredAsset(report, "Editor Assembly", $"{SurvivalRoot}/Editor/CCS.Survival.Editor.asmdef");
            ValidateRequiredAsset(report, "Bootstrap Scene", $"{SurvivalRoot}/Scenes/SCN_CCS_Survival_Bootstrap.unity");
            ValidateRequiredAsset(report, "Bootstrap Prefab", $"{SurvivalRoot}/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab");
            ValidateRequiredAsset(report, "Validation Pipeline", $"{SurvivalRoot}/Editor/Development/Validation/CCS_SurvivalValidationPipeline.cs");

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
                ValidatorId,
                "Foundation validator completed.");
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
