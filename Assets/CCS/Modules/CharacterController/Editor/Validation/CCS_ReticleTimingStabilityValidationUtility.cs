using System.Collections.Generic;
using System.IO;
using CCS.Modules.CharacterController;
using CCS.Modules.CharacterController.Local;
using CCS.Modules.Weapons;
using CCS.Project;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// =============================================================================
// SCRIPT: CCS_ReticleTimingStabilityValidationUtility
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Validates v0.7.10e reticle reveal timing and pitch stability.
// PLACEMENT: Editor validation utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-30
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_ReticleTimingStabilityValidationUtility
    {
        private const string ProfileClassPath =
            "Assets/CCS/Modules/CharacterController/Runtime/Visuals/CCS_RevolverReticlePresentationProfile.cs";

        private const string ReadinessInterfacePath =
            "Assets/CCS/Modules/CharacterController/Runtime/Animation/CCS_IRevolverAimPresentationReadinessSource.cs";

        private const string AimAnimatorSourcePath =
            "Assets/CCS/Modules/CharacterController/Runtime/Animation/CCS_SingleRevolverAimAnimator.cs";

        private const string MuzzleReticleSourcePath =
            "Assets/CCS/Modules/Weapons/Runtime/Aiming/CCS_MuzzleDrivenReticleController.cs";

        private const string EquipmentFitStudioWindowPath =
            "Assets/CCS/Modules/CharacterController/Editor/EquipmentFitStudio/CCS_EquipmentFitStudioWindow.cs";

        private const string CharacterControllerTestsRoot =
            "Assets/CCS/Modules/CharacterController/Tests";

        private const string AnimationFitStudioRoot =
            "Assets/CCS/Modules/CharacterController/Editor/AnimationFitStudio";

        private static readonly Vector3 ExpectedFitPosition = new Vector3(0.099f, 0.176f, 0.014f);

        private static readonly Vector3 ExpectedFitEuler = new Vector3(-48.275f, 103.374f, 64.828f);

        private static readonly Vector3 ExpectedFitScale = Vector3.one;

        public static CCS_SurvivalValidationResult ValidateReticleTimingStability()
        {
            List<string> failures = new List<string>();
            List<string> warnings = new List<string>();

            ValidateRequiredAssets(failures);
            ValidatePresentationProfile(failures);
            ValidateReadinessContract(failures);
            ValidateAimAnimatorRevealWindow(failures);
            ValidateReticleController(failures);
            ValidatePlayerPrefabWiring(failures);
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

            string message = "Reticle timing and pitch stability validated.";
            if (warnings.Count > 0)
            {
                message += " Warnings: " + string.Join(" ", warnings);
            }

            return CCS_SurvivalValidationResult.Pass(message);
        }

        private static void ValidateRequiredAssets(List<string> failures)
        {
            AppendIfMissing(failures, File.Exists(ProfileClassPath), "Missing CCS_RevolverReticlePresentationProfile class.");
            AppendIfMissing(
                failures,
                File.Exists(CCS_CharacterControllerConstants.RevolverReticlePresentationProfilePath),
                "Missing CCS_RevolverReticlePresentationProfile asset.");
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
                profile.DrawRevealNormalizedTime >= 0.1f && profile.DrawRevealNormalizedTime <= 0.95f,
                "drawRevealNormalizedTime must stay within 0.1–0.95.");
            AppendIfMissing(
                failures,
                profile.MaxScreenSnapPixelsPerFrame > 0f,
                "maxScreenSnapPixelsPerFrame must be > 0.");
            AppendIfMissing(
                failures,
                profile.NoHitFallbackDistance > 0f,
                "noHitFallbackDistance must be > 0.");
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
                source.Contains("IsAimPresentationInReticleRevealWindow"),
                "Readiness interface must expose IsAimPresentationInReticleRevealWindow.");
        }

        private static void ValidateAimAnimatorRevealWindow(List<string> failures)
        {
            if (!File.Exists(AimAnimatorSourcePath))
            {
                return;
            }

            string source = File.ReadAllText(AimAnimatorSourcePath);
            AppendIfMissing(
                failures,
                source.Contains("IsAimPresentationInReticleRevealWindow"),
                "CCS_SingleRevolverAimAnimator must expose reveal window property.");
            AppendIfMissing(
                failures,
                source.Contains("reticlePresentationProfile"),
                "CCS_SingleRevolverAimAnimator must reference reticle presentation profile.");
            AppendIfMissing(
                failures,
                source.Contains("ComputeDrawRevealNormalizedThreshold")
                    || source.Contains("ResolveDrawRevealNormalizedThreshold"),
                "Aim animator must compute draw reveal threshold from profile.");
            AppendIfMissing(
                failures,
                source.Contains("SingleRevolverDrawStateName") || source.Contains("revolverDrawStateHash"),
                "Aim animator must evaluate draw state for reveal window.");
        }

        private static void ValidateReticleController(List<string> failures)
        {
            if (!File.Exists(MuzzleReticleSourcePath))
            {
                return;
            }

            string source = File.ReadAllText(MuzzleReticleSourcePath);
            AppendIfMissing(
                failures,
                source.Contains("reticlePresentationProfile"),
                "CCS_MuzzleDrivenReticleController must serialize reticle presentation profile.");
            AppendIfMissing(
                failures,
                source.Contains("IsAimPresentationInReticleRevealWindow"),
                "Reticle controller must honor reveal window property.");
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
                source.Contains("HasAimIntentActive") && source.Contains("IsReticlePresentationVisible"),
                "Reticle controller must combine aim intent with reveal window / hold readiness.");
            AppendIfMissing(
                failures,
                source.Contains("SmoothDamp"),
                "Reticle controller must smooth screen position.");
            AppendIfMissing(
                failures,
                source.Contains("MaxScreenSnapPixelsPerFrame") || source.Contains("ResolveMaxScreenSnapPixelsPerFrame"),
                "Reticle controller must clamp per-frame snap.");
            AppendIfMissing(
                failures,
                source.Contains("NoHitFallbackDistance") || source.Contains("ResolveNoHitFallbackDistance"),
                "Reticle controller must use no-hit fallback distance from profile.");
            AppendIfMissing(
                failures,
                source.Contains("HoldLastValidTargetOnNoHit") || source.Contains("lastValidScreenTarget"),
                "Reticle controller must hold last valid target on invalid projection.");
        }

        private static void ValidatePlayerPrefabWiring(List<string> failures)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath);
            AppendIfMissing(failures, prefab != null, "Missing networked player prefab.");

            if (prefab == null)
            {
                return;
            }

            CCS_RevolverReticlePresentationProfile profile = AssetDatabase.LoadAssetAtPath<CCS_RevolverReticlePresentationProfile>(
                CCS_CharacterControllerConstants.RevolverReticlePresentationProfilePath);
            CCS_MuzzleDrivenReticleController reticleController =
                prefab.GetComponentInChildren<CCS_MuzzleDrivenReticleController>(true);
            CCS_SingleRevolverAimAnimator aimAnimator = prefab.GetComponentInChildren<CCS_SingleRevolverAimAnimator>(true);

            AppendIfMissing(failures, reticleController != null, "Player prefab missing reticle controller.");
            AppendIfMissing(failures, aimAnimator != null, "Player prefab missing aim animator readiness source.");

            if (reticleController == null)
            {
                return;
            }

            SerializedObject serializedReticle = new SerializedObject(reticleController);
            SerializedProperty profileProperty = serializedReticle.FindProperty("reticlePresentationProfile");
            AppendIfMissing(
                failures,
                profileProperty != null && profileProperty.objectReferenceValue != null,
                "Reticle controller must reference reticle presentation profile.");
            AppendIfMissing(
                failures,
                profileProperty != null && profileProperty.objectReferenceValue == profile,
                "Reticle controller profile reference must point to CCS_RevolverReticlePresentationProfile asset.");

            SerializedProperty readinessProperty = serializedReticle.FindProperty("aimPresentationReadinessSourceComponent");
            AppendIfMissing(
                failures,
                readinessProperty != null && readinessProperty.objectReferenceValue == aimAnimator,
                "Reticle controller must reference CCS_SingleRevolverAimAnimator readiness source.");

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
                "Right-hand fit profile position changed in v0.7.10e.");
            AppendIfMissing(
                failures,
                VectorApproximately(fitProfile.SocketLocalEulerAngles, ExpectedFitEuler),
                "Right-hand fit profile rotation changed in v0.7.10e.");
            AppendIfMissing(
                failures,
                VectorApproximately(fitProfile.SocketLocalScale, ExpectedFitScale),
                "Right-hand fit profile scale changed in v0.7.10e.");
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
            warnings.Add("Barrel/muzzle line-of-sight reticle is still not implemented.");
            warnings.Add("Reticle still uses camera/current-mode target with hybrid muzzle drift.");
            warnings.Add("Visual arm alignment to reticle may still need future convergence work.");
        }

        private static bool VectorApproximately(Vector3 left, Vector3 right)
        {
            return Mathf.Approximately(left.x, right.x)
                && Mathf.Approximately(left.y, right.y)
                && Mathf.Approximately(left.z, right.z);
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
