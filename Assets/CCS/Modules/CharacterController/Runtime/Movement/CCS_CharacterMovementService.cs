using CCS.Survival;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CharacterMovementService
// CATEGORY: Modules / CharacterController / Runtime / Movement
// PURPOSE: Runtime owner for CharacterController locomotion, look, snapshots, and events.
// PLACEMENT: Registered as CCS_ISurvivalService by future character controller installer.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: No Survival Core calls. Raises OnStaminaDrainRequested for future composition.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public sealed class CCS_CharacterMovementService : CCS_ISurvivalService
    {
        private const string LogPrefix = "[CCS_CharacterMovementService]";

        #region Variables

        private readonly CCS_CharacterControllerMotor motor = new CCS_CharacterControllerMotor();
        private readonly CCS_CharacterCameraController cameraController = new CCS_CharacterCameraController();
        private readonly CCS_CharacterInputRuntimeBridge defaultInputBridge = new CCS_CharacterInputRuntimeBridge();

        private CCS_CharacterControllerProfile activeProfile;
        private CCS_ICharacterInputProvider inputProvider;
        private UnityEngine.CharacterController characterController;
        private Transform followTransform;
        private Transform cameraTransform;
        private CCS_CharacterMovementSnapshot currentSnapshot;
        private bool isInitialized;

        #endregion

        #region Events

        public event CharacterMovementStateChangedHandler MovementStateChanged;
        public event CharacterGroundedStateChangedHandler GroundedStateChanged;
        public event CharacterJumpedHandler Jumped;
        public event CharacterLandedHandler Landed;
        public event CharacterSprintStateChangedHandler SprintStateChanged;
        public event CharacterCrouchStateChangedHandler CrouchStateChanged;
        public event CharacterStaminaDrainRequestedHandler StaminaDrainRequested;

        #endregion

        #region Properties

        public bool IsInitialized => isInitialized;

        public CCS_CharacterControllerProfile ActiveProfile => activeProfile;

        public CCS_CharacterMovementSnapshot CurrentSnapshot => currentSnapshot;

        public CCS_CharacterLookState LookState => cameraController.LookState;

        public CCS_ICharacterInputProvider InputProvider => inputProvider;

        public CCS_CharacterInputRuntimeBridge DefaultInputBridge => defaultInputBridge;

        #endregion

        #region Public Methods

        public void Initialize()
        {
            if (isInitialized)
            {
                return;
            }

            // Scene binding via InitializeFromScene sets isInitialized when controller and profile are ready.
        }

        public void InitializeFromScene(
            UnityEngine.CharacterController controller,
            CCS_CharacterControllerProfile profile,
            Transform followTarget = null,
            Transform cameraTarget = null,
            CCS_ICharacterInputProvider provider = null)
        {
            characterController = controller;
            activeProfile = profile;
            followTransform = followTarget != null ? followTarget : controller != null ? controller.transform : null;
            cameraTransform = cameraTarget;
            inputProvider = provider ?? defaultInputBridge;

            if (profile == null)
            {
                Debug.LogWarning($"{LogPrefix} Initialize called with null profile.");
                isInitialized = false;
                return;
            }

            CCS_SurvivalValidationResult validation =
                CCS_CharacterControllerValidationUtility.ValidateProfile(profile);

            if (!validation.IsSuccess)
            {
                Debug.LogWarning($"{LogPrefix} Profile validation warning: {validation.Message}");
            }

            motor.Initialize(controller, profile.Movement);
            cameraController.Initialize(
                followTransform,
                cameraTransform,
                profile.Camera,
                followTransform != null ? followTransform.eulerAngles.y : 0f,
                0f);

            currentSnapshot = new CCS_CharacterMovementSnapshot(
                CCS_CharacterMovementState.Idle,
                CCS_CharacterGroundingState.Grounded,
                Vector3.zero,
                0f,
                false,
                false,
                false);

            isInitialized = true;
        }

        public void SetInputProvider(CCS_ICharacterInputProvider provider)
        {
            inputProvider = provider ?? defaultInputBridge;
        }

        public void TickMovement(float deltaTime)
        {
            if (!isInitialized || deltaTime <= 0f)
            {
                return;
            }

            CCS_CharacterInputSnapshot inputSnapshot = inputProvider != null
                ? inputProvider.GetInputSnapshot()
                : CCS_CharacterInputSnapshot.Empty;

            cameraController.TickLook(inputSnapshot, deltaTime);

            CCS_CharacterMovementInput movementInput = BuildMovementInput(inputSnapshot);

            CCS_CharacterMovementSnapshot previousSnapshot = currentSnapshot;
            currentSnapshot = motor.Tick(
                movementInput,
                deltaTime,
                out bool landedThisFrame,
                out bool jumpedThisFrame);

            NotifyMovementTransitions(previousSnapshot, currentSnapshot, landedThisFrame, jumpedThisFrame);
        }

        #endregion

        #region Private Methods

        private CCS_CharacterMovementInput BuildMovementInput(CCS_CharacterInputSnapshot inputSnapshot)
        {
            Vector3 forward = Quaternion.Euler(0f, cameraController.YawDegrees, 0f) * Vector3.forward;
            Vector3 right = Quaternion.Euler(0f, cameraController.YawDegrees, 0f) * Vector3.right;
            Vector3 worldPlanar = forward * inputSnapshot.Move.y + right * inputSnapshot.Move.x;

            return new CCS_CharacterMovementInput(
                worldPlanar,
                inputSnapshot.JumpPressed,
                inputSnapshot.SprintHeld,
                inputSnapshot.CrouchHeld);
        }

        private void NotifyMovementTransitions(
            CCS_CharacterMovementSnapshot previous,
            CCS_CharacterMovementSnapshot current,
            bool landedThisFrame,
            bool jumpedThisFrame)
        {
            if (previous.MovementState != current.MovementState)
            {
                float drainRate = current.IsSprinting && activeProfile != null
                    ? activeProfile.Movement.StaminaDrainPerSecondWhileSprinting
                    : 0f;

                CCS_CharacterMovementEventArgs eventArgs =
                    new CCS_CharacterMovementEventArgs(previous, current, drainRate);
                MovementStateChanged?.Invoke(eventArgs);
            }

            if (previous.GroundingState != current.GroundingState)
            {
                GroundedStateChanged?.Invoke(previous.GroundingState, current.GroundingState);
            }

            if (jumpedThisFrame)
            {
                Jumped?.Invoke(current);
            }

            if (landedThisFrame)
            {
                Landed?.Invoke(current);
            }

            if (previous.IsSprinting != current.IsSprinting)
            {
                SprintStateChanged?.Invoke(current.IsSprinting);

                if (current.IsSprinting && activeProfile != null)
                {
                    StaminaDrainRequested?.Invoke(activeProfile.Movement.StaminaDrainPerSecondWhileSprinting);
                }
            }

            if (previous.IsCrouching != current.IsCrouching)
            {
                CrouchStateChanged?.Invoke(current.IsCrouching);
            }
        }

        #endregion
    }
}
