using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CCS.Modules.CharacterController;
using CCS.Modules.CharacterController.Diagnostics;
using CCS.Modules.CharacterController.Local;
using CCS.Modules.Weapons;
using CCS.Project;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_ValidationCleanupAimDebugToggleValidationUtility
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Validates v0.7.9 validation cleanup and diagnostics aim debug toggle.
// PLACEMENT: Editor validation utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_ValidationCleanupAimDebugToggleValidationUtility
    {
        private const string EquipmentFitStudioWindowPath =
            "Assets/CCS/Modules/CharacterController/Editor/EquipmentFitStudio/CCS_EquipmentFitStudioWindow.cs";

        private const string CharacterControllerTestsRoot =
            "Assets/CCS/Modules/CharacterController/Tests";

        private const string AnimationFitStudioRoot =
            "Assets/CCS/Modules/CharacterController/Editor/AnimationFitStudio";

        private const string DiagnosticsManagerSourcePath =
            "Assets/CCS/Modules/CharacterController/Runtime/Diagnostics/CCS_CharacterControllerDiagnosticsManager.cs";

        private const string AimAnimatorSourcePath =
            "Assets/CCS/Modules/CharacterController/Runtime/Animation/CCS_SingleRevolverAimAnimator.cs";

        private const string AimDebugSourceInterfacePath =
            "Assets/CCS/Modules/CharacterController/Runtime/Data/CCS_IRevolverAimSetupPoseDebugSource.cs";

        private const string EquipmentVisualSourcePath =
            "Assets/CCS/Modules/Weapons/Runtime/Components/CCS_PlayerEquipmentVisualController.cs";

        private static readonly string[] ActiveSourceScanRoots =
        {
            "Assets/CCS/Modules",
            "Assets/CCS/Project",
        };

        private static readonly string[] HistoricalDocScanRoots =
        {
            "Assets/CCS/Modules/CharacterController/Documentation",
            "Assets/CCS/Modules/Interaction/Documentation",
            "Assets/CCS/Project/Documentation",
            "README.md",
        };

        public static CCS_SurvivalValidationResult ValidateValidationCleanupAimDebugToggle()
        {
            List<string> failures = new List<string>();
            List<string> warnings = new List<string>();

            ValidateRemovedDetectionCubeAssets(failures);
            ValidateRemovedDetectionCubeActiveReferences(failures);
            ValidateWeaponDamageTargetLocation(failures, warnings);
            ValidateProductionPlayerPrototypeVisualsRemoved(failures);
            ValidateValidationSceneDiagnostics(failures);
            ValidateAimAnimatorDebugOverride(failures);
            ValidateEquipmentVisualSetupPosePath(failures);
            ValidateSingleRevolverAimLayerPreserved(failures);
            ValidateMissingScripts(failures);
            ValidateTestsFolderRemoved(failures);
            ValidateAnimationFitStudioNotPresent(failures);
            ValidateEquipmentFitStudioPresent(failures);
            CollectDeferredWarnings(warnings);

            if (failures.Count > 0)
            {
                return CCS_SurvivalValidationResult.Fail(string.Join(" ", failures));
            }

            string message = "Validation cleanup and aim debug toggle validated.";
            if (warnings.Count > 0)
            {
                message += " Warnings: " + string.Join(" ", warnings);
            }

            return CCS_SurvivalValidationResult.Pass(message);
        }

        private static void ValidateRemovedDetectionCubeAssets(List<string> failures)
        {
            AppendIfPresent(
                failures,
                File.Exists(CCS_CharacterControllerConstants.LegacyTestDetectionCubeBootstrapScriptPath),
                "CCS_TestDetectionCubeSceneBootstrap must be removed.");

            AppendIfPresent(
                failures,
                File.Exists(CCS_CharacterControllerConstants.LegacyTestDetectionCubeUtilityScriptPath),
                "CCS_TestDetectionCubeUtility must be removed.");
        }

        private static void ValidateRemovedDetectionCubeActiveReferences(List<string> failures)
        {
            Scene scene = EditorSceneManager.OpenScene(
                CCS_CharacterControllerMasterTestLayoutConstants.MasterTestScenePath,
                OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                failures.Add(
                    "Could not open validation scene at "
                    + CCS_CharacterControllerMasterTestLayoutConstants.MasterTestScenePath
                    + ".");
                return;
            }

            foreach (string token in CCS_CharacterControllerConstants.RemovedTestDetectionCubeReferenceTokens)
            {
                AppendIfPresent(
                    failures,
                    FindSceneObjectByName(scene, token) != null,
                    "Validation scene still contains " + token + ".");
            }

            foreach (string scanRoot in ActiveSourceScanRoots)
            {
                if (!Directory.Exists(scanRoot))
                {
                    continue;
                }

                string[] files = Directory.GetFiles(scanRoot, "*.*", SearchOption.AllDirectories)
                    .Where(path =>
                        path.EndsWith(".cs")
                        || path.EndsWith(".unity")
                        || path.EndsWith(".prefab")
                        || path.EndsWith(".asset"))
                    .ToArray();

                for (int i = 0; i < files.Length; i++)
                {
                    string normalizedPath = files[i].Replace('\\', '/');
                    if (ShouldSkipActiveReferenceScan(normalizedPath))
                    {
                        continue;
                    }

                    string contents = File.ReadAllText(normalizedPath);
                    for (int tokenIndex = 0; tokenIndex < CCS_CharacterControllerConstants.RemovedTestDetectionCubeReferenceTokens.Length; tokenIndex++)
                    {
                        string token = CCS_CharacterControllerConstants.RemovedTestDetectionCubeReferenceTokens[tokenIndex];
                        if (contents.Contains(token))
                        {
                            failures.Add("Active reference to removed detection cube token '" + token + "' in " + normalizedPath + ".");
                        }
                    }
                }
            }
        }

        private static bool ShouldSkipActiveReferenceScan(string normalizedPath)
        {
            if (normalizedPath.Contains("/Documentation/"))
            {
                return true;
            }

            return normalizedPath.EndsWith("CCS_ValidationCleanupAimDebugToggleValidationUtility.cs")
                || normalizedPath.EndsWith("CCS_ValidationCleanupAimDebugToggleReportBuilder.cs")
                || normalizedPath.EndsWith("CCS_RevolverHandSocketPreviewValidationUtility.cs")
                || normalizedPath.EndsWith("CCS_RevolverHandSocketPreviewReportBuilder.cs")
                || normalizedPath.EndsWith("CCS_CharacterControllerConstants.cs")
                || normalizedPath.EndsWith("CCS_InteractionModuleValidator.cs")
                || normalizedPath.EndsWith("CCS_InteractionDetectionTestBuilder.cs")
                || normalizedPath.EndsWith("CCS_CharacterControllerMasterTestBuilder.cs");
        }

        private static void ValidateWeaponDamageTargetLocation(List<string> failures, List<string> warnings)
        {
            AppendIfMissing(
                failures,
                File.Exists(CCS_CharacterControllerConstants.PrototypingWeaponDamageTargetPrefabPath),
                "CCS_TestWeaponDamageTarget must exist under CharacterController Prototyping/Targets.");

            AppendIfPresent(
                failures,
                File.Exists(CCS_CharacterControllerConstants.LegacyWeaponsTestDamageTargetPrefabPath),
                "Legacy weapons Tests path for CCS_TestWeaponDamageTarget must not remain.");

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                CCS_CharacterControllerConstants.PrototypingWeaponDamageTargetPrefabPath);
            AppendIfMissing(failures, prefab != null, "Could not load prototyping weapon damage target prefab.");
            if (prefab != null)
            {
                AppendIfMissing(
                    failures,
                    prefab.GetComponent<CCS_TestDamageTarget>() != null,
                    "Prototyping weapon damage target prefab missing CCS_TestDamageTarget.");
                AppendIfMissing(
                    failures,
                    GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(prefab) == 0,
                    "Prototyping weapon damage target prefab has missing scripts.");
            }

            if (CCS_WeaponsConstants.TestDamageTargetPrefabPath
                != CCS_CharacterControllerConstants.PrototypingWeaponDamageTargetPrefabPath)
            {
                failures.Add("CCS_WeaponsConstants.TestDamageTargetPrefabPath must point to CharacterController Prototyping/Targets.");
            }

            warnings.Add("CCS_TestWeaponDamageTarget is a Prototyping asset, not a production gameplay prefab.");
        }

        private static void ValidateValidationSceneDiagnostics(List<string> failures)
        {
            Scene scene = EditorSceneManager.OpenScene(
                CCS_CharacterControllerMasterTestLayoutConstants.MasterTestScenePath,
                OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                return;
            }

            CCS_CharacterControllerDiagnosticsManager[] managers =
                Object.FindObjectsByType<CCS_CharacterControllerDiagnosticsManager>(FindObjectsSortMode.None);
            int sceneManagerCount = 0;
            for (int i = 0; i < managers.Length; i++)
            {
                if (managers[i] != null && managers[i].gameObject.scene == scene)
                {
                    sceneManagerCount++;
                }
            }

            AppendIfMissing(
                failures,
                sceneManagerCount == 1,
                "Validation scene must contain exactly one CCS_CharacterControllerDiagnosticsManager (found "
                + sceneManagerCount
                + ").");

            GameObject diagnosticsObject = GameObject.Find("CCS_DiagnosticsManager");
            AppendIfMissing(
                failures,
                diagnosticsObject != null && diagnosticsObject.scene == scene,
                "Validation scene must contain CCS_DiagnosticsManager.");

            if (diagnosticsObject == null)
            {
                return;
            }

            CCS_CharacterControllerDiagnosticsManager manager =
                diagnosticsObject.GetComponent<CCS_CharacterControllerDiagnosticsManager>();
            AppendIfMissing(
                failures,
                manager != null,
                "CCS_DiagnosticsManager must contain CCS_CharacterControllerDiagnosticsManager.");

            AppendIfPresent(
                failures,
                diagnosticsObject.GetComponent("CCS_TestingManager") != null,
                "Validation scene must not contain CCS_TestingManager.");

            if (manager == null)
            {
                return;
            }

            SerializedObject serializedManager = new SerializedObject(manager);
            SerializedProperty forceSetupPoseProperty = serializedManager.FindProperty("forceRevolverAimSetupPose");
            AppendIfMissing(
                failures,
                forceSetupPoseProperty != null,
                "Diagnostics manager must expose Force Revolver Aim Setup Pose bool (forceRevolverAimSetupPose).");

            AppendIfMissing(
                failures,
                forceSetupPoseProperty == null || !forceSetupPoseProperty.boolValue,
                "Force Revolver Aim Setup Pose must default to false.");

            AppendIfMissing(
                failures,
                manager is CCS_IRevolverAimSetupPoseDebugSource,
                "Diagnostics manager must implement CCS_IRevolverAimSetupPoseDebugSource.");

            string source = File.Exists(DiagnosticsManagerSourcePath)
                ? File.ReadAllText(DiagnosticsManagerSourcePath)
                : string.Empty;
            AppendIfMissing(
                failures,
                source.Contains("ForceRevolverAimSetupPose"),
                "Diagnostics manager must expose ForceRevolverAimSetupPose read-only property.");
            AppendIfMissing(
                failures,
                source.Contains("SetDiagnosticsRevolverAimSetupPoseActive"),
                "Diagnostics manager must drive visual-only revolver setup pose on CCS_PlayerEquipmentVisualController.");
        }

        private static void ValidateProductionPlayerPrototypeVisualsRemoved(List<string> failures)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath);
            AppendIfMissing(failures, prefab != null, "Missing networked player prefab for prototype visual validation.");
            if (prefab == null)
            {
                return;
            }

            Transform[] transforms = prefab.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < CCS_CharacterControllerConstants.ProductionPlayerForbiddenPrototypeVisualObjectNames.Length; i++)
            {
                string forbiddenName = CCS_CharacterControllerConstants.ProductionPlayerForbiddenPrototypeVisualObjectNames[i];
                for (int j = 0; j < transforms.Length; j++)
                {
                    Transform candidate = transforms[j];
                    if (candidate != null && candidate.name == forbiddenName)
                    {
                        failures.Add(
                            "Production player prefab must not contain prototype visual object "
                            + forbiddenName
                            + ".");
                        break;
                    }
                }
            }
        }

        private static void ValidateEquipmentVisualSetupPosePath(List<string> failures)
        {
            AppendIfMissing(
                failures,
                File.Exists(EquipmentVisualSourcePath),
                "Missing CCS_PlayerEquipmentVisualController at " + EquipmentVisualSourcePath + ".");

            if (!File.Exists(EquipmentVisualSourcePath))
            {
                return;
            }

            string source = File.ReadAllText(EquipmentVisualSourcePath);
            AppendIfMissing(
                failures,
                source.Contains("SetDiagnosticsRevolverAimSetupPoseActive")
                    && source.Contains("diagnosticsRevolverAimSetupPoseActive")
                    && source.Contains("ShowEquippedVisual"),
                "CCS_PlayerEquipmentVisualController must support diagnostics right-hand visual-only revolver preview.");

            AppendIfMissing(
                failures,
                !source.Contains("ApplyWeaponDamage")
                    && !source.Contains("GrantRevolver")
                    && !source.Contains("playerWeaponLoadout.Grant"),
                "Diagnostics revolver setup pose visual path must not alter gameplay ownership, ammo, or damage.");
        }

        private static void ValidateAimAnimatorDebugOverride(List<string> failures)
        {
            AppendIfMissing(
                failures,
                File.Exists(AimAnimatorSourcePath),
                "Missing CCS_SingleRevolverAimAnimator at " + AimAnimatorSourcePath + ".");

            AppendIfMissing(
                failures,
                File.Exists(AimDebugSourceInterfacePath),
                "Missing CCS_IRevolverAimSetupPoseDebugSource interface.");

            if (!File.Exists(AimAnimatorSourcePath))
            {
                return;
            }

            string source = File.ReadAllText(AimAnimatorSourcePath);
            AppendIfMissing(
                failures,
                source.Contains("CCS_IRevolverAnimationState")
                    && source.Contains("IsAiming"),
                "CCS_SingleRevolverAimAnimator must read normal gameplay aim state.");

            AppendIfMissing(
                failures,
                source.Contains("CCS_IRevolverAimSetupPoseDebugSource")
                    && source.Contains("ForceRevolverAimSetupPose"),
                "CCS_SingleRevolverAimAnimator must honor diagnostics ForceRevolverAimSetupPose override.");

            AppendIfMissing(
                failures,
                source.Contains("CCS_RevolverAimSetupPoseDebugRegistry"),
                "CCS_SingleRevolverAimAnimator must resolve diagnostics override through CCS_RevolverAimSetupPoseDebugRegistry.");

            AppendIfMissing(
                failures,
                !source.Contains("ApplyWeaponDamage")
                    && !source.Contains("CCS_RevolverController")
                    && !source.Contains(".SetAiming("),
                "CCS_SingleRevolverAimAnimator must remain presentation-only.");
        }

        private static void ValidateSingleRevolverAimLayerPreserved(List<string> failures)
        {
            CCS_SurvivalValidationResult aimLayerResult =
                CCS_SingleRevolverAimLayerValidationUtility.ValidateSingleRevolverAimLayer();
            if (!aimLayerResult.IsSuccess)
            {
                failures.Add(aimLayerResult.Message);
            }
        }

        private static void ValidateMissingScripts(List<string> failures)
        {
            CCS_SurvivalValidationResult missingScriptResult =
                CCS_MissingScriptScanUtility.ValidateProductionAssetsHaveNoMissingScripts();
            if (!missingScriptResult.IsSuccess)
            {
                failures.Add(missingScriptResult.Message);
            }
        }

        private static void ValidateTestsFolderRemoved(List<string> failures)
        {
            AppendIfPresent(
                failures,
                Directory.Exists(CharacterControllerTestsRoot),
                "CharacterController/Tests must not return.");
        }

        private static void ValidateAnimationFitStudioNotPresent(List<string> failures)
        {
            AppendIfPresent(
                failures,
                Directory.Exists(AnimationFitStudioRoot),
                "Animation Fit Studio must remain absent.");
        }

        private static void ValidateEquipmentFitStudioPresent(List<string> failures)
        {
            AppendIfMissing(
                failures,
                File.Exists(EquipmentFitStudioWindowPath),
                "Equipment Fit Studio must remain present.");
        }

        private static void CollectDeferredWarnings(List<string> warnings)
        {
            foreach (string docRoot in HistoricalDocScanRoots)
            {
                if (File.Exists(docRoot))
                {
                    string contents = File.ReadAllText(docRoot);
                    if (contents.Contains("TestDetectionCube") || contents.Contains("CCS_TestDetectionCube"))
                    {
                        warnings.Add("Historical documentation still mentions detection cube in " + docRoot + ".");
                    }
                }
            }

            warnings.Add("Force Revolver Aim Setup Pose is available on validation scene diagnostics only.");
        }

        private static GameObject FindSceneObjectByName(Scene scene, string objectName)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                Transform[] transforms = roots[i].GetComponentsInChildren<Transform>(true);
                for (int j = 0; j < transforms.Length; j++)
                {
                    Transform candidate = transforms[j];
                    if (candidate != null
                        && candidate.name == objectName
                        && candidate.gameObject.scene == scene)
                    {
                        return candidate.gameObject;
                    }
                }
            }

            return null;
        }

        private static void AppendIfMissing(List<string> target, bool condition, string message)
        {
            if (!condition)
            {
                target.Add(message);
            }
        }

        private static void AppendIfPresent(List<string> target, bool condition, string message)
        {
            if (condition)
            {
                target.Add(message);
            }
        }
    }
}
