using CCS.Modules.CharacterController;
using UnityEditor;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_RevolverAimPreviewPoseUtility
// CATEGORY: Modules / CharacterController / Editor / Common
// PURPOSE: Shared editor-only Revolver Aim pose used by Equipment Fit Studio and Animation Fit Studio.
// PLACEMENT: Editor/Common shared pose utility.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Uses player locomotion Animator Controller. Does not modify assets.
// =============================================================================

namespace CCS.Modules.CharacterController.Editor.Common
{
    public static class CCS_RevolverAimPreviewPoseUtility
    {
        public const string DisplayLabel = "Equipment Fit Studio — Revolver Aim";

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

        public static void EnsureAnimationMode()
        {
            if (!AnimationMode.InAnimationMode())
            {
                AnimationMode.StartAnimationMode();
            }
        }

        public static void ApplyRevolverAimPose(Animator animator)
        {
            if (animator == null)
            {
                return;
            }

            EnsureAnimationMode();
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

        public static void PrepareAnimatorForPreview(Animator animator)
        {
            animator.enabled = true;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            animator.fireEvents = false;
            animator.keepAnimatorStateOnDisable = true;
            animator.speed = 1f;
        }

        private static void ResetRevolverTriggers(Animator animator)
        {
            animator.ResetTrigger(CCS_CharacterControllerConstants.AnimatorRevolverFireTriggerParameter);
            animator.ResetTrigger(CCS_CharacterControllerConstants.AnimatorRevolverReloadTriggerParameter);
        }
    }
}
