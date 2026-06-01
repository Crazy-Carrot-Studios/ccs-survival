using System.IO;
using CCS.Modules.Wildlife;
using CCS.Survival;
using CCS.Survival.Composition;
using CCS.Survival.Editor.Development;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_WildlifeAiValidationValidator
// CATEGORY: Modules / Wildlife / Editor / Validation
// PURPOSE: Validates passive wildlife AI scripts, profile assets, and bootstrap content.
// PLACEMENT: Registered on CCS_SurvivalValidationPipeline at editor load.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Ensures no NavMesh, Rigidbody, combat, or editor references in runtime AI code.
// =============================================================================

namespace CCS.Modules.Wildlife.Editor
{
    public sealed class CCS_WildlifeAiValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string ModuleRoot = "Assets/CCS/Modules/Wildlife";
        private const string RuntimeRoot = ModuleRoot + "/Runtime";
        private const string SurvivalRoot = "Assets/CCS/Survival";
        private const string DefaultAiProfilePath = SurvivalRoot + "/Profiles/Wildlife/CCS_DefaultWildlifeAiProfile.asset";
        private const string BootstrapScenePath = SurvivalRoot + "/Scenes/SCN_CCS_Survival_Bootstrap.unity";
        private const string BootstrapPrefabPath = SurvivalRoot + "/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
        private const string CompositionRegistrationPath =
            SurvivalRoot + "/Runtime/Composition/CCS_SurvivalGameplayServiceRegistration.cs";
        private const string HudPresentationPath = "Assets/CCS/Modules/UI/Runtime/Services/CCS_HudPresentationService.cs";
        private const string HudWiringPath = "Assets/CCS/Modules/UI/Runtime/Services/CCS_HudGameplayServiceWiring.cs";
        private const string HudDebugPresenterPath =
            "Assets/CCS/Modules/UI/Runtime/Presentation/CCS_WildlifeAiDebugPresenter.cs";
        private const string ModuleDocPath = ModuleRoot + "/Documentation/CCS_Wildlife_Module.md";

        private static readonly string[] RequiredLivingWildlifeObjectNames =
        {
            "CCS_TestRabbit",
            "CCS_TestDeer"
        };

        #region Properties

        public string ValidatorId => "ccs.survival.validation.wildlifeai";

        #endregion

        #region Public Methods

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateRequiredFolder(report, "Runtime/AI", RuntimeRoot + "/AI");
            ValidateRequiredFolder(report, "Runtime/Movement", RuntimeRoot + "/Movement");
            ValidateRequiredFolder(report, "Runtime/States", RuntimeRoot + "/States");

            ValidateRequiredScript(report, "CCS_WildlifeAiState", RuntimeRoot + "/AI/CCS_WildlifeAiState.cs");
            ValidateRequiredScript(report, "CCS_WildlifeAiSnapshot", RuntimeRoot + "/AI/CCS_WildlifeAiSnapshot.cs");
            ValidateRequiredScript(report, "CCS_WildlifeAiSpecies", RuntimeRoot + "/AI/CCS_WildlifeAiSpecies.cs");
            ValidateRequiredScript(report, "CCS_WildlifeAgent", RuntimeRoot + "/AI/CCS_WildlifeAgent.cs");
            ValidateRequiredScript(report, "CCS_WildlifeAiProfile", RuntimeRoot + "/Profiles/CCS_WildlifeAiProfile.cs");
            ValidateRequiredScript(
                report,
                "CCS_WildlifeMovementController",
                RuntimeRoot + "/Movement/CCS_WildlifeMovementController.cs");
            ValidateRequiredScript(
                report,
                "CCS_WildlifeStateMachine",
                RuntimeRoot + "/States/CCS_WildlifeStateMachine.cs");
            ValidateRequiredScript(
                report,
                "CCS_WildlifeAiService",
                RuntimeRoot + "/Services/CCS_WildlifeAiService.cs");
            ValidateRequiredScript(
                report,
                "CCS_WildlifeAiValidationUtility",
                RuntimeRoot + "/Validation/CCS_WildlifeAiValidationUtility.cs");

            ValidateNoForbiddenRuntimeDependencies(report, RuntimeRoot);
            ValidateRuntimeScriptsAvoidUnityEditor(report, RuntimeRoot);

            if (File.Exists(DefaultAiProfilePath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Default Wildlife AI Profile",
                    $"Asset present: {DefaultAiProfilePath}");

                CCS_WildlifeAiProfile profile =
                    AssetDatabase.LoadAssetAtPath<CCS_WildlifeAiProfile>(DefaultAiProfilePath);
                if (profile != null)
                {
                    CCS_SurvivalValidationResult validation =
                        CCS_WildlifeAiValidationUtility.ValidateProfile(profile);
                    report.AddIssue(
                        validation.IsSuccess
                            ? CCS_SurvivalValidationIssueSeverity.Info
                            : CCS_SurvivalValidationIssueSeverity.Error,
                        "Default Wildlife AI Profile",
                        validation.Message);

                    report.AddIssue(
                        profile.ProfileVersion == "0.9.7"
                            ? CCS_SurvivalValidationIssueSeverity.Info
                            : CCS_SurvivalValidationIssueSeverity.Error,
                        "Default Wildlife AI Profile Version",
                        $"Profile version is {profile.ProfileVersion}.");
                }
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Default Wildlife AI Profile",
                    $"Missing required asset: {DefaultAiProfilePath}");
            }

            ValidateCompositionRegistration(report);
            ValidateHudIntegration(report);
            ValidateBootstrapLivingWildlife(report);
            ValidateDocumentationAsset(report, "Wildlife Module Doc", ModuleDocPath);

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,
                ValidatorId,
                "Wildlife AI validator completed (passive wander/flee foundation; no combat or death).");
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

        private static void ValidateRequiredScript(
            CCS_SurvivalValidationReport report,
            string context,
            string scriptPath)
        {
            if (File.Exists(scriptPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    context,
                    $"File exists: {scriptPath}");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Error,
                context,
                $"Missing required script: {scriptPath}");
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

        private static void ValidateNoForbiddenRuntimeDependencies(
            CCS_SurvivalValidationReport report,
            string runtimeFolderPath)
        {
            if (!Directory.Exists(runtimeFolderPath))
            {
                return;
            }

            string[] scriptPaths = Directory.GetFiles(runtimeFolderPath, "*.cs", SearchOption.AllDirectories);
            for (int index = 0; index < scriptPaths.Length; index++)
            {
                string contents = File.ReadAllText(scriptPaths[index]);
                if (contents.Contains("NavMeshAgent") || contents.Contains("UnityEngine.AI"))
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Runtime NavMesh Dependency",
                        $"{scriptPaths[index]} references NavMesh.");
                }

                if (contents.Contains("RequireComponent(typeof(Rigidbody))"))
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Runtime Rigidbody Dependency",
                        $"{scriptPaths[index]} requires Rigidbody.");
                }
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,
                "Runtime Movement Constraints",
                "Wildlife AI runtime scripts avoid NavMesh and Rigidbody dependencies.");
        }

        private static void ValidateCompositionRegistration(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(CompositionRegistrationPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Wildlife AI Service Registration",
                    $"Missing composition registration script: {CompositionRegistrationPath}");
                return;
            }

            string registrationSource = File.ReadAllText(CompositionRegistrationPath);
            report.AddIssue(
                registrationSource.Contains("CreateWildlifeAiService")
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Wildlife AI Service Registration",
                "Gameplay composition registers CCS_WildlifeAiService.");

            if (!File.Exists(BootstrapPrefabPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Bootstrap Wildlife AI Profile",
                    $"Missing bootstrap prefab: {BootstrapPrefabPath}");
                return;
            }

            string prefabText = File.ReadAllText(BootstrapPrefabPath);
            report.AddIssue(
                prefabText.Contains("wildlifeAiProfile")
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Bootstrap Wildlife AI Profile",
                "Bootstrap root assigns wildlife AI profile.");
        }

        private static void ValidateHudIntegration(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(HudPresentationPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "HUD Wildlife AI Wiring",
                    $"Missing HUD presentation service: {HudPresentationPath}");
                return;
            }

            string presentationSource = File.ReadAllText(HudPresentationPath);
            report.AddIssue(
                presentationSource.Contains("BindWildlifeAiService")
                    && presentationSource.Contains("WildlifeAiDebugLabel")
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "HUD Wildlife AI Wiring",
                "HUD presentation service exposes wildlife AI debug label and binding.");

            if (File.Exists(HudWiringPath))
            {
                string wiringSource = File.ReadAllText(HudWiringPath);
                report.AddIssue(
                    wiringSource.Contains("BindWildlifeAiService")
                        ? CCS_SurvivalValidationIssueSeverity.Info
                        : CCS_SurvivalValidationIssueSeverity.Error,
                    "HUD Wildlife AI Wiring",
                    "HUD gameplay wiring binds CCS_WildlifeAiService.");
            }

            report.AddIssue(
                File.Exists(HudDebugPresenterPath)
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "HUD Wildlife AI Debug Presenter",
                "Optional upper-right wildlife AI debug presenter exists.");
        }

        private static void ValidateBootstrapLivingWildlife(CCS_SurvivalValidationReport report)
        {
            if (!File.Exists(BootstrapScenePath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Bootstrap Living Wildlife",
                    $"Missing bootstrap scene: {BootstrapScenePath}");
                return;
            }

            string sceneText = File.ReadAllText(BootstrapScenePath);
            for (int index = 0; index < RequiredLivingWildlifeObjectNames.Length; index++)
            {
                string objectName = RequiredLivingWildlifeObjectNames[index];
                report.AddIssue(
                    sceneText.Contains(objectName)
                        ? CCS_SurvivalValidationIssueSeverity.Info
                        : CCS_SurvivalValidationIssueSeverity.Error,
                    "Bootstrap Living Wildlife",
                    sceneText.Contains(objectName)
                        ? $"Bootstrap scene contains {objectName}."
                        : $"Bootstrap scene is missing {objectName}.");
            }

            report.AddIssue(
                sceneText.Contains("CCS_WildlifeAgent")
                    ? CCS_SurvivalValidationIssueSeverity.Info
                    : CCS_SurvivalValidationIssueSeverity.Error,
                "Bootstrap Living Wildlife",
                "Bootstrap scene includes CCS_WildlifeAgent components.");
        }

        private static void ValidateRuntimeScriptsAvoidUnityEditor(
            CCS_SurvivalValidationReport report,
            string runtimeFolderPath)
        {
            if (!Directory.Exists(runtimeFolderPath))
            {
                return;
            }

            bool foundEditorReference = false;
            string[] scriptPaths = Directory.GetFiles(runtimeFolderPath, "*.cs", SearchOption.AllDirectories);
            for (int index = 0; index < scriptPaths.Length; index++)
            {
                if (ScriptContainsUnityEditorReference(File.ReadAllText(scriptPaths[index])))
                {
                    foundEditorReference = true;
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Runtime Editor Leak",
                        $"{scriptPaths[index]} references UnityEditor.");
                }
            }

            if (!foundEditorReference)
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Runtime Script Purity",
                    $"Runtime scripts under {runtimeFolderPath} avoid UnityEditor.");
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
