using UnityEngine;
using UnityEngine.UI;

// =============================================================================
// SCRIPT: CCS_WildlifeAiDebugPresenter
// CATEGORY: Modules / UI / Runtime / Presentation
// PURPOSE: Optional upper-right debug readout for passive wildlife AI states.
// PLACEMENT: Child of PF_CCS_HUD_Root wildlife AI debug area.
// AUTHOR: James Schilz
// CREATED: 2026-06-01
// NOTES: Read-only display. No gameplay mutation.
// =============================================================================

namespace CCS.Modules.UI
{
    public sealed class CCS_WildlifeAiDebugPresenter : MonoBehaviour
    {
        #region Variables

        [SerializeField] private Text debugText;

        private CCS_HudPresentationService presentationService;

        #endregion

        #region Public Methods

        public void Bind(CCS_HudPresentationService service, CCS_HudProfile profile)
        {
            Unbind();
            presentationService = service;

            if (presentationService != null)
            {
                presentationService.HudDataRefreshed += HandleHudDataRefreshed;
                presentationService.HudInitialized += HandleHudDataRefreshed;
            }

            gameObject.SetActive(true);
            ApplyTypography(profile);
            RefreshDisplay();
        }

        public void Unbind()
        {
            if (presentationService != null)
            {
                presentationService.HudDataRefreshed -= HandleHudDataRefreshed;
                presentationService.HudInitialized -= HandleHudDataRefreshed;
            }

            presentationService = null;
        }

        #endregion

        #region Unity Callbacks

        private void LateUpdate()
        {
            RefreshDisplay();
        }

        private void OnDestroy()
        {
            Unbind();
        }

        #endregion

        #region Private Methods

        private void HandleHudDataRefreshed(CCS_HudEventArgs eventArgs)
        {
            RefreshDisplay();
        }

        private void RefreshDisplay()
        {
            if (debugText == null)
            {
                return;
            }

            debugText.text = presentationService == null
                ? "Wildlife:\n--"
                : presentationService.WildlifeAiDebugLabel;
        }

        private void ApplyTypography(CCS_HudProfile profile)
        {
            if (profile?.LayoutSettings == null || debugText == null)
            {
                return;
            }

            CCS_HudLayoutApplicator.ApplyTypography(debugText, profile.LayoutSettings.SummaryFontSize);
        }

        #endregion
    }
}
