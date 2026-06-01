using CCS.Modules.CharacterController;
using Unity.Cinemachine;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_PlayerCinemachineCameraDriver
// CATEGORY: Survival / Runtime / Player
// PURPOSE: Wires Cinemachine 3.1 third-person follow to character look targets and profile tuning.
// PLACEMENT: PF_CCS_Player alongside CCS_PlayerGameplayController.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Main Camera uses CinemachineBrain; look yaw/pitch driven by movement service.
// =============================================================================

namespace CCS.Survival.Player
{
    [DefaultExecutionOrder(205)]
    public sealed class CCS_PlayerCinemachineCameraDriver : MonoBehaviour
    {
        #region Variables

        [Header("Cinemachine")]
        [Tooltip("Gameplay CinemachineCamera with ThirdPersonFollow.")]
        [SerializeField] private CinemachineCamera gameplayCamera;

        [Tooltip("Yaw pivot used for movement facing.")]
        [SerializeField] private Transform cameraYawPivot;

        [Tooltip("Look target tracked by Cinemachine third-person follow.")]
        [SerializeField] private Transform cameraLookTarget;

        [Header("Profile")]
        [Tooltip("Character controller profile supplying third-person camera tuning.")]
        [SerializeField] private CCS_CharacterControllerProfile characterControllerProfile;

        private CinemachineThirdPersonFollow thirdPersonFollow;
        private bool profileApplied;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            if (gameplayCamera == null)
            {
                gameplayCamera = GetComponentInChildren<CinemachineCamera>(true);
            }

            if (gameplayCamera != null)
            {
                thirdPersonFollow = gameplayCamera.GetComponent<CinemachineThirdPersonFollow>();
            }

            if (cameraYawPivot == null)
            {
                Transform pivot = transform.Find("CameraPivot");
                if (pivot != null)
                {
                    cameraYawPivot = pivot;
                }
            }

            if (cameraLookTarget == null && cameraYawPivot != null)
            {
                Transform look = cameraYawPivot.Find("CameraLookTarget");
                if (look != null)
                {
                    cameraLookTarget = look;
                }
            }
        }

        private void Start()
        {
            ApplyProfileToCinemachine();
            WireCinemachineTargets();
        }

        #endregion

        #region Public Methods

        public void ApplyProfileToCinemachine()
        {
            if (characterControllerProfile == null || thirdPersonFollow == null)
            {
                return;
            }

            CCS_CharacterCameraProfile cameraProfile = characterControllerProfile.Camera;
            if (cameraProfile == null)
            {
                return;
            }

            thirdPersonFollow.CameraDistance = cameraProfile.CameraDistance;
            thirdPersonFollow.ShoulderOffset = cameraProfile.ShoulderOffset;
            thirdPersonFollow.VerticalArmLength = cameraProfile.VerticalArmLength;
            thirdPersonFollow.CameraSide = cameraProfile.CameraSide;
            thirdPersonFollow.Damping = cameraProfile.FollowDamping;
            profileApplied = true;
        }

        public void WireCinemachineTargets()
        {
            if (gameplayCamera == null || cameraLookTarget == null)
            {
                return;
            }

            gameplayCamera.Target.TrackingTarget = cameraLookTarget;
            gameplayCamera.Target.LookAtTarget = cameraLookTarget;
        }

        #endregion

        #region Properties

        public bool IsConfigured => gameplayCamera != null
            && thirdPersonFollow != null
            && cameraLookTarget != null
            && profileApplied;

        #endregion
    }
}
