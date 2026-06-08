using UnityEngine;
using UnityEngine.InputSystem;

// =============================================================================
// SCRIPT: CCS_CharacterInputActionProvider
// CATEGORY: Modules / CharacterController / Runtime / Components
// PURPOSE: Reads module-owned Input System actions and exposes input state.
// PLACEMENT: PF_CCS_CharacterController_TestPlayer root.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: New Input System only. Tracks last-used device when practical.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public sealed class CCS_CharacterInputActionProvider : MonoBehaviour
    {
        #region Variables

        [Header("Input Actions")]
        [Tooltip("Module-owned Input Actions asset.")]
        [SerializeField] private InputActionAsset inputActionsAsset;

        [Header("Cursor")]
        [Tooltip("Lock cursor on enable for third-person control.")]
        [SerializeField] private bool lockCursorOnEnable = true;

        private InputActionMap gameplayMap;
        private InputAction moveAction;
        private InputAction lookAction;
        private InputAction sprintAction;
        private InputAction jumpAction;
        private InputAction toggleCursorAction;
        private InputAction cameraZoomAction;
        private bool cursorLocked = true;
        private string lastInputDeviceLabel = "None";

        #endregion

        #region Properties

        public Vector2 MoveInput => moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;

        public Vector2 LookInput => lookAction != null ? lookAction.ReadValue<Vector2>() : Vector2.zero;

        public bool SprintHeld => sprintAction != null && sprintAction.IsPressed();

        public bool JumpPressedThisFrame => jumpAction != null && jumpAction.WasPressedThisFrame();

        public float CameraZoomInput => cameraZoomAction != null ? cameraZoomAction.ReadValue<float>() : 0f;

        public bool CursorLocked => cursorLocked;

        public string LastInputDeviceLabel => lastInputDeviceLabel;

        public InputActionAsset InputActionsAsset => inputActionsAsset;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            BindActions();
        }

        private void OnEnable()
        {
            gameplayMap?.Enable();
            if (lockCursorOnEnable)
            {
                SetCursorLocked(true);
            }
        }

        private void OnDisable()
        {
            gameplayMap?.Disable();
        }

        private void Update()
        {
            UpdateLastInputDevice();
            HandleToggleCursor();
        }

        #endregion

        #region Public Methods

        public void SetInputActionsAsset(InputActionAsset asset)
        {
            inputActionsAsset = asset;
            BindActions();
        }

        public void SetCursorLocked(bool locked)
        {
            cursorLocked = locked;
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }

        #endregion

        #region Private Methods

        private void BindActions()
        {
            gameplayMap = null;
            moveAction = null;
            lookAction = null;
            sprintAction = null;
            jumpAction = null;
            toggleCursorAction = null;
            cameraZoomAction = null;

            if (inputActionsAsset == null)
            {
                return;
            }

            gameplayMap = inputActionsAsset.FindActionMap(CCS_CharacterControllerConstants.InputActionMapName, true);
            moveAction = gameplayMap.FindAction(CCS_CharacterControllerConstants.MoveActionName, true);
            lookAction = gameplayMap.FindAction(CCS_CharacterControllerConstants.LookActionName, true);
            sprintAction = gameplayMap.FindAction(CCS_CharacterControllerConstants.SprintActionName, true);
            jumpAction = gameplayMap.FindAction(CCS_CharacterControllerConstants.JumpActionName, true);
            toggleCursorAction = gameplayMap.FindAction(CCS_CharacterControllerConstants.ToggleCursorActionName, true);
            cameraZoomAction = gameplayMap.FindAction(CCS_CharacterControllerConstants.CameraZoomActionName, true);
        }

        private void UpdateLastInputDevice()
        {
            InputControl activeControl = null;

            if (moveAction != null && moveAction.IsInProgress())
            {
                activeControl = moveAction.activeControl;
            }
            else if (lookAction != null && lookAction.IsInProgress())
            {
                activeControl = lookAction.activeControl;
            }
            else if (sprintAction != null && sprintAction.IsPressed())
            {
                activeControl = sprintAction.activeControl;
            }

            if (activeControl?.device == null)
            {
                return;
            }

            InputDevice device = activeControl.device;
            if (device is Gamepad)
            {
                lastInputDeviceLabel = "Gamepad";
                return;
            }

            if (device is Keyboard || device is Mouse)
            {
                lastInputDeviceLabel = "Keyboard/Mouse";
                return;
            }

            lastInputDeviceLabel = device.displayName;
        }

        private void HandleToggleCursor()
        {
            if (toggleCursorAction == null || !toggleCursorAction.WasPressedThisFrame())
            {
                return;
            }

            SetCursorLocked(!cursorLocked);
        }

        #endregion
    }
}
