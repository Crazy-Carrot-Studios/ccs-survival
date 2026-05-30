using System.IO;
using CCS.Modules.Equipment;
using CCS.Survival;
using CCS.Survival.Editor.Development;
using UnityEditor;

// =============================================================================
// SCRIPT: CCS_EquipmentValidationValidator
// CATEGORY: Modules / Equipment / Editor / Validation
// PURPOSE: Validates equipment module folders, asmdefs, profile asset, and scripts.
// PLACEMENT: Registered on CCS_SurvivalValidationPipeline at editor load.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Does not require scene objects, UI, combat, or visual coupling.
// =============================================================================

namespace CCS.Modules.Equipment.Editor
{
    public sealed class CCS_EquipmentValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string ModuleRoot = "Assets/CCS/Modules/Equipment";
        private const string RuntimeRoot = ModuleRoot + "/Runtime";
        private const string EditorRoot = ModuleRoot + "/Editor";
        private const string SurvivalRoot = "Assets/CCS/Survival";
        private const string DefaultProfilePath =
            SurvivalRoot + "/Profiles/Equipment/CCS_DefaultEquipmentProfile.asset";
        private const string ModuleDocPath = ModuleRoot + "/Documentation/CCS_Equipment_Module.md";

        #region Properties

        public string ValidatorId => "ccs.survival.validation.equipment";

        #endregion

        #region Public Methods

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateRequiredFolder(report, "Modules/Equipment", ModuleRoot);
            ValidateRequiredFolder(report, "Runtime/Definitions", RuntimeRoot + "/Definitions");
            ValidateRequiredFolder(report, "Runtime/Data", RuntimeRoot + "/Data");
            ValidateRequiredFolder(report, "Runtime/Slots", RuntimeRoot + "/Slots");
            ValidateRequiredFolder(report, "Runtime/Services", RuntimeRoot + "/Services");
            ValidateRequiredFolder(report, "Runtime/Events", RuntimeRoot + "/Events");
            ValidateRequiredFolder(report, "Runtime/Profiles", RuntimeRoot + "/Profiles");
            ValidateRequiredFolder(report, "Runtime/Validation", RuntimeRoot + "/Validation");
            ValidateRequiredFolder(report, "Editor/Validation", EditorRoot + "/Validation");
            ValidateRequiredFolder(report, "Documentation", ModuleRoot + "/Documentation");

            ValidateRequiredFile(report, "Runtime asmdef", RuntimeRoot + "/CCS.Modules.Equipment.Runtime.asmdef");
            ValidateRequiredFile(report, "Editor asmdef", EditorRoot + "/CCS.Modules.Equipment.Editor.asmdef");

            ValidateRequiredScript(report, "CCS_EquipmentSlotType", RuntimeRoot + "/Definitions/CCS_EquipmentSlotType.cs");
            ValidateRequiredScript(report, "CCS_EquipmentItemDefinition", RuntimeRoot + "/Definitions/CCS_EquipmentItemDefinition.cs");
            ValidateRequiredScript(report, "CCS_EquippedItem", RuntimeRoot + "/Data/CCS_EquippedItem.cs");
            ValidateRequiredScript(report, "CCS_EquipmentSnapshot", RuntimeRoot + "/Data/CCS_EquipmentSnapshot.cs");
            ValidateRequiredScript(report, "CCS_DurabilityState", RuntimeRoot + "/Data/CCS_DurabilityState.cs");
            ValidateRequiredScript(report, "CCS_EquipmentSlot", RuntimeRoot + "/Slots/CCS_EquipmentSlot.cs");
            ValidateRequiredScript(report, "CCS_PlayerEquipmentService", RuntimeRoot + "/Services/CCS_PlayerEquipmentService.cs");
            ValidateRequiredScript(report, "CCS_EquipmentEventArgs", RuntimeRoot + "/Events/CCS_EquipmentEventArgs.cs");
            ValidateRequiredScript(report, "CCS_EquipmentEvents", RuntimeRoot + "/Events/CCS_EquipmentEvents.cs");
            ValidateRequiredScript(report, "CCS_EquipmentProfile", RuntimeRoot + "/Profiles/CCS_EquipmentProfile.cs");
            ValidateRequiredScript(report, "CCS_EquipmentValidationUtility", RuntimeRoot + "/Validation/CCS_EquipmentValidationUtility.cs");

            ValidateDocumentationAsset(report, "Equipment Module Doc", ModuleDocPath);

            if (File.Exists(DefaultProfilePath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Default Equipment Profile",
                    $"Asset present: {DefaultProfilePath}");

                CCS_EquipmentProfile profile =
                    AssetDatabase.LoadAssetAtPath<CCS_EquipmentProfile>(DefaultProfilePath);

                if (profile != null)
                {
                    CCS_SurvivalValidationResult profileValidation =
                        CCS_EquipmentValidationUtility.ValidateProfile(profile);

                    CCS_SurvivalValidationIssueSeverity severity =
                        profileValidation.IsSuccess
                            ? CCS_SurvivalValidationIssueSeverity.Info
                            : profileValidation.IsWarning
                                ? CCS_SurvivalValidationIssueSeverity.Warning
                                : CCS_SurvivalValidationIssueSeverity.Error;

                    report.AddIssue(severity, "Default Equipment Profile", profileValidation.Message);
                }
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Default Equipment Profile",
                    $"Missing required asset: {DefaultProfilePath}");
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,
                ValidatorId,
                "Equipment validator completed (runtime architecture foundation; no UI/combat/visual coupling).");
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
