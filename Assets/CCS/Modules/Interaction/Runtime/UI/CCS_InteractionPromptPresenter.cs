using TMPro;
using Unity.Netcode;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_InteractionPromptPresenter
// CATEGORY: Modules / Interaction / Runtime / UI
// PURPOSE: Presents the local-owner interaction prompt when pickup is ready.
// PLACEMENT: InteractionPromptHudRoot on PF_CCS_CharacterController_Player_Networked.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Hidden by default. Shows CurrentPromptText only when pickup is ready.
// =============================================================================

namespace CCS.Modules.Interaction
{
    public sealed class CCS_InteractionPromptPresenter : MonoBehaviour
    {
        #region Variables

        [SerializeField] private Canvas hudCanvas;
        [SerializeField] private TextMeshProUGUI promptText;
        [SerializeField] private float promptFontSize = CCS_InteractionConstants.InteractionPromptFontSize;
        [SerializeField] private Component interactionTargetSourceComponent;
        [SerializeField] private Component interactionBusySourceComponent;

        private CCS_IInteractionTargetSource interactionTargetSource;
        private CCS_IInteractionBusySource interactionBusySource;
        private bool isLocalOwnerActive;
        private bool promptVisible;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            ResolveReferences();
            ApplyPromptStyle();
            SetPromptVisible(false);
        }

        private void OnEnable()
        {
            ResolveReferences();
            isLocalOwnerActive = IsLocalOwner();
            if (!isLocalOwnerActive)
            {
                if (hudCanvas != null)
                {
                    hudCanvas.enabled = false;
                }

                SetPromptVisible(false);
                gameObject.SetActive(false);
                return;
            }

            if (interactionTargetSource == null)
            {
                Debug.LogWarning(
                    "[Interaction] Prompt presenter missing interaction target source.",
                    this);
            }

            SetPromptVisible(false);
        }

        private void Update()
        {
            if (!isLocalOwnerActive || interactionTargetSource == null)
            {
                return;
            }

            bool shouldShow = interactionTargetSource.HasPickupReadyTarget && !IsInteractionBusy();
            if (shouldShow && promptText != null)
            {
                promptText.text = interactionTargetSource.CurrentPromptText;
            }

            SetPromptVisible(shouldShow);
        }

        #endregion

        #region Private Methods

        private void ResolveReferences()
        {
            if (hudCanvas == null)
            {
                hudCanvas = GetComponent<Canvas>();
            }

            if (interactionTargetSource == null)
            {
                if (interactionTargetSourceComponent is CCS_IInteractionTargetSource fromComponent)
                {
                    interactionTargetSource = fromComponent;
                }
                else
                {
                    interactionTargetSource = GetComponentInParent<CCS_IInteractionTargetSource>();
                }
            }

            if (interactionBusySource == null)
            {
                if (interactionBusySourceComponent is CCS_IInteractionBusySource busyFromComponent)
                {
                    interactionBusySource = busyFromComponent;
                }
                else
                {
                    interactionBusySource = GetComponentInParent<CCS_IInteractionBusySource>();
                }
            }
        }

        private bool IsInteractionBusy()
        {
            return interactionBusySource != null && interactionBusySource.IsInteractionBusy;
        }

        private void ApplyPromptStyle()
        {
            if (promptText == null)
            {
                return;
            }

            promptText.fontSize = promptFontSize;
            promptText.alignment = TextAlignmentOptions.Center;
            promptText.text = CCS_InteractionConstants.DefaultInteractionPromptText;
        }

        private void SetPromptVisible(bool visible)
        {
            if (promptVisible == visible)
            {
                return;
            }

            promptVisible = visible;

            if (promptText != null)
            {
                promptText.enabled = visible;
            }
        }

        private bool IsLocalOwner()
        {
            NetworkObject networkObject = GetComponentInParent<NetworkObject>();
            if (networkObject == null || !networkObject.IsSpawned)
            {
                return true;
            }

            return networkObject.IsOwner;
        }

        #endregion
    }
}
