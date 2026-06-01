using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CharacterCameraProfile
// CATEGORY: Modules / CharacterController / Runtime / Profiles
// PURPOSE: Third-person look sensitivity, pitch clamp, and Cinemachine follow tuning.
// PLACEMENT: Embedded on CCS_CharacterControllerProfile.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Cinemachine 3.1 third-person follow values are applied by survival camera driver.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    [System.Serializable]
    public sealed class CCS_CharacterCameraProfile
    {
        #region Variables

        [Header("Mouse Look")]
        [Tooltip("Yaw degrees applied per unit of horizontal mouse delta.")]
        [SerializeField] private float mouseSensitivityX = 0.12f;

        [Tooltip("Pitch degrees applied per unit of vertical mouse delta.")]
        [SerializeField] private float mouseSensitivityY = 0.1f;

        [Header("Gamepad Look")]
        [Tooltip("Yaw degrees per second per unit of horizontal stick input.")]
        [SerializeField] private float gamepadSensitivityX = 90f;

        [Tooltip("Pitch degrees per second per unit of vertical stick input.")]
        [SerializeField] private float gamepadSensitivityY = 70f;

        [Header("Pitch Clamp")]
        [Tooltip("Minimum pitch angle in degrees (looking down).")]
        [SerializeField] private float minPitch = -35f;

        [Tooltip("Maximum pitch angle in degrees (looking up).")]
        [SerializeField] private float maxPitch = 60f;

        [Header("Follow Targets")]
        [Tooltip("World-space height of the yaw pivot above the character root.")]
        [SerializeField] private float pivotHeight = 1.35f;

        [Tooltip("Local height offset of the look target above the yaw pivot.")]
        [SerializeField] private float lookTargetHeight = 0.25f;

        [Header("Third Person (Cinemachine)")]
        [Tooltip("Default follow distance behind the character.")]
        [SerializeField] private float cameraDistance = 4.5f;

        [Tooltip("Shoulder offset in target-local space.")]
        [SerializeField] private Vector3 shoulderOffset = new Vector3(0.55f, 0.1f, 0f);

        [Tooltip("Vertical arm length from shoulder to look point.")]
        [SerializeField] private float verticalArmLength = 0.4f;

        [Tooltip("Camera side: 0 center, 1 right shoulder, -1 left shoulder.")]
        [SerializeField] private float cameraSide = 1f;

        [Tooltip("Approximate follow damping per axis (camera-local). Smaller = snappier.")]
        [SerializeField] private Vector3 followDamping = new Vector3(0.2f, 0.25f, 0.2f);

        [Header("Pointer Detection")]
        [Tooltip("Input magnitude above this value is treated as pointer delta instead of stick input.")]
        [SerializeField] private float pointerLookThreshold = 1f;

        #endregion

        #region Properties

        public float MouseSensitivityX => mouseSensitivityX;

        public float MouseSensitivityY => mouseSensitivityY;

        public float GamepadSensitivityX => gamepadSensitivityX;

        public float GamepadSensitivityY => gamepadSensitivityY;

        public float MinPitch => minPitch;

        public float MaxPitch => maxPitch;

        public float PivotHeight => pivotHeight;

        public float LookTargetHeight => lookTargetHeight;

        public float CameraDistance => cameraDistance;

        public Vector3 ShoulderOffset => shoulderOffset;

        public float VerticalArmLength => verticalArmLength;

        public float CameraSide => cameraSide;

        public Vector3 FollowDamping => followDamping;

        public float PointerLookThreshold => pointerLookThreshold;

        #endregion
    }
}
