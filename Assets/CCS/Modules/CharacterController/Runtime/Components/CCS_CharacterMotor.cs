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
        [SerializeField] private CCS_CharacterAimLocomotionController aimLocomotionController;
        [SerializeField] private CCS_CharacterCameraFollowAnchor cameraFollowAnchor;
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
        private bool loggedSprintBlockedWhileAiming;

        #endregion

        #region Properties

        public CCS_CharacterMovementProfile MovementProfile => movementProfile;

        public bool IsAimMovementActive =>
            aimLocomotionController != null && aimLocomotionController.IsAimMovementActive;

        public Vector2 AimMoveInput =>
            aimLocomotionController != null ? aimLocomotionController.AimMoveInput : Vector2.zero;

        public bool IsGrounded => characterController != null && characterController.isGrounded;

        public float VerticalVelocity => verticalVelocity;

        public float CurrentSpeed => currentSpeed;

        public float TargetSpeed { get; private set; }

        public bool IsSprinting => isSprinting;

        public CCS_CharacterMovementMode MovementMode =>
            IsCombatLocomotionActive()
                ? CCS_CharacterMovementMode.AimStrafeLocomotion
                : CCS_CharacterMovementMode.GroundedThirdPerson;

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
            ResolveAimLocomotionReferences();
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

        public void SetAimLocomotionController(CCS_CharacterAimLocomotionController controller)
        {
            aimLocomotionController = controller;
        }

        public void SetCameraFollowAnchor(CCS_CharacterCameraFollowAnchor followAnchor)
        {
            cameraFollowAnchor = followAnchor;
        }

        #endregion

        #region Private Methods

        private void ResolveAimLocomotionReferences()
        {
            if (aimLocomotionController == null)
            {
                aimLocomotionController = GetComponent<CCS_CharacterAimLocomotionController>();
            }

            if (cameraFollowAnchor == null)
            {
                cameraFollowAnchor = GetComponentInChildren<CCS_CharacterCameraFollowAnchor>(true);
            }
        }

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
            if (IsCombatLocomotionActive())
            {
                if (IsFirstPersonAimLocomotionActive())
                {
                    ApplyFirstPersonAimMovement(deltaTime);
                }
                else
                {
                    ApplyAimStrafeMovement(deltaTime);
                }

                return;
            }

            ApplyThirdPersonMovement(deltaTime);
        }

        private bool IsCombatLocomotionActive()
        {
            return aimLocomotionController != null && aimLocomotionController.IsCombatLocomotionActive;
        }

        private bool IsFirstPersonAimLocomotionActive()
        {
            ResolveCameraFollowAnchorReference();
            return cameraFollowAnchor != null && cameraFollowAnchor.UsesFirstPersonBodyYawCoupling;
        }

        private void ResolveCameraFollowAnchorReference()
        {
            if (cameraFollowAnchor == null && aimLocomotionController != null)
            {
                cameraFollowAnchor = aimLocomotionController.CameraFollowAnchor;
            }

            if (cameraFollowAnchor == null)
            {
                cameraFollowAnchor = GetComponentInChildren<CCS_CharacterCameraFollowAnchor>(true);
            }
        }

        private void ApplyFirstPersonMovement(float deltaTime)
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

            ResolveCameraFollowAnchorReference();
            Vector3 viewForward = cameraFollowAnchor != null
                ? cameraFollowAnchor.PlanarForward
                : CCS_CharacterMovementCameraContext.GetPlanarForward();
            Vector3 viewRight = cameraFollowAnchor != null
                ? cameraFollowAnchor.PlanarRight
                : CCS_CharacterMovementCameraContext.GetPlanarRight();

            Vector3 desiredDirection = (viewForward * moveInput.y) + (viewRight * moveInput.x);
            if (desiredDirection.sqrMagnitude > 1f)
            {
                desiredDirection.Normalize();
            }

            ApplyPlanarMovement(
                deltaTime,
                moveInput,
                desiredDirection,
                movementProfile.RotationSmoothing,
                rotateTowardMovementDirection: false,
                jumpPressed,
                aimFacingForward: viewForward,
                skipBodyRotation: true);
        }

        private void ApplyFirstPersonAimMovement(float deltaTime)
        {
            Vector2 moveInput = inputProvider.MoveInput;
            bool jumpPressed = inputProvider.JumpPressed;

            if (movementProfile.AimDisableSprint && inputProvider.SprintHeld)
            {
                if (enableMovementDebugLogs && !loggedSprintBlockedWhileAiming)
                {
                    loggedSprintBlockedWhileAiming = true;
                    Debug.Log("[Character Motor] Sprint blocked while aiming.", this);
                }
            }
            else
            {
                loggedSprintBlockedWhileAiming = false;
            }

            if (staminaController != null)
            {
                staminaController.ReportMovementState(false, moveInput.sqrMagnitude > 0.01f, deltaTime);
            }

            isSprinting = false;

            float walkSpeed = movementProfile.WalkSpeed;
            if (staminaController != null)
            {
                walkSpeed *= staminaController.MovementSpeedMultiplier;
            }

            float deadZone = movementProfile.AimStrafeDeadZone;
            float deadZoneSqr = deadZone * deadZone;
            bool hasMoveInput = moveInput.sqrMagnitude > deadZoneSqr;

            ResolveCameraFollowAnchorReference();
            Vector3 viewForward = cameraFollowAnchor != null
                ? cameraFollowAnchor.PlanarForward
                : CCS_CharacterMovementCameraContext.GetPlanarForward();
            Vector3 viewRight = cameraFollowAnchor != null
                ? cameraFollowAnchor.PlanarRight
                : CCS_CharacterMovementCameraContext.GetPlanarRight();

            Vector3 desiredDirection = (viewForward * moveInput.y) + (viewRight * moveInput.x);
            if (desiredDirection.sqrMagnitude > 1f)
            {
                desiredDirection.Normalize();
            }

            float targetSpeed = 0f;
            if (hasMoveInput)
            {
                float speedMultiplier = movementProfile.AimMovementSpeedMultiplier;
                float absX = Mathf.Abs(moveInput.x);
                float absY = Mathf.Abs(moveInput.y);
                if (absY >= absX)
                {
                    speedMultiplier *= moveInput.y >= 0f
                        ? 1f
                        : movementProfile.AimBackpedalMultiplier;
                }
                else
                {
                    speedMultiplier *= movementProfile.AimSideStrafeMultiplier;
                }

                targetSpeed = walkSpeed * speedMultiplier;
            }

            TargetSpeed = targetSpeed;

            ApplyPlanarMovement(
                deltaTime,
                moveInput,
                desiredDirection,
                movementProfile.AimRotationSpeedDegrees,
                rotateTowardMovementDirection: false,
                jumpPressed,
                aimFacingForward: viewForward,
                skipBodyRotation: true);
        }

        private void ApplyThirdPersonMovement(float deltaTime)
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

            ApplyPlanarMovement(
                deltaTime,
                moveInput,
                desiredDirection,
                movementProfile.RotationSmoothing,
                rotateTowardMovementDirection: true,
                jumpPressed);
        }

        private void ApplyAimStrafeMovement(float deltaTime)
        {
            ResolveAimLocomotionReferences();

            Vector2 moveInput = inputProvider.MoveInput;
            bool jumpPressed = inputProvider.JumpPressed;
            bool sprintIntent = inputProvider.SprintHeld && moveInput.sqrMagnitude > 0.01f;

            if (movementProfile.AimDisableSprint && sprintIntent)
            {
                if (enableMovementDebugLogs && !loggedSprintBlockedWhileAiming)
                {
                    loggedSprintBlockedWhileAiming = true;
                    Debug.Log("[Character Motor] Sprint blocked while aiming.", this);
                }

                sprintIntent = false;
            }
            else
            {
                loggedSprintBlockedWhileAiming = false;
            }

            if (staminaController != null)
            {
                staminaController.ReportMovementState(false, moveInput.sqrMagnitude > 0.01f, deltaTime);
            }

            isSprinting = false;

            float walkSpeed = movementProfile.WalkSpeed;
            if (staminaController != null)
            {
                walkSpeed *= staminaController.MovementSpeedMultiplier;
            }

            float deadZone = movementProfile.AimStrafeDeadZone;
            float deadZoneSqr = deadZone * deadZone;
            bool hasMoveInput = moveInput.sqrMagnitude > deadZoneSqr;

            CCS_CharacterCameraFollowAnchor followAnchor = cameraFollowAnchor;
            if (followAnchor == null && aimLocomotionController != null)
            {
                followAnchor = aimLocomotionController.CameraFollowAnchor;
            }

            Vector3 aimForward = followAnchor != null
                ? followAnchor.PlanarForward
                : CCS_CharacterMovementCameraContext.GetPlanarForward();
            Vector3 aimRight = followAnchor != null
                ? followAnchor.PlanarRight
                : CCS_CharacterMovementCameraContext.GetPlanarRight();

            Vector3 desiredDirection = (aimForward * moveInput.y) + (aimRight * moveInput.x);
            if (desiredDirection.sqrMagnitude > 1f)
            {
                desiredDirection.Normalize();
            }

            float targetSpeed = 0f;
            if (hasMoveInput)
            {
                float speedMultiplier = movementProfile.AimMovementSpeedMultiplier;
                float absX = Mathf.Abs(moveInput.x);
                float absY = Mathf.Abs(moveInput.y);
                if (absY >= absX)
                {
                    speedMultiplier *= moveInput.y >= 0f
                        ? 1f
                        : movementProfile.AimBackpedalMultiplier;
                }
                else
                {
                    speedMultiplier *= movementProfile.AimSideStrafeMultiplier;
                }

                targetSpeed = walkSpeed * speedMultiplier;
            }

            TargetSpeed = targetSpeed;

            ApplyPlanarMovement(
                deltaTime,
                moveInput,
                desiredDirection,
                movementProfile.AimRotationSpeedDegrees,
                rotateTowardMovementDirection: false,
                jumpPressed,
                aimFacingForward: aimForward);
        }

        private void ApplyPlanarMovement(
            float deltaTime,
            Vector2 moveInput,
            Vector3 desiredDirection,
            float rotationSpeedDegrees,
            bool rotateTowardMovementDirection,
            bool jumpPressed,
            Vector3 aimFacingForward = default,
            bool skipBodyRotation = false)
        {
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

            Vector3 facingDirection = rotateTowardMovementDirection ? desiredDirection : aimFacingForward;
            if (!skipBodyRotation && facingDirection.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(facingDirection, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    rotationSpeedDegrees * deltaTime);
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

            Vector3 cameraForward = rotateTowardMovementDirection
                ? CCS_CharacterMovementCameraContext.GetPlanarForward()
                : aimFacingForward;

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
