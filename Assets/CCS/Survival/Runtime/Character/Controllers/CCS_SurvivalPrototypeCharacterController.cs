using UnityEngine;
using UnityEngine.InputSystem;

// =============================================================================
// SCRIPT: CCS_SurvivalPrototypeCharacterController
// CATEGORY: Survival / Character / Controllers
// PURPOSE: Prototype CharacterController locomotion for CCS_PlayerRoot via New Input System.
// PLACEMENT: Attach to CCS_PlayerRoot with CharacterController in SCN_CCS_Survival_Bootstrap.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: No survival vitals ownership. Root motion off. Look action not consumed yet.
// =============================================================================

namespace CCS.Survival
{
    public enum CCS_SurvivalPrototypeMovementSpace
    {
        CameraRelative = 0,
        WorldRelative = 1
    }

    [RequireComponent(typeof(CharacterController))]
    public sealed class CCS_SurvivalPrototypeCharacterController : MonoBehaviour
    {
        private const string LogPrefix = "[CCS Survival Movement]";

        #region Variables

        [Header("Input Actions")]
        [Tooltip("Gameplay/Move — WASD, arrows, and gamepad left stick.")]
        [SerializeField] private InputActionReference moveAction;

        [Tooltip("Gameplay/Sprint — Left Shift and gamepad left stick press.")]
        [SerializeField] private InputActionReference sprintAction;

        [Tooltip("Gameplay/Jump — optional; disabled when Enable Jump is off.")]
        [SerializeField] private InputActionReference jumpAction;

        [Tooltip("When enabled, applies jump impulse while grounded. Deferred by default for Phase 1E.")]
        [SerializeField] private bool enableJump;

        [Header("Movement")]
        [Tooltip("Walk speed in meters per second.")]
        [SerializeField] private float walkSpeed = 4f;

        [Tooltip("Sprint speed in meters per second.")]
        [SerializeField] private float sprintSpeed = 7f;

        [Tooltip("Gravity acceleration applied each frame when airborne.")]
        [SerializeField] private float gravity = -20f;

        [Tooltip("Jump height in meters when Enable Jump is on.")]
        [SerializeField] private float jumpHeight = 1.2f;

        [Tooltip("Rotation smoothing time when turning toward movement direction.")]
        [SerializeField] private float rotationSmoothTime = 0.12f;

        [Tooltip("Movement space used to resolve input direction during prototype testing.")]
        [SerializeField] private CCS_SurvivalPrototypeMovementSpace movementSpace = CCS_SurvivalPrototypeMovementSpace.CameraRelative;

        [Header("Camera")]
        [Tooltip("Camera transform for camera-relative movement (e.g. Main Camera). Falls back to world axes if unset.")]
        [SerializeField] private Transform cameraTransform;

        [SerializeField] private bool enableDebugLogs;

        private CharacterController characterController;
        private Vector3 verticalVelocity;
        private float rotationVelocity;
        private bool loggedMissingCamera;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
        }

        private void OnEnable()
        {
            EnableInputAction(moveAction);
            EnableInputAction(sprintAction);
            if (enableJump)
            {
                EnableInputAction(jumpAction);
            }
        }

        private void OnDisable()
        {
            DisableInputAction(moveAction);
            DisableInputAction(sprintAction);
            DisableInputAction(jumpAction);
        }

        private void Update()
        {
            if (!characterController.enabled)
            {
                return;
            }

            ApplyGravityAndGrounding();

            Vector2 moveInput = ReadMoveInput();
            bool isSprinting = ReadSprintInput();

            if (enableJump && characterController.isGrounded && ReadJumpPressed())
            {
                verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            Vector3 moveDirection = ResolveMoveDirection(moveInput);
            float speed = isSprinting ? sprintSpeed : walkSpeed;
            Vector3 horizontalMove = moveDirection * (speed * Time.deltaTime);

            RotateTowardMovement(moveDirection);
            characterController.Move(horizontalMove + (verticalVelocity * Time.deltaTime));
        }

        #endregion

        #region Private Methods

        private void ApplyGravityAndGrounding()
        {
            if (characterController.isGrounded && verticalVelocity.y < 0f)
            {
                verticalVelocity.y = -2f;
            }

            verticalVelocity.y += gravity * Time.deltaTime;
        }

        private Vector2 ReadMoveInput()
        {
            if (moveAction == null || moveAction.action == null)
            {
                return Vector2.zero;
            }

            return moveAction.action.ReadValue<Vector2>();
        }

        private bool ReadSprintInput()
        {
            if (sprintAction == null || sprintAction.action == null)
            {
                return false;
            }

            return sprintAction.action.IsPressed();
        }

        private bool ReadJumpPressed()
        {
            if (jumpAction == null || jumpAction.action == null)
            {
                return false;
            }

            return jumpAction.action.WasPressedThisFrame();
        }

        private Vector3 ResolveMoveDirection(Vector2 moveInput)
        {
            if (moveInput.sqrMagnitude <= 0.0001f)
            {
                return Vector3.zero;
            }

            Vector3 input = new Vector3(moveInput.x, 0f, moveInput.y);
            input.Normalize();

            if (movementSpace == CCS_SurvivalPrototypeMovementSpace.WorldRelative)
            {
                return input;
            }

            if (cameraTransform != null)
            {
                Vector3 forward = cameraTransform.forward;
                forward.y = 0f;
                if (forward.sqrMagnitude > 0.0001f)
                {
                    forward.Normalize();
                }

                Vector3 right = cameraTransform.right;
                right.y = 0f;
                if (right.sqrMagnitude > 0.0001f)
                {
                    right.Normalize();
                }

                return (forward * input.z) + (right * input.x);
            }

            if (!loggedMissingCamera)
            {
                Debug.LogWarning($"{LogPrefix} cameraTransform not assigned; using world-relative movement.");
                loggedMissingCamera = true;
            }

            return input;
        }

        private void RotateTowardMovement(Vector3 moveDirection)
        {
            if (moveDirection.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
            float smoothedAngle = Mathf.SmoothDampAngle(
                transform.eulerAngles.y,
                targetAngle,
                ref rotationVelocity,
                rotationSmoothTime);
            transform.rotation = Quaternion.Euler(0f, smoothedAngle, 0f);
        }

        private static void EnableInputAction(InputActionReference actionReference)
        {
            if (actionReference != null && actionReference.action != null)
            {
                actionReference.action.Enable();
            }
        }

        private static void DisableInputAction(InputActionReference actionReference)
        {
            if (actionReference != null && actionReference.action != null)
            {
                actionReference.action.Disable();
            }
        }

        #endregion
    }
}
