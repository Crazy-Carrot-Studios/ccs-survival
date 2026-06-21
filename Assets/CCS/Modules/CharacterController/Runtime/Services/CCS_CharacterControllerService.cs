using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CharacterControllerService
// CATEGORY: Modules / CharacterController / Runtime / Services
// PURPOSE: Owns character controller state and exposes read-only snapshots.
// PLACEMENT: PF_CCS_CharacterController_TestPlayer_Networked root.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: No inventory, interaction, stats, equipment, save, or combat dependencies.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public sealed class CCS_CharacterControllerService : MonoBehaviour
    {
        #region Variables

        [Header("References")]
        [SerializeField] private CCS_CharacterMotor motor;

        [SerializeField] private CCS_CharacterInputActionProvider inputProvider;

        [SerializeField] private CCS_CharacterCameraController cameraController;

        private readonly CCS_CharacterControllerState state = new CCS_CharacterControllerState();
        private CCS_CharacterControllerSnapshot snapshot;
        private CCS_CharacterCameraController resolvedProfileSource;

        #endregion

        #region Properties

        public CCS_CharacterControllerSnapshot Snapshot => snapshot;

        public CCS_CharacterControllerState State => state;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            ResolveProfileSource();
        }

        private void LateUpdate()
        {
            RefreshState();
            snapshot = state.ToSnapshot();
        }

        #endregion

        #region Public Methods

        public void SetMotor(CCS_CharacterMotor characterMotor)
        {
            motor = characterMotor;
        }

        public void SetInputProvider(CCS_CharacterInputActionProvider provider)
        {
            inputProvider = provider;
        }

        public void SetCameraController(CCS_CharacterCameraController controller)
        {
            cameraController = controller;
            ResolveProfileSource();
        }

        #endregion

        #region Private Methods

        private void ResolveProfileSource()
        {
            if (cameraController != null && cameraController.CinemachineCamera != null)
            {
                resolvedProfileSource = cameraController;
                return;
            }

            CCS_CharacterCameraController[] controllers =
                FindObjectsByType<CCS_CharacterCameraController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < controllers.Length; i++)
            {
                CCS_CharacterCameraController candidate = controllers[i];
                if (candidate == null || candidate.CinemachineCamera == null)
                {
                    continue;
                }

                resolvedProfileSource = candidate;
                return;
            }

            resolvedProfileSource = cameraController;
        }

        private void RefreshState()
        {
            CCS_CharacterCameraController profileSource = resolvedProfileSource ?? cameraController;

            state.MovementMode = motor != null
                ? motor.MovementMode
                : CCS_CharacterMovementMode.Disabled;
            state.CameraMode = profileSource != null
                ? profileSource.ActiveCameraMode
                : CCS_CharacterCameraMode.ThirdPersonSurvival;
            state.IsGrounded = motor != null && motor.IsGrounded;
            state.CurrentSpeed = motor != null ? motor.CurrentSpeed : 0f;
            state.TargetSpeed = motor != null ? motor.TargetSpeed : 0f;
            state.IsSprinting = motor != null && motor.IsSprinting;
            state.InputDeviceLabel = inputProvider != null
                ? inputProvider.LastInputDeviceLabel
                : "None";
            state.MovementInput = inputProvider != null ? inputProvider.MoveInput : Vector2.zero;
            state.LookInput = inputProvider != null ? inputProvider.LookInput : Vector2.zero;
            state.Yaw = CCS_CharacterMovementCameraContext.GetYawDegrees();
            state.Pitch = CCS_CharacterMovementCameraContext.GetPitchDegrees();
            state.PlayerPosition = transform.position;
            state.CameraForward = CCS_CharacterMovementCameraContext.GetPlanarForward();
            state.ActiveCinemachineRigDescription = profileSource != null
                ? profileSource.CinemachineRigDescription
                : string.Empty;

            if (profileSource?.ActiveProfile != null)
            {
                state.ActiveCameraProfileName = profileSource.ActiveProfile.ProfileDisplayName;
                state.ActiveMouseSensitivityX = profileSource.ActiveProfile.MouseSensitivityX;
                state.ActiveMouseSensitivityY = profileSource.ActiveProfile.MouseSensitivityY;
            }
            else
            {
                state.ActiveCameraProfileName = string.Empty;
                state.ActiveMouseSensitivityX = 0f;
                state.ActiveMouseSensitivityY = 0f;
            }
        }

        #endregion
    }
}
