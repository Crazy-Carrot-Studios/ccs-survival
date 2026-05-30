using System.IO;
using CCS.Modules.Interaction;
using CCS.Survival;
using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_InteractionValidationValidator
// CATEGORY: Modules / Interaction / Editor / Validation
// PURPOSE: Validates interaction module folders, asmdefs, profile asset, and scripts.
// PLACEMENT: Registered on CCS_SurvivalValidationPipeline at editor load.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Does not require scene objects or gameplay modules.
// =============================================================================

namespace CCS.Modules.Interaction.Editor
{
    public sealed class CCS_InteractionValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string ModuleRoot = "Assets/CCS/Modules/Interaction";
        private const string RuntimeRoot = ModuleRoot + "/Runtime";
        private const string EditorRoot = ModuleRoot + "/Editor";
        private const string SurvivalRoot = "Assets/CCS/Survival";
        private const string DefaultProfilePath =
            SurvivalRoot + "/Profiles/Interaction/CCS_DefaultInteractionProfile.asset";
        private const string ModuleDocPath = ModuleRoot + "/Documentation/CCS_Interaction_Module.md";

        #region Properties

        public string ValidatorId => "ccs.survival.validation.interaction";

        #endregion

        #region Public Methods

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateRequiredFolder(report, "Modules/Interaction", ModuleRoot);
            ValidateRequiredFolder(report, "Runtime/Detection", RuntimeRoot + "/Detection");
            ValidateRequiredFolder(report, "Runtime/Interaction", RuntimeRoot + "/Interaction");
            ValidateRequiredFolder(report, "Runtime/Interfaces", RuntimeRoot + "/Interfaces");
            ValidateRequiredFolder(report, "Runtime/Events", RuntimeRoot + "/Events");
            ValidateRequiredFolder(report, "Runtime/Profiles", RuntimeRoot + "/Profiles");
            ValidateRequiredFolder(report, "Runtime/Validation", RuntimeRoot + "/Validation");
            ValidateRequiredFolder(report, "Editor/Validation", EditorRoot + "/Validation");
            ValidateRequiredFolder(report, "Documentation", ModuleRoot + "/Documentation");

            ValidateRequiredFile(report, "Runtime asmdef", RuntimeRoot + "/CCS.Modules.Interaction.Runtime.asmdef");
            ValidateRequiredFile(report, "Editor asmdef", EditorRoot + "/CCS.Modules.Interaction.Editor.asmdef");

            ValidateRequiredScript(report, "CCS_IInteractable", RuntimeRoot + "/Interfaces/CCS_IInteractable.cs");
            ValidateRequiredScript(report, "CCS_InteractableBase", RuntimeRoot + "/Interaction/CCS_InteractableBase.cs");
            ValidateRequiredScript(report, "CCS_InteractionService", RuntimeRoot + "/Interaction/CCS_InteractionService.cs");
            ValidateRequiredScript(report, "CCS_InteractionScanner", RuntimeRoot + "/Detection/CCS_InteractionScanner.cs");
            ValidateRequiredScript(report, "CCS_InteractionProfile", RuntimeRoot + "/Profiles/CCS_InteractionProfile.cs");

            ValidateDocumentationAsset(report, "Interaction Module Doc", ModuleDocPath);

            if (File.Exists(DefaultProfilePath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Default Interaction Profile",
                    $"Asset present: {DefaultProfilePath}");

                CCS_InteractionProfile profile =
                    AssetDatabase.LoadAssetAtPath<CCS_InteractionProfile>(DefaultProfilePath);

                if (profile != null)
                {
                    CCS_SurvivalValidationResult profileValidation =
                        CCS_InteractionValidationUtility.ValidateProfile(profile);

                    CCS_SurvivalValidationIssueSeverity severity =
                        profileValidation.IsSuccess
                            ? CCS_SurvivalValidationIssueSeverity.Info
                            : profileValidation.IsWarning
                                ? CCS_SurvivalValidationIssueSeverity.Warning
                                : CCS_SurvivalValidationIssueSeverity.Error;

                    report.AddIssue(severity, "Default Interaction Profile", profileValidation.Message);
                }
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Default Interaction Profile",
                    $"Missing required asset: {DefaultProfilePath}");
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,
                ValidatorId,
                "Interaction validator completed (forward raycast foundation; no gameplay module coupling).");
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
