using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;

// =============================================================================
// SCRIPT: CCS_CharacterCameraController
// CATEGORY: Modules / CharacterController / Runtime / Components
// PURPOSE: Binds shared rig targets and applies profile-driven Cinemachine camera tuning.
// PLACEMENT: Scene camera rig root. Player prefab owns the shared camera rig target.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: v0.6.14 — local self head layer mask + eye-accurate BodyAware anchor; legacy FP_Aim camera disabled.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public sealed class CCS_CharacterCameraController : MonoBehaviour
    {
        public const string DefaultCinemachineRigDescription = "Third Person Follow + Third Person Aim";
        public const string FirstPersonBodyAwareRigDescription = "First Person Body Aware + First Person Aim";

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

        [SerializeField] private CinemachineCamera firstPersonCinemachineCamera;

        [SerializeField] private CinemachineCamera firstPersonAimCinemachineCamera;

        [SerializeField] private bool enableRuntimeCameraDebug;

        [SerializeField] private bool debugCameraModeTransitions;

        [SerializeField] private bool enableAimRayDebug;

        private CCS_CharacterCameraMode activeCameraMode = CCS_CharacterCameraMode.ThirdPersonSurvival;
        private CCS_CharacterCameraProfile activeProfile;
        private Transform firstPersonCameraAnchor;
        private Transform firstPersonAimCameraAnchor;
        private CCS_CharacterCameraFollowAnchor boundFollowAnchor;
        private CCS_LocalFirstPersonHeadVisibility localFirstPersonHeadVisibility;
        private CCS_IWeaponCarryStateCameraSource boundCarryStateSource;
        private CCS_FirstPersonBodyCameraAnchor boundHeadTracker;
        private CinemachineThirdPersonFollow thirdPersonFollow;
        private CinemachineThirdPersonFollow aimThirdPersonFollow;
        private CinemachineThirdPersonAim aimThirdPersonAim;
        private bool isFirearmAimModeActive;
        private int defaultOutputCameraCullingMask = -1;

        #endregion

        #region Properties

        public CCS_CharacterCameraProfileSet CameraProfileSet => cameraProfileSet;

        public CCS_CharacterCameraProfile ActiveProfile => activeProfile;

        public CCS_CharacterCameraMode ActiveCameraMode => activeCameraMode;

        public Transform CameraPivot => cameraPivot;

        public Transform CameraLookTarget => cameraLookTarget;

        public Transform FirstPersonCameraAnchor => firstPersonCameraAnchor;

        public Transform FirstPersonAimCameraAnchor => firstPersonAimCameraAnchor;

        public CinemachineCamera CinemachineCamera => cinemachineCamera;

        public CinemachineCamera AimCinemachineCamera => aimCinemachineCamera;

        public CinemachineCamera FirstPersonCinemachineCamera => firstPersonCinemachineCamera;

        public CinemachineCamera FirstPersonAimCinemachineCamera => firstPersonAimCinemachineCamera;

        public bool IsFirearmAimModeActive => isFirearmAimModeActive;

        public bool IsAimModeActive => isFirearmAimModeActive;

        public bool IsFirstPersonAimActive => isFirearmAimModeActive;

        public bool HasFirearmAimCameraConfigured =>
            firstPersonCinemachineCamera != null && cameraProfileSet?.FirstPersonProfile != null;

        public bool HasAimCameraConfigured => HasFirearmAimCameraConfigured;

        public string CinemachineRigDescription => DescribeCinemachineRig();

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            ResolveCinemachineCameras();
            ResolveCinemachineComponents();
            ApplyActiveProfile();
            ApplyAimProfile();
            ApplyFirstPersonProfile();
            ApplyBrainBlendFromAimProfile();
            isFirearmAimModeActive = false;
            DeactivateLegacyFirstPersonAimCamera();
            CacheDefaultOutputCameraCullingMask();
            ApplyCameraPriorities();
        }

        private void OnEnable()
        {
            ApplyActiveProfile();
            ApplyAimProfile();
            ApplyFirstPersonProfile();
            ApplyBrainBlendFromAimProfile();
        }

        private void OnDisable()
        {
            UnbindCarryStateSource();
            UnregisterMovementCamera();
            SetFirstPersonHeadMask(false);
        }

        private void OnGUI()
        {
            if (!enableRuntimeCameraDebug)
            {
                return;
            }

            CCS_CharacterCameraProfile debugProfile = isFirearmAimModeActive
                ? ResolveFirstPersonProfile()
                : activeProfile;
            if (debugProfile == null)
            {
                return;
            }

            string modeLabel = isFirearmAimModeActive
                ? "FirstPersonAim (BodyAware)"
                : activeCameraMode.ToString();
            float pitch = boundFollowAnchor != null ? boundFollowAnchor.PitchDegrees : 0f;
            float pitchMin = boundFollowAnchor != null ? boundFollowAnchor.MinPitchDegrees : 0f;
            float pitchMax = boundFollowAnchor != null ? boundFollowAnchor.MaxPitchDegrees : 0f;
            float bodyYaw = boundFollowAnchor != null ? boundFollowAnchor.BodyYawDegrees : 0f;
            float cameraYaw = boundFollowAnchor != null ? boundFollowAnchor.CameraYawDegrees : 0f;
            float yawDelta = boundFollowAnchor != null ? boundFollowAnchor.YawDeltaDegrees : 0f;
            bool bodyYawMatches = boundFollowAnchor != null && boundFollowAnchor.BodyYawMatchesCameraYaw;
            bool useHeadTracking = boundHeadTracker != null && boundHeadTracker.IsHeadTrackingActive;
            string activeCinemachineCameraName = ResolveActiveCinemachineCameraName();
            string followTargetName = ResolveActiveFollowTargetName();
            bool bodyAwareDrivesAim = isFirearmAimModeActive
                && activeCinemachineCameraName
                    == CCS_CharacterControllerConstants.FirstPersonBodyAwareCinemachineCameraName;
            bool legacyFpAimInactive = IsLegacyFirstPersonAimCameraInactive();
            Vector3 anchorLocalPosition = firstPersonCameraAnchor != null
                ? firstPersonCameraAnchor.localPosition
                : Vector3.zero;
            Camera outputCamera = GetOutputCamera();
            LayerMask activeCullingMask = outputCamera != null ? outputCamera.cullingMask : default;
            string headMaskDebug = localFirstPersonHeadVisibility != null
                ? localFirstPersonHeadVisibility.BuildDebugReport(
                    activeCameraMode,
                    activeCinemachineCameraName,
                    anchorLocalPosition,
                    pitchMin,
                    pitchMax,
                    activeCullingMask)
                : "Head mask component: not bound";
            GUI.Label(
                new Rect(12f, 12f, 860f, 560f),
                "Camera Mode: "
                + modeLabel
                + "\nActive Cinemachine Camera: "
                + activeCinemachineCameraName
                + "\nBodyAware drives FirstPersonAim: "
                + (bodyAwareDrivesAim ? "Yes" : "No")
                + "\nLegacy FP_Aim inactive: "
                + (legacyFpAimInactive ? "Yes" : "No")
                + "\nActive Profile: "
                + debugProfile.ProfileDisplayName
                + "\nFollow / Tracking Target: "
                + followTargetName
                + "\nAnchor World Position: "
                + FormatVector3(firstPersonCameraAnchor != null ? firstPersonCameraAnchor.position : Vector3.zero)
                + "\nAnchor Local Position: "
                + FormatVector3(anchorLocalPosition)
                + "\nPitch: "
                + pitch.ToString("0.0")
                + "\nPitch Clamp Min / Max: "
                + pitchMin.ToString("0.0")
                + " / "
                + pitchMax.ToString("0.0")
                + "\nBody Yaw: "
                + bodyYaw.ToString("0.0")
                + "\nCamera Yaw: "
                + cameraYaw.ToString("0.0")
                + "\nYaw Delta: "
                + yawDelta.ToString("0.0")
                + "\nBody yaw follows camera yaw: "
                + (bodyYawMatches ? "Yes" : "No")
                + "\nAim Active: "
                + (isFirearmAimModeActive ? "Yes" : "No")
                + "\nDamping: 0"
                + "\nFOV: "
                + debugProfile.FieldOfView.ToString("0.0")
                + "\nNear Clip: "
                + debugProfile.NearClipPlane.ToString("0.000")
                + "\nTP Priority: "
                + GetCameraPriority(cinemachineCamera)
                + "\nFP BodyAware Priority: "
                + GetCameraPriority(firstPersonCinemachineCamera)
                + "\nLegacy FP Aim Priority: "
                + GetCameraPriority(firstPersonAimCinemachineCamera)
                + "\nUse Head Tracking: "
                + (useHeadTracking ? "true" : "false")
                + "\n"
                + headMaskDebug);
        }

        #endregion

        #region Public Methods

        public void SetCameraProfileSet(CCS_CharacterCameraProfileSet profileSet)
        {
            cameraProfileSet = profileSet;
            ApplyActiveProfile();
            ApplyAimProfile();
            ApplyFirstPersonProfile();
            ApplyBrainBlendFromAimProfile();
        }

        public void BindFollowTargets(Transform trackingTarget, Transform lookTarget)
        {
            cameraPivot = trackingTarget;
            cameraLookTarget = lookTarget;
            firstPersonCameraAnchor = ResolveFirstPersonCameraAnchor(trackingTarget);
            firstPersonAimCameraAnchor = ResolveFirstPersonAimCameraAnchor(trackingTarget);
            boundFollowAnchor = trackingTarget != null
                ? trackingTarget.GetComponentInParent<CCS_CharacterCameraFollowAnchor>()
                : null;
            boundHeadTracker = trackingTarget != null
                ? trackingTarget.GetComponentInParent<CCS_FirstPersonBodyCameraAnchor>()
                : null;
            localFirstPersonHeadVisibility = trackingTarget != null
                ? trackingTarget.GetComponentInParent<CCS_LocalFirstPersonHeadVisibility>()
                : null;

            ResolveCinemachineCameras();
            ResolveCinemachineComponents();
            ApplyFirstPersonAnchorLayout();
            ApplyActiveProfile();
            ApplyAimProfile();
            ApplyFirstPersonProfile();
            RegisterMovementCamera();
            ApplyBrainBlendFromAimProfile();
            BindCarryStateSourceFromPlayer(trackingTarget);
            ApplyCameraModeFromCarryState(forceLog: false);
            SetFirstPersonHeadMask(isFirearmAimModeActive);

            if (enableRuntimeCameraDebug || enableAimRayDebug)
            {
                Debug.Log(
                    $"[Character Camera] Bound tracking={trackingTarget?.name ?? "null"} "
                    + $"look={lookTarget?.name ?? "null"} "
                    + $"fpAnchor={firstPersonCameraAnchor?.name ?? "null"} "
                    + $"mode={activeCameraMode}",
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

        public void SetFirearmAimModeActive(bool aimActive)
        {
            ApplyFirearmAimMode(aimActive, "ExternalRequest");
        }

        public void SetAimModeActive(bool aimActive)
        {
            SetFirearmAimModeActive(aimActive);
        }

        internal void ApplyCameraModeFromCarryState(bool forceLog)
        {
            if (boundCarryStateSource == null)
            {
                return;
            }

            if (!boundCarryStateSource.ShouldDriveLocalCamera)
            {
                if (forceLog || debugCameraModeTransitions)
                {
                    Debug.Log("[Camera] Ignored remote carry state for camera switch.", this);
                }

                return;
            }

            bool wantsFirstPersonAim = boundCarryStateSource.WantsFirstPersonAimCamera;
            ApplyFirearmAimMode(wantsFirstPersonAim, "CarryState");
        }

        #endregion

        #region Private Methods

        private void ApplyFirearmAimMode(bool aimActive, string reason)
        {
            if (isFirearmAimModeActive == aimActive)
            {
                return;
            }

            isFirearmAimModeActive = aimActive;
            activeCameraMode = aimActive
                ? CCS_CharacterCameraMode.FirstPersonAim
                : CCS_CharacterCameraMode.ThirdPersonSurvival;
            ApplyBrainBlendForFirearmAimTransition(aimActive);
            ApplyLookProfileForAimState();
            ApplyCameraPriorities();
            ApplyFirstPersonProfile();
            SetFirstPersonHeadMask(aimActive);

            if (debugCameraModeTransitions || enableRuntimeCameraDebug)
            {
                string modeLabel = aimActive ? "FirstPersonAim" : "ThirdPersonSurvival";
                Debug.Log(
                    "[Camera] CarryState="
                    + (boundCarryStateSource != null
                        ? boundCarryStateSource.CarryStateValue.ToString()
                        : "Unknown")
                    + " Local="
                    + (boundCarryStateSource == null || boundCarryStateSource.ShouldDriveLocalCamera)
                    + " -> "
                    + modeLabel
                    + " ("
                    + reason
                    + ")"
                    + "\nTP priority: "
                    + GetCameraPriority(cinemachineCamera)
                    + "\nFP BodyAware priority: "
                    + GetCameraPriority(firstPersonCinemachineCamera)
                    + "\nLegacy FP Aim priority: "
                    + GetCameraPriority(firstPersonAimCinemachineCamera)
                    + "\nCurrent Mode: "
                    + activeCameraMode,
                    this);
            }
        }

        private void BindCarryStateSourceFromPlayer(Transform trackingTarget)
        {
            UnbindCarryStateSource();
            if (trackingTarget == null)
            {
                return;
            }

            MonoBehaviour[] behaviours = trackingTarget.GetComponentsInParent<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is CCS_IWeaponCarryStateCameraSource source)
                {
                    boundCarryStateSource = source;
                    boundCarryStateSource.CarryStateChanged += HandleBoundCarryStateChanged;
                    return;
                }
            }
        }

        private void UnbindCarryStateSource()
        {
            if (boundCarryStateSource == null)
            {
                return;
            }

            boundCarryStateSource.CarryStateChanged -= HandleBoundCarryStateChanged;
            boundCarryStateSource = null;
        }

        private void HandleBoundCarryStateChanged()
        {
            ApplyCameraModeFromCarryState(forceLog: true);
        }

        private static int GetCameraPriority(CinemachineCamera camera)
        {
            return camera != null ? camera.Priority.Value : -1;
        }

        private static string FormatVector3(Vector3 value)
        {
            return "("
                + value.x.ToString("0.000")
                + ", "
                + value.y.ToString("0.000")
                + ", "
                + value.z.ToString("0.000")
                + ")";
        }

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

            if (firstPersonCinemachineCamera == null)
            {
                Transform fpTransform = transform.Find(
                    CCS_CharacterControllerConstants.FirstPersonBodyAwareCinemachineCameraName);
                if (fpTransform != null)
                {
                    firstPersonCinemachineCamera = fpTransform.GetComponent<CinemachineCamera>();
                }
            }

            if (firstPersonAimCinemachineCamera == null)
            {
                Transform fpAimTransform = transform.Find(
                    CCS_CharacterControllerConstants.FirstPersonAimCinemachineCameraName);
                if (fpAimTransform != null)
                {
                    firstPersonAimCinemachineCamera = fpAimTransform.GetComponent<CinemachineCamera>();
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
            if (cameraProfileSet != null && cameraProfileSet.DefaultProfile != null)
            {
                activeCameraMode = cameraProfileSet.DefaultProfile.CameraMode;
            }

            activeProfile = cameraProfileSet != null
                ? cameraProfileSet.ResolveActiveProfile(activeCameraMode)
                : null;

            if (activeProfile != null)
            {
                activeCameraMode = activeProfile.CameraMode;
            }

            ApplyFollowTargets();
            ApplyThirdPersonProfileSettings();
            ApplyFirstPersonAnchorLayout();
            SetFirstPersonHeadMask(isFirearmAimModeActive);
        }

        private void ApplyAimProfile()
        {
            ApplyAimFollowTargets();
            ApplyAimCameraProfileSettings();
        }

        private void ApplyFirstPersonProfile()
        {
            ApplyFirstPersonFollowTargets();
            ApplyFirstPersonAnchorLayout();
            DeactivateLegacyFirstPersonAimCamera();
            ApplyFirstPersonLensSettings(firstPersonCinemachineCamera, ResolveFirstPersonProfile());
            ApplyFirstPersonHardLock(firstPersonCinemachineCamera);
        }

        private void ApplyFollowTargets()
        {
            ApplyFollowTargetsToCamera(cinemachineCamera, cameraPivot);
        }

        private void ApplyAimFollowTargets()
        {
            ApplyFollowTargetsToCamera(aimCinemachineCamera, cameraPivot);
        }

        private void ApplyFirstPersonFollowTargets()
        {
            Transform fpBodyTarget = firstPersonCameraAnchor != null ? firstPersonCameraAnchor : cameraPivot;
            ApplyFollowTargetsToCamera(firstPersonCinemachineCamera, fpBodyTarget);
        }

        private static void ApplyFollowTargetsToCamera(CinemachineCamera camera, Transform trackingTarget)
        {
            if (camera == null || trackingTarget == null)
            {
                return;
            }

            camera.Target.TrackingTarget = trackingTarget;
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
            ApplyBrainBlendForFirearmAimTransition(isFirearmAimModeActive);
        }

        private void ApplyBrainBlendForFirearmAimTransition(bool enteringFirearmAim)
        {
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

            float targetBlendSeconds = enteringFirearmAim
                ? CCS_CharacterControllerConstants.FirearmAimCameraBlendInSeconds
                : CCS_CharacterControllerConstants.FirearmAimCameraBlendOutSeconds;

            CinemachineBlendDefinition defaultBlend = brain.DefaultBlend;
            if (!Mathf.Approximately(defaultBlend.Time, targetBlendSeconds))
            {
                defaultBlend.Time = targetBlendSeconds;
                brain.DefaultBlend = defaultBlend;
            }
        }

        private static void ApplyFirstPersonHardLock(CinemachineCamera camera)
        {
            if (camera == null)
            {
                return;
            }

            CinemachineFollow follow = camera.GetComponent<CinemachineFollow>();
            if (follow != null)
            {
                follow.FollowOffset = Vector3.zero;
                var trackerSettings = follow.TrackerSettings;
                trackerSettings.PositionDamping = Vector3.zero;
                trackerSettings.RotationDamping = Vector3.zero;
                trackerSettings.QuaternionDamping = CCS_CharacterControllerConstants.FirstPersonCinemachineDamping;
                follow.TrackerSettings = trackerSettings;
            }

            CinemachineRotateWithFollowTarget rotateWithFollowTarget =
                camera.GetComponent<CinemachineRotateWithFollowTarget>();
            if (rotateWithFollowTarget != null)
            {
                rotateWithFollowTarget.Damping = CCS_CharacterControllerConstants.FirstPersonCinemachineDamping;
            }
        }

        private CCS_CharacterCameraProfile ResolveAimBlendProfile()
        {
            CCS_CharacterCameraProfile fpAim = ResolveFirstPersonAimProfile();
            if (fpAim != null)
            {
                return fpAim;
            }

            return cameraProfileSet != null ? cameraProfileSet.AimOverShoulderProfile : null;
        }

        private static void ApplyLensProfileSettings(CinemachineCamera camera, CCS_CharacterCameraProfile profile)
        {
            if (camera == null || profile == null)
            {
                return;
            }

            LensSettings lens = camera.Lens;
            bool changed = false;
            if (!Mathf.Approximately(lens.FieldOfView, profile.FieldOfView))
            {
                lens.FieldOfView = profile.FieldOfView;
                changed = true;
            }

            if (IsFirstPersonProfile(profile)
                && !Mathf.Approximately(lens.NearClipPlane, profile.NearClipPlane))
            {
                lens.NearClipPlane = profile.NearClipPlane;
                changed = true;
            }

            if (changed)
            {
                camera.Lens = lens;
            }
        }

        private static void ApplyFirstPersonLensSettings(
            CinemachineCamera camera,
            CCS_CharacterCameraProfile profile)
        {
            ApplyLensProfileSettings(camera, profile);
        }

        private void ApplyFirstPersonAnchorLayout()
        {
            if (firstPersonCameraAnchor == null || activeProfile == null || !IsFirstPersonProfile(activeProfile))
            {
                return;
            }

            if (activeProfile.CameraMode == CCS_CharacterCameraMode.FirstPersonAim)
            {
                return;
            }

            if (HasActiveHeadTrackedAnchor())
            {
                return;
            }

            Vector3 expectedLocalPosition = new Vector3(
                0f,
                activeProfile.FirstPersonVerticalEyeOffset,
                activeProfile.FirstPersonForwardEyeOffset);
            if (firstPersonCameraAnchor.localPosition != expectedLocalPosition)
            {
                firstPersonCameraAnchor.localPosition = expectedLocalPosition;
            }

            if (firstPersonCameraAnchor.localRotation != Quaternion.identity)
            {
                firstPersonCameraAnchor.localRotation = Quaternion.identity;
            }
        }

        private void ApplyFirstPersonAimAnchorLayout()
        {
            if (firstPersonAimCameraAnchor == null)
            {
                return;
            }

            CCS_CharacterCameraProfile aimProfile = ResolveFirstPersonAimProfile();
            if (aimProfile == null)
            {
                return;
            }

            Vector3 expectedLocalPosition = aimProfile.FixedFirstPersonAimAnchorLocalOffset;
            if (firstPersonAimCameraAnchor.localPosition != expectedLocalPosition)
            {
                firstPersonAimCameraAnchor.localPosition = expectedLocalPosition;
            }

            if (firstPersonAimCameraAnchor.localRotation != Quaternion.identity)
            {
                firstPersonAimCameraAnchor.localRotation = Quaternion.identity;
            }
        }

        private bool HasActiveHeadTrackedAnchor()
        {
            if (boundFollowAnchor == null)
            {
                return false;
            }

            CCS_FirstPersonBodyCameraAnchor headTracker =
                boundFollowAnchor.GetComponentInParent<CCS_FirstPersonBodyCameraAnchor>();
            return headTracker != null && headTracker.IsHeadTrackingActive;
        }

        private void ApplyLookProfileForAimState()
        {
            if (boundFollowAnchor == null)
            {
                return;
            }

            if (isFirearmAimModeActive)
            {
                CCS_CharacterCameraProfile bodyAwareProfile = ResolveFirstPersonProfile();
                if (bodyAwareProfile != null)
                {
                    boundFollowAnchor.SetLookProfile(bodyAwareProfile);
                }

                return;
            }

            if (activeProfile != null)
            {
                boundFollowAnchor.SetLookProfile(activeProfile);
            }
        }

        private void ApplyCameraPriorities()
        {
            DeactivateLegacyFirstPersonAimCamera();
            SetCameraPriority(aimCinemachineCamera, CCS_CharacterControllerConstants.CinemachineCameraInactivePriority);

            if (isFirearmAimModeActive)
            {
                SetCameraPriority(cinemachineCamera, CCS_CharacterControllerConstants.CinemachineCameraInactivePriority);
                SetCameraPriority(
                    firstPersonCinemachineCamera,
                    CCS_CharacterControllerConstants.FirstPersonBodyAwareCameraActivePriority);
                return;
            }

            SetCameraPriority(
                firstPersonCinemachineCamera,
                CCS_CharacterControllerConstants.CinemachineCameraInactivePriority);
            SetCameraPriority(
                cinemachineCamera,
                CCS_CharacterControllerConstants.ThirdPersonCameraActivePriority);
        }

        private void DeactivateLegacyFirstPersonAimCamera()
        {
            if (firstPersonAimCinemachineCamera == null)
            {
                return;
            }

            SetCameraPriority(
                firstPersonAimCinemachineCamera,
                CCS_CharacterControllerConstants.LegacyFirstPersonAimCameraInactivePriority);

            if (firstPersonAimCinemachineCamera.gameObject.activeSelf)
            {
                firstPersonAimCinemachineCamera.gameObject.SetActive(false);
            }
        }

        private string ResolveActiveCinemachineCameraName()
        {
            if (isFirearmAimModeActive && firstPersonCinemachineCamera != null)
            {
                return firstPersonCinemachineCamera.name;
            }

            if (cinemachineCamera != null
                && GetCameraPriority(cinemachineCamera)
                    >= CCS_CharacterControllerConstants.ThirdPersonCameraActivePriority)
            {
                return cinemachineCamera.name;
            }

            return "None";
        }

        private string ResolveActiveFollowTargetName()
        {
            if (isFirearmAimModeActive)
            {
                if (firstPersonCinemachineCamera?.Target.TrackingTarget != null)
                {
                    return firstPersonCinemachineCamera.Target.TrackingTarget.name;
                }

                return firstPersonCameraAnchor != null
                    ? firstPersonCameraAnchor.name
                    : "Missing";
            }

            return cameraPivot != null ? cameraPivot.name : "Missing";
        }

        private bool IsLegacyFirstPersonAimCameraInactive()
        {
            if (firstPersonAimCinemachineCamera == null)
            {
                return true;
            }

            return !firstPersonAimCinemachineCamera.gameObject.activeSelf
                && GetCameraPriority(firstPersonAimCinemachineCamera)
                    <= CCS_CharacterControllerConstants.CinemachineCameraInactivePriority;
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

        private static bool IsFirstPersonProfile(CCS_CharacterCameraProfile profile)
        {
            return profile != null
                && (profile.CameraMode == CCS_CharacterCameraMode.FirstPersonBodyAware
                    || profile.CameraMode == CCS_CharacterCameraMode.FirstPerson
                    || profile.CameraMode == CCS_CharacterCameraMode.FirstPersonAim);
        }

        private CCS_CharacterCameraProfile ResolveFirstPersonProfile()
        {
            if (cameraProfileSet == null)
            {
                return activeProfile;
            }

            return cameraProfileSet.FirstPersonProfile != null
                ? cameraProfileSet.FirstPersonProfile
                : activeProfile;
        }

        private CCS_CharacterCameraProfile ResolveFirstPersonAimProfile()
        {
            if (cameraProfileSet == null)
            {
                return activeProfile;
            }

            return cameraProfileSet.FirstPersonAimProfile != null
                ? cameraProfileSet.FirstPersonAimProfile
                : ResolveFirstPersonProfile();
        }

        private static Transform ResolveFirstPersonCameraAnchor(Transform trackingTarget)
        {
            if (trackingTarget == null)
            {
                return null;
            }

            Transform anchor = trackingTarget.Find(CCS_CharacterControllerConstants.FirstPersonCameraAnchorObjectName);
            return anchor;
        }

        private static Transform ResolveFirstPersonAimCameraAnchor(Transform trackingTarget)
        {
            if (trackingTarget == null)
            {
                return null;
            }

            return trackingTarget.Find(CCS_CharacterControllerConstants.FirstPersonAimCameraAnchorObjectName);
        }

        private void SetFirstPersonHeadMask(bool active)
        {
            string activeCameraName = ResolveActiveCinemachineCameraName();
            if (localFirstPersonHeadVisibility != null)
            {
                localFirstPersonHeadVisibility.SetFirstPersonHeadMaskActive(
                    active,
                    activeCameraMode,
                    activeCameraName);
            }

            ApplyOutputCameraCullingMaskForHeadMask(active);
        }

        private void CacheDefaultOutputCameraCullingMask()
        {
            Camera outputCamera = GetOutputCamera();
            if (outputCamera != null)
            {
                defaultOutputCameraCullingMask = outputCamera.cullingMask;
            }
            else
            {
                defaultOutputCameraCullingMask = CCS_CharacterCameraLayerUtility.BuildDefaultOutputCameraCullingMask().value;
            }
        }

        private void ApplyOutputCameraCullingMaskForHeadMask(bool firstPersonHeadMaskActive)
        {
            Camera outputCamera = GetOutputCamera();
            if (outputCamera == null)
            {
                return;
            }

            if (defaultOutputCameraCullingMask < 0)
            {
                CacheDefaultOutputCameraCullingMask();
            }

            LayerMask baseMask = defaultOutputCameraCullingMask;
            outputCamera.cullingMask = firstPersonHeadMaskActive
                ? CCS_CharacterCameraLayerUtility.BuildFirstPersonBodyAwareCullingMask(baseMask)
                : baseMask;
        }

        private void SetBodyVisibilityFirstPerson(bool active)
        {
            SetFirstPersonHeadMask(active);
        }

        private string DescribeCinemachineRig()
        {
            if (isFirearmAimModeActive)
            {
                return firstPersonCinemachineCamera != null
                    ? "First Person Body Aware (Aim)"
                    : "First Person Body Aware (Unassigned)";
            }

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
