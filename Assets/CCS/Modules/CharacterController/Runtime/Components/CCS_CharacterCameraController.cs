using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;

// =============================================================================
// SCRIPT: CCS_CharacterCameraController
// CATEGORY: Modules / CharacterController / Runtime / Components
// PURPOSE: Binds shared rig targets and applies Third Person Follow profile tuning.
// PLACEMENT: Scene camera rig root. Player prefab owns the shared camera rig target.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: TP and Aim share one tracking target yaw/pitch. Priority blend only changes offset/distance.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public sealed class CCS_CharacterCameraController : MonoBehaviour
    {
        public const string DefaultCinemachineRigDescription = "Third Person Follow + Third Person Aim";

        #region Variables

        [Header("Profiles")]
        [SerializeField] private CCS_CharacterCameraProfileSet cameraProfileSet;

        [Header("Transforms")]
        [FormerlySerializedAs("cameraTarget")]
        [SerializeField] private Transform cameraPivot;

        [FormerlySerializedAs("aimTarget")]
        [SerializeField] private Transform cameraLookTarget;

        [Header("Cinemachine")]
        [FormerlySerializedAs("thirdPersonCamera")]
        [SerializeField] private CinemachineCamera cinemachineCamera;

        [SerializeField] private CinemachineCamera aimCinemachineCamera;

        [SerializeField] private bool enableRuntimeCameraDebug;

        [SerializeField] private bool enableAimRayDebug;

        private CCS_CharacterCameraMode activeCameraMode = CCS_CharacterCameraMode.ThirdPersonSurvival;
        private CCS_CharacterCameraProfile activeProfile;
        private CinemachineThirdPersonFollow thirdPersonFollow;
        private CinemachineThirdPersonFollow aimThirdPersonFollow;
        private CinemachineThirdPersonAim aimThirdPersonAim;
        private bool isAimModeActive;

        #endregion

        #region Properties

        public CCS_CharacterCameraProfileSet CameraProfileSet => cameraProfileSet;

        public CCS_CharacterCameraProfile ActiveProfile => activeProfile;

        public CCS_CharacterCameraMode ActiveCameraMode => activeCameraMode;

        public Transform CameraPivot => cameraPivot;

        public Transform CameraLookTarget => cameraLookTarget;

        public CinemachineCamera CinemachineCamera => cinemachineCamera;

        public CinemachineCamera AimCinemachineCamera => aimCinemachineCamera;

        public bool IsAimModeActive => isAimModeActive;

        public bool HasAimCameraConfigured => aimCinemachineCamera != null;

        public string CinemachineRigDescription => DescribeCinemachineRig();

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            ResolveCinemachineCameras();
            ResolveCinemachineComponents();
            ApplyActiveProfile();
            ApplyAimProfile();
            ApplyBrainBlendFromAimProfile();
            isAimModeActive = false;
            ApplyCameraPriorities();
        }

        private void OnEnable()
        {
            ApplyActiveProfile();
            ApplyAimProfile();
            ApplyBrainBlendFromAimProfile();
        }

        private void OnDisable()
        {
            UnregisterMovementCamera();
        }

        #endregion

        #region Public Methods

        public void SetCameraProfileSet(CCS_CharacterCameraProfileSet profileSet)
        {
            cameraProfileSet = profileSet;
            ApplyActiveProfile();
            ApplyAimProfile();
            ApplyBrainBlendFromAimProfile();
        }

        public void BindFollowTargets(Transform trackingTarget, Transform lookTarget)
        {
            cameraPivot = trackingTarget;
            cameraLookTarget = lookTarget;
            ResolveCinemachineCameras();
            ResolveCinemachineComponents();
            ApplyActiveProfile();
            ApplyAimProfile();
            RegisterMovementCamera();
            ApplyBrainBlendFromAimProfile();

            if (enableRuntimeCameraDebug || enableAimRayDebug)
            {
                Debug.Log(
                    $"[Character Camera] Bound tracking={trackingTarget?.name ?? "null"} "
                    + $"look={lookTarget?.name ?? "null"}",
                    this);
            }
        }

        public bool HasFollowTargetsAssigned => cameraPivot != null;

        public Camera GetOutputCamera()
        {
            return GetComponentInChildren<Camera>(true);
        }

        public void RegisterMovementCamera()
        {
            Camera outputCamera = GetOutputCamera();
            if (outputCamera != null)
            {
                CCS_CharacterMovementCameraContext.Register(outputCamera);
            }
        }

        public void UnregisterMovementCamera()
        {
            CCS_CharacterMovementCameraContext.Clear(GetOutputCamera());
        }

        public void SetAimModeActive(bool aimActive)
        {
            if (isAimModeActive == aimActive)
            {
                return;
            }

            isAimModeActive = aimActive;
            ApplyCameraPriorities();

            if (enableRuntimeCameraDebug)
            {
                Debug.Log(
                    aimActive
                        ? "[Character Camera] Aim mode started."
                        : "[Character Camera] Aim mode ended.",
                    this);
            }
        }

        #endregion

        #region Private Methods

        private void ResolveCinemachineCameras()
        {
            if (cinemachineCamera == null)
            {
                Transform tpTransform = transform.Find(CCS_CharacterControllerConstants.ThirdPersonCinemachineCameraName);
                cinemachineCamera = tpTransform != null
                    ? tpTransform.GetComponent<CinemachineCamera>()
                    : GetComponentInChildren<CinemachineCamera>(true);
            }

            if (aimCinemachineCamera == null)
            {
                Transform aimTransform = transform.Find(CCS_CharacterControllerConstants.AimCinemachineCameraName);
                if (aimTransform != null)
                {
                    aimCinemachineCamera = aimTransform.GetComponent<CinemachineCamera>();
                }
            }
        }

        private void ResolveCinemachineComponents()
        {
            thirdPersonFollow = cinemachineCamera != null
                ? cinemachineCamera.GetComponent<CinemachineThirdPersonFollow>()
                : null;
            aimThirdPersonFollow = aimCinemachineCamera != null
                ? aimCinemachineCamera.GetComponent<CinemachineThirdPersonFollow>()
                : null;
            aimThirdPersonAim = aimCinemachineCamera != null
                ? aimCinemachineCamera.GetComponent<CinemachineThirdPersonAim>()
                : null;
        }

        private void ApplyActiveProfile()
        {
            activeProfile = cameraProfileSet != null
                ? cameraProfileSet.ResolveActiveProfile(activeCameraMode)
                : null;

            if (activeProfile != null)
            {
                activeCameraMode = activeProfile.CameraMode;
            }

            ApplyFollowTargets();
            ApplyThirdPersonProfileSettings();
        }

        private void ApplyAimProfile()
        {
            ApplyAimFollowTargets();
            ApplyAimCameraProfileSettings();
        }

        private void ApplyFollowTargets()
        {
            ApplyFollowTargetsToCamera(cinemachineCamera);
        }

        private void ApplyAimFollowTargets()
        {
            ApplyFollowTargetsToCamera(aimCinemachineCamera);
        }

        private void ApplyFollowTargetsToCamera(CinemachineCamera camera)
        {
            if (camera == null || cameraPivot == null)
            {
                return;
            }

            camera.Target.TrackingTarget = cameraPivot;
            CameraTarget target = camera.Target;
            target.CustomLookAtTarget = false;
            target.LookAtTarget = null;
            camera.Target = target;
        }

        private void ApplyThirdPersonProfileSettings()
        {
            ApplyThirdPersonFollowProfileSettings(thirdPersonFollow, activeProfile);
            ApplyLensProfileSettings(cinemachineCamera, activeProfile);
        }

        private void ApplyAimCameraProfileSettings()
        {
            CCS_CharacterCameraProfile aimProfile = cameraProfileSet != null
                ? cameraProfileSet.AimOverShoulderProfile
                : null;
            if (aimProfile == null)
            {
                return;
            }

            ApplyThirdPersonFollowProfileSettings(aimThirdPersonFollow, aimProfile);
            ApplyThirdPersonAimSettings(aimThirdPersonAim, aimProfile);
            ApplyLensProfileSettings(aimCinemachineCamera, aimProfile);
        }

        private static void ApplyThirdPersonFollowProfileSettings(
            CinemachineThirdPersonFollow targetFollow,
            CCS_CharacterCameraProfile profile)
        {
            if (targetFollow == null || profile == null)
            {
                return;
            }

            Vector3 shoulderOffset = profile.ThirdPersonShoulderOffset;

            targetFollow.Damping = new Vector3(
                profile.FollowDampingX,
                profile.FollowDampingY,
                profile.FollowDampingZ);
            targetFollow.ShoulderOffset = shoulderOffset;
            targetFollow.VerticalArmLength = profile.ThirdPersonVerticalArmLength;
            targetFollow.CameraSide = Mathf.Clamp01(Mathf.Abs(profile.ThirdPersonCameraSide));
            targetFollow.CameraDistance = profile.ThirdPersonCameraDistance;

#if CINEMACHINE_PHYSICS
            LayerMask collisionFilter = profile.CollisionLayerMask;
            if (collisionFilter.value == -1)
            {
                collisionFilter = new LayerMask { value = 1 << LayerMask.NameToLayer("Default") };
            }

            var obstacleSettings = targetFollow.AvoidObstacles;
            obstacleSettings.Enabled = profile.ObstacleAvoidanceEnabled;
            obstacleSettings.CollisionFilter = collisionFilter;
            obstacleSettings.IgnoreTag = profile.CollisionIgnoreTag;
            obstacleSettings.CameraRadius = profile.ObstacleAvoidanceRadius;
            obstacleSettings.DampingIntoCollision = profile.CollisionDampingInto;
            obstacleSettings.DampingFromCollision = profile.CollisionDampingFrom;
            targetFollow.AvoidObstacles = obstacleSettings;
#endif
        }

        private static void ApplyThirdPersonAimSettings(
            CinemachineThirdPersonAim thirdPersonAim,
            CCS_CharacterCameraProfile profile)
        {
            if (thirdPersonAim == null || profile == null)
            {
                return;
            }

#if CINEMACHINE_PHYSICS
            LayerMask aimCollisionFilter = profile.CollisionLayerMask;
            if (aimCollisionFilter.value == -1)
            {
                aimCollisionFilter = new LayerMask { value = 1 << LayerMask.NameToLayer("Default") };
            }

            thirdPersonAim.AimCollisionFilter = aimCollisionFilter;
            thirdPersonAim.IgnoreTag = profile.CollisionIgnoreTag;
            thirdPersonAim.AimDistance = 200f;
            thirdPersonAim.NoiseCancellation = true;
#endif
        }

        private void ApplyBrainBlendFromAimProfile()
        {
            CCS_CharacterCameraProfile aimProfile = cameraProfileSet != null
                ? cameraProfileSet.AimOverShoulderProfile
                : null;
            if (aimProfile == null)
            {
                return;
            }

            Camera outputCamera = GetOutputCamera();
            if (outputCamera == null)
            {
                return;
            }

            CinemachineBrain brain = outputCamera.GetComponent<CinemachineBrain>();
            if (brain == null)
            {
                return;
            }

            CinemachineBlendDefinition defaultBlend = brain.DefaultBlend;
            if (!Mathf.Approximately(defaultBlend.Time, aimProfile.AimBlendDurationSeconds))
            {
                defaultBlend.Time = aimProfile.AimBlendDurationSeconds;
                brain.DefaultBlend = defaultBlend;
            }
        }

        private static void ApplyLensProfileSettings(CinemachineCamera camera, CCS_CharacterCameraProfile profile)
        {
            if (camera == null || profile == null)
            {
                return;
            }

            LensSettings lens = camera.Lens;
            if (!Mathf.Approximately(lens.FieldOfView, profile.FieldOfView))
            {
                lens.FieldOfView = profile.FieldOfView;
                camera.Lens = lens;
            }
        }

        private void ApplyCameraPriorities()
        {
            SetCameraPriority(
                cinemachineCamera,
                isAimModeActive
                    ? CCS_CharacterControllerConstants.CinemachineCameraInactivePriority
                    : CCS_CharacterControllerConstants.ThirdPersonCameraActivePriority);
            SetCameraPriority(
                aimCinemachineCamera,
                isAimModeActive
                    ? CCS_CharacterControllerConstants.AimCameraActivePriority
                    : CCS_CharacterControllerConstants.CinemachineCameraInactivePriority);
        }

        private static void SetCameraPriority(CinemachineCamera camera, int priority)
        {
            if (camera == null)
            {
                return;
            }

            PrioritySettings prioritySettings = camera.Priority;
            if (prioritySettings.Value != priority)
            {
                prioritySettings.Value = priority;
                camera.Priority = prioritySettings;
            }
        }

        private string DescribeCinemachineRig()
        {
            if (thirdPersonFollow == null)
            {
                return "Unassigned";
            }

            return aimThirdPersonFollow != null
                ? DefaultCinemachineRigDescription
                : "Third Person Follow";
        }

        #endregion
    }
}
