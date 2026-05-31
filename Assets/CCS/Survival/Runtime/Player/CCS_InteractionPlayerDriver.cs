using CCS.Modules.CharacterController;
using CCS.Modules.Interaction;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_InteractionPlayerDriver
// CATEGORY: Survival / Runtime / Player
// PURPOSE: Drives interaction scan and requests from the player camera forward ray.
// PLACEMENT: PF_CCS_Player alongside CCS_PlayerGameplayController.
// AUTHOR: James Schilz
// CREATED: 2026-05-31
// NOTES: Uses Interaction service only. Prompt HUD updates through existing UI flow.
// =============================================================================

namespace CCS.Survival.Player
{
    [DefaultExecutionOrder(220)]
    public sealed class CCS_InteractionPlayerDriver : MonoBehaviour
    {
        #region Variables

        [Header("Interaction Scan")]
        [Tooltip("Camera used for forward interaction raycasts. Defaults to child camera.")]
        [SerializeField] private Camera interactionCamera;

        private CCS_CharacterInputActionProvider inputProvider;
        private CCS_InteractionService interactionService;
        private bool serviceResolved;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            inputProvider = GetComponent<CCS_CharacterInputActionProvider>();

            if (interactionCamera == null)
            {
                interactionCamera = GetComponentInChildren<Camera>();
            }
        }

        private void Update()
        {
            if (!serviceResolved)
            {
                serviceResolved = CCS_InteractionRuntimeBridge.TryGetInteractionService(out interactionService)
                    && interactionService != null
                    && interactionService.IsInitialized;
            }

            if (!serviceResolved || interactionCamera == null)
            {
                return;
            }

            Transform cameraTransform = interactionCamera.transform;
            interactionService.TickScan(cameraTransform.position, cameraTransform.forward);

            if (inputProvider != null && inputProvider.InteractPressedThisFrame)
            {
                interactionService.RequestInteraction();
            }
        }

        #endregion
    }
}
