using CCS.Project;
using UnityEngine;
using UnityEngine.Serialization;

// =============================================================================
// SCRIPT: CCS_CharacterCameraProfile
// CATEGORY: Modules / CharacterController / Runtime / Profiles
// PURPOSE: Profile-driven Cinemachine Orbital Follow + Rotation Composer tuning.
// PLACEMENT: ScriptableObject asset under Profiles/Camera/.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Look input gains feed editor rig wiring only. Runtime look is Cinemachine-owned.
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

        [Header("Orbital Follow Rig")]
        [Tooltip("Height offset for the spawned CameraPivot transform.")]
        [SerializeField] private float followTargetHeight = 1.05f;

        [FormerlySerializedAs("cameraDistance")]
        [Tooltip("Cinemachine Orbital Follow sphere radius.")]
        [SerializeField] private float orbitalRadius = 4.5f;

        [Tooltip("Shoulder offset Y applied to Orbital Follow target offset.")]
        [SerializeField] private float cameraHeight = 0.12f;

        [Tooltip("Shoulder offset in target-local space.")]
        [SerializeField] private Vector3 cameraShoulderOffset = new Vector3(0.45f, 0f, 0f);

        [Tooltip("Legacy third-person arm length. Reserved for future rig variants.")]
        [SerializeField] private float verticalArmLength = 0.25f;

        [Tooltip("Shoulder side. 1 = right, -1 = left.")]
        [SerializeField] private float cameraSide = 1f;

        [Header("Vertical Orbit Limits")]
        [FormerlySerializedAs("defaultPitch")]
        [Tooltip("Editor/builder neutral vertical orbit hint. Runtime look is Cinemachine-owned.")]
        [SerializeField] private float verticalOrbitDefault;

        [FormerlySerializedAs("minPitch")]
        [Tooltip("Cinemachine Orbital Follow vertical orbit minimum in degrees.")]
        [SerializeField] private float verticalOrbitMin = -35f;

        [FormerlySerializedAs("maxPitch")]
        [Tooltip("Cinemachine Orbital Follow vertical orbit maximum in degrees.")]
        [SerializeField] private float verticalOrbitMax = 60f;

        [Header("Look Sensitivity")]
        [Tooltip("Input Axis Controller mouse gain for Look Orbit X.")]
        [SerializeField] private float mouseSensitivityX = 0.12f;

        [Tooltip("Input Axis Controller mouse gain magnitude for Look Orbit Y.")]
        [SerializeField] private float mouseSensitivityY = 0.1f;

        [Tooltip("Input Axis Controller gamepad gain for Look Orbit X.")]
        [SerializeField] private float gamepadSensitivityX = 90f;

        [Tooltip("Input Axis Controller gamepad gain magnitude for Look Orbit Y.")]
        [SerializeField] private float gamepadSensitivityY = 70f;

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

        public float FollowTargetHeight => followTargetHeight;

        public float OrbitalRadius => orbitalRadius;

        public float CameraHeight => cameraHeight;

        public Vector3 CameraShoulderOffset => cameraShoulderOffset;

        public float VerticalArmLength => verticalArmLength;

        public float CameraSide => cameraSide;

        public float VerticalOrbitDefault => verticalOrbitDefault;

        public float VerticalOrbitMin => verticalOrbitMin;

        public float VerticalOrbitMax => verticalOrbitMax;

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
