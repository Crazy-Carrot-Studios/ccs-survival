using System.Collections.Generic;
using System.IO;
using CCS.Modules.CharacterController;
using CCS.Modules.CharacterController.Local;
using CCS.Modules.Weapons;
using CCS.Project;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_RevolverAimTargetResolverValidationUtility
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Validates v0.7.12 revolver aim target resolver prototype.
// PLACEMENT: Editor validation utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-25
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public static class CCS_RevolverAimTargetResolverValidationUtility
    {
        private const string ResolverSourcePath =
            "Assets/CCS/Modules/CharacterController/Runtime/Aiming/CCS_RevolverAimTargetResolver.cs";

        private const string ProfileClassPath =
            "Assets/CCS/Modules/CharacterController/Runtime/Aiming/CCS_RevolverAimTargetProfile.cs";

        private const string MuzzleReticleSourcePath =
            "Assets/CCS/Modules/Weapons/Runtime/Aiming/CCS_MuzzleDrivenReticleController.cs";

        private const string ArmIkSourcePath =
            "Assets/CCS/Modules/Weapons/Runtime/Aiming/CCS_RevolverArmReticleIK.cs";

        private const string BodyAimSourcePath =
            "Assets/CCS/Modules/Weapons/Runtime/Aiming/CCS_RevolverBodyAimFollowController.cs";

        private const string EquipmentFitStudioWindowPath =
            "Assets/CCS/Modules/CharacterController/Editor/EquipmentFitStudio/CCS_EquipmentFitStudioWindow.cs";

        private const string CharacterControllerTestsRoot =
            "Assets/CCS/Modules/CharacterController/Tests";

        private const string AnimationFitStudioRoot =
            "Assets/CCS/Modules/CharacterController/Editor/AnimationFitStudio";

        private const int ExpectedRootMonoBehaviourCount = 24;

        public static CCS_SurvivalValidationResult ValidateRevolverAimTargetResolver()
        {
            List<string> failures = new List<string>();
            List<string> warnings = new List<string>();

            ValidateRequiredAssets(failures);
            ValidateProfile(failures);
            ValidateResolverSource(failures);
            ValidatePlayerPrefabWiring(failures);
            ValidateRootMonoBehaviourCount(failures);
            ValidateNoGameplayWiring(failures);
            ValidateMissingScripts(failures);
            ValidateTestsFolderRemoved(failures);
            ValidateAnimationFitStudioNotPresent(failures);
            ValidateEquipmentFitStudioPresent(failures);
            CollectDeferredWarnings(warnings);

            if (failures.Count > 0)
            {
                return CCS_SurvivalValidationResult.Fail(string.Join(" ", failures));
            }

            string message = "Revolver aim target resolver prototype validated.";
            if (warnings.Count > 0)
            {
                message += " Warnings: " + string.Join(" ", warnings);
            }

            return CCS_SurvivalValidationResult.Pass(message);
        }

        private static void ValidateRequiredAssets(List<string> failures)
        {
            AppendIfMissing(failures, File.Exists(ProfileClassPath), "Missing CCS_RevolverAimTargetProfile class.");
            AppendIfMissing(failures, File.Exists(ResolverSourcePath), "Missing CCS_RevolverAimTargetResolver.");
            AppendIfMissing(
                failures,
                File.Exists(CCS_CharacterControllerConstants.RevolverAimTargetProfilePath),
                "Missing CCS_RevolverAimTargetProfile asset.");
        }

        private static void ValidateProfile(List<string> failures)
        {
            CCS_RevolverAimTargetProfile profile = AssetDatabase.LoadAssetAtPath<CCS_RevolverAimTargetProfile>(
                CCS_CharacterControllerConstants.RevolverAimTargetProfilePath);
            AppendIfMissing(failures, profile != null, "Could not load aim target profile.");

            if (profile == null)
            {
                return;
            }

            AppendIfMissing(failures, profile.CameraRayDistance > 0f, "cameraRayDistance must be > 0.");
            AppendIfMissing(failures, profile.FallbackDistance > 0f, "fallbackDistance must be > 0.");
            AppendIfMissing(failures, profile.TargetSmoothingTime >= 0f, "targetSmoothingTime must be >= 0.");
            AppendIfMissing(failures, profile.MaxTargetSnapDistance > 0f, "maxTargetSnapDistance must be > 0.");
            AppendIfMissing(failures, profile.LastValidTargetHoldSeconds >= 0f, "lastValidTargetHoldSeconds must be >= 0.");
            AppendIfMissing(failures, profile.MinimumValidDistance > 0f, "minimumValidDistance must be > 0.");
        }

        private static void ValidateResolverSource(List<string> failures)
        {
            if (!File.Exists(ResolverSourcePath))
            {
                return;
            }

            string source = File.ReadAllText(ResolverSourcePath);
            AppendIfMissing(
                failures,
                source.Contains("CCS_IRevolverAimTargetSource"),
                "Resolver must implement CCS_IRevolverAimTargetSource.");
            AppendIfMissing(
                failures,
                source.Contains("IsLocalPresentationOwner"),
                "Resolver must gate on local owner presentation context.");
            AppendIfMissing(
                failures,
                !source.Contains("CCS_RevolverController"),
                "Resolver must not reference gameplay revolver controller.");
            AppendIfMissing(
                failures,
                !source.Contains("ApplyDamage") && !source.Contains("Fire"),
                "Resolver must not drive gameplay fire/damage.");
        }

        private static void ValidatePlayerPrefabWiring(List<string> failures)
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

            if (resolvers == null || resolvers.Length == 0)
            {
                return;
            }

            CCS_RevolverAimTargetResolver resolver = resolvers[0];
            AppendIfMissing(
                failures,
                resolver.gameObject.name == CCS_CharacterControllerConstants.RevolverAimTargetResolverObjectName,
                "Resolver must live on Model/Aiming branch.");
            AppendIfMissing(
                failures,
                resolver.transform.parent != null
                    && resolver.transform.parent.name == "Model",
                "Resolver must be child of Model root.");
            AppendIfMissing(
                failures,
                resolver.gameObject != prefab,
                "Resolver must not be attached to player root.");

            CCS_RevolverAimTargetProfile profile = AssetDatabase.LoadAssetAtPath<CCS_RevolverAimTargetProfile>(
                CCS_CharacterControllerConstants.RevolverAimTargetProfilePath);
            SerializedObject serializedResolver = new SerializedObject(resolver);
            SerializedProperty profileProperty = serializedResolver.FindProperty("aimTargetProfile");
            AppendIfMissing(
                failures,
                profileProperty != null && profileProperty.objectReferenceValue == profile,
                "Resolver must reference CCS_RevolverAimTargetProfile asset.");
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

        private static void ValidateNoGameplayWiring(List<string> failures)
        {
            if (File.Exists(MuzzleReticleSourcePath))
            {
                string reticleSource = File.ReadAllText(MuzzleReticleSourcePath);
                AppendIfMissing(
                    failures,
                    !reticleSource.Contains("CCS_RevolverAimTargetResolver"),
                    "Reticle controller must not wire aim target resolver in v0.7.12.");
                AppendIfMissing(
                    failures,
                    !reticleSource.Contains("CCS_IRevolverAimTargetSource"),
                    "Reticle controller must not consume aim target source in v0.7.12.");
            }

            if (File.Exists(ArmIkSourcePath))
            {
                string armSource = File.ReadAllText(ArmIkSourcePath);
                AppendIfMissing(
                    failures,
                    !armSource.Contains("CCS_RevolverAimTargetResolver"),
                    "Arm IK must not wire aim target resolver in v0.7.12.");
            }

            if (File.Exists(BodyAimSourcePath))
            {
                string bodySource = File.ReadAllText(BodyAimSourcePath);
                AppendIfMissing(
                    failures,
                    !bodySource.Contains("CCS_RevolverAimTargetResolver"),
                    "Body aim follow must not wire aim target resolver in v0.7.12.");
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
            CCS_RevolverAimTargetProfile profile = AssetDatabase.LoadAssetAtPath<CCS_RevolverAimTargetProfile>(
                CCS_CharacterControllerConstants.RevolverAimTargetProfilePath);
            if (profile != null && !profile.DrawDebugRayWhenDiagnosticsEnabled)
            {
                warnings.Add("Resolver debug rays are disabled by profile default.");
            }

            warnings.Add("Remote player aim target replication is not implemented.");
            warnings.Add("Muzzle/barrel line-of-sight remains deferred.");
            warnings.Add("Reticle still uses v0.7.10f event timing and current target behavior.");
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
