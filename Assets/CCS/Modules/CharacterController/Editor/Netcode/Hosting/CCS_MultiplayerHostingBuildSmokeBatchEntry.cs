using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Debug = UnityEngine.Debug;

// =============================================================================
// SCRIPT: CCS_MultiplayerHostingBuildSmokeBatchEntry
// CATEGORY: Modules / CharacterController / Tests / Netcode / Editor
// PURPOSE: Builds Windows player and runs automated host smoke test.
// PLACEMENT: Editor batch utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-20
// =============================================================================

namespace CCS.Modules.CharacterController.Netcode.Editor
{
    public static class CCS_MultiplayerHostingBuildSmokeBatchEntry
    {
        public static void RunFromBatchMode()
        {
            CCS_CharacterControllerHostingMenus.RunBatchBuildAndValidate();

            string outputDirectory = Path.Combine(Application.dataPath, "..", "Builds");
            string executablePath = Path.Combine(outputDirectory, "CCS_Framework.exe");
            string smokeLogPath = Path.Combine(outputDirectory, "host-smoke.log");

            if (File.Exists(smokeLogPath))
            {
                File.Delete(smokeLogPath);
            }

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
            if (report.summary.result != BuildResult.Succeeded)
            {
                Debug.LogError($"[Hosting Smoke] Build failed: {report.summary.result}");
                EditorApplication.Exit(1);
                return;
            }

            bool smokePassed = RunHostSmokeTest(executablePath, smokeLogPath);
            EditorApplication.Exit(smokePassed ? 0 : 1);
        }

        private static bool RunHostSmokeTest(string executablePath, string smokeLogPath)
        {
            if (!File.Exists(executablePath))
            {
                Debug.LogError("[Hosting Smoke] Executable was not found: " + executablePath);
                return false;
            }

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = executablePath,
                Arguments = "-ccsNetcodeHostSmoke -logFile \"" + smokeLogPath + "\"",
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using Process process = Process.Start(startInfo);
            if (process == null)
            {
                Debug.LogError("[Hosting Smoke] Failed to launch executable.");
                return false;
            }

            bool exited = process.WaitForExit(120000);
            if (!exited)
            {
                process.Kill();
                Debug.LogError("[Hosting Smoke] Smoke test timed out.");
                return false;
            }

            string logText = File.Exists(smokeLogPath) ? File.ReadAllText(smokeLogPath) : string.Empty;
            bool hostStarted = logText.Contains("[Hosting Flow] StartHost returned: True")
                || logText.Contains("[Hosting Smoke] StartHost returned: True");
            bool noMissingReference = !logText.Contains("MissingReferenceException");

            if (process.ExitCode == 0 && hostStarted && noMissingReference)
            {
                Debug.Log("[Hosting Smoke] PASS");
                return true;
            }

            Debug.LogError(
                $"[Hosting Smoke] FAIL exitCode={process.ExitCode} hostStarted={hostStarted.ToString()} "
                + $"noMissingReference={noMissingReference.ToString()}");
            return false;
        }
    }
}
