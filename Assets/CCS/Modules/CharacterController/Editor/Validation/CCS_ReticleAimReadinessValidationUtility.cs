using System.Collections.Generic;
using System.IO;
using CCS.Modules.CharacterController;
using CCS.Modules.CharacterController.Local;
using CCS.Modules.Weapons;
using CCS.Project;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;

// =============================================================================
// SCRIPT: CCS_ReticleAimReadinessValidationUtility
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Validates v0.7.10d reticle aim presentation readiness gating.
// PLACEMENT: Editor validation utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-30
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_ReticleAimReadinessValidationUtility
    {
        private const string ReadinessInterfacePath =
            "Assets/CCS/Modules/CharacterController/Runtime/Animation/CCS_IRevolverAimPresentationReadinessSource.cs";

        private const string AimAnimatorSourcePath =
            "Assets/CCS/Modules/CharacterController/Runtime/Animation/CCS_SingleRevolverAimAnimator.cs";

        private const string MuzzleReticleSourcePath =
            "Assets/CCS/Modules/Weapons/Runtime/Aiming/CCS_MuzzleDrivenReticleController.cs";

        private const string BarrelLineOfSightPlanPath =
            "Assets/CCS/Modules/CharacterController/Documentation/CCS_Revolver_Reticle_Barrel_LineOfSight_Plan.md";

        private const string EquipmentFitStudioWindowPath =
            "Assets/CCS/Modules/CharacterController/Editor/EquipmentFitStudio/CCS_EquipmentFitStudioWindow.cs";

        private const string CharacterControllerTestsRoot =
            "Assets/CCS/Modules/CharacterController/Tests";

        private const string AnimationFitStudioRoot =
            "Assets/CCS/Modules/CharacterController/Editor/AnimationFitStudio";

        private static readonly Vector3 ExpectedFitPosition = new Vector3(0.099f, 0.176f, 0.014f);

        private static readonly Vector3 ExpectedFitEuler = new Vector3(-48.275f, 103.374f, 64.828f);

        private static readonly Vector3 ExpectedFitScale = Vector3.one;

        public static CCS_SurvivalValidationResult ValidateReticleAimReadiness()
        {
            List<string> failures = new List<string>();
            List<string> warnings = new List<string>();

            ValidateRequiredAssets(failures);
            ValidateReadinessContract(failures);
            ValidateAimAnimatorReadiness(failures);
            ValidateReticleControllerGating(failures);
            ValidatePlayerPrefabWiring(failures);
            ValidateAnimatorLayerAndHoldState(failures);
            ValidateFitProfileUnchanged(failures);
            ValidateFitValuesNotHardcodedInRuntime(failures);
            ValidateMissingScripts(failures);
            ValidateTestsFolderRemoved(failures);
            ValidateAnimationFitStudioNotPresent(failures);
            ValidateEquipmentFitStudioPresent(failures);
            CollectDeferredWarnings(warnings);

            if (failures.Count > 0)
            {
                return CCS_SurvivalValidationResult.Fail(string.Join(" ", failures));
            }

            string message = "Reticle aim presentation readiness gate validated.";
            if (warnings.Count > 0)
            {
                message += " Warnings: " + string.Join(" ", warnings);
            }

            return CCS_SurvivalValidationResult.Pass(message);
        }

        private static void ValidateRequiredAssets(List<string> failures)
        {
            AppendIfMissing(failures, File.Exists(ReadinessInterfacePath), "Missing readiness interface.");
            AppendIfMissing(failures, File.Exists(AimAnimatorSourcePath), "Missing CCS_SingleRevolverAimAnimator.");
            AppendIfMissing(failures, File.Exists(MuzzleReticleSourcePath), "Missing CCS_MuzzleDrivenReticleController.");
            AppendIfMissing(
                failures,
                File.Exists(BarrelLineOfSightPlanPath),
                "Missing barrel line-of-sight planning doc at " + BarrelLineOfSightPlanPath + ".");
        }

        private static void ValidateReadinessContract(List<string> failures)
        {
            if (!File.Exists(ReadinessInterfacePath))
            {
                return;
            }

            string source = File.ReadAllText(ReadinessInterfacePath);
            AppendIfMissing(
                failures,
                source.Contains("bool IsAimPresentationActive { get; }"),
                "Readiness interface must expose IsAimPresentationActive.");
            AppendIfMissing(
                failures,
                source.Contains("bool IsAimPresentationReadyForReticle { get; }"),
                "Readiness interface must expose IsAimPresentationReadyForReticle.");
            AppendIfMissing(
                failures,
                source.Contains("bool IsAimPresentationInReticleRevealWindow { get; }"),
                "Readiness interface must expose IsAimPresentationInReticleRevealWindow.");
        }

        private static void ValidateAimAnimatorReadiness(List<string> failures)
        {
            if (!File.Exists(AimAnimatorSourcePath))
            {
                return;
            }

            string source = File.ReadAllText(AimAnimatorSourcePath);
            AppendIfMissing(
                failures,
                source.Contains("CCS_IRevolverAimPresentationReadinessSource"),
                "CCS_SingleRevolverAimAnimator must implement CCS_IRevolverAimPresentationReadinessSource.");
            AppendIfMissing(
                failures,
                source.Contains("IsAimPresentationActive"),
                "CCS_SingleRevolverAimAnimator must expose IsAimPresentationActive.");
            AppendIfMissing(
                failures,
                source.Contains("IsAimPresentationReadyForReticle"),
                "CCS_SingleRevolverAimAnimator must expose IsAimPresentationReadyForReticle.");
            AppendIfMissing(
                failures,
                source.Contains(CCS_CharacterControllerConstants.SingleRevolverAimHoldStateName)
                    || source.Contains("SingleRevolverAimHoldStateName"),
                "Aim animator readiness must reference Revolver_Aim_Hold state.");
            AppendIfMissing(
                failures,
                source.Contains(CCS_CharacterControllerConstants.SingleRevolverDrawStateName)
                    || source.Contains("SingleRevolverDrawStateName"),
                "Aim animator readiness must suppress reticle during Revolver_Draw.");
            AppendIfMissing(
                failures,
                source.Contains(CCS_CharacterControllerConstants.SingleRevolverHolsterStateName)
                    || source.Contains("SingleRevolverHolsterStateName"),
                "Aim animator readiness must suppress reticle during Revolver_Holster.");
        }

        private static void ValidateReticleControllerGating(List<string> failures)
        {
            if (!File.Exists(MuzzleReticleSourcePath))
            {
                return;
            }

            string source = File.ReadAllText(MuzzleReticleSourcePath);
            AppendIfMissing(
                failures,
                source.Contains("aimPresentationReadinessSourceComponent"),
                "CCS_MuzzleDrivenReticleController must serialize an aim presentation readiness source.");
            AppendIfMissing(
                failures,
                source.Contains("IsAimPresentationInReticleRevealWindow"),
                "Readiness interface must expose IsAimPresentationInReticleRevealWindow.");
            AppendIfMissing(
                failures,
                source.Contains("IsReticlePresentationVisible") || source.Contains("IsAimPresentationInReticleRevealWindow"),
                "Reticle controller must gate on reveal window or hold readiness.");
            AppendIfMissing(
                failures,
                source.Contains("EnsureReticleHiddenAtStartup"),
                "Reticle controller must hide reticle at startup.");
            AppendIfMissing(
                failures,
                source.Contains("IsHandSocketPreviewActive"),
                "Reticle controller must block reticle during Force Revolver Hand Socket Preview.");
            AppendIfMissing(
                failures,
                source.Contains("ForceRevolverHandSocketPreview"),
                "Reticle controller must honor ForceRevolverHandSocketPreview debug source.");
            AppendIfMissing(
                failures,
                source.Contains("IsLocalPresentationOwner"),
                "Reticle controller must gate on local owner presentation context.");
            AppendIfMissing(
                failures,
                source.Contains("IsDebugAimSetupPoseActive"),
                "Reticle controller must allow setup pose only after readiness is true.");
            AppendIfMissing(
                failures,
                !source.Contains("ApplyReticleScreenPosition(centerScreen, visible: true);"),
                "Reticle controller must not show reticle immediately on aim start before readiness.");
        }

        private static void ValidatePlayerPrefabWiring(List<string> failures)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath);
            AppendIfMissing(failures, prefab != null, "Missing networked player prefab.");

            if (prefab == null)
            {
                return;
            }

            CCS_MuzzleDrivenReticleController reticleController =
                prefab.GetComponentInChildren<CCS_MuzzleDrivenReticleController>(true);
            AppendIfMissing(
                failures,
                reticleController != null,
                "Player prefab missing CCS_MuzzleDrivenReticleController.");

            CCS_SingleRevolverAimAnimator aimAnimator = prefab.GetComponentInChildren<CCS_SingleRevolverAimAnimator>(true);
            AppendIfMissing(
                failures,
                aimAnimator != null,
                "Player prefab missing CCS_SingleRevolverAimAnimator for readiness source.");

            if (reticleController == null)
            {
                return;
            }

            SerializedObject serializedReticle = new SerializedObject(reticleController);
            SerializedProperty readinessProperty = serializedReticle.FindProperty("aimPresentationReadinessSourceComponent");
            AppendIfMissing(
                failures,
                readinessProperty != null && readinessProperty.objectReferenceValue != null,
                "CCS_MuzzleDrivenReticleController must reference an aim presentation readiness source on the player prefab.");

            if (readinessProperty != null
                && readinessProperty.objectReferenceValue != null
                && aimAnimator != null
                && readinessProperty.objectReferenceValue != aimAnimator)
            {
                failures.Add("Reticle readiness source must reference CCS_SingleRevolverAimAnimator.");
            }

            Image[] images = prefab.GetComponentsInChildren<Image>(true);
            Image reticleImage = null;
            for (int i = 0; i < images.Length; i++)
            {
                if (images[i] != null && images[i].name == CCS_WeaponsConstants.WeaponReticleObjectName)
                {
                    reticleImage = images[i];
                    break;
                }
            }

            if (reticleImage != null)
            {
                AppendIfMissing(
                    failures,
                    !reticleImage.enabled,
                    "Weapon reticle Image must be disabled by default on the player prefab.");
            }
        }

        private static void ValidateAnimatorLayerAndHoldState(List<string> failures)
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(
                CCS_CharacterControllerConstants.PlayerLocomotionAnimatorControllerPath);
            AppendIfMissing(failures, controller != null, "Missing player Animator Controller.");

            if (controller == null)
            {
                return;
            }

            int layerIndex = FindLayerIndex(controller, CCS_CharacterControllerConstants.SingleRevolverUpperBodyLayerName);
            AppendIfMissing(
                failures,
                layerIndex >= 0,
                "SingleRevolverUpperBody layer missing from player Animator Controller.");

            if (layerIndex < 0)
            {
                return;
            }

            AnimatorControllerLayer layer = controller.layers[layerIndex];
            AnimatorStateMachine stateMachine = layer.stateMachine;
            AnimatorState holdState = FindState(stateMachine, CCS_CharacterControllerConstants.SingleRevolverAimHoldStateName);
            AppendIfMissing(
                failures,
                holdState != null,
                "Revolver_Aim_Hold state missing from SingleRevolverUpperBody layer.");

            if (holdState != null && holdState.motion is AnimationClip holdClip)
            {
                AppendIfMissing(
                    failures,
                    holdClip.name == CCS_CharacterControllerConstants.WildWestFulldrawIdleClipName,
                    "Revolver_Aim_Hold must use Fulldraw_Idle clip.");
            }
            else
            {
                failures.Add("Revolver_Aim_Hold must reference Fulldraw_Idle clip.");
            }
        }

        private static void ValidateFitProfileUnchanged(List<string> failures)
        {
            CCS_WeaponAttachmentFitProfile profile = AssetDatabase.LoadAssetAtPath<CCS_WeaponAttachmentFitProfile>(
                CCS_EquipmentConstants.RevolverM1879RightHandEquippedFitPath);
            AppendIfMissing(failures, profile != null, "Missing right-hand equipped fit profile.");

            if (profile == null)
            {
                return;
            }

            AppendIfMissing(
                failures,
                VectorApproximately(profile.SocketLocalPosition, ExpectedFitPosition),
                "Right-hand fit profile position changed; v0.7.10d must keep v0.7.10c fit values.");
            AppendIfMissing(
                failures,
                VectorApproximately(profile.SocketLocalEulerAngles, ExpectedFitEuler),
                "Right-hand fit profile rotation changed; v0.7.10d must keep v0.7.10c fit values.");
            AppendIfMissing(
                failures,
                VectorApproximately(profile.SocketLocalScale, ExpectedFitScale),
                "Right-hand fit profile scale changed; v0.7.10d must keep v0.7.10c fit values.");
        }

        private static void ValidateFitValuesNotHardcodedInRuntime(List<string> failures)
        {
            string[] runtimeRoots =
            {
                "Assets/CCS/Modules/CharacterController/Runtime",
                "Assets/CCS/Modules/Weapons/Runtime",
            };

            for (int rootIndex = 0; rootIndex < runtimeRoots.Length; rootIndex++)
            {
                string[] files = Directory.GetFiles(runtimeRoots[rootIndex], "*.cs", SearchOption.AllDirectories);
                for (int fileIndex = 0; fileIndex < files.Length; fileIndex++)
                {
                    string source = File.ReadAllText(files[fileIndex]);
                    if (source.Contains("0.099") && source.Contains("0.176") && source.Contains("0.014"))
                    {
                        failures.Add("Right-hand fit offset values must not be hardcoded in runtime script " + files[fileIndex] + ".");
                    }
                }
            }
        }

        private static void ValidateMissingScripts(List<string> failures)
        {
            AppendValidationFailures(
                failures,
                CCS_MissingScriptScanUtility.ValidateProductionAssetsHaveNoMissingScripts());
        }

        private static void ValidateTestsFolderRemoved(List<string> failures)
        {
            AppendIfMissing(
                failures,
                !Directory.Exists(CharacterControllerTestsRoot),
                "CharacterController/Tests folder must remain removed.");
        }

        private static void ValidateAnimationFitStudioNotPresent(List<string> failures)
        {
            AppendIfMissing(
                failures,
                !Directory.Exists(AnimationFitStudioRoot),
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
            warnings.Add("Barrel line-of-sight reticle behavior is planned but not implemented in v0.7.10d.");
            warnings.Add("Reticle still uses current camera-center / hybrid muzzle drift presentation.");
            warnings.Add("Animation event approach for cosmetic readiness remains deferred.");
        }

        private static bool VectorApproximately(Vector3 left, Vector3 right)
        {
            return Mathf.Approximately(left.x, right.x)
                && Mathf.Approximately(left.y, right.y)
                && Mathf.Approximately(left.z, right.z);
        }

        private static int FindLayerIndex(AnimatorController controller, string layerName)
        {
            for (int i = 0; i < controller.layers.Length; i++)
            {
                if (controller.layers[i].name == layerName)
                {
                    return i;
                }
            }

            return -1;
        }

        private static AnimatorState FindState(AnimatorStateMachine stateMachine, string stateName)
        {
            ChildAnimatorState[] states = stateMachine.states;
            for (int i = 0; i < states.Length; i++)
            {
                if (states[i].state != null && states[i].state.name == stateName)
                {
                    return states[i].state;
                }
            }

            return null;
        }

        private static void AppendValidationFailures(List<string> failures, CCS_SurvivalValidationResult result)
        {
            if (!result.IsSuccess)
            {
                failures.Add(result.Message);
            }
        }

        private static void AppendIfMissing(List<string> target, bool condition, string message)
        {
            if (!condition)
            {
                target.Add(message);
            }
        }
    }
}
