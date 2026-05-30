using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// =============================================================================
// SCRIPT: CCS_NotificationQueue
// CATEGORY: Modules / UI / Runtime / Presentation
// PURPOSE: Manages transient HUD notification messages with lifetime dismissal.
// PLACEMENT: Child of PF_CCS_HUD_Root notification area.
// AUTHOR: James Schilz
// CREATED: 2026-05-30
// NOTES: Presentation-only. Does not mutate gameplay systems.
// =============================================================================

namespace CCS.Modules.UI
{
    public sealed class CCS_NotificationQueue : MonoBehaviour
    {
        #region Variables

        [SerializeField] private RectTransform notificationContainer;
        [SerializeField] private CCS_NotificationPresenter notificationTemplate;

        private readonly List<CCS_NotificationPresenter> activePresenters = new List<CCS_NotificationPresenter>(8);
        private CCS_HudPresentationService presentationService;
        private CCS_HudProfile hudProfile;
        private float defaultLifetimeSeconds = 4f;
        private int maxVisibleCount = 4;

        #endregion

        #region Public Methods

        public void Bind(CCS_HudPresentationService service, CCS_HudProfile profile)
        {
            Unbind();
            presentationService = service;
            hudProfile = profile;

            if (profile != null && profile.NotificationProfile != null)
            {
                defaultLifetimeSeconds = profile.NotificationProfile.NotificationLifetimeSeconds;
                maxVisibleCount = profile.NotificationProfile.MaxVisibleCount;
            }

            if (presentationService != null)
            {
                presentationService.NotificationQueued += HandleNotificationQueued;
                presentationService.NotificationDismissed += HandleNotificationDismissed;
            }

            gameObject.SetActive(profile == null || profile.ShowNotifications);
        }

        public void Unbind()
        {
            if (presentationService != null)
            {
                presentationService.NotificationQueued -= HandleNotificationQueued;
                presentationService.NotificationDismissed -= HandleNotificationDismissed;
            }

            presentationService = null;
            hudProfile = null;
            ClearAllPresenters();
        }

        #endregion

        #region Unity Callbacks

        private void Update()
        {
            for (int index = activePresenters.Count - 1; index >= 0; index--)
            {
                CCS_NotificationPresenter presenter = activePresenters[index];
                if (presenter == null)
                {
                    activePresenters.RemoveAt(index);
                    continue;
                }

                if (presenter.TickLifetime(Time.deltaTime))
                {
                    Destroy(presenter.gameObject);
                    activePresenters.RemoveAt(index);
                }
            }
        }

        private void OnDestroy()
        {
            Unbind();
        }

        #endregion

        #region Private Methods

        private void HandleNotificationQueued(CCS_HudEventArgs eventArgs)
        {
            if (eventArgs == null || string.IsNullOrWhiteSpace(eventArgs.Message))
            {
                return;
            }

            while (activePresenters.Count >= maxVisibleCount)
            {
                CCS_NotificationPresenter oldest = activePresenters[0];
                activePresenters.RemoveAt(0);
                if (oldest != null)
                {
                    Destroy(oldest.gameObject);
                }
            }

            CCS_NotificationPresenter presenter = Instantiate(notificationTemplate, notificationContainer);
            presenter.gameObject.SetActive(true);
            presenter.Show(eventArgs.Message, defaultLifetimeSeconds);
            activePresenters.Add(presenter);
        }

        private void HandleNotificationDismissed(CCS_HudEventArgs eventArgs)
        {
            ClearAllPresenters();
        }

        private void ClearAllPresenters()
        {
            for (int index = 0; index < activePresenters.Count; index++)
            {
                if (activePresenters[index] != null)
                {
                    Destroy(activePresenters[index].gameObject);
                }
            }

            activePresenters.Clear();
        }

        #endregion
    }
}
