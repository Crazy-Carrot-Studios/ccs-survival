using System.IO;

// =============================================================================
// SCRIPT: CCS_SurvivalFoundationValidationValidator
// CATEGORY: Survival / Editor / Development / Validation
// PURPOSE: Foundation milestone validator for folder structure and project version checks.
// PLACEMENT: Registered by CCS_SurvivalValidationPipeline at first run.
// AUTHOR: James Schilz (Developer)
// CREATED: 2026-05-28
// NOTES: Future modules add separate validators; do not extend this class indefinitely.
// =============================================================================

namespace CCS.Survival.Editor.Development
{
    public sealed class CCS_SurvivalFoundationValidationValidator : CCS_ISurvivalValidationValidator
    {
        private const string SurvivalRoot = "Assets/CCS/Survival";
        private const string ExpectedBundleVersion = "0.9.2a";

        #region Properties

        public string ValidatorId => "ccs.survival.validation.foundation";

        #endregion

        #region Public Methods

        public void Validate(CCS_SurvivalValidationReport report)
        {
            ValidateRequiredFolder(report, "Runtime/Development", $"{SurvivalRoot}/Runtime/Development");
            ValidateRequiredFolder(report, "Runtime/Development/Diagnostics", $"{SurvivalRoot}/Runtime/Development/Diagnostics");
            ValidateRequiredFolder(report, "Runtime/Development/Testing", $"{SurvivalRoot}/Runtime/Development/Testing");
            ValidateRequiredFolder(report, "Runtime/Development/Testing/Traversal", $"{SurvivalRoot}/Runtime/Development/Testing/Traversal");
            ValidateRequiredFolder(report, "Runtime/Development/Testing/Simulation", $"{SurvivalRoot}/Runtime/Development/Testing/Simulation");
            ValidateRequiredFolder(report, "Runtime/Development/Testing/Inventory", $"{SurvivalRoot}/Runtime/Development/Testing/Inventory");
            ValidateRequiredFolder(report, "Runtime/Development/Testing/SaveLoad", $"{SurvivalRoot}/Runtime/Development/Testing/SaveLoad");
            ValidateRequiredFolder(report, "Runtime/Development/Settings", $"{SurvivalRoot}/Runtime/Development/Settings");
            ValidateRequiredFolder(report, "Runtime/Development/Bootstrap", $"{SurvivalRoot}/Runtime/Development/Bootstrap");
            ValidateRequiredFolder(report, "Editor/Development", $"{SurvivalRoot}/Editor/Development");
            ValidateRequiredFolder(report, "Editor/Development/Validation", $"{SurvivalRoot}/Editor/Development/Validation");
            ValidateRequiredFolder(report, "Documentation", $"{SurvivalRoot}/Documentation");

            ValidateRequiredAsset(report, "Runtime Assembly", $"{SurvivalRoot}/Runtime/CCS.Survival.Runtime.asmdef");
            ValidateRequiredAsset(report, "Editor Assembly", $"{SurvivalRoot}/Editor/CCS.Survival.Editor.asmdef");
            ValidateRequiredAsset(report, "Bootstrap Scene", $"{SurvivalRoot}/Scenes/SCN_CCS_Survival_Bootstrap.unity");
            ValidateRequiredAsset(report, "Bootstrap Prefab", $"{SurvivalRoot}/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab");
            ValidateRequiredAsset(report, "Validation Pipeline", $"{SurvivalRoot}/Editor/Development/Validation/CCS_SurvivalValidationPipeline.cs");

            ValidateDocumentationAsset(
                report,
                "Development Framework Support Doc",
                $"{SurvivalRoot}/Documentation/CCS_Survival_Development_Framework_Support.md");

            ValidateDocumentationAsset(
                report,
                "Module Roadmap Doc",
                $"{SurvivalRoot}/Documentation/CCS_Survival_Module_Roadmap.md");

            ValidateProjectVersion(report);
            ValidateBootstrapScenePlayerIntegration(report);
            CCS_BootstrapSceneValidationUtility.ValidatePlayableGround(report);

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Info,
                ValidatorId,
                "Foundation validator completed.");
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

        private static void ValidateRequiredAsset(
            CCS_SurvivalValidationReport report,
            string context,
            string assetPath)
        {
            if (File.Exists(assetPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    context,
                    $"Asset present: {assetPath}");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Error,
                context,
                $"Missing required asset: {assetPath}");
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

        private static void ValidateProjectVersion(CCS_SurvivalValidationReport report)
        {
            string projectSettingsPath = "ProjectSettings/ProjectSettings.asset";
            if (!File.Exists(projectSettingsPath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Project Version",
                    "ProjectSettings/ProjectSettings.asset was not found.");
                return;
            }

            string projectSettingsText = File.ReadAllText(projectSettingsPath);
            if (projectSettingsText.Contains($"bundleVersion: {ExpectedBundleVersion}"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Project Version",
                    $"bundleVersion matches expected milestone {ExpectedBundleVersion}.");
                return;
            }

            report.AddIssue(
                CCS_SurvivalValidationIssueSeverity.Warning,
                "Project Version",
                $"Expected bundleVersion {ExpectedBundleVersion}. Review ProjectSettings/ProjectSettings.asset.");
        }

        private static void ValidateBootstrapScenePlayerIntegration(CCS_SurvivalValidationReport report)
        {
            const string bootstrapScenePath = SurvivalRoot + "/Scenes/SCN_CCS_Survival_Bootstrap.unity";
            const string playerPrefabPath = SurvivalRoot + "/Prefabs/Player/PF_CCS_Player.prefab";
            const string bootstrapPrefabPath = SurvivalRoot + "/Prefabs/PF_CCS_Survival_BootstrapRoot.prefab";
            const string inputActionsPath = SurvivalRoot + "/Input/CCS_Survival_InputActions.inputactions";

            ValidateRequiredAsset(report, "Player Prefab", playerPrefabPath);
            ValidateRequiredAsset(report, "Survival Input Actions", inputActionsPath);

            if (!File.Exists(bootstrapScenePath))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Bootstrap Player Integration",
                    $"Missing bootstrap scene: {bootstrapScenePath}");
                return;
            }

            string sceneText = File.ReadAllText(bootstrapScenePath);
            if (sceneText.Contains("PF_CCS_Player"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Bootstrap Player Integration",
                    "Bootstrap scene contains PF_CCS_Player instance.");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Bootstrap Player Integration",
                    "Bootstrap scene is missing PF_CCS_Player instance.");
            }

            if (sceneText.Contains("PF_CCS_Survival_BootstrapRoot")
                || sceneText.Contains("CCS_SurvivalBootstrap")
                || sceneText.Contains("CCS_SurvivalGameplayServiceHost")
                || sceneText.Contains("f1a2b3c4d5e6478990abcdef12345606"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Bootstrap Player Integration",
                    "Bootstrap scene contains survival bootstrap root.");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Bootstrap Player Integration",
                    "Bootstrap scene is missing survival bootstrap root.");
            }

            if (sceneText.Contains("PF_CCS_HUD_Root") || sceneText.Contains("CCS_HudRootPresenter"))
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Info,
                    "Bootstrap Player Integration",
                    "Bootstrap scene contains HUD root.");
            }
            else
            {
                report.AddIssue(
                    CCS_SurvivalValidationIssueSeverity.Error,
                    "Bootstrap Player Integration",
                    "Bootstrap scene is missing HUD root.");
            }

            if (File.Exists(playerPrefabPath))
            {
                string prefabText = File.ReadAllText(playerPrefabPath);
                if (prefabText.Contains("CharacterController:"))
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Info,
                        "Player Prefab Components",
                        "Player prefab includes Unity CharacterController.");
                }
                else
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Player Prefab Components",
                        "Player prefab is missing Unity CharacterController.");
                }

                if (prefabText.Contains("Rigidbody:"))
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Player Prefab Components",
                        "Player prefab must not include Rigidbody.");
                }
                else
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Info,
                        "Player Prefab Components",
                        "Player prefab has no Rigidbody.");
                }

                if (prefabText.Contains("CameraPivot") && prefabText.Contains("Camera:"))
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Info,
                        "Player Prefab Components",
                        "Player prefab includes camera pivot and camera.");
                }
                else
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Player Prefab Components",
                        "Player prefab is missing camera pivot or camera.");
                }
            }

            if (File.Exists(bootstrapPrefabPath))
            {
                string bootstrapPrefabText = File.ReadAllText(bootstrapPrefabPath);
                if (bootstrapPrefabText.Contains("characterControllerProfile"))
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Info,
                        "Bootstrap Gameplay Services",
                        "Bootstrap root assigns character controller profile.");
                }
                else
                {
                    report.AddIssue(
                        CCS_SurvivalValidationIssueSeverity.Error,
                        "Bootstrap Gameplay Services",
                        "Bootstrap root is missing character controller profile assignment.");
                }
            }
        }

        #endregion
    }
}
