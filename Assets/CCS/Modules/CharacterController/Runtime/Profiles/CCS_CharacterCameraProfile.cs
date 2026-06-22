using CCS.Project;

using UnityEngine;

using UnityEngine.Serialization;



// =============================================================================

// SCRIPT: CCS_CharacterCameraProfile

// CATEGORY: Modules / CharacterController / Runtime / Profiles

// PURPOSE: Profile-driven Cinemachine Third Person Follow + aim camera tuning.

// PLACEMENT: ScriptableObject asset under Profiles/Camera/.

// AUTHOR: James Schilz

// CREATED: 2026-06-07

// NOTES: Fields map 1:1 to CinemachineThirdPersonFollow. Shared rig target owns yaw/pitch.

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



        [Header("Tracking Target")]

        [FormerlySerializedAs("followTargetHeight")]

        [Tooltip("Local Y height of CameraPitchTarget under CameraFollowAnchor.")]

        [SerializeField] private float trackingTargetLocalHeight = 1.48f;



        [Header("Third Person Follow Rig")]

        [FormerlySerializedAs("cameraShoulderOffset")]

        [Tooltip("Cinemachine Third Person Follow shoulder offset in tracking-target local space.")]

        [SerializeField] private Vector3 thirdPersonShoulderOffset = new Vector3(0.20f, 0.20f, 0f);



        [FormerlySerializedAs("verticalArmLength")]

        [Tooltip("Cinemachine Third Person Follow vertical arm length.")]

        [SerializeField] private float thirdPersonVerticalArmLength = 0.45f;



        [FormerlySerializedAs("cameraSide")]

        [Tooltip("Cinemachine Third Person Follow camera side. 0 = centered, 1 = right shoulder.")]

        [SerializeField] private float thirdPersonCameraSide;



        [FormerlySerializedAs("orbitalRadius")]

        [FormerlySerializedAs("cameraDistance")]

        [Tooltip("Cinemachine Third Person Follow camera distance.")]

        [SerializeField] private float thirdPersonCameraDistance = 3.0f;



        [Header("Pitch Limits")]

        [FormerlySerializedAs("defaultPitch")]

        [Tooltip("Neutral pitch in degrees when the camera rig initializes.")]

        [SerializeField] private float verticalOrbitDefault;



        [FormerlySerializedAs("minPitch")]

        [Tooltip("Minimum camera pitch in degrees.")]

        [SerializeField] private float verticalOrbitMin = -45f;



        [FormerlySerializedAs("maxPitch")]

        [Tooltip("Maximum camera pitch in degrees.")]

        [SerializeField] private float verticalOrbitMax = 70f;



        [Header("Spawn Orientation")]

        [Tooltip("How camera yaw initializes when the rig binds to a player.")]

        [SerializeField] private CCS_CharacterCameraDefaultYawMode defaultYawMode =

            CCS_CharacterCameraDefaultYawMode.PlayerForward;



        [Header("Look Sensitivity")]

        [Tooltip("Mouse yaw sensitivity multiplier.")]

        [SerializeField] private float mouseSensitivityX = 0.12f;



        [Tooltip("Mouse pitch sensitivity multiplier.")]

        [SerializeField] private float mouseSensitivityY = 0.1f;



        [Tooltip("Gamepad yaw sensitivity multiplier.")]

        [SerializeField] private float gamepadSensitivityX = 90f;



        [Tooltip("Gamepad pitch sensitivity multiplier.")]

        [SerializeField] private float gamepadSensitivityY = 70f;



        [SerializeField] private float lookSmoothing = 12f;



        [Header("Follow Damping")]

        [SerializeField] private float followDampingX = 0.1f;



        [SerializeField] private float followDampingY = 0.12f;



        [SerializeField] private float followDampingZ = 0.1f;



        [Header("Third Person Follow Collision")]

        [SerializeField] private bool obstacleAvoidanceEnabled = true;



        [SerializeField] private LayerMask collisionLayerMask = 1;



        [SerializeField] private string collisionIgnoreTag = "Player";



        [SerializeField] private float obstacleAvoidanceRadius = 0.25f;



        [SerializeField] private float collisionDampingInto = 0.08f;



        [SerializeField] private float collisionDampingFrom = 0.35f;



        [Header("Validation Debug")]

        [Tooltip("When enabled, master test baseline validation runs with obstacle avoidance disabled.")]

        [SerializeField] private bool validationDisableObstacleAvoidanceForBaselinePass = true;



        [Header("Transitions")]

        [Tooltip("Cinemachine Brain blend duration when switching to or from the aim camera.")]

        [SerializeField] private float aimBlendDurationSeconds = 0.45f;



        [Header("Lens")]

        [Tooltip("Cinemachine lens field of view in degrees.")]

        [SerializeField] private float fieldOfView = 62f;



        [Tooltip("Look sensitivity multiplier applied while this profile drives the active aim camera.")]

        [SerializeField] private float aimLookSensitivityMultiplier = 0.85f;



        #endregion



        #region Properties



        public CCS_CharacterCameraMode CameraMode => cameraMode;



        public float TrackingTargetLocalHeight => trackingTargetLocalHeight;



        public Vector3 ThirdPersonShoulderOffset => thirdPersonShoulderOffset;



        public float ThirdPersonVerticalArmLength => thirdPersonVerticalArmLength;



        public float ThirdPersonCameraSide => thirdPersonCameraSide;



        public float ThirdPersonCameraDistance => thirdPersonCameraDistance;



        public float DefaultPitch => verticalOrbitDefault;



        public float VerticalOrbitDefault => verticalOrbitDefault;



        public float MinPitch => verticalOrbitMin;



        public float MaxPitch => verticalOrbitMax;



        public float VerticalOrbitMin => verticalOrbitMin;



        public float VerticalOrbitMax => verticalOrbitMax;



        public CCS_CharacterCameraDefaultYawMode DefaultYawMode => defaultYawMode;



        public float MouseSensitivityX => mouseSensitivityX;



        public float MouseSensitivityY => mouseSensitivityY;



        public float GamepadSensitivityX => gamepadSensitivityX;



        public float GamepadSensitivityY => gamepadSensitivityY;



        public float LookSmoothing => lookSmoothing;



        public float FollowDampingX => followDampingX;



        public float FollowDampingY => followDampingY;



        public float FollowDampingZ => followDampingZ;



        public bool ObstacleAvoidanceEnabled => obstacleAvoidanceEnabled;



        public LayerMask CollisionLayerMask => collisionLayerMask;



        public string CollisionIgnoreTag => collisionIgnoreTag;



        public float ObstacleAvoidanceRadius => obstacleAvoidanceRadius;



        public float CollisionDampingInto => collisionDampingInto;



        public float CollisionDampingFrom => collisionDampingFrom;



        public float AimBlendDurationSeconds => aimBlendDurationSeconds;



        public float FieldOfView => fieldOfView;



        public float AimLookSensitivityMultiplier => aimLookSensitivityMultiplier;



        public bool ValidationDisableObstacleAvoidanceForBaselinePass =>
            validationDisableObstacleAvoidanceForBaselinePass;



        #endregion

    }

}


