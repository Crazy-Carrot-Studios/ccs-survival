using UnityEngine;
using UnityEngine.UI;

// =============================================================================
// SCRIPT: CCS_InteractionPromptPresenter
// CATEGORY: Modules / UI / Runtime / Presentation
// PURPOSE: Displays the current interaction prompt from HUD presentation service.
// PLACEMENT: Child of PF_CCS_HUD_Root interaction prompt area.
// AUTHOR: James Schilz
// CREATED: 2026-05-30
// NOTES: Read-only display. No interaction mutation.
// =============================================================================

namespace CCS.Modules.UI
{
    public sealed class CCS_InteractionPromptPresenter : MonoBehaviour
    {
        #region Variables

        [SerializeField] private Text promptText;

        private CCS_HudPresentationService presentationService;

        #endregion

        #region Public Methods

        public void Bind(CCS_HudPresentationService service, CCS_HudProfile profile)
        {
            Unbind();
            presentationService = service;

            if (presentationService != null)
            {
                presentationService.InteractionPromptChanged += HandlePromptChanged;
                presentationService.HudInitialized += HandlePromptChanged;
            }

            gameObject.SetActive(profile == null || profile.ShowInteractionPrompt);
            RefreshDisplay();
        }

        public void Unbind()
        {
            if (presentationService != null)
            {
                presentationService.InteractionPromptChanged -= HandlePromptChanged;
                presentationService.HudInitialized -= HandlePromptChanged;
            }

            presentationService = null;
        }

        #endregion

        #region Unity Callbacks

        private void OnDestroy()
        {
            Unbind();
        }

        #endregion

        #region Private Methods

        private void HandlePromptChanged(CCS_HudEventArgs eventArgs)
        {
            RefreshDisplay();
        }

        private void RefreshDisplay()
        {
            if (promptText == null)
            {
                return;
            }

            string prompt = presentationService != null
                ? presentationService.CurrentInteractionPrompt
                : string.Empty;

            bool hasPrompt = !string.IsNullOrWhiteSpace(prompt);
            promptText.text = hasPrompt ? prompt : string.Empty;
            promptText.gameObject.SetActive(hasPrompt);
        }

        #endregion
    }
}
