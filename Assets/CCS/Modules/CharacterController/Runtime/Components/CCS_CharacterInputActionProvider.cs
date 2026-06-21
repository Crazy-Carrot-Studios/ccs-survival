using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// =============================================================================
// SCRIPT: CCS_CharacterInputActionProvider
// CATEGORY: Modules / CharacterController / Runtime / Components
// PURPOSE: Reads module-owned Input System actions and exposes input state.
// PLACEMENT: PF_CCS_CharacterController_TestPlayer_Networked root.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Look is diagnostics-only here; Cinemachine owns camera look on the scene rig.
//        Shared InputActionAsset ref-count prevents remote providers from disabling look.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public sealed class CCS_CharacterInputActionProvider : MonoBehaviour
    {
        #region Variables

        private static readonly Dictionary<InputActionAsset, int> SharedActionAssetEnableCounts =
            new Dictionary<InputActionAsset, int>();

        [Header("Input Actions")]
        [SerializeField] private InputActionAsset inputActionsAsset;

        [Header("Cursor")]
        [SerializeField] private bool lockCursorOnEnable = true;

        private InputActionMap gameplayMap;
        private InputAction moveAction;
        private InputAction lookAction;
        private InputAction sprintAction;
        private InputAction jumpAction;
        private InputAction toggleCursorAction;
        private InputAction cameraZoomAction;
        private InputAction aimAction;
        private InputAction fireAction;
        private InputAction reloadAction;
        private bool cursorLocked = true;
        private bool inputAccepted = true;
        private bool sharedMapEnableHeld;
        private string lastInputDeviceLabel = "None";
        private Vector2 externalMoveInput;
        private bool externalMoveActive;

        #endregion

        #region Properties

        public bool InputAccepted => inputAccepted;

        public Vector2 MoveInput =>
            externalMoveActive
                ? externalMoveInput
                : HasAcceptedFocusedInput && moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;

        public Vector2 LookInput =>
            HasAcceptedFocusedInput && lookAction != null ? lookAction.ReadValue<Vector2>() : Vector2.zero;

        public bool SprintHeld =>
            HasAcceptedFocusedInput && sprintAction != null && sprintAction.IsPressed();

        public bool JumpPressed => JumpPressedThisFrame;

        public bool JumpPressedThisFrame =>
            HasAcceptedFocusedInput && jumpAction != null && jumpAction.WasPressedThisFrame();

        public float CameraZoomInput =>
            HasAcceptedFocusedInput && cameraZoomAction != null ? cameraZoomAction.ReadValue<float>() : 0f;

        public bool AimHeld =>
            HasAcceptedFocusedInput && aimAction != null && aimAction.IsPressed();

        public bool FirePressed =>
            HasAcceptedFocusedInput && fireAction != null && fireAction.WasPressedThisFrame();

        public bool ReloadPressed =>
            HasAcceptedFocusedInput && reloadAction != null && reloadAction.WasPressedThisFrame();

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
            AcquireSharedActionMap();
            ApplyCursorLockForCurrentState();
        }

        private void OnDisable()
        {
            ReleaseSharedActionMap();
        }

        private void Update()
        {
            if (!HasAcceptedFocusedInput)
            {
                return;
            }

            UpdateLastInputDevice();
            HandleToggleCursor();
        }

        #endregion

        #region Public Methods

        public void SetInputActionsAsset(InputActionAsset asset)
        {
            if (isActiveAndEnabled)
            {
                ReleaseSharedActionMap();
            }

            inputActionsAsset = asset;
            BindActions();

            if (isActiveAndEnabled)
            {
                AcquireSharedActionMap();
            }
        }

        public void SetInputAccepted(bool accepted)
        {
            inputAccepted = accepted;
            ApplyCursorLockForCurrentState();
        }

        public void SetCursorLocked(bool locked)
        {
            cursorLocked = locked;
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }

        public void SetExternalMoveInput(Vector2 moveInput)
        {
            externalMoveInput = moveInput;
            externalMoveActive = moveInput.sqrMagnitude > 0.0001f;
            inputAccepted = true;
        }

        public void ClearExternalMoveInput()
        {
            externalMoveInput = Vector2.zero;
            externalMoveActive = false;
        }

        #endregion

        #region Private Methods

        private bool HasAcceptedFocusedInput => inputAccepted && (externalMoveActive || Application.isFocused);

        private void BindActions()
        {
            gameplayMap = null;
            moveAction = null;
            lookAction = null;
            sprintAction = null;
            jumpAction = null;
            toggleCursorAction = null;
            cameraZoomAction = null;
            aimAction = null;
            fireAction = null;
            reloadAction = null;

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
            aimAction = gameplayMap.FindAction(CCS_CharacterControllerConstants.AimActionName, true);
            fireAction = gameplayMap.FindAction(CCS_CharacterControllerConstants.FireActionName, true);
            reloadAction = gameplayMap.FindAction(CCS_CharacterControllerConstants.ReloadActionName, true);
        }

        private void AcquireSharedActionMap()
        {
            if (gameplayMap == null || inputActionsAsset == null || sharedMapEnableHeld)
            {
                return;
            }

            InputActionAsset assetKey = inputActionsAsset;
            SharedActionAssetEnableCounts.TryGetValue(assetKey, out int enableCount);
            if (enableCount == 0)
            {
                gameplayMap.Enable();
            }

            SharedActionAssetEnableCounts[assetKey] = enableCount + 1;
            sharedMapEnableHeld = true;
        }

        private void ReleaseSharedActionMap()
        {
            if (gameplayMap == null || inputActionsAsset == null || !sharedMapEnableHeld)
            {
                return;
            }

            InputActionAsset assetKey = inputActionsAsset;
            if (!SharedActionAssetEnableCounts.TryGetValue(assetKey, out int enableCount))
            {
                sharedMapEnableHeld = false;
                return;
            }

            enableCount--;
            if (enableCount <= 0)
            {
                gameplayMap.Disable();
                SharedActionAssetEnableCounts.Remove(assetKey);
            }
            else
            {
                SharedActionAssetEnableCounts[assetKey] = enableCount;
            }

            sharedMapEnableHeld = false;
        }

        private void ApplyCursorLockForCurrentState()
        {
            if (!isActiveAndEnabled || !inputAccepted || !Application.isFocused)
            {
                SetCursorLocked(false);
                return;
            }

            if (lockCursorOnEnable)
            {
                SetCursorLocked(true);
            }
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
            else if (jumpAction != null && jumpAction.IsPressed())
            {
                activeControl = jumpAction.activeControl;
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
