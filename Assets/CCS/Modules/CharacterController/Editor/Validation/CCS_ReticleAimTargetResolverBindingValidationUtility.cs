using System.Collections.Generic;
using System.IO;
using CCS.Modules.CharacterController;
using CCS.Modules.CharacterController.Local;
using CCS.Modules.Weapons;
using CCS.Project;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_ReticleAimTargetResolverBindingValidationUtility
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Validates v0.7.12a reticle aim target resolver binding.
// PLACEMENT: Editor validation utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_ReticleAimTargetResolverBindingValidationUtility
    {
        private const string MuzzleReticleSourcePath =
            "Assets/CCS/Modules/Weapons/Runtime/Aiming/CCS_MuzzleDrivenReticleController.cs";

        private const string AimAnimatorSourcePath =
            "Assets/CCS/Modules/CharacterController/Runtime/Animation/CCS_SingleRevolverAimAnimator.cs";

        private const string BodyAimSourcePath =
            "Assets/CCS/Modules/Weapons/Runtime/Aiming/CCS_RevolverBodyAimFollowController.cs";

        private const string ArmIkSourcePath =
            "Assets/CCS/Modules/Weapons/Runtime/Aiming/CCS_RevolverArmReticleIK.cs";

        private const string EquipmentFitStudioWindowPath =
            "Assets/CCS/Modules/CharacterController/Editor/EquipmentFitStudio/CCS_EquipmentFitStudioWindow.cs";

        private const string CharacterControllerTestsRoot =
            "Assets/CCS/Modules/CharacterController/Tests";

        private const string AnimationFitStudioRoot =
            "Assets/CCS/Modules/CharacterController/Editor/AnimationFitStudio";

        private const int ExpectedRootMonoBehaviourCount = 24;

        public static CCS_SurvivalValidationResult ValidateReticleAimTargetResolverBinding()
        {
            List<string> failures = new List<string>();
            List<string> warnings = new List<string>();

            ValidateReticleControllerSource(failures);
            ValidatePlayerPrefabBinding(failures);
            ValidateRootMonoBehaviourCount(failures);
            ValidateNoDeferredImplementation(failures);
            ValidateMissingScripts(failures);
            ValidateTestsFolderRemoved(failures);
            ValidateAnimationFitStudioNotPresent(failures);
            ValidateEquipmentFitStudioPresent(failures);
            CollectDeferredWarnings(warnings);

            if (failures.Count > 0)
            {
                return CCS_SurvivalValidationResult.Fail(string.Join(" ", failures));
            }

            string message = "Reticle aim target resolver binding validated.";
            if (warnings.Count > 0)
            {
                message += " Warnings: " + string.Join(" ", warnings);
            }

            return CCS_SurvivalValidationResult.Pass(message);
        }

        private static void ValidateReticleControllerSource(List<string> failures)
        {
            AppendIfMissing(failures, File.Exists(MuzzleReticleSourcePath), "Missing CCS_MuzzleDrivenReticleController.");

            if (!File.Exists(MuzzleReticleSourcePath))
            {
                return;
            }

            string source = File.ReadAllText(MuzzleReticleSourcePath);
            AppendIfMissing(
                failures,
                source.Contains("aimTargetSourceComponent"),
                "Reticle controller must serialize aimTargetSourceComponent.");
            AppendIfMissing(
                failures,
                source.Contains("CCS_IRevolverAimTargetSource"),
                "Reticle controller must consume CCS_IRevolverAimTargetSource.");
            AppendIfMissing(
                failures,
                source.Contains("ResolveScreenFromAimTargetSource"),
                "Reticle controller must resolve screen position from aim target source.");
            AppendIfMissing(
                failures,
                source.Contains("HasActiveAimTargetSource"),
                "Reticle controller must gate primary target path on aim target source.");
            AppendIfMissing(
                failures,
                source.Contains("IsAimPresentationReadyForReticle"),
                "Reticle controller must gate on animation event readiness.");
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
                source.Contains("HandleAimEnded"),
                "Reticle controller must hide reticle on holster start.");
            AppendIfMissing(
                failures,
                !source.Contains("ApplyDamage"),
                "Reticle controller must not drive gameplay damage.");
        }

        private static void ValidatePlayerPrefabBinding(List<string> failures)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath);
            AppendIfMissing(failures, prefab != null, "Missing networked player prefab.");

            if (prefab == null)
            {
                return;
            }

            CCS_RevolverAimTargetResolver[] resolvers =
                prefab.GetComponentsInChildren<CCS_RevolverAimTargetResolver>(true);
            AppendIfMissing(
                failures,
                resolvers != null && resolvers.Length == 1,
                "Player prefab must contain exactly one CCS_RevolverAimTargetResolver.");

            CCS_MuzzleDrivenReticleController reticleController =
                prefab.GetComponentInChildren<CCS_MuzzleDrivenReticleController>(true);
            AppendIfMissing(
                failures,
                reticleController != null,
                "Player prefab missing CCS_MuzzleDrivenReticleController.");

            if (resolvers == null || resolvers.Length == 0 || reticleController == null)
            {
                return;
            }

            CCS_RevolverAimTargetResolver resolver = resolvers[0];
            AppendIfMissing(
                failures,
                resolver.gameObject != prefab,
                "Resolver must not be attached to player root.");
            AppendIfMissing(
                failures,
                reticleController.gameObject.name == "WeaponHudRoot",
                "Reticle controller must remain on WeaponHudRoot.");

            SerializedObject serializedReticle = new SerializedObject(reticleController);
            SerializedProperty sourceProperty = serializedReticle.FindProperty("aimTargetSourceComponent");
            AppendIfMissing(
                failures,
                sourceProperty != null && sourceProperty.objectReferenceValue == resolver,
                "Reticle controller must reference Model/Aiming CCS_RevolverAimTargetResolver.");
        }

        private static void ValidateRootMonoBehaviourCount(List<string> failures)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CCS_PlayerPrefabConstants.NetworkedPlayerPrefabPath);
            if (prefab == null)
            {
                return;
            }

            int rootMonoBehaviourCount = prefab.GetComponents<MonoBehaviour>().Length;
            AppendIfMissing(
                failures,
                rootMonoBehaviourCount == ExpectedRootMonoBehaviourCount,
                "Player root MonoBehaviour count changed (expected "
                + ExpectedRootMonoBehaviourCount
                + ", found "
                + rootMonoBehaviourCount
                + ").");
        }

        private static void ValidateNoDeferredImplementation(List<string> failures)
        {
            if (File.Exists(BodyAimSourcePath))
            {
                string bodySource = File.ReadAllText(BodyAimSourcePath);
                AppendIfMissing(
                    failures,
                    !bodySource.Contains("CCS_RevolverAimTargetResolver"),
                    "Body aim must not wire aim target resolver in v0.7.12a.");
            }

            if (File.Exists(ArmIkSourcePath))
            {
                string armSource = File.ReadAllText(ArmIkSourcePath);
                AppendIfMissing(
                    failures,
                    !armSource.Contains("CCS_RevolverAimTargetResolver"),
                    "Arm IK must not wire aim target resolver in v0.7.12a.");
            }

            if (File.Exists(AimAnimatorSourcePath))
            {
                string aimAnimatorSource = File.ReadAllText(AimAnimatorSourcePath);
                AppendIfMissing(
                    failures,
                    aimAnimatorSource.Contains("NotifyRevolverAimHoldAnimationEvent"),
                    "Aim animator must retain animation event readiness notification.");
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
            if (File.Exists(MuzzleReticleSourcePath))
            {
                string source = File.ReadAllText(MuzzleReticleSourcePath);
                if (source.Contains("TryRaycast("))
                {
                    warnings.Add("Legacy camera ray fallback remains as emergency fallback only.");
                }
            }

            warnings.Add("Muzzle/barrel line-of-sight is still deferred.");
            warnings.Add("Body/arm aim is still deferred.");
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
