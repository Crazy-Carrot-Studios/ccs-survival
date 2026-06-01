using UnityEngine;
using UnityEngine.InputSystem;

// =============================================================================
// SCRIPT: CCS_CharacterInputActionProvider
// CATEGORY: Modules / CharacterController / Runtime / Input
// PURPOSE: Reads CCS_Survival_InputActions Gameplay map and produces input snapshots.
// PLACEMENT: PF_CCS_Player alongside CCS_PlayerGameplayController.
// AUTHOR: James Schilz
// CREATED: 2026-05-31
// NOTES: Manual CCS_CharacterInputRuntimeBridge remains for tests. No rebinding UI.
// =============================================================================

namespace CCS.Modules.CharacterController
{
    public sealed class CCS_CharacterInputActionProvider : MonoBehaviour, CCS_ICharacterInputProvider
    {
        #region Variables

        [Header("Input Actions")]
        [Tooltip("Survival input actions asset containing Gameplay and UI action maps.")]
        [SerializeField] private InputActionAsset inputActions;

        private InputAction moveAction;
        private InputAction lookAction;
        private InputAction jumpAction;
        private InputAction sprintAction;
        private InputAction crouchAction;
        private InputAction interactAction;
        private InputAction consumeAction;
        private InputAction primaryAction;
        private InputAction pauseAction;

        private bool sprintHeld;
        private bool crouchHeld;
        private bool jumpPressed;
        private bool interactPressed;
        private bool consumePressed;
        private bool primaryActionPressed;
        private bool pausePressed;
        private bool sprintAllowed = true;
        private bool inputEnabled = true;

        #endregion

        #region Properties

        public bool InputEnabled
        {
            get => inputEnabled;
            set => inputEnabled = value;
        }

        public bool InteractPressedThisFrame => interactPressed;

        public bool ConsumePressedThisFrame => consumePressed;

        public bool PrimaryActionPressedThisFrame => primaryActionPressed;

        public bool PausePressedThisFrame => pausePressed;

        public bool SprintAllowed
        {
            get => sprintAllowed;
            set => sprintAllowed = value;
        }

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            ResolveActions();
        }

        private void OnEnable()
        {
            EnableGameplayActions();
        }

        private void OnDisable()
        {
            DisableGameplayActions();
        }

        private void Update()
        {
            if (moveAction == null)
            {
                return;
            }

            jumpPressed = jumpAction != null && jumpAction.WasPressedThisFrame();
            interactPressed = interactAction != null && interactAction.WasPressedThisFrame();
            consumePressed = consumeAction != null && consumeAction.WasPressedThisFrame();
            primaryActionPressed = primaryAction != null && primaryAction.WasPressedThisFrame();
            pausePressed = pauseAction != null && pauseAction.WasPressedThisFrame();
            sprintHeld = sprintAllowed && sprintAction != null && sprintAction.IsPressed();
            crouchHeld = crouchAction != null && crouchAction.IsPressed();
        }

        #endregion

        #region Public Methods

        public void SetInputActions(InputActionAsset actions)
        {
            DisableGameplayActions();
            inputActions = actions;
            ResolveActions();
            EnableGameplayActions();
        }

        public CCS_CharacterInputSnapshot GetInputSnapshot()
        {
            if (!inputEnabled)
            {
                return CCS_CharacterInputSnapshot.Empty;
            }

            Vector2 move = moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;
            Vector2 look = lookAction != null ? lookAction.ReadValue<Vector2>() : Vector2.zero;

            return new CCS_CharacterInputSnapshot(
                move,
                look,
                jumpPressed,
                sprintHeld,
                crouchHeld);
        }

        public void EnableGameplayActions()
        {
            moveAction?.Enable();
            lookAction?.Enable();
            jumpAction?.Enable();
            sprintAction?.Enable();
            crouchAction?.Enable();
            interactAction?.Enable();
            consumeAction?.Enable();
            primaryAction?.Enable();
            pauseAction?.Enable();
        }

        public void DisableGameplayActions()
        {
            moveAction?.Disable();
            lookAction?.Disable();
            jumpAction?.Disable();
            sprintAction?.Disable();
            crouchAction?.Disable();
            interactAction?.Disable();
            consumeAction?.Disable();
            primaryAction?.Disable();
            pauseAction?.Disable();
        }

        #endregion

        #region Private Methods

        private void ResolveActions()
        {
            moveAction = null;
            lookAction = null;
            jumpAction = null;
            sprintAction = null;
            crouchAction = null;
            interactAction = null;
            consumeAction = null;
            primaryAction = null;
            pauseAction = null;

            if (inputActions == null)
            {
                return;
            }

            InputActionMap gameplayMap = inputActions.FindActionMap("Gameplay", throwIfNotFound: false);
            if (gameplayMap == null)
            {
                Debug.LogWarning("[CCS_CharacterInputActionProvider] Gameplay action map not found.");
                return;
            }

            moveAction = gameplayMap.FindAction("Move", throwIfNotFound: false);
            lookAction = gameplayMap.FindAction("Look", throwIfNotFound: false);
            jumpAction = gameplayMap.FindAction("Jump", throwIfNotFound: false);
            sprintAction = gameplayMap.FindAction("Sprint", throwIfNotFound: false);
            crouchAction = gameplayMap.FindAction("Crouch", throwIfNotFound: false);
            interactAction = gameplayMap.FindAction("Interact", throwIfNotFound: false);
            consumeAction = gameplayMap.FindAction("Consume", throwIfNotFound: false);
            primaryAction = gameplayMap.FindAction("PrimaryAction", throwIfNotFound: false);
            pauseAction = gameplayMap.FindAction("Pause", throwIfNotFound: false);
        }

        #endregion
    }
}
