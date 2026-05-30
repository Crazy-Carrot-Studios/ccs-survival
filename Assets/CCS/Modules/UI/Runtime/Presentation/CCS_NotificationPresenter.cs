using UnityEngine;
using UnityEngine.UI;

// =============================================================================
// SCRIPT: CCS_NotificationPresenter
// CATEGORY: Modules / UI / Runtime / Presentation
// PURPOSE: Displays one transient HUD notification line.
// PLACEMENT: Instantiated by CCS_NotificationQueue.
// AUTHOR: James Schilz
// CREATED: 2026-05-30
// NOTES: Presentation-only placeholder UI.
// =============================================================================

namespace CCS.Modules.UI
{
    public sealed class CCS_NotificationPresenter : MonoBehaviour
    {
        #region Variables

        [SerializeField] private Text messageText;
        [SerializeField] private Image backgroundImage;

        private float remainingLifetimeSeconds;

        #endregion

        #region Public Methods

        public void ConfigureLayout(float rowHeight, int fontSize)
        {
            LayoutElement layoutElement = GetComponent<LayoutElement>();
            if (layoutElement == null)
            {
                layoutElement = gameObject.AddComponent<LayoutElement>();
            }

            layoutElement.preferredHeight = rowHeight;
            layoutElement.minHeight = rowHeight;

            if (messageText != null)
            {
                CCS_HudLayoutApplicator.ApplyTypography(messageText, fontSize);
            }
        }

        public void Show(string message, float lifetimeSeconds)
        {
            if (messageText != null)
            {
                messageText.text = message;
            }

            remainingLifetimeSeconds = lifetimeSeconds;
            gameObject.SetActive(true);
        }

        public bool TickLifetime(float deltaTime)
        {
            remainingLifetimeSeconds -= deltaTime;
            return remainingLifetimeSeconds <= 0f;
        }

        #endregion
    }
}
