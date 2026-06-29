using System;
using CCS.Modules.CharacterController.Diagnostics;
using CCS.Modules.CharacterController.Local;

// =============================================================================
// SCRIPT: CCS_PlayerJoinNotificationFeedRegistry
// CATEGORY: Modules / CharacterController / Tests / Runtime
// PURPOSE: Scene-scoped access to the master test join notification feed.
// PLACEMENT: Runtime static registry. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Master test scene registers one feed instance at runtime.
// =============================================================================

namespace CCS.Modules.CharacterController {
    public static class CCS_PlayerJoinNotificationFeedRegistry
    {
        #region Variables

        private static CCS_PlayerJoinNotificationFeed activeFeed;

        #endregion

        #region Properties

        public static bool IsRegistered => activeFeed != null;

        #endregion

        #region Events

        public static event Action FeedRegistered;

        #endregion

        #region Public Methods

        public static void Register(CCS_PlayerJoinNotificationFeed feed)
        {
            activeFeed = feed;
            CCS_JoinFeedDebugLog.FeedRegistered();
            FeedRegistered?.Invoke();
            CCS_PlayerSessionEvents.RaiseJoinNotificationFeedReady();
        }

        public static void Unregister(CCS_PlayerJoinNotificationFeed feed)
        {
            if (activeFeed == feed)
            {
                activeFeed = null;
            }
        }

        public static bool TryShowPlayerJoined(string playerName)
        {
            if (!CCS_NetworkSessionUtility.CanProcessJoinNotifications() || activeFeed == null)
            {
                return false;
            }

            activeFeed.ShowPlayerJoined(playerName);
            return true;
        }

        public static void ShowPlayerJoined(string playerName)
        {
            TryShowPlayerJoined(playerName);
        }

        public static void Clear()
        {
            activeFeed?.Clear();
        }

        #endregion
    }
}
