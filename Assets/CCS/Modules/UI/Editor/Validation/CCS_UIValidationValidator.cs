using System.IO;
using CCS.Modules.UI;
using CCS.Survival;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_UIValidationValidator
// CATEGORY: Modules / UI / Editor / Validation
// PURPOSE: Validates UI module folders, asmdefs, profile asset, prefab, and scripts.
// PLACEMENT: Registered on CCS_SurvivalValidationPipeline at editor load.
// AUTHOR: James Schilz
// CREATED: 2026-05-30
// NOTES: Does not require gameplay services wired in scene.
// =============================================================================

namespace CCS.Modules.UI.Editor
{
    public sealed class CCS_UIValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string ModuleRoot = "Assets/CCS/Modules/UI";
        private const string RuntimeRoot = ModuleRoot + "/Runtime";
        private const string EditorRoot = ModuleRoot + "/Editor";
        private const string PrefabsRoot = ModuleRoot + "/Prefabs";
        private const string SurvivalRoot = "Assets/CCS/Survival";
        private const string DefaultProfilePath = SurvivalRoot + "/Profiles/UI/CCS_DefaultHudProfile.asset";
        private const string HudPrefabPath = PrefabsRoot + "/PF_CCS_HUD_Root.prefab";
        private const string BootstrapScenePath = SurvivalRoot + "/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string ModuleDocPath = ModuleRoot + "/Documentation/CCS_UI_HUD_Module.md";

        #region Properties

        public string ValidatorId => "ccs.survival.validation.ui";

        #endregion

        #region Public Methods

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateRequiredFolder(report, "Modules/UI", ModuleRoot);
            ValidateRequiredFolder(report, "Runtime/Profiles", RuntimeRoot + "/Profiles");
            ValidateRequiredFolder(report, "Runtime/Services", RuntimeRoot + "/Services");
            ValidateRequiredFolder(report, "Runtime/Presentation", RuntimeRoot + "/Presentation");
            ValidateRequiredFolder(report, "Runtime/Events", RuntimeRoot + "/Events");
            ValidateRequiredFolder(report, "Runtime/Validation", RuntimeRoot + "/Validation");
            ValidateRequiredFolder(report, "Editor/Validation", EditorRoot + "/Validation");
            ValidateRequiredFolder(report, "Prefabs", PrefabsRoot);
            ValidateRequiredFolder(report, "Documentation", ModuleRoot + "/Documentation");

            ValidateRequiredFile(report, "Runtime asmdef", RuntimeRoot + "/CCS.Modules.UI.Runtime.asmdef");
            ValidateRequiredFile(report, "Editor asmdef", EditorRoot + "/CCS.Modules.UI.Editor.asmdef");

            ValidateRequiredScript(report, "CCS_HudProfile", RuntimeRoot + "/Profiles/CCS_HudProfile.cs");
            ValidateRequiredScript(report, "CCS_HudLayoutSettings", RuntimeRoot + "/Profiles/CCS_HudLayoutSettings.cs");
            ValidateRequiredScript(report, "CCS_NotificationProfile", RuntimeRoot + "/Profiles/CCS_NotificationProfile.cs");
            ValidateRequiredScript(report, "CCS_HudPresentationService", RuntimeRoot + "/Services/CCS_HudPresentationService.cs");
            ValidateRequiredScript(report, "CCS_HudRootPresenter", RuntimeRoot + "/Presentation/CCS_HudRootPresenter.cs");
            ValidateRequiredScript(report, "CCS_HudLayoutApplicator", RuntimeRoot + "/Presentation/CCS_HudLayoutApplicator.cs");
            ValidateRequiredScript(report, "CCS_SurvivalBarPresenter", RuntimeRoot + "/Presentation/CCS_SurvivalBarPresenter.cs");
            ValidateRequiredScript(report, "CCS_InteractionPromptPresenter", RuntimeRoot + "/Presentation/CCS_InteractionPromptPresenter.cs");
            ValidateRequiredScript(report, "CCS_InventorySummaryPresenter", RuntimeRoot + "/Presentation/CCS_InventorySummaryPresenter.cs");
            ValidateRequiredScript(report, "CCS_EquipmentSummaryPresenter", RuntimeRoot + "/Presentation/CCS_EquipmentSummaryPresenter.cs");
            ValidateRequiredScript(report, "CCS_NotificationPresenter", RuntimeRoot + "/Presentation/CCS_NotificationPresenter.cs");
            ValidateRequiredScript(report, "CCS_NotificationQueue", RuntimeRoot + "/Presentation/CCS_NotificationQueue.cs");
            ValidateRequiredScript(report, "CCS_HudEvents", RuntimeRoot + "/Events/CCS_HudEvents.cs");
            ValidateRequiredScript(report, "CCS_HudEventArgs", RuntimeRoot + "/Events/CCS_HudEventArgs.cs");
            ValidateRequiredScript(report, "CCS_UIValidationUtility", RuntimeRoot + "/Validation/CCS_UIValidationUtility.cs");

            ValidateDocumentationAsset(report, "UI HUD Module Doc", ModuleDocPath);
            ValidateRuntimeScriptsAvoidUnityEditor(report, RuntimeRoot);

            if (File.Exists(DefaultProfilePath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Default HUD Profile",
                    $"Asset present: {DefaultProfilePath}");

                CCS_HudProfile profile = AssetDatabase.LoadAssetAtPath<CCS_HudProfile>(DefaultProfilePath);
                if (profile != null)
                {
                    CCS_SurvivalValidationResult profileValidation = CCS_UIValidationUtility.ValidateProfile(profile);
                    report.AddIssue(
                        profileValidation.IsSuccess
                            ? CCS_SurvivalValidationIssueSeverity.Info
                            : profileValidation.IsWarning
                                ? CCS_SurvivalValidationIssueSeverity.Warning
                                : CCS_SurvivalValidationIssueSeverity.Error,
                        "Default HUD Profile Validation",
                        profileValidation.Message);
                }
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Default HUD Profile",
                    $"Missing asset: {DefaultProfilePath}");
            }

            if (File.Exists(HudPrefabPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "HUD Prefab",
                    $"Asset present: {HudPrefabPath}");

                GameObject prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(HudPrefabPath);
                if (prefabRoot != null && prefabRoot.GetComponent<CCS_HudRootPresenter>() == null)
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "HUD Prefab",
                        "PF_CCS_HUD_Root is missing CCS_HudRootPresenter.");
                }
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "HUD Prefab",
                    $"Missing asset: {HudPrefabPath}");
            }

            ValidateBootstrapSceneHudInstance(report);
        }

        #endregion

        #region Private Methods

        private static void ValidateRequiredFolder(
            CCS_SurvivalValidationReport report,
            string label,
            string assetPath)
        {
            if (AssetDatabase.IsValidFolder(assetPath))
            {
                report.AddIssue(CCS_SurvivalValidationIssueSeverity.Info, label, $"Folder present: {assetPath}");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Error,
                label,
                $"Missing folder: {assetPath}");
        }

        private static void ValidateRequiredFile(
            CCS_SurvivalValidationReport report,
            string label,
            string assetPath)
        {
            if (File.Exists(assetPath))
            {
                report.AddIssue(CCS_SurvivalValidationIssueSeverity.Info, label, $"File present: {assetPath}");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Error,
                label,
                $"Missing file: {assetPath}");
        }

        private static void ValidateRequiredScript(
            CCS_SurvivalValidationReport report,
            string label,
            string scriptPath)
        {
            if (File.Exists(scriptPath))
            {
                report.AddIssue(CCS_SurvivalValidationIssueSeverity.Info, label, $"Script present: {scriptPath}");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Error,
                label,
                $"Missing script: {scriptPath}");
        }

        private static void ValidateDocumentationAsset(
            CCS_SurvivalValidationReport report,
            string label,
            string assetPath)
        {
            if (File.Exists(assetPath))
            {
                report.AddIssue(CCS_SurvivalValidationIssueSeverity.Info, label, $"Documentation present: {assetPath}");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Error,
                label,
                $"Missing documentation: {assetPath}");
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
                    "Runtime UI scripts do not reference UnityEditor.");
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

        private static void ValidateBootstrapSceneHudInstance(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(BootstrapScenePath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Bootstrap Scene HUD",
                    $"Missing scene: {BootstrapScenePath}");
                return;
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(HudPrefabPath);
            if (prefab == null)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Warning,
                    "Bootstrap Scene HUD",
                    "Cannot verify HUD instance until PF_CCS_HUD_Root prefab exists.");
                return;
            }

            string prefabGuid = AssetDatabase.AssetPathToGUID(HudPrefabPath);
            string sceneText = File.ReadAllText(BootstrapScenePath);
            if (sceneText.Contains(prefabGuid))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Bootstrap Scene HUD",
                    "Bootstrap scene references PF_CCS_HUD_Root.");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Error,
                "Bootstrap Scene HUD",
                "Bootstrap scene is missing PF_CCS_HUD_Root prefab instance.");
        }

        #endregion
    }
}
