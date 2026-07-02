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
// SCRIPT: CCS_ReticleRevealAnimationEventValidationUtility
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Validates v0.7.10f reticle reveal Animation Event wiring.
// PLACEMENT: Editor validation utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_ReticleRevealAnimationEventValidationUtility
    {
        private const string AimAnimatorSourcePath =
            "Assets/CCS/Modules/CharacterController/Runtime/Animation/CCS_SingleRevolverAimAnimator.cs";

        private const string ReceiverSourcePath =
            "Assets/CCS/Modules/CharacterController/Runtime/Animation/CCS_RevolverReticleAnimationEventReceiver.cs";

        private const string MuzzleReticleSourcePath =
            "Assets/CCS/Modules/Weapons/Runtime/Aiming/CCS_MuzzleDrivenReticleController.cs";

        private const string ProfileClassPath =
            "Assets/CCS/Modules/CharacterController/Runtime/Visuals/CCS_RevolverReticlePresentationProfile.cs";

        private const string EquipmentFitStudioWindowPath =
            "Assets/CCS/Modules/CharacterController/Editor/EquipmentFitStudio/CCS_EquipmentFitStudioWindow.cs";

        private const string CharacterControllerTestsRoot =
            "Assets/CCS/Modules/CharacterController/Tests";

        private const string AnimationFitStudioRoot =
            "Assets/CCS/Modules/CharacterController/Editor/AnimationFitStudio";

        private static readonly Vector3 ExpectedFitPosition = new Vector3(0.099f, 0.176f, 0.014f);

        private static readonly Vector3 ExpectedFitEuler = new Vector3(-48.275f, 103.374f, 64.828f);

        private static readonly Vector3 ExpectedFitScale = Vector3.one;

        public static CCS_SurvivalValidationResult ValidateReticleRevealAnimationEvent()
        {
            List<string> failures = new List<string>();
            List<string> warnings = new List<string>();

            ValidateRequiredAssets(failures);
            ValidateHoldClipAndEvent(failures, warnings);
            ValidateReceiverContract(failures);
            ValidateAimAnimatorEventReadiness(failures);
            ValidatePresentationProfile(failures);
            ValidateReticleControllerGating(failures);
            ValidatePlayerPrefabWiring(failures);
            ValidateAnimatorLayerAndHoldState(failures);
            ValidateFitProfileUnchanged(failures);
            ValidateMissingScripts(failures);
            ValidateTestsFolderRemoved(failures);
            ValidateAnimationFitStudioNotPresent(failures);
            ValidateEquipmentFitStudioPresent(failures);
            CollectDeferredWarnings(warnings);

            if (failures.Count > 0)
            {
                return CCS_SurvivalValidationResult.Fail(string.Join(" ", failures));
            }

            string message = "Reticle reveal animation event validated.";
            if (warnings.Count > 0)
            {
                message += " Warnings: " + string.Join(" ", warnings);
            }

            return CCS_SurvivalValidationResult.Pass(message);
        }

        private static void ValidateRequiredAssets(List<string> failures)
        {
            AppendIfMissing(failures, File.Exists(AimAnimatorSourcePath), "Missing CCS_SingleRevolverAimAnimator.");
            AppendIfMissing(failures, File.Exists(ReceiverSourcePath), "Missing CCS_RevolverReticleAnimationEventReceiver.");
            AppendIfMissing(failures, File.Exists(MuzzleReticleSourcePath), "Missing CCS_MuzzleDrivenReticleController.");
            AppendIfMissing(failures, File.Exists(ProfileClassPath), "Missing CCS_RevolverReticlePresentationProfile class.");
            AppendIfMissing(
                failures,
                File.Exists(CCS_CharacterControllerConstants.RevolverReticlePresentationProfilePath),
                "Missing CCS_RevolverReticlePresentationProfile asset.");
            AppendIfMissing(
                failures,
                File.Exists(CCS_CharacterControllerConstants.WildWestFulldrawIdleClipPath),
                "Missing Fulldraw_Idle clip at " + CCS_CharacterControllerConstants.WildWestFulldrawIdleClipPath + ".");
        }

        private static void ValidateHoldClipAndEvent(List<string> failures, List<string> warnings)
        {
            if (!CCS_RevolverFulldrawIdleReticleEventBuilder.TryReadFulldrawIdleReticleEventTime(
                    out float eventTime,
                    out int matchingEventCount))
            {
                failures.Add("Could not read Fulldraw_Idle importer clip events.");
                return;
            }

            AppendIfMissing(
                failures,
                matchingEventCount == 1,
                "Fulldraw_Idle must contain exactly one "
                + CCS_CharacterControllerConstants.RevolverAimHoldReticleRevealAnimationEventName
                + " event.");
            AppendIfMissing(
                failures,
                eventTime >= 0f && eventTime <= 0.05f,
                "Reticle reveal animation event must be at or near clip start.");

            if (Mathf.Abs(eventTime - CCS_CharacterControllerConstants.RevolverAimHoldReticleRevealAnimationEventPreferredTime) > 0.0001f
                && Mathf.Approximately(
                    eventTime,
                    CCS_CharacterControllerConstants.RevolverAimHoldReticleRevealAnimationEventFallbackTime))
            {
                warnings.Add("Reticle reveal animation event uses fallback time 0.01f instead of 0.0f.");
            }
        }

        private static void ValidateReceiverContract(List<string> failures)
        {
            if (!File.Exists(ReceiverSourcePath))
            {
                return;
            }

            string source = File.ReadAllText(ReceiverSourcePath);
            AppendIfMissing(
                failures,
                source.Contains("CCS_OnRevolverAimHoldStarted"),
                "Receiver must expose CCS_OnRevolverAimHoldStarted().");
            AppendIfMissing(
                failures,
                source.Contains("NotifyRevolverAimHoldAnimationEvent"),
                "Receiver must forward to NotifyRevolverAimHoldAnimationEvent().");
        }

        private static void ValidateAimAnimatorEventReadiness(List<string> failures)
        {
            if (!File.Exists(AimAnimatorSourcePath))
            {
                return;
            }

            string source = File.ReadAllText(AimAnimatorSourcePath);
            AppendIfMissing(
                failures,
                source.Contains("reticleRevealEventReceived"),
                "Aim animator must track reticleRevealEventReceived.");
            AppendIfMissing(
                failures,
                source.Contains("NotifyRevolverAimHoldAnimationEvent"),
                "Aim animator must expose NotifyRevolverAimHoldAnimationEvent().");
            AppendIfMissing(
                failures,
                source.Contains("CCS_RevolverReticleRevealSource"),
                "Aim animator must honor reticle reveal source profile setting.");
            AppendIfMissing(
                failures,
                source.Contains("reticleRevealEventReceived = false"),
                "Aim animator must reset reticle reveal event on draw/holster.");
        }

        private static void ValidatePresentationProfile(List<string> failures)
        {
            CCS_RevolverReticlePresentationProfile profile = AssetDatabase.LoadAssetAtPath<CCS_RevolverReticlePresentationProfile>(
                CCS_CharacterControllerConstants.RevolverReticlePresentationProfilePath);
            AppendIfMissing(failures, profile != null, "Could not load reticle presentation profile.");

            if (profile == null)
            {
                return;
            }

            AppendIfMissing(
                failures,
                profile.ReticleRevealSource == CCS_RevolverReticleRevealSource.AnimationEvent
                    || profile.ReticleRevealSource == CCS_RevolverReticleRevealSource.AnimationEventWithStateFallback,
                "Reticle profile must use Animation Event reveal mode.");
            AppendIfMissing(
                failures,
                !profile.RevealDuringDraw,
                "Draw normalized-time reveal must be disabled for v0.7.10f.");
            AppendIfMissing(
                failures,
                profile.MaxScreenSnapPixelsPerFrame > 0f,
                "maxScreenSnapPixelsPerFrame must be > 0.");
            AppendIfMissing(
                failures,
                profile.NoHitFallbackDistance > 0f,
                "noHitFallbackDistance must be > 0.");
            AppendIfMissing(
                failures,
                profile.ScreenSmoothTime > 0f,
                "screenSmoothTime must be retained from v0.7.10e.");
            AppendIfMissing(
                failures,
                profile.HoldLastValidTargetOnNoHit,
                "holdLastValidTargetOnNoHit must be retained from v0.7.10e.");
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
                source.Contains("IsAimPresentationReadyForReticle"),
                "Reticle controller must gate on IsAimPresentationReadyForReticle.");
            AppendIfMissing(
                failures,
                source.Contains("EnsureReticleHiddenAtStartup"),
                "Reticle controller must hide reticle at startup.");
            AppendIfMissing(
                failures,
                source.Contains("IsHandSocketPreviewActive"),
                "Reticle controller must block hand socket preview.");
            AppendIfMissing(
                failures,
                source.Contains("IsLocalPresentationOwner"),
                "Reticle controller must gate on local owner presentation context.");
            AppendIfMissing(
                failures,
                !source.Contains("IsAimPresentationInReticleRevealWindow"),
                "Reticle controller must not use draw reveal window for visibility in v0.7.10f.");
        }

        private static void ValidatePlayerPrefabWiring(List<string> failures)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath);
            AppendIfMissing(failures, prefab != null, "Missing networked player prefab.");

            if (prefab == null)
            {
                return;
            }

            CCS_SingleRevolverAimAnimator aimAnimator = prefab.GetComponentInChildren<CCS_SingleRevolverAimAnimator>(true);
            AppendIfMissing(failures, aimAnimator != null, "Player prefab missing CCS_SingleRevolverAimAnimator.");

            Animator animator = prefab.GetComponentInChildren<Animator>(true);
            AppendIfMissing(failures, animator != null, "Player prefab missing Animator.");

            if (animator == null)
            {
                return;
            }

            CCS_RevolverReticleAnimationEventReceiver receiver =
                animator.GetComponent<CCS_RevolverReticleAnimationEventReceiver>();
            AppendIfMissing(
                failures,
                receiver != null,
                "Reticle animation event receiver must be on the same GameObject as the Animator.");

            if (receiver != null && aimAnimator != null)
            {
                SerializedObject serializedReceiver = new SerializedObject(receiver);
                SerializedProperty aimAnimatorProperty = serializedReceiver.FindProperty("aimAnimator");
                AppendIfMissing(
                    failures,
                    aimAnimatorProperty != null && aimAnimatorProperty.objectReferenceValue == aimAnimator,
                    "Reticle animation event receiver must reference CCS_SingleRevolverAimAnimator.");
            }

            CCS_MuzzleDrivenReticleController reticleController =
                prefab.GetComponentInChildren<CCS_MuzzleDrivenReticleController>(true);
            AppendIfMissing(failures, reticleController != null, "Player prefab missing reticle controller.");

            if (reticleController != null && aimAnimator != null)
            {
                SerializedObject serializedReticle = new SerializedObject(reticleController);
                SerializedProperty readinessProperty = serializedReticle.FindProperty("aimPresentationReadinessSourceComponent");
                AppendIfMissing(
                    failures,
                    readinessProperty != null && readinessProperty.objectReferenceValue == aimAnimator,
                    "Reticle controller must reference CCS_SingleRevolverAimAnimator readiness source.");
            }

            Image[] images = prefab.GetComponentsInChildren<Image>(true);
            for (int i = 0; i < images.Length; i++)
            {
                if (images[i] != null && images[i].name == CCS_WeaponsConstants.WeaponReticleObjectName)
                {
                    AppendIfMissing(
                        failures,
                        !images[i].enabled,
                        "Weapon reticle Image must be disabled by default.");
                    break;
                }
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
            AnimatorState holdState = FindState(layer.stateMachine, CCS_CharacterControllerConstants.SingleRevolverAimHoldStateName);
            AppendIfMissing(failures, holdState != null, "Revolver_Aim_Hold state missing.");

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
            CCS_WeaponAttachmentFitProfile fitProfile = AssetDatabase.LoadAssetAtPath<CCS_WeaponAttachmentFitProfile>(
                CCS_EquipmentConstants.RevolverM1879RightHandEquippedFitPath);
            AppendIfMissing(failures, fitProfile != null, "Missing right-hand equipped fit profile.");

            if (fitProfile == null)
            {
                return;
            }

            AppendIfMissing(
                failures,
                VectorApproximately(fitProfile.SocketLocalPosition, ExpectedFitPosition),
                "Right-hand fit profile position changed in v0.7.10f.");
            AppendIfMissing(
                failures,
                VectorApproximately(fitProfile.SocketLocalEulerAngles, ExpectedFitEuler),
                "Right-hand fit profile rotation changed in v0.7.10f.");
            AppendIfMissing(
                failures,
                VectorApproximately(fitProfile.SocketLocalScale, ExpectedFitScale),
                "Right-hand fit profile scale changed in v0.7.10f.");
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
            warnings.Add("Barrel/muzzle line-of-sight reticle is still deferred.");
            warnings.Add("State readiness fallback remains available via profile but is disabled by default.");
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
