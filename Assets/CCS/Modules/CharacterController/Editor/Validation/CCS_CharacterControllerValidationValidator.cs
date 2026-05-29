using System.IO;
using CCS.Modules.CharacterController;
using CCS.Survival;
using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_CharacterControllerValidationValidator
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Validates module folders, asmdefs, profile asset, and tuning rules.
// PLACEMENT: Registered on CCS_SurvivalValidationPipeline at editor load.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Does not require Rigidbody on character prefabs.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public sealed class CCS_CharacterControllerValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string ModuleRoot = "Assets/CCS/Modules/CharacterController";
        private const string RuntimeRoot = ModuleRoot + "/Runtime";
        private const string EditorRoot = ModuleRoot + "/Editor";
        private const string SurvivalRoot = "Assets/CCS/Survival";
        private const string DefaultProfilePath =
            SurvivalRoot + "/Profiles/CharacterController/CCS_DefaultCharacterControllerProfile.asset";
        private const string ModuleDocPath = ModuleRoot + "/Documentation/CCS_CharacterController_Module.md";

        #region Properties

        public string ValidatorId => "ccs.survival.validation.charactercontroller";

        #endregion

        #region Public Methods

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateRequiredFolder(report, "Modules/CharacterController", ModuleRoot);
            ValidateRequiredFolder(report, "Runtime/Input", RuntimeRoot + "/Input");
            ValidateRequiredFolder(report, "Runtime/Movement", RuntimeRoot + "/Movement");
            ValidateRequiredFolder(report, "Runtime/Camera", RuntimeRoot + "/Camera");
            ValidateRequiredFolder(report, "Runtime/Profiles", RuntimeRoot + "/Profiles");
            ValidateRequiredFolder(report, "Runtime/Events", RuntimeRoot + "/Events");
            ValidateRequiredFolder(report, "Runtime/Validation", RuntimeRoot + "/Validation");
            ValidateRequiredFolder(report, "Editor/Validation", EditorRoot + "/Validation");
            ValidateRequiredFolder(report, "Documentation", ModuleRoot + "/Documentation");

            ValidateRequiredFile(report, "Runtime asmdef", RuntimeRoot + "/CCS.Modules.CharacterController.Runtime.asmdef");
            ValidateRequiredFile(report, "Editor asmdef", EditorRoot + "/CCS.Modules.CharacterController.Editor.asmdef");

            ValidateRequiredScript(report, "CCS_CharacterMovementService", RuntimeRoot + "/Movement/CCS_CharacterMovementService.cs");
            ValidateRequiredScript(report, "CCS_CharacterControllerMotor", RuntimeRoot + "/Movement/CCS_CharacterControllerMotor.cs");
            ValidateRequiredScript(report, "CCS_CharacterControllerProfile", RuntimeRoot + "/Profiles/CCS_CharacterControllerProfile.cs");
            ValidateRequiredScript(report, "CCS_ICharacterInputProvider", RuntimeRoot + "/Input/CCS_ICharacterInputProvider.cs");

            ValidateDocumentationAsset(report, "Character Controller Module Doc", ModuleDocPath);

            if (File.Exists(DefaultProfilePath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Default Character Controller Profile",
                    $"Asset present: {DefaultProfilePath}");

                CCS_CharacterControllerProfile profile =
                    AssetDatabase.LoadAssetAtPath<CCS_CharacterControllerProfile>(DefaultProfilePath);

                if (profile != null)
                {
                    CCS_SurvivalValidationResult profileValidation =
                        CCS_CharacterControllerValidationUtility.ValidateProfile(profile);

                    CCS_SurvivalValidationIssueSeverity severity =
                        profileValidation.IsSuccess
                            ? CCS_SurvivalValidationIssueSeverity.Info
                            : profileValidation.IsWarning
                                ? CCS_SurvivalValidationIssueSeverity.Warning
                                : CCS_SurvivalValidationIssueSeverity.Error;

                    report.AddIssue(severity, "Default Character Controller Profile", profileValidation.Message);
                }
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Default Character Controller Profile",
                    $"Missing required asset: {DefaultProfilePath}");
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,
                ValidatorId,
                "Character controller validator completed (CharacterController-based movement; no Rigidbody required).");
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
