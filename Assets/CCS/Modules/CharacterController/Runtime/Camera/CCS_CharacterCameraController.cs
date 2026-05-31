using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CharacterCameraController
// CATEGORY: Modules / CharacterController / Runtime / Camera
// PURPOSE: Applies look input to yaw/pitch and provides follow/look hooks for a camera transform.
// PLACEMENT: Used by CCS_CharacterMovementService.TickMovement or dedicated look tick.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: No collision, Cinemachine, or final polish in 0.3.8.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public sealed class CCS_CharacterCameraController
    {
        #region Variables

        private readonly CCS_CharacterLookState lookState = new CCS_CharacterLookState();
        private Transform followTarget;
        private Transform cameraTransform;
        private CCS_CharacterCameraProfile activeProfile;

        #endregion

        #region Public Methods

        public void Initialize(
            Transform follow,
            Transform camera,
            CCS_CharacterCameraProfile profile,
            float initialYawDegrees = 0f,
            float initialPitchDegrees = 0f)
        {
            followTarget = follow;
            cameraTransform = camera;
            activeProfile = profile;
            lookState.SetOrientation(initialYawDegrees, initialPitchDegrees);
        }

        public void TickLook(CCS_CharacterInputSnapshot input, float deltaTime)
        {
            if (activeProfile == null || deltaTime <= 0f)
            {
                return;
            }

            float lookMagnitude = Mathf.Max(Mathf.Abs(input.Look.x), Mathf.Abs(input.Look.y));
            bool isPointerLook = lookMagnitude > activeProfile.PointerLookThreshold;
            float yawDelta = input.Look.x * activeProfile.HorizontalSensitivity;
            float pitchDelta = -input.Look.y * activeProfile.VerticalSensitivity;

            if (isPointerLook)
            {
                yawDelta *= activeProfile.PointerLookScale;
                pitchDelta *= activeProfile.PointerLookScale;
            }
            else
            {
                yawDelta *= deltaTime;
                pitchDelta *= deltaTime;
            }

            lookState.AddLookDelta(yawDelta, pitchDelta, activeProfile.MinPitch, activeProfile.MaxPitch);

            ApplyOrientationToTransforms();
        }

        public void ApplyOrientationToTransforms()
        {
            if (followTarget != null)
            {
                followTarget.rotation = Quaternion.Euler(0f, lookState.YawDegrees, 0f);
            }

            if (cameraTransform != null)
            {
                cameraTransform.rotation = Quaternion.Euler(lookState.PitchDegrees, lookState.YawDegrees, 0f);

                if (followTarget != null)
                {
                    Vector3 anchor = followTarget.position
                        + Vector3.up * (activeProfile != null ? activeProfile.FollowHeightOffset : 0f);
                    cameraTransform.position = anchor;
                }
            }
        }

        #endregion

        #region Properties

        public CCS_CharacterLookState LookState => lookState;

        public float YawDegrees => lookState.YawDegrees;

        #endregion
    }
}
