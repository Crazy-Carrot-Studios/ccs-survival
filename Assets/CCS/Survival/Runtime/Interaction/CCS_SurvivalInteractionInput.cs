using CCS.Core;
using UnityEngine;
using UnityEngine.InputSystem;

// =============================================================================
// SCRIPT: CCS_SurvivalInteractionInput
// CATEGORY: Survival / Runtime / Interaction
// PURPOSE: Routes New Input System interact action (and keyboard fallback) to the scanner.
// PLACEMENT: Attach to CCS_PlayerRoot with CCS_SurvivalInteractionScanner.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Disabled with the player root during traversal validation.
// =============================================================================

namespace CCS.Survival.Interaction
{
    public sealed class CCS_SurvivalInteractionInput : MonoBehaviour
    {
        #region Variables

        [Header("References")]
        [Tooltip("Player interaction scanner that resolves targets and performs interactions.")]
        [SerializeField] private CCS_SurvivalInteractionScanner interactionScanner;

        [Header("Input Actions")]
        [Tooltip("Gameplay/Interact — E and gamepad west button.")]
        [SerializeField] private InputActionReference interactAction;

        [Header("Prototype Fallback")]
        [Tooltip("When no InputActionReference is assigned, use this keyboard key in Play Mode.")]
        [SerializeField] private Key keyboardFallbackKey = Key.E;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            if (!CCS_Validation.IsObjectValid(interactionScanner))
            {
                interactionScanner = GetComponent<CCS_SurvivalInteractionScanner>();
            }
        }

        private void OnEnable()
        {
            if (interactAction != null && interactAction.action != null)
            {
                interactAction.action.performed += OnInteractPerformed;
                interactAction.action.Enable();
            }
        }

        private void OnDisable()
        {
            if (interactAction != null && interactAction.action != null)
            {
                interactAction.action.performed -= OnInteractPerformed;
                interactAction.action.Disable();
            }
        }

        private void Update()
        {
            if (interactAction != null && interactAction.action != null)
            {
                return;
            }

            if (Keyboard.current != null && Keyboard.current[keyboardFallbackKey].wasPressedThisFrame)
            {
                TryPerformInteraction();
            }
        }

        #endregion

        #region Private Methods

        private void OnInteractPerformed(InputAction.CallbackContext context)
        {
            if (!context.performed)
            {
                return;
            }

            TryPerformInteraction();
        }

        private void TryPerformInteraction()
        {
            if (!CCS_Validation.IsObjectValid(interactionScanner))
            {
                return;
            }

            interactionScanner.TryPerformInteraction();
        }

        #endregion
    }
}
