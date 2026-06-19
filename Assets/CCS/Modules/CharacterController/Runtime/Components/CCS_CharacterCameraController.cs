using Unity.Cinemachine;
using Unity.Cinemachine.TargetTracking;
using UnityEngine;
using UnityEngine.Serialization;

// =============================================================================
// SCRIPT: CCS_CharacterCameraController
// CATEGORY: Modules / CharacterController / Runtime / Components
// PURPOSE: Binds Cinemachine targets and applies profile tuning for scene camera rigs.
// PLACEMENT: Scene camera rig root. Player prefab keeps pivot references only.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Rig binder only. Cinemachine Input Axis Controller owns look on Orbital Follow.
//        Rotation Composer frames LookAt. Never writes orbital axis Value/Center.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public sealed class CCS_CharacterCameraController : MonoBehaviour
    {
        public const string DefaultCinemachineRigDescription = "Orbital Follow + Rotation Composer";

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

        private CCS_CharacterCameraMode activeCameraMode = CCS_CharacterCameraMode.ThirdPersonSurvival;
        private CCS_CharacterCameraProfile activeProfile;
        private CinemachineOrbitalFollow orbitalFollow;
        private CinemachineRotationComposer rotationComposer;

        #endregion

        #region Properties

        public CCS_CharacterCameraProfileSet CameraProfileSet => cameraProfileSet;

        public CCS_CharacterCameraProfile ActiveProfile => activeProfile;

        public CCS_CharacterCameraMode ActiveCameraMode => activeCameraMode;

        public Transform CameraPivot => cameraPivot;

        public Transform CameraLookTarget => cameraLookTarget;

        public CinemachineCamera CinemachineCamera => cinemachineCamera;

        public string CinemachineRigDescription => DescribeCinemachineRig();

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            ResolveCinemachineCamera();
            ResolveCinemachineComponents();
            ApplyActiveProfile();
        }

        private void OnEnable()
        {
            ApplyActiveProfile();
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
        }

        public void BindFollowTargets(Transform pivot, Transform lookTarget)
        {
            cameraPivot = pivot;
            cameraLookTarget = lookTarget;
            ResolveCinemachineCamera();
            ResolveCinemachineComponents();
            ApplyActiveProfile();
            RegisterMovementCamera();
        }

        public bool HasFollowTargetsAssigned =>
            cameraPivot != null && cameraLookTarget != null;

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

        #endregion

        #region Private Methods

        private void ResolveCinemachineCamera()
        {
            if (cinemachineCamera != null)
            {
                return;
            }

            cinemachineCamera = GetComponentInChildren<CinemachineCamera>(true);
        }

        private void ResolveCinemachineComponents()
        {
            if (cinemachineCamera == null)
            {
                orbitalFollow = null;
                rotationComposer = null;
                return;
            }

            orbitalFollow = cinemachineCamera.GetComponent<CinemachineOrbitalFollow>();
            rotationComposer = cinemachineCamera.GetComponent<CinemachineRotationComposer>();
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
            ApplyOrbitalFollowProfileSettings();
            ApplyRotationComposerProfileSettings();
        }

        private void ApplyFollowTargets()
        {
            if (cinemachineCamera == null)
            {
                return;
            }

            if (cameraPivot != null)
            {
                cinemachineCamera.Target.TrackingTarget = cameraPivot;
            }

            if (cameraLookTarget != null)
            {
                cinemachineCamera.Target.LookAtTarget = cameraLookTarget;
            }
        }

        private void ApplyOrbitalFollowProfileSettings()
        {
            if (orbitalFollow == null || activeProfile == null)
            {
                return;
            }

            Vector3 shoulderOffset = activeProfile.CameraShoulderOffset;
            shoulderOffset.x *= activeProfile.CameraSide >= 0f ? 1f : -1f;
            shoulderOffset.y = activeProfile.CameraHeight;
            orbitalFollow.TargetOffset = shoulderOffset;
            orbitalFollow.OrbitStyle = CinemachineOrbitalFollow.OrbitStyles.Sphere;
            orbitalFollow.Radius = activeProfile.OrbitalRadius;
            orbitalFollow.RecenteringTarget = CinemachineOrbitalFollow.ReferenceFrames.TrackingTarget;

            InputAxis verticalAxis = orbitalFollow.VerticalAxis;
            verticalAxis.Range = new Vector2(activeProfile.VerticalOrbitMin, activeProfile.VerticalOrbitMax);
            verticalAxis.Validate();
            orbitalFollow.VerticalAxis = verticalAxis;

            TrackerSettings trackerSettings = orbitalFollow.TrackerSettings;
            trackerSettings.BindingMode = BindingMode.LockToTargetWithWorldUp;
            trackerSettings.PositionDamping = new Vector3(
                activeProfile.FollowDampingX,
                activeProfile.FollowDampingY,
                activeProfile.FollowDampingZ);
            trackerSettings.Validate();
            orbitalFollow.TrackerSettings = trackerSettings;
        }

        private void ApplyRotationComposerProfileSettings()
        {
            if (rotationComposer == null || activeProfile == null)
            {
                return;
            }

            rotationComposer.Damping = new Vector2(
                activeProfile.FollowDampingX,
                activeProfile.FollowDampingY);
        }

        private string DescribeCinemachineRig()
        {
            if (orbitalFollow == null && rotationComposer == null)
            {
                return "Unassigned";
            }

            if (orbitalFollow != null && rotationComposer != null)
            {
                return DefaultCinemachineRigDescription;
            }

            if (orbitalFollow != null)
            {
                return "Orbital Follow";
            }

            return rotationComposer != null ? "Rotation Composer" : "Unassigned";
        }

        #endregion
    }
}
