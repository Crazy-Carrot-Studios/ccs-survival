using System;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_NotificationProfile
// CATEGORY: Modules / UI / Runtime / Profiles
// PURPOSE: Serializable notification queue tuning for HUD presentation.
// PLACEMENT: Embedded on CCS_HudProfile or referenced as nested settings.
// AUTHOR: James Schilz
// CREATED: 2026-05-30
// NOTES: Presentation-only settings. No gameplay mutation.
// =============================================================================

namespace CCS.Modules.UI
{
    [Serializable]
    public sealed class CCS_NotificationProfile
    {
        #region Variables

        [Tooltip("Maximum number of notifications visible at once.")]
        [SerializeField] private int maxVisibleCount = 4;

        [Tooltip("Seconds before a notification is dismissed automatically.")]
        [SerializeField] private float notificationLifetimeSeconds = 4f;

        #endregion

        #region Properties

        public int MaxVisibleCount => maxVisibleCount < 1 ? 1 : maxVisibleCount;

        public float NotificationLifetimeSeconds =>
            notificationLifetimeSeconds < 0.5f ? 0.5f : notificationLifetimeSeconds;

        #endregion
    }
}
