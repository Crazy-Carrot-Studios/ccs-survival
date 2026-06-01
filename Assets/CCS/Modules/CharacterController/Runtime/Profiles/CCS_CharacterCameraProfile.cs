using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CharacterCameraProfile
// CATEGORY: Modules / CharacterController / Runtime / Profiles
// PURPOSE: Profile-driven camera modes, third-person tuning, and future mode placeholders.
// PLACEMENT: Embedded on CCS_CharacterControllerProfile.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Cinemachine 3.1 applies ThirdPersonSurvival values via survival camera driver.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    [System.Serializable]
    public sealed class CCS_CharacterCameraProfile
    {
        #region Variables

        [Header("Camera Mode")]
        [Tooltip("Active camera mode. Only ThirdPersonSurvival is implemented in 1.1.5.")]
        [SerializeField] private CCS_CharacterCameraMode activeCameraMode = CCS_CharacterCameraMode.ThirdPersonSurvival;

        [Header("Mouse Look")]
        [Tooltip("Yaw degrees applied per unit of horizontal mouse delta.")]
        [SerializeField] private float mouseSensitivityX = 0.1f;

        [Tooltip("Pitch degrees applied per unit of vertical mouse delta.")]
        [SerializeField] private float mouseSensitivityY = 0.085f;

        [Header("Gamepad Look")]
        [Tooltip("Yaw degrees per second per unit of horizontal stick input.")]
        [SerializeField] private float gamepadSensitivityX = 85f;

        [Tooltip("Pitch degrees per second per unit of vertical stick input.")]
        [SerializeField] private float gamepadSensitivityY = 65f;

        [Header("Look Smoothing")]
        [Tooltip("Pointer/gamepad look smoothing strength. Higher = smoother, slower response.")]
        [SerializeField] private float lookSmoothing = 14f;

        [Header("Pitch Clamp")]
        [Tooltip("Minimum pitch angle in degrees (looking down).")]
        [SerializeField] private float minPitch = -35f;

        [Tooltip("Maximum pitch angle in degrees (looking up).")]
        [SerializeField] private float maxPitch = 55f;

        [Header("Follow Targets")]
        [Tooltip("World-space height of the yaw pivot above the character root.")]
        [SerializeField] private float pivotHeight = 1.35f;

        [Tooltip("Local height offset of the look target above the yaw pivot.")]
        [SerializeField] private float lookTargetHeight = 0.25f;

        [Header("Third Person (Cinemachine)")]
        [Tooltip("Default follow distance behind the character.")]
        [SerializeField] private float cameraDistance = 4.75f;

        [Tooltip("Shoulder offset in target-local space.")]
        [SerializeField] private Vector3 shoulderOffset = new Vector3(0.5f, 0.12f, 0f);

        [Tooltip("Vertical arm length from shoulder to look point.")]
        [SerializeField] private float verticalArmLength = 0.42f;

        [Tooltip("Camera side: 0 center, 1 right shoulder, -1 left shoulder.")]
        [SerializeField] private float cameraSide = 1f;

        [Tooltip("Approximate follow damping per axis (camera-local). Higher = smoother follow.")]
        [SerializeField] private Vector3 followDamping = new Vector3(0.28f, 0.32f, 0.28f);

        [Header("Future Mode Placeholders")]
        [Tooltip("Reserved minimum zoom distance for future aim/top-down modes.")]
        [SerializeField] private float zoomDistanceMin = 2.5f;

        [Tooltip("Reserved maximum zoom distance for future aim/top-down modes.")]
        [SerializeField] private float zoomDistanceMax = 8f;

        [Tooltip("Reserved aim/mode transition speed for future camera mode switching.")]
        [SerializeField] private float aimTransitionSpeed = 6f;

        [Header("Obstacle Avoidance")]
        [Tooltip("Pull gameplay camera in front of occluding geometry when enabled.")]
        [SerializeField] private bool enableObstacleAvoidance = true;

        [Tooltip("Sphere radius used for third-person obstacle resolution.")]
        [SerializeField] private float obstacleCameraRadius = 0.25f;

        [Tooltip("Layer mask for camera obstacle casts.")]
        [SerializeField] private LayerMask obstacleLayerMask = ~0;

        [Tooltip("Obstacles with this tag are ignored by camera collision.")]
        [SerializeField] private string obstacleIgnoreTag = "Player";

        [Tooltip("Damping when camera moves into collision.")]
        [SerializeField] private float obstacleDampingIntoCollision = 0.35f;

        [Tooltip("Damping when camera recovers from collision.")]
        [SerializeField] private float obstacleDampingFromCollision = 0.55f;

        [Header("Pointer Detection")]
        [Tooltip("Input magnitude above this value is treated as pointer delta instead of stick input.")]
        [SerializeField] private float pointerLookThreshold = 1f;

        #endregion

        #region Properties

        public CCS_CharacterCameraMode ActiveCameraMode => activeCameraMode;

        public float MouseSensitivityX => mouseSensitivityX;

        public float MouseSensitivityY => mouseSensitivityY;

        public float GamepadSensitivityX => gamepadSensitivityX;

        public float GamepadSensitivityY => gamepadSensitivityY;

        public float LookSmoothing => lookSmoothing;

        public float MinPitch => minPitch;

        public float MaxPitch => maxPitch;

        public float PivotHeight => pivotHeight;

        public float LookTargetHeight => lookTargetHeight;

        public float CameraDistance => cameraDistance;

        public Vector3 ShoulderOffset => shoulderOffset;

        public float VerticalArmLength => verticalArmLength;

        public float CameraSide => cameraSide;

        public Vector3 FollowDamping => followDamping;

        public float ZoomDistanceMin => zoomDistanceMin;

        public float ZoomDistanceMax => zoomDistanceMax;

        public float AimTransitionSpeed => aimTransitionSpeed;

        public bool EnableObstacleAvoidance => enableObstacleAvoidance;

        public float ObstacleCameraRadius => obstacleCameraRadius;

        public LayerMask ObstacleLayerMask => obstacleLayerMask;

        public string ObstacleIgnoreTag => obstacleIgnoreTag;

        public float ObstacleDampingIntoCollision => obstacleDampingIntoCollision;

        public float ObstacleDampingFromCollision => obstacleDampingFromCollision;

        public float PointerLookThreshold => pointerLookThreshold;

        public bool IsThirdPersonSurvivalActive => activeCameraMode == CCS_CharacterCameraMode.ThirdPersonSurvival;

        #endregion
    }
}
