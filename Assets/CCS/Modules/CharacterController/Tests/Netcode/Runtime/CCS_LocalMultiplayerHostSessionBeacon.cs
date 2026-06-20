using System.Globalization;
using UnityEngine;

// =============================================================================
// SCRIPT: CCS_LocalMultiplayerHostSessionBeacon
// CATEGORY: Modules / CharacterController / Tests / Netcode / Runtime
// PURPOSE: Keeps local host session heartbeats alive after the hosting scene unloads.
// PLACEMENT: Spawned by CCS_MultiplayerHostingMenu on successful host start.
// AUTHOR: James Schilz
// CREATED: 2026-06-07
// NOTES: DontDestroyOnLoad test helper. No NetworkManager prefab changes.
// =============================================================================

namespace CCS.Modules.CharacterController.Tests.Netcode
{
    public sealed class CCS_LocalMultiplayerHostSessionBeacon : MonoBehaviour
    {
        #region Variables

        private static CCS_LocalMultiplayerHostSessionBeacon activeBeacon;

        private string serverDisplayName = CCS_NetcodeTestConstants.DefaultServerSessionLabel;
        private string address = CCS_NetcodeTestConstants.DefaultLocalhostAddress;
        private ushort port = CCS_NetcodeTestConstants.DefaultServerPort;
        private float nextHeartbeatTime;

        #endregion

        #region Public Methods

        public static void StartBeacon(string displayName, string connectAddress, ushort listenPort)
        {
            StopBeacon();

            GameObject beaconObject = new GameObject(nameof(CCS_LocalMultiplayerHostSessionBeacon));
            DontDestroyOnLoad(beaconObject);
            activeBeacon = beaconObject.AddComponent<CCS_LocalMultiplayerHostSessionBeacon>();
            activeBeacon.Initialize(displayName, connectAddress, listenPort);
        }

        public static void StopBeacon()
        {
            if (activeBeacon == null)
            {
                CCS_LocalMultiplayerHostSessionCache.Clear();
                return;
            }

            Destroy(activeBeacon.gameObject);
            activeBeacon = null;
            CCS_LocalMultiplayerHostSessionCache.Clear();
        }

        #endregion

        #region Unity Callbacks

        private void OnDestroy()
        {
            if (activeBeacon == this)
            {
                activeBeacon = null;
                CCS_LocalMultiplayerHostSessionCache.Clear();
            }
        }

        private void Update()
        {
            if (Time.unscaledTime < nextHeartbeatTime)
            {
                return;
            }

            nextHeartbeatTime = Time.unscaledTime + CCS_NetcodeTestConstants.HostSessionHeartbeatIntervalSeconds;
            CCS_LocalMultiplayerHostSessionCache.PublishHeartbeat(serverDisplayName, address, port);
        }

        #endregion

        #region Private Methods

        private void Initialize(string displayName, string connectAddress, ushort listenPort)
        {
            serverDisplayName = CCS_MultiplayerServerNameUtility.Sanitize(displayName);
            address = string.IsNullOrWhiteSpace(connectAddress)
                ? CCS_NetcodeTestConstants.DefaultLocalhostAddress
                : connectAddress.Trim();
            port = listenPort;
            nextHeartbeatTime = 0f;
            CCS_LocalMultiplayerHostSessionCache.PublishHeartbeat(serverDisplayName, address, port);
            Debug.Log(
                "[Hosting Flow] Local host session beacon started for "
                + $"{serverDisplayName} ({address}:{port.ToString(CultureInfo.InvariantCulture)}).");
        }

        #endregion
    }
}
