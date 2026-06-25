using CCS.Modules.CharacterController;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_RevolverBodyAimFollowController
// CATEGORY: Modules / Weapons / Runtime / Aiming
// PURPOSE: Limited additive spine/chest rotation toward camera aim while revolver aiming.
// PLACEMENT: PF_CCS_CharacterController_TestPlayer_Networked / VisualRoot.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Body follows aim direction; arm stays on FitTest pose. No hand IK stretching.
// =============================================================================

namespace CCS.Modules.Weapons
{
    [DefaultExecutionOrder(280)]
    public sealed class CCS_RevolverBodyAimFollowController : MonoBehaviour
    {
        #region Variables

        [SerializeField] private bool enableBodyAimFollow = true;
        [SerializeField] private Animator animator;
        [SerializeField] private Component revolverAnimationStateComponent;
        [SerializeField] private CCS_CharacterCameraFollowAnchor cameraFollowAnchor;
        [SerializeField] private float followSmoothing = 14f;
        [SerializeField] private float chestYawMaxDegrees = 25f;
        [SerializeField] private float upperChestYawMaxDegrees = 15f;
        [SerializeField] private float chestPitchMaxUpDegrees = 35f;
        [SerializeField] private float chestPitchMaxDownDegrees = 25f;
        [SerializeField] private float upperChestPitchMaxUpDegrees = 25f;
        [SerializeField] private float upperChestPitchMaxDownDegrees = 15f;
        [SerializeField] private float spineYawWeight = 0.25f;
        [SerializeField] private float spinePitchWeight = 0.20f;
        [SerializeField] private float chestYawWeight = 0.55f;
        [SerializeField] private float chestPitchWeight = 0.50f;
        [SerializeField] private float upperChestYawWeight = 0.75f;
        [SerializeField] private float upperChestPitchWeight = 0.65f;

        private CCS_IRevolverAnimationState revolverAnimationState;
        private Transform spineBone;
        private Transform chestBone;
        private Transform upperChestBone;
        private float smoothedYawOffset;
        private float smoothedPitchOffset;

        #endregion

        #region Properties

        public bool EnableBodyAimFollow => enableBodyAimFollow;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            ResolveReferences();
        }

        private void LateUpdate()
        {
            if (!enableBodyAimFollow || animator == null || !animator.isHuman)
            {
                ResetSmoothedOffsets();
                return;
            }

            ResolveReferences();
            if (!ShouldApplyBodyAimFollow())
            {
                ResetSmoothedOffsets();
                return;
            }

            Camera aimCamera = ResolveAimCamera();
            if (aimCamera == null || cameraFollowAnchor == null || cameraFollowAnchor.BodyRoot == null)
            {
                return;
            }

            ComputeTargetOffsets(
                aimCamera.transform,
                cameraFollowAnchor.BodyRoot,
                out float targetYawOffset,
                out float targetPitchOffset);

            float smoothFactor = 1f - Mathf.Exp(-followSmoothing * Time.deltaTime);
            smoothedYawOffset = Mathf.Lerp(smoothedYawOffset, targetYawOffset, smoothFactor);
            smoothedPitchOffset = Mathf.Lerp(smoothedPitchOffset, targetPitchOffset, smoothFactor);

            ApplyBoneOffset(spineBone, smoothedYawOffset * spineYawWeight, smoothedPitchOffset * spinePitchWeight);
            ApplyBoneOffset(chestBone, smoothedYawOffset * chestYawWeight, smoothedPitchOffset * chestPitchWeight);
            ApplyBoneOffset(
                upperChestBone,
                smoothedYawOffset * upperChestYawWeight,
                smoothedPitchOffset * upperChestPitchWeight);
        }

        #endregion

        #region Public Methods

        public void SetBodyAimFollowEnabled(bool enabled)
        {
            enableBodyAimFollow = enabled;
            if (!enabled)
            {
                ResetSmoothedOffsets();
            }
        }

        #endregion

        #region Private Methods

        private void ResolveReferences()
        {
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>(true);
            }

            if (cameraFollowAnchor == null)
            {
                cameraFollowAnchor = GetComponentInParent<CCS_CharacterCameraFollowAnchor>();
            }

            if (revolverAnimationState == null)
            {
                if (revolverAnimationStateComponent is CCS_IRevolverAnimationState fromComponent)
                {
                    revolverAnimationState = fromComponent;
                }
                else
                {
                    revolverAnimationState = GetComponentInParent<CCS_IRevolverAnimationState>();
                }
            }

            if (animator != null && animator.isHuman)
            {
                if (spineBone == null)
                {
                    spineBone = animator.GetBoneTransform(HumanBodyBones.Spine);
                }

                if (chestBone == null)
                {
                    chestBone = animator.GetBoneTransform(HumanBodyBones.Chest);
                }

                if (upperChestBone == null)
                {
                    upperChestBone = animator.GetBoneTransform(HumanBodyBones.UpperChest);
                    if (upperChestBone == null)
                    {
                        upperChestBone = chestBone;
                    }
                }
            }
        }

        private bool ShouldApplyBodyAimFollow()
        {
            return revolverAnimationState != null
                && revolverAnimationState.IsRevolverOwned
                && revolverAnimationState.RevolverAimHeld
                && !revolverAnimationState.RevolverIsReloading;
        }

        private void ComputeTargetOffsets(
            Transform cameraTransform,
            Transform bodyRoot,
            out float yawOffsetDegrees,
            out float pitchOffsetDegrees)
        {
            Vector3 cameraForward = cameraTransform.forward;
            Vector3 bodyForward = bodyRoot.forward;
            Vector3 upAxis = bodyRoot.up;

            Vector3 cameraPlanar = Vector3.ProjectOnPlane(cameraForward, upAxis);
            Vector3 bodyPlanar = Vector3.ProjectOnPlane(bodyForward, upAxis);
            if (cameraPlanar.sqrMagnitude <= 0.0001f || bodyPlanar.sqrMagnitude <= 0.0001f)
            {
                yawOffsetDegrees = 0f;
                pitchOffsetDegrees = 0f;
                return;
            }

            cameraPlanar.Normalize();
            bodyPlanar.Normalize();
            yawOffsetDegrees = Mathf.Clamp(
                Vector3.SignedAngle(bodyPlanar, cameraPlanar, upAxis),
                -chestYawMaxDegrees,
                chestYawMaxDegrees);

            Vector3 rightAxis = bodyRoot.right;
            float cameraPitch = Vector3.SignedAngle(cameraPlanar, cameraForward, rightAxis);
            pitchOffsetDegrees = Mathf.Clamp(
                cameraPitch,
                -chestPitchMaxDownDegrees,
                chestPitchMaxUpDegrees);
        }

        private static void ApplyBoneOffset(Transform bone, float yawDegrees, float pitchDegrees)
        {
            if (bone == null)
            {
                return;
            }

            if (Mathf.Abs(yawDegrees) <= 0.01f && Mathf.Abs(pitchDegrees) <= 0.01f)
            {
                return;
            }

            Quaternion animatedRotation = bone.localRotation;
            Quaternion offset = Quaternion.Euler(pitchDegrees, yawDegrees, 0f);
            bone.localRotation = animatedRotation * offset;
        }

        private void ResetSmoothedOffsets()
        {
            smoothedYawOffset = 0f;
            smoothedPitchOffset = 0f;
        }

        private static Camera ResolveAimCamera()
        {
            return CCS_CharacterMovementCameraContext.HasActiveCamera
                ? CCS_CharacterMovementCameraContext.ActiveCamera
                : Camera.main;
        }

        #endregion
    }
}
