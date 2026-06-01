using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CharacterCameraController
// CATEGORY: Modules / CharacterController / Runtime / Camera
// PURPOSE: Applies look input to yaw/pitch targets for third-person Cinemachine follow.
// PLACEMENT: Used by CCS_CharacterMovementService.TickMovement.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Does not position the gameplay camera; Cinemachine drives final framing.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public sealed class CCS_CharacterCameraController
    {
        #region Variables

        private readonly CCS_CharacterLookState lookState = new CCS_CharacterLookState();
        private Transform yawPivot;
        private Transform lookTarget;
        private CCS_CharacterCameraProfile activeProfile;

        #endregion

        #region Public Methods

        public void Initialize(
            Transform yawPivotTransform,
            Transform lookTargetTransform,
            CCS_CharacterCameraProfile profile,
            float initialYawDegrees = 0f,
            float initialPitchDegrees = 0f)
        {
            yawPivot = yawPivotTransform;
            lookTarget = lookTargetTransform != null ? lookTargetTransform : yawPivotTransform;
            activeProfile = profile;
            lookState.SetOrientation(initialYawDegrees, initialPitchDegrees);
            ApplyOrientationToTransforms();
        }

        public void TickLook(CCS_CharacterInputSnapshot input, float deltaTime)
        {
            if (activeProfile == null || deltaTime <= 0f)
            {
                return;
            }

            float lookMagnitude = Mathf.Max(Mathf.Abs(input.Look.x), Mathf.Abs(input.Look.y));
            bool isPointerLook = lookMagnitude > activeProfile.PointerLookThreshold;

            float yawDelta;
            float pitchDelta;
            if (isPointerLook)
            {
                yawDelta = input.Look.x * activeProfile.MouseSensitivityX;
                pitchDelta = -input.Look.y * activeProfile.MouseSensitivityY;
            }
            else
            {
                yawDelta = input.Look.x * activeProfile.GamepadSensitivityX * deltaTime;
                pitchDelta = -input.Look.y * activeProfile.GamepadSensitivityY * deltaTime;
            }

            lookState.AddLookDelta(yawDelta, pitchDelta, activeProfile.MinPitch, activeProfile.MaxPitch);
            ApplyOrientationToTransforms();
        }

        public void ApplyOrientationToTransforms()
        {
            if (yawPivot != null)
            {
                yawPivot.rotation = Quaternion.Euler(0f, lookState.YawDegrees, 0f);
            }

            if (lookTarget != null)
            {
                lookTarget.localRotation = Quaternion.Euler(lookState.PitchDegrees, 0f, 0f);
            }
        }

        #endregion

        #region Properties

        public CCS_CharacterLookState LookState => lookState;

        public float YawDegrees => lookState.YawDegrees;

        public Transform LookTarget => lookTarget;

        #endregion
    }
}
