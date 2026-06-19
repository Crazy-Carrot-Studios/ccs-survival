using UnityEngine;

// =============================================================================
// SCRIPT: CCS_JoinFeedDebugLog
// CATEGORY: Modules / CharacterController / Tests / Runtime
// PURPOSE: Optional debug logging for master test join notification flow.
// PLACEMENT: Runtime static utility. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: Toggle via CCS_MasterTestUiConstants.EnableJoinFeedDebugLogs.
// =============================================================================

namespace CCS.Modules.CharacterController.Tests
{
    public static class CCS_JoinFeedDebugLog
    {
        private const string LogPrefix = "[CCS Join Feed Debug]";

        #region Properties

        private static bool IsEnabled => CCS_MasterTestUiConstants.EnableJoinFeedDebugLogs;

        #endregion

        #region Public Methods

        public static void HostStarted(ulong ownerClientId)
        {
            Log($"HostStarted OwnerClientId={ownerClientId}");
        }

        public static void FeedRegistered()
        {
            Log("FeedRegistered");
        }

        public static void NameSubmitted(ulong ownerClientId, string playerName)
        {
            Log($"NameSubmitted OwnerClientId={ownerClientId} Name={FormatName(playerName)}");
        }

        public static void TryAnnounce(ulong ownerClientId, string playerName)
        {
            Log($"TryAnnounce OwnerClientId={ownerClientId} Name={FormatName(playerName)}");
        }

        public static void QueueReason(ulong ownerClientId, string reason)
        {
            Log($"QueueReason OwnerClientId={ownerClientId} Reason={reason}");
        }

        public static void AnnounceSkipped(ulong ownerClientId, string reason)
        {
            Log($"AnnounceSkipped OwnerClientId={ownerClientId} Reason={reason}");
        }

        public static void RpcSent(ulong ownerClientId, string playerName)
        {
            Log($"RpcSent OwnerClientId={ownerClientId} Name={FormatName(playerName)}");
        }

        public static void RpcReceived(ulong ownerClientId, string playerName)
        {
            Log($"RpcReceived OwnerClientId={ownerClientId} Name={FormatName(playerName)}");
        }

        public static void Displayed(string playerName)
        {
            Log($"Displayed Name={FormatName(playerName)}");
        }

        #endregion

        #region Private Methods

        private static void Log(string message)
        {
            if (!IsEnabled)
            {
                return;
            }

            Debug.Log($"{LogPrefix} {message}");
        }

        private static string FormatName(string playerName)
        {
            return string.IsNullOrWhiteSpace(playerName) ? "n/a" : playerName.Trim();
        }

        #endregion
    }
}
