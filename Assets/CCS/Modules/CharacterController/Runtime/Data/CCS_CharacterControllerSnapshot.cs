using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CharacterControllerSnapshot
// CATEGORY: Modules / CharacterController / Runtime / Data
// PURPOSE: Read-only snapshot of character controller state for external modules.
// PLACEMENT: Runtime data struct. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Immutable view for future inventory, interaction, and UI consumers.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public readonly struct CCS_CharacterControllerSnapshot
    {
        #region Public Methods

        public CCS_CharacterControllerSnapshot(
            CCS_CharacterMovementMode movementMode,
            CCS_CharacterCameraMode cameraMode,
            bool isGrounded,
            float currentSpeed,
            float targetSpeed,
            bool isSprinting,
            string inputDeviceLabel,
            Vector2 movementInput,
            Vector2 lookInput,
            float yaw,
            float pitch,
            Vector3 playerPosition,
            Vector3 cameraForward,
            string activeCameraProfileName,
            float activeMouseSensitivityX,
            float activeMouseSensitivityY,
            string activeCinemachineRigDescription)
        {
            MovementMode = movementMode;
            CameraMode = cameraMode;
            IsGrounded = isGrounded;
            CurrentSpeed = currentSpeed;
            TargetSpeed = targetSpeed;
            IsSprinting = isSprinting;
            InputDeviceLabel = inputDeviceLabel ?? "None";
            MovementInput = movementInput;
            LookInput = lookInput;
            Yaw = yaw;
            Pitch = pitch;
            PlayerPosition = playerPosition;
            CameraForward = cameraForward.sqrMagnitude > 0.0001f ? cameraForward.normalized : Vector3.forward;
            ActiveCameraProfileName = activeCameraProfileName ?? string.Empty;
            ActiveMouseSensitivityX = activeMouseSensitivityX;
            ActiveMouseSensitivityY = activeMouseSensitivityY;
            ActiveCinemachineRigDescription = activeCinemachineRigDescription ?? string.Empty;
        }

        #endregion

        #region Properties

        public CCS_CharacterMovementMode MovementMode { get; }

        public CCS_CharacterCameraMode CameraMode { get; }

        public bool IsGrounded { get; }

        public float CurrentSpeed { get; }

        public float TargetSpeed { get; }

        public bool IsSprinting { get; }

        public string InputDeviceLabel { get; }

        public Vector2 MovementInput { get; }

        public Vector2 LookInput { get; }

        public float Yaw { get; }

        public float Pitch { get; }

        public Vector3 PlayerPosition { get; }

        public Vector3 CameraForward { get; }

        public string ActiveCameraProfileName { get; }

        public float ActiveMouseSensitivityX { get; }

        public float ActiveMouseSensitivityY { get; }

        public string ActiveCinemachineRigDescription { get; }

        #endregion
    }
}
