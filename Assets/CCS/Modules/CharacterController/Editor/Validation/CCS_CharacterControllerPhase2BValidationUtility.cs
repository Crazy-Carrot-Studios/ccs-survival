using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CCS.Modules.CharacterController.Tests;
using CCS.Project;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_CharacterControllerPhase2BValidationUtility
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Validates Testing Manager foundation and editor menu reduction (v0.7.1d).
// PLACEMENT: Editor validation utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// NOTES: Invoked from Master Test validator and project audit paths.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_CharacterControllerPhase2BValidationUtility
    {
        private const string TestingManagerPath =
            "Assets/CCS/Modules/CharacterController/Tests/Runtime/Managers/CCS_CharacterControllerTestingManager.cs";

        private const string OfflineBootstrapperPath =
            "Assets/CCS/Modules/CharacterController/Tests/Runtime/Managers/CCS_MasterTestPlayerOfflineBootstrapper.cs";

        private const string TestDamageRouterPath =
            "Assets/CCS/Modules/CharacterController/Tests/Runtime/Diagnostics/CCS_TestPlayerAttributeDebugInputRouter.cs";

        private const string EquipmentFitStudioWindowPath =
            "Assets/CCS/Modules/CharacterController/Editor/EquipmentFitStudio/CCS_EquipmentFitStudioWindow.cs";

        private const string MasterTestBatchEntryPath =
            "Assets/CCS/Project/Editor/CCS_ProjectMasterTestBatchEntry.cs";

        private const string HostingBatchEntryPath =
            "Assets/CCS/Modules/CharacterController/Tests/Netcode/Editor/CCS_MultiplayerHostingSceneBatchEntry.cs";

        private static readonly string[] RuntimeOnGuiSourcePaths =
        {
            "Assets/CCS/Modules/CharacterController/Runtime/Animation/CCS_RevolverUpperBodyAnimator.cs",
            "Assets/CCS/Modules/CharacterController/Runtime/Components/CCS_CharacterCameraController.cs",
        };

        private static readonly string[] RemovedMenuWrapperPaths =
        {
            "Assets/CCS/Modules/CharacterController/Editor/CCS_CharacterControllerMasterTestMenus.cs",
        };

        public static CCS_SurvivalValidationResult ValidatePhase2BFoundation()
        {
            List<string> failures = new List<string>();
            ValidateTestingManagerSource(failures);
            ValidateRuntimeOnGuiPolicy(failures);
            ValidateMasterTestTestingManagerScene(failures);
            ValidateAsmdefBoundaries(failures);
            ValidateMenuReduction(failures);
            ValidateBatchReplacements(failures);
            ValidateEquipmentFitStudioKept(failures);
            ValidateAnimationFitStudioNotReintroduced(failures);

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass(
                    "Character Controller Testing Manager and Phase 2B menu reduction validated.");
        }

        private static void ValidateTestingManagerSource(List<string> failures)
        {
            AppendIfMissing(
                failures,
                File.Exists(TestingManagerPath),
                "Missing CCS_CharacterControllerTestingManager at " + TestingManagerPath);
            AppendIfMissing(
                failures,
                !File.Exists("Assets/CCS/Modules/CharacterController/Tests/Runtime/CCS_MasterTestSceneTestingManager.cs"),
                "CCS_MasterTestSceneTestingManager compatibility wrapper must be removed after Phase 2D migration.");

            if (!File.Exists(TestingManagerPath))
            {
                return;
            }

            string source = File.ReadAllText(TestingManagerPath);
                AppendIfMissing(
                    failures,
                    source.Contains("SetVerboseLogsEnabled")
                        && source.Contains("SetCameraDiagnosticsEnabled")
                        && source.Contains("SetAimDiagnosticsEnabled")
                        && source.Contains("SetAnimationDiagnosticsEnabled")
                        && source.Contains("SetInteractionDiagnosticsEnabled")
                        && source.Contains("SetTestDamageEnabled")
                        && source.Contains("SetVisualDebugHelpersEnabled")
                        && source.Contains("WriteOneShotReport"),
                    "CCS_CharacterControllerTestingManager must expose central debug toggle API.");
        }

        private static void ValidateRuntimeOnGuiPolicy(List<string> failures)
        {
            for (int i = 0; i < RuntimeOnGuiSourcePaths.Length; i++)
            {
                string path = RuntimeOnGuiSourcePaths[i];
                if (!File.Exists(path))
                {
                    failures.Add("Missing runtime source for OnGUI audit: " + path);
                    continue;
                }

                string source = File.ReadAllText(path);
                AppendIfMissing(
                    failures,
                    !Regex.IsMatch(source, @"\bvoid\s+OnGUI\s*\("),
                    path + " must not contain active OnGUI debug UI.");
            }
        }

        private static void ValidateMasterTestTestingManagerScene(List<string> failures)
        {
            if (!File.Exists(CCS_CharacterControllerMasterTestLayoutConstants.MasterTestScenePath))
            {
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(
                CCS_CharacterControllerMasterTestLayoutConstants.MasterTestScenePath,
                OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                failures.Add("Could not open Master Test scene for Testing Manager validation.");
                return;
            }

            CCS_CharacterControllerTestingManager[] managers =
                Object.FindObjectsByType<CCS_CharacterControllerTestingManager>(FindObjectsSortMode.None);
            AppendIfMissing(
                failures,
                managers.Length == 1,
                "Master Test scene must contain exactly one CCS_CharacterControllerTestingManager (found "
                + managers.Length
                + ").");
        }

        private static void ValidateAsmdefBoundaries(List<string> failures)
        {
            string runtimeAsmdefPath = CCS_CharacterControllerConstants.RuntimeAsmdefPath;
            if (!File.Exists(runtimeAsmdefPath))
            {
                failures.Add("Missing Runtime asmdef at " + runtimeAsmdefPath);
                return;
            }

            string runtimeAsmdef = File.ReadAllText(runtimeAsmdefPath);
            AppendIfMissing(
                failures,
                !runtimeAsmdef.Contains("CCS.Modules.CharacterController.Tests"),
                "Runtime asmdef must not reference Tests assemblies.");
        }

        private static void ValidateMenuReduction(List<string> failures)
        {
            for (int i = 0; i < RemovedMenuWrapperPaths.Length; i++)
            {
                string path = RemovedMenuWrapperPaths[i];
                AppendIfMissing(
                    failures,
                    !File.Exists(path),
                    "Obsolete menu wrapper must be removed: " + path);
            }

            string harnessMenuPath =
                "Assets/CCS/Modules/CharacterController/Tests/Netcode/Editor/CCS_CharacterControllerTestHarnessMenus.cs";
            if (File.Exists(harnessMenuPath))
            {
                string harnessSource = File.ReadAllText(harnessMenuPath);
                AppendIfMissing(
                    failures,
                    !harnessSource.Contains("[MenuItem("),
                    "CCS_CharacterControllerTestHarnessMenus must not register MenuItem wrappers.");
            }

            string aimPresetPath =
                "Assets/CCS/Modules/CharacterController/Editor/CCS_CharacterAimCameraProfilePresetUtility.cs";
            if (File.Exists(aimPresetPath))
            {
                string aimPresetSource = File.ReadAllText(aimPresetPath);
                AppendIfMissing(
                    failures,
                    !aimPresetSource.Contains("[MenuItem("),
                    "CCS_CharacterAimCameraProfilePresetUtility must be batch/builder-only (no MenuItem).");
            }

            string fpDefaultsPath =
                "Assets/CCS/Modules/CharacterController/Editor/CCS_CharacterFirstPersonCameraDefaultsUtility.cs";
            if (File.Exists(fpDefaultsPath))
            {
                string fpDefaultsSource = File.ReadAllText(fpDefaultsPath);
                AppendIfMissing(
                    failures,
                    !fpDefaultsSource.Contains("[MenuItem("),
                    "CCS_CharacterFirstPersonCameraDefaultsUtility must be batch/builder-only (no MenuItem).");
            }
        }

        private static void ValidateBatchReplacements(List<string> failures)
        {
            AppendIfMissing(
                failures,
                File.Exists(MasterTestBatchEntryPath),
                "Missing Master Test batch entry at " + MasterTestBatchEntryPath);
            AppendIfMissing(
                failures,
                File.Exists(HostingBatchEntryPath),
                "Missing hosting batch entry at " + HostingBatchEntryPath);

            if (File.Exists(MasterTestBatchEntryPath))
            {
                string masterBatchSource = File.ReadAllText(MasterTestBatchEntryPath);
                AppendIfMissing(
                    failures,
                    masterBatchSource.Contains("CCS_CharacterControllerMasterTestBuilder.SetupMasterTestScene"),
                    "Master Test batch must call CCS_CharacterControllerMasterTestBuilder directly.");
            }
        }

        private static void ValidateEquipmentFitStudioKept(List<string> failures)
        {
            AppendIfMissing(
                failures,
                File.Exists(EquipmentFitStudioWindowPath),
                "Equipment Fit Studio window must remain at " + EquipmentFitStudioWindowPath);
            AppendIfMissing(
                failures,
                Directory.Exists("Assets/CCS/Modules/CharacterController/Editor/EquipmentFitStudio"),
                "Equipment Fit Studio editor folder must remain.");
        }

        private static void ValidateAnimationFitStudioNotReintroduced(List<string> failures)
        {
            AppendIfMissing(
                failures,
                !Directory.Exists("Assets/CCS/Modules/CharacterController/Editor/AnimationFitStudio"),
                "Animation Fit Studio editor folder must not be reintroduced.");
            AppendIfMissing(
                failures,
                !File.Exists("Assets/CCS/Modules/CharacterController/Editor/CCS_RevolverFullDrawNudgeBatchEntry.cs"),
                "Obsolete FullDraw nudge batch entry must not return.");
        }

        private static void AppendIfMissing(List<string> failures, bool condition, string message)
        {
            if (!condition)
            {
                failures.Add(message);
            }
        }
    }
}
