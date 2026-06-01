using System.IO;
using CCS.Survival.Composition;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_PlayerDeathValidationValidator
// CATEGORY: Modules / PlayerDeath / Editor / Validation
// PURPOSE: Validates player death module layout, assets, and composition wiring.
// PLACEMENT: Registered on CCS_SurvivalValidationPipeline at editor load.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Milestone 1.0.1 death, respawn, and save foundation.
// =============================================================================

namespace CCS.Modules.PlayerDeath.Editor
{
    public sealed class CCS_PlayerDeathValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string ModuleRoot = "Assets/CCS/Modules/PlayerDeath";
        private const string RuntimeRoot = ModuleRoot + "/Runtime";
        private const string EditorRoot = ModuleRoot + "/Editor";
        private const string SurvivalRoot = "Assets/CCS/Survival";
        private const string DefaultProfilePath = SurvivalRoot + "/Profiles/PlayerDeath/CCS_DefaultPlayerDeathProfile.asset";
        private const string ModuleDocPath = ModuleRoot + "/Documentation/CCS_PlayerDeath_Module.md";
        private const string BootstrapPrefabPath = SurvivalRoot + "/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
        private const string CompositionRegistrationPath =
            SurvivalRoot + "/Runtime/Composition/CCS_SurvivalGameplayServiceRegistration.cs";

        #region Properties

        public string ValidatorId => "ccs.survival.validation.playerdeath";

        #endregion

        #region Public Methods

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateRequiredFolder(report, "Modules/PlayerDeath", ModuleRoot);
            ValidateRequiredFolder(report, "Runtime/Services", RuntimeRoot + "/Services");
            ValidateRequiredFolder(report, "Runtime/Components", RuntimeRoot + "/Components");
            ValidateRequiredFolder(report, "Runtime/Profiles", RuntimeRoot + "/Profiles");
            ValidateRequiredFolder(report, "Editor/Validation", EditorRoot + "/Validation");
            ValidateRequiredFolder(report, "Documentation", ModuleRoot + "/Documentation");

            ValidateRequiredFile(report, "Runtime asmdef", RuntimeRoot + "/CCS.Modules.PlayerDeath.Runtime.asmdef");
            ValidateRequiredScript(report, "CCS_PlayerDeathService", RuntimeRoot + "/Services/CCS_PlayerDeathService.cs");
            ValidateRequiredScript(report, "CCS_PlayerRespawnPoint", RuntimeRoot + "/Components/CCS_PlayerRespawnPoint.cs");
            ValidateRequiredScript(report, "CCS_PlayerDeathRuntimeBridge", RuntimeRoot + "/Services/CCS_PlayerDeathRuntimeBridge.cs");

            ValidateDefaultProfile(report);
            ValidateCompositionRegistration(report);
            ValidateBootstrapHost(report);
            ValidateDocumentation(report);
        }

        #endregion

        #region Private Methods

        private static void ValidateDefaultProfile(CCS_SurvivalValidationReport report)
        {
            CCS_PlayerDeathProfile profile =
                AssetDatabase.LoadAssetAtPath<CCS_PlayerDeathProfile>(DefaultProfilePath);
            if (profile == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,"Player Death Profile", $"Missing default profile: {DefaultProfilePath}");
                return;
            }

            if (profile.ProfileVersion != "1.0.1")
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Warning,
                    "Player Death Profile",
                    $"Expected profileVersion 1.0.1 but found '{profile.ProfileVersion}'.");
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,"Player Death Profile", "Default player death profile validated.");
        }

        private static void ValidateCompositionRegistration(CCS_SurvivalValidationReport report)
        {
            string source = File.ReadAllText(CompositionRegistrationPath);
            if (source.Contains("CCS_PlayerDeathService") && source.Contains("CreatePlayerDeathService"))
            {
                report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,"Player Death Composition", "CCS_PlayerDeathService is registered in gameplay composition.");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Error,"Player Death Composition", "CCS_PlayerDeathService registration is missing.");
        }

        private static void ValidateBootstrapHost(CCS_SurvivalValidationReport report)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(BootstrapPrefabPath);
            if (prefab == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,"Player Death Bootstrap", $"Missing prefab: {BootstrapPrefabPath}");
                return;
            }

            CCS_SurvivalGameplayServiceHost host = prefab.GetComponent<CCS_SurvivalGameplayServiceHost>();
            SerializedObject serializedHost = new SerializedObject(host);
            if (serializedHost.FindProperty("playerDeathProfile").objectReferenceValue == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,"Player Death Bootstrap", "Gameplay service host playerDeathProfile is not assigned.");
            }
            else
            {
                report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,"Player Death Bootstrap", "Gameplay service host references player death profile.");
            }
        }

        private static void ValidateDocumentation(CCS_SurvivalValidationReport report)
        {
            if (File.Exists(ModuleDocPath))
            {
                report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,"Documentation", "Player death module documentation present.");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Error,"Documentation", $"Missing documentation: {ModuleDocPath}");
        }

        private static void ValidateRequiredFolder(
            CCS_SurvivalValidationReport report,
            string label,
            string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,label, $"Folder present: {folderPath}");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Error,label, $"Missing folder: {folderPath}");
        }

        private static void ValidateRequiredFile(
            CCS_SurvivalValidationReport report,
            string label,
            string filePath)
        {
            if (File.Exists(filePath))
            {
                report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,label, $"File present: {filePath}");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Error,label, $"Missing file: {filePath}");
        }

        private static void ValidateRequiredScript(
            CCS_SurvivalValidationReport report,
            string label,
            string scriptPath)
        {
            ValidateRequiredFile(report, label, scriptPath);
        }

        #endregion
    }
}
