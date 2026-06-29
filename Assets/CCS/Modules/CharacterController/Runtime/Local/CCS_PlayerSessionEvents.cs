using System;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_PlayerSessionEvents
// CATEGORY: Modules / CharacterController / Tests / Runtime
// PURPOSE: Decoupled session events for test player spawn, readiness, and join feed flow.
// PLACEMENT: Runtime static event hub. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-19
// NOTES: Join announcer and feed systems subscribe here instead of spawn timing hooks.
// =============================================================================

namespace CCS.Modules.CharacterController.Local {
    public static class CCS_PlayerSessionEvents
    {
        #region Events

        public static event Action<CCS_PlayerSessionContext> PlayerSpawned;

        public static event Action<CCS_PlayerSessionContext> LocalPlayerReady;

        public static event Action<CCS_PlayerNameChangedContext> PlayerNameChanged;

        public static event Action<CCS_PlayerJoinNotificationQueuedContext> JoinNotificationQueued;

        public static event Action JoinNotificationFeedReady;

        #endregion

        #region Public Methods

        public static void RaisePlayerSpawned(CCS_PlayerSessionContext context)
        {
            PlayerSpawned?.Invoke(context);
        }

        public static void RaiseLocalPlayerReady(CCS_PlayerSessionContext context)
        {
            LocalPlayerReady?.Invoke(context);
        }

        public static void RaisePlayerNameChanged(CCS_PlayerNameChangedContext context)
        {
            PlayerNameChanged?.Invoke(context);
        }

        public static void RaiseJoinNotificationQueued(CCS_PlayerJoinNotificationQueuedContext context)
        {
            JoinNotificationQueued?.Invoke(context);
        }

        public static void RaiseJoinNotificationFeedReady()
        {
            JoinNotificationFeedReady?.Invoke();
        }

        #endregion
    }

    public readonly struct CCS_PlayerSessionContext
    {
        public ulong OwnerClientId { get; }

        public GameObject PlayerRoot { get; }

        public bool IsNetworkSession { get; }

        public bool IsLocalOwner { get; }

        public CCS_PlayerSessionContext(
            ulong ownerClientId,
            GameObject playerRoot,
            bool isNetworkSession,
            bool isLocalOwner)
        {
            OwnerClientId = ownerClientId;
            PlayerRoot = playerRoot;
            IsNetworkSession = isNetworkSession;
            IsLocalOwner = isLocalOwner;
        }
    }

    public readonly struct CCS_PlayerNameChangedContext
    {
        public ulong OwnerClientId { get; }

        public string PlayerName { get; }

        public GameObject PlayerRoot { get; }

        public CCS_PlayerNameChangedContext(ulong ownerClientId, string playerName, GameObject playerRoot)
        {
            OwnerClientId = ownerClientId;
            PlayerName = playerName;
            PlayerRoot = playerRoot;
        }
    }

    public readonly struct CCS_PlayerJoinNotificationQueuedContext
    {
        public ulong OwnerClientId { get; }

        public string PlayerName { get; }

        public string Reason { get; }

        public CCS_PlayerJoinNotificationQueuedContext(ulong ownerClientId, string playerName, string reason)
        {
            OwnerClientId = ownerClientId;
            PlayerName = playerName;
            Reason = reason;
        }
    }
}
