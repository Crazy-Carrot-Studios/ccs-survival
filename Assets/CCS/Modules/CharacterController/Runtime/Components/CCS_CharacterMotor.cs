using CCS.Modules.Attributes;
using CCS.Modules.Interaction;

using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CharacterMotor
// CATEGORY: Modules / CharacterController / Runtime / Components
// PURPOSE: Camera-relative CharacterController movement with profile-driven tuning.
// PLACEMENT: PF_CCS_CharacterController_TestPlayer_Networked root.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Uses bound scene camera from CCS_CharacterMovementCameraContext when set.
//        Falls back to Camera.main only when no bound camera exists.
//        Jump uses coyote time and jump buffer from movement profile.
//        Honors CCS_ICharacterControlLockSource during interaction animations.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    [RequireComponent(typeof(UnityEngine.CharacterController))]
    [DefaultExecutionOrder(50)]
    public sealed class CCS_CharacterMotor : MonoBehaviour
    {
        #region Variables

        [Header("Profiles")]
        [SerializeField] private CCS_CharacterMovementProfile movementProfile;

        [Header("References")]
        [SerializeField] private CCS_CharacterInputActionProvider inputProvider;
        [SerializeField] private CCS_StaminaController staminaController;
        [SerializeField] private Component interactionLockSourceComponent;
        [SerializeField] private bool enableMovementDebugLogs;

        private UnityEngine.CharacterController characterController;
        private CCS_ICharacterControlLockSource controlLockSource;
        private Vector3 horizontalVelocity;
        private float verticalVelocity;
        private float currentSpeed;
        private bool isSprinting;
        private float coyoteTimeRemaining;
        private float jumpBufferTimeRemaining;
        private bool loggedControlLockActive;

        #endregion

        #region Properties

        public CCS_CharacterMovementProfile MovementProfile => movementProfile;

        public bool IsGrounded => characterController != null && characterController.isGrounded;

        public float VerticalVelocity => verticalVelocity;

        public float CurrentSpeed => currentSpeed;

        public float TargetSpeed { get; private set; }

        public bool IsSprinting => isSprinting;

        public CCS_CharacterMovementMode MovementMode => CCS_CharacterMovementMode.GroundedThirdPerson;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            characterController = GetComponent<UnityEngine.CharacterController>();
            if (staminaController == null)
            {
                staminaController = GetComponent<CCS_StaminaController>();
            }

            ResolveControlLockSource();
        }

        private void Update()
        {
            if (movementProfile == null || inputProvider == null || characterController == null || !enabled)
            {
                return;
            }

            UpdateMovement(Time.deltaTime);
        }

        #endregion

        #region Public Methods

        public void SetMovementProfile(CCS_CharacterMovementProfile profile)
        {
            movementProfile = profile;
        }

        public void SetInputProvider(CCS_CharacterInputActionProvider provider)
        {
            inputProvider = provider;
        }

        public void HardStopPlanarMotion()
        {
            horizontalVelocity = Vector3.zero;
            currentSpeed = 0f;
            TargetSpeed = 0f;
            isSprinting = false;
        }

        #endregion

        #region Private Methods

        private void ResolveControlLockSource()
        {
            if (controlLockSource != null)
            {
                return;
            }

            if (interactionLockSourceComponent is CCS_ICharacterControlLockSource fromComponent)
            {
                controlLockSource = fromComponent;
                return;
            }

            if (interactionLockSourceComponent is CCS_IInteractionLockController lockController
                && lockController is CCS_ICharacterControlLockSource lockAsControlSource)
            {
                controlLockSource = lockAsControlSource;
                return;
            }

            controlLockSource = GetComponentInChildren<CCS_ICharacterControlLockSource>();
        }

        private bool IsControlLocked()
        {
            ResolveControlLockSource();
            return controlLockSource != null && controlLockSource.IsControlLocked;
        }

        private void UpdateMovement(float deltaTime)
        {
            if (IsControlLocked())
            {
                ApplyLockedMovement(deltaTime);
                return;
            }

            loggedControlLockActive = false;
            ApplyLocomotionMovement(deltaTime);
        }

        private void ApplyLockedMovement(float deltaTime)
        {
            if (enableMovementDebugLogs && !loggedControlLockActive)
            {
                loggedControlLockActive = true;
                Debug.Log("[Character Motor] Control locked; suppressing movement.", this);
            }

            HardStopPlanarMotion();
            jumpBufferTimeRemaining = 0f;

            if (staminaController != null)
            {
                staminaController.ReportMovementState(false, false, deltaTime);
            }

            if (IsGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }

            verticalVelocity += movementProfile.Gravity * deltaTime;

            Vector3 motion = new Vector3(0f, verticalVelocity, 0f);
            Vector3 positionBefore = transform.position;
            characterController.Move(motion * deltaTime);

            CCS_CharacterMotorAuditHook.MoveLogged?.Invoke(new CCS_CharacterMotorAuditHook.MotorMoveSample
            {
                Source = gameObject,
                DeltaTime = deltaTime,
                MoveInput = Vector2.zero,
                Velocity = motion,
                PositionBefore = positionBefore,
                PositionAfter = transform.position,
                CameraForward = CCS_CharacterMovementCameraContext.GetPlanarForward(),
                MovementCamera = CCS_CharacterMovementCameraContext.ActiveCamera
            });
        }

        private void ApplyLocomotionMovement(float deltaTime)
        {
            Vector2 moveInput = inputProvider.MoveInput;
            bool jumpPressed = inputProvider.JumpPressed;
            bool sprintIntent = inputProvider.SprintHeld && moveInput.sqrMagnitude > 0.01f;

            if (staminaController != null)
            {
                staminaController.ReportMovementState(
                    inputProvider.SprintHeld,
                    moveInput.sqrMagnitude > 0.01f,
                    deltaTime);
                isSprinting = sprintIntent && staminaController.CanSprint;
            }
            else
            {
                isSprinting = sprintIntent;
            }

            float walkSpeed = movementProfile.WalkSpeed;
            if (staminaController != null)
            {
                walkSpeed *= staminaController.MovementSpeedMultiplier;
            }

            float desiredSpeed = isSprinting ? movementProfile.SprintSpeed : walkSpeed;
            TargetSpeed = moveInput.sqrMagnitude > 0.01f ? desiredSpeed : 0f;

            Vector3 cameraForward = CCS_CharacterMovementCameraContext.GetPlanarForward();
            Vector3 cameraRight = CCS_CharacterMovementCameraContext.GetPlanarRight();

            Vector3 desiredDirection =
                (cameraForward * moveInput.y) + (cameraRight * moveInput.x);
            if (desiredDirection.sqrMagnitude > 1f)
            {
                desiredDirection.Normalize();
            }

            Vector3 targetHorizontalVelocity = desiredDirection * TargetSpeed;
            float accelRate = TargetSpeed > currentSpeed
                ? movementProfile.Acceleration
                : movementProfile.Deceleration;

            if (!IsGrounded)
            {
                accelRate *= movementProfile.AirControl;
            }

            horizontalVelocity = Vector3.MoveTowards(
                horizontalVelocity,
                targetHorizontalVelocity,
                accelRate * deltaTime);

            currentSpeed = new Vector3(horizontalVelocity.x, 0f, horizontalVelocity.z).magnitude;

            if (desiredDirection.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(desiredDirection, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    movementProfile.RotationSmoothing * deltaTime);
            }

            UpdateJumpTimers(deltaTime, jumpPressed);

            if (IsGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }

            bool jumpExecuted = TryExecuteJump();

            verticalVelocity += movementProfile.Gravity * deltaTime;

            Vector3 motion = horizontalVelocity;
            motion.y = verticalVelocity;
            Vector3 positionBefore = transform.position;
            characterController.Move(motion * deltaTime);

            if (jumpPressed || jumpExecuted)
            {
                CCS_CharacterJumpAuditHook.JumpLogged?.Invoke(new CCS_CharacterJumpAuditHook.JumpSample
                {
                    Source = gameObject,
                    Grounded = IsGrounded,
                    JumpPressed = jumpPressed,
                    VerticalVelocity = verticalVelocity,
                    PositionBefore = positionBefore,
                    PositionAfter = transform.position,
                    JumpExecuted = jumpExecuted
                });
            }

            CCS_CharacterMotorAuditHook.MoveLogged?.Invoke(new CCS_CharacterMotorAuditHook.MotorMoveSample
            {
                Source = gameObject,
                DeltaTime = deltaTime,
                MoveInput = moveInput,
                Velocity = motion,
                PositionBefore = positionBefore,
                PositionAfter = transform.position,
                CameraForward = cameraForward,
                MovementCamera = CCS_CharacterMovementCameraContext.ActiveCamera
            });
        }

        private void UpdateJumpTimers(float deltaTime, bool jumpPressed)
        {
            if (IsGrounded)
            {
                coyoteTimeRemaining = movementProfile.CoyoteTime;
            }
            else
            {
                coyoteTimeRemaining -= deltaTime;
            }

            if (jumpPressed)
            {
                jumpBufferTimeRemaining = movementProfile.JumpBufferTime;
            }
            else
            {
                jumpBufferTimeRemaining -= deltaTime;
            }
        }

        private bool TryExecuteJump()
        {
            if (!movementProfile.JumpEnabled
                || jumpBufferTimeRemaining <= 0f
                || coyoteTimeRemaining <= 0f)
            {
                return false;
            }

            verticalVelocity = Mathf.Sqrt(movementProfile.JumpHeight * -2f * movementProfile.Gravity);
            coyoteTimeRemaining = 0f;
            jumpBufferTimeRemaining = 0f;
            return true;
        }

        #endregion
    }
}
