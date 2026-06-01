using System.IO;
using CCS.Survival;
using CCS.Survival.Composition;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SaveValidationValidator
// CATEGORY: Modules / SaveSystem / Editor / Validation
// PURPOSE: Validates save system module layout, assets, composition, and smoke tests.
// PLACEMENT: Registered on CCS_SurvivalValidationPipeline at editor load.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Milestone 1.0.1 death, respawn, and save foundation.
// =============================================================================

namespace CCS.Modules.SaveSystem.Editor
{
    public sealed class CCS_SaveValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string ModuleRoot = "Assets/CCS/Modules/SaveSystem";
        private const string RuntimeRoot = ModuleRoot + "/Runtime";
        private const string EditorRoot = ModuleRoot + "/Editor";
        private const string SurvivalRoot = "Assets/CCS/Survival";
        private const string DefaultProfilePath = SurvivalRoot + "/Profiles/SaveSystem/CCS_DefaultSaveProfile.asset";
        private const string ModuleDocPath = ModuleRoot + "/Documentation/CCS_SaveSystem_Module.md";
        private const string BootstrapPrefabPath = SurvivalRoot + "/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
        private const string BootstrapScenePath = SurvivalRoot + "/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string CompositionRegistrationPath =
            SurvivalRoot + "/Runtime/Composition/CCS_SurvivalGameplayServiceRegistration.cs";

        #region Properties

        public string ValidatorId => "ccs.survival.validation.savesystem";

        #endregion

        #region Public Methods

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateRequiredFolder(report, "Modules/SaveSystem", ModuleRoot);
            ValidateRequiredFolder(report, "Runtime/Data", RuntimeRoot + "/Data");
            ValidateRequiredFolder(report, "Runtime/Services", RuntimeRoot + "/Services");
            ValidateRequiredFolder(report, "Runtime/Profiles", RuntimeRoot + "/Profiles");
            ValidateRequiredFolder(report, "Runtime/Events", RuntimeRoot + "/Events");
            ValidateRequiredFolder(report, "Runtime/Validation", RuntimeRoot + "/Validation");
            ValidateRequiredFolder(report, "Editor/Validation", EditorRoot + "/Validation");
            ValidateRequiredFolder(report, "Documentation", ModuleRoot + "/Documentation");

            ValidateRequiredFile(report, "Runtime asmdef", RuntimeRoot + "/CCS.Modules.SaveSystem.Runtime.asmdef");
            ValidateRequiredFile(report, "Editor asmdef", EditorRoot + "/CCS.Modules.SaveSystem.Editor.asmdef");
            ValidateRequiredScript(report, "CCS_SaveProfile", RuntimeRoot + "/Profiles/CCS_SaveProfile.cs");
            ValidateRequiredScript(report, "CCS_SaveData", RuntimeRoot + "/Data/CCS_SaveData.cs");
            ValidateRequiredScript(report, "CCS_SaveService", RuntimeRoot + "/Services/CCS_SaveService.cs");
            ValidateRequiredScript(report, "CCS_SaveRuntimeBridge", RuntimeRoot + "/Services/CCS_SaveRuntimeBridge.cs");
            ValidateRequiredScript(report, "CCS_SaveEventArgs", RuntimeRoot + "/Events/CCS_SaveEventArgs.cs");
            ValidateRequiredScript(report, "CCS_SaveStartupLoader", RuntimeRoot + "/Bootstrap/CCS_SaveStartupLoader.cs");

            ValidateDefaultProfile(report);
            ValidateCompositionRegistration(report);
            ValidateBootstrapWiring(report);
            ValidateSavePathAndSmokeTest(report);
            ValidateDocumentation(report, ModuleDocPath, "Save System module documentation");
        }

        #endregion

        #region Private Methods

        private static void ValidateDefaultProfile(CCS_SurvivalValidationReport report)
        {
            CCS_SaveProfile profile = AssetDatabase.LoadAssetAtPath<CCS_SaveProfile>(DefaultProfilePath);
            if (profile == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,"Save System Profile", $"Missing default profile: {DefaultProfilePath}");
                return;
            }

            CCS_SurvivalValidationResult validation = CCS_SaveValidationUtility.ValidateProfile(profile);
            if (!validation.IsSuccess)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,"Save System Profile", validation.Message);
                return;
            }

            if (profile.ProfileVersion != "1.0.1")
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Warning,
                    "Save System Profile",
                    $"Expected profileVersion 1.0.1 but found '{profile.ProfileVersion}'.");
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,"Save System Profile", "Default save profile validated.");
        }

        private static void ValidateCompositionRegistration(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(CompositionRegistrationPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,"Save System Composition", $"Missing file: {CompositionRegistrationPath}");
                return;
            }

            string source = File.ReadAllText(CompositionRegistrationPath);
            if (source.Contains("CCS_SaveService") && source.Contains("CreateSaveService"))
            {
                report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,"Save System Composition", "CCS_SaveService is registered in gameplay composition.");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,"Save System Composition", "CCS_SaveService registration is missing.");
            }
        }

        private static void ValidateBootstrapWiring(CCS_SurvivalValidationReport report)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(BootstrapPrefabPath);
            if (prefab == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,"Save System Bootstrap", $"Missing prefab: {BootstrapPrefabPath}");
                return;
            }

            CCS_SurvivalGameplayServiceHost host = prefab.GetComponent<CCS_SurvivalGameplayServiceHost>();
            if (host == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,"Save System Bootstrap", "Bootstrap prefab is missing CCS_SurvivalGameplayServiceHost.");
                return;
            }

            SerializedObject serializedHost = new SerializedObject(host);
            if (serializedHost.FindProperty("saveProfile").objectReferenceValue == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,"Save System Bootstrap", "Gameplay service host saveProfile is not assigned.");
            }
            else
            {
                report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,"Save System Bootstrap", "Gameplay service host references save profile.");
            }

            if (prefab.GetComponentInChildren<CCS_SaveStartupLoader>(true) == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,"Save System Bootstrap", "Bootstrap prefab is missing CCS_SaveStartupLoader.");
            }
            else
            {
                report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,"Save System Bootstrap", "CCS_SaveStartupLoader is present on bootstrap prefab.");
            }

            string sceneText = File.Exists(BootstrapScenePath) ? File.ReadAllText(BootstrapScenePath) : string.Empty;
            if (sceneText.Contains("CCS_PlayerRespawnPoint_Bootstrap"))
            {
                report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,"Save System Bootstrap", "Bootstrap scene contains respawn point object.");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,"Save System Bootstrap", "Bootstrap scene is missing CCS_PlayerRespawnPoint_Bootstrap.");
            }
        }

        private static void ValidateSavePathAndSmokeTest(CCS_SurvivalValidationReport report)
        {
            CCS_SaveProfile profile = AssetDatabase.LoadAssetAtPath<CCS_SaveProfile>(DefaultProfilePath);
            string savePath = CCS_SaveValidationUtility.ResolveSaveFilePath(profile);
            CCS_SurvivalValidationResult pathValidation = CCS_SaveValidationUtility.ValidateSaveFilePath(savePath);
            if (!pathValidation.IsSuccess)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,"Save System Path", pathValidation.Message);
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,"Save System Path", $"Resolved save path: {savePath}");

            CCS_SaveData sample = new CCS_SaveData
            {
                saveVersion = CCS_SaveData.CurrentSaveVersion,
                savedAtUtc = System.DateTime.UtcNow.ToString("o")
            };

            if (!CCS_SaveValidationUtility.TryRoundTripSerialize(sample, out string errorMessage))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,"Save System Smoke Test", errorMessage);
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,"Save System Smoke Test", "Save data JSON round-trip smoke test passed.");
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

        private static void ValidateDocumentation(
            CCS_SurvivalValidationReport report,
            string docPath,
            string label)
        {
            if (File.Exists(docPath))
            {
                report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,"Documentation", $"{label} present.");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Error,"Documentation", $"Missing documentation: {docPath}");
        }

        #endregion
    }
}
