using CCS.Project;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CharacterCameraProfile
// CATEGORY: Modules / CharacterController / Runtime / Profiles
// PURPOSE: Profile-driven Cinemachine third-person camera tuning.
// PLACEMENT: ScriptableObject asset under Profiles/Camera/.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: ThirdPersonSurvival is the only active mode in v0.2.0.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    [CreateAssetMenu(
        fileName = "CCS_CharacterCameraProfile",
        menuName = "CCS/Character Controller/Camera Profile",
        order = 1)]
    public sealed class CCS_CharacterCameraProfile : CCS_SurvivalProfileBase
    {
        #region Variables

        [Header("Camera Mode")]
        [Tooltip("Camera mode represented by this profile.")]
        [SerializeField] private CCS_CharacterCameraMode cameraMode = CCS_CharacterCameraMode.ThirdPersonSurvival;

        [Header("Third Person Rig")]
        [Tooltip("Distance from hand to camera.")]
        [SerializeField] private float cameraDistance = 4.5f;

        [Tooltip("Height offset for follow target.")]
        [SerializeField] private float cameraHeight = 1.4f;

        [Tooltip("Shoulder offset in target-local space.")]
        [SerializeField] private Vector3 cameraShoulderOffset = new Vector3(0.45f, 0f, 0f);

        [Tooltip("Vertical arm length below shoulder pivot.")]
        [SerializeField] private float verticalArmLength = 0.35f;

        [Tooltip("Shoulder side. 1 = right, -1 = left.")]
        [SerializeField] private float cameraSide = 1f;

        [Header("Pitch Limits")]
        [SerializeField] private float minPitch = -35f;

        [SerializeField] private float maxPitch = 55f;

        [Header("Look Sensitivity")]
        [SerializeField] private float mouseSensitivityX = 0.1f;

        [SerializeField] private float mouseSensitivityY = 0.085f;

        [SerializeField] private float gamepadSensitivityX = 85f;

        [SerializeField] private float gamepadSensitivityY = 65f;

        [SerializeField] private float lookSmoothing = 12f;

        [Header("Follow Damping")]
        [SerializeField] private float followDampingX = 0.25f;

        [SerializeField] private float followDampingY = 0.3f;

        [SerializeField] private float followDampingZ = 0.25f;

        [Header("Obstacle Avoidance")]
        [SerializeField] private bool obstacleAvoidanceEnabled = true;

        [SerializeField] private float obstacleAvoidanceRadius = 0.25f;

        [Header("Zoom")]
        [SerializeField] private float zoomDistanceMin = 2.5f;

        [SerializeField] private float zoomDistanceMax = 6f;

        [Header("Transitions")]
        [Tooltip("Placeholder aim transition speed.")]
        [SerializeField] private float aimTransitionSpeed = 8f;

        #endregion

        #region Properties

        public CCS_CharacterCameraMode CameraMode => cameraMode;

        public float CameraDistance => cameraDistance;

        public float CameraHeight => cameraHeight;

        public Vector3 CameraShoulderOffset => cameraShoulderOffset;

        public float VerticalArmLength => verticalArmLength;

        public float CameraSide => cameraSide;

        public float MinPitch => minPitch;

        public float MaxPitch => maxPitch;

        public float MouseSensitivityX => mouseSensitivityX;

        public float MouseSensitivityY => mouseSensitivityY;

        public float GamepadSensitivityX => gamepadSensitivityX;

        public float GamepadSensitivityY => gamepadSensitivityY;

        public float LookSmoothing => lookSmoothing;

        public float FollowDampingX => followDampingX;

        public float FollowDampingY => followDampingY;

        public float FollowDampingZ => followDampingZ;

        public bool ObstacleAvoidanceEnabled => obstacleAvoidanceEnabled;

        public float ObstacleAvoidanceRadius => obstacleAvoidanceRadius;

        public float ZoomDistanceMin => zoomDistanceMin;

        public float ZoomDistanceMax => zoomDistanceMax;

        public float AimTransitionSpeed => aimTransitionSpeed;

        #endregion
    }
}
