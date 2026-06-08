using UnityEngine;

// =============================================================================
// SCRIPT: CCS_CharacterMotor
// CATEGORY: Modules / CharacterController / Runtime / Components
// PURPOSE: Camera-relative CharacterController movement with profile-driven tuning.
// PLACEMENT: PF_CCS_CharacterController_TestPlayer root.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: No root motion, animation, or combat. Jump ignored unless profile enables it.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    [RequireComponent(typeof(UnityEngine.CharacterController))]
    public sealed class CCS_CharacterMotor : MonoBehaviour
    {
        #region Variables

        [Header("Profiles")]
        [SerializeField] private CCS_CharacterMovementProfile movementProfile;

        [Header("References")]
        [SerializeField] private CCS_CharacterInputActionProvider inputProvider;

        [SerializeField] private CCS_CharacterCameraController cameraController;

        private UnityEngine.CharacterController characterController;
        private Vector3 horizontalVelocity;
        private float verticalVelocity;
        private float currentSpeed;
        private bool isSprinting;

        #endregion

        #region Properties

        public CCS_CharacterMovementProfile MovementProfile => movementProfile;

        public bool IsGrounded => characterController != null && characterController.isGrounded;

        public float CurrentSpeed => currentSpeed;

        public float TargetSpeed { get; private set; }

        public bool IsSprinting => isSprinting;

        public CCS_CharacterMovementMode MovementMode => CCS_CharacterMovementMode.GroundedThirdPerson;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            characterController = GetComponent<UnityEngine.CharacterController>();
        }

        private void Update()
        {
            if (movementProfile == null || inputProvider == null || characterController == null)
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

        public void SetCameraController(CCS_CharacterCameraController controller)
        {
            cameraController = controller;
        }

        #endregion

        #region Private Methods

        private void UpdateMovement(float deltaTime)
        {
            Vector2 moveInput = inputProvider.MoveInput;
            isSprinting = inputProvider.SprintHeld && moveInput.sqrMagnitude > 0.01f;
            float desiredSpeed = isSprinting ? movementProfile.SprintSpeed : movementProfile.WalkSpeed;
            TargetSpeed = moveInput.sqrMagnitude > 0.01f ? desiredSpeed : 0f;

            Vector3 cameraForward = cameraController != null
                ? cameraController.GetMovementForward()
                : transform.forward;
            Vector3 cameraRight = cameraController != null
                ? cameraController.GetMovementRight()
                : transform.right;

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

            if (IsGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }

            if (movementProfile.JumpEnabled && IsGrounded && inputProvider.JumpPressedThisFrame)
            {
                verticalVelocity = Mathf.Sqrt(movementProfile.JumpHeight * -2f * movementProfile.Gravity);
            }

            verticalVelocity += movementProfile.Gravity * deltaTime;

            Vector3 motion = horizontalVelocity;
            motion.y = verticalVelocity;
            characterController.Move(motion * deltaTime);
        }

        #endregion
    }
}
