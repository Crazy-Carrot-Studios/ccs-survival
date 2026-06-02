using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CharacterCameraController
// CATEGORY: Modules / CharacterController / Runtime / Camera
// PURPOSE: Applies smoothed look input to yaw/pitch targets for third-person Cinemachine follow.
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
        private Vector2 smoothedLookInput;
        private float lastLookInputMagnitude;

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
            smoothedLookInput = Vector2.zero;
            lastLookInputMagnitude = 0f;
            lookState.SetOrientation(initialYawDegrees, initialPitchDegrees);
            ApplyOrientationToTransforms();
        }

        public void TickLook(CCS_CharacterInputSnapshot input, float deltaTime)
        {
            if (activeProfile == null || deltaTime <= 0f)
            {
                return;
            }

            if (!activeProfile.IsThirdPersonSurvivalActive && !activeProfile.IsHorseModeActive)
            {
                return;
            }

            Vector2 rawLook = input.Look;
            float lookSmoothing = Mathf.Max(0f, activeProfile.LookSmoothing);
            if (lookSmoothing > 0f)
            {
                float blend = 1f - Mathf.Exp(-lookSmoothing * deltaTime);
                smoothedLookInput = Vector2.Lerp(smoothedLookInput, rawLook, blend);
            }
            else
            {
                smoothedLookInput = rawLook;
            }

            lastLookInputMagnitude = Mathf.Max(Mathf.Abs(smoothedLookInput.x), Mathf.Abs(smoothedLookInput.y));
            bool isPointerLook = lastLookInputMagnitude > activeProfile.PointerLookThreshold;

            float yawDelta;
            float pitchDelta;
            if (isPointerLook)
            {
                yawDelta = smoothedLookInput.x * activeProfile.MouseSensitivityX;
                pitchDelta = -smoothedLookInput.y * activeProfile.MouseSensitivityY;
            }
            else
            {
                yawDelta = smoothedLookInput.x * activeProfile.GamepadSensitivityX * deltaTime;
                pitchDelta = -smoothedLookInput.y * activeProfile.GamepadSensitivityY * deltaTime;
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

        public float LastLookInputMagnitude => lastLookInputMagnitude;

        #endregion
    }
}
