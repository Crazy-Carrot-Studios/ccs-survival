using System.Collections.Generic;
using System.IO;
using CCS.Modules.CharacterController;
using CCS.Modules.CharacterController.Diagnostics;
using CCS.Modules.CharacterController.Local;
using CCS.Modules.Weapons;
using CCS.Project;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_RevolverHandSocketPreviewValidationUtility
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Validates v0.7.10 revolver hand socket preview diagnostics toggle.
// PLACEMENT: Editor validation utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_RevolverHandSocketPreviewValidationUtility
    {
        private const string EquipmentFitStudioWindowPath =
            "Assets/CCS/Modules/CharacterController/Editor/EquipmentFitStudio/CCS_EquipmentFitStudioWindow.cs";

        private const string CharacterControllerTestsRoot =
            "Assets/CCS/Modules/CharacterController/Tests";

        private const string AnimationFitStudioRoot =
            "Assets/CCS/Modules/CharacterController/Editor/AnimationFitStudio";

        private const string DiagnosticsManagerSourcePath =
            "Assets/CCS/Modules/CharacterController/Runtime/Diagnostics/CCS_CharacterControllerDiagnosticsManager.cs";

        private const string RevolverVisualOnlyPrefabPath =
            "Assets/CCS/Modules/Weapons/Content/RevolverM1879/Prefabs/PF_CCS_RevolverM1879_VisualOnly.prefab";

        private const string RightHandEquippedFitProfilePath =
            "Assets/CCS/Modules/CharacterController/Profiles/EquipmentFitting/RevolverM1879/CCS_RevolverM1879_RightHandEquipped_Fit.asset";

        private const string EquipmentVisualSourcePath =
            "Assets/CCS/Modules/Weapons/Runtime/Components/CCS_PlayerEquipmentVisualController.cs";

        private const string AimAnimatorSourcePath =
            "Assets/CCS/Modules/CharacterController/Runtime/Animation/CCS_SingleRevolverAimAnimator.cs";

        private const string HandSocketPreviewInterfacePath =
            "Assets/CCS/Modules/CharacterController/Runtime/Data/CCS_IRevolverHandSocketPreviewDebugSource.cs";

        public static CCS_SurvivalValidationResult ValidateRevolverHandSocketPreview()
        {
            List<string> failures = new List<string>();
            List<string> warnings = new List<string>();

            ValidateDiagnosticsManager(failures);
            ValidateEquipmentVisualHandSocketPreview(failures);
            ValidateAimAnimatorDoesNotUseHandSocketPreview(failures);
            ValidatePlayerPrefabPrototypeVisualsRemoved(failures);
            ValidateRightHandSocketOnPlayerPrefab(failures);
            ValidateRightHandSocketIsEquipmentAnchor(failures);
            ValidateRevolverVisualAndFitProfileAssets(failures);
            ValidateMissingScripts(failures);
            ValidateTestsFolderRemoved(failures);
            ValidateAnimationFitStudioNotPresent(failures);
            ValidateEquipmentFitStudioPresent(failures);
            ValidateNoTestDetectionCube(failures);
            CollectDeferredWarnings(warnings);

            if (failures.Count > 0)
            {
                return CCS_SurvivalValidationResult.Fail(string.Join(" ", failures));
            }

            string message = "Revolver hand socket preview validated.";
            if (warnings.Count > 0)
            {
                message += " Warnings: " + string.Join(" ", warnings);
            }

            return CCS_SurvivalValidationResult.Pass(message);
        }

        private static void ValidateDiagnosticsManager(List<string> failures)
        {
            Scene scene = EditorSceneManager.OpenScene(
                CCS_CharacterControllerMasterTestLayoutConstants.MasterTestScenePath,
                OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                failures.Add("Could not open validation scene for hand socket preview validation.");
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
                "Validation scene must contain exactly one CCS_CharacterControllerDiagnosticsManager.");

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

            if (manager == null)
            {
                return;
            }

            SerializedObject serializedManager = new SerializedObject(manager);
            SerializedProperty handSocketPreviewProperty =
                serializedManager.FindProperty("forceRevolverHandSocketPreview");
            SerializedProperty aimSetupPoseProperty =
                serializedManager.FindProperty("forceRevolverAimSetupPose");

            AppendIfMissing(
                failures,
                handSocketPreviewProperty != null,
                "Diagnostics manager must expose Force Revolver Hand Socket Preview bool (forceRevolverHandSocketPreview).");

            AppendIfMissing(
                failures,
                handSocketPreviewProperty == null || !handSocketPreviewProperty.boolValue,
                "Force Revolver Hand Socket Preview must default to false.");

            AppendIfMissing(
                failures,
                aimSetupPoseProperty == null || !aimSetupPoseProperty.boolValue,
                "Force Revolver Aim Setup Pose must default to false.");

            AppendIfMissing(
                failures,
                manager is CCS_IRevolverHandSocketPreviewDebugSource,
                "Diagnostics manager must implement CCS_IRevolverHandSocketPreviewDebugSource.");

            string source = File.Exists(DiagnosticsManagerSourcePath)
                ? File.ReadAllText(DiagnosticsManagerSourcePath)
                : string.Empty;
            AppendIfMissing(
                failures,
                source.Contains("ForceRevolverHandSocketPreview"),
                "Diagnostics manager must expose ForceRevolverHandSocketPreview read-only property.");
            AppendIfMissing(
                failures,
                source.Contains("SetDiagnosticsRevolverHandSocketPreviewActive"),
                "Diagnostics manager must drive hand socket preview on CCS_PlayerEquipmentVisualController.");
            AppendIfMissing(
                failures,
                source.Contains("FindFirstObjectByType<CCS_PlayerEquipmentVisualController>"),
                "Diagnostics manager must resolve player equipment visual controller directly (not AI revolver controller).");
        }

        private static void ValidateEquipmentVisualHandSocketPreview(List<string> failures)
        {
            AppendIfMissing(
                failures,
                File.Exists(EquipmentVisualSourcePath),
                "Missing CCS_PlayerEquipmentVisualController.");

            AppendIfMissing(
                failures,
                File.Exists(HandSocketPreviewInterfacePath),
                "Missing CCS_IRevolverHandSocketPreviewDebugSource interface.");

            if (!File.Exists(EquipmentVisualSourcePath))
            {
                return;
            }

            string source = File.ReadAllText(EquipmentVisualSourcePath);
            AppendIfMissing(
                failures,
                source.Contains("SetDiagnosticsRevolverHandSocketPreviewActive")
                    && source.Contains("diagnosticsRevolverHandSocketPreviewActive")
                    && source.Contains("ShouldShowDiagnosticsEquippedVisualPreview")
                    && source.Contains("ShowDiagnosticsEquippedPreview"),
                "CCS_PlayerEquipmentVisualController must support diagnostics hand socket preview.");

            AppendIfMissing(
                failures,
                source.Contains("diagnosticsEquippedVisualInstance")
                    && source.Contains("RightHandRevolverAttachmentOffsetObjectName")
                    && source.Contains(CCS_EquipmentConstants.HandSocketRightId),
                "Diagnostics preview must create a dedicated visual on CCS_HandSocket_Right via CCS_RightHandRevolverAttachmentOffset.");

            AppendIfMissing(
                failures,
                source.Contains("IsIkOnlyAttachmentTransform")
                    && source.Contains("RightHandIkTargetObjectName"),
                "Diagnostics preview must reject IK-only attachment parents.");

            AppendIfMissing(
                failures,
                source.Contains("diagnosticsRevolverAimSetupPoseActive")
                    && source.Contains("diagnosticsRevolverHandSocketPreviewActive")
                    && source.Contains("diagnosticsEquippedVisualInstance"),
                "Equipment visual controller must share one diagnostics equipped visual for debug previews.");

            AppendIfMissing(
                failures,
                !source.Contains("ApplyWeaponDamage")
                    && !source.Contains("GrantRevolver")
                    && !source.Contains("playerWeaponLoadout.Grant"),
                "Hand socket preview path must not alter gameplay ownership, ammo, damage, or fire.");
        }

        private static void ValidateAimAnimatorDoesNotUseHandSocketPreview(List<string> failures)
        {
            if (!File.Exists(AimAnimatorSourcePath))
            {
                failures.Add("Missing CCS_SingleRevolverAimAnimator.");
                return;
            }

            string source = File.ReadAllText(AimAnimatorSourcePath);
            AppendIfMissing(
                failures,
                !source.Contains("ForceRevolverHandSocketPreview")
                    && !source.Contains("HandSocketPreview"),
                "CCS_SingleRevolverAimAnimator must not respond to Force Revolver Hand Socket Preview.");

            AppendIfMissing(
                failures,
                source.Contains("ForceRevolverAimSetupPose"),
                "CCS_SingleRevolverAimAnimator must continue honoring Force Revolver Aim Setup Pose.");
        }

        private static void ValidatePlayerPrefabPrototypeVisualsRemoved(List<string> failures)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath);
            if (prefab == null)
            {
                failures.Add("Missing networked player prefab.");
                return;
            }

            for (int i = 0; i < CCS_CharacterControllerConstants.ProductionPlayerForbiddenPrototypeVisualObjectNames.Length; i++)
            {
                string forbiddenName = CCS_CharacterControllerConstants.ProductionPlayerForbiddenPrototypeVisualObjectNames[i];
                Transform[] transforms = prefab.GetComponentsInChildren<Transform>(true);
                for (int j = 0; j < transforms.Length; j++)
                {
                    if (transforms[j] != null && transforms[j].name == forbiddenName)
                    {
                        failures.Add("Production player prefab must not contain " + forbiddenName + ".");
                        break;
                    }
                }
            }
        }

        private static void ValidateRightHandSocketOnPlayerPrefab(List<string> failures)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath);
            if (prefab == null)
            {
                return;
            }

            CCS_EquipmentSocketRegistry registry = prefab.GetComponentInChildren<CCS_EquipmentSocketRegistry>(true);
            AppendIfMissing(
                failures,
                registry != null,
                "Player prefab must contain CCS_EquipmentSocketRegistry for right-hand socket preview.");

            if (registry == null)
            {
                return;
            }

            AppendIfMissing(
                failures,
                registry.TryGetSocket(CCS_EquipmentConstants.HandSocketRightId, out _),
                "Player prefab must expose right-hand equipment socket for hand socket preview.");
        }

        private static void ValidateRightHandSocketIsEquipmentAnchor(List<string> failures)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath);
            if (prefab == null)
            {
                return;
            }

            CCS_EquipmentSocketAnchor[] anchors = prefab.GetComponentsInChildren<CCS_EquipmentSocketAnchor>(true);
            bool foundRightHandSocket = false;
            for (int i = 0; i < anchors.Length; i++)
            {
                CCS_EquipmentSocketAnchor anchor = anchors[i];
                if (anchor == null || anchor.SocketId != CCS_EquipmentConstants.HandSocketRightId)
                {
                    continue;
                }

                foundRightHandSocket = true;
                AppendIfMissing(
                    failures,
                    anchor.name == "CCS_HandSocket_Right",
                    "Right-hand equipment socket anchor must be named CCS_HandSocket_Right.");
                AppendIfMissing(
                    failures,
                    anchor.name != CCS_EquipmentConstants.RightHandIkTargetObjectName,
                    "Right-hand equipment socket must not use the IK target object name.");
            }

            AppendIfMissing(
                failures,
                foundRightHandSocket,
                "Player prefab must contain a CCS_EquipmentSocketAnchor for CCS_HandSocket_Right.");
        }

        private static void ValidateRevolverVisualAndFitProfileAssets(List<string> failures)
        {
            AppendIfMissing(
                failures,
                File.Exists(RevolverVisualOnlyPrefabPath),
                "Missing revolver visual-only prefab for diagnostics preview.");

            AppendIfMissing(
                failures,
                File.Exists(RightHandEquippedFitProfilePath),
                "Missing CCS_RevolverM1879_RightHandEquipped_Fit profile for right-hand preview.");

            if (!File.Exists(EquipmentVisualSourcePath))
            {
                return;
            }

            string source = File.ReadAllText(EquipmentVisualSourcePath);
            AppendIfMissing(
                failures,
                source.Contains("revolverVisualOnlyPrefab")
                    && source.Contains("EnsureDiagnosticsVisualInstance"),
                "Diagnostics preview must instantiate a visible revolver when no gameplay weapon is owned.");
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

        private static void ValidateNoTestDetectionCube(List<string> failures)
        {
            Scene scene = EditorSceneManager.OpenScene(
                CCS_CharacterControllerMasterTestLayoutConstants.MasterTestScenePath,
                OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                return;
            }

            AppendIfMissing(
                failures,
                FindSceneObjectByName(scene, "CCS_TestDetectionCube") == null,
                "Validation scene must not contain CCS_TestDetectionCube.");
        }

        private static void CollectDeferredWarnings(List<string> warnings)
        {
            warnings.Add("Force Revolver Hand Socket Preview is validation-scene only.");
            warnings.Add("Socket offset may still require manual tuning.");
            warnings.Add("Socket pose may require future animation/IK refinement.");
            warnings.Add("Setup pose and socket preview share one diagnostics visual instance.");
            warnings.Add("IK target gizmo labels require Enable Visual Debug Helpers while playing.");
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
