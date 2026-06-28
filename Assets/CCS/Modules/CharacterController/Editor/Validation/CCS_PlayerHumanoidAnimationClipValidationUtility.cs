using System.Collections.Generic;
using System.IO;
using System.Text;
using CCS.Modules.CharacterController.Editor.AnimationFitStudio;
using CCS.Project;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_PlayerHumanoidAnimationClipValidationUtility
// CATEGORY: Modules / CharacterController / Editor / Validation
// PURPOSE: Validates humanoid clip compatibility for AC_CCS_Player_Locomotion_StarterAssets.
// PLACEMENT: Editor validation utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-28
// NOTES: v0.8.1b — fails hybrid Generic+Humanoid clips and prefab binding warnings.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor
{
    public sealed class CCS_PlayerHumanoidClipValidationReport
    {
        public string ClipPath = string.Empty;
        public string ClipName = string.Empty;
        public float Length;
        public bool Empty;
        public bool Legacy;
        public bool HumanMotion;
        public bool HasHumanoidMuscleCurves;
        public bool HasGenericTransformCurves;
        public int HumanoidMuscleBindingCount;
        public int GenericTransformBindingCount;
        public string SourceKind = "Unknown";
        public string ModelImporterAnimationType = "Unknown";
        public string AvatarSource = "Unknown";
    }

    public static class CCS_PlayerHumanoidAnimationClipValidationUtility
    {
        public static readonly string[] RequiredControllerClipPaths =
        {
            CCS_CharacterControllerConstants.LocomotionAnimationsPath + "/CCS_Locomotion_Idle.anim",
            CCS_CharacterControllerConstants.LocomotionAnimationsPath + "/CCS_Locomotion_Walk_N.anim",
            CCS_CharacterControllerConstants.LocomotionAnimationsPath + "/CCS_Locomotion_Run_N.anim",
            CCS_CharacterControllerConstants.LocomotionAnimationsPath + "/CCS_Locomotion_JumpStart.anim",
            CCS_CharacterControllerConstants.LocomotionAnimationsPath + "/CCS_Locomotion_InAir.anim",
            CCS_CharacterControllerConstants.InteractionPickUpRightHandClipPath,
            CCS_CharacterControllerConstants.InteractionWalkThroughDoorRightHandClipPath,
            CCS_CharacterControllerConstants.RevolverIdleToAimClipPath,
            CCS_CharacterControllerConstants.RevolverAimIdleFullDrawClipPath,
            CCS_CharacterControllerConstants.AimStrafeWalkFwdClipPath,
            CCS_CharacterControllerConstants.AimStrafeWalkBwdClipPath,
            CCS_CharacterControllerConstants.AimStrafeStrafeLeftClipPath,
            CCS_CharacterControllerConstants.AimStrafeStrafeRightClipPath,
        };

        private const string GenericBindingWarningToken =
            "Generic clips animate transforms that are already bound by a Humanoid avatar";

        #region Public Methods

        public static CCS_SurvivalValidationResult ValidateAllPlayerHumanoidAnimationBinding()
        {
            List<string> failures = new List<string>();
            AppendResult(failures, ValidateRequiredControllerClips());
            AppendResult(failures, ValidateControllerParameters());
            AppendResult(failures, ValidatePlayerAnimatorAvatarAndMeshBinding());
            AppendResult(failures, ValidatePrefabGenericBindingWarnings());
            AppendResult(
                failures,
                CCS_PlayerVisualAndAnimatorBindingValidationUtility.ValidateAllPlayerVisualAndAnimatorBinding());

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass("Player humanoid animation binding validation passed.");
        }

        public static CCS_SurvivalValidationResult ValidateRequiredControllerClips()
        {
            List<string> failures = new List<string>();
            List<CCS_PlayerHumanoidClipValidationReport> reports = BuildRequiredClipReports();
            for (int reportIndex = 0; reportIndex < reports.Count; reportIndex++)
            {
                AppendClipFailures(failures, reports[reportIndex]);
            }

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass("Required controller clips are humanoid-compatible.");
        }

        public static CCS_SurvivalValidationResult ValidateControllerParameters()
        {
            return CCS_PlayerVisualAndAnimatorBindingValidationUtility.ValidateControllerParameterAgreement();
        }

        public static CCS_SurvivalValidationResult ValidatePlayerAnimatorAvatarAndMeshBinding()
        {
            List<string> failures = new List<string>();
            string[] prefabPaths =
            {
                CCS_PlayerPrefabConstants.ProductionPlayerPrefabPath,
                CCS_PlayerPrefabConstants.TestHarnessPlayerPrefabPath,
                CCS_PlayerPrefabConstants.LegacyMasterTestPlayerPrefabPath,
            };

            for (int prefabIndex = 0; prefabIndex < prefabPaths.Length; prefabIndex++)
            {
                ValidatePrefabAnimatorAvatarAndMeshes(failures, prefabPaths[prefabIndex]);
            }

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass("Player Animator avatar and mesh binding validated.");
        }

        public static CCS_SurvivalValidationResult ValidatePrefabGenericBindingWarnings()
        {
            List<string> failures = new List<string>();
            string[] prefabPaths =
            {
                CCS_PlayerPrefabConstants.ProductionPlayerPrefabPath,
                CCS_PlayerPrefabConstants.TestHarnessPlayerPrefabPath,
                CCS_PlayerPrefabConstants.LegacyMasterTestPlayerPrefabPath,
            };

            for (int prefabIndex = 0; prefabIndex < prefabPaths.Length; prefabIndex++)
            {
                string prefabPath = prefabPaths[prefabIndex];
                if (!File.Exists(prefabPath))
                {
                    continue;
                }

                string prefabText = File.ReadAllText(prefabPath);
                if (prefabText.Contains(GenericBindingWarningToken))
                {
                    failures.Add(
                        prefabPath
                        + " contains Unity Humanoid/generic clip binding warning. Repair source clips and re-save prefab.");
                }
            }

            return failures.Count > 0
                ? CCS_SurvivalValidationResult.Fail(string.Join(" ", failures))
                : CCS_SurvivalValidationResult.Pass("No prefab generic/humanoid binding warnings found.");
        }

        public static List<CCS_PlayerHumanoidClipValidationReport> BuildRequiredClipReports()
        {
            List<CCS_PlayerHumanoidClipValidationReport> reports = new List<CCS_PlayerHumanoidClipValidationReport>();
            for (int clipIndex = 0; clipIndex < RequiredControllerClipPaths.Length; clipIndex++)
            {
                reports.Add(BuildClipReport(RequiredControllerClipPaths[clipIndex]));
            }

            return reports;
        }

        public static string BuildMarkdownClipReport(IReadOnlyList<CCS_PlayerHumanoidClipValidationReport> reports)
        {
            StringBuilder builder = new StringBuilder(8192);
            builder.AppendLine("# Player Humanoid Clip Validation Report");
            builder.AppendLine();
            builder.AppendLine("| Clip | Length | humanMotion | Muscle Curves | Generic Transforms | Source | Import Type |");
            builder.AppendLine("|---|---:|---|---|---|---|---|");
            for (int reportIndex = 0; reportIndex < reports.Count; reportIndex++)
            {
                CCS_PlayerHumanoidClipValidationReport report = reports[reportIndex];
                builder.Append("| ");
                builder.Append(report.ClipName);
                builder.Append(" | ");
                builder.Append(report.Length.ToString("0.000"));
                builder.Append(" | ");
                builder.Append(report.HumanMotion);
                builder.Append(" | ");
                builder.Append(report.HumanoidMuscleBindingCount);
                builder.Append(" | ");
                builder.Append(report.GenericTransformBindingCount);
                builder.Append(" | ");
                builder.Append(report.SourceKind);
                builder.Append(" | ");
                builder.Append(report.ModelImporterAnimationType);
                builder.AppendLine(" |");
            }

            return builder.ToString();
        }

        #endregion

        #region Private Methods

        private static CCS_PlayerHumanoidClipValidationReport BuildClipReport(string clipAssetPath)
        {
            CCS_PlayerHumanoidClipValidationReport report = new CCS_PlayerHumanoidClipValidationReport
            {
                ClipPath = clipAssetPath,
            };

            if (string.IsNullOrEmpty(clipAssetPath) || !File.Exists(clipAssetPath))
            {
                report.ClipName = Path.GetFileNameWithoutExtension(clipAssetPath);
                report.Empty = true;
                return report;
            }

            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipAssetPath);
            if (clip == null)
            {
                report.ClipName = Path.GetFileNameWithoutExtension(clipAssetPath);
                report.Empty = true;
                return report;
            }

            report.ClipName = clip.name;
            report.Length = clip.length;
            report.Empty = clip.empty || clip.length <= 0.0001f;
            report.Legacy = clip.legacy;
            report.HumanMotion = clip.humanMotion;
            report.SourceKind = clipAssetPath.EndsWith(".anim") ? "Generated .anim" : "Embedded/other";
            report.ModelImporterAnimationType = ResolveModelImporterAnimationType(clipAssetPath);
            report.AvatarSource = ResolveAvatarSourceLabel(clipAssetPath);

            EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(clip);
            for (int bindingIndex = 0; bindingIndex < bindings.Length; bindingIndex++)
            {
                if (CCS_AnimationFitStudioClipCurveModeUtility.IsHumanoidMuscleBinding(bindings[bindingIndex]))
                {
                    report.HumanoidMuscleBindingCount++;
                    report.HasHumanoidMuscleCurves = true;
                    continue;
                }

                if (CCS_PlayerHumanoidAnimationClipRepairUtility.IsGenericTransformBinding(bindings[bindingIndex]))
                {
                    report.GenericTransformBindingCount++;
                    report.HasGenericTransformCurves = true;
                }
            }

            return report;
        }

        private static void AppendClipFailures(List<string> failures, CCS_PlayerHumanoidClipValidationReport report)
        {
            if (string.IsNullOrEmpty(report.ClipPath) || !File.Exists(report.ClipPath))
            {
                failures.Add("Missing required controller clip at " + report.ClipPath + ".");
                return;
            }

            if (report.Empty)
            {
                failures.Add(report.ClipPath + " is empty.");
            }

            if (report.Legacy)
            {
                failures.Add(report.ClipPath + " is legacy.");
            }

            if (!report.HasHumanoidMuscleCurves)
            {
                failures.Add(report.ClipPath + " has no Humanoid muscle curves.");
            }

            if (report.HasGenericTransformCurves)
            {
                failures.Add(
                    report.ClipPath
                    + " contains "
                    + report.GenericTransformBindingCount
                    + " generic transform curve binding(s) on a Humanoid Animator clip set.");
            }

            if (!report.HumanMotion && !report.HasHumanoidMuscleCurves)
            {
                failures.Add(report.ClipPath + " is not humanMotion and cannot drive CC3_Base_PlusAvatar.");
            }
        }

        private static void ValidatePrefabAnimatorAvatarAndMeshes(List<string> failures, string prefabPath)
        {
            if (!File.Exists(prefabPath))
            {
                failures.Add("Missing player prefab at " + prefabPath + ".");
                return;
            }

            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
            if (prefabRoot == null)
            {
                failures.Add("Could not load prefab " + prefabPath + ".");
                return;
            }

            try
            {
                if (!CCS_PlayerVisualAndAnimatorBindingBuilder.TryResolveAuthoritativeAnimator(
                        prefabRoot,
                        out Animator animator)
                    || animator == null)
                {
                    failures.Add(prefabPath + " is missing authoritative Animator.");
                    return;
                }

                if (animator.avatar == null || !animator.avatar.isValid || !animator.avatar.isHuman)
                {
                    failures.Add(prefabPath + " authoritative Animator avatar must be valid Humanoid.");
                }

                if (animator.applyRootMotion)
                {
                    failures.Add(prefabPath + " authoritative Animator.applyRootMotion must be false.");
                }

                if (animator.cullingMode == AnimatorCullingMode.CullCompletely)
                {
                    failures.Add(prefabPath + " authoritative Animator.cullingMode must not be Cull Completely.");
                }

                SkinnedMeshRenderer[] skinnedMeshRenderers =
                    animator.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                if (skinnedMeshRenderers.Length == 0)
                {
                    failures.Add(prefabPath + " has no SkinnedMeshRenderer under authoritative Animator.");
                    return;
                }

                for (int rendererIndex = 0; rendererIndex < skinnedMeshRenderers.Length; rendererIndex++)
                {
                    SkinnedMeshRenderer renderer = skinnedMeshRenderers[rendererIndex];
                    if (renderer == null)
                    {
                        continue;
                    }

                    if (!renderer.transform.IsChildOf(animator.transform))
                    {
                        failures.Add(
                            prefabPath
                            + " SkinnedMeshRenderer "
                            + renderer.name
                            + " is outside authoritative Animator hierarchy.");
                    }
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        private static string ResolveModelImporterAnimationType(string clipAssetPath)
        {
            string sourceAssetPath = ResolveLikelySourceFbxPath(clipAssetPath);
            if (string.IsNullOrEmpty(sourceAssetPath) || !File.Exists(sourceAssetPath))
            {
                return "Unknown";
            }

            ModelImporter importer = AssetImporter.GetAtPath(sourceAssetPath) as ModelImporter;
            if (importer == null)
            {
                return "Unknown";
            }

            return importer.animationType.ToString();
        }

        private static string ResolveAvatarSourceLabel(string clipAssetPath)
        {
            string sourceAssetPath = ResolveLikelySourceFbxPath(clipAssetPath);
            if (string.IsNullOrEmpty(sourceAssetPath))
            {
                return "Unknown";
            }

            ModelImporter importer = AssetImporter.GetAtPath(sourceAssetPath) as ModelImporter;
            if (importer == null)
            {
                return clipAssetPath.EndsWith(".anim") ? "Standalone .anim" : "Unknown";
            }

            return importer.avatarSetup.ToString();
        }

        private static string ResolveLikelySourceFbxPath(string clipAssetPath)
        {
            if (clipAssetPath.Contains("/Locomotion/"))
            {
                if (clipAssetPath.Contains("Idle.anim"))
                {
                    return "Assets/StarterAssets/ThirdPersonController/Character/Animations/Stand--Idle.anim.fbx";
                }

                if (clipAssetPath.Contains("Walk_N.anim"))
                {
                    return "Assets/StarterAssets/ThirdPersonController/Character/Animations/Locomotion--Walk_N.anim.fbx";
                }

                if (clipAssetPath.Contains("Run_N.anim"))
                {
                    return "Assets/StarterAssets/ThirdPersonController/Character/Animations/Locomotion--Run_N.anim.fbx";
                }

                if (clipAssetPath.Contains("JumpStart.anim"))
                {
                    return "Assets/StarterAssets/ThirdPersonController/Character/Animations/Jump--Jump.anim.fbx";
                }

                if (clipAssetPath.Contains("InAir.anim"))
                {
                    return "Assets/StarterAssets/ThirdPersonController/Character/Animations/Jump--InAir.anim.fbx";
                }
            }

            if (clipAssetPath.Contains("/Interaction/"))
            {
                return CCS_CharacterControllerConstants.MapMainFbxPath;
            }

            if (clipAssetPath.Contains("/AimStrafe/"))
            {
                if (clipAssetPath.Contains("WalkFwd"))
                {
                    return CCS_CharacterControllerConstants.MapMainFbxPath;
                }

                return CCS_CharacterControllerConstants.MapAdditionalsFbxPath;
            }

            if (clipAssetPath.Contains("/Combat/Aiming/Revolver/"))
            {
                return CCS_CharacterControllerConstants.WildWestAnimationPackRootPath + "/Idle/Fulldraw_Idle.fbx";
            }

            return string.Empty;
        }

        private static void AppendResult(List<string> failures, CCS_SurvivalValidationResult result)
        {
            if (!result.IsSuccess)
            {
                failures.Add(result.Message);
            }
        }

        #endregion
    }
}
