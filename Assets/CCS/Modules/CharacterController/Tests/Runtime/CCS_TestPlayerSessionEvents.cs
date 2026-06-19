using System;
using CCS.Modules.CharacterController.Tests.Netcode;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_TestPlayerSessionEvents
// CATEGORY: Modules / CharacterController / Tests / Runtime
// PURPOSE: Decoupled session events for test player spawn, readiness, and join feed flow.
// PLACEMENT: Runtime static event hub. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-19
// NOTES: Join announcer and feed systems subscribe here instead of spawn timing hooks.
// =============================================================================

namespace CCS.Modules.CharacterController.Tests
{
    public static class CCS_TestPlayerSessionEvents
    {
        #region Events

        public static event Action<CCS_TestPlayerSessionContext> PlayerSpawned;

        public static event Action<CCS_TestPlayerSessionContext> LocalPlayerReady;

        public static event Action<CCS_TestPlayerNameChangedContext> PlayerNameChanged;

        public static event Action<CCS_TestPlayerJoinNotificationQueuedContext> JoinNotificationQueued;

        public static event Action JoinNotificationFeedReady;

        #endregion

        #region Public Methods

        public static void RaisePlayerSpawned(CCS_TestPlayerSessionContext context)
        {
            PlayerSpawned?.Invoke(context);
        }

        public static void RaiseLocalPlayerReady(CCS_TestPlayerSessionContext context)
        {
            LocalPlayerReady?.Invoke(context);
        }

        public static void RaisePlayerNameChanged(CCS_TestPlayerNameChangedContext context)
        {
            PlayerNameChanged?.Invoke(context);
        }

        public static void RaiseJoinNotificationQueued(CCS_TestPlayerJoinNotificationQueuedContext context)
        {
            JoinNotificationQueued?.Invoke(context);
        }

        public static void RaiseJoinNotificationFeedReady()
        {
            JoinNotificationFeedReady?.Invoke();
        }

        #endregion
    }

    public readonly struct CCS_TestPlayerSessionContext
    {
        public ulong OwnerClientId { get; }

        public GameObject PlayerRoot { get; }

        public CCS_NetworkPlayerNameplate NetworkNameplate { get; }

        public bool IsNetworkSession { get; }

        public bool IsLocalOwner { get; }

        public CCS_TestPlayerSessionContext(
            ulong ownerClientId,
            GameObject playerRoot,
            CCS_NetworkPlayerNameplate networkNameplate,
            bool isNetworkSession,
            bool isLocalOwner)
        {
            OwnerClientId = ownerClientId;
            PlayerRoot = playerRoot;
            NetworkNameplate = networkNameplate;
            IsNetworkSession = isNetworkSession;
            IsLocalOwner = isLocalOwner;
        }
    }

    public readonly struct CCS_TestPlayerNameChangedContext
    {
        public ulong OwnerClientId { get; }

        public string PlayerName { get; }

        public CCS_NetworkPlayerNameplate NetworkNameplate { get; }

        public CCS_TestPlayerNameChangedContext(
            ulong ownerClientId,
            string playerName,
            CCS_NetworkPlayerNameplate networkNameplate)
        {
            OwnerClientId = ownerClientId;
            PlayerName = playerName;
            NetworkNameplate = networkNameplate;
        }
    }

    public readonly struct CCS_TestPlayerJoinNotificationQueuedContext
    {
        public ulong OwnerClientId { get; }

        public string PlayerName { get; }

        public string Reason { get; }

        public CCS_TestPlayerJoinNotificationQueuedContext(ulong ownerClientId, string playerName, string reason)
        {
            OwnerClientId = ownerClientId;
            PlayerName = playerName;
            Reason = reason;
        }
    }
}
