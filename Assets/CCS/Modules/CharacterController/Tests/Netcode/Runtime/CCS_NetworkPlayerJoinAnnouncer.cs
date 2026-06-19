using System;
using System.Collections.Generic;
using CCS.Modules.CharacterController.Tests;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
// SCRIPT: CCS_NetworkPlayerJoinAnnouncer
// CATEGORY: Modules / CharacterController / Tests / Netcode / Runtime
// PURPOSE: Queued, deduplicated join notifications for master test network sessions.
// PLACEMENT: Runtime static utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Dedupes by OwnerClientId. Server marks announced only after ClientRpc is sent.
//        Clients queue until feed registration and flush on scene/feed readiness.
// =============================================================================

namespace CCS.Modules.CharacterController.Tests.Netcode
{
    public static class CCS_NetworkPlayerJoinAnnouncer
    {
        #region Variables

        private static readonly HashSet<ulong> ServerRpcSentOwnerClientIds = new HashSet<ulong>();
        private static readonly HashSet<ulong> LocalDisplayedOwnerClientIds = new HashSet<ulong>();
        private static readonly Dictionary<ulong, PendingServerAnnouncement> ServerPendingAnnouncements =
            new Dictionary<ulong, PendingServerAnnouncement>();
        private static readonly List<PendingClientAnnouncement> ClientPendingAnnouncements =
            new List<PendingClientAnnouncement>();

        private static bool subscribedToFeedRegistry;
        private static bool subscribedToNetworkStop;
        private static bool subscribedToSessionEvents;

        #endregion

        #region Public Methods

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void InitializeLifecycleHooks()
        {
            EnsureLifecycleSubscription();
        }

        public static void ClientReceiveJoinAnnouncement(ulong ownerClientId, string playerName)
        {
            EnsureLifecycleSubscription();
            CCS_JoinFeedDebugLog.RpcReceived(ownerClientId, playerName);

            if (LocalDisplayedOwnerClientIds.Contains(ownerClientId))
            {
                CCS_JoinFeedDebugLog.AnnounceSkipped(ownerClientId, "already_displayed");
                return;
            }

            if (!TryResolveRpcPlayerName(playerName, out string sanitizedName))
            {
                CCS_JoinFeedDebugLog.AnnounceSkipped(ownerClientId, "invalid_rpc_name");
                return;
            }

            if (TryDisplayJoinAnnouncement(ownerClientId, sanitizedName))
            {
                return;
            }

            QueueClientAnnouncement(ownerClientId, sanitizedName, GetClientQueueReason());
        }

        public static void ResetSessionTracking()
        {
            ServerRpcSentOwnerClientIds.Clear();
            LocalDisplayedOwnerClientIds.Clear();
            ServerPendingAnnouncements.Clear();
            ClientPendingAnnouncements.Clear();
            CCS_PlayerJoinNotificationFeedRegistry.Clear();
        }

        #endregion

        #region Private Methods

        private static void EnsureLifecycleSubscription()
        {
            if (!subscribedToSessionEvents)
            {
                CCS_TestPlayerSessionEvents.PlayerSpawned += HandlePlayerSpawned;
                CCS_TestPlayerSessionEvents.PlayerNameChanged += HandlePlayerNameChanged;
                CCS_TestPlayerSessionEvents.JoinNotificationFeedReady += HandleJoinNotificationFeedReady;
                subscribedToSessionEvents = true;
            }

            if (!subscribedToFeedRegistry)
            {
                CCS_PlayerJoinNotificationFeedRegistry.FeedRegistered += HandleFeedRegistered;
                SceneManager.sceneLoaded += HandleSceneLoaded;
                subscribedToFeedRegistry = true;
            }

            if (subscribedToNetworkStop)
            {
                return;
            }

            NetworkManager networkManager = NetworkManager.Singleton;
            if (networkManager == null)
            {
                return;
            }

            networkManager.OnClientStopped += HandleClientStopped;
            subscribedToNetworkStop = true;
        }

        private static void HandleJoinNotificationFeedReady()
        {
            FlushAllServerPendingAnnouncements();
            AnnounceAllConnectedPlayersIfReady();
            FlushClientPendingAnnouncements();
        }

        private static void HandlePlayerSpawned(CCS_TestPlayerSessionContext context)
        {
            if (!context.IsNetworkSession || context.NetworkNameplate == null || !context.NetworkNameplate.IsSpawned)
            {
                return;
            }

            EnsureLifecycleSubscription();

            if (context.NetworkNameplate.IsServer
                && NetworkManager.Singleton != null
                && context.OwnerClientId == NetworkManager.Singleton.LocalClientId)
            {
                CCS_JoinFeedDebugLog.HostStarted(context.OwnerClientId);
            }

            if (context.NetworkNameplate.IsServer)
            {
                FlushAllServerPendingAnnouncements();
            }
        }

        private static void HandlePlayerNameChanged(CCS_TestPlayerNameChangedContext context)
        {
            if (context.NetworkNameplate == null
                || !context.NetworkNameplate.IsSpawned
                || !context.NetworkNameplate.IsServer)
            {
                return;
            }

            EnsureLifecycleSubscription();
            CCS_JoinFeedDebugLog.NameSubmitted(context.OwnerClientId, context.PlayerName);
            ServerTryAnnounceJoin(context.NetworkNameplate, context.PlayerName);
        }

        private static void HandleFeedRegistered()
        {
            FlushAllServerPendingAnnouncements();
            AnnounceAllConnectedPlayersIfReady();
            FlushClientPendingAnnouncements();
        }

        private static void HandleSceneLoaded(Scene scene, LoadSceneMode loadMode)
        {
            if (!IsMasterTestScene(scene))
            {
                return;
            }

            FlushAllServerPendingAnnouncements();
            AnnounceAllConnectedPlayersIfReady();
            FlushClientPendingAnnouncements();
        }

        private static void HandleClientStopped(bool _)
        {
            ResetSessionTracking();
        }

        private static void ServerTryAnnounceJoin(CCS_NetworkPlayerNameplate nameplate, string playerNameOverride)
        {
            if (nameplate == null || !nameplate.IsServer)
            {
                return;
            }

            ulong ownerClientId = nameplate.OwnerClientId;
            string playerName = playerNameOverride ?? nameplate.GetDisplayNameForJoinAnnouncement();
            CCS_JoinFeedDebugLog.TryAnnounce(ownerClientId, playerName);

            if (ServerRpcSentOwnerClientIds.Contains(ownerClientId))
            {
                CCS_JoinFeedDebugLog.AnnounceSkipped(ownerClientId, "rpc_already_sent");
                ServerPendingAnnouncements.Remove(ownerClientId);
                return;
            }

            if (!TryResolveSubmittedPlayerName(playerName, out string sanitizedName))
            {
                QueueServerAnnouncement(nameplate, ownerClientId, string.Empty, "display_name_not_known");
                return;
            }

            if (!CCS_MasterTestNetworkSessionUtility.IsNetworkSessionActive())
            {
                QueueServerAnnouncement(nameplate, ownerClientId, sanitizedName, "network_not_listening");
                return;
            }

            if (!CCS_MasterTestNetworkSessionUtility.IsMasterTestSceneActive())
            {
                QueueServerAnnouncement(nameplate, ownerClientId, sanitizedName, "master_test_scene_not_loaded");
                return;
            }

            nameplate.BroadcastJoinNotification(sanitizedName, ownerClientId);
            ServerRpcSentOwnerClientIds.Add(ownerClientId);
            ServerPendingAnnouncements.Remove(ownerClientId);
            CCS_JoinFeedDebugLog.RpcSent(ownerClientId, sanitizedName);
        }

        private static void QueueServerAnnouncement(
            CCS_NetworkPlayerNameplate nameplate,
            ulong ownerClientId,
            string sanitizedName,
            string reason)
        {
            if (ServerRpcSentOwnerClientIds.Contains(ownerClientId))
            {
                CCS_JoinFeedDebugLog.AnnounceSkipped(ownerClientId, "rpc_already_sent");
                return;
            }

            ServerPendingAnnouncements[ownerClientId] = new PendingServerAnnouncement
            {
                Nameplate = nameplate,
                OwnerClientId = ownerClientId,
                SanitizedName = sanitizedName
            };

            CCS_JoinFeedDebugLog.QueueReason(ownerClientId, reason);
            CCS_TestPlayerSessionEvents.RaiseJoinNotificationQueued(
                new CCS_TestPlayerJoinNotificationQueuedContext(ownerClientId, sanitizedName, reason));
        }

        private static void FlushAllServerPendingAnnouncements()
        {
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer)
            {
                return;
            }

            if (!CCS_MasterTestNetworkSessionUtility.IsNetworkSessionActive())
            {
                return;
            }

            if (ServerPendingAnnouncements.Count == 0)
            {
                return;
            }

            ulong[] ownerClientIds = new ulong[ServerPendingAnnouncements.Count];
            ServerPendingAnnouncements.Keys.CopyTo(ownerClientIds, 0);

            for (int i = 0; i < ownerClientIds.Length; i++)
            {
                ulong ownerClientId = ownerClientIds[i];
                if (!ServerPendingAnnouncements.TryGetValue(ownerClientId, out PendingServerAnnouncement pending))
                {
                    continue;
                }

                CCS_NetworkPlayerNameplate nameplate = ResolveNameplate(pending);
                if (nameplate == null)
                {
                    continue;
                }

                string playerName = !string.IsNullOrWhiteSpace(pending.SanitizedName)
                    ? pending.SanitizedName
                    : nameplate.GetDisplayNameForJoinAnnouncement();
                ServerTryAnnounceJoin(nameplate, playerName);
            }
        }

        private static CCS_NetworkPlayerNameplate ResolveNameplate(PendingServerAnnouncement pending)
        {
            if (pending.Nameplate != null && pending.Nameplate.IsSpawned)
            {
                return pending.Nameplate;
            }

            CCS_NetworkPlayerNameplate[] nameplates =
                UnityEngine.Object.FindObjectsByType<CCS_NetworkPlayerNameplate>(
                    FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None);
            for (int i = 0; i < nameplates.Length; i++)
            {
                CCS_NetworkPlayerNameplate candidate = nameplates[i];
                if (candidate != null
                    && candidate.IsSpawned
                    && candidate.OwnerClientId == pending.OwnerClientId)
                {
                    return candidate;
                }
            }

            return null;
        }

        private static void QueueClientAnnouncement(ulong ownerClientId, string sanitizedName, string reason)
        {
            for (int i = 0; i < ClientPendingAnnouncements.Count; i++)
            {
                if (ClientPendingAnnouncements[i].OwnerClientId == ownerClientId)
                {
                    ClientPendingAnnouncements[i] = new PendingClientAnnouncement
                    {
                        OwnerClientId = ownerClientId,
                        SanitizedName = sanitizedName
                    };
                    CCS_JoinFeedDebugLog.QueueReason(ownerClientId, reason);
                    CCS_TestPlayerSessionEvents.RaiseJoinNotificationQueued(
                        new CCS_TestPlayerJoinNotificationQueuedContext(ownerClientId, sanitizedName, reason));
                    return;
                }
            }

            ClientPendingAnnouncements.Add(new PendingClientAnnouncement
            {
                OwnerClientId = ownerClientId,
                SanitizedName = sanitizedName
            });
            CCS_JoinFeedDebugLog.QueueReason(ownerClientId, reason);
            CCS_TestPlayerSessionEvents.RaiseJoinNotificationQueued(
                new CCS_TestPlayerJoinNotificationQueuedContext(ownerClientId, sanitizedName, reason));
        }

        private static void FlushClientPendingAnnouncements()
        {
            for (int i = ClientPendingAnnouncements.Count - 1; i >= 0; i--)
            {
                PendingClientAnnouncement pending = ClientPendingAnnouncements[i];
                TryDisplayJoinAnnouncement(pending.OwnerClientId, pending.SanitizedName);
            }
        }

        private static void AnnounceAllConnectedPlayersIfReady()
        {
            NetworkManager networkManager = NetworkManager.Singleton;
            if (networkManager == null || !networkManager.IsServer)
            {
                return;
            }

            if (!CCS_MasterTestNetworkSessionUtility.CanProcessJoinNotifications())
            {
                return;
            }

            CCS_NetworkPlayerNameplate[] nameplates =
                UnityEngine.Object.FindObjectsByType<CCS_NetworkPlayerNameplate>(
                    FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None);
            for (int i = 0; i < nameplates.Length; i++)
            {
                CCS_NetworkPlayerNameplate nameplate = nameplates[i];
                if (nameplate == null || !nameplate.IsSpawned)
                {
                    continue;
                }

                ServerTryAnnounceJoin(nameplate, nameplate.GetDisplayNameForJoinAnnouncement());
            }
        }

        private static bool TryDisplayJoinAnnouncement(ulong ownerClientId, string sanitizedName)
        {
            if (LocalDisplayedOwnerClientIds.Contains(ownerClientId))
            {
                CCS_JoinFeedDebugLog.AnnounceSkipped(ownerClientId, "already_displayed");
                RemoveClientPending(ownerClientId);
                return true;
            }

            if (!CCS_MasterTestNetworkSessionUtility.CanProcessJoinNotifications())
            {
                return false;
            }

            if (!CCS_PlayerJoinNotificationFeedRegistry.IsRegistered)
            {
                return false;
            }

            if (!CCS_PlayerJoinNotificationFeedRegistry.TryShowPlayerJoined(sanitizedName))
            {
                return false;
            }

            LocalDisplayedOwnerClientIds.Add(ownerClientId);
            RemoveClientPending(ownerClientId);
            CCS_JoinFeedDebugLog.Displayed(sanitizedName);
            return true;
        }

        private static void RemoveClientPending(ulong ownerClientId)
        {
            for (int i = ClientPendingAnnouncements.Count - 1; i >= 0; i--)
            {
                if (ClientPendingAnnouncements[i].OwnerClientId == ownerClientId)
                {
                    ClientPendingAnnouncements.RemoveAt(i);
                }
            }
        }

        private static string GetClientQueueReason()
        {
            if (!CCS_MasterTestNetworkSessionUtility.IsNetworkSessionActive())
            {
                return "network_not_listening";
            }

            if (!CCS_MasterTestNetworkSessionUtility.IsMasterTestSceneActive())
            {
                return "master_test_scene_not_loaded";
            }

            if (!CCS_PlayerJoinNotificationFeedRegistry.IsRegistered)
            {
                return "join_feed_not_registered";
            }

            return "display_not_ready";
        }

        private static bool TryResolveSubmittedPlayerName(string playerName, out string sanitizedName)
        {
            sanitizedName = string.Empty;
            if (string.IsNullOrWhiteSpace(playerName))
            {
                return false;
            }

            sanitizedName = CCS_MultiplayerPlayerNameUtility.Sanitize(playerName);
            return !string.IsNullOrWhiteSpace(sanitizedName);
        }

        private static bool TryResolveRpcPlayerName(string playerName, out string sanitizedName)
        {
            sanitizedName = string.Empty;
            if (string.IsNullOrWhiteSpace(playerName))
            {
                return false;
            }

            sanitizedName = CCS_MultiplayerPlayerNameUtility.Sanitize(playerName);
            return !string.IsNullOrWhiteSpace(sanitizedName);
        }

        private static bool IsMasterTestScene(Scene scene)
        {
            return scene.name == CCS_MasterTestUiConstants.MasterTestSceneName
                || scene.path == CCS_MasterTestUiConstants.MasterTestScenePath;
        }

        #endregion

        #region Nested Types

        private struct PendingServerAnnouncement
        {
            public CCS_NetworkPlayerNameplate Nameplate;
            public ulong OwnerClientId;
            public string SanitizedName;
        }

        private struct PendingClientAnnouncement
        {
            public ulong OwnerClientId;
            public string SanitizedName;
        }

        #endregion
    }
}
