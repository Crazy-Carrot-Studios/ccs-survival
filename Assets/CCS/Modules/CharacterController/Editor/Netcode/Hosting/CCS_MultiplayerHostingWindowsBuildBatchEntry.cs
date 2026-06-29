using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_MultiplayerHostingWindowsBuildBatchEntry
// CATEGORY: Modules / CharacterController / Tests / Netcode / Editor
// PURPOSE: Batch-mode Windows player build for hosting flow validation.
// PLACEMENT: Editor batch utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-19
// =============================================================================

namespace CCS.Modules.CharacterController.Netcode.Editor
{
    public static class CCS_MultiplayerHostingWindowsBuildBatchEntry
    {
        public static void BuildFromBatchMode()
        {
            CCS_MultiplayerHostingBuilder.VerifyAndRepairScene();

            string outputDirectory = Path.Combine(Application.dataPath, "..", "Builds");
            string executablePath = Path.Combine(outputDirectory, "CCS_Framework.exe");
            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes
                    .Where(scene => scene.enabled)
                    .Select(scene => scene.path)
                    .ToArray(),
                locationPathName = executablePath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development,
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            bool success = report.summary.result == BuildResult.Succeeded;
            if (!success)
            {
                Debug.LogError($"[Hosting Build] Failed: {report.summary.result}");
            }
            else
            {
                Debug.Log($"[Hosting Build] Succeeded: {executablePath}");
            }

            EditorApplication.Exit(success ? 0 : 1);
        }
    }
}
