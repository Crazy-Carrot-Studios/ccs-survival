using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CharacterControllerState
// CATEGORY: Modules / CharacterController / Runtime / Data
// PURPOSE: Mutable runtime state owned by the character controller service.
// PLACEMENT: Runtime data. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Updated each frame by service from motor, input, and camera components.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public sealed class CCS_CharacterControllerState
    {
        #region Variables

        public CCS_CharacterMovementMode MovementMode = CCS_CharacterMovementMode.GroundedThirdPerson;

        public CCS_CharacterCameraMode CameraMode = CCS_CharacterCameraMode.ThirdPersonSurvival;

        public bool IsGrounded;

        public float CurrentSpeed;

        public float TargetSpeed;

        public bool IsSprinting;

        public string InputDeviceLabel = "None";

        public Vector2 MovementInput;

        public Vector2 LookInput;

        public float Yaw;

        public float Pitch;

        public Vector3 PlayerPosition;

        public Vector3 CameraForward = Vector3.forward;

        public string ActiveCameraProfileName = string.Empty;

        public float ActiveMouseSensitivityX;

        public float ActiveMouseSensitivityY;

        #endregion

        #region Public Methods

        public CCS_CharacterControllerSnapshot ToSnapshot()
        {
            return new CCS_CharacterControllerSnapshot(
                MovementMode,
                CameraMode,
                IsGrounded,
                CurrentSpeed,
                TargetSpeed,
                IsSprinting,
                InputDeviceLabel,
                MovementInput,
                LookInput,
                Yaw,
                Pitch,
                PlayerPosition,
                CameraForward,
                ActiveCameraProfileName,
                ActiveMouseSensitivityX,
                ActiveMouseSensitivityY);
        }

        #endregion
    }
}
