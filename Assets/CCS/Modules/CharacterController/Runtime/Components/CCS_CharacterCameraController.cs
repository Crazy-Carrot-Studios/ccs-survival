using Unity.Cinemachine;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CharacterCameraController
// CATEGORY: Modules / CharacterController / Runtime / Components
// PURPOSE: Drives Cinemachine third-person camera yaw/pitch and profile settings.
// PLACEMENT: PF_CCS_CharacterController_TestPlayer root.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Cinemachine 3.1 Third Person Follow. Player body remains visible.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public sealed class CCS_CharacterCameraController : MonoBehaviour
    {
        #region Variables

        [Header("Profiles")]
        [SerializeField] private CCS_CharacterCameraProfileSet cameraProfileSet;

        [Header("Transforms")]
        [SerializeField] private Transform cameraPivot;

        [SerializeField] private Transform cameraLookTarget;

        [Header("Cinemachine")]
        [SerializeField] private CinemachineCamera cinemachineCamera;

        [Header("References")]
        [SerializeField] private CCS_CharacterInputActionProvider inputProvider;

        private CCS_CharacterCameraMode activeCameraMode = CCS_CharacterCameraMode.ThirdPersonSurvival;
        private CCS_CharacterCameraProfile activeProfile;
        private CinemachineThirdPersonFollow thirdPersonFollow;
        private float yaw;
        private float pitch;
        private float currentCameraDistance;

        #endregion

        #region Properties

        public CCS_CharacterCameraProfileSet CameraProfileSet => cameraProfileSet;

        public CCS_CharacterCameraProfile ActiveProfile => activeProfile;

        public CCS_CharacterCameraMode ActiveCameraMode => activeCameraMode;

        public float Yaw => yaw;

        public float Pitch => pitch;

        public Transform CameraPivot => cameraPivot;

        public Transform CameraLookTarget => cameraLookTarget;

        public CinemachineCamera CinemachineCamera => cinemachineCamera;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            ResolveThirdPersonFollow();
            ApplyActiveProfile();
        }

        private void OnEnable()
        {
            ApplyActiveProfile();
        }

        private void LateUpdate()
        {
            if (activeProfile == null || inputProvider == null)
            {
                return;
            }

            UpdateLook(Time.deltaTime);
            ApplyZoomPlaceholder();
            ApplyThirdPersonFollowSettings();
            ApplyPivotRotation();
        }

        #endregion

        #region Public Methods

        public void SetCameraProfileSet(CCS_CharacterCameraProfileSet profileSet)
        {
            cameraProfileSet = profileSet;
            ApplyActiveProfile();
        }

        public void SetInputProvider(CCS_CharacterInputActionProvider provider)
        {
            inputProvider = provider;
        }

        public Vector3 GetMovementForward()
        {
            Vector3 forward = cameraPivot != null ? cameraPivot.forward : transform.forward;
            forward.y = 0f;
            return forward.sqrMagnitude > 0.0001f ? forward.normalized : transform.forward;
        }

        public Vector3 GetMovementRight()
        {
            Vector3 right = cameraPivot != null ? cameraPivot.right : transform.right;
            right.y = 0f;
            return right.sqrMagnitude > 0.0001f ? right.normalized : transform.right;
        }

        public Vector3 GetCameraForward()
        {
            if (cameraPivot != null)
            {
                return cameraPivot.forward;
            }

            return transform.forward;
        }

        #endregion

        #region Private Methods

        private void ResolveThirdPersonFollow()
        {
            if (cinemachineCamera == null)
            {
                return;
            }

            thirdPersonFollow = cinemachineCamera.GetComponent<CinemachineThirdPersonFollow>();
        }

        private void ApplyActiveProfile()
        {
            activeProfile = cameraProfileSet != null
                ? cameraProfileSet.ResolveActiveProfile(activeCameraMode)
                : null;

            if (activeProfile != null)
            {
                activeCameraMode = activeProfile.CameraMode;
                currentCameraDistance = activeProfile.CameraDistance;
            }

            if (cinemachineCamera != null)
            {
                if (cameraPivot != null)
                {
                    cinemachineCamera.Target.TrackingTarget = cameraPivot;
                }

                if (cameraLookTarget != null)
                {
                    cinemachineCamera.Target.LookAtTarget = cameraLookTarget;
                }
            }

            ApplyThirdPersonFollowSettings();
        }

        private void UpdateLook(float deltaTime)
        {
            Vector2 lookInput = inputProvider.LookInput;
            if (lookInput.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            bool useGamepad = inputProvider.LastInputDeviceLabel == "Gamepad";
            float sensitivityX = useGamepad
                ? activeProfile.GamepadSensitivityX
                : activeProfile.MouseSensitivityX;
            float sensitivityY = useGamepad
                ? activeProfile.GamepadSensitivityY
                : activeProfile.MouseSensitivityY;

            float yawDelta = lookInput.x * sensitivityX;
            float pitchDelta = lookInput.y * sensitivityY * (useGamepad ? deltaTime : 1f);

            if (useGamepad)
            {
                yaw += yawDelta * deltaTime;
                pitch -= pitchDelta;
            }
            else
            {
                yaw += yawDelta;
                pitch -= pitchDelta;
            }

            pitch = Mathf.Clamp(pitch, activeProfile.MinPitch, activeProfile.MaxPitch);
        }

        private void ApplyPivotRotation()
        {
            if (cameraPivot == null)
            {
                return;
            }

            cameraPivot.rotation = Quaternion.Euler(0f, yaw, 0f);

            if (cameraLookTarget != null)
            {
                cameraLookTarget.localRotation = Quaternion.Euler(pitch, 0f, 0f);
            }
        }

        private void ApplyZoomPlaceholder()
        {
            float zoomInput = inputProvider.CameraZoomInput;
            if (Mathf.Abs(zoomInput) <= 0.0001f)
            {
                return;
            }

            currentCameraDistance = Mathf.Clamp(
                currentCameraDistance - zoomInput * 0.5f,
                activeProfile.ZoomDistanceMin,
                activeProfile.ZoomDistanceMax);
        }

        private void ApplyThirdPersonFollowSettings()
        {
            if (thirdPersonFollow == null || activeProfile == null)
            {
                return;
            }

            Vector3 shoulderOffset = activeProfile.CameraShoulderOffset;
            shoulderOffset.y = activeProfile.CameraHeight;
            thirdPersonFollow.ShoulderOffset = shoulderOffset;
            thirdPersonFollow.VerticalArmLength = activeProfile.VerticalArmLength;
            thirdPersonFollow.CameraSide = activeProfile.CameraSide;
            thirdPersonFollow.CameraDistance = currentCameraDistance;
            thirdPersonFollow.Damping = new Vector3(
                activeProfile.FollowDampingX,
                activeProfile.FollowDampingY,
                activeProfile.FollowDampingZ);
        }

        #endregion
    }
}
