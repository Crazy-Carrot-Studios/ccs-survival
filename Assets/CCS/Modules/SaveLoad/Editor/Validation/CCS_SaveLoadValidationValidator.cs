using System.IO;
using CCS.Modules.SaveLoad;
using CCS.Survival;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SaveLoadValidationValidator
// CATEGORY: Modules / SaveLoad / Editor / Validation
// PURPOSE: Validates save/load module folders, asmdefs, profile asset, and scripts.
// PLACEMENT: Registered on CCS_SurvivalValidationPipeline at editor load.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Framework-only checks. No world/building/combat persistence requirements.
// =============================================================================

namespace CCS.Modules.SaveLoad.Editor
{
    public sealed class CCS_SaveLoadValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string ModuleRoot = "Assets/CCS/Modules/SaveLoad";
        private const string RuntimeRoot = ModuleRoot + "/Runtime";
        private const string EditorRoot = ModuleRoot + "/Editor";
        private const string SurvivalRoot = "Assets/CCS/Survival";
        private const string DefaultProfilePath =
            SurvivalRoot + "/Profiles/SaveLoad/CCS_DefaultSaveLoadProfile.asset";
        private const string BootstrapPrefabPath = SurvivalRoot + "/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
        private const string BootstrapScenePath = SurvivalRoot + "/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string GameplayServiceRegistrationPath =
            SurvivalRoot + "/Runtime/Composition/CCS_SurvivalGameplayServiceRegistration.cs";
        private const string ModuleDocPath = ModuleRoot + "/Documentation/CCS_Save_Load_Module.md";

        #region Properties

        public string ValidatorId => "ccs.survival.validation.saveload";

        #endregion

        #region Public Methods

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateRequiredFolder(report, "Modules/SaveLoad", ModuleRoot);
            ValidateRequiredFolder(report, "Runtime/Interfaces", RuntimeRoot + "/Interfaces");
            ValidateRequiredFolder(report, "Runtime/Data", RuntimeRoot + "/Data");
            ValidateRequiredFolder(report, "Runtime/Services", RuntimeRoot + "/Services");
            ValidateRequiredFolder(report, "Runtime/Profiles", RuntimeRoot + "/Profiles");
            ValidateRequiredFolder(report, "Runtime/Events", RuntimeRoot + "/Events");
            ValidateRequiredFolder(report, "Runtime/Validation", RuntimeRoot + "/Validation");
            ValidateRequiredFolder(report, "Runtime/Testing", RuntimeRoot + "/Testing");
            ValidateRequiredFolder(report, "Editor/Validation", EditorRoot + "/Validation");
            ValidateRequiredFolder(report, "Documentation", ModuleRoot + "/Documentation");

            ValidateRequiredFile(report, "Runtime asmdef", RuntimeRoot + "/CCS.Modules.SaveLoad.Runtime.asmdef");
            ValidateRequiredFile(report, "Editor asmdef", EditorRoot + "/CCS.Modules.SaveLoad.Editor.asmdef");

            ValidateRequiredScript(report, "CCS_ISaveable", RuntimeRoot + "/Interfaces/CCS_ISaveable.cs");
            ValidateRequiredScript(report, "CCS_SaveGameData", RuntimeRoot + "/Data/CCS_SaveGameData.cs");
            ValidateRequiredScript(report, "CCS_SaveMetadata", RuntimeRoot + "/Data/CCS_SaveMetadata.cs");
            ValidateRequiredScript(report, "CCS_SaveSlotData", RuntimeRoot + "/Data/CCS_SaveSlotData.cs");
            ValidateRequiredScript(report, "CCS_SaveLoadResult", RuntimeRoot + "/Data/CCS_SaveLoadResult.cs");
            ValidateRequiredScript(report, "CCS_SaveLoadService", RuntimeRoot + "/Services/CCS_SaveLoadService.cs");
            ValidateRequiredScript(report, "CCS_SaveableRegistry", RuntimeRoot + "/Services/CCS_SaveableRegistry.cs");
            ValidateRequiredScript(report, "CCS_SavePathUtility", RuntimeRoot + "/Services/CCS_SavePathUtility.cs");
            ValidateRequiredScript(report, "CCS_SaveLoadRuntimeBridge", RuntimeRoot + "/Services/CCS_SaveLoadRuntimeBridge.cs");
            ValidateRequiredScript(report, "CCS_SaveLoadEventArgs", RuntimeRoot + "/Events/CCS_SaveLoadEventArgs.cs");
            ValidateRequiredScript(report, "CCS_SaveLoadEvents", RuntimeRoot + "/Events/CCS_SaveLoadEvents.cs");
            ValidateRequiredScript(report, "CCS_SaveLoadProfile", RuntimeRoot + "/Profiles/CCS_SaveLoadProfile.cs");
            ValidateRequiredScript(report, "CCS_SaveLoadValidationUtility", RuntimeRoot + "/Validation/CCS_SaveLoadValidationUtility.cs");
            ValidateRequiredScript(report, "CCS_TestSaveableComponent", RuntimeRoot + "/Testing/CCS_TestSaveableComponent.cs");
            ValidateRequiredScript(report, "CCS_SaveLoadDebugController", RuntimeRoot + "/Testing/CCS_SaveLoadDebugController.cs");
            ValidateRequiredScript(report, "CCS_SaveLoadDebugPanelPresenter", RuntimeRoot + "/Testing/CCS_SaveLoadDebugPanelPresenter.cs");
            ValidateRequiredScript(report, "CCS_SaveLoadDebugState", RuntimeRoot + "/Testing/CCS_SaveLoadDebugState.cs");

            ValidateDocumentationAsset(report, "Save Load Module Doc", ModuleDocPath);
            ValidateRuntimeScriptsAvoidUnityEditor(report, RuntimeRoot);
            ValidateGameplayServiceRegistration(report);
            ValidateBootstrapSaveLoadProfile(report);
            ValidateBootstrapTestSaveable(report);
            ValidateBootstrapDebugControls(report);
            ValidateSavePathUtility(report);

            if (File.Exists(DefaultProfilePath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Default Save Load Profile",
                    $"Asset present: {DefaultProfilePath}");

                CCS_SaveLoadProfile profile =
                    AssetDatabase.LoadAssetAtPath<CCS_SaveLoadProfile>(DefaultProfilePath);

                if (profile != null)
                {
                    CCS_SurvivalValidationResult profileValidation =
                        CCS_SaveLoadValidationUtility.ValidateProfile(profile);

                    CCS_SurvivalValidationIssueSeverity severity =
                        profileValidation.IsSuccess
                            ? CCS_SurvivalValidationIssueSeverity.Info
                            : profileValidation.IsWarning
                                ? CCS_SurvivalValidationIssueSeverity.Warning
                                : CCS_SurvivalValidationIssueSeverity.Error;

                    report.AddIssue(severity, "Default Save Load Profile", profileValidation.Message);
                }
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Default Save Load Profile",
                    $"Missing required asset: {DefaultProfilePath}");
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,
                ValidatorId,
                "Save/load validator completed (0.6.1 debug controls; gameplay module saves deferred).");
        }

        #endregion

        #region Private Methods

        private static void ValidateSavePathUtility(CCS_SurvivalValidationReport report)
        {
            CCS_SurvivalValidationResult pathValidation = CCS_SaveLoadValidationUtility.ValidateSavePathResolution();
            report.AddIssue(
                pathValidation.IsSuccess
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Save Path Utility",
                pathValidation.Message);
        }

        private static void ValidateBootstrapDebugControls(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(BootstrapScenePath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Warning,
                    "Bootstrap Save Debug Controls",
                    $"Missing bootstrap scene: {BootstrapScenePath}");
                return;
            }

            string sceneText = File.ReadAllText(BootstrapScenePath);
            if (sceneText.Contains("CCS_SaveLoadDebugController"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Bootstrap Save Debug Controls",
                    "Bootstrap scene includes CCS_SaveLoadDebugController.");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Bootstrap Save Debug Controls",
                    "Bootstrap scene is missing CCS_SaveLoadDebugController.");
            }

            if (sceneText.Contains("CCS_SaveLoadDebugPanelPresenter"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Bootstrap Save Debug Panel",
                    "Bootstrap scene includes CCS_SaveLoadDebugPanelPresenter.");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Bootstrap Save Debug Panel",
                    "Bootstrap scene is missing CCS_SaveLoadDebugPanelPresenter.");
            }
        }

        private static void ValidateGameplayServiceRegistration(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(GameplayServiceRegistrationPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Save Load Service Registration",
                    $"Missing gameplay service registration script: {GameplayServiceRegistrationPath}");
                return;
            }

            string registrationSource = File.ReadAllText(GameplayServiceRegistrationPath);
            if (registrationSource.Contains("CreateSaveLoadService")
                && registrationSource.Contains("CCS_SaveLoadProfile"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Save Load Service Registration",
                    "CCS_SurvivalGameplayServiceRegistration registers CCS_SaveLoadService.");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Error,
                "Save Load Service Registration",
                "CCS_SurvivalGameplayServiceRegistration is missing save/load service registration.");
        }

        private static void ValidateBootstrapSaveLoadProfile(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(BootstrapPrefabPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Warning,
                    "Bootstrap Save Load Profile",
                    $"Missing bootstrap prefab: {BootstrapPrefabPath}");
                return;
            }

            GameObject bootstrapPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BootstrapPrefabPath);
            Component gameplayServiceHost = FindGameplayServiceHost(bootstrapPrefab);
            if (gameplayServiceHost == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Bootstrap Save Load Profile",
                    "PF_CCS_Survival_BootstrapRoot is missing CCS_SurvivalGameplayServiceHost.");
                return;
            }

            SerializedObject serializedHost = new SerializedObject(gameplayServiceHost);
            SerializedProperty saveLoadProfileProperty = serializedHost.FindProperty("saveLoadProfile");
            if (saveLoadProfileProperty != null && saveLoadProfileProperty.objectReferenceValue != null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Bootstrap Save Load Profile",
                    "Save/load profile assigned on bootstrap prefab gameplay service host.");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Error,
                "Bootstrap Save Load Profile",
                "Save/load profile is not assigned on PF_CCS_Survival_BootstrapRoot.");
        }

        private static void ValidateBootstrapTestSaveable(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(BootstrapScenePath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Warning,
                    "Bootstrap Test Saveable",
                    $"Missing bootstrap scene: {BootstrapScenePath}");
                return;
            }

            string sceneText = File.ReadAllText(BootstrapScenePath);
            if (sceneText.Contains("CCS_TestSaveableComponent"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Bootstrap Test Saveable",
                    "Bootstrap scene includes CCS_TestSaveableComponent.");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Error,
                "Bootstrap Test Saveable",
                "Bootstrap scene is missing CCS_TestSaveableComponent.");
        }

        private static Component FindGameplayServiceHost(GameObject bootstrapPrefab)
        {
            if (bootstrapPrefab == null)
            {
                return null;
            }

            Component[] components = bootstrapPrefab.GetComponents<Component>();
            for (int index = 0; index < components.Length; index++)
            {
                Component component = components[index];
                if (component != null && component.GetType().Name == "CCS_SurvivalGameplayServiceHost")
                {
                    return component;
                }
            }

            return null;
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

        private static void ValidateRuntimeScriptsAvoidUnityEditor(
            CCS_SurvivalValidationReport report,
            string runtimeRoot)
        {
            bool foundEditorReference = false;
            string[] scriptGuids = AssetDatabase.FindAssets("t:Script", new[] { runtimeRoot });
            for (int index = 0; index < scriptGuids.Length; index++)
            {
                string path = AssetDatabase.GUIDToAssetPath(scriptGuids[index]);
                if (ScriptContainsUnityEditorReference(File.ReadAllText(path)))
                {
                    foundEditorReference = true;
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Runtime Script Editor Reference",
                        $"Runtime script references UnityEditor: {path}");
                }
            }

            if (!foundEditorReference)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Runtime Script Editor Reference",
                    "Runtime save/load scripts do not reference UnityEditor.");
            }
        }

        private static bool ScriptContainsUnityEditorReference(string contents)
        {
            string[] lines = contents.Split('\n');
            for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
            {
                string trimmedLine = lines[lineIndex].Trim();
                if (trimmedLine.StartsWith("//"))
                {
                    continue;
                }

                int commentIndex = trimmedLine.IndexOf("//", System.StringComparison.Ordinal);
                if (commentIndex >= 0)
                {
                    trimmedLine = trimmedLine.Substring(0, commentIndex).Trim();
                }

                if (trimmedLine.StartsWith("using UnityEditor", System.StringComparison.Ordinal) ||
                    trimmedLine.Contains("UnityEditor.", System.StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}
