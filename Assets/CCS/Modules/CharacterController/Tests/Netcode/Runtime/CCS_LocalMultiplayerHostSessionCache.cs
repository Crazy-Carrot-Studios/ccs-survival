using System;
using System.Globalization;
using System.IO;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_LocalMultiplayerHostSessionCache
// CATEGORY: Modules / CharacterController / Tests / Netcode / Runtime
// PURPOSE: Cross-process local host session advertisement for join-list discovery.
// PLACEMENT: Static test-only cache. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: File heartbeat only. No Netcode registry or NetworkManager wiring.
// =============================================================================

namespace CCS.Modules.CharacterController.Tests.Netcode
{
    public static class CCS_LocalMultiplayerHostSessionCache
    {
        #region Variables

        private static readonly string CacheFilePath = Path.Combine(
            Path.GetTempPath(),
            CCS_NetcodeTestConstants.HostSessionCacheFolderName,
            CCS_NetcodeTestConstants.HostSessionCacheFileName);

        #endregion

        #region Public Methods

        public static void PublishHeartbeat(string serverDisplayName, string address, ushort port)
        {
            string sanitizedName = CCS_MultiplayerServerNameUtility.Sanitize(serverDisplayName);
            string sanitizedAddress = string.IsNullOrWhiteSpace(address)
                ? CCS_NetcodeTestConstants.DefaultLocalhostAddress
                : address.Trim();

            HostSessionRecord record = new HostSessionRecord
            {
                serverDisplayName = sanitizedName,
                address = sanitizedAddress,
                port = port,
                heartbeatUtcTicks = DateTime.UtcNow.Ticks,
            };

            try
            {
                string directory = Path.GetDirectoryName(CacheFilePath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(CacheFilePath, JsonUtility.ToJson(record));
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[Hosting Flow] Failed to publish local host session heartbeat: " + exception.Message);
            }
        }

        public static bool TryReadActiveSession(out CCS_MultiplayerServerListEntry entry)
        {
            entry = default;

            if (!TryReadRecord(out HostSessionRecord record))
            {
                return false;
            }

            if (!IsHeartbeatFresh(record.heartbeatUtcTicks))
            {
                return false;
            }

            entry = new CCS_MultiplayerServerListEntry(
                record.serverDisplayName,
                record.address,
                record.port);
            return true;
        }

        public static void Clear()
        {
            try
            {
                if (File.Exists(CacheFilePath))
                {
                    File.Delete(CacheFilePath);
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[Hosting Flow] Failed to clear local host session cache: " + exception.Message);
            }
        }

        #endregion

        #region Private Methods

        private static bool TryReadRecord(out HostSessionRecord record)
        {
            record = default;

            if (!File.Exists(CacheFilePath))
            {
                return false;
            }

            try
            {
                string json = File.ReadAllText(CacheFilePath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    return false;
                }

                record = JsonUtility.FromJson<HostSessionRecord>(json);
                return record.port > 0 && !string.IsNullOrWhiteSpace(record.address);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[Join Flow] Failed to read local host session cache: " + exception.Message);
                return false;
            }
        }

        private static bool IsHeartbeatFresh(long heartbeatUtcTicks)
        {
            if (heartbeatUtcTicks <= 0)
            {
                return false;
            }

            DateTime heartbeatUtc = new DateTime(heartbeatUtcTicks, DateTimeKind.Utc);
            double ageSeconds = (DateTime.UtcNow - heartbeatUtc).TotalSeconds;
            return ageSeconds <= CCS_NetcodeTestConstants.HostSessionHeartbeatSeconds;
        }

        #endregion

        #region Nested Types

        [Serializable]
        private struct HostSessionRecord
        {
            public string serverDisplayName;
            public string address;
            public ushort port;
            public long heartbeatUtcTicks;
        }

        #endregion
    }
}
