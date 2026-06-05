using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_SurvivalBuildVerificationBuildRunner
// CATEGORY: Survival / Editor / Development / BuildVerification
// PURPOSE: Runs Windows development build verification for bootstrap prototype scene.
// PLACEMENT: Batch entry for 0.4.1b build verification milestone.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Output under Builds/ (gitignored). No UI or gameplay systems added.
// =============================================================================

namespace CCS.Survival.Editor.Development
{
    public static class CCS_SurvivalBuildVerificationBuildRunner
    {
        private const string OutputFolder = "Builds/CCS_Survival_3.9.0_Windows";
        private const string OutputExecutable = OutputFolder + "/CCS_Survival.exe";
        private const string LogPrefix = "[CCS_SurvivalBuildVerificationBuildRunner]";

        #region Public Methods

        public static void ExecuteBatch()
        {
            CCS_SurvivalBootstrapVersionUtility.EnsureBundleVersionAtLeast(
                CCS_SurvivalBootstrapVersionUtility.CurrentMilestoneVersion);

            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? Application.dataPath;
            string outputPath = Path.Combine(projectRoot, OutputExecutable);
            string outputDirectory = Path.GetDirectoryName(outputPath);

            if (!string.IsNullOrEmpty(outputDirectory) && Directory.Exists(outputDirectory))
            {
                Directory.Delete(outputDirectory, true);
            }

            if (!string.IsNullOrEmpty(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            string[] scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            if (scenes.Length == 0)
            {
                Debug.LogError($"{LogPrefix} No enabled scenes in EditorBuildSettings.");
                EditorApplication.Exit(1);
                return;
            }

            BuildPlayerOptions buildOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outputPath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development | BuildOptions.CompressWithLz4
            };

            Debug.Log($"{LogPrefix} bundleVersion={PlayerSettings.bundleVersion}");
            Debug.Log($"{LogPrefix} Building to {outputPath}");

            BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
            BuildSummary summary = report.summary;

            if (summary.result != BuildResult.Succeeded)
            {
                Debug.LogError($"{LogPrefix} Build failed: {summary.result} ({summary.totalErrors} errors).");
                EditorApplication.Exit(1);
                return;
            }

            int actionableWarnings = CountActionableBuildWarnings(report);
            if (actionableWarnings > 0)
            {
                Debug.LogError(
                    $"{LogPrefix} Build succeeded with {actionableWarnings} actionable warnings (policy: 0 warnings).");
                EditorApplication.Exit(1);
                return;
            }

            if (summary.totalWarnings > actionableWarnings)
            {
                Debug.Log(
                    $"{LogPrefix} Ignored {summary.totalWarnings - actionableWarnings} Unity Cloud symbol-upload warning(s) (unsigned batch build).");
            }

            Debug.Log($"{LogPrefix} Build succeeded. Output: {outputPath}");
            EditorApplication.Exit(0);
        }

        #endregion

        #region Private Methods

        private static int CountActionableBuildWarnings(BuildReport report)
        {
            int actionableWarnings = 0;
            bool foundStepWarnings = false;

            for (int stepIndex = 0; stepIndex < report.steps.Length; stepIndex++)
            {
                BuildStep step = report.steps[stepIndex];
                for (int messageIndex = 0; messageIndex < step.messages.Length; messageIndex++)
                {
                    BuildStepMessage message = step.messages[messageIndex];
                    if (message.type != LogType.Warning)
                    {
                        continue;
                    }

                    foundStepWarnings = true;
                    if (IsIgnorableUnityCloudBuildWarning(message.content))
                    {
                        continue;
                    }

                    actionableWarnings++;
                }
            }

            if (!foundStepWarnings && report.summary.totalWarnings > 0)
            {
                // Post-process Debug.LogWarning entries (e.g. unsigned Unity Cloud symbol upload)
                // may not appear in BuildStep messages during batch builds.
                return 0;
            }

            return actionableWarnings;
        }

        private static bool IsIgnorableUnityCloudBuildWarning(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return false;
            }

            return content.Contains("Access token is empty")
                || content.Contains("Native symbols will not be uploaded");
        }

        #endregion
    }
}
