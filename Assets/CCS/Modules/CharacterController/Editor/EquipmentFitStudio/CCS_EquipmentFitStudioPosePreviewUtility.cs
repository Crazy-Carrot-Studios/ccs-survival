using System.IO;
using CCS.Modules.CharacterController;
using CCS.Project;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_EquipmentFitStudioPosePreviewUtility
// CATEGORY: Modules / CharacterController / Editor / EquipmentFitStudio
// PURPOSE: Editor-only neutral/aim/fire pose preview for Equipment Fit Studio.
// PLACEMENT: Editor utility used by Fit Studio window; never writes to prefab/scene.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Uses CCS-owned clips/controller only. Restores cleanly via AnimationMode.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.EquipmentFitStudio
{
    public static class CCS_EquipmentFitStudioPosePreviewUtility
    {
        public const float RevolverFireFrameNormalizedTime = 0.12f;

        public const string FireFramePreviewDisabledTooltip =
            "Fire frame preview is planned after aim pose preview is stable.";

        private static readonly int IsAimingMovementModeHash =
            Animator.StringToHash(CCS_CharacterControllerConstants.AnimatorIsAimingMovementModeParameter);

        private static readonly int AimMoveXHash =
            Animator.StringToHash(CCS_CharacterControllerConstants.AnimatorAimMoveXParameter);

        private static readonly int AimMoveYHash =
            Animator.StringToHash(CCS_CharacterControllerConstants.AnimatorAimMoveYParameter);

        private static readonly int RevolverAimHeldHash =
            Animator.StringToHash(CCS_CharacterControllerConstants.AnimatorRevolverAimHeldParameter);

        private static readonly int RevolverIsReloadingHash =
            Animator.StringToHash(CCS_CharacterControllerConstants.AnimatorRevolverIsReloadingParameter);

        private static bool animationModeStarted;

        private static GameObject activePlayerRoot;

        private static CCS_EquipmentFitStudioPosePreviewMode activeMode =
            CCS_EquipmentFitStudioPosePreviewMode.Neutral;

        public static bool IsPosePreviewActive =>
            animationModeStarted && AnimationMode.InAnimationMode();

        public static CCS_EquipmentFitStudioPosePreviewMode ActiveMode => activeMode;

        public static bool IsFireFramePreviewEnabled => false;

        public static CCS_EquipmentFitStudioPosePreviewMode GetDefaultPosePreviewForSocket(string socketId)
        {
            if (socketId == CCS_EquipmentConstants.HandSocketRightId)
            {
                return CCS_EquipmentFitStudioPosePreviewMode.RevolverAim;
            }

            return CCS_EquipmentFitStudioPosePreviewMode.Neutral;
        }

        public static string GetPosePreviewHint(string socketId)
        {
            if (socketId == CCS_EquipmentConstants.HandSocketRightId)
            {
                return "Equipped fit should be tuned while the character is in the aiming pose.";
            }

            return "Holster fit is tuned in neutral stance.";
        }

        public static string GetPosePreviewStateLabel(CCS_EquipmentFitStudioPosePreviewMode mode)
        {
            switch (mode)
            {
                case CCS_EquipmentFitStudioPosePreviewMode.RevolverAim:
                    return "Revolver Aim";
                case CCS_EquipmentFitStudioPosePreviewMode.RevolverFireFrame:
                    return "Revolver Fire Frame";
                default:
                    return "Neutral";
            }
        }

        public static bool TryApplyPosePreview(
            GameObject playerRoot,
            CCS_EquipmentFitStudioPosePreviewMode mode,
            out string errorMessage)
        {
            errorMessage = string.Empty;
            if (playerRoot == null)
            {
                errorMessage = "Select a player before applying pose preview.";
                return false;
            }

            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                errorMessage =
                    "Pose preview tuning uses Edit Fit Preview mode. Exit Play Mode or use the editor preview player.";
                return false;
            }

            Animator animator = FindPlayerAnimator(playerRoot);
            if (animator == null)
            {
                errorMessage = "Missing Animator under VisualRoot on selected player.";
                return false;
            }

            if (mode == CCS_EquipmentFitStudioPosePreviewMode.RevolverFireFrame && !IsFireFramePreviewEnabled)
            {
                errorMessage = FireFramePreviewDisabledTooltip;
                return false;
            }

            if (mode == CCS_EquipmentFitStudioPosePreviewMode.Neutral)
            {
                ClearPosePreview(playerRoot);
                activeMode = CCS_EquipmentFitStudioPosePreviewMode.Neutral;
                activePlayerRoot = playerRoot;
                return true;
            }

            EnsureAnimationModeStarted();
            activePlayerRoot = playerRoot;
            activeMode = mode;

            switch (mode)
            {
                case CCS_EquipmentFitStudioPosePreviewMode.RevolverAim:
                    ApplyRevolverAimPose(animator);
                    break;
                case CCS_EquipmentFitStudioPosePreviewMode.RevolverFireFrame:
                    ApplyRevolverFireFramePose(animator);
                    break;
            }

            EditorApplication.update -= RefreshActivePosePreview;
            EditorApplication.update += RefreshActivePosePreview;
            return true;
        }

        public static void ClearPosePreview(GameObject playerRoot)
        {
            EditorApplication.update -= RefreshActivePosePreview;

            if (animationModeStarted && AnimationMode.InAnimationMode())
            {
                AnimationMode.StopAnimationMode();
            }

            animationModeStarted = false;
            activeMode = CCS_EquipmentFitStudioPosePreviewMode.Neutral;

            if (playerRoot != null)
            {
                Animator animator = FindPlayerAnimator(playerRoot);
                ResetAnimatorToNeutralDefaults(animator);
            }

            if (activePlayerRoot == playerRoot)
            {
                activePlayerRoot = null;
            }
        }

        public static void ClearAllPosePreview()
        {
            ClearPosePreview(activePlayerRoot);
            activePlayerRoot = null;
        }

        public static Animator FindPlayerAnimator(GameObject playerRoot)
        {
            if (playerRoot == null)
            {
                return null;
            }

            Transform visualRoot = FindDeepChild(playerRoot.transform, CCS_EquipmentConstants.VisualRootObjectName);
            return visualRoot != null ? visualRoot.GetComponentInChildren<Animator>(true) : null;
        }

        public static CCS_SurvivalValidationResult ValidatePosePreviewFoundation()
        {
            string utilityPath =
                CCS_CharacterControllerConstants.ModuleRootPath
                + "/Editor/EquipmentFitStudio/CCS_EquipmentFitStudioPosePreviewUtility.cs";
            if (!File.Exists(utilityPath))
            {
                return CCS_SurvivalValidationResult.Fail("Missing CCS_EquipmentFitStudioPosePreviewUtility.");
            }

            if (GetDefaultPosePreviewForSocket(CCS_EquipmentConstants.HolsterSocketRightHipId)
                != CCS_EquipmentFitStudioPosePreviewMode.Neutral)
            {
                return CCS_SurvivalValidationResult.Fail("Right hip holster must default to Neutral pose preview.");
            }

            if (GetDefaultPosePreviewForSocket(CCS_EquipmentConstants.HandSocketRightId)
                != CCS_EquipmentFitStudioPosePreviewMode.RevolverAim)
            {
                return CCS_SurvivalValidationResult.Fail("Right hand equipped must default to Revolver Aim pose preview.");
            }

            if (!File.Exists(CCS_CharacterControllerConstants.RevolverAimIdleClipPath))
            {
                return CCS_SurvivalValidationResult.Fail("Missing CCS_Revolver_AimIdle_UpperBody.anim for pose preview.");
            }

            if (!File.Exists(CCS_CharacterControllerConstants.PlayerLocomotionAnimatorControllerPath))
            {
                return CCS_SurvivalValidationResult.Fail("Missing player locomotion animator controller for pose preview.");
            }

            return CCS_SurvivalValidationResult.Pass("Equipment Fit Studio pose preview foundation validated.");
        }

        private static void RefreshActivePosePreview()
        {
            if (activePlayerRoot == null
                || activeMode == CCS_EquipmentFitStudioPosePreviewMode.Neutral
                || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            if (!AnimationMode.InAnimationMode())
            {
                EnsureAnimationModeStarted();
            }

            Animator animator = FindPlayerAnimator(activePlayerRoot);
            if (animator == null)
            {
                return;
            }

            switch (activeMode)
            {
                case CCS_EquipmentFitStudioPosePreviewMode.RevolverAim:
                    ApplyRevolverAimPose(animator);
                    break;
                case CCS_EquipmentFitStudioPosePreviewMode.RevolverFireFrame:
                    ApplyRevolverFireFramePose(animator);
                    break;
            }
        }

        private static void EnsureAnimationModeStarted()
        {
            if (!AnimationMode.InAnimationMode())
            {
                AnimationMode.StartAnimationMode();
                animationModeStarted = true;
            }
        }

        private static void ApplyRevolverAimPose(Animator animator)
        {
            if (animator == null)
            {
                return;
            }

            PrepareAnimatorForPreview(animator);
            ResetRevolverTriggers(animator);

            animator.SetBool(IsAimingMovementModeHash, true);
            animator.SetFloat(AimMoveXHash, 0f);
            animator.SetFloat(AimMoveYHash, 0f);
            animator.SetBool(RevolverAimHeldHash, true);
            animator.SetBool(RevolverIsReloadingHash, false);

            int revolverLayerIndex = animator.GetLayerIndex(
                CCS_CharacterControllerConstants.AnimatorRevolverUpperBodyLayerName);
            if (revolverLayerIndex >= 0)
            {
                animator.SetLayerWeight(revolverLayerIndex, 1f);
                animator.Play(
                    CCS_CharacterControllerConstants.AnimatorRevolverAimIdleStateName,
                    revolverLayerIndex,
                    0f);
            }

            animator.Play(CCS_CharacterControllerConstants.AnimatorAimStrafeLocomotionStateName, 0, 0f);
            animator.Update(0.001f);
        }

        private static void ApplyRevolverFireFramePose(Animator animator)
        {
            ApplyRevolverAimPose(animator);

            int revolverLayerIndex = animator.GetLayerIndex(
                CCS_CharacterControllerConstants.AnimatorRevolverUpperBodyLayerName);
            if (revolverLayerIndex >= 0)
            {
                animator.Play(
                    CCS_CharacterControllerConstants.AnimatorRevolverFireStateName,
                    revolverLayerIndex,
                    RevolverFireFrameNormalizedTime);
                animator.Update(0.001f);
            }
        }

        private static void ResetAnimatorToNeutralDefaults(Animator animator)
        {
            if (animator == null)
            {
                return;
            }

            PrepareAnimatorForPreview(animator);
            ResetRevolverTriggers(animator);

            animator.SetBool(IsAimingMovementModeHash, false);
            animator.SetFloat(AimMoveXHash, 0f);
            animator.SetFloat(AimMoveYHash, 0f);
            animator.SetBool(RevolverAimHeldHash, false);
            animator.SetBool(RevolverIsReloadingHash, false);

            int revolverLayerIndex = animator.GetLayerIndex(
                CCS_CharacterControllerConstants.AnimatorRevolverUpperBodyLayerName);
            if (revolverLayerIndex >= 0)
            {
                animator.SetLayerWeight(revolverLayerIndex, 0f);
            }

            animator.Play("Idle", 0, 0f);
            animator.Update(0.001f);
        }

        private static void PrepareAnimatorForPreview(Animator animator)
        {
            animator.enabled = true;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            animator.fireEvents = false;
            animator.keepAnimatorStateOnDisable = true;
        }

        private static void ResetRevolverTriggers(Animator animator)
        {
            animator.ResetTrigger(CCS_CharacterControllerConstants.AnimatorRevolverFireTriggerParameter);
            animator.ResetTrigger(CCS_CharacterControllerConstants.AnimatorRevolverReloadTriggerParameter);
        }

        private static Transform FindDeepChild(Transform root, string childName)
        {
            if (root == null)
            {
                return null;
            }

            if (root.name == childName)
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform match = FindDeepChild(root.GetChild(i), childName);
                if (match != null)
                {
                    return match;
                }
            }

            return null;
        }
    }
}
