using System.Collections.Generic;
using System.IO;
using CCS.Survival.Composition;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_PlaytestValidationValidator
// CATEGORY: Modules / Playtesting / Editor / Validation
// PURPOSE: Validates playtesting module layout, assets, composition, and bootstrap HUD.
// PLACEMENT: Registered on CCS_SurvivalValidationPipeline at editor load.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-06-01
// NOTES: Milestone 1.0.2 manual playtest harness.
// =============================================================================

namespace CCS.Modules.Playtesting.Editor
{
    public sealed class CCS_PlaytestValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string ModuleRoot = "Assets/CCS/Modules/Playtesting";
        private const string RuntimeRoot = ModuleRoot + "/Runtime";
        private const string EditorRoot = ModuleRoot + "/Editor";
        private const string DefaultProfilePath = "Assets/CCS/Survival/Profiles/Playtesting/CCS_DefaultPlaytestProfile.asset";
        private const string ModuleDocPath = ModuleRoot + "/Documentation/CCS_Playtesting_Module.md";
        private const string BootstrapPrefabPath = "Assets/CCS/Survival/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
        private const string BootstrapScenePath = "Assets/CCS/Survival/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string CompositionRegistrationPath =
            "Assets/CCS/Survival/Runtime/Composition/CCS_SurvivalGameplayServiceRegistration.cs";

        private static readonly CCS_PlaytestStepType[] RequiredStepTypes =
        {
            CCS_PlaytestStepType.Spawn,
            CCS_PlaytestStepType.GatherResource,
            CCS_PlaytestStepType.EquipWeapon,
            CCS_PlaytestStepType.HuntWildlife,
            CCS_PlaytestStepType.HarvestCarcass,
            CCS_PlaytestStepType.CookFood,
            CCS_PlaytestStepType.EatFood,
            CCS_PlaytestStepType.PlaceBuilding,
            CCS_PlaytestStepType.SaveGame,
            CCS_PlaytestStepType.LoadGame,
            CCS_PlaytestStepType.TriggerDeath,
            CCS_PlaytestStepType.Respawn
        };

        #region Properties

        public string ValidatorId => "ccs.survival.validation.playtesting";

        #endregion

        #region Public Methods

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateRequiredFolder(report, "Modules/Playtesting", ModuleRoot);
            ValidateRequiredFolder(report, "Runtime/Data", RuntimeRoot + "/Data");
            ValidateRequiredFolder(report, "Runtime/Services", RuntimeRoot + "/Services");
            ValidateRequiredFolder(report, "Runtime/Profiles", RuntimeRoot + "/Profiles");
            ValidateRequiredFolder(report, "Runtime/Events", RuntimeRoot + "/Events");
            ValidateRequiredFolder(report, "Runtime/UI", RuntimeRoot + "/UI");
            ValidateRequiredFolder(report, "Editor/Validation", EditorRoot + "/Validation");
            ValidateRequiredFolder(report, "Documentation", ModuleRoot + "/Documentation");

            ValidateRequiredFile(report, "Runtime asmdef", RuntimeRoot + "/CCS.Modules.Playtesting.Runtime.asmdef");
            ValidateRequiredFile(report, "Editor asmdef", EditorRoot + "/CCS.Modules.Playtesting.Editor.asmdef");
            ValidateRequiredScript(report, "CCS_PlaytestService", RuntimeRoot + "/Services/CCS_PlaytestService.cs");
            ValidateRequiredScript(report, "CCS_PlaytestRuntimeBridge", RuntimeRoot + "/Services/CCS_PlaytestRuntimeBridge.cs");
            ValidateRequiredScript(report, "CCS_PlaytestHud", RuntimeRoot + "/UI/CCS_PlaytestHud.cs");

            ValidateDefaultProfile(report);
            ValidateCompositionRegistration(report);
            ValidateBootstrapWiring(report);
            ValidateDocumentation(report);
        }

        #endregion

        #region Private Methods

        private static void ValidateDefaultProfile(CCS_SurvivalValidationReport report)
        {
            CCS_PlaytestProfile profile = AssetDatabase.LoadAssetAtPath<CCS_PlaytestProfile>(DefaultProfilePath);
            if (profile == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Playtesting Profile",
                    $"Missing default profile: {DefaultProfilePath}");
                return;
            }

            if (!profile.EnableHarness)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Warning,
                    "Playtesting Profile",
                    "Default playtest profile has harness disabled.");
            }

            if (profile.ProfileVersion != "1.0.3")
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Warning,
                    "Playtesting Profile",
                    $"Expected profileVersion 1.0.3 but found '{profile.ProfileVersion}'.");
            }

            ValidateRequiredStepTypes(report, profile);
            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,
                "Playtesting Profile",
                "Default playtest profile validated.");
        }

        private static void ValidateRequiredStepTypes(CCS_SurvivalValidationReport report, CCS_PlaytestProfile profile)
        {
            HashSet<CCS_PlaytestStepType> foundTypes = new HashSet<CCS_PlaytestStepType>();
            IReadOnlyList<CCS_PlaytestStepDefinition> steps = profile.StepDefinitions;
            for (int index = 0; index < steps.Count; index++)
            {
                CCS_PlaytestStepDefinition step = steps[index];
                if (step != null)
                {
                    foundTypes.Add(step.StepType);
                }
            }

            for (int index = 0; index < RequiredStepTypes.Length; index++)
            {
                CCS_PlaytestStepType requiredType = RequiredStepTypes[index];
                if (!foundTypes.Contains(requiredType))
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Playtesting Checklist",
                        $"Default checklist is missing required step type: {requiredType}.");
                }
            }
        }

        private static void ValidateCompositionRegistration(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(CompositionRegistrationPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Playtesting Composition",
                    $"Missing file: {CompositionRegistrationPath}");
                return;
            }

            string source = File.ReadAllText(CompositionRegistrationPath);
            if (source.Contains("CCS_PlaytestService") && source.Contains("CreatePlaytestService"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Playtesting Composition",
                    "CCS_PlaytestService is registered in gameplay composition.");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Playtesting Composition",
                    "CCS_PlaytestService registration is missing.");
            }
        }

        private static void ValidateBootstrapWiring(CCS_SurvivalValidationReport report)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(BootstrapPrefabPath);
            if (prefab == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Playtesting Bootstrap",
                    $"Missing prefab: {BootstrapPrefabPath}");
                return;
            }

            CCS_SurvivalGameplayServiceHost host = prefab.GetComponent<CCS_SurvivalGameplayServiceHost>();
            if (host == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Playtesting Bootstrap",
                    "Bootstrap prefab is missing CCS_SurvivalGameplayServiceHost.");
                return;
            }

            SerializedObject serializedHost = new SerializedObject(host);
            if (serializedHost.FindProperty("playtestProfile").objectReferenceValue == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Playtesting Bootstrap",
                    "Gameplay service host playtestProfile is not assigned.");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Playtesting Bootstrap",
                    "Gameplay service host references playtest profile.");
            }

            if (prefab.GetComponentInChildren<CCS_PlaytestHud>(true) == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Playtesting Bootstrap",
                    "Bootstrap prefab is missing CCS_PlaytestHud.");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Playtesting Bootstrap",
                    "CCS_PlaytestHud is present on bootstrap prefab.");
            }

            string sceneText = File.Exists(BootstrapScenePath) ? File.ReadAllText(BootstrapScenePath) : string.Empty;
            if (sceneText.Contains("CCS_PlaytestHud") || sceneText.Contains("PlaytestHarness"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Playtesting Bootstrap",
                    "Bootstrap scene references playtest harness objects.");
            }
            else if (prefab.GetComponentInChildren<CCS_PlaytestHud>(true) != null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Playtesting Bootstrap",
                    "Playtest HUD is wired through bootstrap prefab instance.");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Playtesting Bootstrap",
                    "Bootstrap scene does not reference CCS_PlaytestHud.");
            }
        }

        private static void ValidateDocumentation(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(ModuleDocPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Playtesting Documentation",
                    $"Missing module documentation: {ModuleDocPath}");
                return;
            }

            string documentation = File.ReadAllText(ModuleDocPath);
            ValidateHotkeyDocumented(report, documentation, "F6");
            ValidateHotkeyDocumented(report, documentation, "F7");
            ValidateHotkeyDocumented(report, documentation, "F10");
            ValidateHotkeyDocumented(report, documentation, "F11");
            ValidateHotkeyDocumented(report, documentation, "F12");
            ValidateHotkeyDocumented(report, documentation, "B");

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,
                "Playtesting Documentation",
                "Playtesting module documentation validated.");
        }

        private static void ValidateHotkeyDocumented(
            CCS_SurvivalValidationReport report,
            string documentation,
            string hotkey)
        {
            if (!documentation.Contains(hotkey))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Playtesting Documentation",
                    $"Module documentation does not document hotkey {hotkey}.");
            }
        }

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
                $"Missing folder: {folderPath}");
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
                $"Missing file: {filePath}");
        }

        private static void ValidateRequiredScript(
            CCS_SurvivalValidationReport report,
            string context,
            string scriptPath)
        {
            ValidateRequiredFile(report, context, scriptPath);
        }

        #endregion
    }
}
