using System.IO;
using CCS.Modules.Hotbar;
using CCS.Survival;
using CCS.Survival.Composition;
using CCS.Survival.Editor.Development;
using CCS.Survival.Player;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_HotbarValidationValidator
// CATEGORY: Modules / Hotbar / Editor / Validation
// PURPOSE: Validates hotbar/active item module layout, composition wiring, and player driver.
// PLACEMENT: Registered on CCS_SurvivalValidationPipeline at editor load.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Milestone 1.2.2 active item foundation only. No final hotbar UI validation.
// =============================================================================

namespace CCS.Modules.Hotbar.Editor
{
    public sealed class CCS_HotbarValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string ModuleRoot = "Assets/CCS/Modules/Hotbar";
        private const string ActiveItemRoot = ModuleRoot + "/Runtime/ActiveItem";
        private const string RuntimeRoot = ModuleRoot + "/Runtime";
        private const string EditorRoot = ModuleRoot + "/Editor";
        private const string SurvivalRoot = "Assets/CCS/Survival";
        private const string DefaultProfilePath = SurvivalRoot + "/Profiles/Hotbar/CCS_DefaultActiveItemProfile.asset";
        private const string PlayerPrefabPath = SurvivalRoot + "/Prefabs/Player/PF_CCS_Player.prefab";
        private const string CompositionRegistrationPath =
            SurvivalRoot + "/Runtime/Composition/CCS_SurvivalGameplayServiceRegistration.cs";
        private const string ModuleDocPath = ModuleRoot + "/Documentation/CCS_Hotbar_Module.md";

        public string ValidatorId => "ccs.survival.validation.hotbar";

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateRequiredFolder(report, "Modules/Hotbar", ModuleRoot);
            ValidateRequiredFolder(report, "Runtime/ActiveItem", ActiveItemRoot);
            ValidateRequiredFolder(report, "Runtime/Profiles", RuntimeRoot + "/Profiles");
            ValidateRequiredFolder(report, "Runtime/Validation", RuntimeRoot + "/Validation");
            ValidateRequiredFolder(report, "Editor/Validation", EditorRoot + "/Validation");
            ValidateRequiredFolder(report, "Documentation", ModuleRoot + "/Documentation");

            ValidateRequiredFile(report, "Runtime asmdef", RuntimeRoot + "/CCS.Modules.Hotbar.Runtime.asmdef");
            ValidateRequiredFile(report, "Editor asmdef", EditorRoot + "/CCS.Modules.Hotbar.Editor.asmdef");

            ValidateRequiredScript(report, "CCS_ActiveItemSlotType", ActiveItemRoot + "/CCS_ActiveItemSlotType.cs");
            ValidateRequiredScript(report, "CCS_ActiveItemState", ActiveItemRoot + "/CCS_ActiveItemState.cs");
            ValidateRequiredScript(report, "CCS_ActiveItemSnapshot", ActiveItemRoot + "/CCS_ActiveItemSnapshot.cs");
            ValidateRequiredScript(report, "CCS_ActiveItemUseRequest", ActiveItemRoot + "/CCS_ActiveItemUseRequest.cs");
            ValidateRequiredScript(report, "CCS_ActiveItemUseResult", ActiveItemRoot + "/CCS_ActiveItemUseResult.cs");
            ValidateRequiredScript(report, "CCS_ActiveItemService", ActiveItemRoot + "/CCS_ActiveItemService.cs");
            ValidateRequiredScript(report, "CCS_ActiveItemRuntimeBridge", ActiveItemRoot + "/CCS_ActiveItemRuntimeBridge.cs");
            ValidateRequiredScript(report, "CCS_ActiveItemValidationUtility", RuntimeRoot + "/Validation/CCS_ActiveItemValidationUtility.cs");
            ValidateRequiredScript(report, "CCS_ActiveItemProfile", RuntimeRoot + "/Profiles/CCS_ActiveItemProfile.cs");
            ValidateRequiredScript(report, "CCS_PlayerActiveItemDriver", SurvivalRoot + "/Runtime/Player/CCS_PlayerActiveItemDriver.cs");

            ValidateDocumentationAsset(report, "Hotbar Module Doc", ModuleDocPath);
            ValidateProfileAsset(report);
            ValidateCompositionRegistration(report);
            ValidatePlayerDriver(report);

            CCS_SurvivalValidationResult folderValidation = CCS_ActiveItemValidationUtility.ValidateModuleFolders();
            report.AddIssue(
                folderValidation.IsSuccess
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Active Item Module Folders",
                folderValidation.Message);

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,
                ValidatorId,
                "Hotbar validator completed (1.2.2 active item foundation).");
        }

        private static void ValidateProfileAsset(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(DefaultProfilePath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Default Active Item Profile",
                    $"Missing required asset: {DefaultProfilePath}");
                return;
            }

            CCS_ActiveItemProfile profile = AssetDatabase.LoadAssetAtPath<CCS_ActiveItemProfile>(DefaultProfilePath);
            CCS_SurvivalValidationResult validation = CCS_ActiveItemValidationUtility.ValidateProfile(profile);
            report.AddIssue(
                validation.IsSuccess
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Default Active Item Profile",
                validation.Message);
        }

        private static void ValidateCompositionRegistration(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(CompositionRegistrationPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Active Item Service Registration",
                    $"Missing composition script: {CompositionRegistrationPath}");
                return;
            }

            string source = File.ReadAllText(CompositionRegistrationPath);
            if (source.Contains("CreateActiveItemService")
                && source.Contains("CCS_ActiveItemService")
                && source.Contains("BindCombatService"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Active Item Service Registration",
                    "CCS_SurvivalGameplayServiceRegistration registers and binds CCS_ActiveItemService.");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Active Item Service Registration",
                    "Gameplay service registration is missing active item service wiring.");
            }
        }

        private static void ValidatePlayerDriver(CCS_SurvivalValidationReport report)
        {
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            if (playerPrefab == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Player Active Item Driver",
                    $"Missing player prefab: {PlayerPrefabPath}");
                return;
            }

            if (playerPrefab.GetComponent<CCS_PlayerActiveItemDriver>() != null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Player Active Item Driver",
                    "PF_CCS_Player includes CCS_PlayerActiveItemDriver.");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Player Active Item Driver",
                    "PF_CCS_Player is missing CCS_PlayerActiveItemDriver.");
            }
        }

        private static void ValidateRequiredFolder(CCS_SurvivalValidationReport report, string context, string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                report.AddIssue(CCS_SurvivalValidationIssueSeverity.Info, context, $"Folder present: {folderPath}");
                return;
            }

            report.AddIssue(CCS_SurvivalValidationIssueSeverity.Error, context, $"Missing required folder: {folderPath}");
        }

        private static void ValidateRequiredFile(CCS_SurvivalValidationReport report, string context, string filePath)
        {
            if (File.Exists(filePath))
            {
                report.AddIssue(CCS_SurvivalValidationIssueSeverity.Info, context, $"File present: {filePath}");
                return;
            }

            report.AddIssue(CCS_SurvivalValidationIssueSeverity.Error, context, $"Missing required file: {filePath}");
        }

        private static void ValidateRequiredScript(CCS_SurvivalValidationReport report, string context, string scriptPath)
        {
            ValidateRequiredFile(report, context, scriptPath);
        }

        private static void ValidateDocumentationAsset(CCS_SurvivalValidationReport report, string context, string assetPath)
        {
            if (File.Exists(assetPath))
            {
                report.AddIssue(CCS_SurvivalValidationIssueSeverity.Info, context, $"Documentation present: {assetPath}");
                return;
            }

            report.AddIssue(CCS_SurvivalValidationIssueSeverity.Warning, context, $"Missing documentation: {assetPath}");
        }
    }
}
